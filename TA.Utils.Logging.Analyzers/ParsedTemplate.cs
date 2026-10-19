using System.Collections.Generic;
using System.Linq;

namespace TA.Utils.Logging.Analyzers;

/// <summary>The placeholders found in a constant message template.</summary>
sealed class ParsedTemplate
{
    ParsedTemplate(IReadOnlyList<string> placeholderNames)
    {
        PlaceholderNames = placeholderNames;
        IsPositional = placeholderNames.Count > 0 && placeholderNames.All(IsIndex);
        ExpectedArguments = IsPositional ? placeholderNames.Max(int.Parse) + 1 : placeholderNames.Count;
    }

    /// <summary>Placeholder names in template order, without capture operators, alignment or format.</summary>
    public IReadOnlyList<string> PlaceholderNames { get; }

    /// <summary>True when every placeholder is a numeric index such as <c>{0}</c>.</summary>
    public bool IsPositional { get; }

    public int ExpectedArguments { get; }

    /// <summary>Parses a template, returning null if it is malformed.</summary>
    public static ParsedTemplate? Parse(string template)
    {
        var names = new List<string>();
        var position = 0;
        while (position < template.Length)
        {
            var character = template[position];
            if (character == '{' && position + 1 < template.Length && template[position + 1] == '{')
                position += 2;
            else if (character == '{')
            {
                var end = FindClosingBrace(template, position + 1);
                if (end < 0)
                    return null;
                var name = ExtractName(template.Substring(position + 1, end - position - 1));
                if (name.Length == 0)
                    return null;
                names.Add(name);
                position = end + 1;
            }
            else
                position++;
        }

        return new ParsedTemplate(names);
    }

    static int FindClosingBrace(string template, int start)
    {
        var inFormat = false;
        var position = start;
        while (position < template.Length)
        {
            var character = template[position];
            var isDoubled = position + 1 < template.Length && template[position + 1] == character;
            if (character == ':')
                inFormat = true;
            if (inFormat && isDoubled && (character == '{' || character == '}'))
                position += 2;
            else if (character == '}')
                return position;
            else if (character == '{')
                return -1;
            else
                position++;
        }

        return -1;
    }

    static string ExtractName(string hole)
    {
        var name = hole.TrimStart('@', '$');
        var end = name.IndexOfAny(new[] { ',', ':' });
        return (end < 0 ? name : name.Substring(0, end)).Trim();
    }

    static bool IsIndex(string name) => int.TryParse(name, out var index) && index >= 0;
}
