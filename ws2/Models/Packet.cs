using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ws2.Models
{
    public class Packet
    {
        public string Function = "";
        public UserCredential UserCreds { get; set; }
        public User User { get; set; }
        public Tweets Tweets { get; set; }
        public Error Error { get; set; }
        public Following Following { get; set; }
        public Followers Followers { get; set; }
        public Tweet Tweet { get; set; }
    }
}
