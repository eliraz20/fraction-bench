# Fraction Bench

Compare fractions using the smallest common denominator and show the equivalent numerators.

## Try it

Install the .NET 10 SDK, open this folder, and run:

```sh
dotnet run -- example.tsv
```

Look for `16/24 > 15/24` in the output. To use your own data, copy the example and pass the new filename. Use `-` to read from standard input.

## Input

One comparison per row: numerator A, positive denominator A, numerator B, positive denominator B. Separate columns with actual tabs, omit a header row, and use a period for decimal fractions.

Built for literacy and numeracy coursework. [ISCED-F field 002](https://uis.unesco.org/sites/default/files/documents/isced-fields-of-education-and-training-2013-en.pdf).
