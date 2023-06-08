using Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Esporta
{
    class RecordEsportazioneVersamentiCustom
    {
        public eTipoRecord_ERP V01_ERP_TipoRecord_TrueTesta_FalseRiga = eTipoRecord_ERP.Riga;
        public eTipoMagazzinoCoinvolto_ERP V01_ERP_MagazzinoCoinvolto = eTipoMagazzinoCoinvolto_ERP.PF;
        private eTipoOperazione_ERP V01_ERP_TipoOperazioneAvanzamento = eTipoOperazione_ERP.AvanzamentoFase;
        public DateTime V01_ERP_DataRegistrazione = DateTime.MinValue;
        public string V01_ERP_RiferimentoOrdineProduzione = "";
        public string V01_ERP_CodiceArticolo = "";
        public string V01_ERP_CodiceVarianteArticolo = "";
        public double V01_ERP_QuantitaPrincipale = 0;
        public double V01_ERP_QuantitaScartoPrimaScelta = 0;
        public string V01_ERP_RiferimentoLotto_Alfanumerico = "";
        public bool V01_ERP_RigaSaldata = false;
        public double V01_ERP_MinutiLavorati = 0;
        public string V01_ERP_CodiceCausale = "";
        public string V01_ERP_Descrizione = "";
        public string V01_ERP_CodiceCommessa = "";

        public double V02_ERP_QuantitaScartoSecondaScelta_DaRilavorare = 0;
        public string V02_ERP_RiferimentoLotto_Data = "";//NON ANCORA UTILIZZATO
        public string V02_ERP_RiferimentoLotto_Numero = "";//NON ANCORA UTILIZZATO

        public string V03_ERP_RiferimentoLottoPF_CodiceAlfanumerico = "";
        public string V03_ERP_RiferimentoLottoPF_Data = "";
        public string V03_ERP_RiferimentoLottoPF_Numero = "";

        public int Phase_IdArticolo = 0;
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

            //****INIZIO****VARIANTE ORDINI AUTOMATICI FMCARTON
            
            if (V02_ERP_QuantitaScartoSecondaScelta_DaRilavorare > 0)
            {               
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
                //return tipoRecord + ";" +
                //        ((int)tipoOperazioenAvanzamentoForzata).ToString() + ";" +
                //        V01_ERP_DataRegistrazione.ToString("dd/MM/yyyy") + ";" +
                //        V01_ERP_RiferimentoOrdineProduzione + ";" +
                //        V01_ERP_CodiceArticolo + ";" +
                //        V01_ERP_CodiceVarianteArticolo + ";" +
                //        V01_ERP_QuantitaPrincipale.ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                //        V01_ERP_CodiceRisorsa + ";" +
                //        (V01_ERP_MinutiLavorati / 60).ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                //        V01_ERP_CodiceCausale + ";" +
                //        V01_ERP_Descrizione + ";" +
                //        MagazzinoDestinazione;

                /* esportiamo magazzino e articolo e pezzi  
                   */

                string magazzinoMateriePrime = "";
                string magazzino = "PF";
                string tipoRiga = "1";
                string codiceArticolo = V01_ERP_CodiceArticolo;
                if (V01_ERP_MagazzinoCoinvolto == eTipoMagazzinoCoinvolto_ERP.SC1)
                {
                    magazzino = "SC1";
                }

                if (V01_ERP_MagazzinoCoinvolto == eTipoMagazzinoCoinvolto_ERP.SC2)
                {
                    int index;
                    string substring = "-";

                    if (V01_ERP_CodiceArticolo.Contains("-"))
                    {
                        index = V01_ERP_CodiceArticolo.IndexOf("-");
                        substring = V01_ERP_CodiceArticolo.Substring(index);
                    }

                    codiceArticolo = V01_ERP_CodiceArticolo.Replace(substring, "") + "S";
                    magazzino = "SC2";
                    magazzinoMateriePrime = "MAT";
                    tipoRiga = "3";
                }

                return V01_ERP_DataRegistrazione.ToString("dd/MM/yyyy") + ";" +
                       Phase_idOrdineProduzione + ";" +
                       tipoRiga + ";" +
                       codiceArticolo + ";" +
                       Math.Abs(V01_ERP_QuantitaPrincipale).ToString(CultureInfo.InvariantCulture).Replace('.', ',') + ";" +
                       magazzino + ";" + //magazzino
                       V01_ERP_CodiceCommessa + ";" + //area
                       magazzinoMateriePrime + ";" + V01_ERP_CodiceCommessa; 

                /*
                 * 28/10/2022;1000;1;1779501;100;PF;;;

28/10/2022;1000;2;1779501S;100;MAT;;;

Righe importazione scarto di macchina

28/10/2022;1000;1;1779501;100;SC1;;;

28/10/2022;1000;2;1779501S;100;MAT;;;

 

Righe importazione scarto altro

28/10/2022;1000;3;1779501S;100;SC2;;MAT;*/
            }

            //****FINE*******VARIANTE ORDINI AUTOMATICI FMCARTON


            throw new Exception("Formato esportazione sconosciuto");
        }

        /// <summary>
        /// Crea un record esportazione a partire da una transazione. NB: non somma il valore della transazione corrente.
        /// </summary>
        /// <param name="transazioniDaEsportare"></param>
        /// <param name="tipoRiga"></param>
        public RecordEsportazioneVersamentiCustom(Zero5.Data.Layer.vOrdiniProduzioneFasiProduzioneTransazioni transazioniDaEsportare, eTipoOperazione_ERP tipoRiga, eTipoMagazzinoCoinvolto_ERP magazzino)
        {
            V01_ERP_MagazzinoCoinvolto = magazzino;
            V01_ERP_Descrizione = "PHA" + transazioniDaEsportare.Transazione_IDTransazione.ToString();
            V01_ERP_TipoOperazioneAvanzamento = tipoRiga;
            V01_ERP_DataRegistrazione = transazioniDaEsportare.Transazione_Inizio.Date;
            V01_ERP_RiferimentoOrdineProduzione = transazioniDaEsportare.Fase_CodiceEsterno;
            V01_ERP_CodiceCommessa = transazioniDaEsportare.Ordine_Commessa;

            Phase_idOrdineProduzione = transazioniDaEsportare.Ordine_IDOrdineProduzione;
            Phase_IdFaseProduzione = transazioniDaEsportare.Transazione_IDFaseProduzione;
            Phase_IdArticolo = transazioniDaEsportare.Ordine_IDArticolo;
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
    }
}
