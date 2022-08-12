namespace ComplianceSheriff.TextFormatter
{
    public interface ITextFormatterService
    {
        string IncludedDomainsFormatForDb(string includedDomains);

        string IncludedDomainsFormatForResponse(string includedDomains);
    }
}
