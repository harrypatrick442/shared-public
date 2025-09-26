namespace VirtualSockets
{
    public static class VirtualSocketIdentifierSource
    {

        private static long _NextId=1;
        public static long NextId()
        {
            return _NextId++;
        }
    }
}