using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using POS.View.storage;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Resources;
using System.Reflection;

namespace POS.Classes
{

    class clsReports
    {
        public static int serialsCount { get; set; }
        public static void setReportLanguage(List<ReportParameter> paramarr)
        {

            paramarr.Add(new ReportParameter("lang", AppSettings.Reportlang));

        }
        public static void setInvoiceLanguage(List<ReportParameter> paramarr)
        {

            paramarr.Add(new ReportParameter("lang", AppSettings.invoice_lang));

        }

        public static void Header(List<ReportParameter> paramarr)
        {

            ReportCls rep = new ReportCls();



            if (AppSettings.Reportlang == "ar")
            {

                paramarr.Add(new ReportParameter("companyName", AppSettings.com_name_ar));
                paramarr.Add(new ReportParameter("Address", AppSettings.com_address_ar));
            }
            else
            {
                paramarr.Add(new ReportParameter("companyName", AppSettings.companyName));
                paramarr.Add(new ReportParameter("Address", AppSettings.Address));
            }
            paramarr.Add(new ReportParameter("Fax", AppSettings.Fax));
            paramarr.Add(new ReportParameter("Tel", AppSettings.Mobile));

            paramarr.Add(new ReportParameter("Email", AppSettings.Email));
            paramarr.Add(new ReportParameter("logoImage", "file:\\" + rep.GetLogoImagePath()));
            paramarr.Add(new ReportParameter("show_header", AppSettings.show_header));
            //trans
            paramarr.Add(new ReportParameter("trcomAddress", MainWindow.resourcemanagerreport.GetString("trAddress")));
            paramarr.Add(new ReportParameter("trcomTel", MainWindow.resourcemanagerreport.GetString("tel")));
            paramarr.Add(new ReportParameter("trcomFax", MainWindow.resourcemanagerreport.GetString("fax")));
            paramarr.Add(new ReportParameter("trcomEmail", MainWindow.resourcemanagerreport.GetString("email")));

        }
        public static void InvoiceHeader(List<ReportParameter> paramarr)
        {

            ReportCls rep = new ReportCls();
            // AppSettings.invoice_lang;
            if (AppSettings.invoice_lang == "en")
            {
                paramarr.Add(new ReportParameter("companyName", AppSettings.companyName));
                paramarr.Add(new ReportParameter("Address", AppSettings.Address));
            }
            else if (AppSettings.invoice_lang == "ar")
            {
                paramarr.Add(new ReportParameter("companyName", AppSettings.com_name_ar));
                paramarr.Add(new ReportParameter("Address", AppSettings.com_address_ar));
            }
            else
            {//both
                paramarr.Add(new ReportParameter("companyName", AppSettings.companyName));
                paramarr.Add(new ReportParameter("Address", AppSettings.Address));
                paramarr.Add(new ReportParameter("companyNameAr", AppSettings.com_name_ar));
                paramarr.Add(new ReportParameter("AddressAr", AppSettings.com_address_ar));
                //
                paramarr.Add(new ReportParameter("trcomAddressAr", MainWindow.resourcemanagerAr.GetString("trAddress")));
                paramarr.Add(new ReportParameter("trcomTelAr", MainWindow.resourcemanagerAr.GetString("tel")));
                paramarr.Add(new ReportParameter("trcomFaxAr", MainWindow.resourcemanagerAr.GetString("fax")));
                paramarr.Add(new ReportParameter("trcomEmailAr", MainWindow.resourcemanagerAr.GetString("email")));
            }
            paramarr.Add(new ReportParameter("trcomAddress", MainWindow.resourcemanagerreport.GetString("trAddress")));
            paramarr.Add(new ReportParameter("trcomTel", MainWindow.resourcemanagerreport.GetString("tel")));
            paramarr.Add(new ReportParameter("trcomFax", MainWindow.resourcemanagerreport.GetString("fax")));
            paramarr.Add(new ReportParameter("trcomEmail", MainWindow.resourcemanagerreport.GetString("email")));
            //
            paramarr.Add(new ReportParameter("Fax", AppSettings.Fax.Replace("--", "")));
            paramarr.Add(new ReportParameter("Tel", AppSettings.Phone.Replace("--", "")));

            paramarr.Add(new ReportParameter("Email", AppSettings.Email));
            paramarr.Add(new ReportParameter("logoImage", "file:\\" + rep.GetLogoImagePath()));
            //social
            string iconname = AppSettings.logoImage;//temp value
            paramarr.Add(new ReportParameter("com_tel_icon", "file:\\" + rep.GetIconImagePath("phone")));
            paramarr.Add(new ReportParameter("com_fax_icon", "file:\\" + rep.GetIconImagePath("fax")));
            paramarr.Add(new ReportParameter("com_social_icon", "file:\\" + rep.GetIconImagePath(AppSettings.com_social_icon)));
            paramarr.Add(new ReportParameter("com_social", AppSettings.com_social));
            paramarr.Add(new ReportParameter("com_website_icon", "file:\\" + rep.GetIconImagePath("website")));
            paramarr.Add(new ReportParameter("com_website", AppSettings.com_website));             
            paramarr.Add(new ReportParameter("com_email_icon", "file:\\" + rep.GetIconImagePath("email")));

            paramarr.Add(new ReportParameter("show_header", AppSettings.show_header));
            paramarr.Add(new ReportParameter("com_mobile", AppSettings.Mobile.Replace("--", "")));
            paramarr.Add(new ReportParameter("com_mobile_icon", "file:\\" + rep.GetIconImagePath("mobile")));
        }
        public static void HeaderNoLogo(List<ReportParameter> paramarr)
        {

            ReportCls rep = new ReportCls();
            if (AppSettings.Reportlang == "ar")
            {

                paramarr.Add(new ReportParameter("companyName", AppSettings.com_name_ar));
                paramarr.Add(new ReportParameter("Address", AppSettings.com_address_ar));
            }
            else
            {
                paramarr.Add(new ReportParameter("companyName", AppSettings.companyName));
                paramarr.Add(new ReportParameter("Address", AppSettings.Address));
            }

            paramarr.Add(new ReportParameter("Fax", AppSettings.Fax));
            paramarr.Add(new ReportParameter("Tel", AppSettings.Mobile));

            paramarr.Add(new ReportParameter("Email", AppSettings.Email));
            paramarr.Add(new ReportParameter("show_header", AppSettings.show_header));

            paramarr.Add(new ReportParameter("trcomAddress", MainWindow.resourcemanagerreport.GetString("trAddress")));
            paramarr.Add(new ReportParameter("trcomTel", MainWindow.resourcemanagerreport.GetString("tel")));
            paramarr.Add(new ReportParameter("trcomFax", MainWindow.resourcemanagerreport.GetString("fax")));
            paramarr.Add(new ReportParameter("trcomEmail", MainWindow.resourcemanagerreport.GetString("email")));

        }
        public static void bankdg(List<ReportParameter> paramarr)
        {

            ReportCls rep = new ReportCls();
            paramarr.Add(new ReportParameter("trTransferNumber", MainWindow.resourcemanagerreport.GetString("trTransferNumberTooltip")));


        }
        public static void bondsReport(IEnumerable<Bonds> bondsQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            paramarr.Add(new ReportParameter("trDocNumTooltip", MainWindow.resourcemanagerreport.GetString("trDocNumTooltip")));
            paramarr.Add(new ReportParameter("trRecipientTooltip", MainWindow.resourcemanagerreport.GetString("trRecipientTooltip")));

            paramarr.Add(new ReportParameter("trPaymentTypeTooltip", MainWindow.resourcemanagerreport.GetString("trPaymentTypeTooltip")));

            paramarr.Add(new ReportParameter("trDocDateTooltip", MainWindow.resourcemanagerreport.GetString("trDocDateTooltip")));

            paramarr.Add(new ReportParameter("trPayDate", MainWindow.resourcemanagerreport.GetString("trPayDate")));
            paramarr.Add(new ReportParameter("trCashTooltip", MainWindow.resourcemanagerreport.GetString("trCashTooltip")));

            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trBonds")));
            foreach (var c in bondsQuery)
            {

                c.amount = decimal.Parse(SectionData.DecTostring(c.amount));
            }
            rep.DataSources.Add(new ReportDataSource("DataSetBond", bondsQuery));

            DateFormConv(paramarr);
            AccountSideConv(paramarr);
            cashTransTypeConv(paramarr);

        }

        public static void bondsDocReport(LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();



            DateFormConv(paramarr);


        }
        //public static void orderReport(IEnumerable<Invoice> invoiceQuery, LocalReport rep, string reppath)
        //{
        //    rep.ReportPath = reppath;
        //    rep.EnableExternalImages = true;
        //    rep.DataSources.Clear();
        //    foreach(var o in invoiceQuery)
        //    {
        //        string status = "";
        //        switch (o.status)
        //        {
        //            case "tr":
        //                status = MainWindow.resourcemanager.GetString("trDelivered");
        //                break;
        //            case "rc":
        //                status = MainWindow.resourcemanager.GetString("trInDelivery");
        //                break;
        //            default:
        //                status = "";
        //                break;
        //        }
        //        o.status = status;
        //        o.deserved = decimal.Parse(SectionData.DecTostring(o.deserved));
        //    }
        //    rep.DataSources.Add(new ReportDataSource("DataSetInvoice", invoiceQuery));
        //}
        public static void orderReport(IEnumerable<Invoice> invoiceQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            foreach (var o in invoiceQuery)
            {
                o.deserved = decimal.Parse(SectionData.DecTostring(o.deserved));
                o.payStatus = invoicePayStatusConvert(o.payStatus);
            }
            DeliverStateConv(paramarr);

            paramarr.Add(new ReportParameter("trInvoiceNumber", MainWindow.resourcemanagerreport.GetString("trInvoiceNumber")));
            paramarr.Add(new ReportParameter("trSalesMan", MainWindow.resourcemanagerreport.GetString("trSalesMan")));
            paramarr.Add(new ReportParameter("trCustomer", MainWindow.resourcemanagerreport.GetString("trCustomer")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trCashTooltip", MainWindow.resourcemanagerreport.GetString("trCashTooltip")));
            paramarr.Add(new ReportParameter("trState", MainWindow.resourcemanagerreport.GetString("trState")));

            DateFormConv(paramarr);


            rep.DataSources.Add(new ReportDataSource("DataSetInvoice", invoiceQuery));
        }


        public static string invoicePayStatusConvert(string payStatus)
        {

            switch (payStatus)
            {
                case "payed": return MainWindow.resourcemanagerreport.GetString("trPaid_");

                case "unpayed": return MainWindow.resourcemanagerreport.GetString("trUnPaid");

                case "partpayed": return MainWindow.resourcemanagerreport.GetString("trCredit");

                default: return "";

            }
        }
        public static void DeliverStateConv(List<ReportParameter> paramarr)
        {
            paramarr.Add(new ReportParameter("trDelivered", MainWindow.resourcemanagerreport.GetString("trDelivered")));
            paramarr.Add(new ReportParameter("trInDelivery", MainWindow.resourcemanagerreport.GetString("trInDelivery")));

        }

        public static void bankAccReport(IEnumerable<CashTransfer> cash, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {



            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            paramarr.Add(new ReportParameter("trTransferNumberTooltip", MainWindow.resourcemanagerreport.GetString("trTransferNumberTooltip")));
            paramarr.Add(new ReportParameter("trBank", MainWindow.resourcemanagerreport.GetString("trBank")));
            paramarr.Add(new ReportParameter("trDepositeNumTooltip", MainWindow.resourcemanagerreport.GetString("trDepositeNumTooltip")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trCashTooltip", MainWindow.resourcemanagerreport.GetString("trCashTooltip")));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trBanks")));
            DateFormConv(paramarr);
            foreach (var c in cash)
            {
                ///////////////////
                c.cash = decimal.Parse(SectionData.DecTostring(c.cash));
                string s;
                switch (c.processType)
                {
                    case "cash":
                        s = MainWindow.resourcemanagerreport.GetString("trCash");
                        break;
                    case "doc":
                        s = MainWindow.resourcemanagerreport.GetString("trDocument");
                        break;
                    case "cheque":
                        s = MainWindow.resourcemanagerreport.GetString("trCheque");
                        break;
                    case "balance":
                        s = MainWindow.resourcemanagerreport.GetString("trCredit");
                        break;
                    case "card":
                        s = MainWindow.resourcemanagerreport.GetString("trCreditCard");
                        break;
                    default:
                        s = c.processType;
                        break;
                }
                ///////////////////
                c.processType = s;
                string name = "";
                switch (c.side)
                {
                    case "bnd": break;
                    case "v": name = MainWindow.resourcemanagerreport.GetString("trVendor"); break;
                    case "c": name = MainWindow.resourcemanagerreport.GetString("trCustomer"); break;
                    case "u": name = MainWindow.resourcemanagerreport.GetString("trUser"); break;
                    case "s": name = MainWindow.resourcemanagerreport.GetString("trSalary"); break;

                    case "e": name = MainWindow.resourcemanagerreport.GetString("trGeneralExpenses"); break;
                    case "m":
                        if (c.transType == "d")
                            name = MainWindow.resourcemanagerreport.GetString("trAdministrativeDeposit");
                        if (c.transType == "p")
                            name = MainWindow.resourcemanagerreport.GetString("trAdministrativePull");
                        break;
                    case "sh": name = MainWindow.resourcemanagerreport.GetString("trShippingCompany"); break;
                    case "tax": name = MainWindow.resourcemanager.GetString("trTaxCollection"); break;
                    default: break;
                }
                string fullName = "";
                if (!string.IsNullOrEmpty(c.agentName))
                    fullName = name + " " + c.agentName;
                else if (!string.IsNullOrEmpty(c.usersLName))
                    fullName = name + " " + c.usersLName;
                else if (!string.IsNullOrEmpty(c.shippingCompanyName))
                    fullName = name + " " + c.shippingCompanyName;
                else
                    fullName = name;
                /////////////////////
                c.side = fullName;

                string type;
                if (c.transType.Equals("p")) type = MainWindow.resourcemanagerreport.GetString("trPull");
                else type = MainWindow.resourcemanagerreport.GetString("trDeposit");
                ////////////////////
                c.transType = type;
            }
            rep.DataSources.Add(new ReportDataSource("DataSetBankAcc", cash));
        }

        public static void paymentAccReport(IEnumerable<CashTransfer> query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<CashTransfer> cash = JsonConvert.DeserializeObject<List<CashTransfer>>(JsonConvert.SerializeObject(query));

            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            //foreach (var c in cash)
            //{
            //    ///////////////////
            //    c.cash = decimal.Parse(SectionData.DecTostring(c.cash));
            //    string s;
            //    switch (c.processType)
            //    {
            //        case "cash":
            //            s = MainWindow.resourcemanagerreport.GetString("trCash");
            //            break;
            //        case "doc":
            //            s = MainWindow.resourcemanagerreport.GetString("trDocument");
            //            break;
            //        case "cheque":
            //            s = MainWindow.resourcemanagerreport.GetString("trCheque");
            //            break;
            //        case "balance":
            //            s = MainWindow.resourcemanagerreport.GetString("trCredit");
            //            break;
            //        default:
            //            s = c.processType;
            //            break;
            //    }


            //}


            AccountSideConv(paramarr);

            cashTransTypeConv(paramarr);
         //   cashTransferProcessTypeConv(paramarr);

            paramarr.Add(new ReportParameter("trTransferNumberTooltip", MainWindow.resourcemanagerreport.GetString("trTransferNumberTooltip")));
            paramarr.Add(new ReportParameter("trRecepient", MainWindow.resourcemanagerreport.GetString("trRecepient")));
            paramarr.Add(new ReportParameter("trPaymentTypeTooltip", MainWindow.resourcemanagerreport.GetString("trPaymentTypeTooltip")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trCashTooltip", MainWindow.resourcemanagerreport.GetString("trCashTooltip")));
            paramarr.Add(new ReportParameter("accuracy", AppSettings.accuracy));
            paramarr.Add(new ReportParameter("trUnKnown", MainWindow.resourcemanagerreport.GetString("trUnKnown")));
            paramarr.Add(new ReportParameter("trCashCustomer", MainWindow.resourcemanagerreport.GetString("trCashCustomer")));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trPayments")));

            DateFormConv(paramarr);


            foreach (var c in cash)
            {
                c.notes = depositorNameConverter(c);
                c.cash = decimal.Parse(SectionData.DecTostring(c.cash));
                // c.notes = SectionData.DecTostring(c.cash);
                c.agentName = AgentUnKnownConvert(c.agentId, c.side, c.agentName);
                c.processType = processTypeConvswitch(c.processType, c.cardName);
            }
            rep.DataSources.Add(new ReportDataSource("DataSetBankAcc", cash));
        }
        public static string processTypeConvswitch(string processType, string cardName)
        {
         

            switch (processType)
            {
                case "cash": return MainWindow.resourcemanagerreport.GetString("trCash");
                //break;
                case "doc": return MainWindow.resourcemanagerreport.GetString("trDocument");
                //break;
                case "cheque": return MainWindow.resourcemanagerreport.GetString("trCheque");
                //break;
                case "balance": return MainWindow.resourcemanagerreport.GetString("trCredit");
                //break;
                case "card": return cardName;
                //break;
                case "inv": return MainWindow.resourcemanagerreport.GetString("trInv");
                
                //break;
                default: return processType;
                    //break;
            }
        }
        public static string depositorNameConverter(CashTransfer s)
        {
            try
            {
                //CashTransfer s = value as CashTransfer;
                string name = "";
                switch (s.side)
                {
                    case "bnd": break;
                    case "v": name = MainWindow.resourcemanagerreport.GetString("trVendor"); break;
                    case "c": name = MainWindow.resourcemanagerreport.GetString("trCustomer"); break;
                    case "u": name = MainWindow.resourcemanagerreport.GetString("trUser"); break;
                    case "s": name = MainWindow.resourcemanagerreport.GetString("trSalary"); break;
                    case "e": name = MainWindow.resourcemanagerreport.GetString("trGeneralExpenses"); break;
                    case "m":
                        if (s.transType == "d")
                            name = MainWindow.resourcemanagerreport.GetString("trAdministrativeDeposit");
                        if (s.transType == "p")
                            name = MainWindow.resourcemanagerreport.GetString("trAdministrativePull");
                        break;
                    case "sh": name = MainWindow.resourcemanagerreport.GetString("trShippingCompany"); break;
                    case "shd": name = MainWindow.resourcemanagerreport.GetString("trDeliveryCharges"); break;
                    case "tax": name = MainWindow.resourcemanagerreport.GetString("trTaxCollection"); break;
                    default: break;
                }

                if (!string.IsNullOrEmpty(s.agentName))
                    name = name + " " + s.agentName;
                else if (!string.IsNullOrEmpty(s.usersName) && !string.IsNullOrEmpty(s.usersLName))
                {
                    if (s.processType.Equals("commissionAgent"))
                        name = MainWindow.resourcemanagerreport.GetString("commission_") + " " + MainWindow.resourcemanagerreport.GetString("for") + " " + name + " " + s.usersName + " " + s.usersLName;
                    else
                        name = name + " " + s.usersName + " " + s.usersLName;
                }
                else if (!string.IsNullOrEmpty(s.shippingCompanyName))
                    name = name + " " + s.shippingCompanyName;
                else if ((s.side != "e") && (s.side != "m") && (s.side != "tax"))
                    name = name + " " + MainWindow.resourcemanagerreport.GetString("trUnKnown");

                return name;

            }
            catch
            {
                return "";
            }
        }
        public static void receivedAccReport(IEnumerable<CashTransfer> query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<CashTransfer> cash = JsonConvert.DeserializeObject<List<CashTransfer>>(JsonConvert.SerializeObject(query));

            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            foreach (var c in cash)
            {
                c.notes = depositorNameConverter(c);
                c.cash = decimal.Parse(SectionData.DecTostring(c.cash));
                c.processType = processTypeConvswitch(c.processType, c.cardName);
            }
            DateFormConv(paramarr);
            AccountSideConv(paramarr);

            cashTransTypeConv(paramarr);
          //  cashTransferProcessTypeConv(paramarr)

            paramarr.Add(new ReportParameter("trTransferNumberTooltip", MainWindow.resourcemanagerreport.GetString("trTransferNumberTooltip")));
            paramarr.Add(new ReportParameter("trDepositor", MainWindow.resourcemanagerreport.GetString("trDepositor")));
            paramarr.Add(new ReportParameter("trPaymentTypeTooltip", MainWindow.resourcemanagerreport.GetString("trPaymentTypeTooltip")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trCashTooltip", MainWindow.resourcemanagerreport.GetString("trCashTooltip")));
            paramarr.Add(new ReportParameter("accuracy", AppSettings.accuracy));
            paramarr.Add(new ReportParameter("trUnKnown", MainWindow.resourcemanagerreport.GetString("trUnKnown")));
            paramarr.Add(new ReportParameter("trCashCustomer", MainWindow.resourcemanagerreport.GetString("trCashCustomer")));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trReceived")));

            rep.DataSources.Add(new ReportDataSource("DataSetBankAcc", cash));
        }

        public static void posAccReport(IEnumerable<CashTransfer> cash, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var c in cash)
            {

                c.cash = decimal.Parse(SectionData.DecTostring(c.cash));
            }

            paramarr.Add(new ReportParameter("trTransferNumberTooltip", MainWindow.resourcemanagerreport.GetString("trTransferNumberTooltip")));
            paramarr.Add(new ReportParameter("trCreator", MainWindow.resourcemanagerreport.GetString("trCreator")));
            paramarr.Add(new ReportParameter("trStatus", MainWindow.resourcemanagerreport.GetString("trStatus")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trCashTooltip", MainWindow.resourcemanagerreport.GetString("trCashTooltip")));
            paramarr.Add(new ReportParameter("trConfirmed", MainWindow.resourcemanagerreport.GetString("trConfirmed")));
            paramarr.Add(new ReportParameter("trCanceled", MainWindow.resourcemanagerreport.GetString("trCanceled")));
            paramarr.Add(new ReportParameter("trWaiting", MainWindow.resourcemanagerreport.GetString("trWaiting")));

            DateFormConv(paramarr);

            rep.DataSources.Add(new ReportDataSource("DataSetBankAcc", cash));
        }

        public static void posAccReportSTS(IEnumerable<CashTransfer> cash, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<CashTransfer> query = JsonConvert.DeserializeObject<List<CashTransfer>>(JsonConvert.SerializeObject(cash));
            posAccReport(query, rep, reppath, paramarr);
            paramarr.Add(new ReportParameter("trNum", MainWindow.resourcemanagerreport.GetString("trNo")));

            paramarr.Add(new ReportParameter("trAccoutant", MainWindow.resourcemanagerreport.GetString("trAccoutant")));
            paramarr.Add(new ReportParameter("trAmount", MainWindow.resourcemanagerreport.GetString("trAmount")));

            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));

        }
        public string posTransfersStatusConverter(byte isConfirm1, byte isConfirm2)
        {

            if ((isConfirm1 == 1) && (isConfirm2 == 1))
                return MainWindow.resourcemanager.GetString("trConfirmed");
            else if ((isConfirm1 == 2) || (isConfirm2 == 2))
                return MainWindow.resourcemanager.GetString("trCanceled");
            else
                return MainWindow.resourcemanager.GetString("trWaiting");
        }
        public static void invItem(IEnumerable<InventoryItemLocation> itemLocations, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
           
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
       
            rep.DataSources.Add(new ReportDataSource("DataSetInvItemLocation", itemLocations));
            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));
        }
        public static void invItemShortage(IEnumerable<InventoryItemLocation> itemLocations, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<InventoryItemLocation> query = JsonConvert.DeserializeObject<List<InventoryItemLocation>>(JsonConvert.SerializeObject(itemLocations));

            foreach (InventoryItemLocation row in query)
            {
                row.total = decimal.Parse(SectionData.DecTostring(row.total));
            }
            invItem(query, rep, reppath, paramarr);
           
            List<InventoryItemLocation> queryempty = new List<InventoryItemLocation>();
            rep.DataSources.Add(new ReportDataSource("DataSetItemTransferInvoice", queryempty));
        }
        public static void invItemShortagedaily(IEnumerable<ItemTransferInvoice> itemLocations, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<ItemTransferInvoice> query = JsonConvert.DeserializeObject<List<ItemTransferInvoice>>(JsonConvert.SerializeObject(itemLocations));
          
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (ItemTransferInvoice row in query)
            {
                row.total = decimal.Parse(SectionData.DecTostring(row.total));
            }
            List<InventoryItemLocation> queryempty = new List<InventoryItemLocation>();
            rep.DataSources.Add(new ReportDataSource("DataSetInvItemLocation", queryempty));
            rep.DataSources.Add(new ReportDataSource("DataSetItemTransferInvoice", query));
            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));

        }
        
        public static void section(IEnumerable<Section> sections, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetSection", sections));
        }
        public static void location(IEnumerable<Location> locations, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetLocation", locations));
        }
        public static void itemLocation(IEnumerable<ItemLocation> itemLocations, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetItemLocation", itemLocations));
            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));
        }
        public static void bankReport(IEnumerable<Bank> banksQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            paramarr.Add(new ReportParameter("trBankName", MainWindow.resourcemanagerreport.GetString("trBankName")));
            paramarr.Add(new ReportParameter("trAccNumber", MainWindow.resourcemanagerreport.GetString("trAccNumber")));
            paramarr.Add(new ReportParameter("trAddress", MainWindow.resourcemanagerreport.GetString("trAddress")));
            paramarr.Add(new ReportParameter("trMobile", MainWindow.resourcemanagerreport.GetString("trMobile")));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trBanks")));



            rep.DataSources.Add(new ReportDataSource("DataSetBank", banksQuery));

        }
        public static void ErrorsReport(IEnumerable<ErrorClass> Query, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetError", Query));
        }

