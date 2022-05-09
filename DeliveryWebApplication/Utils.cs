using ClosedXML.Excel;

namespace DeliveryWebApplication
{
    public static class Utils
    {
        public static string FormattedWeight(int? weight)
        {
            if (weight is null)
                return "—";
            if (weight < 1000)
                return weight + " г";
            else
                return ((decimal)weight / 1000).ToString("0.###") + " кг";
        }

        public static string FormattedWeight(decimal? weight) => FormattedWeight((int?)(weight * 1000));

        public static void SetCulture()
        {
            var cul = new System.Globalization.CultureInfo("en-US");
            cul.NumberFormat.NumberDecimalSeparator = ".";
            cul.NumberFormat.NumberGroupSeparator = "";
            Thread.CurrentThread.CurrentCulture = cul;
        }
    }
}
