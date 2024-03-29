﻿using FluentValidation;
using FluentValidation.Results;

using MediatR;

using ValidationException = OneTimePassGen.Application.Common.Exceptions.ValidationException;

namespace OneTimePassGen.Application.Common.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        if (_validators.Any())
        {
            ValidationContext<TRequest> context = new(request);

            IEnumerable<Task<ValidationResult>> validationTasks =
                _validators.Select(v => v.ValidateAsync(context, cancellationToken));

            ValidationResult[] validationResults =
                await Task
                    .WhenAll(validationTasks)
                    .ConfigureAwait(false);

            List<ValidationFailure> failures = validationResults
                .Where(r => r.Errors.Count > 0)
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Count > 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next().ConfigureAwait(false);
    }
}