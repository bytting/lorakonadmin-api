using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lorakonadmin_api.Models
{
    public class ValidationRule
    {
        public ValidationRule()
        {
            Id = Guid.Empty;
            NuclideName = String.Empty;
            ActivityMin = 0f;
            ActivityMax = 0f;
            ConfidenceMin = 0f;
            CanBeAutoApproved = false;
        }

        public ValidationRule(Guid id, string nuclideName, float activityMin, float activityMax, float confidenceMin, bool canBeAutoApproved)
        {
            Id = id;
            NuclideName = nuclideName;
            ActivityMin = activityMin;
            ActivityMax = activityMax;
            ConfidenceMin = confidenceMin;
            CanBeAutoApproved = canBeAutoApproved;
        }        

        public Guid Id { get; set; }
        public string NuclideName { get; set; }
        public float ActivityMin { get; set; }
        public float ActivityMax { get; set; }
        public float ConfidenceMin { get; set; }
        public bool CanBeAutoApproved { get; set; }
    }
}