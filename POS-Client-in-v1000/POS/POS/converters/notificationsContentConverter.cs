using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    public class notificationsContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return MainWindow.GetUntilOrEmpty(value.ToString(), ":")
                          + " : " +
                         MainWindow.resourcemanager.GetString(value.ToString().Substring(value.ToString().LastIndexOf(':') + 1)); ;
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
