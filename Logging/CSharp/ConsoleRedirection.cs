namespace Logging
{
    public static class ConsoleRedirection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Old TextWriter which goes to the actual console output</returns>
        /*public static TextWriter SafelyRedirectConsoleForCmdReturn() {
            TextWriter oldTextWriter = Console.Out;
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
            return oldTextWriter;
        }*/
    }
}
