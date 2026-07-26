using System.ComponentModel.DataAnnotations;

namespace Admission.Application.Managers.Dtos;

public sealed class AssignManagerRequest
{
    [Required]
    public Guid ManagerId { get; init; }
}
