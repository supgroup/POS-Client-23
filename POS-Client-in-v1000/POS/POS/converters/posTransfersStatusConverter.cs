using POS.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Data;

namespace POS.converters
{
    class posTransfersStatusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values != null && values[0] != null && values[1] != null)
                {
                    byte isConfirm1 = (byte)values[0];
                    byte isConfirm2 = (byte)values[1];

                    if ((isConfirm1 == 1) && (isConfirm2 == 1))
                        return MainWindow.resourcemanager.GetString("trConfirmed");
                    else if ((isConfirm1 == 2) || (isConfirm2 == 2))
                        return MainWindow.resourcemanager.GetString("trCanceled");
                    else
                        return MainWindow.resourcemanager.GetString("trWaiting");
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
