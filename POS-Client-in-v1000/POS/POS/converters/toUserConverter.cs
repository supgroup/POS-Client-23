using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class toUserConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                //<Binding Path = "isReaded" />
                //<Binding Path = "userRead" />
                //<Binding Path = "toUserFullName" />


                bool isReaded = (bool)values[0];
                string userRead = (string)values[1];
                string toUserFullName = (string)values[2];

                //if (values[1] != null)
                //    toUser = (string)values[1];
                if (isReaded)
                    return userRead;
                else if (!string.IsNullOrWhiteSpace(toUserFullName))
                    return toUserFullName;
                else
                    return "-";
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
