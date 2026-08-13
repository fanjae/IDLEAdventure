using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class CsvRow
{
    public int LineNumber { get; }
    public Dictionary<string, string> Values { get; }

    public CsvRow(int lineNumber, Dictionary<string, string> values)
    {
        LineNumber = lineNumber;
        Values = values;
    }
}

public sealed class CsvTable
{
    public TextAsset Source { get; }
    public IReadOnlyList<string> Headers { get; }
    public IReadOnlyList<CsvRow> Rows { get; }

    public CsvTable(TextAsset source, List<string> headers, List<CsvRow> rows)
    {
        Source = source;
        Headers = headers;
        Rows = rows;
    }
}

public static class CsvReader
{
    public static CsvTable Read(TextAsset csvAsset)
    {
        if (csvAsset == null)
        {
            throw new ArgumentNullException(nameof(csvAsset));
        }

        if (string.IsNullOrWhiteSpace(csvAsset.text))
        {
            throw new Exception($"{csvAsset.name} CSV가 비어 있습니다."
            );
        }

        string normalizedText = csvAsset.text.Replace("\r\n", "\n").Replace('\r', '\n');

        string[] lines = normalizedText.Split('\n');

        List<string> headers = ParseLine(lines[0], csvAsset.name, 1);

        HeadersInspection(csvAsset.name, headers);

        List<CsvRow> rows = new List<CsvRow>();

        for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex];

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            int csvLineNumber = lineIndex + 1;

            List<string> values = ParseLine(line, csvAsset.name, csvLineNumber);

            if (values.Count > headers.Count)
            {
                throw new Exception($"{csvAsset.name} {csvLineNumber}행의 값 개수가 헤더 개수보다 많음 헤더: {headers.Count}, 값: {values.Count}");
            }

            Dictionary<string, string> row = new Dictionary<string, string>(headers.Count, StringComparer.OrdinalIgnoreCase);

            for (int Index = 0; Index < headers.Count; Index++)
            {
                string value = Index < values.Count
                    ? values[Index].Trim()
                    : string.Empty;

                row.Add(headers[Index], value);
            }

            rows.Add(new CsvRow(csvLineNumber, row));
        }

        return new CsvTable(csvAsset, headers, rows);
    }

    private static void HeadersInspection(string csvName, List<string> headers)
    {
        if (headers.Count == 0)
        {
            throw new Exception($"{csvName}에 헤더 없음");
        }

        headers[0] = headers[0].TrimStart('\uFEFF').Trim();

        HashSet<string> hashHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < headers.Count; i++)
        {
            headers[i] = headers[i].Trim();

            if (string.IsNullOrWhiteSpace(headers[i]))
            {
                throw new Exception($"{csvName}의 {i + 1}번째 헤더가 비어 있음");
            }

            if (!hashHeaders.Add(headers[i]))
            {
                throw new Exception($"{csvName}에 중복된 헤더 있음 {headers[i]}");
            }
        }
    }

    private static List<string> ParseLine(string line, string csvName, int lineNumber)
    {
        List<string> values = new List<string>();
        StringBuilder currentValue = new StringBuilder();

        bool Checkchar = false;

        for (int i = 0; i < line.Length; i++)
        {
            char current = line[i];

            if (current == '"')
            {
                bool isEscape = Checkchar && i + 1 < line.Length && line[i + 1] == '"';

                if (isEscape)
                {
                    currentValue.Append('"');
                    i++;
                }
                else
                {
                    Checkchar = !Checkchar;
                }

                continue;
            }

            if (current == ',' && !Checkchar)
            {
                values.Add(currentValue.ToString());
                currentValue.Clear();
                continue;
            }

            currentValue.Append(current);
        }

        if (Checkchar)
        {
            throw new Exception($"{csvName} {lineNumber}행의 큰따옴표가 정상적으로 닫히지 않음");
        }

        values.Add(currentValue.ToString());

        return values;
    }
}