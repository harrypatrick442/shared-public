namespace ConfigurationCore
{
    public interface IDelaysConfiguration
    {
        public int ProcessorMetricsMinDelayUpdateLatestMilliseconds{ get; }
        public int ProcessorMetricsDelayTotalProcessorTimeMilliseconds { get; }
        public int MaxSubDelayMilliseconds { get; }
    }
}
