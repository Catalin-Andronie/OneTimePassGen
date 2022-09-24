using OneTimePassGen.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace OneTimePassGen.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    /// <summary>
    ///     Gets or sets the <see cref="DbSet<GeneratedPassword>"/>(s) of User generated passwords.
    /// </summary>
    DbSet<UserGeneratedPassword> UserGeneratedPasswords { get; }

    /// <summary>
    ///     Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken"/> to observe while waiting for the task to
    ///     complete.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains
    ///     the number of state entries written to the database.
    /// </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}