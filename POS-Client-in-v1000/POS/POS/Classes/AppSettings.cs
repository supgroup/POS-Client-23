using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace POS.Classes
{
    public class AppSettings
    {

        // app version
      static public string CurrentVersion
        {
            get
            {
                return ApplicationDeployment.IsNetworkDeployed
                       ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString()
                       : Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        //user specialize
        public static string lang;
        public static string menuIsOpen;
        public static string firstPath ;
        public static string secondPath ;
        public static int? firstPathId;
        public static int? secondPathId;
        public static string messageContent;
        public static string messageTitle;
        internal static int invoiceSlice;
        public static int DefaultInvoiceSlice = invoiceSlice;

        //general info
        internal static string accuracy;
        internal static string dateFormat;
        internal static string backupTime;
        internal static string backupDailyEnabled;
        internal static string Currency;
        internal static int CurrencyId;
        internal static int countryId;

        
        //default system info
        internal static string companyName;
        internal static string Address;
        internal static string com_name_ar;
        internal static string com_address_ar;

        internal static string Email;
        internal static string Mobile;
        internal static string Phone;
        internal static string Fax;
        internal static string logoImage;
        //social
        public static string com_website;
        public static string com_website_icon;
        public static string com_social;
        public static string com_social_icon;

        public static decimal PosBalance;
        public static decimal StorageCost;

        // invoices count for logged user
        public static int SalesDraftCount;
        public static int SalesOrdersDraftCount;
        public static int SalesWaitingOrdersCount;
        public static int QuotationsDraftCount;
        public static int PurchaseDraftCount;
        public static int PurchaseOrdersDraftCount;
        public static int ImExpDraftCount;
        public static int DirectStorageDraftCount;

        /////
        /// for reports
        public static string sale_copy_count;
        public static string pur_copy_count;
        public static string print_on_save_sale;
        public static string print_on_save_pur;
        public static string email_on_save_sale;
        public static string email_on_save_pur;
        public static string rep_printer_name;
        public static string sale_printer_name;
        public static string salePaperSize;
        public static string rep_print_count;
        public static string docPapersize;
        public static string Allow_print_inv_count;
        public static string show_header;
        public static string print_on_save_directentry;
        public static string directentry_copy_count;
        public static string itemtax_note;
        public static string sales_invoice_note;
        public static string Reportlang;
        public static string invoice_lang;
        

        //tax
        public static bool? invoiceTax_bool;
        public static decimal? invoiceTax_decimal;
        public static bool? itemsTax_bool;
        public static decimal? itemsTax_decimal;

        // invoiceItemsDetails
        public static bool canSkipProperties;
        public static bool canSkipSerialsNum;


        public static int returnPeriod;
        public static bool freeDelivery;



        public static string activationSite ;

        public static ProgramDetails progDetails ;

    }
}
