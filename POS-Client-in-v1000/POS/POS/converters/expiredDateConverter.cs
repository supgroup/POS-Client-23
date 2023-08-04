using POS.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class expiredDateConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values != null &&
                !(values[0].ToString() == "{DependencyProperty.UnsetValue}" ||
                    values[1].ToString() == "{DependencyProperty.UnsetValue}"))
                {
                    string period = "";
                    DateTime expiredDate = DateTime.Parse(values[0].ToString());
                    bool islimitDate = bool.Parse(values[1].ToString());
                    if (!islimitDate)
                        period = MainWindow.resourcemanager.GetString("trUnLimited");
                    else
                    {
                        DateTimeFormatInfo dtfi = DateTimeFormatInfo.CurrentInfo;
                        DateTime date = DateTime.Now;
                        if (expiredDate is DateTime)
                            date = (DateTime)expiredDate;
                        else
                            period = expiredDate.ToString();

                        switch (AppSettings.dateFormat)
                        {
                            case "ShortDatePattern": period = date.ToString(@"dd/MM/yyyy"); break;
                            case "LongDatePattern": period = date.ToString(@"dddd, MMMM d, yyyy"); break;
                            case "MonthDayPattern": period = date.ToString(@"MMMM dd"); break;
                            case "YearMonthPattern": period = date.ToString(@"MMMM yyyy"); break;
                            default: period = date.ToString(@"dd/MM/yyyy"); break;
                        }

                    }
                    return period;

                }
                return "";
            }
            catch
            {
                return "";
            }
        }

        /*
        
         */
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {

            string[] values = null;
            if (value != null)
                return values = value.ToString().Split(' ');
            return values;
        }

    }
}
