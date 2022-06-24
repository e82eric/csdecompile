using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

public class ProjectIdInfo
{
    public ProjectIdInfo(Microsoft.CodeAnalysis.ProjectId id, bool isDefinedInSolution)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        IsDefinedInSolution = isDefinedInSolution;
    }

    public Microsoft.CodeAnalysis.ProjectId Id { get; set; }
    public bool IsDefinedInSolution { get; set; }

    /// <summary>
    /// Project configurations as defined in solution.
    /// Keys are solution build configuration in '$(Configuration)|$(Platform)' format,
    /// values are according project configurations. Null if there is no solution.
    /// </summary>
    public IReadOnlyDictionary<string, string> SolutionConfiguration { get; set; }
}