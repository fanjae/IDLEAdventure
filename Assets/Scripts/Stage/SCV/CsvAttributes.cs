using System;

[AttributeUsage(AttributeTargets.Field)]
public sealed class CsvIgnoreAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class CsvOptionalAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class CsvColumnAttribute : Attribute
{
    public string[] Names { get; }

    public CsvColumnAttribute(params string[] names)
    {
        if (names == null || names.Length == 0)
        {
            throw new ArgumentException("CSV 열 이름을 하나 이상 지정해야 함", nameof(names));
        }

        Names = names;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class CsvMinAttribute : Attribute
{
    public double MinValue { get; }
    public bool Inclusive { get; }

    public CsvMinAttribute(double minValue, bool inclusive = true)
    {
        MinValue = minValue;
        Inclusive = inclusive;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class CsvMaxAttribute : Attribute
{
    public double MaxValue { get; }
    public bool Inclusive { get; }

    public CsvMaxAttribute(double maxValue, bool inclusive = true)
    {
        MaxValue = maxValue;
        Inclusive = inclusive;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class CsvRangeAttribute : Attribute
{
    public double MinValue { get; }
    public double MaxValue { get; }

    public CsvRangeAttribute(double minValue, double maxValue)
    {
        if (minValue > maxValue)
        {
            throw new ArgumentException("CsvRange의 최솟값이 최댓값보다 클 수 없음");
        }

        MinValue = minValue;
        MaxValue = maxValue;
    }
}
public interface ICsvValidatable
{
    void ValidateCsv(string sourceName, int lineNumber);
}