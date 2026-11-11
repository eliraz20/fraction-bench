using Microsoft.Data.Sqlite;

static class LessonReport
{
    public static void Run(string[] args)
    {
        if (args.Length != 2 && args.Length != 3)
            throw new FormatException("Usage: lesson learning.db study.tsv");
        using var db = Open(args[1]);
        Sql(db, "CREATE TABLE IF NOT EXISTS mistake(a TEXT,b TEXT,c TEXT,d TEXT,PRIMARY KEY(a,b,c,d))");
        var review = Query(db, "SELECT a,b,c,d FROM mistake ORDER BY rowid");
        bool interactive = args.Length == 2;
        using var import = interactive ? null : Begin(db);
        var deck = interactive ? review.ToList() : Rows(args[2], 5);
        if (interactive)
        {
            for (int item = 0; item < 6; item++)
            {
                int denominator = Random.Shared.Next(3, 12);
                int numerator = Random.Shared.Next(1, denominator);
                int sign = item % 2 == 0 ? -1 : 1;
                int otherNumerator = item % 3 == 0 ? 2 * numerator : numerator;
                int otherDenominator = item % 3 == 0 ? 2 * denominator : denominator + Random.Shared.Next(1, 5);
                deck.Add([(sign * numerator).ToString(), denominator.ToString(), (sign * otherNumerator).ToString(), otherDenominator.ToString()]);
            }
        }
        var seen = new HashSet<string>();
        for (int i = 0; i < deck.Count; i++)
        {
            var r = deck[i];
            var a = System.Numerics.BigInteger.Parse(r[0]);
            var b = System.Numerics.BigInteger.Parse(r[1]);
            var c = System.Numerics.BigInteger.Parse(r[2]);
            var d = System.Numerics.BigInteger.Parse(r[3]);
            Require(b > 0 && d > 0, "Use positive denominators.");
            var key = new[]
            {
                a.ToString(),
                b.ToString(),
                c.ToString(),
                d.ToString()
            };
            if (interactive && !seen.Add(string.Join("/", key)))
                continue;
            Console.WriteLine($"{a}/{b}  ?  {c}/{d}");
            var answer = interactive ? Console.ReadLine()?.Trim() : r[4];
            if (answer is null or "q")
                break;
            Require(answer is "<" or "=" or ">", "Enter <, =, > or q.");
            var comparison = (a * d).CompareTo(c * b);
            var sign = comparison < 0 ? "<" : comparison > 0 ? ">" : "=";
            if (answer == sign)
            {
                Console.WriteLine("Correct.");
                Sql(db, "DELETE FROM mistake WHERE a=$a AND b=$b AND c=$c AND d=$d", key);
                continue;
            }

            Sql(db, "INSERT INTO mistake VALUES($a,$b,$c,$d) ON CONFLICT DO NOTHING", key);
            if (a < 0 && c < 0) Console.WriteLine("Both are left of zero. The one farther left is smaller.");
            else if (a * d == c * b) Console.WriteLine("Both fractions locate the same point even though the numerators and denominators differ.");
            Console.WriteLine($"On a common scale: {a * d}/{b * d} {sign} {c * b}/{b * d}.");
            var lo = System.Numerics.BigInteger.Min(0, System.Numerics.BigInteger.Min(a * d, c * b));
            var hi = System.Numerics.BigInteger.Max(b * d, System.Numerics.BigInteger.Max(a * d, c * b));
            var line = new string ('·', 33).ToCharArray();
            int x = (int)(32 * (a * d - lo) / (hi - lo)), y = (int)(32 * (c * b - lo) / (hi - lo));
            line[x] = 'A';
            line[y] = x == y ? '*' : 'B';
            Console.WriteLine($"{lo}/{b * d} |{new string (line)}| {hi}/{b * d}  A={a}/{b}, B={c}/{d}, * = same display position");
            var related = new[]
            {
                (-a).ToString(),
                b.ToString(),
                (-c).ToString(),
                d.ToString()
            };
            Console.WriteLine($"Try the reflection: {related[0]}/{b} ? {related[2]}/{d}");
            if (interactive)
                deck.Insert(i + 1, related);
        }

        import?.Commit();
        Console.WriteLine($"Review next lesson: {Query(db, "SELECT a FROM mistake").Count} missed pair(s).");
    }

    static List<string[]> Rows(string file, int width = 0, bool allowEmpty = false)
    {
        using var reader = file == "-" ? Console.In : File.OpenText(file);
        var rows = new List<string[]>();
        int line = 0;
        while (reader.ReadLine()is string s)
        {
            line++;
            if (string.IsNullOrWhiteSpace(s) || s.TrimStart().StartsWith("#"))
                continue;
            var cells = s.Split('\t', StringSplitOptions.TrimEntries);
            if ((width > 0 && cells.Length != width) || (!allowEmpty && cells.Any(string.IsNullOrEmpty)))
                throw new FormatException($"Invalid columns on line {line}.");
            rows.Add(cells);
        }

        if (rows.Count == 0)
            throw new FormatException("Empty input.");
        return rows;
    }

    static void Require(bool condition, string message)
    {
        if (!condition)
            throw new FormatException(message);
    }

    static SqliteTransaction? transaction;
    static SqliteConnection Open(string file)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = file, ForeignKeys = true }.ToString());
        connection.Open();
        return connection;
    }

    static SqliteTransaction Begin(SqliteConnection db) => transaction = db.BeginTransaction();
    static SqliteCommand Command(SqliteConnection db, string sql, string[] values)
    {
        var cmd = db.CreateCommand();
        if (transaction?.Connection != null)
            cmd.Transaction = transaction;
        cmd.CommandText = sql;
        for (int i = 0; i < values.Length; i++)
            cmd.Parameters.AddWithValue("$" + (char)(97 + i), values[i]);
        return cmd;
    }

    static void Sql(SqliteConnection db, string sql, params string[] values)
    {
        using var cmd = Command(db, sql, values);
        cmd.ExecuteNonQuery();
    }

    static List<string[]> Query(SqliteConnection db, string sql, params string[] values)
    {
        using var cmd = Command(db, sql, values);
        using var reader = cmd.ExecuteReader();
        var records = new List<string[]>();
        while (reader.Read())
            records.Add(Enumerable.Range(0, reader.FieldCount).Select(i => Convert.ToString(reader.GetValue(i), System.Globalization.CultureInfo.InvariantCulture) ?? "").ToArray());
        return records;
    }
}
