namespace OneTimePassGen.Domain.Entities;

/// <summary>
///     Represents a generated password that a user possesses.
/// </summary>
public sealed class UserGeneratedPassword
{
    /// <summary>
    ///     Gets or sets the identifier for this generated password.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the primary key of the user that is linked to this generated password.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the generated password as plain text.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date and time, in UTC, when the generated password expiers.
    /// </summary>
    /// <remarks>
    ///     A value in the feature means the generated password is not expierd.
    /// </remarks>
    public DateTimeOffset ExpiersAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time, in UTC, when the entry was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}