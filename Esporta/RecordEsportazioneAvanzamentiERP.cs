using Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Esporta
{
    class RecordEsportazioneAvanzamentiERP
    {
        public eTipoRecord_ERP V01_ERP_TipoRecord_TrueTesta_FalseRiga = eTipoRecord_ERP.Riga;
        public eTipoOperazione_ERP V01_ERP_TipoOperazioneAvanzamento = eTipoOperazione_ERP.Sconosciuto;
        public DateTime V01_ERP_DataRegistrazione = DateTime.MinValue;
        public string V01_ERP_RiferimentoOrdineProduzione = "";
        public string V01_ERP_CodiceArticolo = "";
        public string V01_ERP_CodiceVarianteArticolo = "";
        public double V01_ERP_QuantitaPrincipale = 0;
        public double V01_ERP_QuantitaScartoPrimaScelta = 0;
        public string V01_ERP_RiferimentoLotto_Alfanumerico = "";
        public bool V01_ERP_RigaSaldata = false;
        public string V01_ERP_CodiceRisorsa = "";
        public double V01_ERP_MinutiLavorati = 0;
        public string V01_ERP_CodiceCausale = "";
        public string V01_ERP_Descrizione = "";

        public double V02_ERP_QuantitaScartoSecondaScelta_DaRilavorare = 0;
        public string V02_ERP_RiferimentoLotto_Data = "";//NON ANCORA UTILIZZATO
        public string V02_ERP_RiferimentoLotto_Numero = "";//NON ANCORA UTILIZZATO

        public string V03_ERP_RiferimentoLottoPF_CodiceAlfanumerico = "";
        public string V03_ERP_RiferimentoLottoPF_Data = "";
        public string V03_ERP_RiferimentoLottoPF_Numero = "";

        public int Phase_IdArticolo = 0;
        public int Phase_IdCausale = 0;
        public int Phase_IdRisorsa = 0;
        public int Phase_IdUbicazione = 0;
        public int Phase_idOrdineProduzione = 0;
        public int Phase_idRigaDistinta = 0;
        public int Phase_IdFaseProduzione = 0;

        public List<int> Phase_lstTransazioniCoinvolte = new List<int>();
        public List<int> Phase_lstMovimentiCoinvolti = new List<int>();
        public bool Esportata = false;

        public string FormatToCsvString(eVersioneFormatoEsportazione formato)
        {
            string MagazzinoDestinazione = "";

            eTipoOperazione_ERP tipoOperazioenAvanzamentoForzata = V01_ERP_TipoOperazioneAvanzamento;

            //****INIZIO****VARIANTE FMCARTON
            if (V02_ERP_QuantitaScartoSecondaScelta_DaRilavorare > 0)
            {
                //I versamenti scarto seconda scelta da rilavorare, da soluzione proposta da Andrea Sartori a cliente, vengono gestiti in modo fittizio come movimentazione di materiale
                tipoOperazioenAvanzamentoForzata = eTipoOperazione_ERP.Addebito;
                MagazzinoDestinazione = "SC2";

                V01_ERP_QuantitaPrincipale = V02_ERP_QuantitaScartoSecondaScelta_DaRilavorare;
                V02_ERP_QuantitaScartoSecondaScelta_DaRilavorare = 0;

                Zero5.Util.Log.WriteLog("VARIANTE FMCARTON: scarto seconda scelta considerato come trasferimento materiali");

                Zero5.Data.Layer.OrdiniProduzione op = new Zero5.Data.Layer.OrdiniProduzione();
                op.Load(op.Fields.IDOrdineProduzione == Phase_idOrdineProduzione);


                Zero5.Data.Layer.DistintaBase distinta = new Zero5.Data.Layer.DistintaBase();
                distinta.Load(distinta.Fields.IDDistintaBasePadre > 0,
                    distinta.Fields.IDDistintaBasePadre == op.IDDistintaBase,
                    distinta.Fields.UnitaMisura == "PZ");

                Zero5.Util.Log.WriteLog($"Transazione {V01_ERP_Descrizione}, quantità scarto da considerare un trasferimento: {V01_ERP_QuantitaPrincipale}. Selezionata la riga di impegno da utilizzare per la trasmissione a eSOLVER:PH{distinta.IDDistintaBase} IDPadre eSOLVER {distinta.RiferimentoEsterno} {distinta.CodiceArticolo} {distinta.UnitaMisura}");
                V01_ERP_RiferimentoOrdineProduzione = distinta.RiferimentoEsterno;
            }

            string tipoRecord = "RIG";
            if (V01_ERP_TipoRecord_TrueTesta_FalseRiga == eTipoRecord_ERP.Testa)
                tipoRecord = "TES";

            if (formato == eVersioneFormatoEsportazione.V01_14Campi)
            {
                Esportata = true;
                return tipoRecord + ";" +
                        ((int)tipoOperazioenAvanzamentoForzata).ToString() + ";" +
                        V01_ERP_DataRegistrazione.ToString("dd/MM/yyyy") + ";" +
                        V01_ERP_RiferimentoOrdineProduzione + ";" +
                        V01_ERP_CodiceArticolo + ";" +
                        V01_ERP_CodiceVarianteArticolo + ";" +
                        V01_ERP_QuantitaPrincipale.ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                        V01_ERP_QuantitaScartoPrimaScelta.ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                        V01_ERP_RiferimentoLotto_Alfanumerico + ";" +
                        (V01_ERP_RigaSaldata ? "1" : "0") + ";" +
                        V01_ERP_CodiceRisorsa + ";" +
                        (V01_ERP_MinutiLavorati / 60).ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                        V01_ERP_CodiceCausale + ";" +
                        V01_ERP_Descrizione + ";" +
                        MagazzinoDestinazione;
            }
            else if (formato == eVersioneFormatoEsportazione.V02_17Campi)
            {
                Esportata = true;
                return tipoRecord + ";" +
                        ((int)tipoOperazioenAvanzamentoForzata).ToString() + ";" +
                        V01_ERP_DataRegistrazione.ToString("dd/MM/yyyy") + ";" +
                        V01_ERP_RiferimentoOrdineProduzione + ";" +
                        V01_ERP_CodiceArticolo + ";" +
                        V01_ERP_CodiceVarianteArticolo + ";" +
                        V01_ERP_QuantitaPrincipale.ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                        V01_ERP_QuantitaScartoPrimaScelta.ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                        V02_ERP_QuantitaScartoSecondaScelta_DaRilavorare.ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                        V01_ERP_RiferimentoLotto_Alfanumerico + ";" +
                        V02_ERP_RiferimentoLotto_Data + ";" +
                        V02_ERP_RiferimentoLotto_Numero + ";" +
                        (V01_ERP_RigaSaldata ? "1" : "0") + ";" +
                        V01_ERP_CodiceRisorsa + ";" +
                        (V01_ERP_MinutiLavorati / 60).ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                        V01_ERP_CodiceCausale + ";" +
                        V01_ERP_Descrizione + ";" +
                        MagazzinoDestinazione;
            }
            else
            if (formato == eVersioneFormatoEsportazione.V03_20Campi)
            {
                Esportata = true;
                return tipoRecord + ";" +
                        ((int)tipoOperazioenAvanzamentoForzata).ToString() + ";" +
                        V01_ERP_DataRegistrazione.ToString("dd/MM/yyyy") + ";" +
                        V01_ERP_RiferimentoOrdineProduzione + ";" +
                        V01_ERP_CodiceArticolo + ";" +
                        V01_ERP_CodiceVarianteArticolo + ";" +
                        V01_ERP_QuantitaPrincipale.ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                        V01_ERP_QuantitaScartoPrimaScelta.ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                        V02_ERP_QuantitaScartoSecondaScelta_DaRilavorare.ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                        V01_ERP_RiferimentoLotto_Alfanumerico + ";" +
                        V02_ERP_RiferimentoLotto_Data + ";" +
                        V02_ERP_RiferimentoLotto_Numero + ";" +
                        (V01_ERP_RigaSaldata ? "1" : "0") + ";" +
                        V01_ERP_CodiceRisorsa + ";" +
                        (V01_ERP_MinutiLavorati / 60).ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                        V01_ERP_CodiceCausale + ";" +
                        V01_ERP_Descrizione + ";" +
                        V03_ERP_RiferimentoLottoPF_CodiceAlfanumerico + ";" +
                        V03_ERP_RiferimentoLottoPF_Data + ";" +
                        V03_ERP_RiferimentoLottoPF_Numero + ";" +
                        MagazzinoDestinazione;
            }

            //****FINE*******VARIANTE FMCARTON


            throw new Exception("Formato esportazione sconosciuto");
        }

        /// <summary>
        /// Crea un record esportazione a partire da una transazione. NB: non somma il valore della transazione corrente.
        /// </summary>
        /// <param name="transazioniDaEsportare"></param>
        /// <param name="tipoRiga"></param>
        public RecordEsportazioneAvanzamentiERP(Zero5.Data.Layer.vOrdiniProduzioneFasiProduzioneTransazioni transazioniDaEsportare, eTipoOperazione_ERP tipoRiga)
        {
            V01_ERP_Descrizione = "PHA" + transazioniDaEsportare.Transazione_IDTransazione.ToString();
            V01_ERP_TipoOperazioneAvanzamento = tipoRiga;
            V01_ERP_DataRegistrazione = transazioniDaEsportare.Transazione_Inizio.Date;
            V01_ERP_RiferimentoOrdineProduzione = transazioniDaEsportare.Fase_CodiceEsterno;

            Phase_idOrdineProduzione = transazioniDaEsportare.Ordine_IDOrdineProduzione;
            Phase_IdFaseProduzione = transazioniDaEsportare.Transazione_IDFaseProduzione;
            Phase_IdArticolo = transazioniDaEsportare.Ordine_IDArticolo;

            Phase_IdRisorsa = transazioniDaEsportare.Transazione_IDRisorsaMacchina;
            if (transazioniDaEsportare.Transazione_IDRisorsaUomo != 0 && tipoRiga == eTipoOperazione_ERP.ConsuntivazioneOre)
                Phase_IdRisorsa = transazioniDaEsportare.Transazione_IDRisorsaUomo;

            if (Configurazioni.Esportazione_VersamentiForzaLottoFittizi &&
                (transazioniDaEsportare.Transazione_PezziBuoni != 0 || transazioniDaEsportare.Transazione_PezziScarto != 0) &&
                tipoRiga == eTipoOperazione_ERP.AvanzamentoFase)
                V01_ERP_RiferimentoLotto_Alfanumerico = Configurazioni.Esportazione_VersamentiLottoFittizio;

            Phase_IdCausale = transazioniDaEsportare.Transazione_Causale;
        }

        /// <summary>
        /// Crea un record esportazione a partire da una fase produzione.
        /// </summary>
        /// <param name="fasiDaEsportare"></param>
        /// <param name="tipoRiga"></param>
        public RecordEsportazioneAvanzamentiERP(Zero5.Data.Layer.vOrdiniProduzioneFasiProduzione fasiDaEsportare, eTipoOperazione_ERP tipoRiga)
        {
            V01_ERP_Descrizione = "PHAF" + fasiDaEsportare.Fase_IDFaseProduzione.ToString();
            V01_ERP_TipoOperazioneAvanzamento = tipoRiga;
            V01_ERP_DataRegistrazione = DateTime.Now.Date;
            V01_ERP_RiferimentoOrdineProduzione = fasiDaEsportare.Fase_CodiceEsterno;

            Phase_idOrdineProduzione = fasiDaEsportare.Ordine_IDOrdineProduzione;
            Phase_IdFaseProduzione = fasiDaEsportare.Fase_IDFaseProduzione;
            Phase_IdArticolo = fasiDaEsportare.Ordine_IDArticolo;

            Phase_IdRisorsa = fasiDaEsportare.Fase_IDRisorsaMacchinaRealeUltima;
        }

        /// <summary>
        ///  Crea un record esportazione a partire da un movimento. NB: non somma il valore del movimento corrente.
        /// </summary>
        /// <param name="movimentiDaEsportare"></param>
        /// <param name="tipoRiga"></param>
        public RecordEsportazioneAvanzamentiERP(Zero5.Data.Layer.vMovimentiArticoliUbicazioniOrdiniFasi movimentiDaEsportare, eTipoOperazione_ERP tipoRiga)
        {
            //PHA serve per poter riconoscere i movimenti presenti in eSolver generati di Phase, escludendoli dall'importazione
            V01_ERP_Descrizione = "PHA" + movimentiDaEsportare.Movimenti_IDMovimento.ToString();
            V01_ERP_TipoOperazioneAvanzamento = tipoRiga;
            V01_ERP_DataRegistrazione = movimentiDaEsportare.Movimenti_DataOra.Date;

            Phase_idOrdineProduzione = movimentiDaEsportare.Movimenti_IDOrdineProduzione;
            Phase_IdFaseProduzione = movimentiDaEsportare.Movimenti_IDFaseProduzione;
            Phase_IdArticolo = movimentiDaEsportare.Movimenti_IDArticolo;

            V01_ERP_RiferimentoLotto_Alfanumerico = movimentiDaEsportare.Movimenti_Lotto;

            Phase_IdRisorsa = movimentiDaEsportare.Movimenti_IDRisorsaMacchina;

            Phase_IdCausale = movimentiDaEsportare.Movimenti_Causale;
            Phase_IdUbicazione = movimentiDaEsportare.Movimenti_IDUbicazione;
            Phase_idRigaDistinta = movimentiDaEsportare.Movimenti_IDRiga;
            V03_ERP_RiferimentoLottoPF_CodiceAlfanumerico = movimentiDaEsportare.Ordini_Lotto;
            //TODO: esportare anche un ubicazione per permettere di scaricare dall'ubicazione corretta
        }

        public void MarcaEsportateTransazioniCoinvolte()
        {
            try
            {
                if (Phase_lstTransazioniCoinvolte.Count > 0)
                {
                    List<List<int>> multipleList = Zero5.Util.Common.SplitList(Phase_lstTransazioniCoinvolte, 300);
                    foreach (List<int> lstTransazioni in multipleList)
                    {
                        Zero5.Data.Layer.Transazioni transUpdate = new Zero5.Data.Layer.Transazioni();
                        transUpdate.Load(transUpdate.Fields.IDTransazione.FilterIn(lstTransazioni));

                        while (!transUpdate.EOF)
                        {
                            transUpdate.Esportato = 1;
                            transUpdate.MoveNext();
                        }
                        transUpdate.Save();
                    }
                }

                if (V01_ERP_RigaSaldata)
                {
                    Zero5.Data.Layer.FasiProduzione fp = new Zero5.Data.Layer.FasiProduzione();
                    fp.Load(fp.Fields.IDFaseProduzione == Phase_IdFaseProduzione);

                    fp.RiferimentoNumerico3 = (double)eStatoRiga_eSOLVER.Terminato + 100;
                    fp.Save();
                }
            }
            catch (Exception ex)
            {
                Zero5.Util.Log.WriteLog("Eccezione salvataggio esportato = 1 per transazioni ID: " +
                    Zero5.Util.StringConverters.IntListToString(Phase_lstTransazioniCoinvolte) + Environment.NewLine + "Exc. " + ex.Message);
                Zero5.Util.Log.WriteLog("ERRORE_ESPORTAZIONE", "Eccezione salvataggio esportato = 1 per transazioni ID: " + Zero5.Util.StringConverters.IntListToString(Phase_lstTransazioniCoinvolte) + Environment.NewLine + "Exc. " + ex.Message);
            }
        }

        public void MarcaEsportatiMovimentiCoinvolti(eLivelloEsportatoMovimenti livelloEsportazione)
        {
            try
            {
                if (Phase_lstMovimentiCoinvolti.Count > 0)
                {
                    List<List<int>> multipleList = Zero5.Util.Common.SplitList(Phase_lstMovimentiCoinvolti, 300);
                    foreach (List<int> lstMovimenti in multipleList)
                    {
                        Zero5.Data.Layer.Movimenti movimentiUpdate = new Zero5.Data.Layer.Movimenti();
                        movimentiUpdate.Load(movimentiUpdate.Fields.IDMovimento.FilterIn(lstMovimenti));

                        while (!movimentiUpdate.EOF)
                        {
                            movimentiUpdate.Esportato = (int)livelloEsportazione;
                            movimentiUpdate.MoveNext();
                        }
                        movimentiUpdate.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                Zero5.Util.Log.WriteLog("Eccezione salvataggio esportato = 1 per movimenti ID: " +
                    Zero5.Util.StringConverters.IntListToString(Phase_lstMovimentiCoinvolti) + Environment.NewLine + "Exc. " + ex.Message);
                Zero5.Util.Log.WriteLog("ERRORE_ESPORTAZIONE", "Eccezione salvataggio esportato = 1 per transazioni ID: " + Zero5.Util.StringConverters.IntListToString(Phase_lstMovimentiCoinvolti) + Environment.NewLine + "Exc. " + ex.Message);
            }
        }
    }
}
