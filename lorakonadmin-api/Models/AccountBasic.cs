using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lorakonadmin_api.Models
{
    public class AccountBasic
    {
        public AccountBasic()
        {
            ID = Guid.Empty;
            Username = String.Empty;
        }

        public AccountBasic(Guid id, string username)
        {
            ID = id;
            Username = username;
        }

        public Guid ID { get; set; }
        public string Username { get; set; }
    }
}