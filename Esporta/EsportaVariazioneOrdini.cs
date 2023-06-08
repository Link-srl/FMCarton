using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Zero5.Data.Layer;

namespace Esporta
{
    class EsportaVariazioneOrdini
    {
        static string nomeFileErrori = "ERRORE_ESPORTAZIONE_VARIAZIONE_Odp";

        public void Esportazione()
        {
            DateTime UltimoCambioStato = Configurazioni.Comune_Esportazione_VariazioniOrdiniFasi_DataUltimoCambioStatoEsportato;
            Configurazioni.Comune_Esportazione_VariazioniOrdiniFasi_DataUltimoCambioStatoEsportato = DateTime.Now;
            if (Configurazioni.Comune_Importazione_ProgrammazioneDisabilitaRicalcoloInizioPrevisto)
                return;
            Zero5.Data.Layer.FasiProduzione fasiProduzione = new Zero5.Data.Layer.FasiProduzione();
            Zero5.Data.Filter.Filter filtro = new Zero5.Data.Filter.Filter();
            filtro.AddOpenBracket();
            filtro.Add(fasiProduzione.Fields.Programmata == true);
            filtro.AddOR();
            filtro.Add(fasiProduzione.Fields.UltimoCambioStato >= UltimoCambioStato.AddMonths(-1));
            filtro.AddCloseBracket();
            filtro.Add(fasiProduzione.Fields.Stato != Zero5.Data.Layer.FasiProduzione.enumFasiProduzioneStati.Finita);
            filtro.Add(fasiProduzione.Fields.CodiceEsterno != "");

            if (Configurazioni.Esportazione_MacchineDisabilitateVariazioniOrdini.Count > 0)
                filtro.Add(fasiProduzione.Fields.IDRisorsaMacchinaProgrammata.FilterNotIn(Configurazioni.Esportazione_MacchineDisabilitateVariazioniOrdini));

            if (Configurazioni.WsSistemi_Esportazione_Test_UtilizzaFaseSingola)
                filtro.Add(fasiProduzione.Fields.CodiceEsterno == Configurazioni.WsSistemi_Esportazione_Test_CodiceEsternoFaseSingola);

            filtro.AddOrderBy(fasiProduzione.Fields.IDOrdineProduzione);
            filtro.AddOrderBy(fasiProduzione.Fields.IDFaseProduzione);
            filtro.AddOrderBy(fasiProduzione.Fields.Stato);
            filtro.AddOrderBy(fasiProduzione.Fields.InizioPrevisto);


            fasiProduzione.Load(filtro);

            if (fasiProduzione.EOF || fasiProduzione.RowCount == 0)
                return;

            Zero5.Data.Layer.Risorse risorse = new Risorse();
            risorse.LoadAll();

            Zero5.Util.Log.WriteLog("Trovate " + fasiProduzione.RowCount + " fasi da verificare");

            int nrFasiEsportate = 0;
            while (!fasiProduzione.EOF)
            {
                try
                {
                    DateTime dtInizioPrevEsportato = DateTime.MinValue;
                    DateTime dtFinePrevEsportato = DateTime.MinValue;

                    if (fasiProduzione.RiferimentoNumerico1 > 0)
                    {
                        try
                        {
                            dtInizioPrevEsportato = new DateTime(((int)fasiProduzione.RiferimentoNumerico1) / 1000, 1, 1);
                            dtInizioPrevEsportato.AddDays((((int)fasiProduzione.RiferimentoNumerico1) % 1000) - 1);
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    if (fasiProduzione.RiferimentoNumerico2 > 0)
                    {
                        try
                        {
                            dtFinePrevEsportato = new DateTime(((int)fasiProduzione.RiferimentoNumerico2) / 1000, 1, 1);
                            dtFinePrevEsportato.AddDays((((int)fasiProduzione.RiferimentoNumerico2) % 1000) - 1);
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    risorse.MoveToPrimaryKey(fasiProduzione.IDRisorsaMacchinaProgrammata);

                    eStatoRiga_eSOLVER statoESolver = Common.PhaseStatoFaseProduzione_eSolverStatoRiga(fasiProduzione);

                    if (dtInizioPrevEsportato != fasiProduzione.InizioPrevisto.Date
                    || dtFinePrevEsportato.Date != fasiProduzione.FinePrevista.Date
                    || fasiProduzione.RiferimentoNumerico3 != (double)statoESolver + 100
                    || fasiProduzione.RiferimentoNumerico4 != risorse.IDRisorsa)
                    {
                        nrFasiEsportate++;

                        bool esportato = Common.POSTVariazioneOrdineEsolver(Configurazioni.WsSistemi_Server, Configurazioni.WsSistemi_User, Configurazioni.WsSistemi_KeySha256, fasiProduzione.CodiceEsterno, fasiProduzione.InizioPrevisto.Date, fasiProduzione.FinePrevista.Date, statoESolver, risorse.CodiceEsterno);
                        if (esportato)
                        {
                            try
                            {
                                fasiProduzione.RiferimentoNumerico1 = fasiProduzione.InizioPrevisto.Year * 1000 + fasiProduzione.InizioPrevisto.DayOfYear;
                                fasiProduzione.RiferimentoNumerico2 = fasiProduzione.FinePrevista.Year * 1000 + fasiProduzione.FinePrevista.DayOfYear;

                                fasiProduzione.RiferimentoNumerico3 = (double)statoESolver + 100;
                                fasiProduzione.RiferimentoNumerico4 = risorse.IDRisorsa;
                                fasiProduzione.Save();
                            }
                            catch (Exception ex)
                            {
                                Zero5.Util.Log.WriteLog("ERRORE_ESPORTAZIONE FaseProduzione IDFAseProduzione: '" + fasiProduzione.IDFaseProduzione + "' messaggio: " + ex.Message);
                                Zero5.Util.Log.WriteLog(nomeFileErrori, "Eccezione esportazione : '" + fasiProduzione.IDFaseProduzione + "' messaggio: " + ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Zero5.Util.Log.WriteLog("IDFaseProduzione:'" + fasiProduzione.IDFaseProduzione + "'  Eccezione messaggio: " + ex.Message);
                    Zero5.Util.Log.WriteLog(nomeFileErrori, "IDFaseProduzione:'" + fasiProduzione.IDFaseProduzione + "'  Eccezione messaggio: " + ex.Message);
                }
                finally
                {
                }
                fasiProduzione.MoveNext();
            }
            Zero5.Util.Log.WriteLog("Esportate " + nrFasiEsportate + " variazioni");

        }
    }
}
