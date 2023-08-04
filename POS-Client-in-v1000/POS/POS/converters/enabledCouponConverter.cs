using POS.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class enabledCouponConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Coupon c = value as Coupon;
                string state = "";

                if ((c.isActive == 1) && ((c.endDate == null) ? true : (c.endDate.Value.Date >= DateTime.Now.Date)) && ((c.startDate == null) ? true : (c.startDate.Value.Date <= DateTime.Now.Date)) && ((c.quantity == 0) || (c.quantity > 0 && c.remainQ != 0)))
                    state = MainWindow.resourcemanager.GetString("trValid");
                else
                    state = MainWindow.resourcemanager.GetString("trExpired");

                return state;
            }
            catch
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
