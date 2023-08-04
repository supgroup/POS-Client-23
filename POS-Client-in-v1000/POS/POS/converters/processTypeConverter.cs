using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class processTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string s = value as string;
                switch (value)
                {
                    case "cash": return MainWindow.resourcemanager.GetString("trCash");
                    //break;
                    case "doc": return MainWindow.resourcemanager.GetString("trDocument");
                    //break;
                    case "cheque": return MainWindow.resourcemanager.GetString("trCheque");
                    //break;
                    case "balance": return MainWindow.resourcemanager.GetString("trCredit");
                    //break;
                    case "card": return MainWindow.resourcemanager.GetString("trAnotherPaymentMethods");
                    //break;
                    case "inv": return MainWindow.resourcemanager.GetString("trInv");
                    case "multiple": return MainWindow.resourcemanager.GetString("trMultiplePayment");

                    //break;
                    default: return s;
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
