using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

public static class CsvMapper
{
    private sealed class FieldBinding
    {
        public FieldInfo Field;
        public string[] CandidateNames;
        public bool IsOptional;

        public CsvMinAttribute MinAttribute;
        public CsvMaxAttribute MaxAttribute;
        public CsvRangeAttribute RangeAttribute;
    }

    private sealed class CompleteField
    {
        public FieldBinding Binding;
        public string HeaderName;
    }

    private static readonly Dictionary<Type, FieldBinding[]> fieldCache = new();

    public static List<T> Read<T>(TextAsset csvAsset, bool noneColumn = true) where T : new()
    {
        CsvTable table = CsvReader.Read(csvAsset);

        FieldBinding[] bindings = GetFieldBindings(typeof(T));

        List<CompleteField> completeFields = CompleteFields(table, typeof(T), bindings);

        if (noneColumn)
        {
            NoneColumn(table, typeof(T), completeFields);
        }

        List<T> results = new List<T>(table.Rows.Count);

        foreach (CsvRow csvRow in table.Rows)
        {
            T instance = new T();

            foreach (CompleteField completeField in completeFields)
            {
                FieldBinding binding = completeField.Binding;

                if (completeField.HeaderName == null)
                {
                    continue;
                }

                string rawValue = csvRow.Values[completeField.HeaderName];

                if (string.IsNullOrWhiteSpace(rawValue))
                {
                    if (binding.IsOptional)
                    {
                        continue;
                    }

                    throw new Exception($"{csvAsset.name} {csvRow.LineNumber}행의 필수 값이 비어 있음 열: {completeField.HeaderName}, 필드: {binding.Field.Name}");
                }

                object convertedValue = ConvertValue(rawValue, binding.Field.FieldType, csvAsset.name, csvRow.LineNumber, binding.Field.Name);

                binding.Field.SetValue(instance, convertedValue);

                FieldValueInspection(binding, convertedValue, csvAsset.name, csvRow.LineNumber);
            }

            ObjectInspection(instance, csvAsset.name, csvRow.LineNumber);

            results.Add(instance);
        }

        return results;
    }

    private static FieldBinding[] GetFieldBindings(Type type)
    {
        if (fieldCache.TryGetValue(type, out FieldBinding[] cachedBindings))
        {
            return cachedBindings;
        }

        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

        List<FieldBinding> bindings = new List<FieldBinding>(fields.Length);

        foreach (FieldInfo field in fields)
        {
            if (field.GetCustomAttribute<CsvIgnoreAttribute>() != null)
            {
                continue;
            }

            if (field.IsInitOnly)
            {
                throw new Exception($"{type.Name}.{field.Name}은 readonly이므로 CSV 값을 넣을 수 없음");
            }

            List<string> candidateNames = new List<string>();

            AddName(candidateNames, field.Name);

            CsvColumnAttribute columnAttribute = field.GetCustomAttribute<CsvColumnAttribute>();

            if (columnAttribute != null)
            {
                foreach (string name in columnAttribute.Names)
                {
                    AddName(candidateNames, name);
                }
            }

            FieldBinding binding = new FieldBinding
            {
                Field = field,
                CandidateNames = candidateNames.ToArray(),
                IsOptional = field.GetCustomAttribute<CsvOptionalAttribute>() != null,
                MinAttribute = field.GetCustomAttribute<CsvMinAttribute>(),
                MaxAttribute = field.GetCustomAttribute<CsvMaxAttribute>(),
                RangeAttribute = field.GetCustomAttribute<CsvRangeAttribute>()
            };

            bindings.Add(binding);
        }

        FieldBinding[] result = bindings.ToArray();

        fieldCache.Add(type, result);

        return result;
    }

    private static List<CompleteField> CompleteFields(CsvTable table, Type targetType, FieldBinding[] bindings)
    {
        List<CompleteField> completeFields = new List<CompleteField>(bindings.Length);

        foreach (FieldBinding binding in bindings)
        {
            string matchedHeader = FindMatchingName(table.Headers, binding.CandidateNames, table.Source.name, binding.Field.Name);

            if (matchedHeader == null && !binding.IsOptional)
            {
                throw new Exception($"{table.Source.name}에 {targetType.Name}.{binding.Field.Name}과 연결할 필수 열이 없음. 허용 이름: {string.Join(", ", binding.CandidateNames)}");
            }

            CompleteField completeField = new CompleteField
            {
                Binding = binding,
                HeaderName = matchedHeader
            };

            completeFields.Add(completeField);
        }

        return completeFields;
    }

