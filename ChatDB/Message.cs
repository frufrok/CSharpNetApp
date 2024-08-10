using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstDB
{
    public class Message
    {
        public int? ID { get; set; }
        public string? Text { get; set; }
        public DateTime? DateTimeSend { get; set; }
        public bool? IsSent { get; set; }
        public int? UserToID { get; set; }
        public int? UserFromID { get; set; }
        public virtual User? UserTo { get; set; }
        public virtual User? UserFrom { get; set; }
    }
}
