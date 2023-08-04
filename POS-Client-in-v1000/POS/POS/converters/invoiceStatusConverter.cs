using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class invoiceStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string s = value as string;
                switch (value)
                {
                    case "InTheWay": return MainWindow.resourcemanager.GetString("trInDelivery");//tr
                                                                                                 //break;
                    case "Done": return MainWindow.resourcemanager.GetString("trDelivered");//rc
                                                                                            //break;
                    default: return "";
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
