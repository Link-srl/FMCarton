using System;
using System.Collections.Generic;
using System.Text;
using Zero5.Data.Layer;

namespace Shared
{
    class Configurazioni
    {
        public static string GetConfigFromOpzioniWithDefaultValue(string defaultValue, Opzioni.enumOpzioniID opzione)
        {
            string config = Zero5.Data.Layer.Opzioni.helper.LoadStringValue(opzione);
            if (config != "")
                return config;
            return defaultValue;
        }

        private static Dictionary<int, string> internalStringOptionDictionary = new Dictionary<int, string>();

        private static string LoadStringFromOpzioni(Zero5.Data.Layer.Opzioni.enumOpzioniID opz)
        {
            if (!internalStringOptionDictionary.ContainsKey((int)opz))
            {
                string value = Zero5.Data.Layer.Opzioni.helper.LoadStringValue(opz);
                internalStringOptionDictionary.Add((int)opz, value);
            }
            return internalStringOptionDictionary[(int)opz];
        }

        private static Dictionary<int, int> internalIntOptionDictionary = new Dictionary<int, int>();
        private static int LoadIntFromOpzioni(Zero5.Data.Layer.Opzioni.enumOpzioniID opz)
        {
            if (!internalIntOptionDictionary.ContainsKey((int)opz))
            {
                int value = Zero5.Data.Layer.Opzioni.helper.LoadIntValue(opz);
                internalIntOptionDictionary.Add((int)opz, value);
            }
            return internalIntOptionDictionary[(int)opz];
        }

        public static eTipoScambioDatiEsolver ModalitaIntegrazioneEsolver
        {
            get
            {
                return (eTipoScambioDatiEsolver)LoadIntFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_TipoScambioDati);
            }
        }

