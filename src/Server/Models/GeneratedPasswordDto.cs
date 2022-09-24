using System.ComponentModel.DataAnnotations;

namespace OneTimePassGen.Server.Models;

/// <summary>
///     Represents a generated password that a user possesses.
/// </summary>
public sealed class GeneratedPasswordDto
{
    /// <summary>
    ///     Gets or sets the identifier for this generated password.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the primary key of the user that is linked to this generated password.
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the generated password as plain text.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date and time, in UTC, when the generated password expires.
    /// </summary>
    /// <remarks>
    ///     A value in the feature means the generated password is not expired.
    /// </remarks>
    [Required]
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time, in UTC, when the entry was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}