using System;
using System.ComponentModel.DataAnnotations;
using DHAFacilitationAPIs.Domain.Common;

namespace DHAFacilitationAPIs.Domain.Entities;

public class Club : BaseAuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? ContactNumber { get; set; }
    public string? AccountCode { get; set; }
}
