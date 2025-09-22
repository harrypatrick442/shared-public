namespace Core.MemoryMappedFiles
{
    public struct DynamicSizeMetadata
    {
        public int CurrentContentLength { get; }
        public DynamicSizeMetadata(int currentContentLength) {
            CurrentContentLength = currentContentLength;
        }
    }
}
