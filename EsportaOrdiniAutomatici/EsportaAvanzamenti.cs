using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Zero5.Data.Layer;
using System.Globalization;
using Shared;

namespace Esporta
{
    class EsportaAvanzamenti
    {
        public void Esportazione()
        {
            InternalEsporta();
            //TODO: gestire un esportazione pacchettizzata per non avere troppi record
        }

        private void InternalEsporta()
        {
            Zero5.Util.Log.WriteLog("Formato esportazione: " + (int)Configurazioni.Esportazione_Formato);

            StringBuilder sb = new StringBuilder();
            SortedDictionary<string, RecordEsportazioneVersamentiCustom> lstRecords = new SortedDictionary<string, RecordEsportazioneVersamentiCustom>();

            CalcolaRecordEsportazione_Avanzamenti_DaTransazioni(lstRecords);
            //CalcolaRecordEsportazione_SaldoFase_DaStatoFase(lstRecords);

            ImpostaDatiAggiuntiviRecord(lstRecords);

            foreach (KeyValuePair<string, RecordEsportazioneVersamentiCustom> kvp in lstRecords)
            {
                if (kvp.Value.V01_ERP_RigaSaldata || kvp.Value.V01_ERP_QuantitaPrincipale != 0 || kvp.Value.V01_ERP_QuantitaScartoPrimaScelta != 0 || kvp.Value.V02_ERP_QuantitaScartoSecondaScelta_DaRilavorare != 0 || kvp.Value.V01_ERP_MinutiLavorati != 0)
                    sb.AppendLine(kvp.Value.FormatToCsvString(Configurazioni.Esportazione_Formato));
            }

            string content = sb.ToString();

            if (content.Length > 0)
            {
                if (Configurazioni.ModalitaIntegrazioneEsolver == eTipoScambioDatiEsolver.InCloud)
                {
                    if (!Common.POSTAvanzamentiAVP(content))
                        throw new Exception("Errore esportazione AVP via ws");
                }
                else
                {
                    string fileEsportazioneAvanzamenti = @"\\192.168.1.100\sistemi\ESOLVER\PhaseMES\OdP\" + System.IO.Path.DirectorySeparatorChar + "PHA_AVP_" + DateTime.Now.ToString("yyyyMMddHHmm") + "_" + DateTime.Now.Ticks + ".phasetmp";

                    System.IO.File.AppendAllText(fileEsportazioneAvanzamenti, content);

                    if (System.IO.File.Exists(fileEsportazioneAvanzamenti))
                    {
                        System.IO.File.Move(fileEsportazioneAvanzamenti, fileEsportazioneAvanzamenti.Replace(".phasetmp", ".txt"));
                        Zero5.Util.Log.WriteLog("Rename " + fileEsportazioneAvanzamenti + " in " + fileEsportazioneAvanzamenti.Replace(".phasetmp", ".txt"));
                    }
                }

                foreach (KeyValuePair<string, RecordEsportazioneVersamentiCustom> kvp in lstRecords)
                {
                    try
                    {
                        if (kvp.Value.Esportata)
                        {
                            kvp.Value.MarcaEsportateTransazioniCoinvolte();
                        }
                    }
                    catch (Exception ex)
                    {
                        Zero5.Util.Log.WriteLog("Eccezione salvataggio Esportato = 1 per " + kvp.Value.V01_ERP_Descrizione + " :  " + ex.Message);
                    }
                }
            }
        }

