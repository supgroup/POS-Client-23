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
    class closingDescriptonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                OpenClosOperatinModel s = value as OpenClosOperatinModel;
                #region old
                /*
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
                    case "sh": name = MainWindow.resourcemanager.GetString("trShippingCompany"); break;
                    default: break;
                }

                if (!string.IsNullOrEmpty(s.agentName))
                    name = name + " " + s.agentName;
                else if (!string.IsNullOrEmpty(s.usersName) && !string.IsNullOrEmpty(s.usersLName))
                    name = name + " " + s.usersName + " " + s.usersLName;
                else if (!string.IsNullOrEmpty(s.shippingCompanyName))
                    name = name + " " + s.shippingCompanyName;
                else if ((s.side != "e") && (s.side != "m"))
                    name = name + " " + MainWindow.resourcemanager.GetString("trUnKnown");

                if (s.transType.Equals("p"))
                {
                    if ((s.side.Equals("bn")) || (s.side.Equals("p")))
                    {
                        return MainWindow.resourcemanager.GetString("trPull") + " " +
                               MainWindow.resourcemanager.GetString("trFrom") + " " +
                               name;//receive
                    }
                    else if ((!s.side.Equals("bn")) || (!s.side.Equals("p")))
                    {
                        return MainWindow.resourcemanager.GetString("trPayment") + " " +
                               MainWindow.resourcemanager.GetString("trTo") + " " +
                               name;//دفع
                    }
                    else return "";
                }
                else if (s.transType.Equals("d"))
                {
                    if ((s.side.Equals("bn")) || (s.side.Equals("p")))
                    {
                        return MainWindow.resourcemanager.GetString("trDeposit") + " " +
                               MainWindow.resourcemanager.GetString("trTo") + " " +
                               name;
                    }
                    else if ((!s.side.Equals("bn")) || (!s.side.Equals("p")))
                    {
                        return MainWindow.resourcemanager.GetString("trReceiptOperation") + " " +
                               MainWindow.resourcemanager.GetString("trFrom") + " " +
                               name;//قبض
                    }
                    else return "";
                }
                else return "";
                */
                #endregion
                string name = "";
                string finalstring = "";
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
                    case "sh": name = MainWindow.resourcemanager.GetString("trShippingCompany"); break;
                    case "tax": name = MainWindow.resourcemanager.GetString("trTaxCollection"); break;
                    case "bn": name = MainWindow.resourcemanager.GetString("trBank"); break;
                    case "p": name = MainWindow.resourcemanager.GetString("trPosTooltip"); break;
                    default: break;
                }
                if (s.side == "bn")
                {
                    name = name + " " + s.bankName;
                }
                else if (s.side == "p")
                {//pos

                    name = s.branch2Name + " / " + s.pos2Name;
                }
                else if (!string.IsNullOrEmpty(s.agentName))
                    name = name + " " + s.agentName;
                else if (!string.IsNullOrEmpty(s.usersName) && !string.IsNullOrEmpty(s.usersLName) && s.side != "bn")
                {
                    name = name + " " + s.usersName + " " + s.usersLName;
                }
                else if (!string.IsNullOrEmpty(s.shippingCompanyName))
                    name = name + " " + s.shippingCompanyName;
                else if ((s.side != "e") && (s.side != "m"))
                    name = name + " " + MainWindow.resourcemanager.GetString("trUnKnown");

                if (s.transType.Equals("p"))
                {
                    if (s.side.Equals("bn") || s.side.Equals("p"))
                    {
                        return MainWindow.resourcemanager.GetString("trPull") + " " +
                               MainWindow.resourcemanager.GetString("trFrom") + " " +
                               name;//receive
                    }
                    else if (!s.side.Equals("bn") && !s.side.Equals("p"))
                    {
                        finalstring = MainWindow.resourcemanager.GetString("trPayment") + " " +
                               MainWindow.resourcemanager.GetString("trTo") + " " +
                               name;//دفع
                        if (s.invId != null)
                        {
                            finalstring = finalstring + " - " + ConvertInvType(s.invType) + " #: " + s.invNumber;
                            // finalstring = " #:" + s.invNumber+finalstring + " - " + ConvertInvType(s.invType) ;
                        }
                        return finalstring;
                    }
                    else return "";
                }
                else if (s.transType.Equals("d"))
                {
                    if (s.side.Equals("bn") || s.side.Equals("p"))
                    {
                        return MainWindow.resourcemanager.GetString("trDeposit") + " " +
                               MainWindow.resourcemanager.GetString("trTo") + " " +
                               name;
                    }
                    else if (!s.side.Equals("bn") && !s.side.Equals("p"))
                    {
                        finalstring = MainWindow.resourcemanager.GetString("trReceiptOperation") + " " +
                               MainWindow.resourcemanager.GetString("trFrom") + " " +
                               name;//قبض
                        if (s.invId != null)
                        {
                            finalstring = finalstring + " - " + ConvertInvType(s.invType) + " #: " + s.invNumber;
                        }
                        return finalstring;

                    }
                    else return "";
                }
             else if (s.transType == "o" && s.processType == "box")
            {
                //open box: processType:box ,transType=o
                finalstring = MainWindow.resourcemanager.GetString("Cashdrawercontents") + " - " + MainWindow.resourcemanager.GetString("trOpenCash");

                return finalstring;
            }
            else return "";

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
        #region old
        //public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        //{
        //    //string s = value as string;
        //    string transType = (string)values[0];
        //    string side = (string)values[1];

        //   if (transType.Equals("p"))
        //    {
        //        if ((side.Equals("bn")) || (side.Equals("p")))
        //        {
        //            return MainWindow.resourcemanager.GetString("trReceiptOperation")+" "+ 
        //                   MainWindow.resourcemanager.GetString("trFrom") + " " + 
        //                   side;//receive
        //        }
        //        else if ((!side.Equals("bn")) || (!side.Equals("p")))
        //        {
        //            return MainWindow.resourcemanager.GetString("trPayment")+" "+
        //                   MainWindow.resourcemanager.GetString("trTo") + " " + 
        //                   side;//دفع
        //        }
        //        else return "";
        //    }
        //    else if (transType.Equals("d"))
        //    {
        //        if ((side.Equals("bn")) || (side.Equals("p")))
        //        {
        //            return MainWindow.resourcemanager.GetString("trDeposit")+" "+
        //                   MainWindow.resourcemanager.GetString("trTo") + " " + 
        //                   side;
        //        }
        //        else if ((!side.Equals("bn")) || (!side.Equals("p")))
        //        {
        //            return MainWindow.resourcemanager.GetString("trReceive")+" "+
        //                   MainWindow.resourcemanager.GetString("trFrom") + " " + 
        //                   side;//قبض
        //        }
        //        else return "";
        //    }
        //    else return "";
        //}

        //public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        //{
        //    throw new NotImplementedException();
        //}
        #endregion
        private string ConvertInvType(string invType)
        {
            string value = "";
            value = invType;

            try
            {

                switch (value)
                {
                    //مشتريات 
                    case "p":
                        value = MainWindow.resourcemanager.GetString("trPurchaseInvoice");
                        break;
                    //فاتورة مشتريات بانتظار الادخال
                    case "pw":
                        value = MainWindow.resourcemanager.GetString("trPurchaseInvoiceWaiting");
                        break;
                    //مبيعات
                    case "s":
                        value = MainWindow.resourcemanager.GetString("trSalesInvoice");
                        break;
                    //مرتجع مبيعات
                    case "sb":
                        value = MainWindow.resourcemanager.GetString("trSalesReturnInvoice");
                        break;
                    //مرتجع مشتريات
                    case "pb":
                        value = MainWindow.resourcemanager.GetString("trPurchaseReturnInvoice");
                        break;
                    //فاتورة مرتجع مشتريات بانتظار الاخراج
                    case "pbw":
                        value = MainWindow.resourcemanager.GetString("trPurchaseReturnInvoiceWaiting");
                        break;
                    //مسودة مشتريات 
                    case "pd":
                        value = MainWindow.resourcemanager.GetString("trDraftPurchaseBill");
                        break;
                    //مسودة مبيعات
                    case "sd":
                        value = MainWindow.resourcemanager.GetString("trSalesDraft");
                        break;
                    //مسودة مرتجع مبيعات
                    case "sbd":
                        value = MainWindow.resourcemanager.GetString("trSalesReturnDraft");
                        break;
                    //مسودة مرتجع مشتريات
                    case "pbd":
                        value = MainWindow.resourcemanager.GetString("trPurchaseReturnDraft");
                        break;
                    // مسودة طلبية مبيعا 
                    case "ord":
                        value = MainWindow.resourcemanager.GetString("trDraft");
                        break;
                    //طلبية مبيعات 
                    case "or":
                        value = MainWindow.resourcemanager.GetString("trSaleOrder");
                        break;
                    //مسودة طلبية شراء 
                    case "pod":
                        value = MainWindow.resourcemanager.GetString("trDraft");
                        break;
                    //طلبية شراء 
                    case "po":
                        value = MainWindow.resourcemanager.GetString("trPurchaceOrder");
                        break;
                    // طلبية شراء أو بيع محفوظة
                    case "pos":
                    case "ors":
                        value = MainWindow.resourcemanager.GetString("trSaved");
                        break;
                    //مسودة عرض 
                    case "qd":
                        value = MainWindow.resourcemanager.GetString("trQuotationsDraft");
                        break;
                    //عرض سعر محفوظ
                    case "qs":
                        value = MainWindow.resourcemanager.GetString("trSaved");
                        break;
                    //فاتورة عرض اسعار
                    case "q":
                        value = MainWindow.resourcemanager.GetString("trQuotations");
                        break;
                    //الإتلاف
                    case "d":
                        value = MainWindow.resourcemanager.GetString("trDestructive");
                        break;
                    //النواقص
                    case "sh":
                        value = MainWindow.resourcemanager.GetString("trShortage");
                        break;
                    //مسودة  استراد
                    case "imd":
                        value = MainWindow.resourcemanager.GetString("trImportDraft");
                        break;
                    // استراد
                    case "im":
                        value = MainWindow.resourcemanager.GetString("trImport");
                        break;
                    // طلب استيراد
                    case "imw":
                        value = MainWindow.resourcemanager.GetString("trImportOrder");
                        break;
                    //مسودة تصدير
                    case "exd":
                        value = MainWindow.resourcemanager.GetString("trExportDraft");
                        break;
                    // تصدير
                    case "ex":
                        value = MainWindow.resourcemanager.GetString("trExport");
                        break;
                    // طلب تصدير
                    case "exw":
                        value = MainWindow.resourcemanager.GetString("trExportOrder");
                        break;
                    // إدخال مباشر
                    case "is":
                        value = MainWindow.resourcemanager.GetString("trDirectEntry");
                        break;
                    // مسودة إدخال مباشر
                    case "isd":
                        value = MainWindow.resourcemanager.GetString("trDirectEntryDraft");
                        break;
                    default: break;
                }
                return value;
            }
            catch
            {
                return "";
            }
        }
    }
}
