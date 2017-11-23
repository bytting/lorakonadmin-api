using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lorakonadmin_api.Models
{
    public class Count
    {
        public Count()
        {
            Value = 0;
        }

        public Count(int count)
        {
            Value = count;
        }

        public int Value { get; set; }
    }
}