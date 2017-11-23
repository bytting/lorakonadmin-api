using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lorakonadmin_api.Models
{
    public class SpectrumInfoBasic
    {
        public SpectrumInfoBasic()
        {

        }

        public Guid ID { get; set; }
        public string AccountName { get; set; }
        public string Operator { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime AcquisitionDate { get; set; }
        public DateTime ReferenceDate { get; set; }
        public string SampleType { get; set; }
        public string SampleComponent { get; set; }
        public bool Approved { get; set; }
        public string ApprovedStatus { get; set; }
        public bool Rejected { get; set; }
    }    
}