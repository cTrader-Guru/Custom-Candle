/*  CTRADER GURU --> Template 1.0.6

    Homepage    : https://ctrader.guru/
    Telegram    : https://t.me/ctraderguru
    Twitter     : https://twitter.com/cTraderGURU/
    Facebook    : https://www.facebook.com/ctrader.guru/
    YouTube     : https://www.youtube.com/channel/UCKkgbw09Fifj65W5t5lHeCQ
    GitHub      : https://github.com/cTraderGURU/
    TOS         : https://ctrader.guru/termini-del-servizio/

*/

using System;
using cAlgo.API;

namespace cAlgo
{

    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
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
        /// Nome del prodotto, identificativo, da modificare con il nome della propria creazione
        /// </summary>
        public const string NAME = "Custom Candle";

        /// <summary>
        /// La versione del prodotto, progressivo, utilie per controllare gli aggiornamenti se viene reso disponibile sul sito ctrader.guru
        /// </summary>
        public const string VERSION = "1.0.2";

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
                Chart.DrawStaticText("Alert", string.Format("{0} : USE THIS INDICATOR ON TIMEFRAME LOWER {1}", NAME.ToUpper(), CandleTimeFrame.ToString().ToUpper()), VerticalAlignment.Center, HorizontalAlignment.Center, Color.Red);

            // --> Stampo nei log la versione corrente
            Print("{0} : {1}", NAME, VERSION);

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
        
        #endregion

    }

}