        public static void couponReport(IEnumerable<Coupon> CouponQuery2, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {

            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var c in CouponQuery2)
            {

                c.discountValue = decimal.Parse(DiscountConvert(c.discountType.ToString(), c.discountValue));

                c.invMin = decimal.Parse(SectionData.DecTostring(c.invMin));
                c.invMax = decimal.Parse(SectionData.DecTostring(c.invMax));

                string state = "";

                if ((c.isActive == 1) && ((c.endDate > DateTime.Now) || (c.endDate == null)) && ((c.quantity == 0) || (c.quantity > 0 && c.remainQ != 0)))
                    state = MainWindow.resourcemanager.GetString("trValid");
                else
                    state = MainWindow.resourcemanager.GetString("trExpired");

                c.state = state;

            }

            rep.DataSources.Add(new ReportDataSource("DataSetCoupon", CouponQuery2));
            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trCoupon")));
            paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trCode")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trDiscount", MainWindow.resourcemanagerreport.GetString("trValue")));
            paramarr.Add(new ReportParameter("trQuantity", MainWindow.resourcemanagerreport.GetString("trQuantity")));
            paramarr.Add(new ReportParameter("trRemainQ", MainWindow.resourcemanagerreport.GetString("trRemainQuantity")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trvalidity")));
            paramarr.Add(new ReportParameter("trUnlimited", MainWindow.resourcemanagerreport.GetString("trUnlimited")));
        }
        public static void couponExportReport(LocalReport rep, string reppath, List<ReportParameter> paramarr, string barcode)
        {

            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            ReportCls repcls = new ReportCls();


            paramarr.Add(new ReportParameter("invNumber", barcode));
            paramarr.Add(new ReportParameter("barcodeimage", "file:\\" + repcls.BarcodeToImage(barcode, "barcode")));

        }
        public static void chartExportReport(LocalReport rep, string reppath, List<ReportParameter> paramarr, string imgPath)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            //  paramarr.Add(new ReportParameter("invNumber", barcode));
            paramarr.Add(new ReportParameter("imagepath", "file:\\" + imgPath));

        }
        public static void packageReport(IEnumerable<Item> packageQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();


            rep.DataSources.Add(new ReportDataSource("DataSetItem", packageQuery));
            //    paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trPackageItems")));
            paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trCode")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trPackage")));
            paramarr.Add(new ReportParameter("trDetails", MainWindow.resourcemanagerreport.GetString("trDetails")));
            paramarr.Add(new ReportParameter("trCategory", MainWindow.resourcemanagerreport.GetString("trCategorie")));

        }
        public static void serviceReport(IEnumerable<Item> serviceQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();


            rep.DataSources.Add(new ReportDataSource("DataSetItem", serviceQuery));
            //    paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));trTheService trTheServices
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trTheServices")));
            paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trCode")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trTheService")));
            paramarr.Add(new ReportParameter("trDetails", MainWindow.resourcemanagerreport.GetString("trDetails")));
            paramarr.Add(new ReportParameter("trCategory", MainWindow.resourcemanagerreport.GetString("trCategorie")));

        }
        public static void offerReport(IEnumerable<Offer> OfferQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var o in OfferQuery)
            {


                o.discountValue = decimal.Parse(DiscountConvert(o.discountType.ToString(), o.discountValue));
            }

            rep.DataSources.Add(new ReportDataSource("DataSetOffer", OfferQuery));
            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));
            paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trCode")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trDiscount", MainWindow.resourcemanagerreport.GetString("trValue")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trOffer")));


        }
        public static void cardReport(IEnumerable<Card> cardsQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            List<Card> query = JsonConvert.DeserializeObject<List<Card>>(JsonConvert.SerializeObject(cardsQuery));
            foreach (Card row in query)
            {
                row.commissionValue = decimal.Parse(SectionData.DecTostring(row.commissionValue));
                row.commissionRatio = decimal.Parse(SectionData.PercentageDecTostring(row.commissionRatio));

            }
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trPaymentMethods")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trNotes", MainWindow.resourcemanagerreport.GetString("trNote")));



            paramarr.Add(new ReportParameter("trcommissionValue", MainWindow.resourcemanagerreport.GetString("commissionValue")));
            paramarr.Add(new ReportParameter("trcommissionRatio", MainWindow.resourcemanagerreport.GetString("commissionRatio")));
            rep.DataSources.Add(new ReportDataSource("DataSetCard", query));
        }

        public static void shippingReport(IEnumerable<ShippingCompanies> shippingCompanies, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {


            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var r in shippingCompanies)
            {
                r.RealDeliveryCost = decimal.Parse(SectionData.DecTostring(r.RealDeliveryCost));
                r.deliveryCost = decimal.Parse(SectionData.DecTostring(r.deliveryCost));
                r.deliveryType = deliveryTypeConvert(r.deliveryType);
            }
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trRealDeliveryCost", MainWindow.resourcemanagerreport.GetString("trRealDeliveryCost")));
            paramarr.Add(new ReportParameter("trDeliveryCost", MainWindow.resourcemanagerreport.GetString("trDeliveryCost")));
            paramarr.Add(new ReportParameter("trDeliveryType", MainWindow.resourcemanagerreport.GetString("trDeliveryType")));
            paramarr.Add(new ReportParameter("trTitle", MainWindow.resourcemanagerreport.GetString("trShippingCompanies")));

            paramarr.Add(new ReportParameter("trNoData", MainWindow.resourcemanagerreport.GetString("thereArenodata")));

            rep.DataSources.Add(new ReportDataSource("DataSetShipping", shippingCompanies));

        }
        public static string deliveryTypeConvert(string deliveryType)
        {
            switch (deliveryType)
            {
                case "local": return MainWindow.resourcemanagerreport.GetString("trLocaly");
                //break;
                case "com": return MainWindow.resourcemanagerreport.GetString("trShippingCompany");
                //break;
                default: return MainWindow.resourcemanagerreport.GetString("");
                    //break;
            }

        }
        public static string itemTypeConverter(string value)
        {
            string s = "";
            switch (value)
            {
                case "n": s = MainWindow.resourcemanagerreport.GetString("trNormals"); break;
                case "d": s = MainWindow.resourcemanagerreport.GetString("trHaveExpirationDates"); break;
                case "sn": s = MainWindow.resourcemanagerreport.GetString("trHaveSerialNumbers"); break;
                case "sr": s = MainWindow.resourcemanagerreport.GetString("trServices"); break;
                case "p": s = MainWindow.resourcemanagerreport.GetString("trPackages"); break;
            }

            return s;
        }
        public static string BranchStoreConverter(string type, string lang)
        {
            string s = "";
            if (lang == "both")
            {
                switch (type)
                {
                    case "b": s = MainWindow.resourcemanagerAr.GetString("tr_Branch"); break;
                    case "s": s = MainWindow.resourcemanagerAr.GetString("tr_Store"); break;

                }
            }
            else
            {
                switch (type)
                {
                    case "b": s = MainWindow.resourcemanagerreport.GetString("tr_Branch"); break;
                    case "s": s = MainWindow.resourcemanagerreport.GetString("tr_Store"); break;

                }
            }


            return s;
        }
        public static void PurStsReport(IEnumerable<ItemTransferInvoice> tempquery, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var r in tempquery)
            {
                //   r.CopdiscountValue = decimal.Parse(SectionData.DecTostring(r.CopdiscountValue));
                r.CopdiscountValue = decimal.Parse(DiscountConvert(r.CopdiscountType.ToString(), r.CopdiscountValue));
                r.couponTotalValue = decimal.Parse(SectionData.DecTostring(r.couponTotalValue));//
                                                                                                //  r.OdiscountValue = decimal.Parse(SectionData.DecTostring(r.OdiscountValue));
                r.OdiscountValue = decimal.Parse(DiscountConvert(r.OdiscountType, r.OdiscountValue));


                r.offerTotalValue = decimal.Parse(SectionData.DecTostring(r.offerTotalValue));
                r.ITprice = decimal.Parse(SectionData.DecTostring(r.ITprice));
                r.price = decimal.Parse(SectionData.DecTostring(r.price));
                r.subTotal = decimal.Parse(SectionData.DecTostring(r.subTotal));
                r.totalNet = decimal.Parse(SectionData.DecTostring(r.totalNet));
                r.discountValue = decimal.Parse(DiscountConvert(r.discountType, r.discountValue));
                r.tax = decimal.Parse(SectionData.PercentageDecTostring(r.tax));
                if (r.itemAvg != null)
                {
                    // r.itemAvg = double.Parse(SectionData.DecTostring(decimal.Parse(r.itemAvg.ToString())));
                    r.ITnotes = SectionData.DecTostring(decimal.Parse(r.itemAvg.ToString()));
                    r.itemAvg = double.Parse(r.ITnotes);
                }

            }

            rep.DataSources.Add(new ReportDataSource("DataSetITinvoice", tempquery));
        }
        public CashTransfer JsonCashTransfer(CashTransferSts item)
        {
            CashTransfer tempquery = JsonConvert.DeserializeObject<CashTransfer>(JsonConvert.SerializeObject(item));
            return tempquery;
        }
        public static void PurItemCostStsReport(IEnumerable<ItemUnitCost> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<ItemUnitCost> tempquery = JsonConvert.DeserializeObject<List<ItemUnitCost>>(JsonConvert.SerializeObject(Query));

            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            ReportCls repcls = new ReportCls();
            foreach (var r in tempquery)
            {
                r.cost = decimal.Parse(repcls.DecTostring(r.cost));
                r.avgPurchasePrice = decimal.Parse(repcls.DecTostring(r.avgPurchasePrice));
                r.diffPercent = decimal.Parse(SectionData.PercentageDecTostring(r.diffPercent));

            }
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trEntryCost", MainWindow.resourcemanagerreport.GetString("trEntryCost")));
            paramarr.Add(new ReportParameter("trRealCost", MainWindow.resourcemanagerreport.GetString("trRealCost")));
            paramarr.Add(new ReportParameter("trCostPercentage", MainWindow.resourcemanagerreport.GetString("trCostPercentage")));

            rep.DataSources.Add(new ReportDataSource("DataSet", tempquery));
        }
        public static string DiscountConvert(string type, decimal? value)
        {
            if (value != null)
            {
                decimal num = (decimal)value;
                string s = num.ToString();

                switch (AppSettings.accuracy)
                {
                    case "0":
                        s = string.Format("{0:F0}", num);
                        break;
                    case "1":
                        s = string.Format("{0:F1}", num);
                        break;
                    case "2":
                        s = string.Format("{0:F2}", num);
                        break;
                    case "3":
                        s = string.Format("{0:F3}", num);
                        break;
                    default:
                        s = string.Format("{0:F1}", num);
                        break;
                }


                if (type == "2")
                {
                    string sdc = string.Format("{0:G29}", decimal.Parse(s));
                    return sdc;
                }
                else
                {

                    return s;
                }
            }
            else
            {
                return "0";
            }

        }
        public static void saleitemStsReport(IEnumerable<ItemTransferInvoice> tempquery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {

            itemTypeConv(paramarr);
            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));
            PurStsReport(tempquery, rep, reppath);
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
        }

        public static void SalePromoStsReport(IEnumerable<ItemTransferInvoice> tempquery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            PurStsReport(tempquery, rep, reppath);
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            itemTransferDiscountTypeConv(paramarr);
            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));

            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            /*
             =IIF(Fields!CopdiscountType.Value="2",
Parameters!trPercentageDiscount.Value,
Parameters!trValueDiscount.Value)
             * */
        }
        public static void itemTransferDiscountTypeConv(List<ReportParameter> paramarr)
        {

            paramarr.Add(new ReportParameter("trValueDiscount", MainWindow.resourcemanagerreport.GetString("trValueDiscount")));
            paramarr.Add(new ReportParameter("trPercentageDiscount", MainWindow.resourcemanagerreport.GetString("trPercentageDiscount")));


        }

        public static void itemTypeConv(List<ReportParameter> paramarr)
        {
            paramarr.Add(new ReportParameter("trNormal", MainWindow.resourcemanagerreport.GetString("trNormal")));
            paramarr.Add(new ReportParameter("trHaveExpirationDate", MainWindow.resourcemanagerreport.GetString("trHaveExpirationDate")));
            paramarr.Add(new ReportParameter("trHaveSerialNumber", MainWindow.resourcemanagerreport.GetString("trHaveSerialNumber")));
            paramarr.Add(new ReportParameter("trService", MainWindow.resourcemanagerreport.GetString("trService")));
            paramarr.Add(new ReportParameter("trPackage", MainWindow.resourcemanagerreport.GetString("trPackage")));
        }
        //clsReports.SaleInvoiceStsReport(itemTransfers, rep, reppath, paramarr);

        public static void SaleInvoiceStsReport(IEnumerable<ItemTransferInvoice> tempquery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {

            PurStsReport(tempquery, rep, reppath);
            paramarr.Add(new ReportParameter("isTax", AppSettings.invoiceTax_bool.ToString()));
            itemTransferInvTypeConv(paramarr);
            paramarr.Add(new ReportParameter("isTax", AppSettings.invoiceTax_bool.ToString()));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            paramarr.Add(new ReportParameter("trRefNo", MainWindow.resourcemanagerreport.GetString("trRefNo.")));
        }
        public static void SaleOrderStsReport(IEnumerable<ItemTransferInvoice> tempquery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {

            PurStsReport(tempquery, rep, reppath);
            paramarr.Add(new ReportParameter("isTax", AppSettings.invoiceTax_bool.ToString()));
            itemTransferInvTypeConv(paramarr);
            paramarr.Add(new ReportParameter("isTax", AppSettings.invoiceTax_bool.ToString()));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));

        }
        public async static Task SaleInvoicePaymentReport(IEnumerable<ItemTransferInvoice> tempquery, LocalReport rep, string reppath, List<ReportParameter> paramarr, int selectedTab, string cb_paymentsValue, string cb_cardValue
             , bool? rad_invoice, bool? rad_return, bool? rad_draft
            )
        {
            List<CardsSts> cardtransList = new List<CardsSts>();
            tempquery = await invoicepayment(tempquery, paramarr, selectedTab, cb_paymentsValue, cb_cardValue, cardtransList
                , rad_invoice, rad_return, rad_draft
                );

            PurStsReport(tempquery, rep, reppath);
            paramarr.Add(new ReportParameter("invoiceClass", MainWindow.resourcemanagerreport.GetString("invoiceClass")));
            paramarr.Add(new ReportParameter("isTax", AppSettings.invoiceTax_bool.ToString()));
            itemTransferInvTypeConv(paramarr);
            //  paramarr.Add(new ReportParameter("isTax", AppSettings.invoiceTax_bool.ToString()));
            // paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            paramarr.Add(new ReportParameter("trRefNo", MainWindow.resourcemanagerreport.GetString("trRefNo.")));

            paramarr.Add(new ReportParameter("trPaymentValue", MainWindow.resourcemanagerreport.GetString("trPaymentValue")));
            paramarr.Add(new ReportParameter("trPaymentType", MainWindow.resourcemanagerreport.GetString("trPaymentType")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));

            paramarr.Add(new ReportParameter("trCredit", MainWindow.resourcemanagerreport.GetString("trCredit")));
            //   paramarr.Add(new ReportParameter("totalValue", MainWindow.resourcemanagerreport.GetString("trPaymentValue")));
            paramarr.Add(new ReportParameter("trPaymentMethodsheader", MainWindow.resourcemanagerreport.GetString("trPaymentMethods")));
            paramarr.Add(new ReportParameter("trCash", MainWindow.resourcemanagerreport.GetString("trCash")));

            rep.DataSources.Add(new ReportDataSource("CardsSts", cardtransList));

        }
        public static string stackToString(StackPanel stackP)
        {
            string selecteditems = "";
            if (stackP.Children.Count > 0)
            {
                foreach (MaterialDesignThemes.Wpf.Chip c in stackP.Children)
                {
                    selecteditems += c.Content.ToString() + " , ";
                }
                selecteditems = selecteditems.Remove(selecteditems.Length - 2);

                return selecteditems;
            }
            else
            {
                return MainWindow.resourcemanagerreport.GetString("trAll");
            }



        }


        public static void SaledailyReport(IEnumerable<ItemTransferInvoice> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<ItemTransferInvoice> tempquery = JsonConvert.DeserializeObject<List<ItemTransferInvoice>>(JsonConvert.SerializeObject(Query));

            decimal sumCash = 0;
            decimal sumCredit = 0;
            decimal sumVisa = 0;
            decimal sumMaster = 0;
            decimal sumKnet = 0;

            decimal totalValue = 0;
            string date = "";

            PurStsReport(tempquery, rep, reppath);
            if (tempquery == null || tempquery.Count() == 0)
            {
                date = "";
            }
            else
            {
                date = SectionData.DateToString(tempquery.FirstOrDefault().updateDate);
            }
            //

            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var r in tempquery)
            {
                //   r.CopdiscountValue = decimal.Parse(SectionData.DecTostring(r.CopdiscountValue));

                sumCash += (decimal)r.cachTransferList.Where(x => x.processType == "cash").Sum(x => x.cash);
                sumCredit += r.deserved == null ? (decimal)0 : (decimal)r.deserved;
                sumVisa += (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardName == "Visa Card").Sum(x => x.cash);
                sumMaster += (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardName == "Master Card").Sum(x => x.cash);
                sumKnet += (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardName == "K-net").Sum(x => x.cash);

                r.totalNet = decimal.Parse(SectionData.DecTostring(r.totalNet));

                totalValue += (decimal)r.totalNet;
            }

            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            //
            paramarr.Add(new ReportParameter("isTax", AppSettings.invoiceTax_bool.ToString()));
            paramarr.Add(new ReportParameter("invDate", date));

            paramarr.Add(new ReportParameter("trPaymentMethodsheader", MainWindow.resourcemanagerreport.GetString("trPaymentMethods")));

            paramarr.Add(new ReportParameter("trCash", MainWindow.resourcemanagerreport.GetString("trCash")));
            paramarr.Add(new ReportParameter("trDocument", MainWindow.resourcemanagerreport.GetString("trDocument")));
            paramarr.Add(new ReportParameter("trCheque", MainWindow.resourcemanagerreport.GetString("trCheque")));
            paramarr.Add(new ReportParameter("trCredit", MainWindow.resourcemanagerreport.GetString("trCredit")));
            paramarr.Add(new ReportParameter("trMultiplePayment", MainWindow.resourcemanagerreport.GetString("trMultiplePayment")));

            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo")));
            paramarr.Add(new ReportParameter("trType", MainWindow.resourcemanagerreport.GetString("trType")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trPOS", MainWindow.resourcemanagerreport.GetString("trPOS")));
            paramarr.Add(new ReportParameter("trUser", MainWindow.resourcemanagerreport.GetString("trUser")));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            paramarr.Add(new ReportParameter("trPaymentValue", MainWindow.resourcemanagerreport.GetString("trPaymentValue")));
            paramarr.Add(new ReportParameter("trPaymentType", MainWindow.resourcemanagerreport.GetString("trPaymentType")));
            paramarr.Add(new ReportParameter("trPayments", MainWindow.resourcemanagerreport.GetString("trPayments")));

            paramarr.Add(new ReportParameter("trVisa", "Visa Card"));
            paramarr.Add(new ReportParameter("trMaster", "Master Card"));
            paramarr.Add(new ReportParameter("trKnet", "K-net"));
            paramarr.Add(new ReportParameter("trInvoicesDate", MainWindow.resourcemanagerreport.GetString("trInvoicesDate")));
            //values
            paramarr.Add(new ReportParameter("totalValue", SectionData.DecTostring(totalValue)));

            paramarr.Add(new ReportParameter("Cash", SectionData.DecTostring(sumCash)));
            paramarr.Add(new ReportParameter("Credit", SectionData.DecTostring(sumCredit)));
            paramarr.Add(new ReportParameter("Visa", SectionData.DecTostring(sumVisa)));
            paramarr.Add(new ReportParameter("Master", SectionData.DecTostring(sumMaster)));
            paramarr.Add(new ReportParameter("Knet", SectionData.DecTostring(sumKnet)));

            itemTransferInvTypeConv(paramarr);
            rep.DataSources.Add(new ReportDataSource("DataSetITinvoice", tempquery));

        }

        public List<PayedInvclass> payedConvert(IEnumerable<PayedInvclass> Query)
        {
            List<PayedInvclass> tempquery = JsonConvert.DeserializeObject<List<PayedInvclass>>(JsonConvert.SerializeObject(Query));

            return tempquery;

        }

        public async static Task SaledailyReport(IEnumerable<ItemTransferInvoice> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr
            , int selectedTab, string cb_paymentsValue, string cb_cardValue
            , bool? rad_invoice, bool? rad_return, bool? rad_draft)
        {
            List<ItemTransferInvoice> tempquery = JsonConvert.DeserializeObject<List<ItemTransferInvoice>>(JsonConvert.SerializeObject(Query));
            //if (selectedTab == 0)
            //{
            //    tempquery = tempquery.Where(x => 

            //    x.invType == "s"||x.invType == "sb"||x.invType == "s").ToList();
            //}
            //else if (selectedTab == 1)
            //{
            //    tempquery = tempquery.Where(x => x.invType == "or").ToList();
            //}
            //else if (selectedTab == 2)
            //{
            //    tempquery = tempquery.Where(x => x.invType == "q").ToList();
            //}
            decimal sumCash = 0;
            decimal sumCredit = 0;
            decimal sumCashRow = 0;
            decimal sumCreditRow = 0;
            decimal sumCardRow = 0;
            //decimal sumVisa = 0;
            //decimal sumMaster = 0;
            //decimal sumKnet = 0;

            decimal totalValue = 0;
            string date = "";

            PurStsReport(tempquery, rep, reppath);
            if (tempquery == null || tempquery.Count() == 0)
            {
                date = "";
            }
            else
            {
                date = SectionData.DateToString(tempquery.FirstOrDefault().updateDate);
            }
            //

            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            List<Card> cardlist = new List<Card>();

            if (FillCombo.cardsList == null)
                await FillCombo.RefreshCards();
            cardlist = FillCombo.cardsList;
            Card cr = new Card();
            List<CardsSts> cardtransList = new List<CardsSts>();
            /*
            cash
card
balance
multiple
*/

            foreach (Card card in cardlist)
            {
                CardsSts tempcard = new CardsSts();

                tempcard.cardId = card.cardId;
                tempcard.name = card.name;
                tempcard.hasProcessNum = card.hasProcessNum;
                tempcard.image = card.image;
                tempcard.isActive = card.isActive;
                tempcard.total = 0;
                cardtransList.Add(tempcard);
            }
            //
            foreach (var r in tempquery)
            {
                sumCashRow = 0;
                sumCreditRow = 0;
                sumCardRow = 0;
                int cachListcount = 0;
                r.totalNet0 = 0;
                r.paidD = r.cachTransferList.Sum(x => x.cash);
                r.deserved = r.totalNet - r.paidD;
                //   r.CopdiscountValue = decimal.Parse(SectionData.DecTostring(r.CopdiscountValue));
                cachListcount = r.cachTransferList.Count();
                // cash 
                if (cb_paymentsValue == "all" || cb_paymentsValue == "cash" || cb_paymentsValue == "multiple")
                {
                    // sumCash += (decimal)r.cachTransferList.Where(x => x.processType == "cash").Sum(x => x.cash);
                    sumCashRow = (decimal)r.cachTransferList.Where(x => x.processType == "cash" && x.transType == "d").Sum(x => x.cash);
                    decimal negsum = (decimal)r.cachTransferList.Where(x => x.processType == "cash" && x.transType == "p").Sum(x => x.cash);
                    sumCashRow = sumCashRow - negsum;
                    sumCash += sumCashRow;

                }
                //oncredit
                //if (cb_paymentsValue == "all" || cb_paymentsValue == "balance" || cb_paymentsValue == "multiple")
                //{
                //    sumCreditRow = r.deserved == null ? (decimal)0 : (decimal)r.deserved;
                //    sumCredit += sumCreditRow;

                //}
                if (cb_paymentsValue == "all" || cb_paymentsValue == "balance" || cb_paymentsValue == "multiple")
                {
                    sumCreditRow = r.deserved == null ? (decimal)0 : (decimal)r.deserved;
                    int cnt = r.cachTransferList.Where(x => x.processType == "balance").ToList().Count();
                    if (cnt > 0)
                    {
                        sumCreditRow = (decimal)r.cachTransferList.Where(x => x.processType == "balance").Sum(x => x.cash);
                    }


                    if (rad_invoice != true && rad_return != true && rad_draft == true)
                    {
                        if (r.invType == "s" || r.invType == "sd" || r.invType == "pb" || r.invType == "pbd")
                        {
                            if (sumCreditRow < 0)
                            {
                                sumCreditRow = sumCreditRow * (-1);
                            }
                        }
                        else if (r.invType == "p" || r.invType == "pd" || r.invType == "sb" || r.invType == "sbd")
                        {
                            if (sumCreditRow > 0)
                            {
                                sumCreditRow = sumCreditRow * (-1);
                            }
                        }
                        sumCredit += sumCreditRow;
                    }
                    else
                    {
                        if (r.invType == "s" || r.invType == "pb")
                        {
                            if (sumCreditRow < 0)
                            {
                                sumCreditRow = sumCreditRow * (-1);
                            }
                            sumCredit += sumCreditRow;
                        }
                        else if (r.invType == "p" || r.invType == "sb")
                        {
                            if (sumCreditRow > 0)
                            {
                                sumCreditRow = sumCreditRow * (-1);
                            }
                            sumCredit += sumCreditRow;
                        }
                    }
                }

                //card sum

                decimal tempcardsum = 0;
                if (cb_paymentsValue == "all" || cb_paymentsValue == "multiple" || (cb_paymentsValue == "card" && cb_cardValue == "all"))
                {
                    foreach (CardsSts card in cardtransList)
                    {
                        tempcardsum = (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardId == card.cardId && x.transType == "d").Sum(x => x.cash);
                        decimal negsum = (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardId == card.cardId && x.transType == "p").Sum(x => x.cash);
                        tempcardsum = tempcardsum - negsum;
                        sumCardRow += tempcardsum;
                        card.total += tempcardsum;

                    }

                }
                else if (cb_paymentsValue == "card" && cb_cardValue != "all")
                {

                    foreach (CardsSts card in cardtransList)
                    {

                        tempcardsum = (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardId == card.cardId && x.cardName == cb_cardValue && x.transType == "d").Sum(x => x.cash);
                        decimal negsum = (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardId == card.cardId && x.cardName == cb_cardValue && x.transType == "p").Sum(x => x.cash);
                        tempcardsum = tempcardsum - negsum;
                        sumCardRow += tempcardsum;
                        card.total += tempcardsum;
                    }
                }

                //
                //sumVisa += (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardName == "Visa Card").Sum(x => x.cash);
                //sumMaster += (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardName == "Master Card").Sum(x => x.cash);
                //sumKnet += (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardName == "K-net").Sum(x => x.cash);

                r.totalNet = decimal.Parse(SectionData.DecTostring(r.totalNet));
                r.totalNet0 = decimal.Parse(SectionData.DecTostring(sumCardRow + sumCreditRow + sumCashRow));
                //  totalValue += (decimal)r.totalNet;

                // procc type converter
                //multiple //   balance
                if (cb_paymentsValue == "cash")
                {
                    r.processType0 = MainWindow.resourcemanagerreport.GetString("trCash");
                }
                else if (cb_paymentsValue == "balance")
                {
                    r.processType0 = MainWindow.resourcemanagerreport.GetString("trCredit");
                }
                else if (cb_paymentsValue == "multiple")
                {
                    r.processType0 = MainWindow.resourcemanagerreport.GetString("trMultiplePayment");
                }
                else if (cb_paymentsValue == "card")
                {
                    if (cb_cardValue == "all")
                    {

                        if (r.cachTransferList.Where(x => x.processType == "card").Count() == 1)
                        {
                            r.processType0 = r.cachTransferList.Where(x => x.processType == "card").FirstOrDefault().cardName;
                        }
                        else
                        {
                            r.processType0 = MainWindow.resourcemanagerreport.GetString("trPaymentMethods");
                        }
                    }
                    else
                    {
                        r.processType0 = cb_cardValue;
                    }


                }
                else if (cb_paymentsValue == "all")//all
                {
                    if (cachListcount == 0)
                    {
                        r.processType = "balance";
                        r.processType0 = MainWindow.resourcemanagerreport.GetString("trCredit");
                    }
                    else if (cachListcount > 0 && r.paidD != r.totalNet || cachListcount > 1)
                    {
                        r.processType = "multiple";
                        r.processType0 = MainWindow.resourcemanagerreport.GetString("trMultiplePayment");
                    }
                    else if (cachListcount == 1 && r.paidD == r.totalNet)
                    {
                        //no credit - cash or card
                        string procType = r.cachTransferList.FirstOrDefault().processType;
                        string cardn = "";
                        if (procType == "card")
                        {
                            cardn = r.cachTransferList.Where(x => x.processType == "card").FirstOrDefault().cardName;

                        }
                        else
                        {
                            cardn = "";
                        }
                        r.processType0 = PaymentConvert(procType, cardn);

                    }
                }


            }
            //positive value
            //if (sumCredit < 0)
            //{
            //    sumCredit = sumCredit * (-1);
            //}

            //converter
            decimal totalcards = 0;
            foreach (CardsSts card in cardtransList)
            {
                totalcards += (decimal)card.total;
                card.total = decimal.Parse(SectionData.DecTostring(card.total));
            }

            totalValue = totalcards + sumCash + sumCredit;
            decimal sumDraft = 0;
            sumDraft = (decimal)tempquery.Where(x => x.invType == "sd" || x.invType == "sbd").Sum(x => x.totalNet).Value;

            /*
            rad_invoice
rad_return
rad_draft
*/
            if (selectedTab == 0)
            {
                if (rad_invoice != true && rad_return != true && rad_draft == true)
                {

                }
                else
                {
                    totalValue = totalValue - sumDraft;
                    sumCredit = sumCredit - sumDraft;
                }

            }
            else if (selectedTab == 1)
            {
                //  sumCredit = totalValue;
                sumCredit = 0;
            }
            else if (selectedTab == 2)
            {
                // sumCredit = totalValue;
                sumCredit = 0;
            }
            /*
             *  (s.invType == "sd" || s.invType == "sbd")
 s.invType == "ord"
 s.invType == "qd" 

             * */
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            //
            paramarr.Add(new ReportParameter("isTax", AppSettings.invoiceTax_bool.ToString()));
            paramarr.Add(new ReportParameter("invDate", date));

            paramarr.Add(new ReportParameter("trPaymentMethodsheader", MainWindow.resourcemanagerreport.GetString("trPaymentMethods")));

            paramarr.Add(new ReportParameter("trCash", MainWindow.resourcemanagerreport.GetString("trCash")));
            paramarr.Add(new ReportParameter("trDocument", MainWindow.resourcemanagerreport.GetString("trDocument")));
            paramarr.Add(new ReportParameter("trCheque", MainWindow.resourcemanagerreport.GetString("trCheque")));
            paramarr.Add(new ReportParameter("trCredit", MainWindow.resourcemanagerreport.GetString("trCredit")));
            paramarr.Add(new ReportParameter("trMultiplePayment", MainWindow.resourcemanagerreport.GetString("trMultiplePayment")));

            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo")));
            paramarr.Add(new ReportParameter("trType", MainWindow.resourcemanagerreport.GetString("trType")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trPOS", MainWindow.resourcemanagerreport.GetString("trPOS")));
            paramarr.Add(new ReportParameter("trUser", MainWindow.resourcemanagerreport.GetString("trUser")));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            paramarr.Add(new ReportParameter("trPaymentValue", MainWindow.resourcemanagerreport.GetString("trPaymentValue")));
            paramarr.Add(new ReportParameter("trPaymentType", MainWindow.resourcemanagerreport.GetString("trPaymentType")));
            paramarr.Add(new ReportParameter("trPayments", MainWindow.resourcemanagerreport.GetString("trPayments")));

            //paramarr.Add(new ReportParameter("trVisa", "Visa Card"));
            //paramarr.Add(new ReportParameter("trMaster", "Master Card"));
            //paramarr.Add(new ReportParameter("trKnet", "K-net"));
            paramarr.Add(new ReportParameter("trInvoicesDate", MainWindow.resourcemanagerreport.GetString("trInvoicesDate")));
            //values
            paramarr.Add(new ReportParameter("totalValue", SectionData.DecTostring(totalValue)));

            paramarr.Add(new ReportParameter("Cash", SectionData.DecTostring(sumCash)));
            paramarr.Add(new ReportParameter("Credit", SectionData.DecTostring(sumCredit)));
            //paramarr.Add(new ReportParameter("Visa", SectionData.DecTostring(sumVisa)));
            //paramarr.Add(new ReportParameter("Master", SectionData.DecTostring(sumMaster)));
            //paramarr.Add(new ReportParameter("Knet", SectionData.DecTostring(sumKnet)));
            paramarr.Add(new ReportParameter("totalCards", totalcards.ToString()));
            paramarr.Add(new ReportParameter("trRefNo", MainWindow.resourcemanagerreport.GetString("trRefNo.")));
            itemTransferInvTypeConv(paramarr);
            rep.DataSources.Add(new ReportDataSource("DataSetITinvoice", tempquery));
            rep.DataSources.Add(new ReportDataSource("CardsSts", cardtransList));
        }

        public async static Task<List<ItemTransferInvoice>> invoicepayment(IEnumerable<ItemTransferInvoice> Query, List<ReportParameter> paramarr, int selectedTab, string cb_paymentsValue, string cb_cardValue, List<CardsSts> cardtransList
            , bool? rad_invoice, bool? rad_return, bool? rad_draft
            )
        {
            List<ItemTransferInvoice> tempquery = JsonConvert.DeserializeObject<List<ItemTransferInvoice>>(JsonConvert.SerializeObject(Query));
            //if (selectedTab == 0)
            //{
            //    tempquery = tempquery.Where(x => x.invType == "s").ToList();
            //}
            //else if (selectedTab == 1)
            //{
            //    tempquery = tempquery.Where(x => x.invType == "or").ToList();
            //}
            //else if (selectedTab == 2)
            //{
            //    tempquery = tempquery.Where(x => x.invType == "q").ToList();
            //}
            decimal sumCash = 0;
            decimal sumCredit = 0;
            decimal sumCashRow = 0;
            decimal sumCreditRow = 0;
            decimal sumCardRow = 0;
            //decimal sumVisa = 0;
            //decimal sumMaster = 0;
            //decimal sumKnet = 0;

            decimal totalValue = 0;
            string date = "";


            if (tempquery == null || tempquery.Count() == 0)
            {
                date = "";
            }
            else
            {
                date = SectionData.DateToString(tempquery.FirstOrDefault().updateDate);
            }
            //

            //rep.ReportPath = reppath;
            //rep.EnableExternalImages = true;
            //rep.DataSources.Clear();
            List<Card> cardlist = new List<Card>();

            if (FillCombo.cardsList == null)
                await FillCombo.RefreshCards();
            cardlist = FillCombo.cardsList;
            Card cr = new Card();

            /*
            cash
card
balance
multiple
*/

            foreach (Card card in cardlist)
            {
                CardsSts tempcard = new CardsSts();

                tempcard.cardId = card.cardId;
                tempcard.name = card.name;
                tempcard.hasProcessNum = card.hasProcessNum;
                tempcard.image = card.image;
                tempcard.isActive = card.isActive;
                tempcard.total = 0;
                cardtransList.Add(tempcard);
            }
            //
            foreach (var r in tempquery)
            {
                sumCashRow = 0;
                sumCreditRow = 0;
                sumCardRow = 0;
                int cachListcount = 0;
                r.totalNet0 = 0;
                r.paidD = r.cachTransferList.Sum(x => x.cash);
                r.deserved = r.totalNet - r.paidD;
                //   r.CopdiscountValue = decimal.Parse(SectionData.DecTostring(r.CopdiscountValue));
                cachListcount = r.cachTransferList.Count();
                // cash 
                if (cb_paymentsValue == "all" || cb_paymentsValue == "cash" || cb_paymentsValue == "multiple")
                {
                    // sumCash += (decimal)r.cachTransferList.Where(x => x.processType == "cash").Sum(x => x.cash);
                    //    sumCashRow = (decimal)r.cachTransferList.Where(x => x.processType == "cash").Sum(x => x.cash);
                    sumCashRow = (decimal)r.cachTransferList.Where(x => x.processType == "cash" && x.transType == "d").Sum(x => x.cash);
                    decimal negsum = (decimal)r.cachTransferList.Where(x => x.processType == "cash" && x.transType == "p").Sum(x => x.cash);
                    sumCashRow = sumCashRow - negsum;
                    sumCash += sumCashRow;

                }
                //oncredit
                if (cb_paymentsValue == "all" || cb_paymentsValue == "balance" || cb_paymentsValue == "multiple")
                {
                    sumCreditRow = r.deserved == null ? (decimal)0 : (decimal)r.deserved;
                    int cnt = r.cachTransferList.Where(x => x.processType == "balance").ToList().Count();
                    if (cnt > 0)
                    {
                        sumCreditRow = (decimal)r.cachTransferList.Where(x => x.processType == "balance").Sum(x => x.cash);
                    }


                    if (rad_invoice != true && rad_return != true && rad_draft == true)
                    {
                        if (r.invType == "s" || r.invType == "sd" || r.invType == "pb" || r.invType == "pbd")
                        {
                            if (sumCreditRow < 0)
                            {
                                sumCreditRow = sumCreditRow * (-1);
                            }
                        }
                        else if (r.invType == "p" || r.invType == "pd" || r.invType == "sb" || r.invType == "sbd")
                        {
                            if (sumCreditRow > 0)
                            {
                                sumCreditRow = sumCreditRow * (-1);
                            }
                        }
                        sumCredit += sumCreditRow;
                    }
                    else
                    {
                        if (r.invType == "s" || r.invType == "pb")
                        {
                            if (sumCreditRow < 0)
                            {
                                sumCreditRow = sumCreditRow * (-1);
                            }
                            sumCredit += sumCreditRow;
                        }
                        else if (r.invType == "p" || r.invType == "sb")
                        {
                            if (sumCreditRow > 0)
                            {
                                sumCreditRow = sumCreditRow * (-1);
                            }
                            sumCredit += sumCreditRow;
                        }
                    }
                }

                //card sum

                decimal tempcardsum = 0;
                if (cb_paymentsValue == "all" || cb_paymentsValue == "multiple" || (cb_paymentsValue == "card" && cb_cardValue == "all"))
                {
                    foreach (CardsSts card in cardtransList)
                    {
                        //  tempcardsum = (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardId == card.cardId).Sum(x => x.cash);
                        tempcardsum = (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardId == card.cardId && x.transType == "d").Sum(x => x.cash);
                        decimal negsum = (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardId == card.cardId && x.transType == "p").Sum(x => x.cash);
                        tempcardsum = tempcardsum - negsum;
                        sumCardRow += tempcardsum;
                        card.total += tempcardsum;

                    }

                }
                else if (cb_paymentsValue == "card" && cb_cardValue != "all")
                {

                    foreach (CardsSts card in cardtransList)
                    {

                        //  tempcardsum = (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardId == card.cardId && x.cardName == cb_cardValue).Sum(x => x.cash);
                        tempcardsum = (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardId == card.cardId && x.cardName == cb_cardValue && x.transType == "d").Sum(x => x.cash);
                        decimal negsum = (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardId == card.cardId && x.cardName == cb_cardValue && x.transType == "p").Sum(x => x.cash);
                        tempcardsum = tempcardsum - negsum;
                        sumCardRow += tempcardsum;
                        card.total += tempcardsum;
                    }
                }

                //
                //sumVisa += (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardName == "Visa Card").Sum(x => x.cash);
                //sumMaster += (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardName == "Master Card").Sum(x => x.cash);
                //sumKnet += (decimal)r.cachTransferList.Where(x => x.processType == "card" && x.cardName == "K-net").Sum(x => x.cash);

                r.totalNet = decimal.Parse(SectionData.DecTostring(r.totalNet));
                r.totalNet0 = decimal.Parse(SectionData.DecTostring(sumCardRow + sumCreditRow + sumCashRow));
                //  totalValue += (decimal)r.totalNet;

                // procc type converter
                //multiple //   balance
                if (cb_paymentsValue == "cash")
                {
                    r.processType0 = MainWindow.resourcemanagerreport.GetString("trCash");
                }
                else if (cb_paymentsValue == "balance")
                {
                    r.processType0 = MainWindow.resourcemanagerreport.GetString("trCredit");
                }
                else if (cb_paymentsValue == "multiple")
                {
                    r.processType0 = MainWindow.resourcemanagerreport.GetString("trMultiplePayment");
                }
                else if (cb_paymentsValue == "card")
                {
                    if (cb_cardValue == "all")
                    {

                        if (r.cachTransferList.Where(x => x.processType == "card").Count() == 1)
                        {
                            r.processType0 = r.cachTransferList.Where(x => x.processType == "card").FirstOrDefault().cardName;
                        }
                        else
                        {
                            r.processType0 = MainWindow.resourcemanagerreport.GetString("trPaymentMethods");
                        }
                    }
                    else
                    {
                        r.processType0 = cb_cardValue;
                    }


                }
                else if (cb_paymentsValue == "all")//all
                {
                    if (cachListcount == 0)
                    {
                        r.processType = "balance";
                        r.processType0 = MainWindow.resourcemanagerreport.GetString("trCredit");
                    }
                    else if (cachListcount > 0 && r.paidD != r.totalNet || cachListcount > 1)
                    {
                        r.processType = "multiple";
                        r.processType0 = MainWindow.resourcemanagerreport.GetString("trMultiplePayment");
                    }
                    else if (cachListcount == 1 && r.paidD == r.totalNet)
                    {
                        //no credit - cash or card
                        string procType = r.cachTransferList.FirstOrDefault().processType;
                        string cardn = "";
                        if (procType == "card")
                        {
                            cardn = r.cachTransferList.Where(x => x.processType == "card").FirstOrDefault().cardName;

                            //  r.processType0 =PaymentConvert(procType, cardn);
                        }
                        else
                        {
                            cardn = "";
                        }
                        r.processType0 = PaymentConvert(procType, cardn);
                        //    r.processType0 = r.cachTransferList.Where(x => x.processType == "card").FirstOrDefault().cardName;
                        //processType r.car


                    }
                }


            }
            //positive value
            //if (sumCredit < 0)
            //{
            //    sumCredit = sumCredit * (-1);
            //}

            //converter
            decimal totalcards = 0;
            foreach (CardsSts card in cardtransList)
            {
                totalcards += (decimal)card.total;
                card.total = decimal.Parse(SectionData.DecTostring(card.total));
            }
            totalValue = totalcards + sumCash + sumCredit;
            decimal sumDraft = 0;
            sumDraft = (decimal)tempquery.Where(x => x.invType == "sd" || x.invType == "sbd" || x.invType == "pd" || x.invType == "pbd").Sum(x => x.totalNet).Value;

            /*
             * pd
"pbd

            rad_invoice
rad_return
rad_draft
*/

            //if (rad_invoice != true && rad_return != true && rad_draft == true)
            //{

            //}
            //else
            //{
            //    totalValue = totalValue - sumDraft;
            //    sumCredit = sumCredit - sumDraft;
            //}


            //if (selectedTab == 0)
            //{

            //}
            //else if (selectedTab == 1)
            //{
            //    sumCredit = totalValue;
            //}
            //else if (selectedTab == 2)
            //{
            //    sumCredit = totalValue;
            //}
            paramarr.Add(new ReportParameter("totalValue", SectionData.DecTostring(totalValue)));

            paramarr.Add(new ReportParameter("Cash", SectionData.DecTostring(sumCash)));
            paramarr.Add(new ReportParameter("Credit", SectionData.DecTostring(sumCredit)));
            paramarr.Add(new ReportParameter("totalCards", totalcards.ToString()));
            return tempquery.ToList();

        }

        public static void ProfitReport(IEnumerable<ItemUnitInvoiceProfit> tempquery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var r in tempquery)
            {

                r.totalNet = decimal.Parse(SectionData.DecTostring(r.totalNet));
                r.invoiceProfit = decimal.Parse(SectionData.DecTostring(r.invoiceProfit));
                r.itemProfit = decimal.Parse(SectionData.DecTostring(r.itemProfit));
                r.itemunitProfit = decimal.Parse(SectionData.DecTostring(r.itemunitProfit));

            }
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo")));
            paramarr.Add(new ReportParameter("trType", MainWindow.resourcemanagerreport.GetString("trType")));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trPOS", MainWindow.resourcemanagerreport.GetString("trPOS")));
            paramarr.Add(new ReportParameter("trProfits", MainWindow.resourcemanagerreport.GetString("trProfits")));

            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));

            paramarr.Add(new ReportParameter("trAmount", MainWindow.resourcemanagerreport.GetString("trAmount")));
            rep.DataSources.Add(new ReportDataSource("DataSetProfit", tempquery));
            paramarr.Add(new ReportParameter("title", MainWindow.resourcemanagerreport.GetString("trProfits")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            itemTransferInvTypeConv(paramarr);

        }
        public static void ProfitNetReport(IEnumerable<ItemUnitInvoiceProfit> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr, DateTime? startDate, DateTime? endDate)
        {
            List<ItemUnitInvoiceProfit> tempquery = JsonConvert.DeserializeObject<List<ItemUnitInvoiceProfit>>(JsonConvert.SerializeObject(Query));


            //  SectionData.DateToString(startDate)
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var r in tempquery)
            {

                //  r.totalNet = decimal.Parse(SectionData.DecTostring(r.totalNet));
                r.invoiceProfit = decimal.Parse(SectionData.DecTostring(r.invoiceProfit));
                // r.itemProfit = decimal.Parse(SectionData.DecTostring(r.itemProfit));
                // r.itemunitProfit = decimal.Parse(SectionData.DecTostring(r.itemunitProfit));
                r.description = ProfitDescriptionConvert(r.invNumber, r.side);

            }


            // paramarr.Add(new ReportParameter("title", MainWindow.resourcemanagerreport.GetString("trProfits")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));

            paramarr.Add(new ReportParameter("startDate", SectionData.DateToString(startDate)));
            paramarr.Add(new ReportParameter("endDate", SectionData.DateToString(endDate)));

            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo")));
            paramarr.Add(new ReportParameter("trType", MainWindow.resourcemanagerreport.GetString("trType")));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trPOS", MainWindow.resourcemanagerreport.GetString("trPOS")));
            paramarr.Add(new ReportParameter("trProfits", MainWindow.resourcemanagerreport.GetString("trProfits")));

            paramarr.Add(new ReportParameter("trAmount", MainWindow.resourcemanagerreport.GetString("trAmount")));
            paramarr.Add(new ReportParameter("trDescription", MainWindow.resourcemanagerreport.GetString("trDescription")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));

            rep.DataSources.Add(new ReportDataSource("DataSetProfit", tempquery));

            //  itemTransferInvTypeConv(paramarr);

        }


        public static string ProfitDescriptionConvert(string invNumber, string side)
        {

            string description = "";

            if (!string.IsNullOrEmpty(invNumber))
                description = MainWindow.resourcemanagerreport.GetString("trProfitInvoice");
            else
            {
                switch (side.ToString())
                {
                    case "bnd": break;
                    case "v": description = MainWindow.resourcemanagerreport.GetString("trVendor"); break;
                    case "c": description = MainWindow.resourcemanagerreport.GetString("trCustomer"); break;
                    case "u": description = MainWindow.resourcemanagerreport.GetString("trUser"); break;
                    case "s": description = MainWindow.resourcemanagerreport.GetString("trSalary"); break;
                    case "e": description = MainWindow.resourcemanagerreport.GetString("trGeneralExpenses"); break;
                    case "m": description = MainWindow.resourcemanagerreport.GetString("trAdministrativePull"); break;
                    case "sh": description = MainWindow.resourcemanagerreport.GetString("trShippingCompany"); break;
                    case "tax": description = MainWindow.resourcemanagerreport.GetString("trTaxCollection"); break;
                    default: break;
                }

                description = MainWindow.resourcemanagerreport.GetString("trPayment") + "-" + description;
            }

            return description;
        }

        public static void AccTaxReport(IEnumerable<ItemTransferInvoiceTax> invoiceItems, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            //trTotal->trWithoutTax
            //trTotalInvoice->trTotal
            rep.DataSources.Add(new ReportDataSource("DataSetITinvoice", invoiceItems));
            paramarr.Add(new ReportParameter("trNum", MainWindow.resourcemanagerreport.GetString("trNo")));// tt
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trWithoutTax")));
            paramarr.Add(new ReportParameter("trTaxValue", MainWindow.resourcemanagerreport.GetString("trTaxValue")));
            paramarr.Add(new ReportParameter("trTaxPercentage", MainWindow.resourcemanagerreport.GetString("trTaxPercentage")));
            paramarr.Add(new ReportParameter("trTotalInvoice", MainWindow.resourcemanagerreport.GetString("trTotal")));
            paramarr.Add(new ReportParameter("trItemUnit", MainWindow.resourcemanagerreport.GetString("trItemUnit")));
            paramarr.Add(new ReportParameter("trOnItem", MainWindow.resourcemanagerreport.GetString("trOnItem")));
            paramarr.Add(new ReportParameter("trPrice", MainWindow.resourcemanagerreport.GetString("trPrice")));

            paramarr.Add(new ReportParameter("trSum", MainWindow.resourcemanagerreport.GetString("trTotalTax")));
            foreach (var r in invoiceItems)
            {
                r.OneItemPriceNoTax = decimal.Parse(SectionData.DecTostring(r.OneItemPriceNoTax));
                r.subTotalNotax = decimal.Parse(SectionData.DecTostring(r.subTotalNotax));//
                r.ItemTaxes = decimal.Parse(SectionData.PercentageDecTostring(r.ItemTaxes));
                r.itemUnitTaxwithQTY = decimal.Parse(SectionData.DecTostring(r.itemUnitTaxwithQTY));
                r.subTotalTax = decimal.Parse(SectionData.DecTostring(r.subTotalTax));

                r.totalNoTax = decimal.Parse(SectionData.DecTostring(r.totalNoTax));
                r.tax = decimal.Parse(SectionData.PercentageDecTostring(r.tax));
                r.invTaxVal = decimal.Parse(SectionData.DecTostring(r.invTaxVal));
                r.totalNet = decimal.Parse(SectionData.DecTostring(r.totalNet));

            }
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));

        }
        public static string ReportTabTitle(string firstTitle, string secondTitle)
        {
            string trtext = "";
            //////////////////////////////////////////////////////////////////////////////
            if (firstTitle == "invoice")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trInvoices");
            else if (firstTitle == "quotation")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trQuotations");
            else if (firstTitle == "promotion")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trThePromotion");
            else if (firstTitle == "internal")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trInternal");
            else if (firstTitle == "external")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trExternal");
            else if (firstTitle == "banksReport")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trBanks");
            else if (firstTitle == "destroied")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trDestructives");
            else if (firstTitle == "usersReport")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trUsers");
            else if (firstTitle == "storageReports")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trStorage");
            else if (firstTitle == "stocktaking")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trStocktaking");
            else if (firstTitle == "stock")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trStock");
            else if (firstTitle == "purchaseOrders")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trPurchaseOrders");
            else if (firstTitle == "saleOrders")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trSalesOrders");

            else if (firstTitle == "saleItems" || firstTitle == "purchaseItem")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trItems");
            else if (firstTitle == "recipientReport")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trReceived");
            else if (firstTitle == "accountStatement")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trAccountStatement");
            else if (firstTitle == "paymentsReport")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trPayments");
            else if (firstTitle == "posReports")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trPOS");
            else if (firstTitle == "dailySalesStatistic")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trDailySales");
            else if (firstTitle == "accountProfits")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trProfits");
            else if (firstTitle == "accountFund")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trCashBalance");
            else if (firstTitle == "quotations")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trQTReport");
            else if (firstTitle == "transfers")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trTransfers");
            else if (firstTitle == "fund")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trCashBalance");
            else if (firstTitle == "DirectEntry")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trDirectEntry");
            else if (firstTitle == "tax")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trTax");
            else if (firstTitle == "closing")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trDailyClosing");
            else if (firstTitle == "orders")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trOrderreport");
            else if (firstTitle == "sales")
                firstTitle = MainWindow.resourcemanagerreport.GetString("tr_Sales");
            else if (firstTitle == "itemsCost")
                firstTitle = MainWindow.resourcemanagerreport.GetString("trItemsCost");

            //itemsCost
            //trTransfers administrativePull operations
            //////////////////////////////////////////////////////////////////////////////

            if (secondTitle == "branch")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trBranches");
            else if (secondTitle == "pos")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trPOSs");
            else if (secondTitle == "vendors" || secondTitle == "vendor")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trVendors");
            else if (secondTitle == "customers" || secondTitle == "customer")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trCustomers");
            else if (secondTitle == "users" || secondTitle == "user")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trUsers");
            else if (secondTitle == "items" || secondTitle == "item")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trItems");
            else if (secondTitle == "coupon")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trCoupons");
            else if (secondTitle == "offers")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trOffer");
            else if (secondTitle == "invoice")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trInvoices");
            else if (secondTitle == "order")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trOrders");
            else if (secondTitle == "quotation")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trQTReport");
            else if (secondTitle == "operator")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trOperator");
            else if (secondTitle == "payments")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trPayments");
            else if (secondTitle == "recipient")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trReceived");
            else if (secondTitle == "destroied")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trDestructives");
            else if (secondTitle == "agent")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trCustomers");
            else if (secondTitle == "stock")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trStock");
            else if (secondTitle == "external")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trExternal");
            else if (secondTitle == "internal")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trInternal");
            else if (secondTitle == "stocktaking")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trStocktaking");
            else if (secondTitle == "archives")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trArchive");
            else if (secondTitle == "shortfalls")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trShortages");
            else if (secondTitle == "location")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trLocation");
            else if (secondTitle == "collect")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trCollect");
            else if (secondTitle == "shipping")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trShipping");
            else if (secondTitle == "salary")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trSalary");
            else if (secondTitle == "generalExpenses")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trGeneralExpenses");
            else if (secondTitle == "administrativePull")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trAdministrativePull");
            else if (secondTitle == "AdministrativeDeposit")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trAdministrativeDeposit");
            else if (secondTitle == "BestSeller")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trBestSeller");
            else if (secondTitle == "MostPurchased")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trMostPurchased");
            else if (secondTitle == "cash")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trCash_");
            else if (secondTitle == "operations")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trOperations");
            else if (secondTitle == "pull")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trPull");
            else if (secondTitle == "deposit")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trDeposit");
            else if (secondTitle == "delivered")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trDelivered");
            else if (secondTitle == "indelivery")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trInDelivery");
            else if (secondTitle == "taxCollection")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trTaxCollection");
            else if (secondTitle == "netProfit")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trNetProfit");
            else if (secondTitle == "tragent")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trAgents");
            else if (secondTitle == "trExpired")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trExpired");
            else if (secondTitle == "slices")
                secondTitle = MainWindow.resourcemanagerreport.GetString("slices");
            else if (secondTitle == "done")
                secondTitle = MainWindow.resourcemanagerreport.GetString("trDone");



            //////////////////////////////////////////////////////////////////////////////
            if (firstTitle == "" && secondTitle == "")
            {
                trtext = "";
            }
            else if (secondTitle == "" && firstTitle != "")
            {
                trtext = firstTitle;
            }
            else if (firstTitle == "" && secondTitle != "")
            {
                trtext = secondTitle;
            }
            else
            {
                trtext = firstTitle + " / " + secondTitle;
            }

            //  trtext = firstTitle + " / " + secondTitle;

            return trtext;
        }

        public async static Task PurInvStsReport(IEnumerable<ItemTransferInvoice> tempquery, LocalReport rep, string reppath, List<ReportParameter> paramarr, int selectedTab, string cb_paymentsValue, string cb_cardValue
            , bool? rad_invoice, bool? rad_return, bool? rad_draft
            )
        {
            List<CardsSts> cardtransList = new List<CardsSts>();
            tempquery = await invoicepayment(tempquery, paramarr, selectedTab, cb_paymentsValue, cb_cardValue, cardtransList
                , rad_invoice, rad_return, rad_draft
                );
            PurStsReport(tempquery, rep, reppath);
            itemTransferInvTypeConv(paramarr);
            paramarr.Add(new ReportParameter("isTax", AppSettings.invoiceTax_bool.ToString()));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            paramarr.Add(new ReportParameter("trRefNo", MainWindow.resourcemanagerreport.GetString("trRefNo.")));

            paramarr.Add(new ReportParameter("trPaymentValue", MainWindow.resourcemanagerreport.GetString("trPaymentValue")));
            paramarr.Add(new ReportParameter("trPaymentType", MainWindow.resourcemanagerreport.GetString("trPaymentType")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trCredit", MainWindow.resourcemanagerreport.GetString("trCredit")));
            //   paramarr.Add(new ReportParameter("totalValue", MainWindow.resourcemanagerreport.GetString("trPaymentValue")));
            paramarr.Add(new ReportParameter("trPaymentMethodsheader", MainWindow.resourcemanagerreport.GetString("trPaymentMethods")));
            paramarr.Add(new ReportParameter("trCash", MainWindow.resourcemanagerreport.GetString("trCash")));
            //
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trType", MainWindow.resourcemanagerreport.GetString("trType")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trPOS", MainWindow.resourcemanagerreport.GetString("trPOS")));
            paramarr.Add(new ReportParameter("trCompany", MainWindow.resourcemanagerreport.GetString("trCompany")));
            paramarr.Add(new ReportParameter("trUser", MainWindow.resourcemanagerreport.GetString("trUser")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            paramarr.Add(new ReportParameter("trDiscount", MainWindow.resourcemanagerreport.GetString("trDiscount")));
            paramarr.Add(new ReportParameter("trPrice", MainWindow.resourcemanagerreport.GetString("trPrice")));
            paramarr.Add(new ReportParameter("trTax", MainWindow.resourcemanagerreport.GetString("trTax")));
            paramarr.Add(new ReportParameter("trPayments", MainWindow.resourcemanagerreport.GetString("trPayments")));
            paramarr.Add(new ReportParameter("trVendor", MainWindow.resourcemanagerreport.GetString("trVendor")));
            rep.DataSources.Add(new ReportDataSource("CardsSts", cardtransList));
        }

        public static void PurOrderStsReport(IEnumerable<ItemTransferInvoice> tempquery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            PurStsReport(tempquery, rep, reppath);
            itemTransferInvTypeConv(paramarr);
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));

        }

        public static void posReport(IEnumerable<Pos> possQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            paramarr.Add(new ReportParameter("trPosName", MainWindow.resourcemanagerreport.GetString("trPosName")));
            paramarr.Add(new ReportParameter("trPosCode", MainWindow.resourcemanagerreport.GetString("trPosCode")));
            paramarr.Add(new ReportParameter("trBranchName", MainWindow.resourcemanagerreport.GetString("trBranchName")));
            paramarr.Add(new ReportParameter("trNote", MainWindow.resourcemanagerreport.GetString("trNote")));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trPOSs")));
            rep.DataSources.Add(new ReportDataSource("DataSetPos", possQuery));
        }
        public static void posClosingReport(IEnumerable<Pos> possQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<Pos> Query = JsonConvert.DeserializeObject<List<Pos>>(JsonConvert.SerializeObject(possQuery));
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (Pos r in Query)
            {
                r.boxState = ConvertBoxStat(r.boxState);
                r.balance = decimal.Parse(SectionData.DecTostring(r.balance));
            }
            paramarr.Add(new ReportParameter("trPosTooltip", MainWindow.resourcemanagerreport.GetString("trPosTooltip")));
            paramarr.Add(new ReportParameter("trCash", MainWindow.resourcemanagerreport.GetString("trCash")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trBoxState", MainWindow.resourcemanagerreport.GetString("trBoxState")));
            rep.DataSources.Add(new ReportDataSource("DataSet", Query));
        }

        public static string ConvertBoxStat(string boxState)
        {
            try
            {
                if (boxState != null)
                {
                    string s = boxState.ToString();
                    switch (s)
                    {
                        case "c":
                            return MainWindow.resourcemanagerreport.GetString("trClosed");
                        case "o":
                            return MainWindow.resourcemanagerreport.GetString("trOpen");
                        default:
                            return "";
                    }
                }
                else return "";
            }
            catch
            {
                return "";
            }
        }
        public static void customerReport(IEnumerable<Agent> customersQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trCode")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trCompany", MainWindow.resourcemanagerreport.GetString("trCompany")));
            paramarr.Add(new ReportParameter("trMobile", MainWindow.resourcemanagerreport.GetString("trMobile")));
            paramarr.Add(new ReportParameter("agentTitle", MainWindow.resourcemanagerreport.GetString("trCustomers")));


            rep.DataSources.Add(new ReportDataSource("AgentDataSet", customersQuery));
        }

        public static void branchReport(IEnumerable<Branch> branchQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trCode")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trAddress", MainWindow.resourcemanagerreport.GetString("trAddress")));
            paramarr.Add(new ReportParameter("trNote", MainWindow.resourcemanagerreport.GetString("trNote")));

            rep.DataSources.Add(new ReportDataSource("DataSetBranches", branchQuery));
        }

        public static void userReport(IEnumerable<User> usersQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trJob", MainWindow.resourcemanagerreport.GetString("trJob")));
            paramarr.Add(new ReportParameter("trWorkHours", MainWindow.resourcemanagerreport.GetString("trWorkHours")));
            paramarr.Add(new ReportParameter("trNote", MainWindow.resourcemanagerreport.GetString("trNote")));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trUsers")));
            rep.DataSources.Add(new ReportDataSource("DataSetUser", usersQuery));
        }

        public static void vendorReport(IEnumerable<Agent> vendorsQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trCode")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trCompany", MainWindow.resourcemanagerreport.GetString("trCompany")));
            paramarr.Add(new ReportParameter("trMobile", MainWindow.resourcemanagerreport.GetString("trMobile")));
            paramarr.Add(new ReportParameter("agentTitle", MainWindow.resourcemanagerreport.GetString("trVendors")));
            rep.DataSources.Add(new ReportDataSource("AgentDataSet", vendorsQuery));
        }

        public static void storeReport(IEnumerable<Branch> storesQuery, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetBranches", storesQuery));
        }

        public static List<string> strToList(string strSerial)
        {
            List<string> serialNumLst = new List<string>();
            if (strSerial != "")
            {
                string[] serialsArray = strSerial.Split(',');
                foreach (string s in serialsArray)
                {
                    serialNumLst.Add(s.Trim());
                }

            }

            return serialNumLst;

        }
        public static void purchaseInvoiceReportold(List<ItemTransfer> invoiceItems, LocalReport rep, string reppath)
        {

            List<string> tmpserialLst = new List<string>();
            List<Serial> serialLst = new List<Serial>();
            //Serial serialTemp = new Serial();
            ItemTransfer itemtrTemp = new ItemTransfer();
            //List<ItemTransfer> itemSerialLst = new List<ItemTransfer>();
            List<ItemTransfer> MaininvoiceItems = new List<ItemTransfer>();
            int totalserialscount = 0;
            int seq = 0;
            foreach (var i in invoiceItems)
            {
                int num = 0;
                i.price = decimal.Parse(SectionData.DecTostring(i.price));
                //add item row
                seq++;
                itemtrTemp = i;
                itemtrTemp.newLocked = seq;
                if (i.packageItems == null || i.packageItems.Count() == 0)
                {
                    //normal 
                    //check if has sub rows
                    if ((i.packageItems == null || i.packageItems.Count() == 0) && //normal item
                        (!(i.itemSerials == null || i.itemSerials.Count() == 0) //has serial
                        || (i.warrantyName != null && i.warrantyName != ""))) // or has warranty
                    {
                        itemtrTemp.lockedQuantity = 1;// to hide bottom border 
                    }
                }
                else
                {

                    itemtrTemp.itemUnitId = -5;//for main package

                    itemtrTemp.lockedQuantity = 1;// to hide bottom border 
                }

                if (itemtrTemp.unitName == "package")
                {


                    itemtrTemp.itemUnitId = -5;//for main package
                }



                MaininvoiceItems.Add(itemtrTemp);
                //normal item

                if (i.packageItems == null || i.packageItems.Count() == 0)//normal item
                {
                    //add serials
                    if (!(i.itemSerials == null || i.itemSerials.Count() == 0))
                    {
                        //has serial
                        foreach (Serial srow in i.itemSerials)
                        {
                            itemtrTemp = new ItemTransfer();
                            itemtrTemp.itemName = srow.serialNum;
                            itemtrTemp.itemUnitId = 0;//for serial
                            MaininvoiceItems.Add(itemtrTemp);

                            totalserialscount++;

                        }
                    }
                    //end add serials
                    //add warranty
                    if (i.warrantyName != null && i.warrantyName != "")
                    {
                        itemtrTemp = new ItemTransfer();
                        itemtrTemp.itemName = i.warrantyName;
                        itemtrTemp.itemUnitId = -1;//for warranty
                        MaininvoiceItems.Add(itemtrTemp);

                        totalserialscount++;
                    }
                    //end add warranty
                }
                else //package 
                {

                    List<Item> packageitemsList = new List<Item>();

                    packageitemsList = i.packageItems;

                    foreach (Item itemRow in packageitemsList)
                    {
                        //has serial
                        List<Serial> currentItemSerialsList = new List<Serial>();
                        if (!(i.itemSerials == null || i.itemSerials.Count() == 0))
                        {
                            currentItemSerialsList = i.itemSerials.Where(s => s.itemUnitId == itemRow.itemUnitId).ToList();
                        }

                        // check if has serials or waranty to add item row
                        if (currentItemSerialsList.Count > 0 //for serials
                            || (itemRow.warrantyName != null && itemRow.warrantyName != "")//for warranty
                            )
                        {
                            //add item name row
                            itemtrTemp = new ItemTransfer();
                            itemtrTemp.itemName = itemRow.name;
                            itemtrTemp.unitName = itemRow.unitName;
                            itemtrTemp.itemUnitId = -2;//for item of package
                            MaininvoiceItems.Add(itemtrTemp);
                            totalserialscount++;
                            //end add item name row
                            //has serial
                            if (currentItemSerialsList.Count > 0)
                            {
                                //add serials
                                foreach (Serial srow in currentItemSerialsList)
                                {
                                    itemtrTemp = new ItemTransfer();
                                    itemtrTemp.itemName = srow.serialNum;
                                    itemtrTemp.itemUnitId = -3;//for serial of  item package 
                                    MaininvoiceItems.Add(itemtrTemp);
                                    totalserialscount++;
                                }
                                //end add serial

                            }
                            //has warranty
                            if (itemRow.warrantyName != null && itemRow.warrantyName != "")
                            {
                                //add warranty
                                itemtrTemp = new ItemTransfer();
                                itemtrTemp.itemName = itemRow.warrantyName;
                                itemtrTemp.itemUnitId = -4;//for warranty of  item package 
                                MaininvoiceItems.Add(itemtrTemp);
                                totalserialscount++;
                                //end add warranty
                            }
                            //end has warranty
                        }


                    }


                }


                //end normal item

                //  tmpserialLst = new List<string>();
                //if (i.itemSerial != null && i.itemSerial != "")
                //{

                //    // num++;
                //    tmpserialLst = strToList(i.itemSerial);
                //    //add item name
                //    //serialTemp = new Serial();
                //    //serialTemp.serialNum = i.itemName;
                //    //serialLst.Add(serialTemp);
                //    //add serials of item
                //    // num = 1;
                //    //add serials rows for current item
                //    foreach (string s in tmpserialLst)
                //    {
                //        itemtrTemp = new ItemTransfer();
                //        itemtrTemp.itemName = s;
                //        itemtrTemp.itemUnitId = 0;
                //        MaininvoiceItems.Add(itemtrTemp);

                //        totalserialscount++;
                //    }
                //}

            }

            // old table
            //foreach (var i in invoiceItems)
            //{
            //    int num = 0;
            //    i.price = decimal.Parse(SectionData.DecTostring(i.price));
            //    tmpserialLst = new List<string>();
            //    if (i.itemSerial != null && i.itemSerial != "")
            //    {

            //        // num++;
            //        tmpserialLst = strToList(i.itemSerial);
            //        //add item name
            //        serialTemp = new Serial();
            //        serialTemp.serialNum = i.itemName;
            //        serialLst.Add(serialTemp);
            //        //add serials of item
            //        num = 1;
            //        foreach (string s in tmpserialLst)
            //        {
            //            serialTemp = new Serial();
            //            serialTemp.serialId = num;
            //            serialTemp.serialNum = s;
            //            serialLst.Add(serialTemp);
            //            num++;
            //        }
            //    }

            //}
            serialsCount = totalserialscount;
            // serialsCount = serialLst.Count();
            //  rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetItemTransfer", MaininvoiceItems));
            rep.DataSources.Add(new ReportDataSource("DataSetSerial", serialLst));
            rep.EnableExternalImages = true;

        }
 
        public static void purchaseInvoiceReport(List<ItemTransfer> invoiceItems, LocalReport rep, string reppath)
        {

            List<string> tmpserialLst = new List<string>();
            List<Serial> serialLst = new List<Serial>();
            //Serial serialTemp = new Serial();
            ItemTransfer itemtrTemp = new ItemTransfer();
            //List<ItemTransfer> itemSerialLst = new List<ItemTransfer>();
            List<ItemTransfer> MaininvoiceItems = new List<ItemTransfer>();
            int totalserialscount = 0;
            int seq = 0;
            foreach (var i in invoiceItems)
            {
                //check item quantity
                if (i.quantity > 0)
                {
                    int num = 0;
                    i.price = decimal.Parse(SectionData.DecTostring(i.price));
                    //add item row
                    seq++;
                    itemtrTemp = i;
                    itemtrTemp.newLocked = seq;
                    if (i.packageItems == null || i.packageItems.Count() == 0)
                    {
                        //normal 
                        //check if has sub rows
                        if ((i.packageItems == null || i.packageItems.Count() == 0) && //normal item
                            (!(i.itemSerials == null || i.itemSerials.Count() == 0) //has serial
                            || (i.warrantyName != null && i.warrantyName != "") // or has warranty
                                || (i.offerValue != null && i.offerValue > 0))) // or hasOffer
                        {
                            itemtrTemp.lockedQuantity = 1;// to hide bottom border 
                        }
                    }
                    else
                    {

                        itemtrTemp.itemUnitId = -5;//for main package

                        itemtrTemp.lockedQuantity = 1;// to hide bottom border 
                    }

                    if (itemtrTemp.unitName == "package")
                    {


                        itemtrTemp.itemUnitId = -5;//for main package
                    }



                    MaininvoiceItems.Add(itemtrTemp);
                    //normal item

                    if (i.packageItems == null || i.packageItems.Count() == 0)//normal item
                    {
                        //add OFFER
                        if (i.offerValue != null)
                        {
                            if (i.offerValue > 0)
                            {
                                itemtrTemp = new ItemTransfer();

                                //  itemtrTemp.itemName = i.offerType == 2 ? SectionData.PercentageDecTostring(i.offerValue) : SectionData.DecTostring(i.offerValue);
                                itemtrTemp.itemName = i.offerName;

                                itemtrTemp.itemUnitId = -7;//for OFFER
                                itemtrTemp.offerType = i.offerType;
                                itemtrTemp.offerValue = i.offerValue;
                                itemtrTemp.offerId = i.offerId;
                                MaininvoiceItems.Add(itemtrTemp);

                                totalserialscount++;
                            }

                        }
                        /*
                         =Parameters!trOfferOnRep.Value + ": " + iif(Fields!offerType.Value = 2, Fields!itemName.Value + " % ", Fields!itemName.Value + " " + Parameters!Currency.Value) + " " + Parameters!trDiscountOffer.Value
                         * */
                        //end add add OFFER
                        //add serials
                        if (!(i.itemSerials == null || i.itemSerials.Count() == 0))
                        {
                            //has serial
                            int firstnserial = 0;
                            foreach (Serial srow in i.itemSerials)
                            {
                                itemtrTemp = new ItemTransfer();
                                itemtrTemp.itemName = srow.serialNum;
                                if (firstnserial == 0)
                                {
                                    itemtrTemp.itemUnitId = -100;//for first serial
                                }
                                else
                                {
                                    itemtrTemp.itemUnitId = 0;//for serial
                                }

                                MaininvoiceItems.Add(itemtrTemp);

                                totalserialscount++;
                                firstnserial++;
                            }
                        }
                        //end add serials
                        //add warranty
                        if (i.warrantyName != null && i.warrantyName != "")
                        {
                            itemtrTemp = new ItemTransfer();
                            itemtrTemp.itemName = i.warrantyName;
                            itemtrTemp.itemUnitId = -1;//for warranty
                            MaininvoiceItems.Add(itemtrTemp);

                            totalserialscount++;
                        }
                        //end add warranty
                    }
                    else //package 
                    {

                        List<Item> packageitemsList = new List<Item>();

                        packageitemsList = i.packageItems;
                        //contents
                        string contents = "";
                        foreach (Item itemRow in packageitemsList)
                        {
                            contents += itemRow.itemCount + " " + itemRow.name + ", ";
                        }
                        contents = contents.Remove(contents.LastIndexOf(','));
                        //add contents of package row
                        itemtrTemp = new ItemTransfer();
                        itemtrTemp.itemName = contents;

                        itemtrTemp.itemUnitId = -6;//for contents of package
                        MaininvoiceItems.Add(itemtrTemp);
                        totalserialscount++;
                        //end add contents of package
                        //add OFFER
                        if (i.offerValue != null)
                        {
                            if (i.offerValue > 0)
                            {
                                itemtrTemp = new ItemTransfer();

                                //  itemtrTemp.itemName = i.offerType == 2 ? SectionData.PercentageDecTostring(i.offerValue) : SectionData.DecTostring(i.offerValue);
                                itemtrTemp.itemName = i.offerName;

                                itemtrTemp.itemUnitId = -7;//for OFFER
                                itemtrTemp.offerType = i.offerType;
                                itemtrTemp.offerValue = i.offerValue;
                                itemtrTemp.offerId = i.offerId;
                                MaininvoiceItems.Add(itemtrTemp);

                                totalserialscount++;
                            }

                        }
                        //end add add OFFER
                        //serials section
                        int firstserial = 0;
                        foreach (Item itemRow in packageitemsList)
                        {
                            //has serial
                            List<Serial> currentItemSerialsList = new List<Serial>();
                            if (!(i.itemSerials == null || i.itemSerials.Count() == 0))
                            {
                                currentItemSerialsList = i.itemSerials.Where(s => s.itemUnitId == itemRow.itemUnitId).ToList();
                            }

                            // check if has serials or waranty to add item row
                            if (currentItemSerialsList.Count > 0 //for serials

                                )
                            {
                                ////add item name row
                                //itemtrTemp = new ItemTransfer();
                                //itemtrTemp.itemName = itemRow.name;
                                //itemtrTemp.unitName = itemRow.unitName;
                                //itemtrTemp.itemUnitId = -2;//for item of package
                                //MaininvoiceItems.Add(itemtrTemp);
                                //totalserialscount++;
                                ////end add item name row
                                ///

                                //has serial

                                //add serials

                                foreach (Serial srow in currentItemSerialsList)
                                {

                                    itemtrTemp = new ItemTransfer();
                                    itemtrTemp.itemName = srow.serialNum;
                                    if (firstserial == 0)
                                    {
                                        itemtrTemp.itemUnitId = -33;//for first serial of  item package 
                                    }
                                    else
                                    {
                                        itemtrTemp.itemUnitId = -3;//for serial of  item package 
                                    }

                                    MaininvoiceItems.Add(itemtrTemp);
                                    totalserialscount++;
                                    firstserial++;
                                }
                                //end add serial


                                //has warranty
                                //if (itemRow.warrantyName != null && itemRow.warrantyName != "")
                                //{
                                //    //add warranty
                                //    itemtrTemp = new ItemTransfer();
                                //    itemtrTemp.itemName = itemRow.warrantyName;
                                //    itemtrTemp.itemUnitId = -4;//for warranty of  item package 
                                //    MaininvoiceItems.Add(itemtrTemp);
                                //    totalserialscount++;
                                //    //end add warranty
                                //}
                                //end has warranty
                            }


                        }
                        //warranty section
                        int firstwaranty = 0;
                        foreach (Item itemRow in packageitemsList)
                        {
                            //has serial
                            //List<Serial> currentItemSerialsList = new List<Serial>();
                            //if (!(i.itemSerials == null || i.itemSerials.Count() == 0))
                            //{
                            //    currentItemSerialsList = i.itemSerials.Where(s => s.itemUnitId == itemRow.itemUnitId).ToList();
                            //}

                            // check if has serials or waranty to add item row
                            if (
                                (itemRow.warrantyName != null && itemRow.warrantyName != "")//for warranty
                                )
                            {
                                ////add item name row
                                //itemtrTemp = new ItemTransfer();
                                //itemtrTemp.itemName = itemRow.name;
                                //itemtrTemp.unitName = itemRow.unitName;
                                //itemtrTemp.itemUnitId = -2;//for item of package
                                //MaininvoiceItems.Add(itemtrTemp);
                                //totalserialscount++;
                                ////end add item name row
                                //has serial
                                //if (currentItemSerialsList.Count > 0)
                                //{
                                //    //add serials
                                //    foreach (Serial srow in currentItemSerialsList)
                                //    {
                                //        itemtrTemp = new ItemTransfer();
                                //        itemtrTemp.itemName = srow.serialNum;
                                //        itemtrTemp.itemUnitId = -3;//for serial of  item package 
                                //        MaininvoiceItems.Add(itemtrTemp);
                                //        totalserialscount++;
                                //    }
                                //    //end add serial

                                //}
                                //has warranty

                                //add warranty
                                itemtrTemp = new ItemTransfer();
                                itemtrTemp.itemName = itemRow.name;
                                itemtrTemp.warrantyName = itemRow.warrantyName;
                                if (firstwaranty == 0)
                                {
                                    itemtrTemp.itemUnitId = -44;//for first warranty of  item package 
                                }
                                else
                                {
                                    itemtrTemp.itemUnitId = -4;//for warranty of  item package 
                                }

                                MaininvoiceItems.Add(itemtrTemp);
                                totalserialscount++;
                                //end add warranty
                                firstwaranty++;
                                //end has warranty

                            }


                        }


                    }


                    //end normal item

                    //  tmpserialLst = new List<string>();
                    //if (i.itemSerial != null && i.itemSerial != "")
                    //{

                    //    // num++;
                    //    tmpserialLst = strToList(i.itemSerial);
                    //    //add item name
                    //    //serialTemp = new Serial();
                    //    //serialTemp.serialNum = i.itemName;
                    //    //serialLst.Add(serialTemp);
                    //    //add serials of item
                    //    // num = 1;
                    //    //add serials rows for current item
                    //    foreach (string s in tmpserialLst)
                    //    {
                    //        itemtrTemp = new ItemTransfer();
                    //        itemtrTemp.itemName = s;
                    //        itemtrTemp.itemUnitId = 0;
                    //        MaininvoiceItems.Add(itemtrTemp);

                    //        totalserialscount++;
                    //    }
                    //}



                    //end if qty>0
                }
                //end foreach


            }


            serialsCount = totalserialscount;
            // serialsCount = serialLst.Count();
            //  rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetItemTransfer", MaininvoiceItems));
            rep.DataSources.Add(new ReportDataSource("DataSetSerial", serialLst));
            rep.EnableExternalImages = true;

        }
        public List<PayedInvclass> cashPayedinvoice(List<PayedInvclass> PayedInvclassList, Invoice prInvoice)
        {
            int i = 0;
            ReportCls reportclass = new ReportCls();
            List<PayedInvclass> mainPayedList = new List<PayedInvclass>();
            PayedInvclass newRow = new PayedInvclass();
            foreach (var p in PayedInvclassList)
            {
                //  newRow = new PayedInvclass();
                //add main row
                newRow = new PayedInvclass();
                if (p.processType == "cash")
                {
                    newRow.cash = decimal.Parse(reportclass.DecTostring(p.cash + prInvoice.cashReturn));
                    //if (AppSettings.invoice_lang == "both")
                    //{
                    //    newRow.cardName = MainWindow.resourcemanagerreport.GetString("trCashType") + " / " + MainWindow.resourcemanagerAr.GetString("trCashType");
                    //}
                    //else
                    //{
                    //    newRow.cardName = MainWindow.resourcemanagerreport.GetString("trCashType");
                    //}
              //    ResourceManager resourcemanageren = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    newRow.cardName = MainWindow.resourcemanagerEn.GetString("trCashType");
                    
                }
                else
                {
                    newRow.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                    newRow.cardName = p.cardName;
                }
             
                newRow.cardId = p.cardId;
                newRow.processType = p.processType;
                //    newRow.cash = p.cash;
            

                newRow.commissionRatio = p.commissionRatio;
                newRow.commissionValue = p.commissionValue;
                newRow.docNum = p.docNum;
                //  // check if card And  has processNum for border
                if (p.cardId > 0 && p.docNum != null && p.docNum != "")
                {
                    newRow.sequenc = -1;
                }
                mainPayedList.Add(newRow);
                //end add main row

                // check if card And  has processNum
                //if (p.cardId > 0 && p.docNum != null && p.docNum != "")
                //{

                //    //add Code row
                //    newRow = new PayedInvclass();
                //    newRow.cardId = -1;
                //    newRow.processType = p.processType;
                //    newRow.cash = 0;

                //    //   newRow.cardName = p.cardName;
                //    //newRow.sequenc = p.sequenc;
                //    //newRow.commissionRatio = p.commissionRatio;
                //    //newRow.commissionValue = p.commissionValue;
                //    newRow.docNum = p.docNum;
                //    mainPayedList.Add(newRow);
                //    //End add Code row
                //}
            }
            return mainPayedList;

        }



        //before change invoice design
        //public List<PayedInvclass> cashPayedinvoice(List<PayedInvclass> PayedInvclassList, Invoice prInvoice)
        //{
        //    int i = 0;
        //    ReportCls reportclass = new ReportCls();
        //    List<PayedInvclass> mainPayedList = new List<PayedInvclass>();
        //    PayedInvclass newRow = new PayedInvclass();
        //    foreach (var p in PayedInvclassList)
        //    {
        //        //  newRow = new PayedInvclass();
        //        //add main row
        //        newRow = new PayedInvclass();
        //        if (p.processType == "cash")
        //        {
        //            newRow.cash = decimal.Parse(reportclass.DecTostring(p.cash + prInvoice.cashReturn));
        //        }
        //        else
        //        {
        //            newRow.cash = decimal.Parse(reportclass.DecTostring(p.cash));
        //        }

        //        newRow.cardId = p.cardId;
        //        newRow.processType = p.processType;
        //        //    newRow.cash = p.cash;
        //        newRow.cardName = p.cardName;

        //        newRow.commissionRatio = p.commissionRatio;
        //        newRow.commissionValue = p.commissionValue;
        //        newRow.docNum = p.docNum;
        //        //  // check if card And  has processNum for border
        //        if (p.cardId > 0 && p.docNum != null && p.docNum != "")
        //        {
        //            newRow.sequenc = -1;
        //        }
        //        mainPayedList.Add(newRow);
        //        //end add main row
        //        // check if card And  has processNum
        //        if (p.cardId > 0 && p.docNum != null && p.docNum != "")
        //        {

        //            //add Code row
        //            newRow = new PayedInvclass();
        //            newRow.cardId = -1;
        //            newRow.processType = p.processType;
        //            newRow.cash = 0;

        //            //   newRow.cardName = p.cardName;
        //            //newRow.sequenc = p.sequenc;
        //            //newRow.commissionRatio = p.commissionRatio;
        //            //newRow.commissionValue = p.commissionValue;
        //            newRow.docNum = p.docNum;
        //            mainPayedList.Add(newRow);
        //            //End add Code row
        //        }
        //    }
        //    return mainPayedList;

        //}

        public static void storage(IEnumerable<Storage> storageQuery, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var r in storageQuery)
            {
                if (r.startDate != null)
                    r.startDate = DateTime.Parse(SectionData.DateToString(r.startDate));//
                if (r.endDate != null)
                    r.endDate = DateTime.Parse(SectionData.DateToString(r.endDate));
                //r.inventoryDate = DateTime.Parse(SectionData.DateToString(r.inventoryDate));
                //r.IupdateDate = DateTime.Parse(SectionData.DateToString(r.IupdateDate));

                //r.diffPercentage = decimal.Parse(SectionData.DecTostring(r.diffPercentage));
                r.storageCostValue = decimal.Parse(SectionData.DecTostring(r.storageCostValue));
                r.quantity = (int)r.quantity;
            }
            rep.DataSources.Add(new ReportDataSource("DataSetStorage", storageQuery));
        }

        /* free zone
         =iif((Fields!section.Value="FreeZone")And(Fields!location.Value="0-0-0"),
"-",Fields!section.Value+"-"+Fields!location.Value)
         * */

        public static void storageStock(IEnumerable<Storage> storageQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            storage(storageQuery, rep, reppath);
            DateFormConv(paramarr);
            paramarr.Add(new ReportParameter("trBranchStoreHint", MainWindow.resourcemanagerreport.GetString("trBranch/Store")));
            paramarr.Add(new ReportParameter("trStartDateHint", MainWindow.resourcemanagerreport.GetString("trStartDate")));

            paramarr.Add(new ReportParameter("trEndDateHint", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trItemUnit", MainWindow.resourcemanagerreport.GetString("trItemUnit")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trSectionLocation", MainWindow.resourcemanagerreport.GetString("trSectionLocation")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trExpiredDate", MainWindow.resourcemanagerreport.GetString("trExpiredDate")));
            paramarr.Add(new ReportParameter("trMinStock", MainWindow.resourcemanagerreport.GetString("trMinStock")));
            paramarr.Add(new ReportParameter("trMaxStock", MainWindow.resourcemanagerreport.GetString("trMaxStock")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            paramarr.Add(new ReportParameter("trSection", MainWindow.resourcemanagerreport.GetString("trSection")));
            paramarr.Add(new ReportParameter("trLocation", MainWindow.resourcemanagerreport.GetString("trLocation")));
        }
        // stocktaking
        public static void StocktakingArchivesReport(IEnumerable<InventoryClass> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            foreach (var r in Query)
            {

                //r.inventoryDate = DateTime.Parse(SectionData.DateToString(r.inventoryDate));
                //r.IupdateDate = DateTime.Parse(SectionData.DateToString(r.IupdateDate));

                r.diffPercentage = decimal.Parse(SectionData.DecTostring(r.diffPercentage));
                //r.storageCostValue = decimal.Parse(SectionData.DecTostring(r.storageCostValue));
            }


            rep.DataSources.Add(new ReportDataSource("DataSetInventoryClass", Query));
            DateFormConv(paramarr);
            InventoryTypeConv(paramarr);
        }

        public static void StocktakingShortfallsReport(IEnumerable<ItemTransferInvoice> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            //foreach (var r in Query)
            //{
            //    //if (r.startDate != null)
            //    //    r.startDate = DateTime.Parse(SectionData.DateToString(r.startDate));//
            //    //if (r.endDate != null)
            //    //    r.endDate = DateTime.Parse(SectionData.DateToString(r.endDate));

            //    //r.inventoryDate = DateTime.Parse(SectionData.DateToString(r.inventoryDate));
            //    //r.IupdateDate = DateTime.Parse(SectionData.DateToString(r.IupdateDate));

            //    //r.diffPercentage = decimal.Parse(SectionData.DecTostring(r.diffPercentage));
            //    //r.storageCostValue = decimal.Parse(SectionData.DecTostring(r.storageCostValue));
            //}


            rep.DataSources.Add(new ReportDataSource("DataSetItemTransferInvoice", Query));
            DateFormConv(paramarr);
            InventoryTypeConv(paramarr);

        }
        /*
        = Switch(Fields!inventoryType.Value="a",Parameters!trArchived.Value
,Fields!inventoryType.Value="n",Parameters!trSaved.Value
,Fields!inventoryType.Value="d",Parameters!trDraft.Value
)

         * */
        //

        public static void cashTransferStsBank(IEnumerable<CashTransferSts> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<CashTransferSts> cashTransfers = JsonConvert.DeserializeObject<List<CashTransferSts>>(JsonConvert.SerializeObject(Query));

            cashTransferSts(cashTransfers, rep, reppath);
            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));
            paramarr.Add(new ReportParameter("trPull", MainWindow.resourcemanagerreport.GetString("trPull")));
            paramarr.Add(new ReportParameter("trDeposit", MainWindow.resourcemanagerreport.GetString("trDeposit")));
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));

        }
        public static string archiveTypeConverter(string type)
        {
            string res = "";
            switch (type)
            {
                case "a": res = MainWindow.resourcemanagerreport.GetString("trArchived"); break;
                case "d": res = MainWindow.resourcemanagerreport.GetString("trDraft"); break;
                case "n": res = MainWindow.resourcemanagerreport.GetString("trSaved"); break;
                default: res = ""; break;
            };
            return res;
        }

        public static string processTypeAndCardConverter(string processType, string cardName,string transType)
        {
            string pType = processType;
            string cName = cardName;

            switch (pType)
            {
                case "statement":
                    if (transType == "p")
            { return MainWindow.resourcemanagerreport.GetString("trRequired"); }
            else { return MainWindow.resourcemanagerreport.GetString("trWorthy"); }
                case "cash": return MainWindow.resourcemanagerreport.GetString("trCash");
                //break;
                case "doc": return MainWindow.resourcemanagerreport.GetString("trDocument");
                //break;
                case "cheque": return MainWindow.resourcemanagerreport.GetString("trCheque");
                //break;
                case "balance": return MainWindow.resourcemanagerreport.GetString("trCredit");
                //break;
                case "card": return cName;
                //break;
                case "inv": return "-";
                case "multiple": return MainWindow.resourcemanagerreport.GetString("trMultiplePayment");

                case "commissionAgent":
                case "destroy":
                case "shortage":
                case "deliver": return "-";
                //break;
                default: return pType;
                    //break;
            }

        }
        public static string processTypeAndBankCardConverter(string processType, string cardName, string side)
        {
            //r.processType, r.cardName, r.side
            string pType = processType;
            string cName = cardName;
            if (side == "bn" || side == "p")
            {
                return MainWindow.resourcemanagerreport.GetString("trCash");


            }
            else
            {

                switch (pType)
                {
                    case "cash": return MainWindow.resourcemanagerreport.GetString("trCash");
                    //break;
                    case "doc": return MainWindow.resourcemanagerreport.GetString("trDocument");
                    //break;
                    case "cheque": return MainWindow.resourcemanagerreport.GetString("trCheque");
                    //break;
                    case "balance": return MainWindow.resourcemanagerreport.GetString("trCredit");
                    //break;
                    case "card": return cName;
                    //break;
                    case "inv": return "-";
                    case "multiple": return MainWindow.resourcemanagerreport.GetString("trMultiplePayment");
                    case "box": return MainWindow.resourcemanagerreport.GetString("trCash");//open box
                    //break;
                    default: return pType;
                        //break;
                }
            }

        }
        public static string StsStatementPaymentConvert(string value)
        {
            string s = "";
            switch (value)
            {
                case "cash":
                    s = MainWindow.resourcemanagerreport.GetString("trCash");
                    break;
                case "doc":
                    s = MainWindow.resourcemanagerreport.GetString("trDocument");
                    break;
                case "cheque":
                    s = MainWindow.resourcemanagerreport.GetString("trCheque");
                    break;
                case "balance":
                    s = MainWindow.resourcemanagerreport.GetString("trCredit");
                    break;
                case "card":
                    s = MainWindow.resourcemanagerreport.GetString("trAnotherPaymentMethods");
                    break;
                case "inv":
                    s = MainWindow.resourcemanagerreport.GetString("trInv");
                    break;
                default:
                    s = value;
                    break;


            }
            return s;
        }
        public static string PaymenttypeConvert(string value)
        {
            string s = "";
            switch (value)
            {
                case "cash":
                    s = MainWindow.resourcemanagerreport.GetString("trCash");
                    break;
                case "doc":
                    s = MainWindow.resourcemanagerreport.GetString("trDocument");
                    break;
                case "cheque":
                    s = MainWindow.resourcemanagerreport.GetString("trCheque");
                    break;
                case "balance":
                    s = MainWindow.resourcemanagerreport.GetString("trCredit");
                    break;
                case "card":
                    s = MainWindow.resourcemanagerreport.GetString("trAnotherPaymentMethods");
                    break;
                case "inv":
                    s = MainWindow.resourcemanagerreport.GetString("trInv");
                    break;
                default:
                    s = value;
                    break;


            }
            return s;
        }
        public static string PaymentConvert(string processType, string cardName)
        {
            string s = "";
            switch (processType)
            {
                case "cash":
                    s = MainWindow.resourcemanagerreport.GetString("trCash");
                    break;
                case "doc":
                    s = MainWindow.resourcemanagerreport.GetString("trDocument");
                    break;
                case "cheque":
                    s = MainWindow.resourcemanagerreport.GetString("trCheque");
                    break;
                case "balance":
                    s = MainWindow.resourcemanagerreport.GetString("trCredit");
                    break;
                case "card":
                    s = cardName;
                    break;
                case "inv":
                    s = MainWindow.resourcemanagerreport.GetString("trInv");
                    break;
                default:
                    s = processType;
                    break;


            }
            return s;
        }
        public static void cashTransferStsStatement(IEnumerable<CashTransferSts> cashTransfers, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            cashTransferStatSts(cashTransfers, rep, reppath);

            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo")));

        }
        public static void cashTransferStsPayment(IEnumerable<CashTransferSts> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<CashTransferSts> cashTransfers = JsonConvert.DeserializeObject<List<CashTransferSts>>(JsonConvert.SerializeObject(Query));

            cashTransferSts(cashTransfers, rep, reppath);

          //  cashTransferProcessTypeConv(paramarr);
            DateFormConv(paramarr);
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo")));

            paramarr.Add(new ReportParameter("trPaymentType", MainWindow.resourcemanagerreport.GetString("trPaymentType")));
            paramarr.Add(new ReportParameter("trAccoutant", MainWindow.resourcemanagerreport.GetString("trAccoutant")));
            paramarr.Add(new ReportParameter("trRecipientTooltip", MainWindow.resourcemanagerreport.GetString("trRecipientTooltip")));
            paramarr.Add(new ReportParameter("trRecepient", MainWindow.resourcemanagerreport.GetString("trRecepient")));
            paramarr.Add(new ReportParameter("trCompany", MainWindow.resourcemanagerreport.GetString("trCompany")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trAmount", MainWindow.resourcemanagerreport.GetString("trAmount")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            paramarr.Add(new ReportParameter("trNote", MainWindow.resourcemanagerreport.GetString("trNote")));
            paramarr.Add(new ReportParameter("trPosTooltip", MainWindow.resourcemanagerreport.GetString("trPosTooltip")));


        }
        public static void cashTransferStsPos(IEnumerable<CashTransferSts> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<CashTransferSts> cashTransfers = JsonConvert.DeserializeObject<List<CashTransferSts>>(JsonConvert.SerializeObject(Query));

            cashTransferSts(cashTransfers, rep, reppath);
            cashTransTypeConv(paramarr);
            DateFormConv(paramarr);

        }

        public static void cashTransferSts(IEnumerable<CashTransferSts> cashTransfers, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var r in cashTransfers)
            {
                r.updateDate = DateTime.Parse(SectionData.DateToString(r.updateDate));
                r.cash = decimal.Parse(SectionData.DecTostring(r.cash));
                r.agentName = AgentUnKnownConvert(r.agentId, r.side, r.agentName);
                r.agentCompany = AgentCompanyUnKnownConvert(r.agentId, r.side, r.agentCompany);
                r.processType = processTypeConvswitch(r.processType, r.cardName);
            }
            rep.DataSources.Add(new ReportDataSource("DataSetCashTransferSts", cashTransfers));
        }
        public static string AgentUnKnownConvert(int? agentId, string side, string agentName)
        {

            if (agentId == null|| agentId == 0)
            {
                if (side == "v")
                {
                    agentName = MainWindow.resourcemanagerreport.GetString("trUnKnown");
                }
                else if (side == "c")
                {
                    agentName = MainWindow.resourcemanagerreport.GetString("trCashCustomer");
                }
            }
            return agentName;

        }
        public static string AgentCompanyUnKnownConvert(int? agentId, string side, string agentCompany)
        {
            if (agentId == null || agentId == 0)
            {
                //  agentCompany = MainWindow.resourcemanagerreport.GetString("trUnKnown");
                agentCompany = "";
            }
            return agentCompany;
        }
        public static void cashTransferStatSts(IEnumerable<CashTransferSts> cashTransfers, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (CashTransferSts r in cashTransfers)
            {
                r.updateDate = DateTime.Parse(SectionData.DateToString(r.updateDate));
                r.cash = decimal.Parse(SectionData.DecTostring(r.cash));

                r.paymentreport = processTypeAndCardConverter(r.Description3, r.cardName,r.transType);


            }
            rep.DataSources.Add(new ReportDataSource("DataSetCashTransferSts", cashTransfers));
        }

        public static void FundStsReport(IEnumerable<BalanceSTS> query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var r in query)
            {

                r.balance = decimal.Parse(SectionData.DecTostring(r.balance));
            }
            rep.DataSources.Add(new ReportDataSource("DataSetBalanceSTS", query));
            paramarr.Add(new ReportParameter("title", MainWindow.resourcemanagerreport.GetString("trBalance")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));


        }
        public static void ClosingStsReport(IEnumerable<POSOpenCloseModel> query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var r in query)
            {
                r.cash = decimal.Parse(SectionData.DecTostring(r.cash));
                r.openCash = decimal.Parse(SectionData.DecTostring(r.openCash));

            }
            rep.DataSources.Add(new ReportDataSource("DataSetBalanceSTS", query));
            paramarr.Add(new ReportParameter("trNum", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trPOS", MainWindow.resourcemanagerreport.GetString("trPOS")));
            paramarr.Add(new ReportParameter("trOpenDate", MainWindow.resourcemanagerreport.GetString("trOpenDate")));
            paramarr.Add(new ReportParameter("trOpenCash", MainWindow.resourcemanagerreport.GetString("trOpenCash")));
            paramarr.Add(new ReportParameter("trCloseDate", MainWindow.resourcemanagerreport.GetString("trCloseDate")));
            paramarr.Add(new ReportParameter("trCloseCash", MainWindow.resourcemanagerreport.GetString("trCloseCash")));

            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));


        }
        public static void ClosingOpStsReport(IEnumerable<OpenClosOperatinModel> query, LocalReport rep, string reppath, List<ReportParameter> paramarr, POSOpenCloseModel openclosrow)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var r in query)
            {
                r.cash = decimal.Parse(SectionData.DecTostring(r.cash));
                //  r.openCash = decimal.Parse(SectionData.DecTostring(r.openCash));
                //    r.notes = closingDescriptonConverter(r);
                r.notes = closingDescriptonBoxConverter(r);

            }
            rep.DataSources.Add(new ReportDataSource("DataSetBalanceSTS", query));
            paramarr.Add(new ReportParameter("trNum", MainWindow.resourcemanagerreport.GetString("trNo")));
            paramarr.Add(new ReportParameter("trPOS", MainWindow.resourcemanagerreport.GetString("trPOS")));
            paramarr.Add(new ReportParameter("trOpenDate", MainWindow.resourcemanagerreport.GetString("trOpenDate")));
            paramarr.Add(new ReportParameter("trOpenCash", MainWindow.resourcemanagerreport.GetString("trOpenCash")));
            paramarr.Add(new ReportParameter("trCloseDate", MainWindow.resourcemanagerreport.GetString("trCloseDate")));
            paramarr.Add(new ReportParameter("trCloseCash", MainWindow.resourcemanagerreport.GetString("trCloseCash")));

            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));

            paramarr.Add(new ReportParameter("OpenDate", openclosrow.openDate.ToString()));
            paramarr.Add(new ReportParameter("OpenCash", SectionData.DecTostring(openclosrow.openCash)));
            paramarr.Add(new ReportParameter("CloseDate", openclosrow.updateDate.ToString()));
            paramarr.Add(new ReportParameter("CloseCash", SectionData.DecTostring(openclosrow.cash)));
            paramarr.Add(new ReportParameter("pos", openclosrow.branchName + " / " + openclosrow.posName));

            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trDescription", MainWindow.resourcemanagerreport.GetString("trDescription")));
            paramarr.Add(new ReportParameter("trCashTooltip", MainWindow.resourcemanagerreport.GetString("trCashTooltip")));



        }

        public static void BoxStateReport(IEnumerable<OpenClosOperatinModel> ListQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr, List<CardsSts> cardList, string totalCash, string branchName, string posName)
        {
            List<OpenClosOperatinModel> Query = JsonConvert.DeserializeObject<List<OpenClosOperatinModel>>(JsonConvert.SerializeObject(ListQuery));
            List<CardsSts> cardtransList = JsonConvert.DeserializeObject<List<CardsSts>>(JsonConvert.SerializeObject(cardList));

            //   cardtransList = cardtransList.Where(x => x.cardId > 0).ToList();
            List<OpenClosOperatinModel> finalQuery = Query.Where(x => x.processType != "box").ToList();
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            bool open = false;
            decimal sumCash = 0;
            decimal totalValue = 0;
            decimal openCash = 0;
            DateTime? opendate;
            string desc;
            OpenClosOperatinModel openrow = Query.Where(x => x.transType == "o" && x.processType == "box").FirstOrDefault();
            OpenClosOperatinModel closerow = Query.Where(x => x.transType == "c" && x.processType == "box").FirstOrDefault();
            // get box status -open or close
            if (openrow == null || closerow == null)
            {
                if (closerow == null)
                {
                    paramarr.Add(new ReportParameter("CloseDate", "-"));
                    paramarr.Add(new ReportParameter("CloseCash", "-"));
                }
                else
                {

                }
                if (openrow == null)
                {
                    paramarr.Add(new ReportParameter("OpenDate", "-"));

                    paramarr.Add(new ReportParameter("OpenCash", "0"));
                }
                else
                {
                    paramarr.Add(new ReportParameter("OpenDate", openrow.updateDate.ToString()));

                    paramarr.Add(new ReportParameter("OpenCash", SectionData.DecTostring(openrow.cash)));
                   
                }

            }
            else
            {


                if (openrow.updateDate > closerow.updateDate)
                {
                    open = true;
                    paramarr.Add(new ReportParameter("CloseDate", "-"));
                    paramarr.Add(new ReportParameter("CloseCash", "-"));

                }
                else
                {
                    open = false;
                    paramarr.Add(new ReportParameter("CloseDate", closerow.updateDate.ToString()));
                    paramarr.Add(new ReportParameter("CloseCash", SectionData.DecTostring(closerow.cash)));


                }
                paramarr.Add(new ReportParameter("OpenDate", openrow.updateDate.ToString()));

                paramarr.Add(new ReportParameter("OpenCash", SectionData.DecTostring(openrow.cash)));
            }
            //open
            if (openrow == null)
            {
              
            }
            else
            {
                paramarr.Add(new ReportParameter("OpenDate", openrow.updateDate.ToString()));

                paramarr.Add(new ReportParameter("OpenCash", SectionData.DecTostring(openrow.cash)));
                finalQuery.Insert(0, openrow);
            }
            paramarr.Add(new ReportParameter("trCloseCash", MainWindow.resourcemanagerreport.GetString("trCloseCash")));

            decimal pay = 0;
            decimal deposit = 0;
            //pay = (decimal)finalQuery.Where(x => (x.processType == "cash" && x.transType == "p") || (x.side == "bn" && x.transType == "d")).Sum(x => x.cash);
            //deposit = (decimal)finalQuery.Where(x => (x.processType == "cash" && x.transType == "d") || (x.side == "bn" && x.transType == "p")).Sum(x => x.cash);
            //sumCash = deposit - pay;
            // sumCash = (decimal)finalQuery.Where(x => x.processType == "cash").Sum(x => x.cash);
            //paramarr.Add(new ReportParameter("Cash", SectionData.DecTostring(sumCash)));
            //paramarr.Add(new ReportParameter("trCash", MainWindow.resourcemanagerreport.GetString("trCash")));
            //
            //boxcash
            paramarr.Add(new ReportParameter("totalCash", totalCash));
            paramarr.Add(new ReportParameter("trTotalCash", MainWindow.resourcemanagerreport.GetString("trCashBalance")));
            //total all
            pay = 0;
            deposit = 0;
            //pay = (decimal)finalQuery.Where(x => (x.side != "bn" && x.transType == "p") || (x.side == "bn" && x.transType == "d")).Sum(x => x.cash);
            //deposit = (decimal)finalQuery.Where(x => (x.side != "bn" && x.transType == "d") || (x.side == "bn" && x.transType == "p")).Sum(x => x.cash);

            pay = (decimal)finalQuery.Where(x => ((x.processType == "cash" || x.processType == "card") && x.transType == "p") || (x.side == "p" && x.transType == "d") || (x.side == "bn" && x.transType == "d") ).Sum(x => x.cash);
            deposit = (decimal)finalQuery.Where(x => ((x.processType == "cash" || x.processType == "card") && x.transType == "d") || (x.side == "p" && x.transType == "p") || (x.side == "bn" && x.transType == "p") || (x.processType == "box" || x.transType == "o")).Sum(x => x.cash);

            totalValue = deposit - pay;
            //  totalValue = (decimal)finalQuery.Sum(x => x.cash);
            paramarr.Add(new ReportParameter("totalValue", SectionData.DecTostring(totalValue)));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            //
            foreach (var r in finalQuery)
            {
                //if ((((r.processType == "cash" || r.processType == "card") && r.transType == "d") || (r.side == "p" && r.transType == "p") || (r.side == "bn" && r.transType == "p") || (r.processType == "box" || r.transType == "o"))) {
                //    //deposit +
                //    r.cash = +r.cash;
                //} else
                if ((((r.processType == "cash" || r.processType == "card") && r.transType == "p") || (r.side == "p" && r.transType == "d") || (r.side == "bn" && r.transType == "d")))
                {
                    //pay -
                    r.cash = -r.cash;
                }
                    //box: processType:box ,transType=o
                    r.cash = decimal.Parse(SectionData.DecTostring(r.cash));
                //  r.openCash = decimal.Parse(SectionData.DecTostring(r.openCash));
                r.notes = closingDescriptonBoxConverter(r);
                r.processType = processTypeAndBankCardConverter(r.processType, r.cardName, r.side);//r.processType, r.cardName, r.side
            }
            rep.DataSources.Add(new ReportDataSource("DataSetBalanceSTS", finalQuery));
            paramarr.Add(new ReportParameter("trNum", MainWindow.resourcemanagerreport.GetString("trNo")));
            paramarr.Add(new ReportParameter("trPOS", MainWindow.resourcemanagerreport.GetString("trPOS")));
            paramarr.Add(new ReportParameter("trOpenDate", MainWindow.resourcemanagerreport.GetString("trOpenDate")));
            paramarr.Add(new ReportParameter("trOpenCash", MainWindow.resourcemanagerreport.GetString("trOpenCash")));
            paramarr.Add(new ReportParameter("trCloseDate", MainWindow.resourcemanagerreport.GetString("trCloseDate")));


            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));



            //paramarr.Add(new ReportParameter("CloseDate", openclosrow.updateDate.ToString()));
            //paramarr.Add(new ReportParameter("CloseCash", SectionData.DecTostring(openclosrow.cash)));
            paramarr.Add(new ReportParameter("pos", branchName + " / " + posName));

            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trDescription", MainWindow.resourcemanagerreport.GetString("trDescription")));
            paramarr.Add(new ReportParameter("trCashTooltip", MainWindow.resourcemanagerreport.GetString("trCashTooltip")));
            paramarr.Add(new ReportParameter("trPaymentType", MainWindow.resourcemanagerreport.GetString("trPaymentType")));
            paramarr.Add(new ReportParameter("trPaymentMethodsheader", MainWindow.resourcemanagerreport.GetString("trPaymentMethods")));
            int cardcount = 0;
          
            //cash 
            cardtransList[0].name = MainWindow.resourcemanagerreport.GetString("trCash");
            cardtransList[0].total = cardtransList[0].total+ openrow.cash;
            //cardtransList[0].total=    sumCash
            //if (cardtransList.Sum(x => x.total) == 0)
            //{
                cardcount = 1;
            //}
            paramarr.Add(new ReportParameter("cardcount", cardcount.ToString()));
            paramarr.Add(new ReportParameter("trNoData", MainWindow.resourcemanagerreport.GetString("thereArenodata")));
            rep.DataSources.Add(new ReportDataSource("CardsSts", cardtransList));

        }
        public static CardsSts BoxCashCalc(List<OpenClosOperatinModel> Query)
        {
            CardsSts cardcashrow = new CardsSts();
            // cash
            decimal sumCash = 0;
            decimal pay = 0;
            decimal deposit = 0;
            pay = (decimal)Query.Where(x => (x.processType == "cash" && x.transType == "p") || (x.side == "p" && x.transType == "d") || (x.side == "bn" && x.transType == "d")).Sum(x => x.cash);
            deposit = (decimal)Query.Where(x => (x.processType == "cash" && x.transType == "d") || (x.side == "p" && x.transType == "p") || (x.side == "bn" && x.transType == "p")).Sum(x => x.cash);

            sumCash = deposit - pay;
            cardcashrow.name = "cash";
            cardcashrow.total = decimal.Parse(SectionData.DecTostring(sumCash)); ;

            return cardcashrow;
        }
        public static CardsSts BoxOpenCashCalc(List<OpenClosOperatinModel> Query)
        {
            CardsSts cardcashrow = new CardsSts();         
         //   pay = (decimal)Query.Where(x => (x.processType == "cash" && x.transType == "p") || (x.side == "p" && x.transType == "d") || (x.side == "bn" && x.transType == "d")).Sum(x => x.cash);
          //  deposit = (decimal)Query.Where(x => (x.processType == "cash" && x.transType == "d") || (x.side == "p" && x.transType == "p") || (x.side == "bn" && x.transType == "p")).Sum(x => x.cash);
            OpenClosOperatinModel openrow = Query.Where(x => x.transType == "o" && x.processType == "box").FirstOrDefault();

            if (openrow == null)
            {
                cardcashrow.name = "";
                cardcashrow.total = decimal.Parse(SectionData.DecTostring(0));
            }
            else
            {                              
                cardcashrow.name = "obox";
                cardcashrow.total = decimal.Parse(SectionData.DecTostring(openrow.cash));
            }      

            return cardcashrow;
        }
        public static string ConvertInvType(string invType)
        {
            string value = "";
            value = invType;

            try
            {

                switch (value)
                {
                    //مشتريات 
                    case "p":
                        value = MainWindow.resourcemanagerreport.GetString("trPurchaseInvoice");
                        break;
                    //فاتورة مشتريات بانتظار الادخال
                    case "pw":
                        value = MainWindow.resourcemanagerreport.GetString("trPurchaseInvoiceWaiting");
                        break;
                    //مبيعات
                    case "s":
                        value = MainWindow.resourcemanagerreport.GetString("trSalesInvoice");
                        break;
                    //مرتجع مبيعات
                    case "sb":
                        value = MainWindow.resourcemanagerreport.GetString("trSalesReturnInvoice");
                        break;
                    //مرتجع مشتريات
                    case "pb":
                        value = MainWindow.resourcemanagerreport.GetString("trPurchaseReturnInvoice");
                        break;
                    //فاتورة مرتجع مشتريات بانتظار الاخراج
                    case "pbw":
                        value = MainWindow.resourcemanagerreport.GetString("trPurchaseReturnInvoiceWaiting");
                        break;
                    //مسودة مشتريات 
                    case "pd":
                        value = MainWindow.resourcemanagerreport.GetString("trDraftPurchaseBill");
                        break;
                    //مسودة مبيعات
                    case "sd":
                        value = MainWindow.resourcemanagerreport.GetString("trSalesDraft");
                        break;
                    //مسودة مرتجع مبيعات
                    case "sbd":
                        value = MainWindow.resourcemanagerreport.GetString("trSalesReturnDraft");
                        break;
                    //مسودة مرتجع مشتريات
                    case "pbd":
                        value = MainWindow.resourcemanagerreport.GetString("trPurchaseReturnDraft");
                        break;
                    // مسودة طلبية مبيعا 
                    case "ord":
                        value = MainWindow.resourcemanagerreport.GetString("trDraft");
                        break;
                    //طلبية مبيعات 
                    case "or":
                        value = MainWindow.resourcemanagerreport.GetString("trSaleOrder");
                        break;
                    //مسودة طلبية شراء 
                    case "pod":
                        value = MainWindow.resourcemanagerreport.GetString("trDraft");
                        break;
                    //طلبية شراء 
                    case "po":
                        value = MainWindow.resourcemanagerreport.GetString("trPurchaceOrder");
                        break;
                    // طلبية شراء أو بيع محفوظة
                    case "pos":
                    case "ors":
                        value = MainWindow.resourcemanagerreport.GetString("trSaved");
                        break;
                    //مسودة عرض 
                    case "qd":
                        value = MainWindow.resourcemanagerreport.GetString("trQuotationsDraft");
                        break;
                    //عرض سعر محفوظ
                    case "qs":
                        value = MainWindow.resourcemanagerreport.GetString("trSaved");
                        break;
                    //فاتورة عرض اسعار
                    case "q":
                        value = MainWindow.resourcemanagerreport.GetString("trQuotations");
                        break;
                    //الإتلاف
                    case "d":
                        value = MainWindow.resourcemanagerreport.GetString("trDestructive");
                        break;
                    //النواقص
                    case "sh":
                        value = MainWindow.resourcemanagerreport.GetString("trShortage");
                        break;
                    //مسودة  استراد
                    case "imd":
                        value = MainWindow.resourcemanagerreport.GetString("trImportDraft");
                        break;
                    // استراد
                    case "im":
                        value = MainWindow.resourcemanagerreport.GetString("trImport");
                        break;
                    // طلب استيراد
                    case "imw":
                        value = MainWindow.resourcemanagerreport.GetString("trImportOrder");
                        break;
                    //مسودة تصدير
                    case "exd":
                        value = MainWindow.resourcemanagerreport.GetString("trExportDraft");
                        break;
                    // تصدير
                    case "ex":
                        value = MainWindow.resourcemanagerreport.GetString("trExport");
                        break;
                    // طلب تصدير
                    case "exw":
                        value = MainWindow.resourcemanagerreport.GetString("trExportOrder");
                        break;
                    // إدخال مباشر
                    case "is":
                        value = MainWindow.resourcemanagerreport.GetString("trDirectEntry");
                        break;
                    // مسودة إدخال مباشر
                    case "isd":
                        value = MainWindow.resourcemanagerreport.GetString("trDirectEntryDraft");
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
        public static string closingDescriptonConverter(OpenClosOperatinModel s)
        {

            string name = "";
            switch (s.side)
            {
                case "bnd": break;
                case "v": name = MainWindow.resourcemanagerreport.GetString("trVendor"); break;
                case "c": name = MainWindow.resourcemanagerreport.GetString("trCustomer"); break;
                case "u": name = MainWindow.resourcemanagerreport.GetString("trUser"); break;
                case "s": name = MainWindow.resourcemanagerreport.GetString("trSalary"); break;
                case "e": name = MainWindow.resourcemanagerreport.GetString("trGeneralExpenses"); break;
                case "m":
                    if (s.transType == "d")
                        name = MainWindow.resourcemanagerreport.GetString("trAdministrativeDeposit");
                    if (s.transType == "p")
                        name = MainWindow.resourcemanagerreport.GetString("trAdministrativePull");
                    break;
                case "sh": name = MainWindow.resourcemanagerreport.GetString("trShippingCompany"); break;
                case "tax": name = MainWindow.resourcemanagerreport.GetString("trTaxCollection"); break;

                default: break;
            }

            if (!string.IsNullOrEmpty(s.agentName))
                name = name + " " + s.agentName;
            else if (!string.IsNullOrEmpty(s.usersName) && !string.IsNullOrEmpty(s.usersLName))
            {
                name = name + " " + s.usersName + " " + s.usersLName;
            }
            else if (!string.IsNullOrEmpty(s.shippingCompanyName))
                name = name + " " + s.shippingCompanyName;
            else if ((s.side != "e") && (s.side != "m"))
                name = name + " " + MainWindow.resourcemanagerreport.GetString("trUnKnown");

            if (s.transType.Equals("p"))
            {
                if ((s.side.Equals("bn")) || (s.side.Equals("p")))
                {
                    return MainWindow.resourcemanagerreport.GetString("trPull") + " " +
                           MainWindow.resourcemanagerreport.GetString("trFrom") + " " +
                           name;//receive
                }
                else if ((!s.side.Equals("bn")) || (!s.side.Equals("p")))
                {
                    return MainWindow.resourcemanagerreport.GetString("trPayment") + " " +
                           MainWindow.resourcemanagerreport.GetString("trTo") + " " +
                           name;//دفع
                }
                else return "";
            }
            else if (s.transType.Equals("d"))
            {
                if ((s.side.Equals("bn")) || (s.side.Equals("p")))
                {
                    return MainWindow.resourcemanagerreport.GetString("trDeposit") + " " +
                           MainWindow.resourcemanagerreport.GetString("trTo") + " " +
                           name;
                }
                else if ((!s.side.Equals("bn")) || (!s.side.Equals("p")))
                {
                    return MainWindow.resourcemanagerreport.GetString("trReceiptOperation") + " " +
                           MainWindow.resourcemanagerreport.GetString("trFrom") + " " +
                           name;//قبض
                }
                else return "";
            }
            else return "";

        }
        public static string closingDescriptonBoxConverter(OpenClosOperatinModel s)
        {
            //trPosTooltip
            string name = "";
            string finalstring = "";
            switch (s.side)
            {
                case "bnd": break;
                case "v": name = MainWindow.resourcemanagerreport.GetString("trVendor"); break;
                case "c": name = MainWindow.resourcemanagerreport.GetString("trCustomer"); break;
                case "u": name = MainWindow.resourcemanagerreport.GetString("trUser"); break;
                case "s": name = MainWindow.resourcemanagerreport.GetString("trSalary"); break;
                case "e": name = MainWindow.resourcemanagerreport.GetString("trGeneralExpenses"); break;
                case "m":
                    if (s.transType == "d")
                        name = MainWindow.resourcemanagerreport.GetString("trAdministrativeDeposit");
                    if (s.transType == "p")
                        name = MainWindow.resourcemanagerreport.GetString("trAdministrativePull");
                    break;
                case "sh": name = MainWindow.resourcemanagerreport.GetString("trShippingCompany"); break;
                case "tax": name = MainWindow.resourcemanagerreport.GetString("trTaxCollection"); break;
                case "bn": name = MainWindow.resourcemanagerreport.GetString("trBank"); break;
                case "p": name = MainWindow.resourcemanagerreport.GetString("trPosTooltip"); break;
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
                name = name + " " + MainWindow.resourcemanagerreport.GetString("trUnKnown");

            if (s.transType.Equals("p"))
            {
                if (s.side.Equals("bn") || s.side.Equals("p"))
                {
                    return MainWindow.resourcemanagerreport.GetString("trPull") + " " +
                           MainWindow.resourcemanagerreport.GetString("trFrom") + " " +
                           name;//receive
                }
                else if (!s.side.Equals("bn") && !s.side.Equals("p"))
                {
                    finalstring = MainWindow.resourcemanagerreport.GetString("trPayment") + " " +
                           MainWindow.resourcemanagerreport.GetString("trTo") + " " +
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
                    return MainWindow.resourcemanagerreport.GetString("trDeposit") + " " +
                           MainWindow.resourcemanagerreport.GetString("trTo") + " " +
                           name;
                }
                else if (!s.side.Equals("bn") && !s.side.Equals("p"))
                {
                    finalstring = MainWindow.resourcemanagerreport.GetString("trReceiptOperation") + " " +
                           MainWindow.resourcemanagerreport.GetString("trFrom") + " " +
                           name;//قبض
                    if (s.invId != null)
                    {
                        finalstring = finalstring + " - " + ConvertInvType(s.invType) + " #: " + s.invNumber;
                    }
                    return finalstring;

                }
                else return "";
            }else if (s.transType=="o" && s.processType=="box")
            { 
                //open box: processType:box ,transType=o
                finalstring = MainWindow.resourcemanagerreport.GetString("Cashdrawercontents")+  " - " +MainWindow.resourcemanagerreport.GetString("trOpenCash");
              
                return finalstring;
            }
            else return "";

        }
        //calc totals
        //public async static Task  calcTotalscash(IEnumerable<OpenClosOperatinModel> Query, List<ReportParameter> paramarr)
        //{
        //    List<OpenClosOperatinModel> tempquery = JsonConvert.DeserializeObject<List<OpenClosOperatinModel>>(JsonConvert.SerializeObject(Query));
        //    List<OpenClosOperatinModel> queryrOps = tempquery.Where(x => x.processType != "box").ToList();
        //    decimal sumCash = 0;

        //    decimal sumCashRow = 0;
        //    decimal totalValue = 0;

        //    List<Card> cardlist = new List<Card>();

        //    if (FillCombo.cardsList == null)
        //        await FillCombo.RefreshCards();
        //    cardlist = FillCombo.cardsList;
        ////    Card cr = new Card();
        //    List<CardsSts> cardtransList = new List<CardsSts>();


        //    foreach (Card card in cardlist)
        //    {
        //        CardsSts tempcard = new CardsSts();

        //        tempcard.cardId = card.cardId;
        //        tempcard.name = card.name;
        //        tempcard.hasProcessNum = card.hasProcessNum;
        //        tempcard.image = card.image;
        //        tempcard.isActive = card.isActive;
        //        tempcard.total = 0;
        //        cardtransList.Add(tempcard);
        //    }
        //    sumCashRow = (decimal)queryrOps.Where(x => x.processType == "cash").Sum(x => x.cash);

        //        //card sum
        //            foreach (CardsSts card in cardtransList)
        //            {
        //            card.total = (decimal)queryrOps.Where(x => x.processType == "card" && x.cardId == card.cardId).Sum(x => x.cash);


        //            }
        //    //converter
        //    decimal totalcards = 0;
        //    totalcards=(decimal) cardtransList.Sum(x => x.total);
        //    foreach (CardsSts card in cardtransList)
        //    {
        //       // totalcards += (decimal)card.total;
        //        card.total = decimal.Parse(SectionData.DecTostring(card.total));
        //    }
        //    totalValue = totalcards + sumCash ;

        //    //paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
        //    //
        //    //paramarr.Add(new ReportParameter("isTax", AppSettings.invoiceTax_bool.ToString()));
        //    //paramarr.Add(new ReportParameter("invDate", date));

        //    //paramarr.Add(new ReportParameter("trPaymentMethodsheader", MainWindow.resourcemanagerreport.GetString("trPaymentMethods")));

        //    //paramarr.Add(new ReportParameter("trCash", MainWindow.resourcemanagerreport.GetString("trCash")));
        //    //paramarr.Add(new ReportParameter("trDocument", MainWindow.resourcemanagerreport.GetString("trDocument")));
        //    //paramarr.Add(new ReportParameter("trCheque", MainWindow.resourcemanagerreport.GetString("trCheque")));
        //    //paramarr.Add(new ReportParameter("trCredit", MainWindow.resourcemanagerreport.GetString("trCredit")));
        //    //paramarr.Add(new ReportParameter("trMultiplePayment", MainWindow.resourcemanagerreport.GetString("trMultiplePayment")));

        //    //paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo")));
        //    //paramarr.Add(new ReportParameter("trType", MainWindow.resourcemanagerreport.GetString("trType")));
        //    //paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
        //    //paramarr.Add(new ReportParameter("trPOS", MainWindow.resourcemanagerreport.GetString("trPOS")));
        //    //paramarr.Add(new ReportParameter("trUser", MainWindow.resourcemanagerreport.GetString("trUser")));
        //    //paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
        //    //paramarr.Add(new ReportParameter("trPaymentValue", MainWindow.resourcemanagerreport.GetString("trPaymentValue")));
        //    //paramarr.Add(new ReportParameter("trPaymentType", MainWindow.resourcemanagerreport.GetString("trPaymentType")));
        //    //paramarr.Add(new ReportParameter("trPayments", MainWindow.resourcemanagerreport.GetString("trPayments")));


        //    //paramarr.Add(new ReportParameter("trInvoicesDate", MainWindow.resourcemanagerreport.GetString("trInvoicesDate")));
        //    ////values
        //    //paramarr.Add(new ReportParameter("totalValue", SectionData.DecTostring(totalValue)));

        //    //paramarr.Add(new ReportParameter("Cash", SectionData.DecTostring(sumCash)));
        //    //paramarr.Add(new ReportParameter("Credit", SectionData.DecTostring(sumCredit)));

        ////    paramarr.Add(new ReportParameter("totalCards", totalcards.ToString()));
        //   // itemTransferInvTypeConv(paramarr);
        //    //rep.DataSources.Add(new ReportDataSource("DataSetITinvoice", tempquery));
        //    //rep.DataSources.Add(new ReportDataSource("CardsSts", cardtransList));
        //}

        public async static Task<List<CardsSts>> calctotalCards(IEnumerable<OpenClosOperatinModel> Query)
        {
            List<OpenClosOperatinModel> tempquery = JsonConvert.DeserializeObject<List<OpenClosOperatinModel>>(JsonConvert.SerializeObject(Query));
            List<OpenClosOperatinModel> queryrOps = tempquery.Where(x => x.processType != "box").ToList();
            List<Card> cardlist = new List<Card>();
            if (FillCombo.cardsList == null)
                await FillCombo.RefreshCards();
            cardlist = FillCombo.cardsList;
            List<CardsSts> cardtransList = new List<CardsSts>();
            foreach (Card card in cardlist)
            {
                CardsSts tempcard = new CardsSts();

                tempcard.cardId = card.cardId;
                tempcard.name = card.name;
                tempcard.hasProcessNum = card.hasProcessNum;
                tempcard.image = card.image;
                tempcard.isActive = card.isActive;
                tempcard.total = 0;
                cardtransList.Add(tempcard);
            }

            //card sum
            foreach (CardsSts card in cardtransList)
            {
                decimal pay = 0;
                decimal deposit = 0;
                pay = (decimal)queryrOps.Where(x => x.processType == "card" && x.cardId == card.cardId && x.transType == "p").Sum(x => x.cash);
                deposit = (decimal)queryrOps.Where(x => x.processType == "card" && x.cardId == card.cardId && x.transType == "d").Sum(x => x.cash);
                card.total = deposit - pay;

                //card.total = (decimal)queryrOps.Where(x => x.processType == "card" && x.cardId == card.cardId).Sum(x => x.cash);
                //converter
                card.total = decimal.Parse(SectionData.DecTostring(card.total));

            }

            return cardtransList;


        }

        public static void cashTransferStsRecipient(IEnumerable<CashTransferSts> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<CashTransferSts> cashTransfers = JsonConvert.DeserializeObject<List<CashTransferSts>>(JsonConvert.SerializeObject(Query));

            cashTransferSts(cashTransfers, rep, reppath);

         //   cashTransferProcessTypeConv(paramarr);
            DateFormConv(paramarr);
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));

            paramarr.Add(new ReportParameter("trPaymentType", MainWindow.resourcemanagerreport.GetString("trPaymentType")));
            paramarr.Add(new ReportParameter("trAccoutant", MainWindow.resourcemanagerreport.GetString("trAccoutant")));
            paramarr.Add(new ReportParameter("trRecipientTooltip", MainWindow.resourcemanagerreport.GetString("trRecipientTooltip")));
            paramarr.Add(new ReportParameter("trDepositor", MainWindow.resourcemanagerreport.GetString("trDepositor")));
            paramarr.Add(new ReportParameter("trCompany", MainWindow.resourcemanagerreport.GetString("trCompany")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trAmount", MainWindow.resourcemanagerreport.GetString("trAmount")));
            paramarr.Add(new ReportParameter("trNote", MainWindow.resourcemanagerreport.GetString("trNote")));
            paramarr.Add(new ReportParameter("trPosTooltip", MainWindow.resourcemanagerreport.GetString("trPosTooltip")));
        }
        public static void itemTransferInvoice(IEnumerable<ItemTransferInvoice> itemTransferInvoices, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetItemTransferInvoice", itemTransferInvoices));

        }
        public static void DateFormConv(List<ReportParameter> paramarr)
        {

            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));
        }

        public static void InventoryTypeConv(List<ReportParameter> paramarr)
        {

            paramarr.Add(new ReportParameter("trArchived", MainWindow.resourcemanagerreport.GetString("trArchived")));
            paramarr.Add(new ReportParameter("trSaved", MainWindow.resourcemanagerreport.GetString("trSaved")));
            paramarr.Add(new ReportParameter("trDraft", MainWindow.resourcemanagerreport.GetString("trDraft")));
        }
        public static void cashTransTypeConv(List<ReportParameter> paramarr)
        {

            paramarr.Add(new ReportParameter("trPull", MainWindow.resourcemanagerreport.GetString("trPull")));
            paramarr.Add(new ReportParameter("trDeposit", MainWindow.resourcemanagerreport.GetString("trDeposit")));

        }

        public static void cashTransferProcessTypeConv(List<ReportParameter> paramarr)
        {
            paramarr.Add(new ReportParameter("trCash", MainWindow.resourcemanagerreport.GetString("trCash")));
            paramarr.Add(new ReportParameter("trDocument", MainWindow.resourcemanagerreport.GetString("trDocument")));
            paramarr.Add(new ReportParameter("trCheque", MainWindow.resourcemanagerreport.GetString("trCheque")));
            paramarr.Add(new ReportParameter("trCredit", MainWindow.resourcemanagerreport.GetString("trCredit")));
            paramarr.Add(new ReportParameter("trInv", MainWindow.resourcemanagerreport.GetString("trInv")));
            paramarr.Add(new ReportParameter("trCard", MainWindow.resourcemanagerreport.GetString("trCreditCard")));

        }
        public static void itemTransferInvTypeConv(List<ReportParameter> paramarr)
        {
            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));
            paramarr.Add(new ReportParameter("trPurchaseInvoice", MainWindow.resourcemanagerreport.GetString("trPurchaseInvoice")));
            paramarr.Add(new ReportParameter("trPurchaseInvoiceWaiting", MainWindow.resourcemanagerreport.GetString("trPurchaseInvoiceWaiting")));
            paramarr.Add(new ReportParameter("trSalesInvoice", MainWindow.resourcemanagerreport.GetString("trSalesInvoice")));
            paramarr.Add(new ReportParameter("trSalesReturnInvoice", MainWindow.resourcemanagerreport.GetString("trSalesReturnInvoice")));
            paramarr.Add(new ReportParameter("trPurchaseReturnInvoice", MainWindow.resourcemanagerreport.GetString("trPurchaseReturnInvoice")));
            paramarr.Add(new ReportParameter("trPurchaseReturnInvoiceWaiting", MainWindow.resourcemanagerreport.GetString("trPurchaseReturnInvoiceWaiting")));
            paramarr.Add(new ReportParameter("trDraftPurchaseBill", MainWindow.resourcemanagerreport.GetString("trDraftPurchaseBill")));
            paramarr.Add(new ReportParameter("trSalesDraft", MainWindow.resourcemanagerreport.GetString("trSalesDraft")));
            paramarr.Add(new ReportParameter("trSalesReturnDraft", MainWindow.resourcemanagerreport.GetString("trSalesReturnDraft")));

            paramarr.Add(new ReportParameter("trSaleOrderDraft", MainWindow.resourcemanagerreport.GetString("trSaleOrderDraft")));
            paramarr.Add(new ReportParameter("trSaleOrder", MainWindow.resourcemanagerreport.GetString("trSaleOrder")));
            paramarr.Add(new ReportParameter("trPurchaceOrderDraft", MainWindow.resourcemanagerreport.GetString("trPurchaceOrderDraft")));
            paramarr.Add(new ReportParameter("trPurchaceOrder", MainWindow.resourcemanagerreport.GetString("trPurchaceOrder")));
            paramarr.Add(new ReportParameter("trQuotationsDraft", MainWindow.resourcemanagerreport.GetString("trQuotationsDraft")));
            paramarr.Add(new ReportParameter("trQuotations", MainWindow.resourcemanagerreport.GetString("trQuotations")));
            paramarr.Add(new ReportParameter("trDestructive", MainWindow.resourcemanagerreport.GetString("trDestructive")));
            paramarr.Add(new ReportParameter("trShortage", MainWindow.resourcemanagerreport.GetString("trShortage")));
            paramarr.Add(new ReportParameter("trImportDraft", MainWindow.resourcemanagerreport.GetString("trImportDraft")));
            paramarr.Add(new ReportParameter("trImport", MainWindow.resourcemanagerreport.GetString("trImport")));
            paramarr.Add(new ReportParameter("trImportOrder", MainWindow.resourcemanagerreport.GetString("trImportOrder")));
            paramarr.Add(new ReportParameter("trExportDraft", MainWindow.resourcemanagerreport.GetString("trExportDraft")));
            paramarr.Add(new ReportParameter("trExport", MainWindow.resourcemanagerreport.GetString("trExport")));
            paramarr.Add(new ReportParameter("trExportOrder", MainWindow.resourcemanagerreport.GetString("trExportOrder")));

        }
        public static void invoiceSideConv(List<ReportParameter> paramarr)
        {


            paramarr.Add(new ReportParameter("trVendor", MainWindow.resourcemanagerreport.GetString("trVendor")));
            paramarr.Add(new ReportParameter("trCustomer", MainWindow.resourcemanagerreport.GetString("trCustomer")));


        }
        public static void AccountSideConv(List<ReportParameter> paramarr)
        {

            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));

            paramarr.Add(new ReportParameter("trVendor", MainWindow.resourcemanagerreport.GetString("trVendor")));
            paramarr.Add(new ReportParameter("trCustomer", MainWindow.resourcemanagerreport.GetString("trCustomer")));
            paramarr.Add(new ReportParameter("trUser", MainWindow.resourcemanagerreport.GetString("trUser")));
            paramarr.Add(new ReportParameter("trSalary", MainWindow.resourcemanagerreport.GetString("trSalary")));
            paramarr.Add(new ReportParameter("trGeneralExpenses", MainWindow.resourcemanagerreport.GetString("trGeneralExpenses")));

            paramarr.Add(new ReportParameter("trAdministrativeDeposit", MainWindow.resourcemanagerreport.GetString("trAdministrativeDeposit")));

            paramarr.Add(new ReportParameter("trAdministrativePull", MainWindow.resourcemanagerreport.GetString("trAdministrativePull")));
            paramarr.Add(new ReportParameter("trShippingCompany", MainWindow.resourcemanagerreport.GetString("trShippingCompany")));
            paramarr.Add(new ReportParameter("trTaxCollection", MainWindow.resourcemanagerreport.GetString("trTaxCollection")));



        }
        public static void itemTransferInvoiceExternal(IEnumerable<ItemTransferInvoice> itemTransferInvoices, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {



            itemTransferInvTypeConv(paramarr);
            invoiceSideConv(paramarr);

            
            paramarr.Add(new ReportParameter("trBranchStoreHint", MainWindow.resourcemanagerreport.GetString("trBranch/Store")));
            paramarr.Add(new ReportParameter("trStartDateHint", MainWindow.resourcemanagerreport.GetString("trStartDate")));

            paramarr.Add(new ReportParameter("trEndDateHint", MainWindow.resourcemanagerreport.GetString("trEndDate")));

itemTransferInvoice(itemTransferInvoices, rep, reppath);
        }
        public static void itemTransferInvoiceDirect(IEnumerable<ItemTransferInvoice> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<ItemTransferInvoice> itemTransferInvoices = JsonConvert.DeserializeObject<List<ItemTransferInvoice>>(JsonConvert.SerializeObject(Query));

            itemTransferInvTypeConv(paramarr);
            invoiceSideConv(paramarr);

           
            paramarr.Add(new ReportParameter("trStartDateHint", MainWindow.resourcemanagerreport.GetString("trStartDate")));

            paramarr.Add(new ReportParameter("trEndDateHint", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            paramarr.Add(new ReportParameter("trPrice", MainWindow.resourcemanagerreport.GetString("trPrice")));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            paramarr.Add(new ReportParameter("trType", MainWindow.resourcemanagerreport.GetString("trType")));
            foreach (var r in itemTransferInvoices)
            {
                r.price = decimal.Parse(SectionData.DecTostring(r.price));
                r.total = decimal.Parse(SectionData.DecTostring(r.total));
                r.totalNet = decimal.Parse(SectionData.DecTostring(r.totalNet));
                r.invType = ConvertInvType(r.invType);
            }
            itemTransferInvoice(itemTransferInvoices, rep, reppath);
        }

        public static void itemTransferInvoiceInternal(IEnumerable<ItemTransferInvoice> itemTransferInvoices, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            itemTransferInvTypeConv(paramarr);
            itemTransferInvoice(itemTransferInvoices, rep, reppath);
            paramarr.Add(new ReportParameter("trStartDateHint", MainWindow.resourcemanagerreport.GetString("trStartDate")));

            paramarr.Add(new ReportParameter("trEndDateHint", MainWindow.resourcemanagerreport.GetString("trEndDate")));

        }
        public static void itemTransferInvoiceDestroied(IEnumerable<ItemTransferInvoice> itemTransferInvoices, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<ItemTransferInvoice> Query = JsonConvert.DeserializeObject<List<ItemTransferInvoice>>(JsonConvert.SerializeObject(itemTransferInvoices));


            foreach (ItemTransferInvoice row in Query)
            {
                row.userdestroy = destructiveConverter(row.userdestroy);

            }
            itemTransferInvoice(Query, rep, reppath);
            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));

        }
        public static string destructiveConverter(string userdestroy)
        {
            try
            {
                if (userdestroy != null && userdestroy != "")
                {
                    return MainWindow.resourcemanagerreport.GetString("onUser") + " : " + userdestroy;
                }
                //else if (values[1] != null)
                //    return MainWindow.resourcemanager.GetString("onCompany") + " : " + values[1];
                else
                    return MainWindow.resourcemanagerreport.GetString("onCompany");
            }
            catch
            {
                return "";
            }
        }
        public static void categoryReport(IEnumerable<Category> categoryQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var r in categoryQuery)
            {
                r.taxes = decimal.Parse(SectionData.DecTostring(r.taxes));
            }
            rep.DataSources.Add(new ReportDataSource("DataSetCategory", categoryQuery));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trCategories")));
            paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trCode")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trDetails", MainWindow.resourcemanagerreport.GetString("trDetails")));
        }
        //public static void itemReport(IEnumerable<Item> itemQuery, LocalReport rep, string reppath)
        //{
        //    rep.ReportPath = reppath;
        //    rep.EnableExternalImages = true;
        //    rep.DataSources.Clear();
        //    rep.DataSources.Add(new ReportDataSource("DataSetItem", itemQuery));

        //}
        public static void itemReport(IEnumerable<Item> _items, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (Item r in _items)
            {
                r.taxes = decimal.Parse(SectionData.DecTostring(r.taxes));
            }
            rep.DataSources.Add(new ReportDataSource("DataSetItem", _items));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trItems")));
            paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trCode")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trDetails", MainWindow.resourcemanagerreport.GetString("trDetails")));
            paramarr.Add(new ReportParameter("trCategory", MainWindow.resourcemanagerreport.GetString("trCategorie")));
        }
        public static void properyReport(IEnumerable<Property> propertyQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetProperty", propertyQuery));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trProperties")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trProperty")));
            paramarr.Add(new ReportParameter("trValues", MainWindow.resourcemanagerreport.GetString("trValues")));
            paramarr.Add(new ReportParameter("trsequence", MainWindow.resourcemanagerreport.GetString("sequence")));

        }
        public static void storageCostReport(IEnumerable<StorageCost> storageCostQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (var s in storageCostQuery)
            {
                s.cost = decimal.Parse(SectionData.DecTostring(s.cost));
            }
            rep.DataSources.Add(new ReportDataSource("DataSetStorageCost", storageCostQuery));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trStorageCost")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trCost", MainWindow.resourcemanagerreport.GetString("trStorageCost")));

        }
        public static void unitReport(IEnumerable<Unit> unitQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetUnit", unitQuery));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trUnitName")));
            paramarr.Add(new ReportParameter("trNotes", MainWindow.resourcemanagerreport.GetString("trNote")));

        }
        public static void warrantyReport(IEnumerable<Warranty> ListQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<Warranty> Query = JsonConvert.DeserializeObject<List<Warranty>>(JsonConvert.SerializeObject(ListQuery));
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSet", Query));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trWarranty")));
            paramarr.Add(new ReportParameter("trPeriod", MainWindow.resourcemanagerreport.GetString("trPeriod")));
            paramarr.Add(new ReportParameter("trDescription", MainWindow.resourcemanagerreport.GetString("trDescription")));
            paramarr.Add(new ReportParameter("trNote", MainWindow.resourcemanagerreport.GetString("trNote")));







        }
        public static void inventoryReport(IEnumerable<InventoryItemLocation> invItemsLocations, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetInventory", invItemsLocations));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trStocktakingItems")));// tt
            paramarr.Add(new ReportParameter("trNum", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trSec_Loc", MainWindow.resourcemanagerreport.GetString("trSectionLocation")));//
            //paramarr.Add(new ReportParameter("trItem_UnitName", MainWindow.resourcemanagerreport.GetString("trUnitName")+"-" + MainWindow.resourcemanagerreport.GetString("")));
            paramarr.Add(new ReportParameter("trItem_UnitName", MainWindow.resourcemanagerreport.GetString("trItemUnit")));
            paramarr.Add(new ReportParameter("trRealAmount", MainWindow.resourcemanagerreport.GetString("trRealAmount")));
            paramarr.Add(new ReportParameter("trInventoryAmount", MainWindow.resourcemanagerreport.GetString("trInventoryAmount")));
            paramarr.Add(new ReportParameter("trDestroyCount", MainWindow.resourcemanagerreport.GetString("trDestoryCount")));
        }


        public static void ItemsExportReport(IEnumerable<ItemTransfer> invoiceItems, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetItemsExport", invoiceItems));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trItemsImport/Export")));// tt
            paramarr.Add(new ReportParameter("trNum", MainWindow.resourcemanagerreport.GetString("trNum")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trAmount", MainWindow.resourcemanagerreport.GetString("trQuantity")));
        }
        public static void ReceiptPurchaseReport(IEnumerable<ItemTransfer> invoiceItems, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetItemsExport", invoiceItems));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trReceiptOfPurchasesBill")));// tt
            paramarr.Add(new ReportParameter("trNum", MainWindow.resourcemanagerreport.GetString("trNum")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trAmount", MainWindow.resourcemanagerreport.GetString("trQuantity")));
        }
        public static void itemLocation(IEnumerable<ItemLocation> itemLocations, LocalReport rep, string reppath)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            rep.DataSources.Add(new ReportDataSource("DataSetItemLocation", itemLocations));
        }


        public static void deliveryManagement(IEnumerable<Invoice> Query1, LocalReport rep, string reppath, List<ReportParameter> paramarr, int isdriver)
        {
            List<Invoice> Query = JsonConvert.DeserializeObject<List<Invoice>>(JsonConvert.SerializeObject(Query1));
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();


            //table columns
            paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trInvoiceCharp")));
            paramarr.Add(new ReportParameter("deliveryTime", MainWindow.resourcemanagerreport.GetString("deliveryTime")));
            paramarr.Add(new ReportParameter("trStatus", MainWindow.resourcemanagerreport.GetString("trStatus")));
            if (isdriver == 1)
            {
                paramarr.Add(new ReportParameter("deliveryMan", MainWindow.resourcemanagerreport.GetString("trDeliveryMan")));
            }
            else
            {
                paramarr.Add(new ReportParameter("deliveryMan", MainWindow.resourcemanagerreport.GetString("trCompany")));
            }
            foreach (var row in Query)
            {
                row.status = preparingOrderStatusConvert(row.status);
                //  row.orderTimeConv = dateTimeToTimeConvert(row.orderTime);
                row.shipUserName = driverOrShipcompanyConvert(isdriver, row.shipUserName, "", row.shipCompanyName);
            }
            paramarr.Add(new ReportParameter("trNoData", MainWindow.resourcemanagerreport.GetString("thereArenodata")));
            rep.DataSources.Add(new ReportDataSource("DataSet", Query));

            //title
            paramarr.Add(new ReportParameter("trTitle", MainWindow.resourcemanagerreport.GetString("trDeliveryManagement")));

        }
        public static string preparingOrderStatusConvert(string status)
        {
            switch (status)
            {
                case "Listed": return MainWindow.resourcemanagerreport.GetString("trListed");
                case "Preparing": return MainWindow.resourcemanagerreport.GetString("trPreparing");
                case "Ready": return MainWindow.resourcemanagerreport.GetString("trReady");
                case "Collected": return MainWindow.resourcemanagerreport.GetString("withDelivery");
                case "InTheWay": return MainWindow.resourcemanagerreport.GetString("onTheWay");
                case "Done": return MainWindow.resourcemanagerreport.GetString("trDone");// gived to customer
                                                                                         //  case "withDelivery":return MainWindow.resourcemanagerreport.GetString("withDelivery");

                default: return "";
            }
        }
        public static string driverOrShipcompanyConvert(int isDriver, string shipUserName, string shipUserLastName, string shippingCompanyName)
        {
            string name = "";
            if (isDriver == 1)
            {
                name = shipUserName + " " + shipUserLastName;
            }
            else
            {
                name = shippingCompanyName;
            }

            return name;
        }

        public static void driverManagement(List<Invoice> Query1, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {

            List<Invoice> Query = JsonConvert.DeserializeObject<List<Invoice>>(JsonConvert.SerializeObject(Query1));

            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            //table columns
            paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trInvoiceCharp")));

            //  paramarr.Add(new ReportParameter("deliveryTime", MainWindow.resourcemanagerreport.GetString("deliveryTime")));
            //paramarr.Add(new ReportParameter("trStatus", AppSettings.resourcemanagerreport.GetString("trStatus")));


            paramarr.Add(new ReportParameter("trCustomer", MainWindow.resourcemanagerreport.GetString("trCustomer")));
            paramarr.Add(new ReportParameter("trCustomerAddress", MainWindow.resourcemanagerreport.GetString("trAddress")));

            paramarr.Add(new ReportParameter("trCustomerMobile", MainWindow.resourcemanagerreport.GetString("trMobile")));


            //foreach (var row in Query)
            //{
            //    //row.status = preparingOrderStatusConvert(row.status);
            //    row.orderTimeConv = dateTimeToTimeConvert(row.orderTime);
            //    row.agentAddress = agentResSectorsAddressConv(row.agentResSectorsName, row.agentAddress);
            //}
            rep.DataSources.Add(new ReportDataSource("DataSet", Query));
            //title
            paramarr.Add(new ReportParameter("trTitle", MainWindow.resourcemanagerreport.GetString("deliveryList")));

            paramarr.Add(new ReportParameter("trNoData", MainWindow.resourcemanagerreport.GetString("thereArenodata")));

        }

        public static void ShippingCompanies(IEnumerable<ShippingCompanies> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            //title
            paramarr.Add(new ReportParameter("trTitle", MainWindow.resourcemanagerreport.GetString("trShippingCompanies")));
            //table columns
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trRealDeliveryCost", MainWindow.resourcemanagerreport.GetString("trRealDeliveryCost")));
            paramarr.Add(new ReportParameter("trDeliveryCost", MainWindow.resourcemanagerreport.GetString("trDeliveryCost")));
            paramarr.Add(new ReportParameter("trDeliveryType", MainWindow.resourcemanagerreport.GetString("trDeliveryType")));
            paramarr.Add(new ReportParameter("trNoData", MainWindow.resourcemanagerreport.GetString("thereArenodata")));

            foreach (var row in Query)
            {

                row.RealDeliveryCost = decimal.Parse(SectionData.DecTostring(row.RealDeliveryCost));
                row.deliveryCost = decimal.Parse(SectionData.DecTostring(row.deliveryCost));
                row.deliveryType = deliveryTypeConvert(row.deliveryType);
                //deliveryTypeConverter
            }
            rep.DataSources.Add(new ReportDataSource("DataSet", Query));

        }

        public static void DeliveryReport(IEnumerable<Invoice> list, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<Invoice> Query = JsonConvert.DeserializeObject<List<Invoice>>(JsonConvert.SerializeObject(list));


            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));

            paramarr.Add(new ReportParameter("trInvoiceNumber", MainWindow.resourcemanagerreport.GetString("trInvoiceCharp")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));

            paramarr.Add(new ReportParameter("trCustomer", MainWindow.resourcemanagerreport.GetString("trCustomer")));
            paramarr.Add(new ReportParameter("trCompany", MainWindow.resourcemanagerreport.GetString("trCompany")));
            paramarr.Add(new ReportParameter("trDriver", MainWindow.resourcemanagerreport.GetString("trDriver")));
            //  paramarr.Add(new ReportParameter("duration", MainWindow.resourcemanagerreport.GetString("duration")));
            paramarr.Add(new ReportParameter("trStatus", MainWindow.resourcemanagerreport.GetString("trStatus")));

            DateFormConv(paramarr);


            foreach (Invoice row in Query)
            {
                //row.statusConv = preparingOrderStatusConvert(row.status);
                //row.orderDurationConv = SectionData.decimalToTime(row.orderDuration);
                row.shipCompanyName = shippingCompanyNameConvert(row.shipCompanyName);
                row.shipUserName = driverConvert(row.shipUserName, row.shipUserId);
                row.status = isFreeShipConverter(row.isFreeShip);

            }


            rep.DataSources.Add(new ReportDataSource("DataSet", Query));
        }
        public static void DeliveryOrderStatReport(IEnumerable<Invoice> list, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<Invoice> Query = JsonConvert.DeserializeObject<List<Invoice>>(JsonConvert.SerializeObject(list));
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));

            paramarr.Add(new ReportParameter("trInvoiceNumber", MainWindow.resourcemanagerreport.GetString("trInvoiceCharp")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));

            //paramarr.Add(new ReportParameter("trCustomer", MainWindow.resourcemanagerreport.GetString("trCustomer")));
            //paramarr.Add(new ReportParameter("trCompany", MainWindow.resourcemanagerreport.GetString("trCompany")));
            //paramarr.Add(new ReportParameter("trDriver", MainWindow.resourcemanagerreport.GetString("trDriver")));
            //  paramarr.Add(new ReportParameter("duration", MainWindow.resourcemanagerreport.GetString("duration")));
            paramarr.Add(new ReportParameter("trStatus", MainWindow.resourcemanagerreport.GetString("trStatus")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            DateFormConv(paramarr);
            foreach (Invoice row in Query)
            {
                //row.statusConv = preparingOrderStatusConvert(row.status);
                //row.orderDurationConv = SectionData.decimalToTime(row.orderDuration);
                //row.shipCompanyName = shippingCompanyNameConvert(row.shipCompanyName);
                //row.shipUserName = driverConvert(row.shipUserName, row.shipUserId);
                row.notes = ConvertdateFormat(row.updateDate);
                row.status = preparingOrderStatusConvert(row.status);

            }
            rep.DataSources.Add(new ReportDataSource("DataSet", Query));
        }

        public static void DeliveryAllStatReport(IEnumerable<invoiceStatus> list, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<invoiceStatus> Query = JsonConvert.DeserializeObject<List<invoiceStatus>>(JsonConvert.SerializeObject(list));
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));

            paramarr.Add(new ReportParameter("trInvoiceNumber", MainWindow.resourcemanagerreport.GetString("trInvoiceCharp")));
            //   paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trTime", MainWindow.resourcemanagerreport.GetString("trTime")));


            paramarr.Add(new ReportParameter("trStatus", MainWindow.resourcemanagerreport.GetString("trStatus")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            DateFormConv(paramarr);
            foreach (invoiceStatus row in Query)
            {

                row.notes = ConvertdateFormat(row.updateDate);
                row.status = preparingOrderStatusConvert(row.status);
                row.time = dateTimeToTimeConverter(row.updateDate);

            }
            rep.DataSources.Add(new ReportDataSource("DataSet", Query));
        }

        public static string shippingCompanyNameConvert(string shippingCompanyName)
        {
            if (shippingCompanyName != null)
            {
                string s = shippingCompanyName.Trim();
                if (s.Equals("Local"))
                    return "-";
                else
                    return s;
            }
            else return "-";
        }

        public static string driverConvert(string shipUserName, int? shipUserId)
        {
            if (shipUserId != null)
            {
                return shipUserName;
            }
            else return "-";
        }


        public static void InvClassReport(IEnumerable<Slice> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<Slice> tempquery = JsonConvert.DeserializeObject<List<Slice>>(JsonConvert.SerializeObject(Query));


            //  SectionData.DateToString(startDate)
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("invoiceClasses")));
            // paramarr.Add(new ReportParameter("title", MainWindow.resourcemanagerreport.GetString("trProfits")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trclass", MainWindow.resourcemanagerreport.GetString("class")));
            paramarr.Add(new ReportParameter("trNote", MainWindow.resourcemanagerreport.GetString("trNote")));


            rep.DataSources.Add(new ReportDataSource("DataSet", tempquery));

            //  itemTransferInvTypeConv(paramarr);

        }
        public static void PriceReport(IEnumerable<Price> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<Price> tempquery = JsonConvert.DeserializeObject<List<Price>>(JsonConvert.SerializeObject(Query));


            //  SectionData.DateToString(startDate)
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            tempquery = tempquery.Where(x => x.isSelect == true).ToList();

            foreach (Price row in tempquery)
            {
                row.price = decimal.Parse(SectionData.DecTostring(row.price));
                row.unitCost = decimal.Parse(SectionData.DecTostring(row.unitCost));
                row.itemUnitPrice = decimal.Parse(SectionData.DecTostring(row.itemUnitPrice));
            }
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("pricesList")));
            paramarr.Add(new ReportParameter("trinvoiceClass", MainWindow.resourcemanagerreport.GetString("invoiceClass")));
            // paramarr.Add(new ReportParameter("title", MainWindow.resourcemanagerreport.GetString("trProfits")));
            //   paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            //  paramarr.Add(new ReportParameter("trclass", MainWindow.resourcemanagerreport.GetString("class")));
            //   paramarr.Add(new ReportParameter("trNote", MainWindow.resourcemanagerreport.GetString("trNote")));

            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trPrice", MainWindow.resourcemanagerreport.GetString("trPrice")));
            paramarr.Add(new ReportParameter("trCost", MainWindow.resourcemanagerreport.GetString("trCost")));
            paramarr.Add(new ReportParameter("basePrice", MainWindow.resourcemanagerreport.GetString("basePrice")));


            rep.DataSources.Add(new ReportDataSource("DataSet", tempquery));



        }

        public static void SerialMainReport(IEnumerable<ItemUnit> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<ItemUnit> tempquery = JsonConvert.DeserializeObject<List<ItemUnit>>(JsonConvert.SerializeObject(Query));
            //  SectionData.DateToString(startDate)
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("features")));
            // paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trSerials")));

            // paramarr.Add(new ReportParameter("title", MainWindow.resourcemanagerreport.GetString("trProfits")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            paramarr.Add(new ReportParameter("trCount", MainWindow.resourcemanagerreport.GetString("trCount")));
            paramarr.Add(new ReportParameter("trSerials", MainWindow.resourcemanagerreport.GetString("trSerials")));
            paramarr.Add(new ReportParameter("trProperties", MainWindow.resourcemanagerreport.GetString("trProperties")));




            rep.DataSources.Add(new ReportDataSource("DataSet", tempquery));

            //  itemTransferInvTypeConv(paramarr);

        }

        public static void SerialReport(IEnumerable<Serial> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<Serial> tempquery = JsonConvert.DeserializeObject<List<Serial>>(JsonConvert.SerializeObject(Query));
            tempquery = tempquery.Where(x => x.isSelected == true).ToList();

            //  SectionData.DateToString(startDate)
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trSerials")));
            // paramarr.Add(new ReportParameter("title", MainWindow.resourcemanagerreport.GetString("trProfits")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trCount", MainWindow.resourcemanagerreport.GetString("trCount")));
            paramarr.Add(new ReportParameter("trQuantity", MainWindow.resourcemanagerreport.GetString("trQuantity")));
            paramarr.Add(new ReportParameter("trSerial", MainWindow.resourcemanagerreport.GetString("trSerial")));


            rep.DataSources.Add(new ReportDataSource("DataSet", tempquery));

            //  itemTransferInvTypeConv(paramarr);

        }

        public static void SerialReportSTS(IEnumerable<SerialSts> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<SerialSts> tempquery = JsonConvert.DeserializeObject<List<SerialSts>>(JsonConvert.SerializeObject(Query));

            //  SectionData.DateToString(startDate)
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            foreach (SerialSts row in tempquery)
            {
                row.status = ConvertIsSold(row.isSold);
                //  row.updateDateStr =  SectionData.DateToString(row.updateDate) ;
                row.updateDateStr = ConvertdateFormat(row.updateDate);

                row.invNumber = emptytoSlashConvert(row.invNumber);
            }

            paramarr.Add(new ReportParameter("invnum", MainWindow.resourcemanagerreport.GetString("trInvoiceCharp")));
            paramarr.Add(new ReportParameter("trSerialNum", MainWindow.resourcemanagerreport.GetString("trSerialNum")));
            //  paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            // paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));

            paramarr.Add(new ReportParameter("trStatus", MainWindow.resourcemanagerreport.GetString("trStatus")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            //paramarr.Add(new ReportParameter("trCount", MainWindow.resourcemanagerreport.GetString("trCount")));
            //paramarr.Add(new ReportParameter("trQuantity", MainWindow.resourcemanagerreport.GetString("trQuantity")));
            //paramarr.Add(new ReportParameter("trSerial", MainWindow.resourcemanagerreport.GetString("trSerial")));


            rep.DataSources.Add(new ReportDataSource("DataSet", tempquery));

            //  itemTransferInvTypeConv(paramarr);

        }

        public static string ConvertIsSold(bool? isSold)
        {
            string value = "";
            try
            {
                if (isSold == true)
                    value = MainWindow.resourcemanagerreport.GetString("trSold");
                else if (isSold == false)
                    value = MainWindow.resourcemanagerreport.GetString("trAvailable");
                else
                    value = "";
                return value;
            }
            catch
            {
                return "";
            }
        }
        public static void itemCostReportSTS(IEnumerable<Storage> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<Storage> tempquery = JsonConvert.DeserializeObject<List<Storage>>(JsonConvert.SerializeObject(Query));
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();
            foreach (Storage row in tempquery)
            {

                row.Secname = ConvertfreeZone(row.SectionLoactionName);
                row.avgPurchasePrice = decimal.Parse(SectionData.DecTostring(row.avgPurchasePrice));

            }
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trSection", MainWindow.resourcemanagerreport.GetString("trSection") + "-" + MainWindow.resourcemanagerreport.GetString("trLocation")));
            paramarr.Add(new ReportParameter("trCost", MainWindow.resourcemanagerreport.GetString("trCost")));
            rep.DataSources.Add(new ReportDataSource("DataSet", tempquery));
        }
        public static string ConvertfreeZone(string SectionLoactionName)
        {
            try
            {
                string s = SectionLoactionName;
                if (s.Contains("FreeZone"))
                    return "-";
                else return s;
            }
            catch
            {
                return "";
            }
        }


        public static string PaymentComboConvert(string Value)
        {
            switch (Value)
            {
                case "cash": return MainWindow.resourcemanagerreport.GetString("trCash");
                case "card": return MainWindow.resourcemanagerreport.GetString("trAnotherPaymentMethods");
                case "balance": return MainWindow.resourcemanagerreport.GetString("trCredit");
                case "multiple": return MainWindow.resourcemanagerreport.GetString("trMultiplePayment");
                default: return "";
            }
        }
        public static string emptytoSlashConvert(string value)
        {
            try
            {
                if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
                    return "-";
                else
                    return value;
            }
            catch
            {
                return "";
            }
        }

        public static string ConvertdateFormat(DateTime? value)
        {
            try
            {
                DateTime date;
                if (value is DateTime)
                    date = (DateTime)value;
                else return value.ToString();

                switch (AppSettings.dateFormat)
                {
                    case "ShortDatePattern":
                        return date.ToString(@"dd/MM/yyyy");
                    case "LongDatePattern":
                        return date.ToString(@"dddd, MMMM d, yyyy");
                    case "MonthDayPattern":
                        return date.ToString(@"MMMM dd");
                    case "YearMonthPattern":
                        return date.ToString(@"MMMM yyyy");
                    default:
                        return date.ToString(@"dd/MM/yyyy");
                }
            }
            catch
            {
                return "";
            }
        }

        public static void PropertyReportSTS(IEnumerable<StorePropertySts> Query, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<StorePropertySts> tempquery = JsonConvert.DeserializeObject<List<StorePropertySts>>(JsonConvert.SerializeObject(Query));

            //  SectionData.DateToString(startDate)
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            foreach (StorePropertySts row in tempquery)
            {

                //  row.updateDateStr =  SectionData.DateToString(row.updateDate) ;
                row.updateDateStr = ConvertdateFormat(row.updateDate);
                // row.updateDate =DateTime.Parse( ConvertdateFormat(row.updateDate));
                row.invNumber = emptytoSlashConvert(row.invNumber);

            }

            paramarr.Add(new ReportParameter("invnum", MainWindow.resourcemanagerreport.GetString("trInvoiceCharp")));
            //   paramarr.Add(new ReportParameter("trSerialNum", MainWindow.resourcemanagerreport.GetString("trSerialNum")));
            //  paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            // paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));

            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            //    paramarr.Add(new ReportParameter("trStatus", MainWindow.resourcemanagerreport.GetString("trStatus")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trProperties", MainWindow.resourcemanagerreport.GetString("trProperties")));

            //paramarr.Add(new ReportParameter("trCount", MainWindow.resourcemanagerreport.GetString("trCount")));
            //paramarr.Add(new ReportParameter("trQuantity", MainWindow.resourcemanagerreport.GetString("trQuantity")));
            //paramarr.Add(new ReportParameter("trSerial", MainWindow.resourcemanagerreport.GetString("trSerial")));


            rep.DataSources.Add(new ReportDataSource("DataSet", tempquery));

            //  itemTransferInvTypeConv(paramarr);

        }

        public static string isFreeShipConverter(int isFreeShip)
        {
            try
            {
                int value = isFreeShip;
                string result = "";
                if (value == 0)
                    result = MainWindow.resourcemanagerreport.GetString("trPaid");
                else if (value == 1)
                    result = MainWindow.resourcemanagerreport.GetString("trFree");
                else
                    result = "";
                return result;
            }
            catch
            {
                return "";
            }
        }

        public static string isPaidConverter(int isCommissionPaid)
        {
            try
            {
                int value = isCommissionPaid;
                string result = "";
                if (value == 0)
                    result = MainWindow.resourcemanagerreport.GetString("trUnPaid");
                else if (value == 1)
                    result = MainWindow.resourcemanagerreport.GetString("trPaid");
                else
                    result = "";
                return result;
            }
            catch
            {
                return "";
            }
        }

        public static string dateTimeToTimeConverter(DateTime? value)
        {
            try
            {
                if (value != null)
                {
                    DateTime dt = (DateTime)value;
                    return dt.ToShortTimeString();
                }
                else
                    return "-";
            }
            catch
            {
                return "";
            }
        }

        public static void CommissionReport(IEnumerable<CashTransferSts> list, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            List<CashTransferSts> Query = JsonConvert.DeserializeObject<List<CashTransferSts>>(JsonConvert.SerializeObject(list));


            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            paramarr.Add(new ReportParameter("trProcessCharp", MainWindow.resourcemanagerreport.GetString("trProcessCharp")));

            paramarr.Add(new ReportParameter("trInvoiceCharp", MainWindow.resourcemanagerreport.GetString("trInvoiceCharp")));
            paramarr.Add(new ReportParameter("paymentAgent", MainWindow.resourcemanagerreport.GetString("paymentAgent")));

            paramarr.Add(new ReportParameter("salesEmployee", MainWindow.resourcemanagerreport.GetString("salesEmployee")));
            paramarr.Add(new ReportParameter("trAmount", MainWindow.resourcemanagerreport.GetString("trCash_")));
            paramarr.Add(new ReportParameter("trPercentageDiscount", MainWindow.resourcemanagerreport.GetString("trPercentageDiscount")));
            //  paramarr.Add(new ReportParameter("duration", MainWindow.resourcemanagerreport.GetString("duration")));
            paramarr.Add(new ReportParameter("trValue", MainWindow.resourcemanagerreport.GetString("trValue")));
            paramarr.Add(new ReportParameter("commission", MainWindow.resourcemanagerreport.GetString("commission")));
            paramarr.Add(new ReportParameter("trStatus", MainWindow.resourcemanagerreport.GetString("trPaid")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            DateFormConv(paramarr);
            foreach (CashTransferSts row in Query)
            {
                //row.bondNumber=
                row.notes = ConvertdateFormat(row.updateDate);
                row.cashSource = decimal.Parse(SectionData.DecTostring(row.cashSource));
                row.cash = decimal.Parse(SectionData.DecTostring(row.cash));
                row.totalNet = decimal.Parse(SectionData.DecTostring(row.totalNet));
                row.commissionValue = decimal.Parse(SectionData.DecTostring(row.commissionValue));
                row.commissionRatio = decimal.Parse(SectionData.PercentageDecTostring(row.commissionRatio));
                // row.bondNumber = isPaidConverter(row.isCommissionPaid);
                row.paid = decimal.Parse(SectionData.DecTostring(row.paid));

            }


            rep.DataSources.Add(new ReportDataSource("DataSet", Query));
        }

        public static void SysEmailReport(IEnumerable<SysEmails> eQuery, LocalReport rep, string reppath, List<ReportParameter> paramarr)
        {
            rep.ReportPath = reppath;
            rep.EnableExternalImages = true;
            rep.DataSources.Clear();

            List<SysEmails> query = JsonConvert.DeserializeObject<List<SysEmails>>(JsonConvert.SerializeObject(eQuery));
            foreach (SysEmails row in query)
            {
                //row.commissionValue = decimal.Parse(SectionData.DecTostring(row.commissionValue));
                //row.commissionRatio = decimal.Parse(SectionData.PercentageDecTostring(row.commissionRatio));

                row.emailId = row.isMajor == true ? 1 : 0;
            }
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trSysEmails")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trEmail", MainWindow.resourcemanagerreport.GetString("trEmail")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trMajor", MainWindow.resourcemanagerreport.GetString("trMajor")));
            rep.DataSources.Add(new ReportDataSource("DataSet", query));
        }
        public async Task<reportSize> CheckPrinterSetting(string invType)
        {
            reportSize repsset = new reportSize();
            ///printer
            if (FillCombo.printersList is null)
            {
                await FillCombo.RefreshPrintersList();
            }
            InvoiceTypesPrinters currentPrinter = FillCombo.printersList.Where(b => b.invoiceType == invType).FirstOrDefault();
            string printrName = "";

            if (currentPrinter == null)
            {
                //get default
                if (MainWindow.salePaperSize == "")
                {
                    repsset.paperSize = "5.7cm";
                }
                else
                {
                    repsset.paperSize = MainWindow.salePaperSize;
                }
            }
            else
            {
                repsset.paperSize = currentPrinter.sizeValue;
                printrName = currentPrinter.printerSysName;//
            }
            repsset.printerName = printrName;
            return repsset;
            //end
            //
        }
    }
}
