namespace BasicEnvironment.Abstractions
{
    public interface IClusterDataOptions : IClusterOptions
    {
        string DatabaseHost { get; }
        string DatabaseMdfBackupDirectory { get; }
        string DatabaseLdfBackupDirectory { get; }
        bool UseWindowsAuthentication { get; }
        string AdminUserName { get; }
        string AdminPassword { get; }
    }
}
