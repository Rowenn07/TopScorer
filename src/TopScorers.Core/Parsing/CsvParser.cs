using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using TopScorers.Core.Models;

namespace TopScorers.Core.Parsing;

public class CsvParser(ILogger<CsvParser> logger) : ICsvParser
{
    private static readonly string[] FirstNameHeaders = ["first name", "firstname"];
    private static readonly string[] SecondNameHeaders = ["second name", "secondname", "surname", "last name"];
    private static readonly string[] ScoreHeaders = ["score", "mark", "points"];

    public IReadOnlyList<PersonScore> Parse(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
        {
            logger.LogWarning("CSV content was empty");
            return Array.Empty<PersonScore>();
        }

        var lines = csvContent.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            logger.LogWarning("CSV content did not contain any lines");
            return Array.Empty<PersonScore>();
        }

        var headerMap = BuildHeaderMap(ParseLine(lines[0]));
        if (headerMap is null)
        {
            logger.LogWarning("CSV headers missing required columns (First Name, Second Name, Score)");
            return Array.Empty<PersonScore>();
        }

        var results = new List<PersonScore>();
        for (var i = 1; i < lines.Length; i++)
        {
            var fields = ParseLine(lines[i]);
            if (fields.Count == 0)
            {
                continue;
            }

            try
            {
                var firstName = GetField(fields, headerMap.Value.FirstNameIndex);
                var secondName = GetField(fields, headerMap.Value.SecondNameIndex);
                var scoreField = GetField(fields, headerMap.Value.ScoreIndex);

                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(secondName))
                {
                    logger.LogWarning("Skipping row {Row} due to missing name values", i + 1);
                    continue;
                }

                if (!int.TryParse(scoreField, NumberStyles.Integer, CultureInfo.InvariantCulture, out var score))
                {
                    logger.LogWarning("Skipping row {Row} due to invalid score value '{Score}'", i + 1, scoreField);
                    continue;
                }

                results.Add(new PersonScore
                {
                    FirstName = firstName.Trim(),
                    SecondName = secondName.Trim(),
                    Score = score
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse row {Row}", i + 1);
            }
        }

        if (results.Count > 0)
        {
            var totalDataRows = lines.Length - 1; // Exclude header
            logger.LogInformation("Successfully parsed {ParsedCount} of {TotalRows} data rows from CSV", 
                results.Count, totalDataRows);
        }

        return results;
    }

    private static (int FirstNameIndex, int SecondNameIndex, int ScoreIndex)? BuildHeaderMap(IReadOnlyList<string> headers)
    {
        static int FindIndex(IReadOnlyList<string> source, string[] candidates)
        {
            for (var i = 0; i < source.Count; i++)
            {
                var name = source[i].Trim().ToLowerInvariant();
                if (candidates.Any(candidate => candidate.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return i;
                }
            }

            return -1;
        }

        var firstNameIndex = FindIndex(headers, FirstNameHeaders);
        var secondNameIndex = FindIndex(headers, SecondNameHeaders);
        var scoreIndex = FindIndex(headers, ScoreHeaders);

        if (firstNameIndex < 0 || secondNameIndex < 0 || scoreIndex < 0)
        {
            return null;
        }

        return (firstNameIndex, secondNameIndex, scoreIndex);
    }

    private static string GetField(IReadOnlyList<string> fields, int index) =>
        index >= 0 && index < fields.Count ? fields[index] : string.Empty;

    private static List<string> ParseLine(string line)
    {
        var result = new List<string>();
        var builder = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var current = line[i];

            if (current == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    builder.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (current == ',' && !inQuotes)
            {
                result.Add(builder.ToString());
                builder.Clear();
            }
            else
            {
                builder.Append(current);
            }
        }

        result.Add(builder.ToString());
        return result;
    }
}

