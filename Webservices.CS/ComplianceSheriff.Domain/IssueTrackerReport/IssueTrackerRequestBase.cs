using ComplianceSheriff.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class IssueTrackerRequestBase : IValidatableObject
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

        [RegularExpression(@"(\-*\d+)", ErrorMessage = "{0}MustBePositiveOrNegativeNumber")]
        public string ScanId { get; set; }

        [RegularExpression("([0-9]+)", ErrorMessage = "{0}MustBeInteger")]
        public string ScanGroupId { get; set; }

        public string CheckpointId { get; set; }

        [ValidValues("", "Issue", "Pages", "Impact", "SeverityId", "State", "PriorityLevel", "Occurrences", "HighestPageLevel", "Checkpoint")]
        public string SortColumn { get; set; }

        [ValidValues("", "ASC", "DESC")]
        public string SortDirection { get; set; }

        public string[] State { get; set; }

        public string Page { get; set; }

        public string[] PriorityLevel { get; set; }

        public string CheckpointGroupId { get; set; }

        public string[] Severity { get; set; }

        public List<ImpactRange> ImpactRange { get; set; }

        public IssueTrackerRequestBase()
        {
            this.ImpactRange = new List<ImpactRange>();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
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

            if (State != null)
            {
                var validStateValues = new string[3] { "Failed", "Visual", "Warning" };

                for (var i = 0; i < State.Length; i++)
                {
                    if (!validStateValues.Contains(State[i], StringComparer.OrdinalIgnoreCase))
                    {
                        yield return new ValidationResult("invalidValueForState");
                    }
                }
            }

            if (PriorityLevel != null)
            {
                for (var i = 0; i < PriorityLevel.Length; i++)
                {
                    if (!Int32.TryParse(PriorityLevel[i], out int number))
                    {
                        yield return new ValidationResult("PriorityLevelInvalid");
                    }
                    else
                    {
                        if (!Enumerable.Range(1, 3).Contains(number))
                        {
                            yield return new ValidationResult("PriorityLevelRangeBetween1And3");
                        }
                    }
                }
            }

            if (Severity != null)
            {
                for (var i = 0; i < Severity.Length; i++)
                {
                    Regex r = new Regex("^[a-zA-Z]*$");
                    if (!r.IsMatch(Severity[i]))
                    {
                        yield return new ValidationResult("SeverityValueInvalid");
                    }
                }
            }

            if (ImpactRange != null)
            {
                foreach (var range in ImpactRange)
                {
                    if (!Int32.TryParse(range.MinImpact, out int number))
                    {
                        yield return new ValidationResult("MinRangeValueMustBeInteger");
                    }

                    if (!Int32.TryParse(range.MaxImpact, out number))
                    {
                        yield return new ValidationResult("MaxRangeValueMustBeInteger");
                    }
                }
            }
        }
    }
}
