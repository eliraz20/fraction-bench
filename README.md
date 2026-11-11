# Fraction Bench

Practice fraction comparisons, see misplaced answers on a number line, and revisit personal mistakes next lesson.

## Try it

From this folder with the .NET 10 SDK installed:

```sh
dotnet run -- lesson learning.db study.tsv
```

Run lesson learning.db for interactive practice (enter <, =, > or q). For a tutor-led import use four integers and the learner’s answer per TSV row. A mistake generates a related pair, stores the missed problem and displays both positions on a scaled number line. A correct replay clears that problem.

The included study.tsv uses tab-separated fields. Comment lines start with `#`.

The database is created on first use. Use a different database filename for another class or team.

The original exercise remains available with `dotnet run -- example.tsv`.

## When to use it

A tutor needs a record of which comparisons a learner missed, and a related question for the next meeting.

Example result:

```text
-7/12  ?  -4/7
Both are left of zero. The one farther left is smaller.
On a common scale: -49/84 < -48/84.
-49/84 |*································| 84/84  A=-7/12, B=-4/7, * = same display position
Try the reflection: 7/12 ? 4/7
2/3  ?  5/8
Correct.
Review next lesson: 1 missed pair(s).
```

Related course or field workflow: [Research on preservice teachers diagnosing fraction errors](https://doi.org/10.3390/math9161870).
