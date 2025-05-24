using System.ComponentModel.DataAnnotations;

namespace GeoCheckInBackend.Models.Requests;

public class RegisterUserRequest
{
    [Required(ErrorMessage = "UserName is required.")]
    [StringLength(100, ErrorMessage = "UserName cannot be longer than 100 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "UserName can only contain letters, numbers, and underscores.")]
    [Display(Name = "User Name")]
    public required string UserName { get; set; } = string .Empty;
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [Display(Name = "Email Address")]
    [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
    public required string Email { get; set; }
    public int? GroupId { get; set; }
    public string? GroupName { get; set; } = null;
}
