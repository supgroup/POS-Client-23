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
    class depositorNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                CashTransfer s = value as CashTransfer;
                string name = "";
                switch (s.side)
                {
                    case "bnd": break;
                    case "v": name = MainWindow.resourcemanager.GetString("trVendor"); break;
                    case "c": name = MainWindow.resourcemanager.GetString("trCustomer"); break;
                    case "u": name = MainWindow.resourcemanager.GetString("trUser"); break;
                    case "s": name = MainWindow.resourcemanager.GetString("trSalary"); break;
                    case "e": name = MainWindow.resourcemanager.GetString("trGeneralExpenses"); break;
                    case "m":
                        if (s.transType == "d")
                            name = MainWindow.resourcemanager.GetString("trAdministrativeDeposit");
                        if (s.transType == "p")
                            name = MainWindow.resourcemanager.GetString("trAdministrativePull");
                        break;
                    case "sh":  name = MainWindow.resourcemanager.GetString("trShippingCompany"); break;
                    case "shd": name = MainWindow.resourcemanager.GetString("trDeliveryCharges"); break;
                    case "tax": name = MainWindow.resourcemanager.GetString("trTaxCollection"); break;
                    default: break;
                }

                if (!string.IsNullOrEmpty(s.agentName))
                    name = name + " " + s.agentName;
                else if (!string.IsNullOrEmpty(s.usersName) && !string.IsNullOrEmpty(s.usersLName))
                {
                    if(s.processType.Equals("commissionAgent"))
                        name = MainWindow.resourcemanager.GetString("commission_") +" "+ MainWindow.resourcemanager.GetString("for")  + " " + name + " " + s.usersName + " " + s.usersLName;
                    else
                        name = name + " " + s.usersName + " " + s.usersLName;
                }
                else if (!string.IsNullOrEmpty(s.shippingCompanyName))
                    name = name + " " + s.shippingCompanyName;
                else if ((s.side != "e") && (s.side != "m") && (s.side != "tax"))
                    name = name + " " + MainWindow.resourcemanager.GetString("trUnKnown");

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
