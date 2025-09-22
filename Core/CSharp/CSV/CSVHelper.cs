namespace Core.CSV
{
    public static class CSVHelper
    {
        public static string? EscapeCsvString(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Check if the string contains characters that require escaping
            bool containsSpecialChars = value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r");

            if (containsSpecialChars)
            {
                // Escape double quotes by doubling them
                value = value.Replace("\"", "\"\"");
                // Enclose the entire field in double quotes
                return $"\"{value}\"";
            }

            return value;
        }}
}