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
     class boxStateConverter : IValueConverter
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
                            //txt_stateValue.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush; ;
                            return MainWindow.resourcemanager.GetString("trClosed");
                        case "o":
                            //txt_stateValue.Foreground = Application.Current.Resources["mediumGreen"] as SolidColorBrush; ;
                            return MainWindow.resourcemanager.GetString("trOpen");
                        default:
                            return "";
                    }
                }
                else return "";
            }
            catch
            {
                return "";
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
