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
    class preparingOrderStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch (value)
                {
                    case "Listed": return MainWindow.resourcemanager.GetString("trListed");
                    case "Preparing": return MainWindow.resourcemanager.GetString("trPreparing");
                    case "Ready": return MainWindow.resourcemanager.GetString("trReady");
                    case "Collected": return MainWindow.resourcemanager.GetString("withDelivery");
                    case "InTheWay": return MainWindow.resourcemanager.GetString("onTheWay");
                    case "Done": return MainWindow.resourcemanager.GetString("trDone"); // gived to customer
                    default: return "";
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
