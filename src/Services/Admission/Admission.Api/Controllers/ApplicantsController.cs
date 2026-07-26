using Admission.Application.Applicants.Dtos;
using Admission.Application.Applicants.Contracts;
using Admission.Application.Applicants.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;

namespace Admission.Api.Controllers;

[ApiController]
[Route("api/v1/applicants")]
[Produces("application/json")]
public sealed class ApplicantsController : ControllerBase
{
    private readonly IApplicantProfileService _profiles;
    private readonly IApplicantDocumentService _documents;

    public ApplicantsController(
        IApplicantProfileService profiles,
        IApplicantDocumentService documents)
    {
        _profiles = profiles;
        _documents = documents;
    }

    [HttpGet("me")]
    [Authorize(Roles = Roles.Applicant)]
    [ProducesResponseType(typeof(ApplicantProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicantProfileResponse>> GetMyProfile(
        CancellationToken cancellationToken) =>
        Ok(await _profiles.GetMyProfileAsync(cancellationToken));

    [HttpPatch("me")]
    [Authorize(Roles = Roles.Applicant)]
    [ProducesResponseType(typeof(ApplicantProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApplicantProfileResponse>> UpdateMyProfile(
        [FromBody] UpdateApplicantProfileRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _profiles.UpdateMyProfileAsync(request, cancellationToken));

    [HttpGet("me/passport")]
    [Authorize(Roles = Roles.Applicant)]
    [ProducesResponseType(typeof(PassportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PassportResponse>> GetMyPassport(CancellationToken cancellationToken) =>
        Ok(await _documents.GetMyPassportAsync(cancellationToken));

    [HttpPost("me/passport")]
    [Authorize(Roles = Roles.Applicant)]
    [ProducesResponseType(typeof(PassportResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PassportResponse>> AddMyPassport(
        [FromBody] CreatePassportRequest request,
        CancellationToken cancellationToken)
    {
        var passport = await _documents.AddMyPassportAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetMyPassport), null, passport);
    }

    [HttpGet("me/education-document")]
    [Authorize(Roles = Roles.Applicant)]
    [ProducesResponseType(typeof(EducationDocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EducationDocumentResponse>> GetMyEducationDocument(
        CancellationToken cancellationToken) =>
        Ok(await _documents.GetMyEducationDocumentAsync(cancellationToken));

    [HttpPost("me/education-document")]
    [Authorize(Roles = Roles.Applicant)]
    [ProducesResponseType(typeof(EducationDocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EducationDocumentResponse>> AddMyEducationDocument(
        [FromBody] SaveEducationDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var document = await _documents.AddMyEducationDocumentAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetMyEducationDocument), null, document);
    }

    [HttpGet("{applicantId:guid}")]
    [Authorize(Roles = Roles.AnyManagerOrAdmin)]
    [ProducesResponseType(typeof(FullApplicantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FullApplicantResponse>> GetApplicant(
        Guid applicantId,
        CancellationToken cancellationToken) =>
        Ok(await _profiles.GetFullProfileAsync(applicantId, cancellationToken));

    [HttpPatch("{applicantId:guid}")]
    [Authorize(Roles = Roles.AnyManagerOrAdmin)]
    [ProducesResponseType(typeof(ApplicantProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicantProfileResponse>> UpdateApplicant(
        Guid applicantId,
        [FromBody] UpdateApplicantProfileRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _profiles.UpdateProfileAsync(applicantId, request, cancellationToken));

    [HttpPut("{applicantId:guid}/passport")]
    [Authorize(Roles = Roles.AnyManagerOrAdmin)]
    [ProducesResponseType(typeof(PassportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PassportResponse>> UpdatePassport(
        Guid applicantId,
        [FromBody] UpdatePassportRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _documents.UpdatePassportAsync(applicantId, request, cancellationToken));

    [HttpPut("{applicantId:guid}/education-document")]
    [Authorize(Roles = Roles.AnyManagerOrAdmin)]
    [ProducesResponseType(typeof(EducationDocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EducationDocumentResponse>> UpdateEducationDocument(
        Guid applicantId,
        [FromBody] SaveEducationDocumentRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _documents.UpdateEducationDocumentAsync(applicantId, request, cancellationToken));
}
