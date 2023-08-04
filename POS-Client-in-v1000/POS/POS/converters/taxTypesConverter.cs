using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class taxTypesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null)
                {
                    string s = value.ToString();
                    switch (s)
                    {
                        case "sales":
                            s = MainWindow.resourcemanager.GetString("SalesTax");
                            break;
                        //case "income":
                        //    s = MainWindow.resourcemanager.GetString("IncomeTax");
                        //    break;
                        default:
                            s = "";
                            break;
                    }
                    return s;
                }
                else return "0";
            }
            catch
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
