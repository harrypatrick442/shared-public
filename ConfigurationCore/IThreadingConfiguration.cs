namespace ConfigurationCore
{
    public interface IThreadingConfiguration
    {
        public int MaxNThreadsQuadTreeGetIds { get; }
        public int MaxNThreadsQuadTreeDelete { get; }
        public int MaxNThreadsQuadTreeSets { get; }
    }
}
