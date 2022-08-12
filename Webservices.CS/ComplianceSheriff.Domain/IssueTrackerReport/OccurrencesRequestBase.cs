using ComplianceSheriff.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class OccurrencesRequestBase : IValidatableObject
    {
        protected string currentPage;
        protected string recordsToReturn;

        public virtual string CurrentPage
        {
            get { return currentPage; }
            set { currentPage = value; }
        }

        public virtual string RecordsToReturn
        {
            get { return recordsToReturn; }
            set { recordsToReturn = value; }
        }

        [JsonIgnore]
        public string ReferrerUrl { get; set; }

        //[Required(ErrorMessage = "{0}IsRequired.")]
        //[RegularExpression("([0-9]+)", ErrorMessage = "{0}MustBeInteger")]
        //public string CurrentPage { get; set; }

        //[Required(ErrorMessage = "{0}IsRequired.")]
        //[RegularExpression("([0-9]+)", ErrorMessage = "{0}MustBeInteger")]
        //public string RecordsToReturn { get; set; }

        [Required(ErrorMessage = "{0}IsRequired.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "{0}MustBeInteger")]
        public virtual string IssueId { get; set; }

        [Required(ErrorMessage = "{0}IsRequired.")]
        public virtual string CheckpointId { get; set; }

        public string CheckpointGroupId { get; set; }

        [RegularExpression(@"(\-*\d+)", ErrorMessage = "{0}MustBePositiveOrNegativeNumber")]
        public string ScanId { get; set; }

        [RegularExpression("([0-9]+)", ErrorMessage = "{0}MustBeInteger")]
        public string ScanGroupId { get; set; }

        [ValidValues("", "Failed", "Visual", "Warning")]
        public string State { get; set; }

        public string PageTitle { get; set; }
        public string PageUrl { get; set; }
        public string KeyAttribute { get; set; }
        public string Element { get; set; }
        public string ContainerId { get; set; }
        public string ScanName { get; set; }

        [ValidValues("", "ContainerId", "Element", "KeyAttrName", "PageTitle", "ScanDisplayName", "ScanId", "Url")]
        public virtual string SortColumn { get; set; }

        [ValidValues("", "ASC", "DESC")]
        public string SortDirection { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!String.IsNullOrWhiteSpace(currentPage))
            {
                bool result = Int32.TryParse(currentPage, out int number);

                if (number == 0 || !result || !Enumerable.Range(1, Int32.MaxValue).Contains(number))
                {
                    yield return new ValidationResult("CurrentPageRangeBetween1AndInt32Max");
                }
            }

            if (!String.IsNullOrWhiteSpace(recordsToReturn))
            {
                bool result = Int32.TryParse(recordsToReturn, out int number);

                if (number == 0 || !result || !Enumerable.Range(1, Int32.MaxValue).Contains(number))
                {
                    yield return new ValidationResult("RecordsToReturnRangeBetween1AndInt32Max");
                }
            }

            if (String.IsNullOrWhiteSpace(ScanId) && String.IsNullOrWhiteSpace(ScanGroupId))
            {
                yield return new ValidationResult("scanIdOrScanGroupIdRequired");
            }

            if (!String.IsNullOrWhiteSpace(ScanId) && !String.IsNullOrWhiteSpace(ScanGroupId))
            {
                yield return new ValidationResult("onlyScanIdOrScanGroupIdRequired");
            }

            if (!String.IsNullOrWhiteSpace(ScanGroupId) && ScanGroupId != null)
            {
                bool result = Int32.TryParse(ScanGroupId, out int number);

                if (!result || !Enumerable.Range(0, Int32.MaxValue).Contains(number))
                {
                    yield return new ValidationResult("ScanGroupIdRangeBetween1AndInt32Max");
                }
            }

            if (!String.IsNullOrWhiteSpace(IssueId) && IssueId != null)
            {
                bool result = Int32.TryParse(IssueId, out int number);

                if (!result || !Enumerable.Range(0, Int32.MaxValue).Contains(number))
                {
                    yield return new ValidationResult("IssueIdRangeBetween1AndInt32Max");
                }
            }
        }
    }
}
