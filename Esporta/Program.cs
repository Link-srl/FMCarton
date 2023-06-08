using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace Esporta
{
    static class Program
    {


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Zero5.Data.Link.TCPDataLink.ServerIP = "127.0.0.1";
            try
            {
                if (!Zero5.Threading.SingleInstance.ImAloneWithinSystem())
                {
                    Zero5.Util.Log.WriteLog("...double istance, closing.");
                    return;
                }

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //Gestione configurazioni obsolete su files
                FileConfigurazione Parametri = new FileConfigurazione();
                Parametri.MigraParametriDaFileAOpzioni();


                EsportaAvanzamenti esportatore = new EsportaAvanzamenti();
                EsportaVariazioneOrdini esportatoreVariazioniOdP = new EsportaVariazioneOrdini();

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                Zero5.Util.Log.WriteLog("***********    START ESPORTA AvP ["+Application.ProductName+" v" +Application.ProductVersion+"]   ***********");
                esportatore.Esportazione();

                Zero5.Util.Log.WriteLog("Fine esportazione AvP. Elapsed: " + sw.Elapsed.ToString(@"dd\.hh\:mm\:ss"));
                Zero5.Util.Log.WriteLog("***********    END ESPORTA AvP [" + Application.ProductName + " v" + Application.ProductVersion + "]   ***********");

                try
                {
                    Zero5.Util.Log.WriteLog("***********    START ESPORTA VARIAZIONI OdP [" + Application.ProductName + " v" + Application.ProductVersion + "]   ***********");
                    esportatoreVariazioniOdP.Esportazione();
                }
                catch (Exception ex)
                {
                    Zero5.Util.Log.WriteLog("ERRORE ex: " + ex.Message);
                }
                finally
                {
                    Zero5.Util.Log.WriteLog("Fine esportazione variazione OdP. Elapsed: " + sw.Elapsed.ToString(@"dd\.hh\:mm\:ss"));
                    Zero5.Util.Log.WriteLog("***********    END ESPORTA VARIAZIONI AvP [" + Application.ProductName + " v" + Application.ProductVersion + "]   ***********");
                }

                Zero5.Data.Layer.AllarmiSistema allarmeSistema = new Zero5.Data.Layer.AllarmiSistema();
                allarmeSistema.AddNewAndNewID();
                allarmeSistema.DataOraAllarme = DateTime.Now;
                allarmeSistema.CodiceAllarme = "EsportazioneCompletata";
                allarmeSistema.NoteAllarme = "Esportazione Completata. " + DateTime.Now.ToString();
                allarmeSistema.StatoAllarme = Zero5.Data.Layer.AllarmiSistema.eAllarmiSistemaStato.Inserito;
                allarmeSistema.TipoAllarmeSistema = Zero5.Data.Layer.AllarmiSistema.eAllarmiSistemaTipo.Info;
                allarmeSistema.Save();


            }
            catch (Exception ex)
            {
                Zero5.Util.Log.WriteLog("Esporta", "Errore: " + ex.Message);

                Zero5.Data.Layer.AllarmiSistema allarmeSistema = new Zero5.Data.Layer.AllarmiSistema();
                allarmeSistema.AddNewAndNewID();
                allarmeSistema.DataOraAllarme = DateTime.Now;
                allarmeSistema.CodiceAllarme = "ErroreEsportazione";
                allarmeSistema.NoteAllarme = "Errore esportazione, operazione interrotta. " + DateTime.Now.ToString();
                allarmeSistema.StatoAllarme = Zero5.Data.Layer.AllarmiSistema.eAllarmiSistemaStato.Inserito;
                allarmeSistema.TipoAllarmeSistema = Zero5.Data.Layer.AllarmiSistema.eAllarmiSistemaTipo.Info;
                allarmeSistema.Save();
            }
        }

    }
}
