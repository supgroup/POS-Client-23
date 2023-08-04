using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class depositorTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string s = value as string;
                switch (value)
                {
                    case "v": return MainWindow.resourcemanager.GetString("trVendor");
                    //break;
                    case "c": return MainWindow.resourcemanager.GetString("trCustomer");
                    //break;
                    case "u": return MainWindow.resourcemanager.GetString("trUser");
                    //break;
                    default: return MainWindow.resourcemanager.GetString("trAdministrativeDeposit");
                        //break;
                }
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
