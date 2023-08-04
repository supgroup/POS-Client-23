using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace POS.converters
{
    public class permissionsNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch (value)
                {
                    case "unit_basics":
                        value = MainWindow.resourcemanager.GetString("trUnits");
                        break;
                    case "locations_addRange":
                        value = MainWindow.resourcemanager.GetString("trAddRange");
                        break;
                    case "section_selectLocation":
                        value = MainWindow.resourcemanager.GetString("trSelectLocations");
                        break;
                    case "reciptOfInvoice_recipt":
                        value = MainWindow.resourcemanager.GetString("trReciptOfInvoice");
                        break;
                    case "itemsStorage_transfer":
                        value = MainWindow.resourcemanager.GetString("trTransfer");
                        break;
                    case "importExport_import":
                        value = MainWindow.resourcemanager.GetString("trImport");
                        break;
                    case "importExport_export":
                        value = MainWindow.resourcemanager.GetString("trExport");
                        break;
                    case "itemsDestroy_destroy":
                        value = MainWindow.resourcemanager.GetString("trDestructive");
                        break;
                    case "inventory_archiving":
                        value = MainWindow.resourcemanager.GetString("trArchive");
                        break;
                    case "reciptInvoice_executeOrder":
                        value = MainWindow.resourcemanager.GetString("trExecuteOrder");
                        break;
                    case "reciptInvoice_quotation":
                        value = MainWindow.resourcemanager.GetString("trQuotations");
                        break;
                    case "offer_items":
                        value = MainWindow.resourcemanager.GetString("trItems");
                        break;
                    case "package_items":
                        value = MainWindow.resourcemanager.GetString("trItems");
                        break;

                    case "medals_customers":
                        value = MainWindow.resourcemanager.GetString("trCustomers");
                        break;
                    case "membership_customers":
                        value = MainWindow.resourcemanager.GetString("trCustomers");
                        break;
                    case "membership_subscriptionFees":
                        value = MainWindow.resourcemanager.GetString("trSubscriptionFees");
                        break;
                   
                    case "posAccounting_transAdmin":
                        value = MainWindow.resourcemanager.GetString("trTransfersAdmin");
                        break;
                    case "Permissions_users":
                        value = MainWindow.resourcemanager.GetString("trUsers");
                        break;
                    case "importExport_package":
                        value = MainWindow.resourcemanager.GetString("trPackage");
                        break;
                    case "importExport_unitConversion":
                        value = MainWindow.resourcemanager.GetString("trUnitConversion");
                        break;
                    case "ordersAccounting_allBranches":
                        value = MainWindow.resourcemanager.GetString("trBranchs/Stores");
                        break;
                    case "storageAlerts_minMaxItem":
                        value = MainWindow.resourcemanager.GetString("trOverrideStorageLimitAlert");
                        break;
                    case "storageAlerts_ImpExp":
                        value = MainWindow.resourcemanager.GetString("trMovements");
                        break;
                    case "storageAlerts_ctreatePurchaseInvoice":
                        value = MainWindow.resourcemanager.GetString("trPurchaseInvoiceWaiting");
                        break;
                    case "storageAlerts_ctreatePurchaseReturnInvoice":
                        value = MainWindow.resourcemanager.GetString("trPurchaseReturnInvoiceWaiting");
                        break;
                    case "saleAlerts_executeOrder":
                        value = MainWindow.resourcemanager.GetString("trWaitingExecuteOrder");
                        break;
                    case "trUnits":
                        value = MainWindow.resourcemanager.GetString("trWaitingExecuteOrder");
                        break;
                    case "reciptOfInvoice_inputs":
                        value = MainWindow.resourcemanager.GetString("trDirectEntry");
                        break;
                    case "deliveryManagement_update":
                        value = MainWindow.resourcemanager.GetString("trView");
                        break;
                    case "setUserSetting_delivery":
                        value = MainWindow.resourcemanager.GetString("trDelivery");
                        break;
                    case "setUserSetting_administrativeMessages":
                        value = MainWindow.resourcemanager.GetString("composeAdministrativeMessages");
                        break;
                    case "setUserSetting_administrativePosTransfers":
                        value = MainWindow.resourcemanager.GetString("administrativeCashTransfers");
                        break;
                     case "dailyClosing_transfer":
                        value = MainWindow.resourcemanager.GetString("trTransfers");
                        break;
                    case "dailyClosing_boxState":
                        value = MainWindow.resourcemanager.GetString("trBoxState");
                        break;
                    default:
                        {
                            if (value.ToString().Contains("_basics"))
                            {
                                value = MainWindow.resourcemanager.GetString("trPermissionsBasics");
                            }
                            else if (value.ToString().Contains("_create"))
                            {
                                value = MainWindow.resourcemanager.GetString("trCreate");
                            }
                            else if (value.ToString().Contains("_save"))
                            {
                                value = MainWindow.resourcemanager.GetString("trSave");
                            }
                            else if (value.ToString().Contains("_report"))
                            {
                                value = MainWindow.resourcemanager.GetString("trReports");
                            }
                            else if (value.ToString().Contains("_return"))
                            {
                                value = MainWindow.resourcemanager.GetString("trReturn");
                            }
                            else if (value.ToString().Contains("_sendEmail"))
                            {
                                value = MainWindow.resourcemanager.GetString("trSendEmail");
                            }
                            else if (value.ToString().Contains("_invoice"))
                            {
                                value = MainWindow.resourcemanager.GetString("trCreateInvocie");
                            }
                            else if (value.ToString().Contains("_payments"))
                            {
                                value = MainWindow.resourcemanager.GetString("trPayments");
                            }
                            else if (value.ToString().Contains("_view"))
                            {
                                value = MainWindow.resourcemanager.GetString("trView");
                            }
                            else if (value.ToString().Contains("_initializeShortage"))
                            {
                                value = MainWindow.resourcemanager.GetString("trInitializeShortage");
                            }
                            else if (value.ToString().Contains("_initializeShortage"))
                            {
                                value = MainWindow.resourcemanager.GetString("trInitializeShortage");
                            }
                            else if (value.ToString().Contains("_openOrder"))
                            {
                                value = MainWindow.resourcemanager.GetString("trOrders");
                            }
                            else if (value.ToString().Contains("_printCount"))
                            {
                                value = MainWindow.resourcemanager.GetString("trPrintCount");
                            }
                            else if (value.ToString().Contains("_statistic"))
                            {
                                value = MainWindow.resourcemanager.GetString("trStatistic");
                            }
                            else if (value.ToString().Contains("_delete"))
                            {
                                value = MainWindow.resourcemanager.GetString("trDelete");
                            }
                            else if (value.ToString().Contains("_pricing"))
                            {
                                value = MainWindow.resourcemanager.GetString("pricing");
                            }
                            else if (value.ToString().Contains("_sold"))
                            {
                                value = MainWindow.resourcemanager.GetString("sold");
                            }
                            else if (value.ToString().Contains("perExpiredItem"))
                            {
                                value = MainWindow.resourcemanager.GetString("expirationApproaching");
                            }
                            else if (value.ToString().Contains("delivery"))
                            {
                                value = MainWindow.resourcemanager.GetString("trDelivery");
                            }
                            else if (value.Equals("users_stores") || value.Equals("branches_branches") || value.Equals("stores_branches")
                                || value.ToString().Contains("_branches"))
                            {
                                value = MainWindow.resourcemanager.GetString("trBranchs/Stores");
                            }
                            else if (value.Equals("general_usersSettings") || value.Equals("reports_usersSettings"))
                            {
                                value = MainWindow.resourcemanager.GetString("trUsersSettings");
                            }
                            else if (value.Equals("general_companySettings") || value.Equals("reports_companySettings"))
                            {
                                value = MainWindow.resourcemanager.GetString("trCompanySettings");
                            }
                            break;
                        }
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
