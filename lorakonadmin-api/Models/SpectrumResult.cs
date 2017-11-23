using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lorakonadmin_api.Models
{
    public class SpectrumResult
    {
        public SpectrumResult()
        {

        }

        public Guid ID { get; set; }
        public Guid SpectrumInfoID { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string NuclideName { get; set; }
        public float Confidence { get; set; }
        public float Activity { get; set; }
        public float ActivityUncertainty { get; set; }
        public float MDA { get; set; }
        public bool Evaluated { get; set; }
        public bool Approved { get; set; }
        public bool ApprovedIsMDA { get; set; }
        public string ApprovedStatus { get; set; }
        public bool Rejected { get; set; }
        public string Comment { get; set; }
    }
}