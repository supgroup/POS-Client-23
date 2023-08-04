using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using POS.Classes;

namespace POS.converters
{
    class itemHasSerialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null)
                {
                    POS.View.uc_receiptInvoice.BillDetails item = value as POS.View.uc_receiptInvoice.BillDetails;
                    if (item.type == "sn" ||
                (item.type.Equals("p") && item.packageItems != null && item.packageItems.Where(x => x.type == "sn").FirstOrDefault() != null))
                        return true;
                    else return false;
                }
                else
                    return "";
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
