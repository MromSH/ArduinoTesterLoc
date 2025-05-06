using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Security.RightsManagement;

namespace ArduinoTester
{
    internal class AppContext: DbContext
    {
        public DbSet<Configuration> Configurations { get; set; }

        public AppContext(): base("DefaultConnection") { }

    }
}
