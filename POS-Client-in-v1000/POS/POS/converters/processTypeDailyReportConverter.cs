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
    class processTypeDailyReportConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values != null && values[0] != null && values[1] != null && values[2] != null)
                {
                    List<CashTransfer> s = values[0] as List<CashTransfer>;
                    decimal total = (decimal)values[1];
                    decimal cashSum = s.Sum(x => x.cash.Value);
                    string selectedProcessType = values[2].ToString();

                    //if (selectedProcessType.Equals("processType"))
                    {
                        if (s.Count() == 0)
                            return MainWindow.resourcemanager.GetString("trCredit");
                        else if (s.Count() == 1)
                        {
                            //if (s.Sum(x => x.cash.Value) < total)
                            if (cashSum < total)
                                return MainWindow.resourcemanager.GetString("trMultiplePayment");
                            else
                                switch (s.Select(x => x.processType.ToString()).FirstOrDefault())
                                {
                                    case "cash": return MainWindow.resourcemanager.GetString("trCash");
                                    case "doc": return MainWindow.resourcemanager.GetString("trDocument");
                                    case "cheque": return MainWindow.resourcemanager.GetString("trCheque");
                                    case "balance": return MainWindow.resourcemanager.GetString("trCredit");
                                    //case "card": return MainWindow.resourcemanager.GetString("trAnotherPaymentMethods");
                                    case "card": return selectedProcessType;//s.Select(x => new { x.cardName});
                                    case "inv": return MainWindow.resourcemanager.GetString("trInv");
                                    case "multiple": return MainWindow.resourcemanager.GetString("trMultiplePayment");
                                    default: return s;
                                }
                        }
                        else if (s.Count() > 1)
                        {
                            //if (s.Sum(x => x.cash.Value) < total)
                            //if (cashSum < total)
                            //    return MainWindow.resourcemanager.GetString("trCredit");
                            //else
                            return MainWindow.resourcemanager.GetString("trMultiplePayment");
                        }
                        else
                            return "";
                    }
                    //else
                    //    return "";
                }
                //return values;

                if (values != null && values[0] != null)
                {
                    List<CashTransfer> s = values[0] as List<CashTransfer>;
                    if (s.Count() == 1)
                    {
                        CashTransfer ctemp = s.FirstOrDefault();
                        if (ctemp.processType == "cash")
                        {
                            return MainWindow.resourcemanager.GetString("trCash");
                        }
                        else if (ctemp.processType == "card")
                        {
                            return ctemp.cardName;
                        }
                        else
                        {
                            return "";
                        }
                    }
                    else if (s.Count() == 0)
                    {
                        return MainWindow.resourcemanager.GetString("trCredit");
                    }
                  
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
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
