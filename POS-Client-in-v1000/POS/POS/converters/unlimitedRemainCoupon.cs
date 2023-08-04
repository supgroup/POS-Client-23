using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class unlimitedRemainCoupon : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values != null && values[0] != null && values[1] != null)
                {
                    int quantity = (int)values[0];
                    decimal remain = (int)values[1];

                    if (quantity == 0)
                        return MainWindow.resourcemanager.GetString("trUnlimited");
                    else
                        return remain.ToString();
                }
                return values;
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
