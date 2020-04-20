/*  CTRADER GURU --> Template 1.0.4

Changelog (Armando Brecciaroli):
v.1.0.5 (April 20, 2020) Add ON/OFF alert

    Homepage    : https://ctrader.guru/
    Telegram    : https://t.me/ctraderguru
    Twitter     : https://twitter.com/cTraderGURU/
    Facebook    : https://www.facebook.com/ctrader.guru/
    YouTube     : https://www.youtube.com/channel/UCKkgbw09Fifj65W5t5lHeCQ
    GitHub      : https://github.com/cTraderGURU/
    TOS         : https://ctrader.guru/termini-del-servizio/

*/

using System;
using System.IO;
using cAlgo.API;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

// --> Microsoft Visual Studio 2017 --> Strumenti --> Gestione pacchetti NuGet --> Gestisci pacchetti NuGet per la soluzione... --> Installa
using Newtonsoft.Json;

namespace cAlgo
{

    // --> AccessRights = AccessRights.FullAccess se si vuole controllare gli aggiornamenti
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class CustomCandle : Indicator
    {

        #region Enums & Class

        public enum CandleMode
        {
            HighLow,
            OpenClose
        }

        #endregion

        #region Identity

        /// <summary>
        /// ID prodotto, identificativo, viene fornito da ctrader.guru, 74909 è il riferimento del template in uso
        /// </summary>
        public const int ID = 76497;

        /// <summary>
        /// Nome del prodotto, identificativo, da modificare con il nome della propria creazione
        /// </summary>
        public const string NAME = "Custom Candle";

        /// <summary>
        /// La versione del prodotto, progressivo, utilie per controllare gli aggiornamenti se viene reso disponibile sul sito ctrader.guru
        /// </summary>
        public const string VERSION = "1.0.1";

        #endregion

        #region Params

        /// <summary>
        /// Identità del prodotto nel contesto di ctrader.guru
        /// </summary>
        [Parameter(NAME + " " + VERSION, Group = "Identity", DefaultValue = "https://ctrader.guru/product/custom-candle/")]
        public string ProductInfo { get; set; }

        /// <summary>
        /// Il numero di giorni da visualizzare
        /// </summary>
        [Parameter("Candle TimeFrame", Group = "Params", DefaultValue = 8, Step = 1)]
        public TimeFrame CandleTimeFrame { get; set; }

/// ABRE <summary>
/// Scegliere se visualizzare l'alert oppure no
/// ABRE </summary>
[Parameter("View ALERT", Group = "Params", DefaultValue = true)]
public bool imp_ViewAlert { get; set; }

        /// <summary>
        /// Il numero di giorni da visualizzare
        /// </summary>
        [Parameter("Candle Mode", Group = "Params", DefaultValue = CandleMode.HighLow)]
        public CandleMode MyCandleMode { get; set; }

        /// <summary>
        /// Il numero di canele da visualizzare
        /// </summary>
        [Parameter("Candles To Show", Group = "Params", DefaultValue = 10, MinValue = 1, Step = 1)]
        public int CandleShow { get; set; }

        /// <summary>
        /// Il Box, lo stile del bordo
        /// </summary>
        [Parameter("Line Style Box", Group = "Styles", DefaultValue = LineStyle.Solid)]
        public LineStyle LineStyleBox { get; set; }

        /// <summary>
        /// Il Box, lo spessore del bordo
        /// </summary>
        [Parameter("Tickness", Group = "Styles", DefaultValue = 1, MaxValue = 5, MinValue = 1, Step = 1)]
        public int TicknessBox { get; set; }

        /// <summary>
        /// Il Box, il colore del massimo
        /// </summary>
        [Parameter("High/Open/Long Color", Group = "Styles", DefaultValue = "DodgerBlue")]
        public string ColorHigh { get; set; }

        /// <summary>
        /// Il Box, il colore del minimo
        /// </summary>
        [Parameter("Low/Close/Short Color", Group = "Styles", DefaultValue = "Red")]
        public string ColorLow { get; set; }

        /// <summary>
        /// Il Box, l'opacità
        /// </summary>
        [Parameter("Opacity", Group = "Styles", DefaultValue = 30, MinValue = 1, MaxValue = 100, Step = 1)]
        public int Opacity { get; set; }

        /// <summary>
        /// Il Box, il riempimento
        /// </summary>
        [Parameter("Boxed ?", Group = "Styles", DefaultValue = true)]
        public bool Boxed { get; set; }

        /// <summary>
        /// Il Box, il riempimento
        /// </summary>
        [Parameter("Fill Box ?", Group = "Styles", DefaultValue = true)]
        public bool FillBox { get; set; }

        #endregion

        #region Property



        #endregion

        #region Indicator Events

        /// <summary>
        /// Viene generato all'avvio dell'indicatore, si inizializza l'indicatore
        /// </summary>
        protected override void Initialize()
        {

            // --> Se il timeframe è superiore o uguale al corrente devo uscire
            if (TimeFrame >= CandleTimeFrame)
/// ABRE <summary>
/// Se abilitato appare l'alert
/// ABRE </summary>
if (imp_ViewAlert == true)

                Chart.DrawStaticText("Alert", string.Format("{0} : USE THIS INDICATOR ON TIMEFRAME LOWER {1}", NAME.ToUpper(), CandleTimeFrame.ToString().ToUpper()), VerticalAlignment.Center, HorizontalAlignment.Center, Color.Red);

            // --> Stampo nei log la versione corrente
            Print("{0} : {1}", NAME, VERSION);

            // --> Se viene settato l'ID effettua un controllo per verificare eventuali aggiornamenti
            _checkProductUpdate();

            // --> L'utente potrebbe aver inserito un colore errato
            if (Color.FromName(ColorHigh).ToArgb() == 0)
                ColorHigh = "DodgerBlue";

            if (Color.FromName(ColorLow).ToArgb() == 0)
                ColorLow = "Red";

        }

        /// <summary>
        /// Generato ad ogni tick, vengono effettuati i calcoli dell'indicatore
        /// </summary>
        /// <param name="index">L'indice della candela in elaborazione</param>
        public override void Calculate(int index)
        {


            // --> Non esiste ancora un metodo per rimuovere l'indicatore dal grafico, quindi ci limitiamo a uscire
            // --> Risparmio risorse controllando solo quando mi trovo sull'ultima candela, quella corrente
            // --> Devo avere in memoria abbastanza candele daily
            if (TimeFrame >= CandleTimeFrame)
                return;

            try
            {

                _drawLevelFromCustomBar();



            } catch (Exception exp)
            {

                Chart.DrawStaticText("Alert", string.Format("{0} : error, {1}", NAME, exp), VerticalAlignment.Center, HorizontalAlignment.Center, Color.Red);

            }


        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Parto dalle ultime candele personalizzate e le disegno ogni volta
        /// </summary>
        /// <param name="index"></param>
        private void _drawLevelFromCustomBar()
        {

            // --> Prelevo le candele daily
            Bars BarsCustom = MarketData.GetBars(CandleTimeFrame);

            int index = BarsCustom.Count - 1;

            // --> Potrei non avere un numero sufficiente di candele
            if (index < CandleShow || index < 1)
                return;

            // --> eseguo un ciclo aretroso per disegnare le ultime candele
            for (int i = 0; i < CandleShow; i++)
            {

                // --> Il numero di candele da visualizzare potrebbero essere troppe
                try
                {

                    // --> TimeSpan DiffTime = BarsCustom[index - i].OpenTime.Subtract(BarsCustom[(index - i) - 1].OpenTime); // <-- Strategia da valutare

                    DateTime thisCandle = BarsCustom[index - i].OpenTime;
                    DateTime nextCandle = (i == 0) ? thisCandle.AddMinutes(_getTimeFrameCandleInMinutes(CandleTimeFrame)) : BarsCustom[index - i + 1].OpenTime;

                    string rangeFlag = thisCandle.ToString();
                    string RangeColor = (BarsCustom[index - i].Close > BarsCustom[index - i].Open) ? ColorHigh : ColorLow;

                    switch (MyCandleMode)
                    {

                        case CandleMode.HighLow:

                            if (Boxed)
                            {

                                ChartRectangle MyBox = Chart.DrawRectangle("HighLow" + rangeFlag, thisCandle, BarsCustom[index - i].High, nextCandle, BarsCustom[index - i].Low, Color.FromArgb(Opacity, Color.FromName(RangeColor)), TicknessBox, LineStyleBox);

                                MyBox.IsFilled = FillBox;

                            }
                            else
                            {

                                Chart.DrawTrendLine("High" + rangeFlag, thisCandle, BarsCustom[index - i].High, nextCandle, BarsCustom[index - i].High, Color.FromName(ColorHigh), TicknessBox, LineStyleBox);
                                Chart.DrawTrendLine("Low" + rangeFlag, thisCandle, BarsCustom[index - i].Low, nextCandle, BarsCustom[index - i].Low, Color.FromName(ColorLow), TicknessBox, LineStyleBox);

                            }

                            break;

                        case CandleMode.OpenClose:

                            if (Boxed)
                            {

                                ChartRectangle MyBox = Chart.DrawRectangle("OpenClose" + rangeFlag, thisCandle, BarsCustom[index - i].Open, nextCandle, BarsCustom[index - i].Close, Color.FromArgb(Opacity, Color.FromName(RangeColor)), TicknessBox, LineStyleBox);

                                MyBox.IsFilled = FillBox;

                            }
                            else
                            {

                                Chart.DrawTrendLine("Open" + rangeFlag, thisCandle, BarsCustom[index - i].Open, nextCandle, BarsCustom[index - i].Open, Color.FromName(ColorHigh), TicknessBox, LineStyleBox);
                                Chart.DrawTrendLine("Close" + rangeFlag, thisCandle, BarsCustom[index - i].Close, nextCandle, BarsCustom[index - i].Close, Color.FromName(ColorLow), TicknessBox, LineStyleBox);

                            }

                            break;

                    }

                } catch
                {


                }

            }

        }

        private int _getTimeFrameCandleInMinutes(TimeFrame MyCandle)
        {

            if (MyCandle == TimeFrame.Daily)
                return 60 * 24;
            if (MyCandle == TimeFrame.Day2)
                return 60 * 24 * 2;
            if (MyCandle == TimeFrame.Day3)
                return 60 * 24 * 3;
            if (MyCandle == TimeFrame.Hour)
                return 60;
            if (MyCandle == TimeFrame.Hour12)
                return 60 * 12;
            if (MyCandle == TimeFrame.Hour2)
                return 60 * 2;
            if (MyCandle == TimeFrame.Hour3)
                return 60 * 3;
            if (MyCandle == TimeFrame.Hour4)
                return 60 * 4;
            if (MyCandle == TimeFrame.Hour6)
                return 60 * 6;
            if (MyCandle == TimeFrame.Hour8)
                return 60 * 8;
            if (MyCandle == TimeFrame.Minute)
                return 1;
            if (MyCandle == TimeFrame.Minute10)
                return 10;
            if (MyCandle == TimeFrame.Minute15)
                return 15;
            if (MyCandle == TimeFrame.Minute2)
                return 2;
            if (MyCandle == TimeFrame.Minute20)
                return 20;
            if (MyCandle == TimeFrame.Minute3)
                return 3;
            if (MyCandle == TimeFrame.Minute30)
                return 30;
            if (MyCandle == TimeFrame.Minute4)
                return 4;
            if (MyCandle == TimeFrame.Minute45)
                return 45;
            if (MyCandle == TimeFrame.Minute5)
                return 5;
            if (MyCandle == TimeFrame.Minute6)
                return 6;
            if (MyCandle == TimeFrame.Minute7)
                return 7;
            if (MyCandle == TimeFrame.Minute8)
                return 8;
            if (MyCandle == TimeFrame.Minute9)
                return 9;
            if (MyCandle == TimeFrame.Monthly)
                return 60 * 24 * 30;
            if (MyCandle == TimeFrame.Weekly)
                return 60 * 24 * 7;

            return 0;

        }

        /// <summary>
        /// Effettua un controllo sul sito ctrader.guru per mezzo delle API per verificare la presenza di aggiornamenti, solo in realtime
        /// </summary>
        private void _checkProductUpdate()
        {

            // --> Controllo solo se solo in realtime, evito le chiamate in backtest
            if (RunningMode != RunningMode.RealTime)
                return;

            // --> Organizzo i dati per la richiesta degli aggiornamenti
            Guru.API.RequestProductInfo Request = new Guru.API.RequestProductInfo
            {

                MyProduct = new Guru.Product
                {

                    ID = ID,
                    Name = NAME,
                    Version = VERSION

                },
                AccountBroker = Account.BrokerName,
                AccountNumber = Account.Number

            };

            // --> Effettuo la richiesta
            Guru.API Response = new Guru.API(Request);

            // --> Controllo per prima cosa la presenza di errori di comunicazioni
            if (Response.ProductInfo.Exception != "")
            {

                Print("{0} Exception : {1}", NAME, Response.ProductInfo.Exception);

            }
            // --> Chiedo conferma della presenza di nuovi aggiornamenti
            else if (Response.HaveNewUpdate())
            {

                string updatemex = string.Format("{0} : Updates available {1} ( {2} )", NAME, Response.ProductInfo.LastProduct.Version, Response.ProductInfo.LastProduct.Updated);

                // --> Informo l'utente con un messaggio sul grafico e nei log del cbot
                Chart.DrawStaticText(NAME + "Updates", updatemex, VerticalAlignment.Top, HorizontalAlignment.Left, Color.Red);
                Print(updatemex);

            }

        }

        #endregion

    }

}

/// <summary>
/// NameSpace che racchiude tutte le feature ctrader.guru
/// </summary>
namespace Guru
{
    /// <summary>
    /// Classe che definisce lo standard identificativo del prodotto nel marketplace ctrader.guru
    /// </summary>
    public class Product
    {

