using System.Runtime.Serialization;

using FluentValidation.Results;

namespace OneTimePassGen.Application.Common.Exceptions;

[Serializable]
public class ValidationException : Exception
{
    public ValidationException()
    {
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures have occurred.")
    {
        var errors = failures?.GroupBy(e => e.PropertyName, e => e.ErrorMessage, StringComparer.Ordinal)
                          .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray(), StringComparer.Ordinal);

        Errors = errors ?? new Dictionary<string, string[]>(StringComparer.Ordinal);
    }

    public ValidationException(string? message)
        : base(message)
    {
    }

    public ValidationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected ValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public IDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>(StringComparer.Ordinal);
}