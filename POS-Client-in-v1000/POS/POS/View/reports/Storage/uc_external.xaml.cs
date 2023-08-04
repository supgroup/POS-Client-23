using LiveCharts;
using LiveCharts.Wpf;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using LiveCharts.Helpers;
using POS.View.windows;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using static POS.Classes.Statistics;
using System.Globalization;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using POS.View.storage;
using System.Resources;
using System.Reflection;

namespace POS.View.reports
{
    public partial class uc_external
    {
        #region variables
        List<ItemTransferInvoice> itemsTransfer;

        IEnumerable<ItemTransferInvoice> agentsCount;
        IEnumerable<ItemTransferInvoice> invCount;

        Statistics statisticModel = new Statistics();

        private int selectedExternalTab = 0;
        List<ExternalitemCombo> comboExternalItemsItems;
        List<ExternalUnitCombo> comboExternalItemsUnits;
        List<AgentCombo> comboExternalAgentsAgents;
        List<InvTypeCombo> comboExternalInvType;
        List<InvCombo> comboExternalInvoiceInvoice;
        #endregion

        private static uc_external _instance;
        public static uc_external Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_external();
                return _instance;
            }
        }
        public uc_external()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.RightToLeft;
                }
                translate();
                #endregion

                #region key up
                //item
                cb_externalItemsBranches.IsTextSearchEnabled = false;
                cb_externalItemsBranches.IsEditable = true;
                cb_externalItemsBranches.StaysOpenOnEdit = true;
                cb_externalItemsBranches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_externalItemsBranches.Text = "";

                cb_externalItemsItems.IsTextSearchEnabled = false;
                cb_externalItemsItems.IsEditable = true;
                cb_externalItemsItems.StaysOpenOnEdit = true;
                cb_externalItemsItems.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_externalItemsItems.Text = "";

                cb_externalItemsUnits.IsTextSearchEnabled = false;
                cb_externalItemsUnits.IsEditable = true;
                cb_externalItemsUnits.StaysOpenOnEdit = true;
                cb_externalItemsUnits.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                cb_externalItemsUnits.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_externalItemsUnits.Text = "";
                //agent
                cb_externalAgentsBranches.IsTextSearchEnabled = false;
                cb_externalAgentsBranches.IsEditable = true;
                cb_externalAgentsBranches.StaysOpenOnEdit = true;
                cb_externalAgentsBranches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_externalAgentsBranches.Text = "";

                cb_externalAgentsAgentsType.IsTextSearchEnabled = false;
                cb_externalAgentsAgentsType.IsEditable = true;
                cb_externalAgentsAgentsType.StaysOpenOnEdit = true;
                cb_externalAgentsAgentsType.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_externalAgentsAgentsType.Text = "";

                cb_externalAgentsCustomer.IsTextSearchEnabled = false;
                cb_externalAgentsCustomer.IsEditable = true;
                cb_externalAgentsCustomer.StaysOpenOnEdit = true;
                cb_externalAgentsCustomer.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_externalAgentsCustomer.Text = "";
                #endregion

                col_reportChartWidth = col_reportChart.ActualWidth;
                itemsTransfer = await statisticModel.GetExternalMov((int)MainWindow.branchID, (int)MainWindow.userID);

                comboExternalItemsItems = statisticModel.getExternalItemCombo(itemsTransfer);
                comboExternalItemsUnits = statisticModel.getExternalUnitCombo(itemsTransfer);
                comboExternalAgentsAgents = statisticModel.GetExternalAgentCombos(itemsTransfer);
                comboExternalInvType = statisticModel.GetExternalInvoiceTypeCombos(itemsTransfer);
                comboExternalInvoiceInvoice = statisticModel.GetExternalInvoiceCombos(itemsTransfer);

                fillComboExternalAgentsAgentsType();
                fillComboExternalInvType();
                fillComboExternalInvoiceInvoice();
                fillComboExternalAgentsAgents();

                chk_externalAgentsIn.IsChecked = true;
                chk_externalItemsIn.IsChecked = true;
                chk_externalAgentsOut.IsChecked = true;
                chk_externalItemsOut.IsChecked = true;

                btn_externalItems_Click(btn_item, null);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region methods
        private void hideAllColumn()
        {
            col_branch.Visibility = Visibility.Hidden;
            col_item.Visibility = Visibility.Hidden;
            col_unit.Visibility = Visibility.Hidden;
            col_quantity.Visibility = Visibility.Hidden;
            col_itemUnits.Visibility = Visibility.Hidden;
            col_invNumber.Visibility = Visibility.Hidden;
            col_invTypeNumber.Visibility = Visibility.Hidden;
            col_agentType.Visibility = Visibility.Hidden;
            col_agent.Visibility = Visibility.Hidden;
            col_agentTypeAgent.Visibility = Visibility.Hidden;
        }
        private void showSelectedTabColumn()
        {

            hideAllColumn();

            if (selectedExternalTab == 0)
            {
                col_branch.Visibility = Visibility.Visible;
                col_item.Visibility = Visibility.Visible;
                col_unit.Visibility = Visibility.Visible;
                col_quantity.Visibility = Visibility.Visible;
                col_agentTypeAgent.Visibility = Visibility.Visible;
                col_invTypeNumber.Visibility = Visibility.Visible;
                col_invNumber.Visibility = Visibility.Visible;
                col_invDate.Visibility = Visibility.Visible;
            }
            else if (selectedExternalTab == 1)
            {
                col_branch.Visibility = Visibility.Visible;
                col_itemUnits.Visibility = Visibility.Visible;
                col_agent.Visibility = Visibility.Visible;
                col_quantity.Visibility = Visibility.Visible;
                col_agentType.Visibility = Visibility.Visible;
                col_invTypeNumber.Visibility = Visibility.Visible;
                col_invNumber.Visibility = Visibility.Visible;
                col_invDate.Visibility = Visibility.Visible;
            }
            else if (selectedExternalTab == 2)
            {
                col_branch.Visibility = Visibility.Visible;
                col_invNumber.Visibility = Visibility.Visible;
                col_invDate.Visibility = Visibility.Visible;
                col_quantity.Visibility = Visibility.Visible;
                col_agentTypeAgent.Visibility = Visibility.Visible;
                col_itemUnits.Visibility = Visibility.Visible;
            }

        }
        private void translate()
        {
            tt_item.Content = MainWindow.resourcemanager.GetString("trItems");
            tt_agent.Content = MainWindow.resourcemanager.GetString("trAgents");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_externalItemsBranches, MainWindow.resourcemanager.GetString("trBranch/StoreHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_externalItemsItems, MainWindow.resourcemanager.GetString("trItemHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_externalItemsUnits, MainWindow.resourcemanager.GetString("trUnitHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_externalItemsStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_externalItemsEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_externalAgentsBranches, MainWindow.resourcemanager.GetString("trBranch/StoreHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_externalAgentsAgentsType, MainWindow.resourcemanager.GetString("agentType")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_externalAgentsCustomer, MainWindow.resourcemanager.GetString("trVendor")+"/"+ MainWindow.resourcemanager.GetString("trCustomer") +"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_externalAgentsStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_externalAgentsEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));

            chk_externalItemsAllBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_externalItemsAllItems.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_externalItemsAllUnits.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_externalItemsIn.Content = MainWindow.resourcemanager.GetString("trIn");
            chk_externalItemsOut.Content = MainWindow.resourcemanager.GetString("trOut");
            chk_externalAgentsAllAgentsType.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_externalAgentsAllBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_externalAgentsAllCustomers.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_externalInvoicesAllBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_externalInvoicesALlInvoice.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_externalInvoicesAllInvoicesType.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_invNumber.Header = MainWindow.resourcemanager.GetString("trNo");
            col_invTypeNumber.Header = MainWindow.resourcemanager.GetString("trType");
            col_invDate.Header = MainWindow.resourcemanager.GetString("trDate");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_item.Header = MainWindow.resourcemanager.GetString("trItem");
            col_unit.Header = MainWindow.resourcemanager.GetString("trUnit");
            col_agentType.Header = MainWindow.resourcemanager.GetString("trAgentType");
            col_agent.Header = MainWindow.resourcemanager.GetString("trName");
            col_itemUnits.Header = MainWindow.resourcemanager.GetString("trItemUnit");
            col_agentTypeAgent.Header = MainWindow.resourcemanager.GetString("trType-Name");
            col_quantity.Header = MainWindow.resourcemanager.GetString("trQTR");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print3.Content = MainWindow.resourcemanager.GetString("trPrint");
        }
        public void paintExternlaChilds()
        {
            //bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);

            grid_externalItems.Visibility = Visibility.Hidden;
            grid_externalAgents.Visibility = Visibility.Hidden;
            grid_externalInvoices.Visibility = Visibility.Hidden;


            path_item.Fill = Brushes.White;
            path_agent.Fill = Brushes.White;

            bdr_agent.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_item.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
        }
        List<ExternalitemCombo> items = new List<ExternalitemCombo>();
        private void fillComboExternalItemsItems()
        {
            var temp = cb_externalItemsBranches.SelectedItem as Branch;
            cb_externalItemsItems.SelectedValuePath = "ItemId";
            cb_externalItemsItems.DisplayMemberPath = "ItemName";
            if (temp == null)
            {
                items = comboExternalItemsItems
                    .GroupBy(x => x.ItemId)
                    .Select(g => new ExternalitemCombo
                    {
                        ItemId = g.FirstOrDefault().ItemId,
                        ItemName = g.FirstOrDefault().ItemName,
                        BranchId = g.FirstOrDefault().BranchId
                    }).ToList();
                cb_externalItemsItems.ItemsSource = items;
            }
            else
            {
                items = comboExternalItemsItems
                    .Where(x => x.BranchId == temp.branchId)
                    .GroupBy(x => x.ItemId)
                    .Select(g => new ExternalitemCombo
                    {
                        ItemId = g.FirstOrDefault().ItemId,
                        ItemName = g.FirstOrDefault().ItemName,
                        BranchId = g.FirstOrDefault().BranchId
                    }).ToList();
                cb_externalItemsItems.ItemsSource = items;
            }
        }
        List<ExternalUnitCombo> units = new List<ExternalUnitCombo>();
        private void fillComboExternalItemsUnits()
        {
            var temp = cb_externalItemsItems.SelectedItem as ExternalitemCombo;
            var temp1 = cb_externalItemsBranches.SelectedItem as Branch;

            cb_externalItemsUnits.SelectedValuePath = "UnitId";
            cb_externalItemsUnits.DisplayMemberPath = "UnitName";
            if (temp == null && temp1 == null)
            {
                units = comboExternalItemsUnits
                    .GroupBy(x => x.UnitId)
                    .Select(g => new ExternalUnitCombo
                    {
                        UnitId = g.FirstOrDefault().UnitId,
                        UnitName = g.FirstOrDefault().UnitName,
                        ItemId = g.FirstOrDefault().ItemId
                    }).ToList();
                cb_externalItemsUnits.ItemsSource = units;
            }
            else if (temp != null && temp1 == null)
            {
                units = comboExternalItemsUnits
                     .Where(x => x.ItemId == temp.ItemId)
                    .GroupBy(x => x.UnitId)
                    .Select(g => new ExternalUnitCombo
                    {
                        UnitId = g.FirstOrDefault().UnitId,
                        UnitName = g.FirstOrDefault().UnitName,
                        ItemId = g.FirstOrDefault().ItemId
                    }).ToList();
                cb_externalItemsUnits.ItemsSource = units;
            }
            else if (temp == null && temp1 != null)
            {
                units = comboExternalItemsUnits
                     .Where(x => x.BranchId == temp1.branchId)
                    .GroupBy(x => x.UnitId)
                    .Select(g => new ExternalUnitCombo
                    {
                        UnitId = g.FirstOrDefault().UnitId,
                        UnitName = g.FirstOrDefault().UnitName,
                        ItemId = g.FirstOrDefault().ItemId
                    }).ToList();
                cb_externalItemsUnits.ItemsSource = units;
            }
            else
            {
                units = comboExternalItemsUnits
                    .Where(x => x.ItemId == temp.ItemId && x.BranchId == temp1.branchId)
                    .GroupBy(x => x.UnitId)
                    .Select(g => new ExternalUnitCombo
                    {
                        UnitId = g.FirstOrDefault().UnitId,
                        UnitName = g.FirstOrDefault().UnitName,
                        ItemId = g.FirstOrDefault().ItemId
                    }).ToList();
                cb_externalItemsUnits.ItemsSource = units;
            }
        }
        private void fillComboExternalAgentsAgentsType()
        {
            var dislist = new[] {
                new { Text = MainWindow.resourcemanager.GetString("trVendor")    , Value = "v" },
                new { Text = MainWindow.resourcemanager.GetString("trCustomer")  , Value = "c" }
                 };
            cb_externalAgentsAgentsType.DisplayMemberPath = "Text";
            cb_externalAgentsAgentsType.SelectedValuePath = "Value";
            cb_externalAgentsAgentsType.ItemsSource = dislist;
            cb_externalAgentsAgentsType.SelectedIndex = 0;
        }

        List<AgentCombo> agents = new List<AgentCombo>();
        private void fillComboExternalAgentsAgents()
        {
            var temp = cb_externalAgentsAgentsType.SelectedItem as AgentTypeCombo;
            var temp1 = cb_externalAgentsBranches.SelectedItem as Branch;
            cb_externalAgentsCustomer.SelectedValuePath = "AgentId";
            cb_externalAgentsCustomer.DisplayMemberPath = "AgentName";
            if (temp == null)
            {
                agents = comboExternalAgentsAgents
                    .GroupBy(x => x.AgentId)
                    .Select(g => new AgentCombo
                    {
                        AgentId = g.FirstOrDefault().AgentId,
                        AgentName = g.FirstOrDefault().AgentName,
                        BranchId = g.FirstOrDefault().BranchId,
                        AgentType = g.FirstOrDefault().AgentType
                    }).ToList();
                cb_externalAgentsCustomer.ItemsSource = agents;
            }
            else if (temp != null && temp1 == null)
            {
                agents = comboExternalAgentsAgents
                        .Where(x => x.AgentType == temp.AgentType)
                    .GroupBy(x => x.AgentId)
                    .Select(g => new AgentCombo
                    {
                        AgentId = g.FirstOrDefault().AgentId,
                        AgentName = g.FirstOrDefault().AgentName,
                        BranchId = g.FirstOrDefault().BranchId,
                        AgentType = g.FirstOrDefault().AgentType
                    }).ToList();
                cb_externalAgentsCustomer.ItemsSource = agents;
            }
            else if (temp == null && temp1 != null)
            {
                agents = comboExternalAgentsAgents
                        .Where(x => x.BranchId == temp1.branchId)
                    .GroupBy(x => x.AgentId)
                    .Select(g => new AgentCombo
                    {
                        AgentId = g.FirstOrDefault().AgentId,
                        AgentName = g.FirstOrDefault().AgentName,
                        BranchId = g.FirstOrDefault().BranchId,
                        AgentType = g.FirstOrDefault().AgentType
                    }).ToList();
                cb_externalAgentsCustomer.ItemsSource = agents;
            }
            else
            {
                agents = comboExternalAgentsAgents
                         .Where(x => x.AgentType == temp.AgentType && x.BranchId == temp1.branchId)
                    .GroupBy(x => x.AgentId)
                    .Select(g => new AgentCombo
                    {
                        AgentId = g.FirstOrDefault().AgentId,
                        AgentName = g.FirstOrDefault().AgentName,
                        BranchId = g.FirstOrDefault().BranchId,
                        AgentType = g.FirstOrDefault().AgentType
                    }).ToList();
                cb_externalAgentsCustomer.ItemsSource = agents;
            }

        }
        private void fillComboExternalInvType()
        {
            var temp = cb_externalInvoicesBranches.SelectedItem as Branch;
            //cb_externalInvoicesInvoiceType.SelectedValuePath = "InvoiceId";
            cb_externalInvoicesInvoiceType.SelectedValuePath = "InvoiceType";
            cb_externalInvoicesInvoiceType.DisplayMemberPath = "InvoiceType";
            if (temp == null)
            {
                var lst = comboExternalInvType
                    .GroupBy(x => x.InvoiceType)
                    .Select(g => new InvTypeCombo
                    {
                        InvoiceType = g.FirstOrDefault().InvoiceType,
                        BranchId = g.FirstOrDefault().BranchId
                    }).ToList();

                foreach (var l in lst)
                {
                    l.InvoiceType = getInvoiceType(l.InvoiceType);
                }
                cb_externalInvoicesInvoiceType.ItemsSource = lst;
            }
            else
            {
                var lst = comboExternalInvType
                    .Where(x => x.BranchId == temp.branchId)
                    .GroupBy(x => x.InvoiceType)
                    .Select(g => new InvTypeCombo
                    {
                        InvoiceType = g.FirstOrDefault().InvoiceType,
                        BranchId = g.FirstOrDefault().BranchId
                    }).ToList();

                foreach (var l in lst)
                {
                    l.InvoiceType = getInvoiceType(l.InvoiceType);
                }
                cb_externalInvoicesInvoiceType.ItemsSource = lst;
            }
        }
        private string getInvoiceType(string s)
        {
            string value = "";
            switch (s)
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
                // مسودة طلبية مبيعات 
                case "ord":
                    value = MainWindow.resourcemanager.GetString("trSaleOrderDraft");
                    break;
                //   طلبية مبيعات 
                case "or":
                    value = MainWindow.resourcemanager.GetString("trSaleOrder");
                    break;
                case "ors":
                    value = MainWindow.resourcemanager.GetString("trSaleOrder");
                    break;
                // مسودة طلبية شراء 
                case "pod":
                    value = MainWindow.resourcemanager.GetString("trPurchaceOrderDraft");
                    break;
                // طلبية شراء 
                case "po":
                    value = MainWindow.resourcemanager.GetString("trPurchaceOrder");
                    break;
                case "pos":
                    value = MainWindow.resourcemanager.GetString("trPurchaceOrder");
                    break;
                //مسودة عرض 
                case "qd":
                    value = MainWindow.resourcemanager.GetString("trQuotationsDraft");
                    break;
                //فاتورة عرض اسعار
                case "q":
                    value = MainWindow.resourcemanager.GetString("trQuotations");
                    break;
                case "qs":
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
                default: break;
            }

            return value;

        }
        private void fillComboExternalInvoiceInvoice()
        {
            var temp = cb_externalInvoicesInvoiceType.SelectedItem as InvTypeCombo;
            var temp1 = cb_externalInvoicesBranches.SelectedItem as Branch;
            cb_externalInvoicesInvoice.SelectedValuePath = "InvoiceId";
            cb_externalInvoicesInvoice.DisplayMemberPath = "InvoiceNumber";
            if (temp == null && temp1 == null)
            {
                cb_externalInvoicesInvoice.ItemsSource = comboExternalInvoiceInvoice
                    .GroupBy(x => x.InvoiceId)
                    .Select(g => new InvCombo
                    {
                        InvoiceId = g.FirstOrDefault().InvoiceId,
                        InvoiceNumber = g.FirstOrDefault().InvoiceNumber,
                        BranchId = g.FirstOrDefault().BranchId,
                        InvoiceType = g.FirstOrDefault().InvoiceType
                    }).ToList();
            }
            else if (temp != null && temp1 == null)
            {
                cb_externalInvoicesInvoice.ItemsSource = comboExternalInvoiceInvoice
                      .Where(x => getInvoiceType(x.InvoiceType) == temp.InvoiceType)
                    .GroupBy(x => x.InvoiceId)
                    .Select(g => new InvCombo
                    {
                        InvoiceId = g.FirstOrDefault().InvoiceId,
                        InvoiceNumber = g.FirstOrDefault().InvoiceNumber,
                        BranchId = g.FirstOrDefault().BranchId,
                        InvoiceType = g.FirstOrDefault().InvoiceType
                    }).ToList();
            }
            else if (temp == null && temp1 != null)
            {
                cb_externalInvoicesInvoice.ItemsSource = comboExternalInvoiceInvoice
                      .Where(x => x.BranchId == temp1.branchId)
                    .GroupBy(x => x.InvoiceId)
                    .Select(g => new InvCombo
                    {
                        InvoiceId = g.FirstOrDefault().InvoiceId,
                        InvoiceNumber = g.FirstOrDefault().InvoiceNumber,
                        BranchId = g.FirstOrDefault().BranchId,
                        InvoiceType = g.FirstOrDefault().InvoiceType
                    }).ToList();
            }
            else
            {
                cb_externalInvoicesInvoice.ItemsSource = comboExternalInvoiceInvoice
                    .Where(x => x.InvoiceType == temp.InvoiceType && x.BranchId == temp1.branchId)
                     .GroupBy(x => x.InvoiceId)
                    .Select(g => new InvCombo
                    {
                        InvoiceId = g.FirstOrDefault().InvoiceId,
                        InvoiceNumber = g.FirstOrDefault().InvoiceNumber,
                        BranchId = g.FirstOrDefault().BranchId,
                        InvoiceType = g.FirstOrDefault().InvoiceType
                    }).ToList();
            }
        }
        IEnumerable<ItemTransferInvoice> itemTransferInvoicesLst;
        private IEnumerable<ItemTransferInvoice> fillList(IEnumerable<ItemTransferInvoice> itemsTransfer, ComboBox comboBranch, ComboBox comboItem, ComboBox comboUnit, DatePicker startDate, DatePicker endDate, CheckBox chkAllBranches, CheckBox chkAllItems, CheckBox chkAllUnits, CheckBox chkIn, CheckBox chkOut)
        {
            var selectedBranch = comboBranch.SelectedItem as Branch;
            var selectedItem = comboItem.SelectedItem as ExternalitemCombo;
            var selectedUnit = comboUnit.SelectedItem as ExternalUnitCombo;
            var selectedAgentType = comboItem.SelectedItem as AgentTypeCombo;
            var selectedAgent = comboUnit.SelectedItem as AgentCombo;
            var selectedInvoiceType = comboItem.SelectedItem as InvTypeCombo;
            var selectedInvoice = comboUnit.SelectedItem as InvCombo;

            var result = itemsTransfer.Where(x => (
                  (selectedExternalTab == 0 ? (
                            ((chkIn.IsChecked == true ? (x.invType == "p") || (x.invType == "sb") : false)
                            || (chkOut.IsChecked == true ? (x.invType == "s") || (x.invType == "pb") : false))
                          && (comboBranch.SelectedItem != null ? (x.branchId == selectedBranch.branchId) : true)
                          && (comboItem.SelectedItem != null ? (x.itemId == selectedItem.ItemId) : true)
                          && (comboUnit.SelectedItem != null ? (x.unitId == selectedUnit.UnitId) : true)
                          && (dp_externalItemsStartDate.SelectedDate != null ? (x.updateDate.Value.Date >= startDate.SelectedDate.Value.Date) : true)
                          && (dp_externalItemsEndDate.SelectedDate != null ? (x.updateDate.Value.Date <= endDate.SelectedDate.Value.Date) : true)
                          )
                          : true
                          )
                          && (selectedExternalTab == 1 ? (
                           ((chkIn.IsChecked == true ? (x.invType == "p") || (x.invType == "sb") : false)
                            || (chkOut.IsChecked == true ? (x.invType == "s") || (x.invType == "pb") : false))
                          && (comboBranch.SelectedItem != null ? (x.branchId == selectedBranch.branchId) : true)
                          && (comboItem.SelectedItem != null ? (x.agentType == cb_externalAgentsAgentsType.SelectedValue.ToString()) : true)
                          && (comboUnit.SelectedItem != null ? (x.agentId == selectedAgent.AgentId) : true)
                          && (dp_externalAgentsStartDate.SelectedDate != null ? (x.updateDate.Value.Date >= startDate.SelectedDate.Value.Date) : true)
                          && (dp_externalAgentsEndDate.SelectedDate != null ? (x.updateDate.Value.Date <= endDate.SelectedDate.Value.Date) : true)
                          )
                          : true
                          )
                          && (selectedExternalTab == 2 ? (
                            (comboBranch.SelectedItem != null ? (x.branchId == selectedBranch.branchId) : true)
                          && (comboItem.SelectedItem != null ? (getInvoiceType(x.invType) == selectedInvoiceType.InvoiceType) : true)
                          && (comboUnit.SelectedItem != null ? (x.invoiceId == selectedInvoice.InvoiceId) : true)
                          && (dp_externalInvoicesStartDate.SelectedDate != null ? (x.updateDate.Value.Date >= startDate.SelectedDate.Value.Date) : true)
                          && (dp_externalInvoicesEndDate.SelectedDate != null ? (x.updateDate.Value.Date <= endDate.SelectedDate.Value.Date) : true)
                          )
                          : true
                          )
            ));

            itemTransferInvoicesLst = result;
            return result;

        }
        private void fillEvents()
        {
            if (selectedExternalTab == 0)
                temp = fillList(itemsTransfer, cb_externalItemsBranches, cb_externalItemsItems, cb_externalItemsUnits, dp_externalItemsStartDate, dp_externalItemsEndDate, chk_externalItemsAllBranches, chk_externalItemsAllItems, chk_externalItemsAllUnits, chk_externalItemsIn, chk_externalItemsOut);
            else if (selectedExternalTab == 1)
                temp = fillList(itemsTransfer, cb_externalAgentsBranches, cb_externalAgentsAgentsType, cb_externalAgentsCustomer, dp_externalAgentsStartDate, dp_externalAgentsEndDate, chk_externalAgentsAllBranches, chk_externalAgentsAllAgentsType, chk_externalAgentsAllCustomers, chk_externalAgentsIn, chk_externalAgentsOut);
            else if (selectedExternalTab == 2)
                temp = fillList(itemsTransfer, cb_externalInvoicesBranches, cb_externalInvoicesInvoiceType, cb_externalInvoicesInvoice, dp_externalInvoicesStartDate, dp_externalInvoicesEndDate, chk_externalInvoicesAllBranches, chk_externalInvoicesAllInvoicesType, chk_externalInvoicesALlInvoice, null, null);

            dgStock.ItemsSource = temp;
            txt_count.Text = temp.Count().ToString();

            fillExternalColumnChart();
            fillExternalPieChart();
            fillExternalRowChart();
        }
        private void fillEmptyEvents()
        {
            temp = new List<ItemTransferInvoice>();

            dgStock.ItemsSource = temp;
            txt_count.Text = temp.Count().ToString();
            itemTransferInvoicesLst = new List<ItemTransferInvoice>();
            fillExternalColumnChart();
            fillExternalPieChart();
            fillExternalRowChart();
        }
        #endregion

        #region events
        private void cb_externalItemsBranches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalItemsItems.IsEnabled = false;
                chk_externalItemsAllItems.IsEnabled = true;
                chk_externalItemsAllItems.IsChecked = true;

                fillComboExternalItemsItems();

                fillEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalItemsAllBranches_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //branch
                cb_externalItemsBranches.IsEnabled = false;
                cb_externalItemsBranches.SelectedItem = null;
                //items
                cb_externalItemsItems.IsEnabled = false;
                cb_externalItemsItems.SelectedItem = null;
                chk_externalItemsAllItems.IsEnabled = true;
                chk_externalItemsAllItems.IsChecked = true;
                //unit
                chk_externalItemsAllUnits.IsChecked = true;
                chk_externalItemsAllUnits.IsEnabled = true;
                cb_externalItemsUnits.IsEnabled = false;
                cb_externalItemsUnits.SelectedItem = null;
                fillComboExternalItemsItems();

                cb_externalItemsBranches.Text = "";
                cb_externalItemsBranches.ItemsSource = SectionData.BranchesAllWithoutMainList;
                fillEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalItemsAllBranches_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //branch
                cb_externalItemsBranches.IsEnabled = true;
                cb_externalItemsBranches.SelectedItem = null;
                //item
                cb_externalItemsItems.IsEnabled = false;
                cb_externalItemsItems.SelectedItem = null;
                chk_externalItemsAllItems.IsEnabled = false;
                //chk_externalItemsAllItems.IsChecked = false;
                //unit
                cb_externalItemsUnits.IsEnabled = false;
                cb_externalItemsUnits.SelectedItem = null;
                //chk_externalItemsAllUnits.IsChecked = false;
                chk_externalItemsAllUnits.IsEnabled = false;

                fillEmptyEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void cb_externalItemsItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalItemsUnits.IsEnabled = false;
                chk_externalItemsAllUnits.IsEnabled = true;

                fillComboExternalItemsUnits();

                fillEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalItemsAllItems_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalItemsItems.IsEnabled = false;
                cb_externalItemsItems.SelectedItem = null;

                cb_externalItemsUnits.IsEnabled = false;
                cb_externalItemsUnits.SelectedItem = null;

                chk_externalItemsAllUnits.IsChecked = true;
                chk_externalItemsAllUnits.IsEnabled = true;

                cb_externalItemsItems.Text = "";
                cb_externalItemsItems.ItemsSource = items;

                fillEvents();

                fillComboExternalItemsUnits();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalItemsAllItems_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalItemsItems.IsEnabled = true;
                cb_externalItemsItems.SelectedItem = null;

                cb_externalItemsUnits.IsEnabled = false;
                cb_externalItemsUnits.SelectedItem = null;
                chk_externalItemsAllUnits.IsEnabled = false;
                //chk_externalItemsAllUnits.IsChecked = false;

                fillEmptyEvents();
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalItemsAllUnits_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalItemsUnits.IsEnabled = false;
                cb_externalItemsUnits.SelectedItem = null;

                chk_externalItemsAllUnits.IsChecked = true;
                chk_externalItemsAllUnits.IsEnabled = true;

                cb_externalItemsUnits.Text = "";
                cb_externalItemsUnits.ItemsSource = units;

                fillEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalItemsAllUnits_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalItemsUnits.IsEnabled = true;


                chk_externalItemsAllUnits.IsChecked = false;
                chk_externalItemsAllUnits.IsEnabled = true;

                cb_externalItemsUnits.SelectedItem = null;
                fillEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void cb_externalAgentsBranches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                cb_externalAgentsAgentsType.IsEnabled = false;
                chk_externalAgentsAllAgentsType.IsEnabled = true;
                chk_externalAgentsAllAgentsType.IsChecked = true;
                fillEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void cb_externalAgentsAgentsType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //
                cb_externalAgentsCustomer.IsEnabled = false;
                chk_externalAgentsAllCustomers.IsEnabled = true;

                fillComboExternalAgentsAgents();

                fillEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalAgentsAllBranches_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //branch
                cb_externalAgentsBranches.IsEnabled = false;
                cb_externalAgentsBranches.SelectedItem = null;

                //agent type
                cb_externalAgentsAgentsType.IsEnabled = false;
                cb_externalAgentsAgentsType.SelectedItem = null;
                chk_externalAgentsAllAgentsType.IsEnabled = true;
                chk_externalAgentsAllAgentsType.IsChecked = true;
                //customer
                chk_externalAgentsAllCustomers.IsChecked = true;
                chk_externalAgentsAllCustomers.IsEnabled = true;
                cb_externalAgentsCustomer.IsEnabled = false;
                cb_externalAgentsCustomer.SelectedItem = null;
                fillComboExternalAgentsAgentsType();
                cb_externalAgentsBranches.Text = "";
                cb_externalAgentsBranches.ItemsSource = SectionData.BranchesAllWithoutMainList;

                fillEvents();
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalAgentsAllAgentsType_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalAgentsAgentsType.IsEnabled = false;
                cb_externalAgentsAgentsType.SelectedItem = null;

                cb_externalAgentsCustomer.IsEnabled = false;
                cb_externalAgentsCustomer.SelectedItem = null;

                chk_externalAgentsAllCustomers.IsChecked = true;
                chk_externalAgentsAllCustomers.IsEnabled = true;

                fillEvents();
                fillComboExternalAgentsAgents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalAgentsAllCustomers_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalAgentsCustomer.IsEnabled = false;
                cb_externalAgentsCustomer.SelectedItem = null;

                chk_externalAgentsAllCustomers.IsChecked = true;
                chk_externalAgentsAllCustomers.IsEnabled = true;
                cb_externalAgentsCustomer.Text = "";
                cb_externalAgentsCustomer.ItemsSource = agents;
                fillEvents();
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalAgentsAllBranches_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                
                //branch
                cb_externalAgentsBranches.IsEnabled = true;
                cb_externalAgentsBranches.SelectedItem = null;
                //agenttype
                cb_externalAgentsAgentsType.IsEnabled = false;
                cb_externalAgentsAgentsType.SelectedItem = null;
                chk_externalAgentsAllAgentsType.IsEnabled = false;

                //customer
                cb_externalAgentsCustomer.IsEnabled = false;
                cb_externalAgentsCustomer.SelectedItem = null;
                //chk_externalItemsAllUnits.IsChecked = false;
                chk_externalAgentsAllCustomers.IsEnabled = false;

                fillEmptyEvents();
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalAgentsAllAgentsType_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);



                cb_externalAgentsAgentsType.IsEnabled = true;
                cb_externalAgentsAgentsType.SelectedItem = null;

                cb_externalAgentsCustomer.IsEnabled = false;
                cb_externalAgentsCustomer.SelectedItem = null;
                chk_externalAgentsAllCustomers.IsEnabled = false;
                //chk_externalAgentsAllCustomers.IsChecked = false;

                fillEmptyEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalAgentsAllCustomers_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                //
                cb_externalAgentsCustomer.IsEnabled = true;


                chk_externalAgentsAllCustomers.IsChecked = false;
                chk_externalAgentsAllCustomers.IsEnabled = true;

                cb_externalAgentsCustomer.SelectedItem = null;
                fillEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void cb_externalInvoicesBranches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalInvoicesInvoiceType.IsEnabled = true;
                chk_externalInvoicesAllInvoicesType.IsEnabled = true;

                fillComboExternalInvType();

                fillEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalInvoicesAllBranches_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalInvoicesBranches.IsEnabled = false;
                cb_externalInvoicesBranches.SelectedItem = null;
                cb_externalInvoicesInvoiceType.IsEnabled = true;
                chk_externalInvoicesAllInvoicesType.IsEnabled = true;
                fillComboExternalInvType();


                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalInvoicesAllBranches_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalInvoicesBranches.IsEnabled = true;
                cb_externalInvoicesInvoiceType.IsEnabled = false;
                cb_externalInvoicesInvoiceType.SelectedItem = null;
                chk_externalInvoicesAllInvoicesType.IsEnabled = false;
                chk_externalInvoicesAllInvoicesType.IsChecked = false;

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void cb_externalInvoicesInvoiceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalInvoicesInvoice.IsEnabled = true;
                chk_externalInvoicesALlInvoice.IsEnabled = true;

                fillComboExternalInvoiceInvoice();

                fillEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalInvoicesAllInvoicesType_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalInvoicesInvoiceType.IsEnabled = false;
                cb_externalInvoicesInvoiceType.SelectedItem = null;
                cb_externalInvoicesInvoice.IsEnabled = true;
                chk_externalInvoicesALlInvoice.IsEnabled = true;

                fillComboExternalInvoiceInvoice();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalInvoicesAllInvoicesType_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalInvoicesInvoiceType.IsEnabled = true;
                cb_externalInvoicesInvoice.IsEnabled = false;
                cb_externalInvoicesInvoice.SelectedItem = null;
                chk_externalInvoicesALlInvoice.IsEnabled = false;
                chk_externalInvoicesALlInvoice.IsChecked = false;

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalInvoicesALlInvoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalInvoicesInvoice.IsEnabled = false;
                cb_externalInvoicesInvoice.SelectedItem = null;

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_externalInvoicesALlInvoice_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_externalInvoicesInvoice.IsEnabled = true;

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        IEnumerable<ItemTransferInvoice> temp = null;
        private void txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (selectedExternalTab == 0)
                {
                    //temp = itemTransferInvoicesLst
                    temp = temp
                    .Where(s => (s.branchName.Contains(txt_search.Text.ToLower()) ||
                       s.itemName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.unitName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.agentName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.agentType.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.invNumber.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.invType.ToLower().Contains(txt_search.Text.ToLower())
                       ));
                }
                else if (selectedExternalTab == 1)
                {
                    //temp = itemTransferInvoicesLst
                    temp = temp
                          .Where(s => (s.branchName.ToLower().Contains(txt_search.Text.ToLower()) ||
                   s.itemName.ToLower().Contains(txt_search.Text.ToLower()) ||
                   s.unitName.ToLower().Contains(txt_search.Text.ToLower()) ||
                   s.agentName.ToLower().Contains(txt_search.Text.ToLower()) ||
                   s.agentType.ToLower().Contains(txt_search.Text.ToLower()) ||
                   s.invNumber.ToLower().Contains(txt_search.Text.ToLower()) ||
                   s.invType.ToLower().Contains(txt_search.Text.ToLower())
                   ));
                }
                else
                {
                    //temp = itemTransferInvoicesLst
                    temp = temp
                        .GroupBy(x => new { x.branchId, x.invoiceId })
                            .Select(s => new ItemTransferInvoice
                            {
                                branchId = s.FirstOrDefault().branchId,
                                branchName = s.FirstOrDefault().branchName,
                                AgentTypeAgent = s.FirstOrDefault().AgentTypeAgent,
                                ItemUnits = s.FirstOrDefault().ItemUnits
                            ,
                                invNumber = s.FirstOrDefault().invNumber,
                                invType = s.FirstOrDefault().invType,
                                quantity = s.FirstOrDefault().quantity
                            })
                        .Where(s => (s.branchName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.ItemUnits.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.AgentTypeAgent.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.invNumber.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.invType.ToLower().Contains(txt_search.Text.ToLower())
                       ));
                }
                dgStock.ItemsSource = temp;
                txt_count.Text = temp.Count().ToString();
                fillExternalPieChart();
                fillExternalColumnChart();
                fillExternalRowChart();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                itemsTransfer = await statisticModel.GetExternalMov((int)MainWindow.branchID, (int)MainWindow.userID);

                txt_search.Text = "";
                if (selectedExternalTab == 0)
                {
                    cb_externalItemsBranches.SelectedItem = null;
                    cb_externalItemsItems.SelectedItem = null;
                    cb_externalItemsUnits.SelectedItem = null;
                    chk_externalItemsAllBranches.IsChecked = true;
                    chk_externalItemsAllItems.IsChecked = true;
                    chk_externalItemsAllUnits.IsChecked = true;
                    chk_externalItemsIn.IsChecked = true;
                    chk_externalItemsOut.IsChecked = true;
                    dp_externalItemsEndDate.SelectedDate = null;
                    dp_externalItemsStartDate.SelectedDate = null;
                }
                else if (selectedExternalTab == 1)
                {
                    cb_externalAgentsBranches.SelectedItem = null;
                    cb_externalAgentsAgentsType.SelectedItem = null;
                    cb_externalAgentsCustomer.SelectedItem = null;
                    chk_externalAgentsAllBranches.IsChecked = true;
                    chk_externalAgentsAllAgentsType.IsChecked = true;
                    chk_externalAgentsAllCustomers.IsChecked = true;
                    chk_externalAgentsIn.IsChecked = true;
                    chk_externalAgentsOut.IsChecked = true;
                    dp_externalAgentsEndDate.SelectedDate = null;
                    dp_externalAgentsStartDate.SelectedDate = null;
                }

                else if (selectedExternalTab == 2)
                {
                    cb_externalInvoicesBranches.SelectedItem = null;
                    cb_externalInvoicesInvoice.SelectedItem = null;
                    cb_externalInvoicesInvoiceType.SelectedItem = null;
                    chk_externalInvoicesAllBranches.IsChecked = true;
                    chk_externalInvoicesALlInvoice.IsChecked = true;
                    chk_externalInvoicesAllInvoicesType.IsChecked = true;

                    dp_externalInvoicesEndDate.SelectedDate = null;
                    dp_externalInvoicesStartDate.SelectedDate = null;
                }
                fillEvents();
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void selectionChanged(object sender, RoutedEventArgs e)
        {
            fillEventsCall(sender);
        }
        private void fillEventsCall(object sender)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            fillEventsCall(sender);
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GC.Collect();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        Invoice invoice;
        private async void DgStock_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                invoice = new Invoice();
                if (dgStock.SelectedIndex != -1)
                {
                    ItemTransferInvoice item = dgStock.SelectedItem as ItemTransferInvoice;
                    if (item.invoiceId > 0)
                    {
                        invoice = await invoice.GetByInvoiceId(item.invoiceId);
                        MainWindow.mainWindow.BTN_storage_Click(MainWindow.mainWindow.btn_storage, null);
                        View.uc_storage.Instance.UserControl_Loaded(null, null);
                        View.uc_storage.Instance.Btn_itemsExport_Click(View.uc_storage.Instance.btn_importExport, null);
                        uc_itemsExport.Instance.UserControl_Loaded(null, null);
                        uc_itemsExport._ProcessType = invoice.invType;
                        uc_itemsExport.Instance.invoice = invoice;
                        uc_itemsExport.isFromReport = true;
                        await uc_itemsExport.Instance.fillOrderInputs(invoice);
                    }
                }
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_externalItemsBranches_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = SectionData.BranchesAllWithoutMainList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_externalItemsItems_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = items.Where(p => p.ItemName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_externalItemsUnits_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = units.Where(p => p.UnitName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_externalAgentsBranches_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = SectionData.BranchesAllWithoutMainList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_externalAgentsCustomer_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = agents.Where(p => p.AgentName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        #endregion

        #region tabs
        private async void btn_externalItems_Click(object sender, RoutedEventArgs e)
        {//items
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                selectedExternalTab = 0;
                txt_search.Text = "";

                paintExternlaChilds();
                grid_externalItems.Visibility = Visibility.Visible;
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_item);
                path_item.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                showSelectedTabColumn();

                await SectionData.fillBranchesWithoutMain(cb_externalItemsBranches);
                chk_externalItemsIn.IsChecked = true;
                chk_externalItemsOut.IsChecked = true;
                dp_externalItemsStartDate.SelectedDate = null;
                dp_externalItemsEndDate.SelectedDate = null;
                chk_externalItemsAllBranches.IsChecked = true;

                // chk_externalItemsAllBranches_Checked(chk_externalItemsAllBranches, null);
                fillEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void btn_externalAgents_Click(object sender, RoutedEventArgs e)
        {//agents
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                selectedExternalTab = 1;
                txt_search.Text = "";

                paintExternlaChilds();
                grid_externalAgents.Visibility = Visibility.Visible;
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_agent);
                path_agent.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                showSelectedTabColumn();

                await SectionData.fillBranchesWithoutMain(cb_externalAgentsBranches);

                chk_externalAgentsIn.IsChecked = true;
                chk_externalAgentsOut.IsChecked = true;
                dp_externalAgentsStartDate.SelectedDate = null;
                dp_externalAgentsEndDate.SelectedDate = null;
                chk_externalAgentsAllBranches.IsChecked = true;
                chk_externalAgentsAllBranches_Checked(chk_externalAgentsAllBranches, null);
                fillEvents();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region charts
        private void fillExternalRowChart()
        {
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            List<int> pTemp = new List<int>();
            List<int> sTemp = new List<int>();
            List<int> pbTemp = new List<int>();
            List<int> sbTemp = new List<int>();

            //var temp = itemTransferInvoicesLst;

            var result = temp.GroupBy(x => new { x.branchId, x.invoiceId }).Select(x => new ItemTransferInvoice
            {
                invType = x.FirstOrDefault().invType,
                branchId = x.FirstOrDefault().branchId
            });

            invCount = result.GroupBy(x => x.branchId).Select(x => new ItemTransferInvoice
            {
                PCount = x.Where(g => g.invType == "p").Count(),
                SCount = x.Where(g => g.invType == "s").Count(),
                PbCount = x.Where(g => g.invType == "pb").Count(),
                SbCount = x.Where(g => g.invType == "sb").Count()
            });

            for (int i = 0; i < agentsCount.Count(); i++)
            {
                pTemp.Add(invCount.ToList().Skip(i).FirstOrDefault().PCount);
                pbTemp.Add(invCount.ToList().Skip(i).FirstOrDefault().PbCount);
                sTemp.Add(invCount.ToList().Skip(i).FirstOrDefault().SCount);
                sbTemp.Add(invCount.ToList().Skip(i).FirstOrDefault().SbCount);
            }
            var tempName = temp.GroupBy(s => new { s.branchId, s.invType }).Select(s => new
            {
                locationName = s.FirstOrDefault().branchName
            });
            names.AddRange(tempName.Select(nn => nn.locationName));

            SeriesCollection rowChartData = new SeriesCollection();
            for (int i = 0; i < pTemp.Count(); i++)
            {
                MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            rowChartData.Add(
          new LineSeries
          {
              Values = pTemp.AsChartValues(),
              Title = MainWindow.resourcemanager.GetString("tr_Purchases")

          }); ;
            rowChartData.Add(
      new LineSeries
      {
          Values = pbTemp.AsChartValues(),
          Title = MainWindow.resourcemanager.GetString("trPurchaseReturnInvoice")
      });
            rowChartData.Add(
      new LineSeries
      {
          Values = sTemp.AsChartValues(),
          Title = MainWindow.resourcemanager.GetString("tr_Sales")
      });
            rowChartData.Add(
      new LineSeries
      {
          Values = sbTemp.AsChartValues(),
          Title = MainWindow.resourcemanager.GetString("trSalesReturnInvoice")

      });
            rowChart.Series = rowChartData;
            DataContext = this;
        }
        private void fillExternalColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();

            //var temp = itemTransferInvoicesLst;
            var res = temp.GroupBy(x => new { x.branchId, x.agentId }).Select(x => new ItemTransferInvoice
            {
                agentId = x.FirstOrDefault().agentId,
                branchId = x.FirstOrDefault().branchId,
                agentType = x.FirstOrDefault().agentType,
                branchName = x.FirstOrDefault().branchName
            });
            agentsCount = res.GroupBy(x => x.branchId).Select(x => new ItemTransferInvoice
            {
                VenCount = x.Where(g => g.agentType == "v").Count(),
                CusCount = x.Where(g => g.agentType == "c").Count()
            }
            );

            var tempName = res.GroupBy(s => new { s.branchId }).Select(s => new
            {
                itemName = s.FirstOrDefault().branchName,
            });
            names.AddRange(tempName.Select(nn => nn.itemName));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<int> cP = new List<int>();
            List<int> cPb = new List<int>();

            int xCount = 6;
            if (agentsCount.Count() <= 6) xCount = agentsCount.Count();

            for (int i = 0; i < xCount; i++)
            {
                cP.Add(agentsCount.ToList().Skip(i).FirstOrDefault().VenCount);
                cPb.Add(agentsCount.ToList().Skip(i).FirstOrDefault().CusCount);
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (agentsCount.Count() > 6)
            {
                int c = 0, v = 0;
                for (int i = 6; i < agentsCount.Count(); i++)
                {
                    v = v + agentsCount.ToList().Skip(i).FirstOrDefault().VenCount;
                    c = c + agentsCount.ToList().Skip(i).FirstOrDefault().CusCount;
                }
                if (!((c == 0) && (v == 0)))
                {
                    cP.Add(v);
                    cPb.Add(c);
                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cP.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("tr_Vendor")
            }); ;
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cPb.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("tr_Customer")
            });

            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillExternalPieChart()
        {
            List<string> titles = new List<string>();
            List<string> titles1 = new List<string>();
            List<decimal> x = new List<decimal>();
            titles.Clear();
            titles1.Clear();

            // var temp = itemTransferInvoicesLst;
            var result = temp
                .GroupBy(s => new { s.itemId, s.unitId })
                .Select(s => new ItemTransferInvoice
                {
                    itemId = s.FirstOrDefault().itemId,
                    unitId = s.FirstOrDefault().unitId,
                    quantity = s.Sum(g => g.quantity),
                    itemName = s.FirstOrDefault().itemName,
                    unitName = s.FirstOrDefault().unitName,
                }).OrderByDescending(s => s.quantity);
            x = result.Select(m => (decimal)m.quantity).ToList();
            titles = result.Select(m => m.itemName).ToList();
            titles1 = result.Select(m => m.unitName).ToList();
            int count = x.Count();
            SeriesCollection piechartData = new SeriesCollection();
            for (int i = 0; i < count; i++)
            {
                List<decimal> final = new List<decimal>();
                List<string> lable = new List<string>();
                if (i < 5)
                {
                    final.Add(x.Max());
                    lable.Add(titles.Skip(i).FirstOrDefault() + "-" + titles1.Skip(i).FirstOrDefault());
                    piechartData.Add(
                      new PieSeries
                      {
                          Values = final.AsChartValues(),
                          Title = lable.FirstOrDefault(),
                          DataLabels = true,
                      }
                  );
                    x.Remove(x.Max());
                }
                else
                {
                    final.Add(x.Sum());
                    piechartData.Add(
                      new PieSeries
                      {
                          Values = final.AsChartValues(),
                          Title = MainWindow.resourcemanager.GetString("trOthers"),
                          DataLabels = true,
                      }
                  ); ;
                    break;
                }

            }
            chart1.Series = piechartData;
        }
        #endregion

        #region report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();


        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            string firstTitle = "external";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string inchk = "";
            string outchk = "";
            string startDate = "";
            string endDate = "";
            string branchval = "";
            string itemval = "";
            string unitval = "";
            string searchval = "";
            //paramarr.Add(new ReportParameter("trIn", MainWindow.resourcemanagerreport.GetString("trIn")));
            //paramarr.Add(new ReportParameter("trOut", MainWindow.resourcemanagerreport.GetString("trOut")));

            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            if (isArabic)
            {
                if (selectedExternalTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Storage\External\Ar\ArItem.rdlc";
                    secondTitle = "items";
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItem")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trUnit")));
                    startDate = dp_externalItemsStartDate.SelectedDate != null ? SectionData.DateToString(dp_externalItemsStartDate.SelectedDate) : "";

                    endDate = dp_externalItemsEndDate.SelectedDate != null ? SectionData.DateToString(dp_externalItemsEndDate.SelectedDate) : "";

                    branchval = cb_externalItemsBranches.SelectedItem != null
               && (chk_externalItemsAllBranches.IsChecked == false || chk_externalItemsAllBranches.IsChecked == null)
               ? cb_externalItemsBranches.Text : (chk_externalItemsAllBranches.IsChecked == true ? all : "");

                    itemval = cb_externalItemsItems.SelectedItem != null
                       && (chk_externalItemsAllItems.IsChecked == false || chk_externalItemsAllItems.IsChecked == null)
                       && branchval != ""
                       ? cb_externalItemsItems.Text : (chk_externalItemsAllItems.IsChecked == true && branchval != "" ? all : "");

                    unitval = cb_externalItemsUnits.SelectedItem != null
                      && (chk_externalItemsAllUnits.IsChecked == false || chk_externalItemsAllUnits.IsChecked == null)
                    && itemval != ""
                      ? cb_externalItemsUnits.Text : (chk_externalItemsAllUnits.IsChecked == true && itemval != "" ? all : "");
                    inchk = chk_externalItemsIn.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trIn") : "";
                    outchk = chk_externalItemsOut.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trOut") : "";

                }
                else if (selectedExternalTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Storage\External\Ar\ArAgent.rdlc";
                    secondTitle = "tragent";
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trAgentType")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trVendor/Customer")));
                    startDate = dp_externalAgentsStartDate.SelectedDate != null ? SectionData.DateToString(dp_externalAgentsStartDate.SelectedDate) : "";

                    endDate = dp_externalAgentsEndDate.SelectedDate != null ? SectionData.DateToString(dp_externalAgentsEndDate.SelectedDate) : "";

                    branchval = cb_externalAgentsBranches.SelectedItem != null
               && (chk_externalAgentsAllBranches.IsChecked == false || chk_externalAgentsAllBranches.IsChecked == null)
               ? cb_externalAgentsBranches.Text : (chk_externalAgentsAllBranches.IsChecked == true ? all : "");

                    itemval = cb_externalAgentsAgentsType.SelectedItem != null
                       && (chk_externalAgentsAllAgentsType.IsChecked == false || chk_externalAgentsAllAgentsType.IsChecked == null)
                      && branchval != ""
                       ? cb_externalAgentsAgentsType.Text : (chk_externalAgentsAllAgentsType.IsChecked == true && branchval != "" ? all : "");

                    unitval = cb_externalAgentsCustomer.SelectedItem != null
                      && (chk_externalAgentsAllCustomers.IsChecked == false || chk_externalAgentsAllCustomers.IsChecked == null)
                      && itemval != ""
                      ? cb_externalAgentsCustomer.Text : (chk_externalAgentsAllCustomers.IsChecked == true && itemval != "" ? all : "");

                    inchk = chk_externalItemsIn.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trIn") : "";
                    outchk = chk_externalItemsOut.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trOut") : "";
                    // trAgentType
                    //trVendor/Customer
                }
                else if (selectedExternalTab == 2)
                {
                    addpath = @"\Reports\StatisticReport\Storage\External\Ar\ArInvoice.rdlc";
                    secondTitle = "invoice";
                }
            }
            else
            {
                if (selectedExternalTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Storage\External\En\Item.rdlc";
                    secondTitle = "items";
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItem")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trUnit")));
                    startDate = dp_externalItemsStartDate.SelectedDate != null ? SectionData.DateToString(dp_externalItemsStartDate.SelectedDate) : "";

                    endDate = dp_externalItemsEndDate.SelectedDate != null ? SectionData.DateToString(dp_externalItemsEndDate.SelectedDate) : "";

                    branchval = cb_externalItemsBranches.SelectedItem != null
               && (chk_externalItemsAllBranches.IsChecked == false || chk_externalItemsAllBranches.IsChecked == null)
               ? cb_externalItemsBranches.Text : (chk_externalItemsAllBranches.IsChecked == true ? all : "");

                    itemval = cb_externalItemsItems.SelectedItem != null
                       && (chk_externalItemsAllItems.IsChecked == false || chk_externalItemsAllItems.IsChecked == null)
                       && branchval != ""
                       ? cb_externalItemsItems.Text : (chk_externalItemsAllItems.IsChecked == true && branchval != "" ? all : "");

                    unitval = cb_externalItemsUnits.SelectedItem != null
                      && (chk_externalItemsAllUnits.IsChecked == false || chk_externalItemsAllUnits.IsChecked == null)
                    && itemval != ""
                      ? cb_externalItemsUnits.Text : (chk_externalItemsAllUnits.IsChecked == true && itemval != "" ? all : "");
                    inchk = chk_externalItemsIn.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trIn") : "";
                    outchk = chk_externalItemsOut.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trOut") : "";

                }
                else if (selectedExternalTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Storage\External\En\Agent.rdlc";
                    secondTitle = "tragent";
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trAgentType")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trVendor/Customer")));
                    startDate = dp_externalAgentsStartDate.SelectedDate != null ? SectionData.DateToString(dp_externalAgentsStartDate.SelectedDate) : "";

                    endDate = dp_externalAgentsEndDate.SelectedDate != null ? SectionData.DateToString(dp_externalAgentsEndDate.SelectedDate) : "";

                    branchval = cb_externalAgentsBranches.SelectedItem != null
               && (chk_externalAgentsAllBranches.IsChecked == false || chk_externalAgentsAllBranches.IsChecked == null)
               ? cb_externalAgentsBranches.Text : (chk_externalAgentsAllBranches.IsChecked == true ? all : "");

                    itemval = cb_externalAgentsAgentsType.SelectedItem != null
                       && (chk_externalAgentsAllAgentsType.IsChecked == false || chk_externalAgentsAllAgentsType.IsChecked == null)
                      && branchval != ""
                       ? cb_externalAgentsAgentsType.Text : (chk_externalAgentsAllAgentsType.IsChecked == true && branchval != "" ? all : "");

                    unitval = cb_externalAgentsCustomer.SelectedItem != null
                      && (chk_externalAgentsAllCustomers.IsChecked == false || chk_externalAgentsAllCustomers.IsChecked == null)
                      && itemval != ""
                      ? cb_externalAgentsCustomer.Text : (chk_externalAgentsAllCustomers.IsChecked == true && itemval != "" ? all : "");

                    inchk = chk_externalItemsIn.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trIn") : "";
                    outchk = chk_externalItemsOut.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trOut") : "";

                }
                else if (selectedExternalTab == 2)
                {
                    addpath = @"\Reports\StatisticReport\Storage\External\En\Invoice.rdlc";
                    secondTitle = "invoice";
                }
            }
            paramarr.Add(new ReportParameter("BranchStore", branchval));
            paramarr.Add(new ReportParameter("ItemVal", itemval));
            paramarr.Add(new ReportParameter("UnitVal", unitval));
            inchk = inchk + " , " + outchk;
            inchk = inchk.IndexOf(',') == 1 || inchk.IndexOf(',') == inchk.Length - 2 ? inchk.Replace(",", "") : inchk;

            paramarr.Add(new ReportParameter("trIn", inchk));
            //  paramarr.Add(new ReportParameter("trOut", outchk));

            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));


            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //
            /*
             trNo
trType
trDate
trBranch
trItem
trUnit
trAgentType
trName
trItemUnit
trType-Name
trQTR

             * */
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trType", MainWindow.resourcemanagerreport.GetString("trType")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trAgentType", MainWindow.resourcemanagerreport.GetString("trAgentType")));
            paramarr.Add(new ReportParameter("trName", MainWindow.resourcemanagerreport.GetString("trName")));
            paramarr.Add(new ReportParameter("trItemUnit", MainWindow.resourcemanagerreport.GetString("trItemUnit")));
            paramarr.Add(new ReportParameter("trTypeName", MainWindow.resourcemanagerreport.GetString("trType-Name")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
        
            
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trStorageReport") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));

            //itemTransferInvoiceExternal
            clsReports.itemTransferInvoiceExternal(temp, rep, reppath, paramarr);

            clsReports.Header(paramarr);

            rep.SetParameters(paramarr);

            rep.Refresh();


        }

        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
                BuildReport();

                saveFileDialog.Filter = "PDF|*.pdf;";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;
                    LocalReportExtensions.ExportToPDF(rep, filepath);
                }
                #endregion

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
                BuildReport();
                LocalReportExtensions.PrintToPrinter(rep);
                #endregion

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
                //Thread t1 = new Thread(() =>
                //  {
                BuildReport();
                this.Dispatcher.Invoke(() =>
            {
                saveFileDialog.Filter = "EXCEL|*.xls;";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;
                    LocalReportExtensions.ExportToExcel(rep, filepath);
                }
            });


                //});
                //  t1.Start();

                #endregion

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
                Window.GetWindow(this).Opacity = 0.2;
                string pdfpath = "";



                pdfpath = @"\Thumb\report\temp.pdf";
                pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                BuildReport();

                LocalReportExtensions.ExportToPDF(rep, pdfpath);
                wd_previewPdf w = new wd_previewPdf();
                w.pdfPath = pdfpath;
                if (!string.IsNullOrEmpty(w.pdfPath))
                {
                    w.ShowDialog();
                    w.wb_pdfWebViewer.Dispose();
                }
                Window.GetWindow(this).Opacity = 1;
                #endregion

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion
        
        bool showChart = true;

        double col_reportChartWidth = 0;
        private void btn_pieChart_Click(object sender, RoutedEventArgs e)
        {

            showChart = !showChart;

            if (showChart)
            {
                col_reportChart.Width = new GridLength(col_reportChartWidth);

                path_reportPath1.Fill = Application.Current.Resources["reportPath1"] as SolidColorBrush;
                path_reportPath2.Fill = Application.Current.Resources["reportPath2"] as SolidColorBrush;
                path_reportPath3.Fill = Application.Current.Resources["reportPath3"] as SolidColorBrush;
                tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");
            }
            else
            {
                col_reportChart.Width = new GridLength(0);

                path_reportPath1.Fill = Application.Current.Resources["Grey"] as SolidColorBrush;
                path_reportPath2.Fill = Application.Current.Resources["Grey"] as SolidColorBrush;
                path_reportPath3.Fill = Application.Current.Resources["Grey"] as SolidColorBrush;
                tt_pieChart.Content = MainWindow.resourcemanager.GetString("trShow");
            }
        }
        private void Btn_printChart_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Button button = sender as Button;
                string buttonTag = button.Tag.ToString();
                Window.GetWindow(this).Opacity = 0.2;

                wd_chart w = new wd_chart();
                if (buttonTag.Equals("cartesianChart"))
                {
                    w.type = "cartesianChart";
                    w.cartesianChart.Series = cartesianChart.Series;
                    w.axcolumn.Labels = axcolumn.Labels;

                }
                else if (buttonTag.Equals("chart1"))
                {
                    w.type = "pieChart";
                    w.pieChart.Series = chart1.Series;

                }
                else if (buttonTag.Equals("rowChart"))
                {
                    w.type = "cartesianChart";
                    w.cartesianChart.Series = rowChart.Series;
                    w.axcolumn.Labels = MyAxis.Labels;
                }
                w.ShowDialog();

                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);


            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }

}




