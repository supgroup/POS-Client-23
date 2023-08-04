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

    public class invoiceTypeNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                ItemTransferInvoice inv = value as ItemTransferInvoice;
                string result = "";
                switch (inv.invType)
                {
                    //مشتريات 
                    case "p":
                        result = MainWindow.resourcemanager.GetString("trPurchaseInvoice");
                        break;
                    //فاتورة مشتريات بانتظار الادخال
                    case "pw":
                        result = MainWindow.resourcemanager.GetString("trPurchaseInvoiceWaiting");
                        break;
                    //مبيعات
                    case "s":
                        result = MainWindow.resourcemanager.GetString("trSalesInvoice");
                        break;
                    //مرتجع مبيعات
                    case "sb":
                        result = MainWindow.resourcemanager.GetString("trSalesReturnInvoice");
                        break;
                    //مرتجع مشتريات
                    case "pb":
                        result = MainWindow.resourcemanager.GetString("trPurchaseReturnInvoice");
                        break;
                    //فاتورة مرتجع مشتريات بانتظار الاخراج
                    case "pbw":
                        result = MainWindow.resourcemanager.GetString("trPurchaseReturnInvoiceWaiting");
                        break;
                    //مسودة مشتريات 
                    case "pd":
                        result = MainWindow.resourcemanager.GetString("trDraftPurchaseBill");
                        break;
                    //مسودة مبيعات
                    case "sd":
                        result = MainWindow.resourcemanager.GetString("trSalesDraft");
                        break;
                    //مسودة مرتجع مبيعات
                    case "sbd":
                        result = MainWindow.resourcemanager.GetString("trSalesReturnDraft");
                        break;
                    //مسودة مرتجع مشتريات
                    case "pbd":
                        result = MainWindow.resourcemanager.GetString("trPurchaseReturnDraft");
                        break;
                    // مسودة طلبية مبيعات 
                    case "ord":
                        result = MainWindow.resourcemanager.GetString("trSaleOrderDraft");
                        break;
                    //   طلبية مبيعات 
                    case "or":
                        result = MainWindow.resourcemanager.GetString("trSaleOrder");
                        break;
                    // مسودة طلبية شراء 
                    case "pod":
                        result = MainWindow.resourcemanager.GetString("trPurchaceOrderDraft");
                        break;
                    // طلبية شراء 
                    case "po":
                        result = MainWindow.resourcemanager.GetString("trPurchaceOrder");
                        break;
                    //مسودة عرض 
                    case "qd":
                        result = MainWindow.resourcemanager.GetString("trQuotationsDraft");
                        break;
                    //فاتورة عرض اسعار
                    case "q":
                        result = MainWindow.resourcemanager.GetString("trQuotations");
                        break;
                    //الإتلاف
                    case "d":
                        result = MainWindow.resourcemanager.GetString("trDestructive");
                        break;
                    //النواقص
                    case "sh":
                        result = MainWindow.resourcemanager.GetString("trShortage");
                        break;
                    //مسودة  استراد
                    case "imd":
                        result = MainWindow.resourcemanager.GetString("trImportDraft");
                        break;
                    // استراد
                    case "im":
                        result = MainWindow.resourcemanager.GetString("trImport");
                        break;
                    // طلب استيراد
                    case "imw":
                        result = MainWindow.resourcemanager.GetString("trImportOrder");
                        break;
                    //مسودة تصدير
                    case "exd":
                        result = MainWindow.resourcemanager.GetString("trExportDraft");
                        break;
                    // تصدير
                    case "ex":
                        result = MainWindow.resourcemanager.GetString("trExport");
                        break;
                    // طلب تصدير
                    case "exw":
                        result = MainWindow.resourcemanager.GetString("trExportOrder");
                        break;
                    default: break;
                }
                return result + "-" + inv.invNumber;
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
