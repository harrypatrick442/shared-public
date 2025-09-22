using System;
using System.Collections.Generic;
using System.Text;


namespace Core.Arguments
{
    public class ParsedArgs
    {
        private readonly Dictionary<string, string> _KeyValues = new Dictionary<string, string>();
        private readonly HashSet<string> _Flags = new HashSet<string>();

        public ParsedArgs(string[] args)
        {
            Parse(args);
        }

        private void Parse(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                // Handle long flag with '=' (e.g., --key=value)
                if (arg.StartsWith("--") && arg.Contains("="))
                {
                    var parts = arg.Substring(2).Split('=', 2);
                    _KeyValues[parts[0]] = RemoveQuotes(parts[1]);
                }
                // Handle long flag with space-separated value (e.g., --key value)
                else if (arg.StartsWith("--"))
                {
                    string key = arg.Substring(2);
                    if (i + 1 < args.Length && !IsFlag(args[i + 1]))
                    {
                        _KeyValues[key] = RemoveQuotes(args[++i]);
                    }
                    else
                    {
                        _Flags.Add(key); // It's a standalone flag
                    }
                }
                // Handle combined short flags (e.g., -abc)
                else if (arg.StartsWith("-") && !arg.StartsWith("--") && arg.Length > 1)
                {
                    foreach (char c in arg.Substring(1))
                    {
                        _Flags.Add(c.ToString());
                    }
                }
                // Handle invalid or standalone values
                else
                {
                    throw new ArgumentException($"Unexpected argument format: {arg}");
                }
            }
        }

        private static bool IsFlag(string arg)
        {
            return arg.StartsWith("-"); // Detects if an argument is a flag (long or short)
        }

        private static string RemoveQuotes(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Substring(1, value.Length - 2); // Remove surrounding quotes
            }

            // Replace escaped quotes inside the value
            return value.Replace("\\\"", "\"");
        }

        // Public Methods to Access Parsed Data
        public bool HasFlag(string flag) => _Flags.Contains(flag);

        public string? TryGetValue(string key)
        {
            return _KeyValues.TryGetValue(key, out var value) ? value : null;
        }
        public string GetValue(string key)
        {
            if (_KeyValues.TryGetValue(key, out var value)) {
                return value;
            }
            throw new KeyNotFoundException(key);
        }

        public IEnumerable<string> GetFlags() => _Flags;

        public IEnumerable<KeyValuePair<string, string>> GetKeyValues() => _KeyValues;
    }



}
