using System;
using System.Collections.Generic;
using System.Text;

namespace invex_api.Services
{
    public class CustomIdGeneratorService
    {
        private static readonly Random _random = new();
        private const string Base36Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Base62Chars =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Generate a custom ID from a mask string and the current item count (for sequence).
        /// </summary>
        public string GenerateCustomId(string mask, int currentItemCount)
        {
            if (string.IsNullOrWhiteSpace(mask))
                return Guid.NewGuid().ToString("N")[..8].ToUpper();

            var segments = SplitMask(mask);
            var sb = new StringBuilder();

            foreach (var segment in segments)
            {
                sb.Append(GenerateSegment(segment, currentItemCount));
            }

            return sb.Length > 0 ? sb.ToString() : Guid.NewGuid().ToString("N")[..8].ToUpper();
        }

        // ── Split mask on unescaped '$' ────────────────────────────────────
        private static List<string> SplitMask(string mask)
        {
            var segments = new List<string>();
            var current = new StringBuilder();

            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i] == '\\' && i + 1 < mask.Length)
                {
                    current.Append(mask[i]);
                    current.Append(mask[i + 1]);
                    i++;
                }
                else if (mask[i] == '$')
                {
                    if (current.Length > 0)
                        segments.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(mask[i]);
                }
            }

            if (current.Length > 0)
                segments.Add(current.ToString());

            return segments;
        }

        // ── Unescape a value (remove backslash from \$, \(, \), \\) ───────
        private static string Unescape(string s)
        {
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    sb.Append(s[i + 1]);
                    i++;
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            return sb.ToString();
        }

        // ── Generate one segment ──────────────────────────────────────────
        private string GenerateSegment(string segment, int currentItemCount)
        {
            Console.WriteLine($"Generating segment: {segment}");
            var parenIdx = segment.IndexOf('(');
            if (parenIdx == -1)
                return "";

            var code = segment[..parenIdx];
            var inner = segment[(parenIdx + 1)..^1]; // strip ( and )

            return code switch
            {
                "C" => GenerateCustomText(inner),
                "20B" => Generate20Bit(inner),
                "32B" => Generate32Bit(inner),
                "6D" => Generate6Digit(inner),
                "9D" => Generate9Digit(inner),
                "G" => GenerateGuid(inner),
                "DT" => GenerateDateTime(inner),
                "S" => GenerateSequence(inner, currentItemCount),
                _ => "",
            };
        }

        // ── C(text) — Static custom text ──────────────────────────────────
        private static string GenerateCustomText(string inner) => Unescape(inner);

        // ── 20B(format,,grouping) — 20-bit random ─────────────────────────
        private string Generate20Bit(string inner)
        {
            var parts = inner.Split(',');
            var format = parts.Length > 0 ? parts[0] : "Hex";
            var grouping = parts.Length > 2 && parts[2] == "1";

            int value = _random.Next(0, 0xFFFFF + 1); // 0 to 1048575
            string result = format switch
            {
                "Decimal" => value.ToString(),
                "Base36" => ToBase(value, Base36Chars),
                _ => value.ToString("X5"), // Hex
            };

            if (grouping && result.Length > 3)
                result = result[..3] + "-" + result[3..];

            return result;
        }

        // ── 32B(format,,grouping) — 32-bit random ─────────────────────────
        private string Generate32Bit(string inner)
        {
            var parts = inner.Split(',');
            var format = parts.Length > 0 ? parts[0] : "Hex";
            var grouping = parts.Length > 2 && parts[2] == "1";

            uint value = (uint)_random.Next(int.MinValue, int.MaxValue);
            string result = format switch
            {
                "Decimal" => value.ToString(),
                "Base36" => ToBase(value, Base36Chars),
                "Base62" => ToBase(value, Base62Chars),
                _ => value.ToString("X8"), // Hex
            };

            if (grouping && result.Length > 4)
                result = result[..4] + "-" + result[4..];

            return result;
        }

        // ── 6D(,leadingZeros,grouping) — 6-digit random ───────────────────
        private string Generate6Digit(string inner)
        {
            var parts = inner.Split(',');
            var leadingZeros = parts.Length > 1 && int.TryParse(parts[1], out var lz) ? lz : 0;
            var grouping = parts.Length > 2 && parts[2] == "1";

            int value = _random.Next(0, 999999);
            string result = value.ToString().PadLeft(6, '0');

            // Apply leading zeros: first N digits are forced to '0'
            if (leadingZeros > 0 && leadingZeros < result.Length)
            {
                var remaining = _random.Next(0, (int)Math.Pow(10, result.Length - leadingZeros));
                result =
                    new string('0', leadingZeros)
                    + remaining.ToString().PadLeft(result.Length - leadingZeros, '0');
            }

            if (grouping)
                result = result[..3] + "-" + result[3..];

            return result;
        }

        // ── 9D(,leadingZeros,grouping) — 9-digit random ───────────────────
        private string Generate9Digit(string inner)
        {
            var parts = inner.Split(',');
            var leadingZeros = parts.Length > 1 && int.TryParse(parts[1], out var lz) ? lz : 0;
            var grouping = parts.Length > 2 && parts[2] == "1";

            int value = _random.Next(0, 999999999);
            string result = value.ToString().PadLeft(9, '0');

            if (leadingZeros > 0 && leadingZeros < result.Length)
            {
                var remaining = _random.Next(0, (int)Math.Pow(10, result.Length - leadingZeros));
                result =
                    new string('0', leadingZeros)
                    + remaining.ToString().PadLeft(result.Length - leadingZeros, '0');
            }

            if (grouping)
                result = result[..3] + "-" + result[3..6] + "-" + result[6..];

            return result;
        }

        // ── G(format,,grouping) — GUID ────────────────────────────────────
        private static string GenerateGuid(string inner)
        {
            var parts = inner.Split(',');
            var format = parts.Length > 0 ? parts[0] : "Uppercase";
            var grouping = parts.Length > 2 && parts[2] == "1";

            string result = grouping
                ? Guid.NewGuid().ToString("D") // with dashes
                : Guid.NewGuid().ToString("N"); // no dashes

            return format == "Lowercase" ? result.ToLower() : result.ToUpper();
        }

        // ── DT(pattern) — Date/time ───────────────────────────────────────
        private static string GenerateDateTime(string inner)
        {
            var pattern = Unescape(inner);
            var now = DateTime.UtcNow;

            // Convert our custom tokens to actual values
            var result = pattern
                .Replace("YYYY", now.ToString("yyyy"))
                .Replace("YY", now.ToString("yy"))
                .Replace("MM", now.ToString("MM"))
                .Replace("DD", now.ToString("dd"))
                .Replace("HH", now.ToString("HH"))
                .Replace("SS", now.ToString("ss"));

            return result;
        }

        // ── S(value) — Sequence ───────────────────────────────────────────
        private static string GenerateSequence(string inner, int currentItemCount)
        {
            var template = Unescape(inner);
            int nextValue = currentItemCount + 1;

            // Pad to the same length as the template
            int padLength = Math.Max(template.Length, 1);
            return nextValue.ToString().PadLeft(padLength, '0');
        }

        // ── Base conversion helper ────────────────────────────────────────
        private static string ToBase(long value, string chars)
        {
            if (value == 0)
                return chars[0].ToString();

            var result = new StringBuilder();
            int baseNum = chars.Length;
            long v = Math.Abs(value);
            while (v > 0)
            {
                result.Insert(0, chars[(int)(v % baseNum)]);
                v /= baseNum;
            }

            return result.ToString();
        }
    }
}