    private static void FieldValueInspection(FieldBinding binding, object value, string sourceName, int lineNumber)
    {
        if (binding.MinAttribute == null && binding.MaxAttribute == null && binding.RangeAttribute == null)
        {
            return;
        }

        double number = ConvertToDouble(value, sourceName, lineNumber, binding.Field.Name);

        CsvMinAttribute min = binding.MinAttribute;

        if (min != null)
        {
            bool invalid = min.Inclusive ? number < min.MinValue : number <= min.MinValue;

            if (invalid)
            {
                string comparison = min.Inclusive ? "이상" : "초과";

                throw new Exception($"{sourceName} {lineNumber}행의 {binding.Field.Name} 값은 {min.MinValue} {comparison}여야 함 현재 값: {number}");
            }
        }

        CsvMaxAttribute max = binding.MaxAttribute;

        if (max != null)
        {
            bool invalid = max.Inclusive ? number > max.MaxValue : number >= max.MaxValue;

            if (invalid)
            {
                string comparison = max.Inclusive ? "이하" : "미만";

                throw new Exception($"{sourceName} {lineNumber}행의 {binding.Field.Name} 값은 {max.MaxValue} {comparison}여야 함 현재 값: {number}");
            }
        }

        CsvRangeAttribute range = binding.RangeAttribute;

        if (range != null)
        {
            if (number < range.MinValue || number > range.MaxValue)
            {
                throw new Exception($"{sourceName} {lineNumber}행의 {binding.Field.Name} 값은 {range.MinValue}~{range.MaxValue} 범위여야 함 현재 값: {number}");
            }
        }
    }

    private static double ConvertToDouble(object value, string sourceName, int lineNumber, string fieldName)
    {
        if (value == null)
        {
            throw new Exception($"{sourceName} {lineNumber}행의 {fieldName} 값을 검사 할 수 없음.");
        }

        try
        {
            return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            throw new Exception($"{sourceName} {lineNumber}행의 {fieldName}에 숫자 검증 Attribute가 붙어 있지만 숫자 자료형이 아님");
        }
    }

    private static void ObjectInspection<T>(T instance, string sourceName, int lineNumber)
    {
        if (instance is ICsvValidatable validatable)
        {
            validatable.ValidateCsv(sourceName, lineNumber);
        }
    }

    private static object ConvertValue(string rawValue, Type targetType, string sourceName, int lineNumber, string fieldName)
    {
        Type nullableType = Nullable.GetUnderlyingType(targetType);

        if (nullableType != null)
        {
            targetType = nullableType;
        }

        if (targetType == typeof(string))
        {
            return rawValue;
        }

        if (targetType == typeof(int))
        {
            if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
            {
                return intValue;
            }
        }
        else if (targetType == typeof(long))
        {
            if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out long longValue))
            {
                return longValue;
            }
        }
        else if (targetType == typeof(float))
        {
            if (float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
            {
                return floatValue;
            }
        }
        else if (targetType == typeof(double))
        {
            if (double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue))
            {
                return doubleValue;
            }
        }
        else if (targetType == typeof(decimal))
        {
            if (decimal.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal decimalValue))
            {
                return decimalValue;
            }
        }
        else if (targetType == typeof(bool))
        {
            if (bool.TryParse(rawValue, out bool boolValue))
            {
                return boolValue;
            }

            if (rawValue == "1")
            {
                return true;
            }

            if (rawValue == "0")
            {
                return false;
            }
        }
        else if (targetType.IsEnum)
        {
            if (Enum.TryParse(targetType, rawValue, true, out object enumValue))
            {
                return enumValue;
            }
        }
        else
        {
            throw new Exception($"{sourceName}에서 지원하지 않는 CSV 자료 필드: {fieldName}, 자료형: {targetType.Name}");
        }

        throw new Exception($"{sourceName} {lineNumber}행의 '{rawValue}' 값을 {targetType.Name}으로 변환할 수 없음 필드: {fieldName}");
    }

    private static string FindMatchingName(IEnumerable<string> availableNames, string[] candidateNames, string sourceName, string fieldName)
    {
        string matchedName = null;

        foreach (string candidate in candidateNames)
        {
            foreach (string available in availableNames)
            {
                if (!string.Equals(candidate, available, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (matchedName != null && !string.Equals(matchedName, available, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception($"{sourceName}에서 {fieldName} 필드에 연결할 수 있는 이름이 두 개 이상 : {matchedName}, {available}");
                }

                matchedName = available;
            }
        }

        return matchedName;
    }

    private static void NoneColumn(CsvTable table, Type targetType, List<CompleteField> completeFields)
    {
        HashSet<string> usedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (CompleteField completeField in completeFields)
        {
            if (completeField.HeaderName != null)
            {
                usedHeaders.Add(completeField.HeaderName);
            }
        }

        foreach (string header in table.Headers)
        {
            if (!usedHeaders.Contains(header))
            {
                Debug.LogWarning($"{table.Source.name}의 '{header}' 열은 {targetType.Name}에서 사용하지 않음");
            }
        }
    }

    private static void AddName(List<string> names, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        foreach (string existingName in names)
        {
            if (string.Equals(existingName, name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        names.Add(name);
    }
}