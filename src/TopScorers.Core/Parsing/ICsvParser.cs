using TopScorers.Core.Models;

namespace TopScorers.Core.Parsing;

public interface ICsvParser
{
    IReadOnlyList<PersonScore> Parse(string csvContent);
}

