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
    class profitsDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                ItemUnitInvoiceProfit p = value as ItemUnitInvoiceProfit;
                string description = "";

                if (!string.IsNullOrEmpty(p.invNumber))
                    description = MainWindow.resourcemanager.GetString("trProfitInvoice");
                else
                {
                    switch (p.side.ToString())
                    {
                        case "bnd": break;
                        case "v": description = MainWindow.resourcemanager.GetString("trVendor"); break;
                        case "c": description = MainWindow.resourcemanager.GetString("trCustomer"); break;
                        case "u": description = MainWindow.resourcemanager.GetString("trUser"); break;
                        case "s": description = MainWindow.resourcemanager.GetString("trSalary"); break;
                        case "e": description = MainWindow.resourcemanager.GetString("trGeneralExpenses"); break;
                        case "m": description = MainWindow.resourcemanager.GetString("trAdministrativePull"); break;
                        case "sh": description = MainWindow.resourcemanager.GetString("trShippingCompany"); break;
                        case "tax": description = MainWindow.resourcemanager.GetString("trTaxCollection"); break;
                        default: break;
                    }

                    description = MainWindow.resourcemanager.GetString("trPayment") + "-" + description;
                }

                return description;
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
