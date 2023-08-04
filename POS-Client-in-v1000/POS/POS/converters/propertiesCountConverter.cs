using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class propertiesCountConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values != null && values[0] != null && values[1] != null)
                {
                    string itemType = values[0].ToString();
                    Nullable<long> propertiesCount = (Nullable<long>)values[1];

                    if (itemType == "p")
                        return "-";
                    else
                        return propertiesCount.ToString();
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
