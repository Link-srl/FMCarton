using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Shared
{
    class Common
    {
        private static int _tipoAttVersamentoDaGestionale = 0;
        public static int TipoAttivitaVersamentoDaGestionale
        {
            get
            {
                if (_tipoAttVersamentoDaGestionale == 0)
                {
                    Zero5.Data.Layer.TipoAttivita att = new Zero5.Data.Layer.TipoAttivita();
                    att.Load(att.Fields.Codice == "VERP", att.Fields.Atomica == Zero5.Data.Layer.TipoAttivita.eZeroFalseOneTrue.Yes_True);
                    if (att.EOF)
                    {
                        att.AddNewAndNewID();
                        att.Codice = "VERP";
                        att.Atomica = Zero5.Data.Layer.TipoAttivita.eZeroFalseOneTrue.Yes_True;
                        att.CodiceEsterno = "VERP";
                        att.Descrizione = "Versamento ERP";
                        att.InizioPossibile = Zero5.Data.Layer.TipoAttivita.eZeroFalseOneTrue.No_False;
                        att.FinePossibile = Zero5.Data.Layer.TipoAttivita.eZeroFalseOneTrue.No_False;
                        att.PerUomo = Zero5.Data.Layer.TipoAttivita.eZeroFalseOneTrue.Yes_True;
                        att.PerMacchina = Zero5.Data.Layer.TipoAttivita.eZeroFalseOneTrue.Yes_True;
                        att.Save();
                    }
                    _tipoAttVersamentoDaGestionale = att.IDTipoAttivita;
                }
                return _tipoAttVersamentoDaGestionale;
            }
        }

        private static int _causaleAttVersamentoDaGestionale = 0;
        public static int CausaleVersamentoDaGestionale
        {
            get
            {
                if (_causaleAttVersamentoDaGestionale == 0)
                {
                    Zero5.Data.Layer.CausaliAttivita caus = new Zero5.Data.Layer.CausaliAttivita();
                    caus.Load(caus.Fields.CodiceCausale == "VERP_I");
                    if (caus.EOF)
                    {
                        caus.AddNewAndNewID();
                        caus.CodiceCausale = "VERP_I";
                        caus.CodiceEsterno = "VERP_I";
                        caus.IDTipoAttivita = TipoAttivitaVersamentoDaGestionale;
                        caus.DescrizioneCausale = "Versamento ERP Importazione";
                        caus.QuantitaSuFase = Zero5.Data.Layer.CausaliAttivita.enumQuantitaSuFase.ContatiLavorazione;
                        caus.Save();
                    }
                    _causaleAttVersamentoDaGestionale = caus.IDCausaleAttivita;
                }
                return _causaleAttVersamentoDaGestionale;
            }
        }

        private static List<int> _lstTipiOrdineGenerico = new List<int>();
        public static List<int> ListaTipiOrdineGenerico
        {
            get
            {
                if (_lstTipiOrdineGenerico.Count == 0)
                {
                    Zero5.Data.Layer.TipiOrdine tipiOrdineClasseGenerico = new Zero5.Data.Layer.TipiOrdine();
                    tipiOrdineClasseGenerico.Load(tipiOrdineClasseGenerico.Fields.ClasseOrdine == Zero5.Data.Layer.TipiOrdine.eClasseOrdine.Generico);
                    _lstTipiOrdineGenerico = tipiOrdineClasseGenerico.GetIntListFromPrimaryKey();

                    if (!ListaTipiOrdineGenerico.Contains(0))
                        _lstTipiOrdineGenerico.Add(0);
                }
                return _lstTipiOrdineGenerico;
            }
        }


        private static int _IDCausaleMovimentoAddebito = 0;
        /// <summary>
        ///  IDCausaleMovimentoAddebito - allineata con analoga configurazione TTS
        /// </summary>
        public static int IDCausaleMovimentoAddebito
        {
            get
            {
                if (_IDCausaleMovimentoAddebito == 0)
                {
                    Zero5.Data.Layer.CausaliMovimento causaliMov = new Zero5.Data.Layer.CausaliMovimento();
                    causaliMov.Load(causaliMov.Fields.Descrizione == "Addebito Produzione");
                    if (causaliMov.EOF)
                    {
                        causaliMov.AddNewAndNewID();
                        causaliMov.Descrizione = "Addebito Produzione";
                        causaliMov.Colore = System.Drawing.Color.PaleGreen;
                        causaliMov.Save();
                    }

                    _IDCausaleMovimentoAddebito = causaliMov.IDCausaleMovimento;
                }
                return _IDCausaleMovimentoAddebito;
            }
        }

        private static int _IDCausaleMovimentoReso = 0;
        /// <summary>
        /// IDCausaleMovimentoReso - allineata con analoga configurazione TTS
        /// </summary>
        public static int IDCausaleMovimentoReso
        {
            get
            {
                if (_IDCausaleMovimentoReso == 0)
                {
                    Zero5.Data.Layer.CausaliMovimento causaliMov = new Zero5.Data.Layer.CausaliMovimento();
                    causaliMov.Load(causaliMov.Fields.Descrizione == "Reso Produzione");
                    if (causaliMov.EOF)
                    {
                        causaliMov.AddNewAndNewID();
                        causaliMov.Descrizione = "Reso Produzione";
                        causaliMov.Colore = System.Drawing.Color.LightSalmon;
                        causaliMov.Save();
                    }

                    _IDCausaleMovimentoReso = causaliMov.IDCausaleMovimento;
                }
                return _IDCausaleMovimentoReso;
            }
        }

        private static HttpClient _internalClient = null;
        public static HttpClient RestClient
        {
            get
            {
                if (_internalClient == null)
                {
                    _internalClient = new HttpClient();
                    _internalClient.BaseAddress = new Uri("http://localhost:9050/");
                    _internalClient.DefaultRequestHeaders.Accept.Clear();
                    _internalClient.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                }
                return _internalClient;
            }
        }

        private static string _eSolverKey = "";
        public static string GetAuthCodeEsolver()
        {
            if (_eSolverKey == "")
            {
                _eSolverKey = GetAuthCodeEsolverAsync().GetAwaiter().GetResult();
                if (_eSolverKey.Length > 2)
                    _eSolverKey = _eSolverKey.Substring(1, _eSolverKey.Length - 2);
                Zero5.Util.Log.WriteLog("Key " + _eSolverKey);
            }
            return _eSolverKey;
        }

        private static async Task<string> GetAuthCodeEsolverAsync()
        {
            string server = Configurazioni.WsSistemi_Server;


            HttpContent content = null;
            string payload = "";
            try
            {
                payload = JsonConvert.SerializeObject(
                new
                {
                    User = Configurazioni.WsSistemi_User,
                    Key = Configurazioni.WsSistemi_KeySha256
                }
            );

                content = new StringContent(payload, Encoding.UTF8, "application/json");


                HttpResponseMessage response = await RestClient.PostAsync($"{server}/auth/App-{Configurazioni.WsSistemi_AppId}/Token", content);
                response.EnsureSuccessStatusCode();

                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Zero5.Util.Log.WriteLog("------EX---------------");

                Zero5.Util.Log.WriteLog("url: " + $"{server}/auth/App-{Configurazioni.WsSistemi_AppId}/Token");
                Zero5.Util.Log.WriteLog("payload: " + payload);
                Zero5.Util.Log.WriteLog("Errore durante richiesta codice autcode " + ex.ToString());
                Zero5.Util.Log.WriteLog("Stack: " + ex.StackTrace);

                Zero5.Util.Log.WriteLog("------END EX---------------");


                throw ex;
            }
        }

        public static bool POSTAvanzamentiAVP(string contenuto)
        {
            return POSTAvanzamentiAVPAsync(contenuto).GetAwaiter().GetResult();
        }

        private static async Task<bool> POSTAvanzamentiAVPAsync(string contenuto)
        {
            string uri = $"{Configurazioni.WsSistemi_Server}/api/App-{Configurazioni.WsSistemi_AppId}/ImportaDaFile";

            string payload = JsonConvert.SerializeObject(
                new POSTStruct_AVP()
                {
                    Parametri = new POSTStruct_AVP_Parametri()
                    {
                        Tipologia = "AVP",
                        Modello = Configurazioni.WsSistemi_Esportazione_ModelloAVP,
                        DaConfermare = 0,
                        Simulazione = 0,
                        File = new POSTStruct_AVP_Parametri_File()
                        {
                            BUFFER = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(contenuto))
                        }
                    },
                    Risposta = new POSTStruct_AVP_Risposta()
                }
             );
            try
            {
                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                content.Headers.Add("X-BC-Authorization", Configurazioni.WsToken);

                string CodiceGruppo = Configurazioni.WsSistemi_GruppoForzato;
                if (CodiceGruppo != "")
                    content.Headers.Add("X-BC-Gruppo", CodiceGruppo);


                HttpResponseMessage httpResp = await RestClient.PostAsync(uri, content);
                httpResp.EnsureSuccessStatusCode();

                string rawResponse = httpResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                //Stream responseStream = await httpResp.Content.ReadAsStreamAsync();
                //JsonTextReader reader = new JsonTextReader(new StreamReader(responseStream));

                //JsonSerializer serializer = new JsonSerializer();


                POSTStruct_AVP response = JsonConvert.DeserializeObject<POSTStruct_AVP>(httpResp.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                //POSTStructureVarODP response = serializer.Deserialize<POSTStructureVarODP>(reader);
                if (response.Risposta.Esito == 1)
                    return true;
                else
                {
                    Zero5.Util.Log.WriteLog("POSTAvanzamentiAVPAsync uri: " + uri + ". Payload: " + payload);
                    Zero5.Util.Log.WriteLog("POSTAvanzamentiAVPAsync RESP: " + response.Risposta.Errori);
                }
            }
            catch (Exception ex)
            {
                Zero5.Util.Log.WriteLog("POSTAvanzamentiAVPAsync EX: " + ex.Message);
                Zero5.Util.Log.WriteLog("POSTAvanzamentiAVPAsync EX: " + ex.InnerException);
                Zero5.Util.Log.WriteLog("POSTAvanzamentiAVPAsync uri: " + uri + ". Payload: " + payload);

            }

            return false;
        }


        public static bool POSTVariazioneOrdineEsolver(string server, string user, string key, string idOdpFase, DateTime inizioPrevisto, DateTime finePrevista, eStatoRiga_eSOLVER statoEsolver, string risorsaMacchinaPrevista)
        {
            return POSTVariazioneOrdineEsolverAsync(server, user, key, idOdpFase, inizioPrevisto, finePrevista, statoEsolver, risorsaMacchinaPrevista).GetAwaiter().GetResult();
        }

        private static async Task<bool> POSTVariazioneOrdineEsolverAsync(string server, string user, string key, string idOdpFase, DateTime inizioPrevisto, DateTime finePrevista, eStatoRiga_eSOLVER statoEsolver, string risorsaMacchinaPrevista)
        {
            string strInizioPrevisto = "";
            string strFinePrevista = "";

            if (inizioPrevisto != DateTime.MinValue && finePrevista != DateTime.MinValue)
            {
                strInizioPrevisto = inizioPrevisto.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                strFinePrevista = finePrevista.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }

            string payload = JsonConvert.SerializeObject(
                new POSTStruct_AggiornaPianificazioneOdP()
                {
                    Parametri = new POSTStruct_AggiornaPianificazioneOdP_Parametri
                    {
                        IdOdpFase = idOdpFase,
                        DataInizioSched = strInizioPrevisto,
                        DataFineSched = strFinePrevista,
                        Stato = (int)statoEsolver,
                        CodRisorsaProd = risorsaMacchinaPrevista
                    }
                    ,
                    Risposta = new POSTStruct_AggiornaPianificazioneOdP_Risposta()
                }
             );
            try
            {
                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                content.Headers.Add("X-BC-Authorization", Configurazioni.WsToken);

                string CodiceGruppo = Configurazioni.WsSistemi_GruppoForzato;
                if (CodiceGruppo != "")
                    content.Headers.Add("X-BC-Gruppo", CodiceGruppo);

                HttpResponseMessage httpResp = await RestClient.PostAsync($"{server}/api/App-SISTEMI/AggiornaPianificazioneOdP", content);
                httpResp.EnsureSuccessStatusCode();

                string rawResponse = httpResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                //Stream responseStream = await httpResp.Content.ReadAsStreamAsync();
                //JsonTextReader reader = new JsonTextReader(new StreamReader(responseStream));

                //JsonSerializer serializer = new JsonSerializer();


                POSTStruct_AggiornaPianificazioneOdP response = JsonConvert.DeserializeObject<POSTStruct_AggiornaPianificazioneOdP>(httpResp.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                //POSTStructureVarODP response = serializer.Deserialize<POSTStructureVarODP>(reader);
                if (response.Risposta.Esito == 1)
                {
                    Zero5.Util.Log.WriteLog("Eseguito, guid " + response.Risposta.GuidOperazione);
                    return true;
                }
                else
                {
                    Zero5.Util.Log.WriteLog("POSTVariazioneOrdineEsolverAsynch REQ: " + payload);
                    Zero5.Util.Log.WriteLog("POSTVariazioneOrdineEsolverAsynch RESP: " + response.Risposta.Errori);
                }
            }
            catch (Exception ex)
            {
                Zero5.Util.Log.WriteLog("POSTVariazioneOrdineEsolverAsynch EX: " + ex.Message);
                Zero5.Util.Log.WriteLog("POSTVariazioneOrdineEsolverAsynch EX: " + ex.InnerException.Message);
                Zero5.Util.Log.WriteLog("POSTVariazioneOrdineEsolverAsynch REQ: " + payload);

            }

            return false;
        }

        public static bool LeggiVistaTramiteWS(Zero5.Data.MemoryDataObject dbObj)
        {
            string jsonData = LeggiDatiDaWSAsync(dbObj.TableName, "").GetAwaiter().GetResult();
            dynamic deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(jsonData);

            dbObj.LoadNone();

            List<string> dbObjFieldName = new List<string>();
            foreach (Zero5.Data.Filter.Field fld in dbObj.FieldsList.Items)
                if (!dbObjFieldName.Contains(fld.FieldName))
                    dbObjFieldName.Add(fld.FieldName);

            if (((IDictionary<String, object>)deserialized).ContainsKey("result"))
            {
                List<object> items = (List<object>)deserialized.result;
                Zero5.Util.Log.WriteLog("Read from ws:"
                    + items.Count + " items.");


                foreach (object item in items)
                {
                    IDictionary<string, object> row = (IDictionary<string, object>)item;

                    dbObj.AddNew();
                    foreach (string fields in dbObjFieldName)
                    {
                        try
                        {
                            if (row.ContainsKey("@" + fields))
                                dbObj.DataRow[fields] = row["@" + fields];
                            else if (row.ContainsKey(fields))
                                dbObj.DataRow[fields] = row[fields];

                        }
                        catch (Exception ex)
                        {
                            Zero5.Util.Log.WriteLog("Exception mapping field " + fields + " on " + dbObj.TableName + ": " + ex.Message);
                        }
                    }
                }
            }

            dbObj.MoveFirst();

            Zero5.Util.Log.WriteLog("Load from WS " + dbObj.TableName + ":" + dbObj.RowCount);
            return true;
        }

        private static async Task<string> LeggiDatiDaWSAsync(string nomeVista, string gruppo)
        {
            try
            {
                string server = Configurazioni.WsSistemi_Server;
                string user = Configurazioni.WsSistemi_User;
                string key = Configurazioni.WsSistemi_KeySha256;
                string appId = Configurazioni.WsSistemi_AppId;

                if (server == "" || user == "" || key == "")
                    throw new Exception("Opzioni eSOLVER scambio dati non configurate");

                HttpContent content = new StringContent("{}", Encoding.UTF8, "application/json");
                content.Headers.Add("X-BC-Authorization", Configurazioni.WsToken);

                if (Configurazioni.WsSistemi_GruppoForzato != "")
                    gruppo = Configurazioni.WsSistemi_GruppoForzato;
                if (gruppo != "")
                    content.Headers.Add("X-BC-Gruppo", gruppo);

                HttpResponseMessage httpResp = await RestClient.PostAsync($"{server}/report/App-{appId}/{nomeVista}", content);
                httpResp.EnsureSuccessStatusCode();

                string rawResponse = httpResp.Content.ReadAsStringAsync().GetAwaiter().GetResult();


                return rawResponse;

            }
            catch (Exception ex)
            {
                Zero5.Util.Log.WriteLog("LeggiDatiDaWSAsync EX: " + ex.Message);
                Zero5.Util.Log.WriteLog("LeggiDatiDaWSAsync REQ: {}");

            }

            return "";
        }

        public static eStatoRiga_eSOLVER PhaseStatoFaseProduzione_eSolverStatoRiga(Zero5.Data.Layer.FasiProduzione fp)
        {
            bool avanzata = fp.QtaBuonaTotale + fp.QtaScartoTotale > 0;
            return PhaseStatoFaseProduzione_eSolverStatoRiga(fp.Stato, fp.Programmata, avanzata);
        }

        public static eStatoRiga_eSOLVER PhaseStatoFaseProduzione_eSolverStatoRiga(Zero5.Data.Layer.FasiProduzione.enumFasiProduzioneStati stato, bool programmata, bool avanzata)
        {
            if (stato == Zero5.Data.Layer.FasiProduzione.enumFasiProduzioneStati.Inserita)
            {
                if (programmata)
                    return eStatoRiga_eSOLVER.Pianificato;
                else
                    return eStatoRiga_eSOLVER.DaPianificare;
            }

            if (stato == Zero5.Data.Layer.FasiProduzione.enumFasiProduzioneStati.Finita)
                return eStatoRiga_eSOLVER.Terminato;

            if (avanzata)
                return eStatoRiga_eSOLVER.InAvanzamento;

            return eStatoRiga_eSOLVER.Lanciato;
        }





        private static int _idTipoAttEsolver = 0;
        public static int TipoAttivitaBaseEsolver()
        {
            if (_idTipoAttEsolver == 0)
            {

                Zero5.Data.Layer.TipoAttivita tipoAttivita = new Zero5.Data.Layer.TipoAttivita();
                tipoAttivita.Load(tipoAttivita.Fields.Codice == "eSOLVER");
                if (tipoAttivita.EOF)
                {
                    tipoAttivita.AddNewAndNewID();
                    tipoAttivita.Codice = "eSOLVER";
                    tipoAttivita.CodiceEsterno = "eSOLVER";
                    tipoAttivita.Descrizione = "Causali eSOLVER da categorizzare";
                    tipoAttivita.Save();
                }
                _idTipoAttEsolver = tipoAttivita.IDTipoAttivita;
            }
            return _idTipoAttEsolver;
        }

        private static int _idCausaleMovimentoCaricoInizialeERP = 0;
        public static int IDCausaleMovimentoCaricoInizialeERP
        {
            get
            {
                if (_idCausaleMovimentoCaricoInizialeERP == 0)
                {
                    Zero5.Data.Layer.CausaliMovimento causale = new Zero5.Data.Layer.CausaliMovimento();
                    causale.Load(causale.Fields.CodiceEsterno == "ERP_CI");
                    if (causale.EOF)
                    {
                        causale.AddNewAndNewID();
                        causale.CodiceEsterno = "ERP_CI";
                        causale.Descrizione = "Carico Iniziale ERP";
                        causale.Save();
                    }
                    _idCausaleMovimentoCaricoInizialeERP = causale.IDCausaleMovimento;
                }
                return _idCausaleMovimentoCaricoInizialeERP;
            }
        }


        private static int _idCausaleMovimentoScaricoERP = 0;
        public static int IDCausaleMovimentoScaricoERP
        {
            get
            {
                if (_idCausaleMovimentoScaricoERP == 0)
                {
                    Zero5.Data.Layer.CausaliMovimento causale = new Zero5.Data.Layer.CausaliMovimento();
                    causale.Load(causale.Fields.CodiceEsterno == "ERP_S");
                    if (causale.EOF)
                    {
                        causale.AddNewAndNewID();
                        causale.CodiceEsterno = "ERP_S";
                        causale.Descrizione = "Scarico da ERP";
                        causale.Save();
                    }
                    _idCausaleMovimentoScaricoERP = causale.IDCausaleMovimento;
                }
                return _idCausaleMovimentoScaricoERP;
            }
        }


        private static int _idCausaleMovimentoCaricoERP = 0;
        public static int IDCausaleMovimentoCaricoERP
        {
            get
            {
                if (_idCausaleMovimentoCaricoERP == 0)
                {
                    Zero5.Data.Layer.CausaliMovimento causale = new Zero5.Data.Layer.CausaliMovimento();
                    causale.Load(causale.Fields.CodiceEsterno == "ERP_C");
                    if (causale.EOF)
                    {
                        causale.AddNewAndNewID();
                        causale.CodiceEsterno = "ERP_C";
                        causale.Descrizione = "Carico da ERP";
                        causale.Save();
                    }
                    _idCausaleMovimentoCaricoERP = causale.IDCausaleMovimento;
                }
                return _idCausaleMovimentoCaricoERP;
            }
        }

        public static void AllineaCampiDaOggettoDati(Zero5.Data.EditableDataObject destination, Zero5.Data.MemoryDataObject source, Dictionary<string, string> dicSourceByDestination)
        {
            foreach (KeyValuePair<string, string> kvp in dicSourceByDestination)
            {
                try
                {
                    string fldNameSource = kvp.Value;
                    string fldNameDestination = kvp.Key;

                    Zero5.Data.Filter.Field fldSource = source.FieldsList.ByFieldName(fldNameSource);
                    Zero5.Data.Filter.Field fldDestination = destination.FieldsList.ByFieldName(fldNameDestination);

                    if (fldSource == null)
                        Zero5.Util.Log.WriteLog($"WARN: campo {fldNameSource} mancante su {source.TableName}");

                    if (fldDestination == null)
                        Zero5.Util.Log.WriteLog($"WARN: campo {fldNameDestination} mancante su {destination.TableName}");

                    if (fldSource != null &&
                        fldDestination != null)
                    {


                        string sDestinationValue = "";
                        object destinationValue = destination.Get_FieldValue(fldDestination);
                        if (destinationValue != null)
                        {
                            sDestinationValue = destinationValue.ToString();
                            if (destinationValue.GetType().IsEnum)
                                sDestinationValue = ((int)destinationValue).ToString();
                        }

                        string sourceValue = source.Get_FieldValue(fldSource).ToString();

                        if (sDestinationValue != sourceValue.ToString())
                        {
                            Zero5.Util.Log.WriteLog(destination.TableName + "." + destination.PrimaryKeyField.FieldName + " " + destination.PrimaryKeyValue + "\t" + fldNameDestination + " changed (" + source.TableName + "." + fldNameSource + ").\t" + destinationValue.ToString() + "->" + sourceValue.ToString());
                            destination.Set_FieldValue(fldDestination, source.Get_FieldValue(fldSource));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Zero5.Util.Log.WriteLog("Ex Allineamento campi da oggetto dati tra campo destination: " + kvp.Key.ToString() + " e campo sorgente: " + kvp.Value.ToString() + " ex:" + ex.Message);
                }
            }
        }

        public static void AllineaCampiDaValori(Zero5.Data.EditableDataObject destination, Dictionary<Zero5.Data.Filter.Field, object> dicValoreByField)
        {

            foreach (KeyValuePair<Zero5.Data.Filter.Field, object> kvp in dicValoreByField)
            {
                try
                {
                    Zero5.Data.Filter.Field fldDestination = kvp.Key;
                    object sourceValue = kvp.Value;

                    if (destination.FieldsList.ByFieldName(fldDestination.FieldName) != null &&
                        sourceValue != null)
                    {
                        object destinationValue = destination.Get_FieldValue(fldDestination);
                        if (destinationValue.GetType().IsEnum)
                            destinationValue = (int)destinationValue;
                        if (sourceValue.GetType().IsEnum)
                            sourceValue = (int)sourceValue;

                        //TODO: attenzione con enum il toString risulta sempre una variazione e logga per nulla
                        if (destinationValue.ToString() != sourceValue.ToString())
                        {
                            Zero5.Util.Log.WriteLog("Campo " + fldDestination.TableName + "." + fldDestination.FieldName + " valore differente. " + destinationValue.ToString() + "->" + sourceValue.ToString());
                            destination.Set_FieldValue(fldDestination, sourceValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Zero5.Util.Log.WriteLog("Ex Allineamento campi da oggetto dati tra campo destination: " + kvp.Key.FieldName + " e valore: " + kvp.Value + " ex:" + ex.Message);
                }
            }
        }

        public static bool VerificaNecessitaAggiornamento(Zero5.Data.EditableDataObject destination, Zero5.Data.MemoryDataObject source, Dictionary<string, string> dicSourceByDestination)
        {
            foreach (KeyValuePair<string, string> kvp in dicSourceByDestination)
            {
                Zero5.Data.Filter.Field fldSource = source.FieldsList.ByFieldName(kvp.Value);
                Zero5.Data.Filter.Field fldDestination = destination.FieldsList.ByFieldName(kvp.Key);

                if (destination.FieldsList.ByFieldName(fldDestination.FieldName) != null &&
                    source.FieldsList.ByFieldName(fldSource.FieldName) != null)
                {
                    object destinationValue = destination.Get_FieldValue(fldDestination);
                    object sourceValue = source.Get_FieldValue(fldSource);

                    if (destinationValue.ToString() != sourceValue.ToString())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Dictionary<string, string> GetMappingDictionaryFromJson(string nomeFileConfigurazione, Dictionary<string, string> defaultValue)
        {
            string directory = Zero5.IO.Util.LocalPath() + System.IO.Path.DirectorySeparatorChar + "eSOLVERStandardImportConfig";
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            JavaScriptSerializer js = new JavaScriptSerializer();

            string pathConfigurazione = Zero5.IO.Util.LocalPathFile("eSOLVERStandardImportConfig", nomeFileConfigurazione + ".json");

            if (!System.IO.File.Exists(pathConfigurazione))
                System.IO.File.WriteAllText(pathConfigurazione, js.Serialize(defaultValue));

            Dictionary<string, string> dicMapping = new Dictionary<string, string>();
            {
                string content = "";
                if (System.IO.File.Exists(pathConfigurazione))
                    content = System.IO.File.ReadAllText(pathConfigurazione);

                try
                {
                    if (content != "")
                        dicMapping = js.Deserialize<Dictionary<string, string>>(content);
                }
                catch (Exception ex)
                {
                    Zero5.Util.Log.WriteLog("ERR: Configurazione file " + pathConfigurazione + " compromessa. " + ex.Message);
                }
            }

            return dicMapping;
        }


        public static double GetMinutiFromTempoESOLVER(double tempo, eUMTempo_eSolver um)
        {
            if (um == eUMTempo_eSolver.Ore)
                return tempo * 60;
            if (um == eUMTempo_eSolver.Minuti)
                return tempo;
            if (um == eUMTempo_eSolver.Secondi)
                return tempo / 60;
            return 0;
        }

        public static string GetCodiceArticoloFromEsolver(string codiceArticoloERP, string varianteERP)
        {
            string codArticolo = codiceArticoloERP;
            if (!string.IsNullOrEmpty(varianteERP))
                codArticolo += "_" + varianteERP;
            return codArticolo;
        }

        public static string GetCodiceEsternoArticoloFromEsolver(string codiceArticoloERP, string varianteERP)
        {
            return codiceArticoloERP;
        }

        public static string GetCodiceEsternoOrdineFromCodiceEsternoFase(string codiceEsternoFase)
        {
            if (codiceEsternoFase.Length < 13)
            {
                Zero5.Util.Log.WriteLog($"Impossibile ricavare il codice esterno ordine da codice esterno fase per la fase {codiceEsternoFase}");
                return "";
            }

            string sidDoc = codiceEsternoFase.Substring(0, 9);
            string sidRiga = codiceEsternoFase.Substring(9, 4);
            int idDoc = 0;
            int idRiga = 0;
            int.TryParse(sidDoc, out idDoc);
            int.TryParse(sidRiga, out idRiga);

            return idDoc.ToString() + ";" + idRiga.ToString();

        }

    }

    public class POSTStruct_AggiornaPianificazioneOdP
    {
        public POSTStruct_AggiornaPianificazioneOdP_Parametri Parametri { get; set; }
        public POSTStruct_AggiornaPianificazioneOdP_Risposta Risposta { get; set; }
    }

    public class POSTStruct_AggiornaPianificazioneOdP_Parametri
    {
        public string IdOdpFase { get; set; }
        public string DataInizioSched { get; set; }
        public string DataFineSched { get; set; }
        public int Stato { get; set; }
        public string CodRisorsaProd { get; set; }
    }

    public class POSTStruct_AggiornaPianificazioneOdP_Risposta
    {
        public int Esito { get; set; }
        public string GuidOperazione { get; set; }
        public string Errori { get; set; }
    }

    public class POSTStruct_AVP
    {
        public POSTStruct_AVP_Parametri Parametri { get; set; }
        public POSTStruct_AVP_Risposta Risposta { get; set; }
    }

    public class POSTStruct_AVP_Parametri
    {
        public string Tipologia { get; set; }
        public int Modello { get; set; }
        public int DaConfermare { get; set; }
        public int Simulazione { get; set; }
        public POSTStruct_AVP_Parametri_File File { get; set; }
    }

    public class POSTStruct_AVP_Parametri_File
    {
        public string BUFFER { get; set; }
    }

    public class POSTStruct_AVP_Risposta
    {
        public int Esito { get; set; }
        public string GuidOperazione { get; set; }
        public string[] Errori { get; set; }
    }


    public enum eStatoRiga_eSOLVER
    {
        DaPianificare = 0,
        Pianificato = 1,
        Lanciato = 2,
        InAvanzamento = 3,
        Terminato = 4,
    }

    public enum eTipoRecord_ERP
    {
        Riga = 0,
        Testa = 1
    }
    public enum eTipoMagazzinoCoinvolto_ERP
    {
        SC1 = 0,
        SC2 = 1,
        PF = 2
    }
    public enum eTipoOperazione_ERP
    {
        Sconosciuto = 0,
        AvanzamentoFase = 1,
        ConsuntivazioneOre = 2,
        ConsumoMateriali = 3,
        Addebito = 4,
        Reso = 5,
        FlagSaldoFase = 99
    }

    public enum eLivelloEsportatoMovimenti
    {
        NonEsportato = 0,
        EsportatoAddebitoReso = 1,
        EsportatoConsumi = 2
    }


    public enum eStatoFase_eSOLVER
    {
        DaPianificare = 0,
        Pianificato = 1,
        Lanciato = 2,
        InAvanzamento = 3,
        Finito = 4
    }

    public enum eTipoMovimento_eSOLVER
    {
        Carico = 0,//TODO: verificare con Andrea
        Scarico = 1,//TODO: verificare con Andrea
    }

    public enum eUMTempo_eSolver
    {
        Ore = 1,
        Minuti = 2,
        Secondi = 3,
    }

    public enum eVersioneFormatoEsportazione
    {
        Sconosciuto = -1,
        V01_14Campi = 1,
        V02_17Campi = 2,
        V03_20Campi = 3,
    }

}
