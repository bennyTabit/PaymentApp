using System.ComponentModel.DataAnnotations;

namespace PaymentApp.Models.DTOs;

public class CreateUserDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }
}

public class UpdateUserDto
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }
}

public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}