        public static string WsSistemi_Server
        {
            get
            {
                string server = LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_WsSistemi_Server);
                if (!server.StartsWith("http"))
                    server = "http://" + server;
                return server;
            }
        }
        public static string WsSistemi_User
        {
            get
            {
                return LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_WsSistemi_User);
            }
        }

        public static string WsSistemi_KeySha256
        {
            get
            {
                return LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_WsSistemi_KeySha256);
            }
        }

        public static string WsSistemi_AppId
        {
            get
            {
                string valore = LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_WsSistemi_AppId);

                if (valore == "")
                {
                    if (ModalitaIntegrazioneEsolver == eTipoScambioDatiEsolver.InCloud)
                        valore = "DA8A942A-4B97-455D-819B-6D4AEECD6C12";
                    else
                        valore = "SISTEMI";
                }
                return valore;
            }
        }

        public static string WsSistemi_GruppoForzato
        {
            get
            {
                return LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_XbcGruppoForzato);
            }
        }

        public static List<int> Esportazione_MacchineDisabilitateAvanzamenti
        {
            get
            {
                return Zero5.Util.StringConverters.StringToIntList(LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_MacchineDisabilitateAvanzamenti));
            }
        }

        public static List<int> Esportazione_MacchineDisabilitateVariazioniOrdini
        {
            get
            {
                return Zero5.Util.StringConverters.StringToIntList(LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_MacchineDisabilitateVariazioniOrdini));
            }
        }

        public static string OnPremise_DatabaseDiScambio
        {
            get
            {
                return LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_OnPremise_Importazione_DatabaseDiScambio);
            }
        }

        public static string OnPremise_Esportazione_PathFileAvpCsv
        {
            get
            {
                return LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_OnPremise_Esportazione_PathFileAvpCsv);
            }
        }
        public static string WsSistemi_Esportazione_Test_CodiceEsternoFaseSingola
        {
            get
            {
                return LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_WsSistemi_Esportazione_TestVariazioniOrdini_CodiceEsternoFaseSingola);
            }
        }

        public static bool Comune_Importazione_AbilitaProgrammazioneAutomatica
        {
            get
            {
                return LoadIntFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Importazione_AbilitaProgrammazioneAutomatica) == 1;
            }
        }

        public static bool Comune_Importazione_ForzaChiusuraOrdini
        {
            get
            {
                return LoadIntFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Importazione_ForzaChiusuraOrdini) == 1;
            }
        }

        public static bool Comune_Importazione_ImportaGiacenzeIniziali
        {
            get
            {
                return LoadIntFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Importazione_ImportaGiacenzeIniziali) == 1;
            }
            set
            {
                Zero5.Data.Layer.Opzioni.helper.SaveBoolValue(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Importazione_ImportaGiacenzeIniziali, value);
                RicaricaConfigurazioni();
            }
        }

        public static bool Comune_Importazione_ImportaAnagraficaArticoli
        {
            get
            {
                return LoadIntFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Importazione_ImportaAnagraficaArticoli) == 1;
            }
        }
        public static bool Comune_Importazione_ImportaGiacenzeEMovimenti
        {
            get
            {
                return LoadIntFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Importazione_ImportaGiacenzeEMovimenti) == 1;
            }
        }

        public static bool WsSistemi_Esportazione_Test_UtilizzaFaseSingola
        {
            get
            {
                return LoadIntFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_WsSistemi_Esportazione_TestVariazioniOrdini_UtilizzaFaseSingola) == 1;
            }
        }

        public static bool Esportazione_ForzaEsclusioneVariante
        {
            get
            {
                return LoadIntFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_ForzaEsclusioneVariante) == 1;
            }
        }

        public static bool Esportazione_VersamentiForzaLottoFittizi
        {
            get
            {
                return LoadIntFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_VersamentiForzaLottoFittizio) == 1;
            }
        }

        public static bool Comune_Importazione_ProgrammazioneDisabilitaRicalcoloInizioPrevisto
        {
            get
            {
                return LoadIntFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Importazione_ProgrammazioneDisabilitaRicalcoloInizioPrevisto) == 1;
            }
        }

        public static string Esportazione_VersamentiLottoFittizio
        {
            get
            {
                return LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_VersamentiLottoFittizio);
            }
        }

        public static bool Esportazione_MovimentiEsportaSoloConsumi
        {
            get
            {
                return LoadIntFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_MovimentiEsportaSoloConsumi) == 1;
            }
        }

        private static eVersioneFormatoEsportazione _versioneFormatoEsportazione = eVersioneFormatoEsportazione.Sconosciuto;
        public static eVersioneFormatoEsportazione Esportazione_Formato
        {
            get
            {
                if (_versioneFormatoEsportazione == eVersioneFormatoEsportazione.Sconosciuto)
                {
                    int numeroVersioneDaOpzioni = LoadIntFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_VersioneFormatoEsportazione);

                    if (numeroVersioneDaOpzioni == 0)
                    {
                        numeroVersioneDaOpzioni = 1;
                        Zero5.Data.Layer.Opzioni.helper.SaveIntValue(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_VersioneFormatoEsportazione, 1);
                    }

                    if (Enum.IsDefined(typeof(eVersioneFormatoEsportazione), numeroVersioneDaOpzioni))
                        _versioneFormatoEsportazione = (eVersioneFormatoEsportazione)numeroVersioneDaOpzioni;
                }

                return _versioneFormatoEsportazione;
            }
        }

        public static int WsSistemi_Esportazione_ModelloAVP
        {
            get
            {
                int value = LoadIntFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_WsSistemi_Esportazione_AVP_Modello);
                if (value <= 0)
                    value = 1;
                return value;
            }
        }

        public static DateTime Esportazione_DataInizio
        {
            get
            {
                DateTime result = DateTime.Now.Date;
                string tmp = LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_DataInizio);
                DateTime.TryParse(tmp, out result);
                return result;
            }
        }



        public static DateTime Comune_Importazione_DataUltimoMovimentoImportato
        {
            get
            {
                DateTime result = DateTime.Now.Date;
                string tmp = LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Importazione_DataUltimoMovimentoImportato);
                DateTime.TryParse(tmp, out result);
                return result;
            }
            set
            {
                Zero5.Data.Layer.Opzioni.helper.SaveStringValue(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Importazione_DataUltimoMovimentoImportato, value.ToString());
                RicaricaConfigurazioni();
            }
        }

        public static DateTime Comune_Esportazione_VariazioniOrdiniFasi_DataUltimoCambioStatoEsportato
        {
            get
            {
                DateTime result = DateTime.Now.Date;
                string tmp = LoadStringFromOpzioni(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_VariazioniOrdiniFasi_DataUltimoCambioStatoEsportato);
                DateTime.TryParse(tmp, out result);
                return result;
            }
            set
            {
                Zero5.Data.Layer.Opzioni.helper.SaveStringValue(Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_VariazioniOrdiniFasi_DataUltimoCambioStatoEsportato, value.ToString());
                RicaricaConfigurazioni();
            }
        }


        private static string _wsToken = "";
        public static string WsToken
        {
            get
            {
                if (_wsToken == "")
                {
                    _wsToken = Common.GetAuthCodeEsolver();
                }
                return _wsToken;
            }
        }

        public static void RicaricaConfigurazioni()
        {
            internalIntOptionDictionary.Clear();
            internalStringOptionDictionary.Clear();
        }


    }

    public enum eTipoScambioDatiEsolver
    {
        OnPremise = 0,
        InCloud = 1,
    }

}