        private void CalcolaRecordEsportazione_Avanzamenti_DaTransazioni(SortedDictionary<string, RecordEsportazioneVersamentiCustom> records)
        {
            try
            {
                Zero5.Server.Produzione srvProd = new Zero5.Server.Produzione();
                List<int> lstCausaliDaEsportareDaConfigurazioniSistema = new List<int>();

                bool consideraAncheVersatiLogisticaEContabilizzati = false;
                try
                {
                    lstCausaliDaEsportareDaConfigurazioniSistema = new List<int>();
                    lstCausaliDaEsportareDaConfigurazioniSistema.AddRange(srvProd.CalcolaCausaliTransazioniDaEsportare());
                    if (lstCausaliDaEsportareDaConfigurazioniSistema.Count == 0)
                        lstCausaliDaEsportareDaConfigurazioniSistema.Add(-1);
                    consideraAncheVersatiLogisticaEContabilizzati = true;
                }
                catch (Exception ex)
                {
                    string versioneServer = Zero5.Data.Layer.Opzioni.helper.LoadStringValue(Opzioni.enumOpzioniID.InfoServer_VersionePhaseServer);
                    Zero5.Util.Log.WriteLog("Versione PhaseServer installata " + versioneServer + ". Aggiornare a >=605 per personalizzare le causali da esportare.");
                }

                Zero5.Data.Layer.CausaliAttivita causali = new CausaliAttivita();
                causali.Load(causali.Fields.IDCausaleAttivita != Common.CausaleVersamentoDaGestionale);

                List<int> lstCausaliPezzi = new List<int>();

                while (!causali.EOF)
                {
                    if (lstCausaliDaEsportareDaConfigurazioniSistema.Count == 0 || lstCausaliDaEsportareDaConfigurazioniSistema.Contains(causali.IDCausaleAttivita))
                    {

                        if (causali.QuantitaSuFase == CausaliAttivita.enumQuantitaSuFase.ContatiLavorazione ||
                            causali.QuantitaSuFase == CausaliAttivita.enumQuantitaSuFase.ContatiAvviamento ||
                            causali.QuantitaSuFase == CausaliAttivita.enumQuantitaSuFase.ContatiSetup ||
                            (consideraAncheVersatiLogisticaEContabilizzati && causali.QuantitaSuFase == CausaliAttivita.enumQuantitaSuFase.Contabilizzati) ||
                           (consideraAncheVersatiLogisticaEContabilizzati && causali.QuantitaSuFase == CausaliAttivita.enumQuantitaSuFase.Logistica))
                        {
                            lstCausaliPezzi.Add(causali.IDCausaleAttivita);
                        }

                    }
                    causali.MoveNext();
                }

                if (lstCausaliPezzi.Count == 0)
                    lstCausaliPezzi.Add(-1);

                Zero5.Util.Log.WriteLog(" -- 1 -- ");
                Zero5.Data.Layer.vOrdiniProduzioneFasiProduzioneTransazioni transazioniDaEsportare = new Zero5.Data.Layer.vOrdiniProduzioneFasiProduzioneTransazioni();
                Zero5.Data.Filter.Filter filtro = new Zero5.Data.Filter.Filter();
                filtro.Add(transazioniDaEsportare.Fields.Transazione_Esportato == 0);
                filtro.Add(transazioniDaEsportare.Fields.Transazione_Fine.FilterNotIsNull());
                filtro.Add(transazioniDaEsportare.Fields.Ordine_IDTipoOrdine == 3);
                if (Configurazioni.Esportazione_MacchineDisabilitateAvanzamenti.Count > 0)
                    filtro.Add(transazioniDaEsportare.Fields.Transazione_IDRisorsaMacchina.FilterNotIn(Configurazioni.Esportazione_MacchineDisabilitateAvanzamenti));
                //if (Configurazioni.Esportazione_DataInizio > DateTime.MinValue)
                //    filtro.Add(transazioniDaEsportare.Fields.Transazione_Inizio >= Configurazioni.Esportazione_DataInizio);               

                filtro.AddOpenBracket();
                filtro.AddOpenBracket();
                filtro.Add(transazioniDaEsportare.Fields.Transazione_PezziBuoni != 0);
                filtro.AddOR();
                filtro.Add(transazioniDaEsportare.Fields.Transazione_PezziScarto != 0);
                filtro.AddCloseBracket();
                filtro.Add(transazioniDaEsportare.Fields.Transazione_Causale.FilterIn(lstCausaliPezzi));
                filtro.AddCloseBracket();
                filtro.Add(transazioniDaEsportare.Fields.Transazione_Inizio >= new DateTime(2022, 11, 21));
                filtro.AddOrderBy(transazioniDaEsportare.Fields.Ordine_IDArticolo);
                filtro.AddOrderBy(transazioniDaEsportare.Fields.Ordine_IDOrdineProduzione);
                filtro.AddOrderBy(transazioniDaEsportare.Fields.Fase_IDFaseProduzione);
                filtro.AddOrderBy(transazioniDaEsportare.Fields.Transazione_Inizio);

                transazioniDaEsportare.Load(filtro);
                Zero5.Util.Log.WriteLog("trovate " + transazioniDaEsportare.RowCount + " transazioni ordini automatici da esportare");

                if (transazioniDaEsportare.RowCount > 10000)
                {
                    Zero5.Util.Log.WriteLog("CalcoloRecordEsportazione_Avanzamenti_DaTransazioni >10000 elementi :" + filtro.ToStringHumanized());
                }

                if (transazioniDaEsportare.EOF || transazioniDaEsportare.RowCount == 0)
                    return;

                while (!transazioniDaEsportare.EOF)
                {
                    try
                    {
                        eTipoOperazione_ERP tipoRecord = eTipoOperazione_ERP.AvanzamentoFase;
                        List<eTipoMagazzinoCoinvolto_ERP> magazziniCoinvolti = new List<eTipoMagazzinoCoinvolto_ERP>();

                        if (transazioniDaEsportare.Transazione_Causale == 238)
                        {
                            magazziniCoinvolti.Add(eTipoMagazzinoCoinvolto_ERP.SC1);
                            if (transazioniDaEsportare.Transazione_PezziBuoni != 0)
                                magazziniCoinvolti.Add(eTipoMagazzinoCoinvolto_ERP.PF);
                        }
                        else if (transazioniDaEsportare.Transazione_Causale == 240)
                        {
                            magazziniCoinvolti.Add(eTipoMagazzinoCoinvolto_ERP.SC2);
                            if (transazioniDaEsportare.Transazione_PezziBuoni != 0)
                                magazziniCoinvolti.Add(eTipoMagazzinoCoinvolto_ERP.PF);
                        }
                        else
                        {
                            magazziniCoinvolti.Add(eTipoMagazzinoCoinvolto_ERP.PF);
                        }
                        foreach (eTipoMagazzinoCoinvolto_ERP tipoMagazzino in magazziniCoinvolti)
                        {


                            string key = CalcolaChiaveRecord(transazioniDaEsportare, tipoMagazzino);

                            if (!records.ContainsKey(key))
                                records.Add(key, new RecordEsportazioneVersamentiCustom(transazioniDaEsportare, tipoRecord, tipoMagazzino));


                            causali.MoveToPrimaryKey(transazioniDaEsportare.Transazione_Causale);

                            if (tipoMagazzino == eTipoMagazzinoCoinvolto_ERP.PF)
                                records[key].V01_ERP_QuantitaPrincipale += transazioniDaEsportare.Transazione_PezziBuoni;
                            else
                            {
                                    records[key].V01_ERP_QuantitaPrincipale += transazioniDaEsportare.Transazione_PezziScarto;
                            }


                            if (!records[key].Phase_lstTransazioniCoinvolte.Contains(transazioniDaEsportare.Transazione_IDTransazione))
                                records[key].Phase_lstTransazioniCoinvolte.Add(transazioniDaEsportare.Transazione_IDTransazione);
                        }
                    }
                    catch (Exception ex)
                    {
                        Zero5.Util.Log.WriteLog("Exc. on CalcolaRecordEsportazione_Avanzamenti_DaTransazioni. Transazione " + transazioniDaEsportare.Transazione_IDTransazione + " " + ex.Message);
                    }
                    transazioniDaEsportare.MoveNext();
                }
            }
            catch (ArgumentException ex)
            {
                Zero5.Util.Log.WriteLog("Exc. on CalcolaRecordEsportazione_Avanzamenti_DaTransazioni. " + ex.Message);
            }
        }



