using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    class processTypeAndCardConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string pType = (string)values[0];
                string cName = (string)values[1];
                string transType="" ;
                if (values.Count() >2 && values[2]!=null)
                {
                    transType = (string)values[2];
                }
              
                    switch (pType)
                    {
                        case "statement":
                            if (transType == "p")
                            { return MainWindow.resourcemanager.GetString("trRequired"); }
                            else { return MainWindow.resourcemanager.GetString("trWorthy"); }
                        case "cash": return MainWindow.resourcemanager.GetString("trCash");
                        //break;
                        case "doc": return MainWindow.resourcemanager.GetString("trDocument");
                        //break;
                        case "cheque": return MainWindow.resourcemanager.GetString("trCheque");
                        //break;
                        case "balance": return MainWindow.resourcemanager.GetString("trCredit");
                        //break;
                        case "card": return cName;
                        //break;
                        //case "inv": return MainWindow.resourcemanager.GetString("trInv");
                        case "inv": return "-";
                        case "multiple": return MainWindow.resourcemanager.GetString("trMultiplePayment");
                        case "commissionAgent":
                        case "destroy":
                        case "shortage":
                        case "deliver": return "-";
                        //break;
                        default: return pType;
                            //break;
                    }
                
               
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
