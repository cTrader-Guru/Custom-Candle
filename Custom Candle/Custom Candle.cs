/*  CTRADER GURU --> Template 1.0.6

    Homepage    : https://ctrader.guru/
    Telegram    : https://t.me/ctraderguru
    Twitter     : https://twitter.com/cTraderGURU/
    Facebook    : https://www.facebook.com/ctrader.guru/
    YouTube     : https://www.youtube.com/channel/UCKkgbw09Fifj65W5t5lHeCQ
    GitHub      : https://github.com/cTraderGURU/

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

        public const string NAME = "Custom Candle";

        public const string VERSION = "1.0.2";

        #endregion

        #region Params

        [Parameter(NAME + " " + VERSION, Group = "Identity", DefaultValue = "https://www.google.com/search?q=ctrader+guru+custom+candle")]
        public string ProductInfo { get; set; }

        [Parameter("Candle TimeFrame", Group = "Params", DefaultValue = 8, Step = 1)]
        public TimeFrame CandleTimeFrame { get; set; }

        [Parameter("Candle Mode", Group = "Params", DefaultValue = CandleMode.HighLow)]
        public CandleMode MyCandleMode { get; set; }

        [Parameter("Candles To Show", Group = "Params", DefaultValue = 10, MinValue = 1, Step = 1)]
        public int CandleShow { get; set; }

        [Parameter("Line Style Box", Group = "Styles", DefaultValue = LineStyle.Solid)]
        public LineStyle LineStyleBox { get; set; }

        [Parameter("Tickness", Group = "Styles", DefaultValue = 1, MaxValue = 5, MinValue = 1, Step = 1)]
        public int TicknessBox { get; set; }

        [Parameter("High/Open/Long Color", Group = "Styles", DefaultValue = "DodgerBlue")]
        public string ColorHigh { get; set; }

        [Parameter("Low/Close/Short Color", Group = "Styles", DefaultValue = "Red")]
        public string ColorLow { get; set; }

        [Parameter("Opacity", Group = "Styles", DefaultValue = 30, MinValue = 1, MaxValue = 100, Step = 1)]
        public int Opacity { get; set; }

        [Parameter("Boxed ?", Group = "Styles", DefaultValue = true)]
        public bool Boxed { get; set; }

        [Parameter("Fill Box ?", Group = "Styles", DefaultValue = true)]
        public bool FillBox { get; set; }

        #endregion

        #region Property



        #endregion

        #region Indicator Events

        protected override void Initialize()
        {

            if (TimeFrame >= CandleTimeFrame)
                Chart.DrawStaticText("Alert", string.Format("{0} : USE THIS INDICATOR ON TIMEFRAME LOWER {1}", NAME.ToUpper(), CandleTimeFrame.ToString().ToUpper()), VerticalAlignment.Center, HorizontalAlignment.Center, Color.Red);

            Print("{0} : {1}", NAME, VERSION);

            if (Color.FromName(ColorHigh).ToArgb() == 0)
                ColorHigh = "DodgerBlue";

            if (Color.FromName(ColorLow).ToArgb() == 0)
                ColorLow = "Red";

        }

        public override void Calculate(int index)
        {

            if (TimeFrame >= CandleTimeFrame)
                return;

            try
            {

                DrawLevelFromCustomBar();

            } catch (Exception exp)
            {

                Chart.DrawStaticText("Alert", string.Format("{0} : error, {1}", NAME, exp), VerticalAlignment.Center, HorizontalAlignment.Center, Color.Red);

            }


        }

        #endregion

        #region Private Methods

        private void DrawLevelFromCustomBar()
        {

            Bars BarsCustom = MarketData.GetBars(CandleTimeFrame);

            int index = BarsCustom.Count - 1;

            if (index < CandleShow || index < 1)
                return;

            for (int i = 0; i < CandleShow; i++)
            {

                try
                {

                    DateTime thisCandle = BarsCustom[index - i].OpenTime;
                    DateTime nextCandle = (i == 0) ? thisCandle.AddMinutes(GetTimeFrameCandleInMinutes(CandleTimeFrame)) : BarsCustom[index - i + 1].OpenTime;

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

        private int GetTimeFrameCandleInMinutes(TimeFrame MyCandle)
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