        private string CalcolaChiaveRecord(Zero5.Data.Layer.vOrdiniProduzioneFasiProduzioneTransazioni transazione, eTipoMagazzinoCoinvolto_ERP magazzino)
        {
            return transazione.Transazione_Inizio.ToString("ddMMyyyy") + "_" +
                                  transazione.Fase_IDFaseProduzione + "_" +
                                  magazzino;
        }

        private void ImpostaDatiAggiuntiviRecord(SortedDictionary<string, RecordEsportazioneVersamentiCustom> lstRecord)
        {
            List<int> idArticoliCoinvolti = new List<int>();
            List<int> idRigheDistintaCoinvolte = new List<int>();

            foreach (KeyValuePair<string, RecordEsportazioneVersamentiCustom> kvp in lstRecord)
            {
                if (!idArticoliCoinvolti.Contains(kvp.Value.Phase_IdArticolo))
                    idArticoliCoinvolti.Add(kvp.Value.Phase_IdArticolo);

                if (!idRigheDistintaCoinvolte.Contains(kvp.Value.Phase_idRigaDistinta))
                    idRigheDistintaCoinvolte.Add(kvp.Value.Phase_idRigaDistinta);
            }

            Zero5.Data.Layer.Articoli articoli = new Articoli();
            {
                Zero5.Data.Filter.Filter fil = new Zero5.Data.Filter.Filter();
                fil.Add(articoli.Fields.IDArticolo.FilterIn(idArticoliCoinvolti));
                articoli.Load(fil);
            }

            Zero5.Data.Layer.DistintaBase righeDistinta = new DistintaBase();
            {
                if (idRigheDistintaCoinvolte.Count > 0)
                {
                    Zero5.Data.Filter.Filter fil = new Zero5.Data.Filter.Filter();
                    fil.Add(righeDistinta.Fields.IDDistintaBase.FilterIn(idRigheDistintaCoinvolte));
                    righeDistinta.Load(fil);
                }
            }

            CausaliAttivita causaliTransazioni = new CausaliAttivita();
            causaliTransazioni.LoadAll();

            CausaliMovimento causaliMovimento = new CausaliMovimento();
            causaliMovimento.LoadAll();

            DateTime dtRef = DateTime.MinValue;
            int idFaseProd = 0;

            foreach (KeyValuePair<string, RecordEsportazioneVersamentiCustom> kvp in lstRecord)
            {
                articoli.MoveToPrimaryKey(kvp.Value.Phase_IdArticolo);

                if (articoli.CodiceEsterno != "")
                    kvp.Value.V01_ERP_CodiceArticolo = articoli.CodiceEsterno;
                else
                    kvp.Value.V01_ERP_CodiceArticolo = articoli.CodiceArticolo;

                {
                    string[] tokenCodiceArticolo = articoli.CodiceArticolo.Split('_');
                    if (!Shared.Configurazioni.Esportazione_ForzaEsclusioneVariante
                        && tokenCodiceArticolo.Length > 1
                        )
                        kvp.Value.V01_ERP_CodiceVarianteArticolo = tokenCodiceArticolo[tokenCodiceArticolo.Length - 1];
                }


                if (kvp.Value.V01_ERP_DataRegistrazione.Date != dtRef.Date || kvp.Value.Phase_IdFaseProduzione != idFaseProd)
                {
                    kvp.Value.V01_ERP_TipoRecord_TrueTesta_FalseRiga = eTipoRecord_ERP.Testa;
                    dtRef = kvp.Value.V01_ERP_DataRegistrazione;
                    idFaseProd = kvp.Value.Phase_IdFaseProduzione;
                }
            }
        }
    }
}
