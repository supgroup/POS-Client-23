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
    class paymentBtnVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values != null && values[0] != null && values[1] != null)
                {
                    List<CashTransfer> s = values[0] as List<CashTransfer>;
                    decimal total = (decimal)values[1];

                    if (s.Count() == 1)
                    {
                        if (s.Sum(x => x.cash.Value) < total)
                            return Visibility.Visible;//visible
                        else
                            switch (s.Select(x => x.processType.ToString()).FirstOrDefault())
                            {
                                case "multiple": return Visibility.Visible;//visible
                                default: return Visibility.Hidden;//hidden
                            }
                    }
                    else if (s.Count() > 1)
                    {
                        //if (s.Sum(x => x.cash.Value) < total)
                        //    return Visibility.Hidden;//hidden
                        //else
                            return Visibility.Visible;//visible
                    }
                    else
                        return Visibility.Hidden;//hidden
                }
                return Visibility.Hidden;//hidden
            }
            catch
            {
                return Visibility.Hidden;//hidden
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
