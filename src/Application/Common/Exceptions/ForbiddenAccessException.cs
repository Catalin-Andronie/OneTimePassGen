﻿using System.Runtime.Serialization;

namespace OneTimePassGen.Application.Common.Exceptions;

[Serializable]
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
    {
    }

    public ForbiddenAccessException(string? message)
        : base(message)
    {
    }

    public ForbiddenAccessException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected ForbiddenAccessException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}