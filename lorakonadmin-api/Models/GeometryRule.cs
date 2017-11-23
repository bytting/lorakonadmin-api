using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lorakonadmin_api.Models
{
    public class GeometryRule
    {
        public GeometryRule()
        {
            Id = Guid.Empty;
            Geometry = "";
            Unit = "";
            Minimum = 0.0f;
            Maximum = 0.0f;
        }

        public GeometryRule(Guid id, string geometry, string unit, float minimum, float maximum)
        {
            Id = id;
            Geometry = geometry;
            Unit = unit;
            Minimum = minimum;
            Maximum = maximum;
        }

        public Guid Id { get; set; }
        public string Geometry { get; set; }
        public string Unit { get; set; }
        public float Minimum { get; set; }
        public float Maximum { get; set; }
    }
}