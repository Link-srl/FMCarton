using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FMCarton
{
    static class Program
    {
        public static FileConfigurazione Parametri = new FileConfigurazione();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //Zero5.Data.Link.TCPDataLink.ServerIP = "127.0.0.1";
            //Zero5.Data.Link.TCPDataLink.ServerIP = "192.168.73.2";


            if (!Zero5.Threading.SingleInstance.ImAloneWithinSystem())
            {
                return;
            }

            try
            {
                Zero5.Util.Log.WriteLog("***********    START    ***********");

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                Importa importatore = new Importa();
                Zero5.Util.Log.WriteLog("Inizio calcolo bancali previsti.");
                sw.Start();
                importatore.CalcolaBancaliPrevisti();
                sw.Stop();
                Zero5.Util.Log.WriteLog("Fine calcolo bancali previsti. Elapsed: " + sw.Elapsed.ToString(@"dd\.hh\:mm\:ss"));
                
                Zero5.Util.Log.WriteLog("***********    END    ***********");
            }
            catch (Exception ex)
            {
                Zero5.Util.Log.WriteLog("Errore Generico: " + ex.Message);
            }
        }
    }
}