using Shared.Kernel.Exceptions;

namespace Admission.Domain.Applicants;

public sealed class Passport : ApplicantDocument
{
    public string Series { get; private set; } = string.Empty;

    public string PlaceOfBirth { get; private set; } = string.Empty;

    public string IssuedBy { get; private set; } = string.Empty;

    public DateOnly IssueDate { get; private set; }

    private Passport()
    {
    }

    private Passport(
        Guid applicantId,
        string series,
        string placeOfBirth,
        string issuedBy,
        DateOnly issueDate)
        : base(applicantId)
    {
        Series = series;
        PlaceOfBirth = placeOfBirth;
        IssuedBy = issuedBy;
        IssueDate = issueDate;
    }

    internal static Passport Create(
        Guid applicantId,
        string series,
        string placeOfBirth,
        string issuedBy,
        DateOnly issueDate)
    {
        Validate(series, placeOfBirth, issuedBy, issueDate);
        return new Passport(applicantId, series.Trim(), placeOfBirth.Trim(), issuedBy.Trim(), issueDate);
    }

    public void Update(
        string? series,
        string? placeOfBirth,
        string? issuedBy,
        DateOnly? issueDate)
    {
        if (series is not null)
        {
            RequireText(series, nameof(series));
            Series = series.Trim();
        }

        if (placeOfBirth is not null)
        {
            RequireText(placeOfBirth, nameof(placeOfBirth));
            PlaceOfBirth = placeOfBirth.Trim();
        }

        if (issuedBy is not null)
        {
            RequireText(issuedBy, nameof(issuedBy));
            IssuedBy = issuedBy.Trim();
        }

        if (issueDate is not null)
        {
            RequireIssueDate(issueDate.Value);
            IssueDate = issueDate.Value;
        }

        LastModifiedUtc = DateTime.UtcNow;
    }

    private static void Validate(string series, string placeOfBirth, string issuedBy, DateOnly issueDate)
    {
        RequireText(series, nameof(series));
        RequireText(placeOfBirth, nameof(placeOfBirth));
        RequireText(issuedBy, nameof(issuedBy));
        RequireIssueDate(issueDate);
    }

    private static void RequireText(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException(field, $"{field} is required.");
        }
    }

    private static void RequireIssueDate(DateOnly issueDate)
    {
        if (issueDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ValidationException(nameof(issueDate), "Issue date cannot be in the future.");
        }
    }
}
