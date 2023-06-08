using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Esporta
{
    class FileConfigurazione : Zero5.Util.FileParametri
    {
        public FileConfigurazione() : base(Zero5.IO.Util.LocalPathFile("Esporta.cfg"))
        {
        }

        public void MigraParametriDaFileAOpzioni()
        {
            if (!System.IO.File.Exists(Zero5.IO.Util.LocalPathFile("Esporta.cfg")))
                return;

            Zero5.Util.Log.WriteLog("Migrazione configurazioni da file a opzioni...");

            {
                string AvpCsvPath = GetParametro("AvpCsvPath");
                if (AvpCsvPath == "")
                    AvpCsvPath = Zero5.IO.Util.LocalPathFile("EsolverExport", "");
                if (!System.IO.Directory.Exists(AvpCsvPath))
                    System.IO.Directory.CreateDirectory(AvpCsvPath);

                Zero5.Data.Layer.Opzioni.helper.SaveStringValue(Zero5.Data.Layer.Opzioni.enumOpzioniID.Esolver_ScambioDati_OnPremise_Esportazione_PathFileAvpCsv, AvpCsvPath);
            }

            {
                string configOnFile = GetParametro("DataInizioEsportazione");
                if (configOnFile != "")
                    Zero5.Data.Layer.Opzioni.helper.SaveStringValue(Zero5.Data.Layer.Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_DataInizio, configOnFile);
            }

            {
                string configOnFile = GetParametro("UltimoCambioStato");
                if (configOnFile != "")
                    Zero5.Data.Layer.Opzioni.helper.SaveStringValue(Zero5.Data.Layer.Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_VariazioniOrdiniFasi_DataUltimoCambioStatoEsportato, configOnFile);
            }

            {
                string configOnFile = GetParametro("MacchineDisabilitateEsportazioneAvanzamenti");
                if (configOnFile != "")
                    Zero5.Data.Layer.Opzioni.helper.SaveStringValue(Zero5.Data.Layer.Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_MacchineDisabilitateAvanzamenti, configOnFile);
            }

            {
                string configOnFile = GetParametro("EsolverWS_MacchineDisabilitateEsportazioneVariazioneOrdini");
                if (configOnFile != "")
                    Zero5.Data.Layer.Opzioni.helper.SaveStringValue(Zero5.Data.Layer.Opzioni.enumOpzioniID.Esolver_ScambioDati_Comune_Esportazione_MacchineDisabilitateVariazioniOrdini, configOnFile);
            }

            {
                string configOnFile = GetParametro("EsolverWS_CodiceEsternoFaseSingolaTest");
                if (configOnFile != "")
                    Zero5.Data.Layer.Opzioni.helper.SaveStringValue(Zero5.Data.Layer.Opzioni.enumOpzioniID.Esolver_ScambioDati_WsSistemi_Esportazione_TestVariazioniOrdini_CodiceEsternoFaseSingola, configOnFile);
            }

            {
                string configOnFile = GetParametro("EsolverWS_Server");
                if (configOnFile != "")
                    Zero5.Data.Layer.Opzioni.helper.SaveStringValue(Zero5.Data.Layer.Opzioni.enumOpzioniID.Esolver_ScambioDati_WsSistemi_Server, configOnFile);
            }

            {
                string configOnFile = GetParametro("EsolverWS_User");
                if (configOnFile != "")
                    Zero5.Data.Layer.Opzioni.helper.SaveStringValue(Zero5.Data.Layer.Opzioni.enumOpzioniID.Esolver_ScambioDati_WsSistemi_User, configOnFile);
            }

            {
                string configOnFile = GetParametro("EsolverWS_KeySha256");
                if (configOnFile != "")
                    Zero5.Data.Layer.Opzioni.helper.SaveStringValue(Zero5.Data.Layer.Opzioni.enumOpzioniID.Esolver_ScambioDati_WsSistemi_KeySha256, configOnFile);
            }

            Configurazioni.RicaricaConfigurazioni();
            Zero5.Util.Log.WriteLog("Migrazione configurazioni da file a opzioni conclusa.");

            System.IO.File.Delete(Zero5.IO.Util.LocalPathFile("Esporta.cfg"));
            Zero5.Util.Log.WriteLog("Eliminato il file Esporta.cfg");
        }

        //public string EsolverWS_Server
        //{
        //    get
        //    {
        //        return GetParametro("EsolverWS_Server", "localhost:9050");
        //    }
        //    set
        //    {
        //        SetParametro("EsolverWS_Server", value);
        //    }
        //}

        //public string EsolverWS_User
        //{
        //    get
        //    {
        //        return GetParametro("EsolverWS_User", "PHASE");
        //    }
        //    set
        //    {
        //        SetParametro("EsolverWS_User", value);
        //    }
        //}

        //public string EsolverWS_KeySha256
        //{
        //    get
        //    {
        //        return GetParametro("EsolverWS_KeySha256", "02195b8e989603e4dfb45352b902e8dc34bff40bb228f3c26e56a6ddebdb85ad");
        //    }
        //    set
        //    {
        //        SetParametro("EsolverWS_KeySha256", value);
        //    }
        //}

    }
}
