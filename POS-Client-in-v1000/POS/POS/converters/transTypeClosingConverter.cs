using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class transTypeClosingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                //string s = value as string;
                string transType = (string)values[0];
                string side = (string)values[1];

                if (transType.Equals("o"))
                    return MainWindow.resourcemanager.GetString("trOpen");
                else if (transType.Equals("c"))
                    return MainWindow.resourcemanager.GetString("trClose");
                else if (transType.Equals("p"))
                {
                    if ((side.Equals("bn")) || (side.Equals("p")))
                    {
                        return MainWindow.resourcemanager.GetString("trReceiptOperation");//receive
                    }
                    else if ((!side.Equals("bn")) || (!side.Equals("p")))
                    {
                        return MainWindow.resourcemanager.GetString("trPay");//دفع
                    }
                    else return "";
                }
                else if (transType.Equals("d"))
                {
                    if ((side.Equals("bn")) || (side.Equals("p")))
                    {
                        return MainWindow.resourcemanager.GetString("trDeposit");
                    }
                    else if ((!side.Equals("bn")) || (!side.Equals("p")))
                    {
                        return MainWindow.resourcemanager.GetString("trReceive");//قبض
                    }
                    else return "";
                }
                else return "";
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
