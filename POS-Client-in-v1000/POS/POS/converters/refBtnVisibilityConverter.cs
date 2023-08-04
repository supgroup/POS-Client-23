using POS.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace POS.converters
{
    class refBtnVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values != null && values[0] != null && values[1] != null)
                {
                    //List<CashTransfer> s = values[0] as List<CashTransfer>;
                    //decimal total = (decimal)values[1];
                    string type = (string)values[0];
                    List<Invoice> returnLst = values[1] as List<Invoice>;

                    if (type == "sd" || type == "sbd" || type == "pd" || type == "pbd")//draft
                        return Visibility.Hidden;
                    else if (type == "sb" || type == "pb")//////////////return
                        return Visibility.Visible;
                    else if (type == "s" || type == "p" || type == "pw")///////////////invoice
                    {
                        if (returnLst.Count > 0)
                            return Visibility.Visible;
                        else
                            return Visibility.Hidden;
                    }
                    else
                        return Visibility.Hidden;
                }
                else
                    return Visibility.Hidden;
            }
            catch
            {
                return Visibility.Hidden;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
