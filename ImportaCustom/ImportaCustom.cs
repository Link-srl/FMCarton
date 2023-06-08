using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace FMCarton
{
    class Importa
    {
        public static FileConfigurazione Parametri = new FileConfigurazione();

        public void CalcolaBancaliPrevisti()
        {
            Zero5.Util.Log.WriteLog("Inizio CalcolaBancaliPrevisti.");

            Zero5.Data.Layer.FasiProduzione fasiProduzioneDaAggiornare = new Zero5.Data.Layer.FasiProduzione();
            Zero5.Data.Filter.Filter filtro = new Zero5.Data.Filter.Filter();
            filtro.Add(fasiProduzioneDaAggiornare.Fields.Stato != Zero5.Data.Layer.FasiProduzione.enumFasiProduzioneStati.Finita);
            filtro.AddOpenBracket();
            filtro.Add(fasiProduzioneDaAggiornare.Fields.extInt01 == 0);
            filtro.AddOR();
            filtro.Add(fasiProduzioneDaAggiornare.Fields.extInt01.FilterIsNull());
            filtro.AddCloseBracket();
            fasiProduzioneDaAggiornare.Load(filtro);

            Zero5.Util.Log.WriteLog("Trovati " + fasiProduzioneDaAggiornare.RowCount.ToString("N0") + " fasi da aggiornare.");

            if (fasiProduzioneDaAggiornare.RowCount == 0)
            {
                return;
            }

            Zero5.Data.Layer.OrdiniProduzione ordiniProduzioneCoinvolti = new Zero5.Data.Layer.OrdiniProduzione();
            ordiniProduzioneCoinvolti.Load(ordiniProduzioneCoinvolti.Fields.IDOrdineProduzione.FilterIn(fasiProduzioneDaAggiornare.GetIntListFromField(fasiProduzioneDaAggiornare.Fields.IDOrdineProduzione)));

            while (!fasiProduzioneDaAggiornare.EOF)
            {
                System.Threading.Thread.Sleep(1);

                try
                {
                    ordiniProduzioneCoinvolti.MoveToNextFieldValue(ordiniProduzioneCoinvolti.Fields.IDOrdineProduzione, fasiProduzioneDaAggiornare.IDOrdineProduzione, true);

                    Zero5.Data.Layer.AliasArticoliClientiFornitori alias = new Zero5.Data.Layer.AliasArticoliClientiFornitori();
                    filtro = new Zero5.Data.Filter.Filter();
                    filtro.Add(alias.Fields.IDArticolo == ordiniProduzioneCoinvolti.IDArticolo);
                    filtro.Add(alias.Fields.UnitaDiMisura == "PL");
                    alias.Load(filtro);

                    if (!alias.EOF)
                    {
                        fasiProduzioneDaAggiornare.PezziPerUnitaLogistica = (int)alias.FattoreDiConversione;

                        if (fasiProduzioneDaAggiornare.PezziPerUnitaLogistica != 0)
                        {
                            fasiProduzioneDaAggiornare.extInt01 = (int)(fasiProduzioneDaAggiornare.QtaPrevista / fasiProduzioneDaAggiornare.PezziPerUnitaLogistica);
                        }
                        
                        if (fasiProduzioneDaAggiornare.RowChangedCount > 0)
                            fasiProduzioneDaAggiornare.Save();
                    }

                }
                catch (Exception ex)
                {
                    Zero5.Util.Log.WriteLog("Errore calcolo bancali previsti fase " + fasiProduzioneDaAggiornare.IDFaseProduzione + " - " + ex.Message);
                }

                if (fasiProduzioneDaAggiornare.RowIndex % 50 == 0)
                {
                    Zero5.Util.Log.WriteLog("Fasi rimanenti " + (fasiProduzioneDaAggiornare.RowCount - fasiProduzioneDaAggiornare.RowIndex).ToString("N0"));
                }

                fasiProduzioneDaAggiornare.MoveNext();
            }
        }
    }
}