        public int ID = 0;
        public string Name = "";
        public string Version = "";
        public string Updated = "";

    }

    public class CookieInformation
    {

        public DateTime LastCheck = new DateTime();

    }

    /// <summary>
    /// Offre la possibilità di utilizzare le API messe a disposizione da ctrader.guru per verificare gli aggiornamenti del prodotto.
    /// Permessi utente "AccessRights = AccessRights.FullAccess" per accedere a internet ed utilizzare JSON
    /// </summary>
    public class API
    {
        /// <summary>
        /// Costante da non modificare, corrisponde alla pagina dei servizi API
        /// </summary>
        private const string Service = "https://ctrader.guru/api/product_info/";

        /// <summary>
        /// Costante da non modificare, utilizzata per filtrare le richieste
        /// </summary>
        private const string UserAgent = "cTrader Guru";

        /// <summary>
        /// Variabile dove verranno inserite le direttive per la richiesta
        /// </summary>
        private RequestProductInfo RequestProduct = new RequestProductInfo();

        /// <summary>
        /// Il percorso della cartella dove riporre i cookie
        /// </summary>
        private readonly string _mainpath = string.Format("{0}\\cAlgo\\cTrader GURU\\Cookie", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        /// <summary>
        /// Il percorso completo del file che verrà utilizzato per il controllo degli aggiornamenti
        /// </summary>
        private readonly string _pathsetup;

        /// <summary>
        /// Legge e rende disponibile i contenuti del cookie
        /// </summary>
        /// <returns></returns>
        private string _loadSetup()
        {

            try
            {

                using (StreamReader r = new StreamReader(_pathsetup))
                {
                    string json = r.ReadToEnd();

                    return json;
                }

            }
            catch
            {

                return null;

            }

        }

        /// <summary>
        /// Scrive i valori del cookie
        /// </summary>
        /// <param name="mysetup">I valori da registrare</param>
        /// <returns></returns>
        private bool _writeSetup(CookieInformation mysetup)
        {

            try
            {

                Directory.CreateDirectory(_mainpath);

                using (StreamWriter file = File.CreateText(_pathsetup))
                {

                    JsonSerializer serializer = new JsonSerializer();

                    serializer.Serialize(file, mysetup);

                }

                return true;

            }
            catch
            {

                return false;

            }

        }

        /// <summary>
        /// Variabile dove verranno inserite le informazioni identificative dal server dopo l'inizializzazione della classe API
        /// </summary>
        public ResponseProductInfo ProductInfo = new ResponseProductInfo();

        /// <summary>
        /// Classe che formalizza i parametri di richiesta, vengono inviate le informazioni del prodotto e di profilazione a fini statistici
        /// </summary>
        public class RequestProductInfo
        {

            /// <summary>
            /// Il prodotto corrente per il quale richiediamo le informazioni
            /// </summary>
            public Product MyProduct = new Product();

            /// <summary>
            /// Broker con il quale effettiamo la richiesta
            /// </summary>
            public string AccountBroker = "";

            /// <summary>
            /// Il numero di conto con il quale chiediamo le informazioni
            /// </summary>
            public int AccountNumber = 0;

        }

        /// <summary>
        /// Classe che formalizza lo standard per identificare le informazioni del prodotto
        /// </summary>
        public class ResponseProductInfo
        {

            /// <summary>
            /// Il prodotto corrente per il quale vengono fornite le informazioni
            /// </summary>
            public Product LastProduct = new Product();

            /// <summary>
            /// Eccezioni in fase di richiesta al server, da utilizzare per controllare l'esito della comunicazione
            /// </summary>
            public string Exception = "";

            /// <summary>
            /// La risposta del server
            /// </summary>
            public string Source = "";

        }

        /// <summary>
        /// Richiede le informazioni del prodotto richiesto
        /// </summary>
        /// <param name="Request"></param>
        public API(RequestProductInfo Request)
        {

            RequestProduct = Request;

            // --> Non controllo se non ho l'ID del prodotto
            if (Request.MyProduct.ID <= 0)
                return;

            // --> Rendo disponibile il file del cookie
            _pathsetup = string.Format("{0}\\{1}.json", _mainpath, Request.MyProduct.ID);

            CookieInformation MySetup = new CookieInformation();
            DateTime now = DateTime.Now;

            // --> Evito di chiamare il server se non sono passate almeno 24h
            try
            {

                string json = _loadSetup();

                if (json != null && json.Trim().Length > 0)
                {

                    json = json.Trim();

                    MySetup = JsonConvert.DeserializeObject<CookieInformation>(json);
                    DateTime ExpireDate = MySetup.LastCheck.AddDays(1);

                    // --> Impedisco di controllare se non è passato il tempo necessario
                    if (now < ExpireDate)
                    {

                        ProductInfo.Exception = string.Format("Check for updates scheduled for {0}", ExpireDate.ToString());
                        return;

                    }

                }

            }
            catch (Exception Exp)
            {

                // --> Setup corrotto ? resetto!
                _writeSetup(MySetup);

                // --> Se ci sono errori non controllo perchè non è gestito ed evito di sovraccaricare il server che mi bloccherebbe
                ProductInfo.Exception = Exp.Message;
                return;

            }

            // --> Dobbiamo supervisionare la chiamata per registrare l'eccexione
            try
            {

                // --> Strutturo le informazioni per la richiesta POST
                NameValueCollection data = new NameValueCollection
                {
                    {
                        "account_broker",
                        Request.AccountBroker
                    },
                    {
                        "account_number",
                        Request.AccountNumber.ToString()
                    },
                    {
                        "my_version",
                        Request.MyProduct.Version
                    },
                    {
                        "productid",
                        Request.MyProduct.ID.ToString()
                    }
                };

                // --> Autorizzo tutte le pagine di questo dominio
                Uri myuri = new Uri(Service);
                string pattern = string.Format("{0}://{1}/.*", myuri.Scheme, myuri.Host);

                Regex urlRegEx = new Regex(pattern);
                WebPermission p = new WebPermission(NetworkAccess.Connect, urlRegEx);
                p.Assert();

                // --> Protocollo di sicurezza https://
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;

                // -->> Richiedo le informazioni al server
                using (var wb = new WebClient())
                {

                    wb.Headers.Add("User-Agent", UserAgent);

                    var response = wb.UploadValues(myuri, "POST", data);
                    ProductInfo.Source = Encoding.UTF8.GetString(response);

                }

                // -->>> Nel cBot necessita l'attivazione di "AccessRights = AccessRights.FullAccess"
                ProductInfo.LastProduct = JsonConvert.DeserializeObject<Product>(ProductInfo.Source);

                // --> Salviamo la sessione
                MySetup.LastCheck = now;
                _writeSetup(MySetup);

            }
            catch (Exception Exp)
            {

                // --> Qualcosa è andato storto, registro l'eccezione
                ProductInfo.Exception = Exp.Message;

            }

        }

        /// <summary>
        /// Esegue un confronto tra le versioni per determinare la presenza di aggiornamenti
        /// </summary>
        /// <returns></returns>
        public bool HaveNewUpdate()
        {

            // --> Voglio essere sicuro che stiamo lavorando con le informazioni giuste
            return (ProductInfo.LastProduct.ID == RequestProduct.MyProduct.ID && ProductInfo.LastProduct.Version != "" && RequestProduct.MyProduct.Version != "" && new Version(RequestProduct.MyProduct.Version).CompareTo(new Version(ProductInfo.LastProduct.Version)) < 0);

        }

    }

}
