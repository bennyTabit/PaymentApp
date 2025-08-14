using System.ComponentModel.DataAnnotations;

namespace PaymentApp.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    public ICollection<Payment>? Payments { get; set; }
}
