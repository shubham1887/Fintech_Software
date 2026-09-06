import re, sys

src = open('_full_flat.sql', encoding='utf-8-sig').read()

# Drop batch separators / transaction wrappers; we emit our own GO per statement.
lines = [l for l in src.splitlines()
         if l.strip().upper() not in ('GO', 'BEGIN TRANSACTION;', 'COMMIT;')]
content = '\n'.join(lines)

# Split into statements on blank lines (EF puts one blank line between statements;
# multi-line CREATE TABLE / DECLARE blocks contain no internal blank lines).
chunks = [c.strip() for c in re.split(r'\n[ \t]*\n', content) if c.strip()]

out = []
skipped = {'altercolumn': 0, 'drop': 0, 'other_passthrough': 0}
counts = {'table': 0, 'index': 0, 'addcol': 0, 'addconstraint': 0, 'history': 0}

def wrap(guard, body):
    out.append(f"IF {guard}\nBEGIN\n{body}\nEND;\nGO\n")

for c in chunks:
    u = c.upper()

    # History table bootstrap — keep verbatim.
    if c.startswith("IF OBJECT_ID(N'[__EFMigrationsHistory]')"):
        out.append(c + "\nGO\n")
        continue

    # Modifications / deletions — an additive script must not touch existing data.
    if 'ALTER COLUMN' in u or c.startswith('DECLARE @VAR') or c.upper().startswith('DECLARE @VAR'):
        skipped['altercolumn'] += 1
        continue
    if 'DROP COLUMN' in u or re.match(r'ALTER TABLE \[[^\]]+\] DROP', c, re.I):
        skipped['drop'] += 1
        continue

    m = re.match(r'CREATE TABLE \[([^\]]+)\]', c)
    if m:
        t = m.group(1)
        wrap(f"OBJECT_ID(N'[{t}]', N'U') IS NULL", c)
        counts['table'] += 1
        continue

    m = re.match(r'CREATE (?:UNIQUE )?INDEX \[([^\]]+)\] ON \[([^\]]+)\]', c)
    if m:
        ix, t = m.group(1), m.group(2)
        wrap(f"NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'{ix}' AND object_id = OBJECT_ID(N'[{t}]'))", c)
        counts['index'] += 1
        continue

    m = re.match(r'ALTER TABLE \[([^\]]+)\] ADD CONSTRAINT \[([^\]]+)\]', c)
    if m:
        t, con = m.group(1), m.group(2)
        wrap(f"NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = N'{con}' AND parent_object_id = OBJECT_ID(N'[{t}]'))", c)
        counts['addconstraint'] += 1
        continue

    m = re.match(r'ALTER TABLE \[([^\]]+)\] ADD \[([^\]]+)\]', c)
    if m:
        t, col = m.group(1), m.group(2)
        wrap(f"COL_LENGTH(N'{t}', N'{col}') IS NULL", c)
        counts['addcol'] += 1
        continue

    m = re.match(r"INSERT INTO \[__EFMigrationsHistory\].*?VALUES \(N'([^']+)'", c, re.S)
    if m:
        mid = m.group(1)
        wrap(f"NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'{mid}')", c)
        counts['history'] += 1
        continue

    # Anything unrecognised: keep verbatim so nothing silently disappears.
    out.append(c + "\nGO\n")
    skipped['other_passthrough'] += 1

header = """/* ============================================================================
   Fino_API — production schema catch-up (ADD-ONLY, idempotent)
   Safe to run on any state: every object is created ONLY if missing.
   - Adds missing tables, columns, indexes, constraints.
   - Does NOT drop or alter existing columns (no data loss).
   - Re-runnable; independent of __EFMigrationsHistory.
   - Also seeds __EFMigrationsHistory so future EF migrations stay in sync.
   Take a database backup before running.
   ============================================================================ */
"""
open('migrate_prod_addmissing.sql', 'w', encoding='utf-8-sig', newline='\r\n').write(header + "\n" + "\n".join(out))

print("counts:", counts)
print("skipped:", skipped)
print("total chunks:", len(chunks))
