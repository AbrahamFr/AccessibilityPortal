using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class IssueTrackerRequest : IssueTrackerRequestBase
    {
        [Required(ErrorMessage = "{0}IsRequired")]
        [RegularExpression("([0-9]+)", ErrorMessage = "{0}MustBeInteger")]
        public override string CurrentPage
        {
            get { return currentPage; }
            set { currentPage = value; }
        }

        [Required(ErrorMessage = "{0}IsRequired")]
        [RegularExpression("([0-9]+)", ErrorMessage = "{0}MustBeInteger")]
        public override string RecordsToReturn
        {
            get { return recordsToReturn; }
            set { recordsToReturn = value; }
        }
    }
}
