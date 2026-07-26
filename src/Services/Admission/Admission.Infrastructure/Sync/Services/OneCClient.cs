using System.Net.Http.Json;
using System.Text.Json;
using Admission.Application.Options;
using Admission.Application.Sync.Dtos;
using Admission.Application.Sync.Contracts;
using Microsoft.Extensions.Options;
using Shared.Kernel.Exceptions;

namespace Admission.Infrastructure.Sync.Services;

public sealed class OneCClient : IOneCClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly OneCOptions _options;

    public OneCClient(HttpClient httpClient, IOptions<OneCOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public Task<IReadOnlyList<FacultySyncDto>> GetFacultiesAsync(CancellationToken cancellationToken = default) =>
        GetListAsync<FacultySyncDto>(_options.FacultiesPath, cancellationToken);

    public Task<IReadOnlyList<EducationLevelSyncDto>> GetEducationLevelsAsync(
        CancellationToken cancellationToken = default) =>
        GetListAsync<EducationLevelSyncDto>(_options.EducationLevelsPath, cancellationToken);

    public Task<IReadOnlyList<EducationDocumentTypeSyncDto>> GetDocumentTypesAsync(
        CancellationToken cancellationToken = default) =>
        GetListAsync<EducationDocumentTypeSyncDto>(_options.DocumentTypesPath, cancellationToken);

    public async Task<IReadOnlyList<EducationProgramSyncDto>> GetProgramsAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new List<EducationProgramSyncDto>();
        var page = 1;

        while (true)
        {
            var path = $"{_options.ProgramsPath}?page={page}&size={_options.PageSize}";
            var response = await _httpClient.GetAsync(path, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new DomainRuleException(
                    $"1C programs request failed with status {(int)response.StatusCode} for '{path}'.");
            }

            var payload = await response.Content
                .ReadFromJsonAsync<EducationProgramPageSyncDto>(SerializerOptions, cancellationToken);

            if (payload is null || payload.Programs.Count == 0)
            {
                break;
            }

            results.AddRange(payload.Programs);

            var pagination = payload.Pagination;
            if (pagination is null || pagination.Current >= pagination.Count || pagination.Count <= 0)
            {
                break;
            }

            page++;
        }

        return results;
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string path, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(path, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new DomainRuleException(
                $"1C request failed with status {(int)response.StatusCode} for '{path}'.");
        }

        var payload = await response.Content
            .ReadFromJsonAsync<List<T>>(SerializerOptions, cancellationToken);

        return payload ?? new List<T>();
    }
}
