namespace BasicEnvironment.Abstractions
{
    public interface IClusterOptions
    {
        string ClusterName { get; }
        string ServiceAccountName { get; }
    }
}