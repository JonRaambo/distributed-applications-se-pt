using System.ComponentModel.DataAnnotations;

namespace CarService.Api.Dtos;

public class UpdateDocumentDto
{
    [StringLength(200)]
    public string? Description { get; set; }
}
