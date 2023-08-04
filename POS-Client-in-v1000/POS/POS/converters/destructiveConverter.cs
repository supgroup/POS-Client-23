using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class destructiveConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] != null)
                {
                   return MainWindow.resourcemanager.GetString("onUser") +" : "+values[0];
                }
                //else if (values[1] != null)
                //    return MainWindow.resourcemanager.GetString("onCompany") + " : " + values[1];
                else
                    return MainWindow.resourcemanager.GetString("onCompany");
            }
            catch
            {
                return "";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
