using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace VAuto.Core.Services
{
    internal static class SandboxCsvWriter
    {
        private static readonly string[] BaselineHeader =
        {
            "version", "snapshot_id", "player_key", "character_name", "platform_id", "zone_id", "captured_utc",
            "row_type", "component_type", "assembly_qualified_type", "existed", "payload_base64", "payload_hash"
        };

        private static readonly string[] DeltaHeader =
        {
            "version", "snapshot_id", "player_key", "character_name", "platform_id", "zone_id", "captured_utc",
            "row_type", "operation", "component_type", "before_payload_base64", "after_payload_base64",
            "tech_guid", "tech_name", "entity_index", "entity_version", "prefab_guid", "prefab_name",
            "pos_x", "pos_y", "pos_z"
        };

        public static void WriteBaseline(string path, IEnumerable<BaselineRow> rows)
        {
            WriteCsv(path, BaselineHeader, rows, row => new[]
            {
                row.Version.ToString(CultureInfo.InvariantCulture),
                row.SnapshotId,
                row.PlayerKey,
                row.CharacterName,
                row.PlatformId.ToString(CultureInfo.InvariantCulture),
                row.ZoneId,
                row.CapturedUtc.ToString("O", CultureInfo.InvariantCulture),
                row.RowType,
                row.ComponentType,
                row.AssemblyQualifiedType,
                row.Existed ? "true" : "false",
                row.PayloadBase64,
                row.PayloadHash
            });
        }

        public static void WriteDelta(string path, IEnumerable<DeltaRow> rows)
        {
            WriteCsv(path, DeltaHeader, rows, row => new[]
            {
                row.Version.ToString(CultureInfo.InvariantCulture),
                row.SnapshotId,
                row.PlayerKey,
                row.CharacterName,
                row.PlatformId.ToString(CultureInfo.InvariantCulture),
                row.ZoneId,
                row.CapturedUtc.ToString("O", CultureInfo.InvariantCulture),
                row.RowType,
                row.Operation,
                row.ComponentType,
                row.BeforePayloadBase64,
                row.AfterPayloadBase64,
                row.TechGuid.ToString(CultureInfo.InvariantCulture),
                row.TechName,
                row.EntityIndex.ToString(CultureInfo.InvariantCulture),
                row.EntityVersion.ToString(CultureInfo.InvariantCulture),
                row.PrefabGuid.ToString(CultureInfo.InvariantCulture),
                row.PrefabName,
                row.PosX.ToString("R", CultureInfo.InvariantCulture),
                row.PosY.ToString("R", CultureInfo.InvariantCulture),
                row.PosZ.ToString("R", CultureInfo.InvariantCulture)
            });
        }

        public static List<BaselineRow> ReadBaseline(string path)
        {
            var lines = ReadCsv(path);
            var rows = new List<BaselineRow>();

            for (var i = 1; i < lines.Count; i++)
            {
                var cols = ParseCsvLine(lines[i]);
                if (cols.Count < BaselineHeader.Length)
                {
                    continue;
                }

                rows.Add(new BaselineRow
                {
                    Version = ParseInt(cols[0]),
                    SnapshotId = cols[1],
                    PlayerKey = cols[2],
                    CharacterName = cols[3],
                    PlatformId = ParseUlong(cols[4]),
                    ZoneId = cols[5],
                    CapturedUtc = ParseDate(cols[6]),
                    RowType = cols[7],
                    ComponentType = cols[8],
                    AssemblyQualifiedType = cols[9],
                    Existed = ParseBool(cols[10]),
                    PayloadBase64 = cols[11],
                    PayloadHash = cols[12]
                });
            }

            return rows;
        }

        public static List<DeltaRow> ReadDelta(string path)
        {
            var lines = ReadCsv(path);
            var rows = new List<DeltaRow>();

            for (var i = 1; i < lines.Count; i++)
            {
                var cols = ParseCsvLine(lines[i]);
                if (cols.Count < DeltaHeader.Length)
                {
                    continue;
                }

                rows.Add(new DeltaRow
                {
                    Version = ParseInt(cols[0]),
                    SnapshotId = cols[1],
                    PlayerKey = cols[2],
                    CharacterName = cols[3],
                    PlatformId = ParseUlong(cols[4]),
                    ZoneId = cols[5],
                    CapturedUtc = ParseDate(cols[6]),
                    RowType = cols[7],
                    Operation = cols[8],
                    ComponentType = cols[9],
                    BeforePayloadBase64 = cols[10],
                    AfterPayloadBase64 = cols[11],
                    TechGuid = ParseLong(cols[12]),
                    TechName = cols[13],
                    EntityIndex = ParseInt(cols[14]),
                    EntityVersion = ParseInt(cols[15]),
                    PrefabGuid = ParseLong(cols[16]),
                    PrefabName = cols[17],
                    PosX = ParseFloat(cols[18]),
                    PosY = ParseFloat(cols[19]),
                    PosZ = ParseFloat(cols[20])
                });
            }

            return rows;
        }

        private static void WriteCsv<T>(string path, IReadOnlyList<string> header, IEnumerable<T> rows, Func<T, string[]> selector)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Allow concurrent readers (log viewers / diagnostics) while writing snapshots.
            // FileShare.None can fail on Windows if another process already opened the file for read.
            using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var gzip = new GZipStream(file, CompressionLevel.Optimal);
            using var writer = new StreamWriter(gzip, Encoding.UTF8);

            writer.WriteLine(string.Join(",", header));
            foreach (var row in rows)
            {
                var fields = selector(row);
                writer.WriteLine(JoinCsvFields(fields));
            }
        }

        private static List<string> ReadCsv(string path)
        {
            var result = new List<string>();
            if (!File.Exists(path))
            {
                return result;
            }

            using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var gzip = new GZipStream(file, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip, Encoding.UTF8);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line != null)
                {
                    result.Add(line);
                }
            }

            return result;
        }

        private static string JoinCsvFields(IReadOnlyList<string> fields)
        {
            var escaped = new string[fields.Count];
            for (var i = 0; i < fields.Count; i++)
            {
                escaped[i] = EscapeCsvField(fields[i] ?? string.Empty);
            }

            return string.Join(",", escaped);
        }

        private static string EscapeCsvField(string value)
        {
            var requiresQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            if (!requiresQuotes)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            var builder = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (inQuotes)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            builder.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        builder.Append(ch);
                    }
                }
                else
                {
                    if (ch == ',')
                    {
                        values.Add(builder.ToString());
                        builder.Clear();
                    }
                    else if (ch == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        builder.Append(ch);
                    }
                }
            }

            values.Add(builder.ToString());
            return values;
        }

        private static int ParseInt(string value)
            => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;

        private static ulong ParseUlong(string value)
            => ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0UL;

        private static long ParseLong(string value)
            => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0L;

        private static float ParseFloat(string value)
            => float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0f;

        private static bool ParseBool(string value)
            => bool.TryParse(value, out var parsed) && parsed;

        private static DateTime ParseDate(string value)
            => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed) ? parsed : DateTime.UtcNow;
    }
}
