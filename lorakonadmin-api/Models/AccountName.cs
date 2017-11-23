using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lorakonadmin_api.Models
{
    public class AccountName
    {
        public AccountName()
        {
            Name = String.Empty;
        }

        public AccountName(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}