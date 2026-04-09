namespace ConfigurationClassBuilder
{
    public class AlreadyWroteWatcher
    {
        private HashSet<string> _AlreadyWrotes = new HashSet<string>();
        public void ImGoingToWrite(string filePath) {
            string normalized = filePath.ToLowerInvariant();
            if(!_AlreadyWrotes.Add(normalized))
            {
                throw new Exception("The file {filePath} was already written. There's a mistake in your code!");
            }
        }
    }
}
