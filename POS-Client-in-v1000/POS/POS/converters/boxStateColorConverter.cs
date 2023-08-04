using POS.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace POS.converters
{
    class boxStateColorConverter : IValueConverter
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
                        case "c":
                            return System.Windows.Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                        case "o":
                            return System.Windows.Application.Current.Resources["mediumGreen"] as SolidColorBrush;
                        default:
                            return System.Windows.Application.Current.Resources["textColor"] as SolidColorBrush;
                    }
                }
                else return System.Windows.Application.Current.Resources["textColor"] as SolidColorBrush;
            }
            catch
            {
                return System.Windows.Application.Current.Resources["textColor"] as SolidColorBrush;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                throw new NotImplementedException();

            }
            catch
            {
                return value;
            }
        }
    }
}
