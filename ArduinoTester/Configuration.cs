using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoTester
{
    public class Configuration
    {
        public short id { get; set; }

        public string naming { get; set; }
        public string description { get; set; }
        public string inofile { get; set; }
        public byte[] scheme { get; set; }
        
       
        public Configuration() { }

        public Configuration (string naming, string description, string iNOFile, byte[] scheme)
        {
            this.naming = naming;
            this.description = description;
            this.inofile = iNOFile;
            this.scheme = scheme;
        }
    }
}
