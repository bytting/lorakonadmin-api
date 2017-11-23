using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lorakonadmin_api.Models
{
    public class SpectrumInfo
    {
        public SpectrumInfo()
        {

        }

        public Guid ID { get; set; }
        public Guid AccountID { get; set; }
        public string AccountName { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime AcquisitionDate { get; set; }
        public DateTime ReferenceDate { get; set; }
        public string Filename { get; set; }
        public string BackgroundFile { get; set; }
        public string LibraryFile { get; set; }
        public float Sigma { get; set; }
        public string SampleType { get; set; }
        public string SampleComponent { get; set; }
        public int Livetime { get; set; }
        public string Laboratory { get; set; }
        public string Operator { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float Altitude { get; set; }
        public string LocationType { get; set; }
        public string Location { get; set; }
        public string Community { get; set; }
        public float SampleWeight { get; set; }
        public string SampleWeightUnit { get; set; }
        public string SampleGeometry { get; set; }
        public string ExternalID { get; set; }
        public bool Approved { get; set; }
        public string ApprovedStatus { get; set; }
        public bool Rejected { get; set; }
        public string Comment { get; set; }
    }
}