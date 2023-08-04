using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class refInvoiceConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[2] != null )
                {
                    string type = values[0].ToString();
                    string num = values[1].ToString();
                    string refNum = values[2].ToString();

                    if (type == "sb")
                        return num + " - " + refNum;
                    else
                        return num;

                }
                else return values[1].ToString();
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
