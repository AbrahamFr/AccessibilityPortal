namespace BasicEnvironment.Abstractions
{
    public interface IClusterDataDirectoryOptions : IClusterOptions
    {
        string SharedDataDirectory { get; }
    }
}
