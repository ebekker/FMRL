using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FMRL.Model
{
    public class EncodedMessage
    {
        public long Created { get; set; } = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

        public string IV { get; set; }

        public string Encoded { get; set; }

        public string Digested { get; set; }

        public bool PasswordUsed { get; set; }
    }
}
