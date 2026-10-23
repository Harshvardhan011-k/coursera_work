using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models;

/// <summary>
/// Request payload for creating a new user.
/// ID is omitted because it is generated safely by the server.
/// </summary>
public class UserCreateRequest
{
    private string _name = string.Empty;
    private string _email = string.Empty;

    /// <summary>
    /// User name, trimmed of leading and trailing whitespace.
    /// Must be between 2 and 80 characters.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 80 characters.")]
    public string Name
    {
        get => _name;
        set => _name = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// User email address, trimmed and validated for format.
    /// Must be unique across all users (case-insensitive).
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(120, ErrorMessage = "Email must not exceed 120 characters.")]
    public string Email
    {
        get => _email;
        set => _email = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// User age, must be between 1 and 120.
    /// </summary>
    [Required(ErrorMessage = "Age is required.")]
    [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
    public int Age { get; set; }
}
