# Database Table Counts Comparison (Original vs New) ✅

This file explains how to collect table row counts from *each* SQL Server database (run the query in each database context), and provides a ready-to-use template + scripts to produce a side-by-side comparison markdown/CSV you can paste or upload.

---

## 1) Per-database SQL to get table row counts 🔧
Run this in each database (no cross-database required):

```sql
-- Run in the target database (use USE to change DB if needed)
SET NOCOUNT ON;
SELECT
  s.name AS schema_name,
  t.name AS table_name,
  SUM(p.rows) AS rows
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1)
WHERE t.is_ms_shipped = 0
GROUP BY s.name, t.name
ORDER BY SUM(p.rows) DESC;
```

Notes:
- Run once in the **original** DB and once in the **new** DB.
- Export results to CSV if you prefer automated merging.

---

## 2) Export to CSV (sqlcmd example) ⤓
Example using Windows Authentication and sqlcmd:

```powershell
sqlcmd -S "<server>" -d "OriginalDB" -E -W -s"," -Q "SET NOCOUNT ON; SELECT s.name, t.name, SUM(p.rows) rows FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1) WHERE t.is_ms_shipped = 0 GROUP BY s.name, t.name" -o "OriginalDB_counts.csv"

sqlcmd -S "<server>" -d "NewDB" -E -W -s"," -Q "...same query..." -o "NewDB_counts.csv"
```

Adjust server, authentication, and file paths as required.

---

## 3) Quick PowerShell to merge both CSVs and produce a comparison 🔁
Save both CSVs with headers `schema_name,table_name,rows` and run:

```powershell
$orig = Import-Csv 'OriginalDB_counts.csv'
$new  = Import-Csv 'NewDB_counts.csv'

$allKeys = @($orig | ForEach-Object { "$($_.schema_name).$($_.table_name)" }) + @($new | ForEach-Object { "$($_.schema_name).$($_.table_name)" })
$allKeys = $allKeys | Sort-Object -Unique

$rows = foreach ($k in $allKeys) {
  $parts = $k -split '\.';
  $schema = $parts[0]; $table = $parts[1]
  $a = ($orig | Where-Object { $_.schema_name -eq $schema -and $_.table_name -eq $table }).rows -as [long]
  $b = ($new  | Where-Object { $_.schema_name -eq $schema -and $_.table_name -eq $table }).rows -as [long]
  $a = if ($null -eq $a) { 0 } else { $a }
  $b = if ($null -eq $b) { 0 } else { $b }
  [PSCustomObject]@{
    schema = $schema
    table = $table
    rows_in_original = $a
    rows_in_new      = $b
    diff_rows = $b - $a
    pct_change = if ($a -eq 0) { $null } else { [math]::Round((($b - $a)/$a)*100,2) }
  }
}

$rows | Sort-Object @{Expression={($_.rows_in_original + $_.rows_in_new)};Descending=$true} | Export-Csv 'table_counts_comparison.csv' -NoTypeInformation
```

This generates `table_counts_comparison.csv` with the fields: schema, table, rows_in_original, rows_in_new, diff_rows, pct_change.

---

## 4) Markdown comparison template (paste results here) ✍️
Paste or paste-convert the merged CSV into this table format. Example:

| Schema | Table | Rows (Original) | Rows (New) | Diff | % Change |
|---|---:|---:|---:|---:|---:|
| dbo | Users | 1,234 | 1,234 | 0 | 0.00% |
| dbo | TalonCommands | 12,345 | 12,300 | -45 | -0.36% |

---

## 5) If you want, I can do the merge for you ✅
- Upload the two CSV files (`OriginalDB_counts.csv` and `NewDB_counts.csv`) or paste the query outputs here, and I'll merge and return a ready-to-read markdown table showing diffs and percent changes.

---

## 6) Example (sample data)
Here's a small sample you can copy into the template above:

| Schema | Table | Rows (Original) | Rows (New) | Diff | % Change |
|---|---:|---:|---:|---:|---:|
| dbo | Languages | 42 | 42 | 0 | 0.00% |
| dbo | TalonLists | 3,200 | 3,210 | 10 | 0.31% |

---

If you'd like, I can create the final comparison table for you — just upload the two CSVs or paste the output from each database and I'll merge them and add a sorted markdown table.

*File created by GitHub Copilot.*
