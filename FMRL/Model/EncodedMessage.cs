using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FMRL.Model
{
    public class EncodedMessage
    {
        public DateTime Created { get; set; } = DateTime.UtcNow;

        public string IV { get; set; }

        public string Encoded { get; set; }

        public string Digested { get; set; }

        public bool PasswordUsed { get; set; }
    }
}
