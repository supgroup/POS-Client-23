using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class isFreeShipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value.ToString().Equals("0"))
                    value = MainWindow.resourcemanager.GetString("trPaid");
                else if (value.ToString().Equals("1"))
                    value = MainWindow.resourcemanager.GetString("trFree");
                else
                    value = "";
                return value;
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
