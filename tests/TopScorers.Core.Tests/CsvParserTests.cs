using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TopScorers.Core.Parsing;

namespace TopScorers.Core.Tests;

public class CsvParserTests
{
    private readonly CsvParser parser = new(NullLogger<CsvParser>.Instance);

    [Fact]
    public void Parse_ShouldHandleQuotedFieldsAndEscapedQuotes()
    {
        const string csv = """
            First Name,Second Name,Score
            "George","Of ""The"" Jungle",78
            """;

        var result = parser.Parse(csv);

        result.Should().HaveCount(1);
        result[0].FirstName.Should().Be("George");
        result[0].SecondName.Should().Be(@"Of ""The"" Jungle");
        result[0].Score.Should().Be(78);
    }

    [Fact]
    public void Parse_ShouldSkipRowsWithInvalidScores()
    {
        const string csv = """
            First Name,Second Name,Score
            Dee,Moore,not-a-number
            """;

        var result = parser.Parse(csv);

        result.Should().BeEmpty();
    }
}

