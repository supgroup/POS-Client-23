using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    public class invoiceTypeVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                //return Task.Run(() => itemUnit.GetItemUnits(int.Parse(value.ToString()))).Result;
                string s = value as string;
                //if (value.Equals("pd"))
                //    return MainWindow.resourcemanager.GetString("trSaleInvoice");
                //else 
                if (value.Equals("pbd") || value.Equals("pbd"))
                    return System.Windows.Visibility.Visible;
                else return System.Windows.Visibility.Collapsed;
            }
            catch
            {
                return System.Windows.Visibility.Collapsed;
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
