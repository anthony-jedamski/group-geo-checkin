using System.ComponentModel.DataAnnotations;

namespace GeoCheckInBackend.Models.Requests;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "UserName is required.")]
    [StringLength(100, ErrorMessage = "UserName cannot be longer than 100 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "UserName can only contain letters, numbers, and underscores.")]
    [Display(Name = "User Name")]
    public required string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "OldGroupName is required.")]
    [StringLength(100, ErrorMessage = "OldGroupName cannot be longer than 100 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_ ]+$", ErrorMessage = "OldGroupName can only contain letters, numbers, spaces, and underscores.")]
    [Display(Name = "Old Group Name")]
    public string OldGroupName { get; set; } = string.Empty;
    [Required(ErrorMessage = "NewGroupName is required.")]
    [StringLength(100, ErrorMessage = "NewGroupName cannot be longer than 100 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_ ]+$", ErrorMessage = "NewGroupName can only contain letters, numbers, spaces, and underscores.")]
    [Display(Name = "New Group Name")]
    public string NewGroupName { get; set; } = string.Empty;
}
