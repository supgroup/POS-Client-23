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
    class depositorBondeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Bonds s = value as Bonds;
                string name = "";
                switch (s.ctside)
                {
                    case "bnd": break;
                    case "v": name = MainWindow.resourcemanager.GetString("trVendor"); break;
                    case "c": name = MainWindow.resourcemanager.GetString("trCustomer"); break;
                    case "u": name = MainWindow.resourcemanager.GetString("trUser"); break;
                    case "s": name = MainWindow.resourcemanager.GetString("trSalary"); break;
                    case "e": name = MainWindow.resourcemanager.GetString("trGeneralExpenses"); break;
                    case "m":
                        if (s.cttransType == "d")
                            name = MainWindow.resourcemanager.GetString("trAdministrativeDeposit");
                        if (s.cttransType == "p")
                            name = MainWindow.resourcemanager.GetString("trAdministrativePull");
                        break;
                    case "sh": name = MainWindow.resourcemanager.GetString("trShippingCompany"); break;
                    default: break;
                }

                if (!string.IsNullOrEmpty(s.ctagentName))
                    return name + " " + s.ctagentName;
                else if (!string.IsNullOrEmpty(s.ctusersName))
                    return name + " " + s.ctusersName + " " + s.ctusersLName;
                else if (!string.IsNullOrEmpty(s.ctshippingCompanyName))
                    return name + " " + s.ctshippingCompanyName;
                else
                    return name;

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
