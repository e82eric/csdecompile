using System;

internal class InvalidSolutionFileException : Exception
{
    public InvalidSolutionFileException(string message) : base(message)
    {
    }
}