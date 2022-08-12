using System;

namespace ComplianceSheriff.TextFormatter
{
    public class TextFormatterService : ITextFormatterService
    {
        public static char[] includedDomainsSeparator = { ',', '\n', '|', ' ' };

        public string IncludedDomainsFormatForDb(string includedDomains)
        {
            string[] domainLists = includedDomains.Split(TextFormatterService.includedDomainsSeparator, StringSplitOptions.RemoveEmptyEntries);
            var result = String.Join(Environment.NewLine, domainLists);
            return result;
        }

        public string IncludedDomainsFormatForResponse(string includedDomains)
        {
            string[] domainLists = includedDomains.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var result = String.Join(Environment.NewLine, domainLists).Replace(Environment.NewLine, ", ");
            return result;
        }
    }
}
