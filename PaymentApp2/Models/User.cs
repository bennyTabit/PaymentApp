using System.ComponentModel.DataAnnotations;

namespace PaymentApp.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
