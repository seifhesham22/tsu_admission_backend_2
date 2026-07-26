using Admission.Application.Sync.Dtos;
using Admission.Application.Sync.Contracts;
using Admission.Domain.Catalogue;
using Admission.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Exceptions;

namespace Admission.Infrastructure.Sync.Services;

public sealed class CatalogueSyncService : ICatalogueSyncService
{
    private readonly AdmissionDbContext _context;
    private readonly IOneCClient _client;
    private readonly ILogger<CatalogueSyncService> _logger;

    public CatalogueSyncService(
        AdmissionDbContext context,
        IOneCClient client,
        ILogger<CatalogueSyncService> logger)
    {
        _context = context;
        _client = client;
        _logger = logger;
    }

    public async Task<CatalogueSyncResult> SyncAllAsync(CancellationToken cancellationToken = default)
    {
        var faculties = await _client.GetFacultiesAsync(cancellationToken);
        var levels = await _client.GetEducationLevelsAsync(cancellationToken);
        var documentTypes = await _client.GetDocumentTypesAsync(cancellationToken);
        var programs = await _client.GetProgramsAsync(cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await SyncFacultiesAsync(faculties, cancellationToken);
            await SyncEducationLevelsAsync(levels, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await SyncDocumentTypesAsync(documentTypes, cancellationToken);
            await SyncProgramsAsync(programs, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Catalogue sync completed: {Faculties} faculties, {Levels} levels, {Types} document types, {Programs} programs.",
                faculties.Count,
                levels.Count,
                documentTypes.Count,
                programs.Count);

            return new CatalogueSyncResult(
                faculties.Count,
                levels.Count,
                documentTypes.Count,
                programs.Count);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task SyncFacultiesAsync(
        IReadOnlyList<FacultySyncDto> incoming,
        CancellationToken cancellationToken)
    {
        var existing = await _context.Faculties.ToListAsync(cancellationToken);
        var incomingIds = incoming.Select(x => x.Id).ToHashSet();

        _context.Faculties.RemoveRange(existing.Where(x => !incomingIds.Contains(x.OneCId)));

        foreach (var dto in incoming)
        {
            var entity = existing.FirstOrDefault(x => x.OneCId == dto.Id);
            if (entity is null)
            {
                _context.Faculties.Add(Faculty.Create(dto.Id, dto.Name));
            }
            else
            {
                entity.Rename(dto.Name);
            }
        }
    }

    private async Task SyncEducationLevelsAsync(
        IReadOnlyList<EducationLevelSyncDto> incoming,
        CancellationToken cancellationToken)
    {
        var existing = await _context.EducationLevels.ToListAsync(cancellationToken);
        var incomingIds = incoming.Select(x => x.Id).ToHashSet();

        _context.EducationLevels.RemoveRange(existing.Where(x => !incomingIds.Contains(x.OneCId)));

        foreach (var dto in incoming)
        {
            var entity = existing.FirstOrDefault(x => x.OneCId == dto.Id);
            if (entity is null)
            {
                _context.EducationLevels.Add(EducationLevel.Create(dto.Id, dto.Name));
            }
            else
            {
                entity.Rename(dto.Name);
            }
        }
    }

    private async Task SyncDocumentTypesAsync(
        IReadOnlyList<EducationDocumentTypeSyncDto> incoming,
        CancellationToken cancellationToken)
    {
        var levels = await _context.EducationLevels.ToListAsync(cancellationToken);
        var existing = await _context.EducationDocumentTypes
            .Include(x => x.NextEducationLevels)
            .ToListAsync(cancellationToken);

        var incomingIds = incoming.Select(x => x.Id).ToHashSet();
        _context.EducationDocumentTypes.RemoveRange(existing.Where(x => !incomingIds.Contains(x.OneCId)));

        foreach (var dto in incoming)
        {
            var entity = existing.FirstOrDefault(x => x.OneCId == dto.Id);
            if (entity is null)
            {
                entity = EducationDocumentType.Create(dto.Id, dto.Name);
                _context.EducationDocumentTypes.Add(entity);
            }
            else
            {
                entity.Rename(dto.Name);
            }

            if (dto.EducationLevel is not null)
            {
                entity.SetCurrentEducationLevel(ResolveLevel(levels, dto.EducationLevel.Id));
            }

            var nextLevels = dto.NextEducationLevels
                .Select(next => ResolveLevel(levels, next.Id))
                .ToList();

            entity.ReplaceNextEducationLevels(nextLevels);
        }
    }

    private async Task SyncProgramsAsync(
        IReadOnlyList<EducationProgramSyncDto> incoming,
        CancellationToken cancellationToken)
    {
        var faculties = await _context.Faculties.ToListAsync(cancellationToken);
        var levels = await _context.EducationLevels.ToListAsync(cancellationToken);
        var existing = await _context.EducationPrograms.ToListAsync(cancellationToken);

        var incomingIds = incoming.Select(x => x.Id).ToHashSet();
        _context.EducationPrograms.RemoveRange(existing.Where(x => !incomingIds.Contains(x.OneCId)));

        foreach (var dto in incoming)
        {
            if (dto.Faculty is null || dto.EducationLevel is null)
            {
                _logger.LogWarning(
                    "Skipping program {ProgramId} because faculty or education level is missing.",
                    dto.Id);
                continue;
            }

            var faculty = faculties.FirstOrDefault(x => x.OneCId == dto.Faculty.Id)
                ?? throw new NotFoundException($"Faculty with 1C id '{dto.Faculty.Id}' was not found.");

            var level = ResolveLevel(levels, dto.EducationLevel.Id);

            var entity = existing.FirstOrDefault(x => x.OneCId == dto.Id);
            if (entity is null)
            {
                _context.EducationPrograms.Add(EducationProgram.Create(
                    dto.Id,
                    faculty.Id,
                    level.Id,
                    dto.Name,
                    dto.Code,
                    dto.Language,
                    dto.EducationForm));
            }
            else
            {
                entity.Update(
                    faculty.Id,
                    level.Id,
                    dto.Name,
                    dto.Code,
                    dto.Language,
                    dto.EducationForm);
            }
        }
    }

    private static EducationLevel ResolveLevel(IReadOnlyList<EducationLevel> levels, int oneCId) =>
        levels.FirstOrDefault(x => x.OneCId == oneCId)
        ?? throw new NotFoundException($"Education level with 1C id '{oneCId}' was not found.");
}
