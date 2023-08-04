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
     
    public class invoiceTypeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
   

}
