using System.Globalization;
using System.Numerics;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
if (args.Length > 0 && args[0] == "lesson")
{
    try { LessonReport.Run(args); return 0; }
    catch (Exception e) when (e is FormatException or IOException or ArgumentException or OverflowException or UnauthorizedAccessException or Microsoft.Data.Sqlite.SqliteException)
    { Console.Error.WriteLine(e.Message); return 1; }
}

if (args.Length != 1 || args[0] == "--help")
{
    Console.WriteLine("Usage: dotnet run -- lesson learning.db study.tsv  |  original format: <input-file|->");
    return args.Length == 1 ? 0 : 2;
}

try
{
    string[] lines = args[0] == "-" ? Console.In.ReadToEnd().ReplaceLineEndings("\n").TrimEnd('\n').Split('\n') : File.ReadAllLines(args[0]);
    Check(lines.Length >= 1 && lines.Any(line => !string.IsNullOrWhiteSpace(line)), "Input is empty or missing records.");
    foreach (var line in lines)
    {
        var f = Fields(line, 4);
        BigInteger a = BigInteger.Parse(f[0]), b = BigInteger.Parse(f[1]), c = BigInteger.Parse(f[2]), d = BigInteger.Parse(f[3]);
        Check(b > 0 && d > 0, "Denominators must be positive.");
        var common = b / BigInteger.GreatestCommonDivisor(b, d) * d;
        var left = a * (common / b);
        var right = c * (common / d);
        string sign = left == right ? "=" : left < right ? "<" : ">";
        Console.WriteLine($"{left}/{common} {sign} {right}/{common}");
    }

    return 0;
}
catch (Exception e)when (e is IOException or FormatException or OverflowException or ArgumentException or UnauthorizedAccessException or DivideByZeroException)
{
    Console.Error.WriteLine(e.Message);
    return 1;
}

static void Check(bool valid, string message)
{
    if (!valid)
        throw new FormatException(message);
}

static string[] Fields(string line, int count)
{
    var fields = line.Split('\t', StringSplitOptions.TrimEntries);
    Check(fields.Length == count && fields.All(s => s.Length > 0), $"Expected {count} tab-separated values in '{line}'.");
    return fields;
}
