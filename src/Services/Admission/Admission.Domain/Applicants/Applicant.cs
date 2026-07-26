using Admission.Domain.Abstractions;
using Admission.Domain.Admissions;
using Admission.Domain.Catalogue;
using Shared.Kernel.Exceptions;

namespace Admission.Domain.Applicants;

public sealed class Applicant : AggregateRoot
{
    private readonly List<ApplicantDocument> _documents = new();
    private readonly List<ApplicantAdmission> _admissions = new();

    public Guid AuthId { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public DateOnly? BirthDate { get; private set; }

    public Gender? Gender { get; private set; }

    public string? Citizenship { get; private set; }

    public string? PhoneNumber { get; private set; }

    public DateTime LastModifiedUtc { get; private set; }

    public IReadOnlyCollection<ApplicantDocument> Documents => _documents.AsReadOnly();

    public IReadOnlyCollection<ApplicantAdmission> Admissions => _admissions.AsReadOnly();

    public Passport? Passport => _documents.OfType<Passport>().FirstOrDefault();

    public EducationDocument? EducationDocument => _documents.OfType<EducationDocument>().FirstOrDefault();

    private Applicant()
    {
    }

    private Applicant(Guid authId, string email, string fullName)
    {
        AuthId = authId;
        Email = email;
        FullName = fullName;
        LastModifiedUtc = DateTime.UtcNow;
    }

    public static Applicant Register(Guid authId, string email, string? fullName)
    {
        if (authId == Guid.Empty)
        {
            throw new DomainRuleException("An applicant requires a valid identity identifier.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainRuleException("An applicant requires an email address.");
        }

        return new Applicant(authId, email.Trim(), string.IsNullOrWhiteSpace(fullName)
            ? email.Trim()
            : fullName.Trim());
    }

    public void UpdateProfile(
        string? fullName,
        string? email,
        DateOnly? birthDate,
        Gender? gender,
        string? citizenship,
        string? phoneNumber)
    {
        if (fullName is not null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ValidationException(nameof(fullName), "Full name cannot be empty.");
            }

            FullName = fullName.Trim();
        }

        if (email is not null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationException(nameof(email), "Email cannot be empty.");
            }

            Email = email.Trim();
        }

        if (birthDate is not null)
        {
            if (birthDate.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new ValidationException(nameof(birthDate), "Birth date cannot be in the future.");
            }

            BirthDate = birthDate;
        }

        if (gender is not null)
        {
            if (!Enum.IsDefined(gender.Value))
            {
                throw new ValidationException(nameof(gender), "Unknown gender value.");
            }

            Gender = gender;
        }

        if (citizenship is not null)
        {
            Citizenship = citizenship.Trim();
        }

        if (phoneNumber is not null)
        {
            PhoneNumber = phoneNumber.Trim();
        }

        LastModifiedUtc = DateTime.UtcNow;
    }

    public void SyncFromIdentity(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            Email = email.Trim();
            LastModifiedUtc = DateTime.UtcNow;
        }
    }

    public Passport AddPassport(
        string series,
        string placeOfBirth,
        string issuedBy,
        DateOnly issueDate)
    {
        if (Passport is not null)
        {
            throw new ConflictException("This applicant already has a passport record.");
        }

        var passport = Passport.Create(Id, series, placeOfBirth, issuedBy, issueDate);
        _documents.Add(passport);
        LastModifiedUtc = DateTime.UtcNow;
        return passport;
    }

    public EducationDocument AddEducationDocument(EducationDocumentType documentType)
    {
        ArgumentNullException.ThrowIfNull(documentType);

        if (EducationDocument is not null)
        {
            throw new ConflictException(
                "This applicant already has an education document. Update the existing one instead.");
        }

        var document = EducationDocument.Create(Id, documentType);
        _documents.Add(document);
        LastModifiedUtc = DateTime.UtcNow;
        return document;
    }
}
