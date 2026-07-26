using System.ComponentModel.DataAnnotations;

namespace Admission.Application.Applicants.Dtos;

public sealed class SaveEducationDocumentRequest
{
    [Required]
    public Guid DocumentTypeId { get; init; }
}
