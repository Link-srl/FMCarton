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
            SortedDictionary<string, RecordEsportazioneAvanzamentiERP> lstRecords = new SortedDictionary<string, RecordEsportazioneAvanzamentiERP>();

            CalcolaRecordEsportazione_Avanzamenti_DaTransazioni(lstRecords);
            CalcolaRecordEsportazione_AddebitiResi_DaMovimenti(lstRecords);
            CalcolaRecordEsportazione_Consumi_DaMovimenti(lstRecords);
            CalcolaRecordEsportazione_SaldoFase_DaStatoFase(lstRecords);

            ImpostaDatiAggiuntiviRecord(lstRecords);

            foreach (KeyValuePair<string, RecordEsportazioneAvanzamentiERP> kvp in lstRecords)
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
                    string fileEsportazioneAvanzamenti = Configurazioni.OnPremise_Esportazione_PathFileAvpCsv + System.IO.Path.DirectorySeparatorChar + "PHA_AVP_" + DateTime.Now.ToString("yyyyMMddHHmm") + "_" + DateTime.Now.Ticks + ".phasetmp";

                    System.IO.File.AppendAllText(fileEsportazioneAvanzamenti, content);

                    if (System.IO.File.Exists(fileEsportazioneAvanzamenti))
                    {
                        System.IO.File.Move(fileEsportazioneAvanzamenti, fileEsportazioneAvanzamenti.Replace(".phasetmp", ".txt"));
                        Zero5.Util.Log.WriteLog("Rename " + fileEsportazioneAvanzamenti + " in " + fileEsportazioneAvanzamenti.Replace(".phasetmp", ".txt"));
                    }
                }

                foreach (KeyValuePair<string, RecordEsportazioneAvanzamentiERP> kvp in lstRecords)
                {
                    try
                    {
                        if (kvp.Value.Esportata)
                        {
                            if (kvp.Value.V01_ERP_TipoOperazioneAvanzamento == eTipoOperazione_ERP.AvanzamentoFase ||
                                kvp.Value.V01_ERP_TipoOperazioneAvanzamento == eTipoOperazione_ERP.ConsuntivazioneOre ||
                                kvp.Value.V01_ERP_TipoOperazioneAvanzamento == eTipoOperazione_ERP.FlagSaldoFase)
                                kvp.Value.MarcaEsportateTransazioniCoinvolte();

                            if (kvp.Value.V01_ERP_TipoOperazioneAvanzamento == eTipoOperazione_ERP.Addebito ||
                                kvp.Value.V01_ERP_TipoOperazioneAvanzamento == eTipoOperazione_ERP.Reso)
                                kvp.Value.MarcaEsportatiMovimentiCoinvolti(eLivelloEsportatoMovimenti.EsportatoAddebitoReso);

                            if (kvp.Value.V01_ERP_TipoOperazioneAvanzamento == eTipoOperazione_ERP.ConsumoMateriali)
                                kvp.Value.MarcaEsportatiMovimentiCoinvolti(eLivelloEsportatoMovimenti.EsportatoConsumi);
                        }
                    }
                    catch (Exception ex)
                    {
                        Zero5.Util.Log.WriteLog("Eccezione salvataggio Esportato = 1 per " + kvp.Value.V01_ERP_Descrizione + " :  " + ex.Message);
                    }
                }
            }
        }

        private void CalcolaRecordEsportazione_Avanzamenti_DaTransazioni(SortedDictionary<string, RecordEsportazioneAvanzamentiERP> records)
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
                List<int> lstCausaliTempo = new List<int>();

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

                        if (causali.TempiMacchinaSuFase == CausaliAttivita.enumTempiMacchinaSuFase.Avviamento ||
                            causali.TempiMacchinaSuFase == CausaliAttivita.enumTempiMacchinaSuFase.CicloFermoOperativo ||
                            causali.TempiMacchinaSuFase == CausaliAttivita.enumTempiMacchinaSuFase.CicloLavorazione ||
                            causali.TempiMacchinaSuFase == CausaliAttivita.enumTempiMacchinaSuFase.FermoNonOperativo ||
                            causali.TempiUomoSuFase == CausaliAttivita.enumTempiUomoSuFase.Avviamento ||
                            causali.TempiUomoSuFase == CausaliAttivita.enumTempiUomoSuFase.CicloLavorazione ||
                            causali.TempiUomoSuFase == CausaliAttivita.enumTempiUomoSuFase.Setup)
                        {
                            lstCausaliTempo.Add(causali.IDCausaleAttivita);
                        }
                    }
                    causali.MoveNext();
                }

                if (lstCausaliPezzi.Count == 0)
                    lstCausaliPezzi.Add(-1);

                if (lstCausaliTempo.Count == 0)
                    lstCausaliTempo.Add(-1);

                Zero5.Data.Layer.vOrdiniProduzioneFasiProduzioneTransazioni transazioniDaEsportare = new Zero5.Data.Layer.vOrdiniProduzioneFasiProduzioneTransazioni();
                Zero5.Data.Filter.Filter filtro = new Zero5.Data.Filter.Filter();
                filtro.Add(transazioniDaEsportare.Fields.Transazione_Esportato == 0);
                filtro.Add(transazioniDaEsportare.Fields.Transazione_Fine.FilterNotIsNull());
                filtro.Add(transazioniDaEsportare.Fields.Ordine_IDTipoOrdine.FilterIn(Common.ListaTipiOrdineGenerico));
                filtro.Add(transazioniDaEsportare.Fields.Fase_CodiceEsterno != "");//idcompletofaseODP
                if (Configurazioni.Esportazione_MacchineDisabilitateAvanzamenti.Count > 0)
                    filtro.Add(transazioniDaEsportare.Fields.Transazione_IDRisorsaMacchina.FilterNotIn(Configurazioni.Esportazione_MacchineDisabilitateAvanzamenti));
                if (Configurazioni.Esportazione_DataInizio > DateTime.MinValue)
                    filtro.Add(transazioniDaEsportare.Fields.Transazione_Inizio >= Configurazioni.Esportazione_DataInizio);

                filtro.AddOpenBracket();
                filtro.AddOpenBracket();
                filtro.AddOpenBracket();
                filtro.Add(transazioniDaEsportare.Fields.Transazione_PezziBuoni != 0);
                filtro.AddOR();
                filtro.Add(transazioniDaEsportare.Fields.Transazione_PezziScarto != 0);
                filtro.AddCloseBracket();
                filtro.Add(transazioniDaEsportare.Fields.Transazione_Causale.FilterIn(lstCausaliPezzi));
                filtro.AddCloseBracket();
                filtro.AddOR();
                filtro.AddOpenBracket();
                filtro.Add(transazioniDaEsportare.Fields.Transazione_Minuti > 0);
                filtro.Add(transazioniDaEsportare.Fields.Transazione_Elaborato > 0);
                filtro.Add(transazioniDaEsportare.Fields.Transazione_Causale.FilterIn(lstCausaliTempo));
                filtro.Add(transazioniDaEsportare.Fields.Transazione_Fine <= DateTime.Now.Date.AddMinutes(-15));//NECESSARIO PER CALCOLO SOVRAPPOSIZIONI
                //TODO: Approfondire il filtro sopra
                filtro.Add(transazioniDaEsportare.Fields.Transazione_Fine >= new DateTime(2010, 1, 1));
                filtro.AddCloseBracket();
                filtro.AddCloseBracket();

                filtro.AddOrderBy(transazioniDaEsportare.Fields.Ordine_IDArticolo);
                filtro.AddOrderBy(transazioniDaEsportare.Fields.Ordine_IDOrdineProduzione);
                filtro.AddOrderBy(transazioniDaEsportare.Fields.Fase_IDFaseProduzione);
                filtro.AddOrderBy(transazioniDaEsportare.Fields.Transazione_Inizio);

                transazioniDaEsportare.Load(filtro);
                Zero5.Util.Log.WriteLog("trovate " + transazioniDaEsportare.RowCount + " transazioni da esportare");

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
                        eTipoOperazione_ERP tipoRecord = eTipoOperazione_ERP.Sconosciuto;
                        if (transazioniDaEsportare.Transazione_Minuti != 0)
                            tipoRecord = eTipoOperazione_ERP.ConsuntivazioneOre;
                        else
                            tipoRecord = eTipoOperazione_ERP.AvanzamentoFase;

                        string key = CalcolaChiaveRecord(transazioniDaEsportare, tipoRecord);

                        if (!records.ContainsKey(key))
                            records.Add(key, new RecordEsportazioneAvanzamentiERP(transazioniDaEsportare, tipoRecord));

                        switch (records[key].V01_ERP_TipoOperazioneAvanzamento)
                        {
                            case eTipoOperazione_ERP.AvanzamentoFase:
                                causali.MoveToPrimaryKey(transazioniDaEsportare.Transazione_Causale);
                                records[key].V01_ERP_QuantitaPrincipale += transazioniDaEsportare.Transazione_PezziBuoni;

                                if (causali.UsaPerRilavorazione)
                                {
                                    //********VARIAZIONE PER FMCARTON, gli scarti seconda scelta vengono poi rimappati come movimenti

                                    //if (Configurazioni.Esportazione_Formato == eVersioneFormatoEsportazione.V01_14Campi)
                                    //{
                                    //    Zero5.Util.Log.WriteLog($"Attenzione, transazione {transazioniDaEsportare.Transazione_IDTransazione} causale utilizzata per rilavorazione. Il formato V01 non supporta gli scarti seconda scelta. La quantità è stata esportata come scarto normale.");
                                    //    records[key].V01_ERP_QuantitaScartoPrimaScelta += transazioniDaEsportare.Transazione_PezziScarto;
                                    //}
                                    //else
                                    records[key].V02_ERP_QuantitaScartoSecondaScelta_DaRilavorare += transazioniDaEsportare.Transazione_PezziScarto;
                                }
                                else
                                    records[key].V01_ERP_QuantitaScartoPrimaScelta += transazioniDaEsportare.Transazione_PezziScarto;

                                break;
                            case eTipoOperazione_ERP.ConsuntivazioneOre:
                                records[key].V01_ERP_MinutiLavorati += transazioniDaEsportare.Transazione_Minuti;
                                break;
                        }

                        if (!records[key].Phase_lstTransazioniCoinvolte.Contains(transazioniDaEsportare.Transazione_IDTransazione))
                            records[key].Phase_lstTransazioniCoinvolte.Add(transazioniDaEsportare.Transazione_IDTransazione);
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

        private void CalcolaRecordEsportazione_SaldoFase_DaStatoFase(SortedDictionary<string, RecordEsportazioneAvanzamentiERP> records)
        {
            try
            {
                Zero5.Data.Layer.vOrdiniProduzioneFasiProduzione fasiFiniteDaEsportare = new Zero5.Data.Layer.vOrdiniProduzioneFasiProduzione();
                Zero5.Data.Filter.Filter filtro = new Zero5.Data.Filter.Filter();
                filtro.Add(fasiFiniteDaEsportare.Fields.Fase_Stato == Zero5.Data.Layer.FasiProduzione.enumFasiProduzioneStati.Finita);
                filtro.Add(fasiFiniteDaEsportare.Fields.Fase_RiferimentoNumerico3 != (double)eStatoRiga_eSOLVER.Terminato + 100);
                filtro.Add(fasiFiniteDaEsportare.Fields.Ordine_IDTipoOrdine.FilterIn(Common.ListaTipiOrdineGenerico));
                filtro.Add(fasiFiniteDaEsportare.Fields.Fase_CodiceEsterno != "");//idcompletofaseODP
                if (Configurazioni.Esportazione_DataInizio > DateTime.MinValue)
                    filtro.Add(fasiFiniteDaEsportare.Fields.Fase_UltimoCambioStato >= Configurazioni.Esportazione_DataInizio);

                filtro.AddOrderBy(fasiFiniteDaEsportare.Fields.Ordine_IDArticolo);
                filtro.AddOrderBy(fasiFiniteDaEsportare.Fields.Ordine_IDOrdineProduzione);
                filtro.AddOrderBy(fasiFiniteDaEsportare.Fields.Fase_IDFaseProduzione);

                fasiFiniteDaEsportare.Load(filtro);
                Zero5.Util.Log.WriteLog("trovate " + fasiFiniteDaEsportare.RowCount + " fasi finite da esportare");

                if (fasiFiniteDaEsportare.RowCount > 10000)
                {
                    Zero5.Util.Log.WriteLog(filtro.ToStringHumanized());
                }

                if (fasiFiniteDaEsportare.EOF || fasiFiniteDaEsportare.RowCount == 0)
                    return;

                while (!fasiFiniteDaEsportare.EOF)
                {
                    try
                    {
                        eTipoOperazione_ERP tipoRecord = eTipoOperazione_ERP.FlagSaldoFase;
                        string key = "FINEFASE" + fasiFiniteDaEsportare.Fase_IDFaseProduzione;

                        if (!records.ContainsKey(key))
                            records.Add(key, new RecordEsportazioneAvanzamentiERP(fasiFiniteDaEsportare, tipoRecord));
                        records[key].V01_ERP_RigaSaldata = true;

                    }
                    catch (Exception ex)
                    {
                        Zero5.Util.Log.WriteLog("Exc. on CalcolaRecordEsportazione_SaldoFase_DaStatoFase. Fase " + fasiFiniteDaEsportare.Fase_IDFaseProduzione + " " + ex.Message);
                    }
                    fasiFiniteDaEsportare.MoveNext();
                }
            }
            catch (ArgumentException ex)
            {
                Zero5.Util.Log.WriteLog("Exc. on CalcolaRecordEsportazione_Avanzamenti_DaTransazioni. " + ex.Message);
            }
        }



        private void CalcolaRecordEsportazione_AddebitiResi_DaMovimenti(SortedDictionary<string, RecordEsportazioneAvanzamentiERP> records)
        {
            if (Configurazioni.Esportazione_MovimentiEsportaSoloConsumi)
                return;

            try
            {
                Zero5.Data.Layer.vMovimentiArticoliUbicazioniOrdiniFasi movimentiDaEsportare = new Zero5.Data.Layer.vMovimentiArticoliUbicazioniOrdiniFasi();
                Zero5.Data.Filter.Filter filtro = new Zero5.Data.Filter.Filter();
                filtro.Add(movimentiDaEsportare.Fields.Movimenti_Esportato == (int)eLivelloEsportatoMovimenti.NonEsportato);

                filtro.Add(movimentiDaEsportare.Fields.Ordini_IDTipoOrdine.FilterIn(Common.ListaTipiOrdineGenerico));
                filtro.Add(movimentiDaEsportare.Fields.Fasi_CodiceEsterno != "");
                filtro.Add(movimentiDaEsportare.Fields.Movimenti_Causale.FilterIn(new List<int>() { Common.IDCausaleMovimentoAddebito, Common.IDCausaleMovimentoReso }));

                filtro.AddOrderBy(movimentiDaEsportare.Fields.Ubicazioni_IDUbicazione);
                filtro.AddOrderBy(movimentiDaEsportare.Fields.Articoli_IDArticolo);
                filtro.AddOrderBy(movimentiDaEsportare.Fields.Movimenti_Lotto);
                filtro.AddOrderBy(movimentiDaEsportare.Fields.Ordini_IDOrdineProduzione);
                filtro.AddOrderBy(movimentiDaEsportare.Fields.Fasi_IDFaseProduzione);

                movimentiDaEsportare.Load(filtro);

                if (movimentiDaEsportare.EOF || movimentiDaEsportare.RowCount == 0)
                    return;

                Zero5.Util.Log.WriteLog(movimentiDaEsportare.RowCount + " movimenti da esportare per addebiti-resi");


                while (!movimentiDaEsportare.EOF)
                {
                    try
                    {
                        eTipoOperazione_ERP tipoRecord = eTipoOperazione_ERP.Sconosciuto;
                        if (movimentiDaEsportare.Movimenti_Tipo == Movimenti.enumMovimentiTipo.Carico)
                            tipoRecord = eTipoOperazione_ERP.Reso;
                        else
                            tipoRecord = eTipoOperazione_ERP.Addebito;

                        string key = CalcolaChiaveRecord(movimentiDaEsportare, tipoRecord);

                        if (!records.ContainsKey(key))
                            records.Add(key, new RecordEsportazioneAvanzamentiERP(movimentiDaEsportare, tipoRecord));

                        records[key].V01_ERP_QuantitaPrincipale += movimentiDaEsportare.Movimenti_Quantita;

                        if (!records[key].Phase_lstMovimentiCoinvolti.Contains(movimentiDaEsportare.Movimenti_IDMovimento))
                            records[key].Phase_lstMovimentiCoinvolti.Add(movimentiDaEsportare.Movimenti_IDMovimento);

                    }
                    catch (Exception ex)
                    {
                        Zero5.Util.Log.WriteLog("Exc. on CalcolaRecordEsportazione_AddebitiResi_DaMovimenti. Movimenti " + movimentiDaEsportare.Movimenti_IDMovimento + " " + ex.Message);
                    }
                    movimentiDaEsportare.MoveNext();
                }
            }
            catch (Exception ex)
            {
                Zero5.Util.Log.WriteLog("Exc. on CalcolaRecordEsportazione_AddebitiResi_DaMovimenti. " + ex.Message);
            }
        }

        private void CalcolaRecordEsportazione_Consumi_DaMovimenti(SortedDictionary<string, RecordEsportazioneAvanzamentiERP> records)
        {
            try
            {
                Zero5.Data.Layer.vMovimentiArticoliUbicazioniOrdiniFasi movimentiDaEsportare = new Zero5.Data.Layer.vMovimentiArticoliUbicazioniOrdiniFasi();
                Zero5.Data.Filter.Filter filtro = new Zero5.Data.Filter.Filter();

                if (Configurazioni.Esportazione_MovimentiEsportaSoloConsumi)
                    filtro.Add(movimentiDaEsportare.Fields.Movimenti_Esportato == (int)eLivelloEsportatoMovimenti.NonEsportato);
                else
                    filtro.Add(movimentiDaEsportare.Fields.Movimenti_Esportato == (int)eLivelloEsportatoMovimenti.EsportatoAddebitoReso);

                filtro.Add(movimentiDaEsportare.Fields.Ordini_IDTipoOrdine.FilterIn(Common.ListaTipiOrdineGenerico));
                filtro.Add(movimentiDaEsportare.Fields.Fasi_CodiceEsterno != "");
                // filtro.Add(movimentiDaEsportare.Fields.Movimenti_IDRiga != 0);
                filtro.Add(movimentiDaEsportare.Fields.Movimenti_Causale.FilterIn(new List<int>() { Common.IDCausaleMovimentoAddebito, Common.IDCausaleMovimentoReso }));
                filtro.Add(movimentiDaEsportare.Fields.Fasi_Stato == Zero5.Data.Layer.FasiProduzione.enumFasiProduzioneStati.Finita);

                filtro.AddOrderBy(movimentiDaEsportare.Fields.Ubicazioni_IDUbicazione);
                filtro.AddOrderBy(movimentiDaEsportare.Fields.Articoli_IDArticolo);
                filtro.AddOrderBy(movimentiDaEsportare.Fields.Movimenti_Lotto);
                filtro.AddOrderBy(movimentiDaEsportare.Fields.Ordini_IDOrdineProduzione);
                filtro.AddOrderBy(movimentiDaEsportare.Fields.Fasi_IDFaseProduzione);

                movimentiDaEsportare.Load(filtro);

                if (movimentiDaEsportare.EOF || movimentiDaEsportare.RowCount == 0)
                    return;

                Zero5.Util.Log.WriteLog(movimentiDaEsportare.RowCount + " movimenti da esportare per consumi");

                while (!movimentiDaEsportare.EOF)
                {
                    try
                    {
                        string key = CalcolaChiaveRecord(movimentiDaEsportare, eTipoOperazione_ERP.ConsumoMateriali);

                        if (!records.ContainsKey(key))
                            records.Add(key, new RecordEsportazioneAvanzamentiERP(movimentiDaEsportare, eTipoOperazione_ERP.ConsumoMateriali));

                        if (movimentiDaEsportare.Movimenti_Tipo == Movimenti.enumMovimentiTipo.Carico)
                            records[key].V01_ERP_QuantitaPrincipale -= movimentiDaEsportare.Movimenti_Quantita;
                        else
                            records[key].V01_ERP_QuantitaPrincipale += movimentiDaEsportare.Movimenti_Quantita;

                        if (!records[key].Phase_lstMovimentiCoinvolti.Contains(movimentiDaEsportare.Movimenti_IDMovimento))
                            records[key].Phase_lstMovimentiCoinvolti.Add(movimentiDaEsportare.Movimenti_IDMovimento);

                    }
                    catch (Exception ex)
                    {
                        Zero5.Util.Log.WriteLog("Exc. on CalcolaRecordEsportazione_Consumi_DaMovimenti. Movimenti " + movimentiDaEsportare.Movimenti_IDMovimento + " " + ex.Message);
                    }
                    movimentiDaEsportare.MoveNext();
                }
            }
            catch (Exception ex)
            {
                Zero5.Util.Log.WriteLog("Exc. on CalcolaRecordEsportazione_Consumi_DaMovimenti. " + ex.Message);
            }
        }


        private string CalcolaChiaveRecord(Zero5.Data.Layer.vOrdiniProduzioneFasiProduzioneTransazioni transazione, eTipoOperazione_ERP tipoAvanzamento)
        {
            return transazione.Transazione_Inizio.ToString("ddMMyyyy") + "_" +
                                  transazione.Fase_IDFaseProduzione + "_" +
                                  transazione.Transazione_IDRisorsaMacchina + "_" +
                                  transazione.Transazione_IDRisorsaUomo + "_" +
                                  transazione.Transazione_Causale + "_" +
                                  (int)tipoAvanzamento;
        }

        private string CalcolaChiaveRecord(Zero5.Data.Layer.vMovimentiArticoliUbicazioniOrdiniFasi movimento, eTipoOperazione_ERP tipoAvanzamento)
        {
            string key =
                                  movimento.Movimenti_IDFaseProduzione + "_" +
                                  movimento.Movimenti_IDRiga + "_" +
                                  movimento.Movimenti_IDRisorsaMacchina + "_" +
                                  movimento.Movimenti_IDArticolo + "_" +
                                  movimento.Movimenti_Lotto + "_" +
                                  movimento.Movimenti_IDUbicazione + "_" +
                                  (int)tipoAvanzamento;

            if (tipoAvanzamento == eTipoOperazione_ERP.Addebito || tipoAvanzamento == eTipoOperazione_ERP.Reso)
                key += movimento.Movimenti_Causale + "_" +
                    movimento.Movimenti_Tipo;
            return key;
        }

        private void ImpostaDatiAggiuntiviRecord(SortedDictionary<string, RecordEsportazioneAvanzamentiERP> lstRecord)
        {
            List<int> idArticoliCoinvolti = new List<int>();
            List<int> idRigheDistintaCoinvolte = new List<int>();

            foreach (KeyValuePair<string, RecordEsportazioneAvanzamentiERP> kvp in lstRecord)
            {
                if (!idArticoliCoinvolti.Contains(kvp.Value.Phase_IdArticolo))
                    idArticoliCoinvolti.Add(kvp.Value.Phase_IdArticolo);

                if (!idRigheDistintaCoinvolte.Contains(kvp.Value.Phase_idRigaDistinta))
                    idRigheDistintaCoinvolte.Add(kvp.Value.Phase_idRigaDistinta);
            }

            Zero5.Data.Layer.Risorse risorse = new Risorse();
            risorse.LoadAll();

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

            foreach (KeyValuePair<string, RecordEsportazioneAvanzamentiERP> kvp in lstRecord)
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

                risorse.MoveToPrimaryKey(kvp.Value.Phase_IdRisorsa);
                kvp.Value.V01_ERP_CodiceRisorsa = risorse.CodiceEsterno;

                if (kvp.Value.V01_ERP_TipoOperazioneAvanzamento == eTipoOperazione_ERP.ConsumoMateriali ||
                    kvp.Value.V01_ERP_TipoOperazioneAvanzamento == eTipoOperazione_ERP.Addebito ||
                    kvp.Value.V01_ERP_TipoOperazioneAvanzamento == eTipoOperazione_ERP.Reso)
                {
                    causaliMovimento.MoveToPrimaryKey(kvp.Value.Phase_IdCausale);
                    kvp.Value.V01_ERP_CodiceCausale = causaliMovimento.CodiceEsterno;

                    if (kvp.Value.Phase_idRigaDistinta != 0)
                    {
                        righeDistinta.MoveToPrimaryKey(kvp.Value.Phase_idRigaDistinta);
                        kvp.Value.V01_ERP_RiferimentoOrdineProduzione = righeDistinta.RiferimentoEsterno;
                    }
                    else if (kvp.Value.Phase_IdFaseProduzione != 0)
                    {
                        Zero5.Data.Layer.FasiProduzione fp = new FasiProduzione();
                        fp.LoadByPrimaryKey(kvp.Value.Phase_IdFaseProduzione);
                        kvp.Value.V01_ERP_RiferimentoOrdineProduzione = fp.CodiceEsterno;
                    }
                }
                else if (kvp.Value.V01_ERP_TipoOperazioneAvanzamento == eTipoOperazione_ERP.AvanzamentoFase ||
                   kvp.Value.V01_ERP_TipoOperazioneAvanzamento == eTipoOperazione_ERP.ConsuntivazioneOre)
                {
                    causaliTransazioni.MoveToPrimaryKey(kvp.Value.Phase_IdCausale);
                    kvp.Value.V01_ERP_CodiceCausale = causaliTransazioni.CodiceEsterno;
                }
                if (kvp.Value.V01_ERP_TipoOperazioneAvanzamento == eTipoOperazione_ERP.FlagSaldoFase)
                    kvp.Value.V01_ERP_TipoRecord_TrueTesta_FalseRiga = eTipoRecord_ERP.Testa;

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
