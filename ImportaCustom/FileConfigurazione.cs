using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FMCarton
{
    class FileConfigurazione : Zero5.Util.FileParametri
    {
        public FileConfigurazione()
            : base(Zero5.IO.Util.LocalPathFile("ImportaCustom.cfg"))
        {
            this.IpServer = this.IpServer;
            this.DatabaseScambio = this.DatabaseScambio;
        }

        public string IpServer
        {
            get
            {
                return GetParametro("IpServer", "127.0.0.1");
            }
            set
            {
                SetParametro("IpServer", value);
            }
        }

        public string DatabaseScambio
        {
            get
            {
                return GetParametro("DatabaseGestionale", "");
            }

            set
            {
                SetParametro("DatabaseGestionale", value);
            }
        }
    }
}
