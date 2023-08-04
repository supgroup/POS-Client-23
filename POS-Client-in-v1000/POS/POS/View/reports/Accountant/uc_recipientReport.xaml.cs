using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static POS.Classes.Statistics;

namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_recipientReport.xaml
    /// </summary>
    public partial class uc_recipientReport : UserControl
    {
        #region variables
        Statistics statisticModel = new Statistics();
        List<CashTransferSts> payments;

        IEnumerable<VendorCombo> vendorCombo;

        IEnumerable<PaymentsTypeCombo> payCombo;

        IEnumerable<AccountantCombo> accCombo;
        IEnumerable<ShippingCombo> shippingCombo;

        int selectedTab = 0;
        #endregion

        public uc_recipientReport()
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
        private static uc_recipientReport _instance;
        public static uc_recipientReport Instance
        {
            get
            {
                if (_instance == null) _instance = new uc_recipientReport();
                return _instance;
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
                col_reportChartWidth = col_reportChart.ActualWidth;

                tb_totalCurrency.Text = AppSettings.Currency;

                payments = await statisticModel.GetReceipt();

                Btn_vendor_Click(btn_vendor , null);

                #region key up
                // key_up search Person name
                cb_vendors.IsTextSearchEnabled = false;
                cb_vendors.IsEditable = true;
                cb_vendors.StaysOpenOnEdit = true;
                cb_vendors.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_vendors.Text = "";
                // key_up search Person name
                cb_vendorAccountant.IsTextSearchEnabled = false;
                cb_vendorAccountant.IsEditable = true;
                cb_vendorAccountant.StaysOpenOnEdit = true;
                cb_vendorAccountant.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_vendorAccountant.Text = "";
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

        #region methods
        private void translate()
        {
            tt_vendor.Content = MainWindow.resourcemanager.GetString("trVendors");
            tt_customer.Content = MainWindow.resourcemanager.GetString("trCustomers");
            tt_user.Content = MainWindow.resourcemanager.GetString("trUsers");
            tt_administrativeDeposit.Content = MainWindow.resourcemanager.GetString("trAdministrativeDeposits");
            tt_shipping.Content = MainWindow.resourcemanager.GetString("trShippingCompanies");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendors, MainWindow.resourcemanager.GetString("trVendor") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendorPayType, MainWindow.resourcemanager.GetString("trPaymentType") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendorAccountant, MainWindow.resourcemanager.GetString("trPaymentType") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendorAccountant, MainWindow.resourcemanager.GetString("trAccoutant") + "...");//

            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_vendorStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_vendorEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));

            chk_allVendors.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allVendorsPaymentType.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allVendorsAccountant.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
            //col_agentName
            col_tansNum.Header = MainWindow.resourcemanager.GetString("trNo");
            col_pos.Header = MainWindow.resourcemanager.GetString("trPosTooltip");
            col_processType.Header = MainWindow.resourcemanager.GetString("trPaymentType");
            col_updateUserAcc.Header = MainWindow.resourcemanager.GetString("trAccoutant");
            col_agentName.Header = MainWindow.resourcemanager.GetString("trDepositor");
            col_customer.Header = MainWindow.resourcemanager.GetString("trRecipientTooltip");
            col_user.Header = MainWindow.resourcemanager.GetString("trDepositor");
            col_company.Header = MainWindow.resourcemanager.GetString("trCompany");
            col_shipping.Header = MainWindow.resourcemanager.GetString("trCompany");
            col_updateDate.Header = MainWindow.resourcemanager.GetString("trDate");
            col_cash.Header = MainWindow.resourcemanager.GetString("trAmount");
            col_notes.Header = MainWindow.resourcemanager.GetString("trNote");

            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");

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
        List<Agent> vendors = new List<Agent>();
        private void fillVendor()
        {
            if (selectedTab == 0)
                //vendors = payments.Where(p => p.side == "v" && (p.invShippingCompanyId == null || (p.invShippingCompanyId != null && p.shipUserId != null))).GroupBy(g => g.agentId).Select(g => new Agent { agentId = g.FirstOrDefault().agentId.Value, name = g.FirstOrDefault().agentName }).ToList();
                vendors = payments.Where(g => g.agentId != null && g.side == "v").GroupBy(g => g.agentId).Select(g => new Agent { agentId = g.FirstOrDefault().agentId.Value, name = g.FirstOrDefault().agentName }).ToList();
            else if (selectedTab == 1)
                //vendors = payments.Where(p => p.side == "c" && (p.invShippingCompanyId == null || (p.invShippingCompanyId != null && p.shipUserId != null))).GroupBy(g => g.agentId).Select(g => new Agent { agentId = g.FirstOrDefault().agentId.Value, name = g.FirstOrDefault().agentName }).ToList();
                vendors = payments.Where(g => g.agentId != null && g.side == "c").GroupBy(g => g.agentId).Select(g => new Agent { agentId = g.FirstOrDefault().agentId.Value, name = g.FirstOrDefault().agentName }).ToList();

            if (vendors.Where(x => x.name == "unknown").Count() > 0)
                vendors.Where(x => x.name == "unknown").FirstOrDefault().name = MainWindow.resourcemanager.GetString("trUnKnown");

            cb_vendors.SelectedValuePath = "agentId";
            cb_vendors.DisplayMemberPath = "name";
            cb_vendors.ItemsSource = vendors;
        }
        private void fillVendorCombo(IEnumerable<VendorCombo> list, ComboBox cb)
        {
            cb.SelectedValuePath = "VendorId";
            cb.DisplayMemberPath = "VendorName";
            cb.ItemsSource = list;
        }
        private void fillPaymentsTypeCombo(ComboBox cb)
        {
            cb.SelectedValuePath = "PaymentsTypeName";
            cb.DisplayMemberPath = "PaymentsTypeText";
            cb.ItemsSource = payCombo;
        }
        private void fillAccCombo(IEnumerable<AccountantCombo> list, ComboBox cb)
        {
            cb.SelectedValuePath = "Accountant";
            cb.DisplayMemberPath = "Accountant";
            cb.ItemsSource = list;
        }
        private void fillSalaryCombo(IEnumerable<VendorCombo> list, ComboBox cb)
        {
            cb.SelectedValuePath = "UserId";
            cb.DisplayMemberPath = "UserAcc";
            cb.ItemsSource = list;
        }
        private void fillShippingCombo(IEnumerable<ShippingCombo> list, ComboBox cb)
        {
            cb.SelectedValuePath = "ShippingId";
            cb.DisplayMemberPath = "ShippingName";
            cb.ItemsSource = list;
        }
        List<CashTransferSts> recLst;
        private List<CashTransferSts> fillList(List<CashTransferSts> payments, ComboBox vendor, ComboBox payType, ComboBox accountant
           , DatePicker startDate, DatePicker endDate)
        {

            var selectedItem1 = vendor.SelectedItem as Agent;
            var selectedItem2 = payType.SelectedItem as PaymentsTypeCombo;
            var selectedItem3 = accountant.SelectedItem as AccountantCombo;
            var selectedItem4 = vendor.SelectedItem as VendorCombo;
            var selectedItem5 = vendor.SelectedItem as ShippingCombo;

            var result = payments.Where(x => (
              (vendor.SelectedItem != null ? x.agentId == selectedItem1.agentId : true)
                        && (payType.SelectedItem != null ? x.processType == selectedItem2.PaymentsTypeName : true)
                        && (accountant.SelectedItem != null ? x.updateUserAcc == selectedItem3.Accountant : true)
                        && (startDate.SelectedDate != null ? x.updateDate.Value.Date >= startDate.SelectedDate.Value.Date : true)
                        && (endDate.SelectedDate != null ? x.updateDate.Value.Date <= endDate.SelectedDate.Value.Date : true)));
            if (selectedTab == 2)
            {
                result = payments.Where(x => (
             (vendor.SelectedItem != null ? x.userId == selectedItem4.UserId : true)
                       && (payType.SelectedItem != null ? x.processType == selectedItem2.PaymentsTypeName : true)
                       && (accountant.SelectedItem != null ? x.updateUserAcc == selectedItem3.Accountant : true)
                       && (startDate.SelectedDate != null ? x.updateDate.Value.Date >= startDate.SelectedDate.Value.Date : true)
                       && (endDate.SelectedDate != null ? x.updateDate.Value.Date <= endDate.SelectedDate.Value.Date : true)));
            }
            else if (selectedTab == 4)
            {
                if(selectedItem5 != null)
                    result = payments.Where(x => (
                 (vendor.SelectedItem != null ? x.shippingCompanyId == selectedItem5.ShippingId : true)
                           && (payType.SelectedItem != null    ? x.processType == selectedItem2.PaymentsTypeName : true)
                           && (accountant.SelectedItem != null ? x.updateUserAcc == selectedItem3.Accountant : true)
                           && (startDate.SelectedDate != null  ? x.updateDate.Value.Date >= startDate.SelectedDate.Value.Date : true)
                           && (endDate.SelectedDate != null    ? x.updateDate.Value.Date <= endDate.SelectedDate.Value.Date : true)));
            }
            recLst = result.ToList();
            return result.ToList();
        }
        private void fillbyComboValue()
        {
            if ((cb_vendorAccountant.SelectedItem == null && chk_allVendorsAccountant.IsChecked == false) ||
                 (cb_vendorPayType.SelectedItem == null && chk_allVendorsPaymentType.IsChecked == false) ||
                 (cb_vendors.SelectedItem == null && chk_allVendors.IsChecked == false))
            {
                fillEmptyBySide();
            }
            else
            {
                fillBySide();
            }
        }
        private void fillEmptyEvents(string side)
        {
            temp = new List<CashTransferSts>();


            dgPayments.ItemsSource = temp;
            txt_count.Text = temp.Count().ToString();
            
            decimal total = 0;
            total = temp.Select(b => b.cash.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);

            //charts
            fillColumnChart();
            fillPieChart();
            fillRowChart();
        }
        private void fillEmptyBySide()
        {
            if (selectedTab == 0)
                fillEmptyEvents("v");
            else if (selectedTab == 1)
                fillEmptyEvents("c");
            else if (selectedTab == 2)
                fillEmptyEvents("u");
            
            else if (selectedTab == 5)
                fillEmptyEvents("m");
            else if (selectedTab == 6)
                fillEmptyEvents("sh");
         
        }
        public void paint()
        {
            //bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);

            bdr_customer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_vendor.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_user.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_administrativeDeposit.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_shipping.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_customer.Fill = Brushes.White;
            path_vendor.Fill = Brushes.White;
            path_user.Fill = Brushes.White;
            path_administrativeDeposit.Fill = Brushes.White;
            path_shipping.Fill = Brushes.White;

        }
        private void isEnabledButtons()
        {
            btn_customer.IsEnabled = true;
            btn_vendor.IsEnabled = true;
            btn_user.IsEnabled = true;
            btn_administrativeDeposit.IsEnabled = true;
            btn_shipping.IsEnabled = true;
        }
        private void hideAllColumn()
        {

            col_tansNum.Visibility = Visibility.Hidden;
            col_processType.Visibility = Visibility.Hidden;
            col_updateUserAcc.Visibility = Visibility.Hidden;

            col_agentName.Visibility = Visibility.Hidden;
            col_customer.Visibility = Visibility.Hidden;
            col_user.Visibility = Visibility.Hidden;
            col_shipping.Visibility = Visibility.Hidden;

            col_company.Visibility = Visibility.Hidden;
            col_updateDate.Visibility = Visibility.Hidden;
            col_cash.Visibility = Visibility.Hidden;
        }
        private void fillBySide()
        {
            if (selectedTab == 0)
                fillEvents("v");
            else if (selectedTab == 1)
                fillEvents("c");
            else if (selectedTab == 2)
                fillEvents("u");
            else if (selectedTab == 3)
                fillEvents("m");
            else if (selectedTab == 4)
                fillEvents("sh");
        }
        IEnumerable<CashTransferSts> temp = null;
        private void fillEvents(string side)
        {
            temp = fillList(payments, cb_vendors, cb_vendorPayType, cb_vendorAccountant, dp_vendorStartDate, dp_vendorEndDate)
                .Where(x => x.side == side && x.processType != "balance" && x.processType != "destroy" && x.processType != "shortage" && x.processType != "commissionAgent");
            if (selectedTab == 1)//customer
            {
                temp = temp.Where(t => (t.shippingCompanyId == null && t.userId == null && t.agentId != null) ||
                                       (t.shippingCompanyId != null && t.userId != null && t.agentId != null) ||
                                       t.agentId == null);
            }
            else if (selectedTab == 4)//shipping
            {
                temp = temp.Where(t => (t.shippingCompanyId != null && t.userId == null && t.agentId != null) ||
                                       (t.shippingCompanyId != null && t.userId == null && t.agentId == null));
            }
            dgPayments.ItemsSource = temp;
            txt_count.Text = dgPayments.Items.Count.ToString();

            decimal total = 0;
            total = temp.Select(b => b.cash.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);

            fillColumnChart();
            fillPieChart();
            fillRowChart();
        }
        #endregion

        #region tabs
        private void Btn_vendor_Click(object sender, RoutedEventArgs e)
        {//vendors
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendors, MainWindow.resourcemanager.GetString("trVendorHint"));
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                hideAllColumn();
                selectedTab = 0;
                //view columns
                col_tansNum.Visibility = Visibility.Visible;
                col_processType.Visibility = Visibility.Visible;
                col_updateUserAcc.Visibility = Visibility.Visible;
                col_agentName.Visibility = Visibility.Visible;
                col_company.Visibility = Visibility.Visible;
                col_updateDate.Visibility = Visibility.Visible;
                col_cash.Visibility = Visibility.Visible;

                txt_search.Text = "";

                paint();
                bdr_vendor.Background = Brushes.White;
                path_vendor.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                isEnabledButtons();
                btn_vendor.IsEnabled = false;
                btn_vendor.Opacity = 1;

                cb_vendors.Visibility = Visibility.Visible;
                cb_vendors.SelectedItem = null;
                chk_allVendors.Visibility = Visibility.Visible;

                dp_vendorStartDate.SelectedDate = null;
                dp_vendorEndDate.SelectedDate = null;

                chk_allVendorsPaymentType.IsChecked = true;
                chk_allVendors.IsChecked = true;
                chk_allVendorsAccountant.IsChecked = true;

                //vendorCombo = statisticModel.getVendorCombo(payments, "v");
                //fillVendorCombo(vendorCombo, cb_vendors);
                fillVendor();

                payCombo = statisticModel.getPaymentsTypeComboBySide(payments, "v");
                fillPaymentsTypeCombo(cb_vendorPayType);

                accCombo = statisticModel.getAccounantCombo(payments, "v");
                fillAccCombo(accCombo, cb_vendorAccountant);
                //
                
                fillEvents("v");
                //
           

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
        private void Btn_customer_Click(object sender, RoutedEventArgs e)
        {//customers
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendors, MainWindow.resourcemanager.GetString("trCustomerHint"));
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                hideAllColumn();
                selectedTab = 1;
                //view columns
                col_tansNum.Visibility = Visibility.Visible;
                col_processType.Visibility = Visibility.Visible;
                col_updateUserAcc.Visibility = Visibility.Visible;
                col_customer.Visibility = Visibility.Visible;
                col_company.Visibility = Visibility.Visible;
                col_updateDate.Visibility = Visibility.Visible;
                col_cash.Visibility = Visibility.Visible;

                txt_search.Text = "";

                paint();
                bdr_customer.Background = Brushes.White;
                path_customer.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                isEnabledButtons();
                btn_customer.IsEnabled = false;
                btn_customer.Opacity = 1;

                cb_vendors.Visibility = Visibility.Visible;
                cb_vendors.SelectedItem = null;
                chk_allVendors.Visibility = Visibility.Visible;

                dp_vendorStartDate.SelectedDate = null;
                dp_vendorEndDate.SelectedDate = null;

                chk_allVendorsPaymentType.IsChecked = true;
                chk_allVendors.IsChecked = true;
                chk_allVendorsAccountant.IsChecked = true;

                //vendorCombo = statisticModel.getVendorCombo(payments, "c");
                //fillVendorCombo(vendorCombo, cb_vendors);
                fillVendor();

                payCombo = statisticModel.getPaymentsTypeComboBySide(payments, "c");
                fillPaymentsTypeCombo(cb_vendorPayType);

                accCombo = statisticModel.getAccounantCombo(payments, "c");
                fillAccCombo(accCombo, cb_vendorAccountant);

               
                fillEvents("c");

                

                // key_up search Person name
                cb_vendors.IsTextSearchEnabled = false;
                cb_vendors.IsEditable = true;
                cb_vendors.StaysOpenOnEdit = true;
                cb_vendors.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_vendors.Text = "";

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
        private void Btn_user_Click(object sender, RoutedEventArgs e)
        {//users
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendors, MainWindow.resourcemanager.GetString("trUserHint"));
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                hideAllColumn();
                selectedTab = 2;
                //view columns
                col_tansNum.Visibility = Visibility.Visible;
                col_processType.Visibility = Visibility.Visible;
                col_updateUserAcc.Visibility = Visibility.Visible;
                col_user.Visibility = Visibility.Visible;
                col_updateDate.Visibility = Visibility.Visible;
                col_cash.Visibility = Visibility.Visible;

                txt_search.Text = "";

                paint();
                bdr_user.Background = Brushes.White;
                path_user.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                isEnabledButtons();
                btn_vendor.IsEnabled = false;
                btn_vendor.Opacity = 1;

                cb_vendors.Visibility = Visibility.Visible;
                cb_vendors.SelectedItem = null;
                chk_allVendors.Visibility = Visibility.Visible;

                dp_vendorStartDate.SelectedDate = null;
                dp_vendorEndDate.SelectedDate = null;

                chk_allVendorsPaymentType.IsChecked = true;
                chk_allVendors.IsChecked = true;
                chk_allVendorsAccountant.IsChecked = true;

                vendorCombo = statisticModel.getUserAcc(payments, "u");
                fillSalaryCombo(vendorCombo, cb_vendors);

                payCombo = statisticModel.getPaymentsTypeComboBySide(payments, "u");
                fillPaymentsTypeCombo(cb_vendorPayType);

                accCombo = statisticModel.getAccounantCombo(payments, "u");
                fillAccCombo(accCombo, cb_vendorAccountant);

               
                fillEvents("u");

            

                // key_up search Person name
                cb_vendors.IsTextSearchEnabled = false;
                cb_vendors.IsEditable = true;
                cb_vendors.StaysOpenOnEdit = true;
                cb_vendors.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_vendors.Text = "";

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
        private void Btn_administrativeDeposit_Click(object sender, RoutedEventArgs e)
        {//administrative deposite
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendors, MainWindow.resourcemanager.GetString("trAdministrativeDeposit")+"...");
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                hideAllColumn();
                selectedTab = 3;
                //view columns
                col_tansNum.Visibility = Visibility.Visible;
                col_processType.Visibility = Visibility.Visible;
                col_updateUserAcc.Visibility = Visibility.Visible;
                //col_shipping.Visibility = Visibility.Visible;
                col_updateDate.Visibility = Visibility.Visible;
                col_cash.Visibility = Visibility.Visible;

                txt_search.Text = "";

                paint();
                bdr_administrativeDeposit.Background = Brushes.White;
                path_administrativeDeposit.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                isEnabledButtons();
                btn_administrativeDeposit.IsEnabled = false;
                btn_administrativeDeposit.Opacity = 1;
            
                cb_vendors.Visibility = Visibility.Collapsed;
                cb_vendors.SelectedItem = null;
                chk_allVendors.Visibility = Visibility.Collapsed;


                dp_vendorStartDate.SelectedDate = null;
                dp_vendorEndDate.SelectedDate = null;

                chk_allVendorsPaymentType.IsChecked = true;
                chk_allVendors.IsChecked = true;
                chk_allVendorsAccountant.IsChecked = true;

                /*
                var iulist = payments.Where(g => g.shippingCompanyId != null).GroupBy(g => g.shippingCompanyId).Select(g => new ShippingCombo { ShippingId = g.FirstOrDefault().shippingCompanyId, ShippingName = g.FirstOrDefault().shippingCompanyName }).ToList();
                cb_vendors.SelectedValuePath = "ShippingId";
                cb_vendors.DisplayMemberPath = "ShippingName";
                cb_vendors.ItemsSource = iulist;
                */
                payCombo = statisticModel.getPaymentsTypeComboBySide(payments, "m");
                fillPaymentsTypeCombo(cb_vendorPayType);

                accCombo = statisticModel.getAccounantCombo(payments, "m");
                fillAccCombo(accCombo, cb_vendorAccountant);

                fillEvents("m");

              

                // key_up search Person name
                cb_vendors.IsTextSearchEnabled = true;
                cb_vendors.IsEditable = false;
                cb_vendors.StaysOpenOnEdit = false;
                cb_vendors.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_vendors.Text = "";

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
        private async void Btn_shipping_Click(object sender, RoutedEventArgs e)
        {//shipping
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendors, MainWindow.resourcemanager.GetString("trShippingCompanyHint"));
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                hideAllColumn();
                selectedTab = 4;
                //view columns
                col_tansNum.Visibility = Visibility.Visible;
                col_processType.Visibility = Visibility.Visible;
                col_updateUserAcc.Visibility = Visibility.Visible;
                col_shipping.Visibility = Visibility.Visible;
                col_updateDate.Visibility = Visibility.Visible;
                col_cash.Visibility = Visibility.Visible;

                txt_search.Text = "";

                paint();
                bdr_shipping.Background = Brushes.White;
                path_shipping.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                isEnabledButtons();
                btn_shipping.IsEnabled = false;
                btn_shipping.Opacity = 1;

                cb_vendors.Visibility = Visibility.Visible;
                cb_vendors.SelectedItem = null;
                chk_allVendors.Visibility = Visibility.Visible;

                dp_vendorStartDate.SelectedDate = null;
                dp_vendorEndDate.SelectedDate = null;

                chk_allVendorsPaymentType.IsChecked = true;
                chk_allVendors.IsChecked = true;

                chk_allVendorsAccountant.IsChecked = true;
                shippingCombo = payments.Where(g => g.shippingCompanyId != null).GroupBy(g => g.shippingCompanyId).Select(g => new ShippingCombo { ShippingId = g.FirstOrDefault().shippingCompanyId, ShippingName = g.FirstOrDefault().shippingCompanyName }).ToList();
                cb_vendors.SelectedValuePath = "ShippingId";
                cb_vendors.DisplayMemberPath = "ShippingName";
                cb_vendors.ItemsSource = shippingCombo;


                payCombo = statisticModel.getPaymentsTypeComboBySide(payments, "sh");
                fillPaymentsTypeCombo(cb_vendorPayType);

                accCombo = statisticModel.getAccounantCombo(payments, "sh");
                fillAccCombo(accCombo, cb_vendorAccountant);

               
                fillEvents("sh");
                
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
        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            List<int> resultList = new List<int>();
            titles.Clear();

            var result = temp
                .GroupBy(s => new { s.processType })
                .Select(s => new CashTransferSts
                {
                    processTypeCount = s.Count(),
                    processType = s.FirstOrDefault().processType,
                });
            resultList = result.Select(m => m.processTypeCount).ToList();
            titles = result.Select(m => m.processType).ToList();
            for (int t = 0; t < titles.Count; t++)
            {
                string s = "";
                switch (titles[t])
                {
                    case "cash": s = MainWindow.resourcemanager.GetString("trCash"); break;
                    case "doc": s = MainWindow.resourcemanager.GetString("trDocument"); break;
                    case "cheque": s = MainWindow.resourcemanager.GetString("trCheque"); break;
                    case "balance": s = MainWindow.resourcemanager.GetString("trCredit"); break;
                    case "card": s = MainWindow.resourcemanager.GetString("trAnotherPaymentMethods"); break;
                    case "inv": s = MainWindow.resourcemanager.GetString("trInv"); break;
                }
                titles[t] = s;
            }
            SeriesCollection piechartData = new SeriesCollection();
            for (int i = 0; i < resultList.Count(); i++)
            {
                List<int> final = new List<int>();
                List<string> lable = new List<string>();

                final.Add(resultList.Skip(i).FirstOrDefault());
                lable = titles;
                piechartData.Add(
                  new PieSeries
                  {
                      Values = final.AsChartValues(),
                      Title = lable.Skip(i).FirstOrDefault(),
                      DataLabels = true,
                  }
              );

            }
            chart1.Series = piechartData;
        }
        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            List<CashTransferSts> resultList = new List<CashTransferSts>();

            #region group data by selected tab
            //agent
            if ((selectedTab == 0) || (selectedTab == 1))
            {
                var res = temp.Where(x => x.agentId != null).GroupBy(x => new { x.agentId, x.processType }).Select(x => new CashTransferSts
                {
                    processType = x.FirstOrDefault().processType,
                    agentId = x.FirstOrDefault().agentId,
                    agentName = x.FirstOrDefault().agentName,
                    cash = x.Sum(g => g.cash),

                });
                resultList = res.GroupBy(x => x.agentId).Select(x => new CashTransferSts
                {
                    processType = x.FirstOrDefault().processType,
                    cashTotal = x.Where(g => g.processType == "cash").Sum(g => (decimal)g.cash),
                    cardTotal = x.Where(g => g.processType == "card").Sum(g => (decimal)g.cash),
                    chequeTotal = x.Where(g => g.processType == "cheque").Sum(g => (decimal)g.cash),
                    docTotal = x.Where(g => g.processType == "doc").Sum(g => (decimal)g.cash),
                    balanceTotal = x.Where(g => g.processType == "balance").Sum(g => (decimal)g.cash),
                    invoiceTotal = x.Where(g => g.processType == "inv").Sum(g => (decimal)g.cash),
                    agentName = x.FirstOrDefault().agentName,
                    agentId = x.FirstOrDefault().agentId,
                }
                ).ToList();

                var tempName = res.GroupBy(s => new { s.agentId }).Select(s => new
                {
                    agentName = s.FirstOrDefault().agentName,
                });
                names.AddRange(tempName.Select(nn => nn.agentName));
            }
            //user
            if (selectedTab == 2) 
            {
                var res = temp.GroupBy(x => new { x.userId, x.processType }).Select(x => new CashTransferSts
                {
                    processType = x.FirstOrDefault().processType,
                    userId = x.FirstOrDefault().userId,
                    usersName = x.FirstOrDefault().userAcc,
                    cash = x.Sum(g => g.cash),

                });
                resultList = res.GroupBy(x => x.userId).Select(x => new CashTransferSts
                {
                    processType = x.FirstOrDefault().processType,
                    cashTotal = x.Where(g => g.processType == "cash").Sum(g => (decimal)g.cash),
                    cardTotal = x.Where(g => g.processType == "card").Sum(g => (decimal)g.cash),
                    chequeTotal = x.Where(g => g.processType == "cheque").Sum(g => (decimal)g.cash),
                    docTotal = x.Where(g => g.processType == "doc").Sum(g => (decimal)g.cash),
                    balanceTotal = x.Where(g => g.processType == "balance").Sum(g => (decimal)g.cash),
                    invoiceTotal = x.Where(g => g.processType == "inv").Sum(g => (decimal)g.cash),
                    usersName = x.FirstOrDefault().usersName,
                    userId = x.FirstOrDefault().userId,
                }
                ).ToList();

                var tempName = res.GroupBy(s => new { s.userId }).Select(s => new
                {
                    userName = s.FirstOrDefault().usersName,
                });
                names.AddRange(tempName.Select(nn => nn.userName));
            }
            //administrative deposite
            if (selectedTab == 3)
            {
                var res = temp;
                resultList = res.GroupBy(x => x.userId).Select(x => new CashTransferSts
                {
                    processType = x.FirstOrDefault().processType,
                    cashTotal = x.Where(g => g.processType == "cash").Sum(g => (decimal)g.cash),
                    cardTotal = x.Where(g => g.processType == "card").Sum(g => (decimal)g.cash),
                    chequeTotal = x.Where(g => g.processType == "cheque").Sum(g => (decimal)g.cash),
                    docTotal = x.Where(g => g.processType == "doc").Sum(g => (decimal)g.cash),
                    balanceTotal = x.Where(g => g.processType == "balance").Sum(g => (decimal)g.cash),
                    invoiceTotal = x.Where(g => g.processType == "inv").Sum(g => (decimal)g.cash),
                    usersName = x.FirstOrDefault().usersName,
                    userId = x.FirstOrDefault().userId,
                }
                ).ToList();
            }
            //shipping
            if (selectedTab == 4)
            {
                var res = temp.Where(x => x.shippingCompanyId != null).GroupBy(x => new { x.shippingCompanyId, x.processType }).Select(x => new CashTransferSts
                {
                    processType = x.FirstOrDefault().processType,
                    shippingCompanyId = x.FirstOrDefault().shippingCompanyId,
                    shippingCompanyName = x.FirstOrDefault().shippingCompanyName,
                    cash = x.Sum(g => g.cash),

                });
                resultList = res.GroupBy(x => x.shippingCompanyId).Select(x => new CashTransferSts
                {
                    processType = x.FirstOrDefault().processType,
                    cashTotal = x.Where(g => g.processType == "cash").Sum(g => (decimal)g.cash),
                    cardTotal = x.Where(g => g.processType == "card").Sum(g => (decimal)g.cash),
                    chequeTotal = x.Where(g => g.processType == "cheque").Sum(g => (decimal)g.cash),
                    docTotal = x.Where(g => g.processType == "doc").Sum(g => (decimal)g.cash),
                    balanceTotal = x.Where(g => g.processType == "balance").Sum(g => (decimal)g.cash),
                    invoiceTotal = x.Where(g => g.processType == "inv").Sum(g => (decimal)g.cash),
                    shippingCompanyName = x.FirstOrDefault().shippingCompanyName,
                    shippingCompanyId = x.FirstOrDefault().shippingCompanyId,
                }
                ).ToList();

                var tempName = res.GroupBy(s => new { s.shippingCompanyId }).Select(s => new
                {
                    shippingCompanyName = s.FirstOrDefault().shippingCompanyName,
                });
                names.AddRange(tempName.Select(nn => nn.shippingCompanyName));
            }
            #endregion

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> cash = new List<decimal>();
            List<decimal> card = new List<decimal>();
            List<decimal> doc = new List<decimal>();
            List<decimal> cheque = new List<decimal>();
            List<decimal> balance = new List<decimal>();
            List<decimal> invoice = new List<decimal>();

            int xCount = 6;
            if (resultList.Count() <= 6)
                xCount = resultList.Count();
            for (int i = 0; i < xCount; i++)
            {
                cash.Add(resultList.ToList().Skip(i).FirstOrDefault().cashTotal);
                card.Add(resultList.ToList().Skip(i).FirstOrDefault().cardTotal);
                doc.Add(resultList.ToList().Skip(i).FirstOrDefault().docTotal);
                cheque.Add(resultList.ToList().Skip(i).FirstOrDefault().chequeTotal);
                invoice.Add(resultList.ToList().Skip(i).FirstOrDefault().invoiceTotal);

                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (resultList.Count() > 6)
            {
                decimal cashSum = 0, cardSum = 0, docSum = 0, chequeSum = 0, balanceSum = 0, invoiceSum = 0;
                for (int i = 6; i < resultList.Count; i++)
                {
                    cashSum = cashSum + resultList.ToList().Skip(i).FirstOrDefault().cashTotal;
                    cardSum = cardSum + resultList.ToList().Skip(i).FirstOrDefault().cardTotal;
                    docSum = docSum + resultList.ToList().Skip(i).FirstOrDefault().docTotal;
                    chequeSum = chequeSum + resultList.ToList().Skip(i).FirstOrDefault().chequeTotal;
                    invoiceSum = invoiceSum + resultList.ToList().Skip(i).FirstOrDefault().invoiceTotal;
                }
                if (!((cashSum == 0) && (cardSum == 0) && (docSum == 0) && (chequeSum == 0) && (chequeSum == 0) && (balanceSum == 0) && (invoiceSum == 0)))
                {
                    cash.Add(cashSum);
                    card.Add(cardSum);
                    doc.Add(docSum);
                    cheque.Add(chequeSum);
                    invoice.Add(invoiceSum);

                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cash.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("trCash")
            });
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = card.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("trAnotherPaymentMethods")
            });
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = doc.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("trDocument")
            });
            columnChartData.Add(
         new StackedColumnSeries
         {
             Values = cheque.AsChartValues(),
             DataLabels = true,
             Title = MainWindow.resourcemanager.GetString("trCheque")
         });
            columnChartData.Add(
         new StackedColumnSeries
         {
             Values = invoice.AsChartValues(),
             DataLabels = true,
             Title = MainWindow.resourcemanager.GetString("trInv")
         });

            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillRowChart()
        {
            int endYear = DateTime.Now.Year;
            int startYear = endYear - 1;
            int startMonth = DateTime.Now.Month;
            int endMonth = startMonth;
            if (dp_vendorStartDate.SelectedDate != null && dp_vendorEndDate.SelectedDate != null)
            {
                startYear = dp_vendorStartDate.SelectedDate.Value.Year;
                endYear = dp_vendorEndDate.SelectedDate.Value.Year;
                startMonth = dp_vendorStartDate.SelectedDate.Value.Month;
                endMonth = dp_vendorEndDate.SelectedDate.Value.Month;
            }


            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            List<CashTransferSts> resultList = new List<CashTransferSts>();

            SeriesCollection rowChartData = new SeriesCollection();
            //agent
            if ((selectedTab == 0) || (selectedTab == 1))
            {
                var tempName = temp.GroupBy(s => new { s.agentId }).Select(s => new
                {
                    Name = s.FirstOrDefault().updateDate,
                });
                names.AddRange(tempName.Select(nn => nn.Name.ToString()));
            }//user
            else if (selectedTab == 2)
            {
                var tempName = temp.GroupBy(s => new { s.userId }).Select(s => new
                {
                    Name = s.FirstOrDefault().updateDate,
                });
                names.AddRange(tempName.Select(nn => nn.Name.ToString()));
            }
            else if (selectedTab == 3)
            {
                var tempName = temp.GroupBy(s => new { s.shippingCompanyId }).Select(s => new
                {
                    Name = s.FirstOrDefault().updateDate,
                });
                names.AddRange(tempName.Select(nn => nn.Name.ToString()));
            }
            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> cash = new List<decimal>();
            List<decimal> card = new List<decimal>();
            List<decimal> doc = new List<decimal>();
            List<decimal> cheque = new List<decimal>();
            List<decimal> invoice = new List<decimal>();

            if (endYear - startYear <= 1)
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    for (int month = startMonth; month <= 12; month++)
                    {
                        var firstOfThisMonth = new DateTime(year, month, 1);
                        var firstOfNextMonth = firstOfThisMonth.AddMonths(1);
                        var drawCash = temp.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.processType == "cash").Select(c => c.cash.Value).Sum();
                        var drawCard = temp.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.processType == "card").Select(c => c.cash.Value).Sum();
                        var drawDoc = temp.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.processType == "doc").Select(c => c.cash.Value).Sum();
                        var drawCheque = temp.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.processType == "cheque").Select(c => c.cash.Value).Sum();
                        var drawBalance = temp.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.processType == "balance").Select(c => c.cash.Value).Sum();
                        var drawInvoice = temp.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.processType == "inv").Select(c => c.cash.Value).Sum();

                        cash.Add(drawCash);
                        card.Add(drawCard);
                        doc.Add(drawDoc);
                        cheque.Add(drawCheque);
                        invoice.Add(drawInvoice);
                        MyAxis.Labels.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + "/" + year);

                        if (year == endYear && month == endMonth)
                        {
                            break;
                        }
                        if (month == 12)
                        {
                            startMonth = 1;
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    var firstOfThisYear = new DateTime(year, 1, 1);
                    var firstOfNextMYear = firstOfThisYear.AddYears(1);
                    var drawCash = temp.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.processType == "cash").Select(c => c.cash.Value).Sum();
                    var drawCard = temp.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.processType == "card").Select(c => c.cash.Value).Sum();
                    var drawDoc = temp.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.processType == "doc").Select(c => c.cash.Value).Sum();
                    var drawCheque = temp.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.processType == "cheque").Select(c => c.cash.Value).Sum();
                    var drawInvoice = temp.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.processType == "inv").Select(c => c.cash.Value).Sum();

                    cash.Add(drawCash);
                    card.Add(drawCard);
                    doc.Add(drawDoc);
                    cheque.Add(drawCheque);
                    invoice.Add(drawInvoice);
                    MyAxis.Labels.Add(year.ToString());
                }
            }
            rowChartData.Add(
          new LineSeries
          {
              Values = cash.AsChartValues(),
              Title = MainWindow.resourcemanager.GetString("trCash")
          }); ;
            rowChartData.Add(
         new LineSeries
         {
             Values = card.AsChartValues(),
             Title = MainWindow.resourcemanager.GetString("trAnotherPaymentMethods")
         });
            rowChartData.Add(
        new LineSeries
        {
            Values = doc.AsChartValues(),
            Title = MainWindow.resourcemanager.GetString("trDocument")

        });
            rowChartData.Add(
            new LineSeries
            {
                Values = cheque.AsChartValues(),
                Title = MainWindow.resourcemanager.GetString("trCheque")

            });
            rowChartData.Add(
            new LineSeries
            {
                Values = invoice.AsChartValues(),
                Title = MainWindow.resourcemanager.GetString("trInv")

            });
            DataContext = this;
            rowChart.Series = rowChartData;
        }
        #endregion

        #region events
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                payments = await statisticModel.GetReceipt();
                txt_search.Text = "";

                cb_vendors.SelectedItem = null;
                cb_vendorPayType.SelectedItem = null;
                cb_vendorAccountant.SelectedItem = null;
                chk_allVendors.IsChecked = true;
                chk_allVendorsAccountant.IsChecked = true;
                chk_allVendorsPaymentType.IsChecked = true;
                dp_vendorEndDate.SelectedDate = null;
                dp_vendorStartDate.SelectedDate = null;
                chk_allVendorsAccountant.IsChecked = true;
                fillBySide();
                //if (selectedTab == 0)
                //{
                //    fillEvents("v");
                //}
                //else if (selectedTab == 1)
                //{
                //    fillEvents("c");
                //}
                //else if (selectedTab == 2)
                //{
                //    fillEvents("u");
                //}
                //else if (selectedTab == 3)
                //{
                //    fillEvents("m");
                //}
                //else if (selectedTab == 4)
                //{
                //    fillEvents("sh");
                //}

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
        private void Txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (selectedTab == 0)
                {
                    temp = temp.Where(obj => (
                    obj.side == "v" 
                    &&
                    (obj.transNum.ToLower().Contains(txt_search.Text.ToLower()) ||
                    obj.processType.ToLower().Contains(txt_search.Text.ToLower()) ||
                    obj.updateUserAcc.ToLower().Contains(txt_search.Text.ToLower()) ||
                    obj.agentCompany.ToLower().Contains(txt_search.Text.ToLower()) ||
                    obj.agentName.ToLower().Contains(txt_search.Text.ToLower())
                    )));
                }
                if (selectedTab == 1)
                {
                    temp = temp.Where(obj => (
                    obj.side == "c"
                    &&
                    (obj.transNum.ToLower().Contains(txt_search.Text.ToLower()) ||
                    obj.processType.ToLower().Contains(txt_search.Text.ToLower()) ||
                    obj.updateUserAcc.ToLower().Contains(txt_search.Text.ToLower()) 
                    //||
                    //(obj.agentCompany == null ? obj.agentCompany.Contains(txt_search.Text) : true)
                    //||
                    //obj.agentName.Contains(txt_search.Text)
                    )));
            }
                else if (selectedTab == 2)
                {
                    temp = temp.Where(obj => (
                        obj.side == "u"
                        &&
                        (
                        obj.transNum.ToLower().Contains(txt_search.Text.ToLower()) ||
                        obj.processType.ToLower().Contains(txt_search.Text.ToLower()) ||
                        obj.updateUserAcc.ToLower().Contains(txt_search.Text.ToLower()) ||
                        obj.userAcc.ToLower().Contains(txt_search.Text.ToLower())
                        )));
                }
                else if (selectedTab == 3)
                {
                    temp = temp.Where(obj => (
                     obj.side == "m"
                        &&
                        (
                       obj.transNum.ToLower().Contains(txt_search.Text.ToLower()) ||
                       obj.processType.ToLower().Contains(txt_search.Text.ToLower()) ||
                       obj.updateUserAcc.ToLower().Contains(txt_search.Text.ToLower()) ||
                       obj.shippingCompanyName.ToLower().Contains(txt_search.Text.ToLower())
                       )));
                }
                else if (selectedTab == 4)
                {
                    temp = temp.Where(obj => (
                     obj.side == "sh"
                        &&
                        (
                       obj.transNum.ToLower().Contains(txt_search.Text.ToLower()) ||
                       obj.processType.ToLower().Contains(txt_search.Text.ToLower()) ||
                       obj.updateUserAcc.ToLower().Contains(txt_search.Text.ToLower()) ||
                       obj.shippingCompanyName.ToLower().Contains(txt_search.Text.ToLower())
                       )));
                }
                dgPayments.ItemsSource = temp;
                txt_count.Text = dgPayments.Items.Count.ToString();

                decimal total = 0;
                total = temp.Select(b => b.cash.Value).Sum();
                tb_total.Text = SectionData.DecTostring(total);

                fillColumnChart();
                fillPieChart();
                fillRowChart();

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
        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillbyComboValue();

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
        private void Dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillBySide();

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
        private void Cb_vendors_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (selectedTab == 0)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                    tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                    combo.ItemsSource = vendors.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
                }
                else if (selectedTab == 1)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                    tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                    combo.ItemsSource = vendors.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();//???
                }
                else if (selectedTab == 2)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                    combo.ItemsSource = vendorCombo.Where(p => p.UserAcc.ToLower().Contains(tb.Text.ToLower()) || (p.UserMobile != null && p.UserMobile.Contains(tb.Text))).ToList();
                }
                else if (selectedTab == 4)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                    combo.ItemsSource = shippingCombo.Where(p => p.ShippingName.ToLower().Contains(tb.Text.ToLower()) || (p.ShippingMobile != null && p.ShippingMobile.Contains(tb.Text))).ToList();
                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_vendorAccountant_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = accCombo.Where(p => p.Accountant.ToLower().Contains(tb.Text.ToLower()) || (p.AccountMobile != null && p.AccountMobile.Contains(tb.Text))).ToList();

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Chk_allVendors_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_vendors.SelectedItem = null;
                cb_vendors.IsEnabled = false;
                fillbyComboValue();
                cb_vendors.Text = "";
                if (selectedTab == 0)
                    cb_vendors.ItemsSource = vendorCombo;
                else if (selectedTab == 1)
                    cb_vendors.ItemsSource = vendorCombo;
                else if (selectedTab == 2)
                    cb_vendors.ItemsSource = vendorCombo;
                else if (selectedTab == 4)
                    cb_vendors.ItemsSource = shippingCombo;
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
        private void Chk_allVendors_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_vendors.IsEnabled = true;
                fillEmptyBySide();
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
        private void Chk_allVendorsPaymentType_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_vendorPayType.SelectedItem = null;
                cb_vendorPayType.IsEnabled = false;
                fillbyComboValue();
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
        private void Chk_allVendorsPaymentType_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_vendorPayType.IsEnabled = true;
                fillEmptyBySide();
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
        private void Chk_allVendorsAccountant_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_vendorAccountant.SelectedItem = null;
                cb_vendorAccountant.IsEnabled = false;
                cb_vendorAccountant.Text = "";
                cb_vendorAccountant.ItemsSource = accCombo;

                fillbyComboValue();
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
        private void Chk_allVendorsAccountant_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_vendorAccountant.IsEnabled = true;
                fillEmptyBySide();
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

        #region datagrid buttons
        private void previewRowinDatagrid(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        CashTransferSts itemrowsts = (CashTransferSts)dgPayments.SelectedItems[0];
                        clsReports clsr = new clsReports();

                        CashTransfer itemrow = clsr.JsonCashTransfer(itemrowsts);
                        string pdfpath = "";

                        pdfpath = @"\Thumb\report\temp.pdf";
                        pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                        BuildvoucherReport(itemrow);
                        LocalReportExtensions.ExportToPDF(rep, pdfpath);
                        wd_previewPdf w = new wd_previewPdf();
                        w.pdfPath = pdfpath;
                        if (!string.IsNullOrEmpty(w.pdfPath))
                        {
                            w.ShowDialog();
                            w.wb_pdfWebViewer.Dispose();
                        }
                        Window.GetWindow(this).Opacity = 1;

                    }

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

        private void pdfRowinDatagrid(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        CashTransferSts itemrowsts = (CashTransferSts)dgPayments.SelectedItems[0];
                        clsReports clsr = new clsReports();

                        CashTransfer itemrow = clsr.JsonCashTransfer(itemrowsts);
                        BuildvoucherReport(itemrow);

                        saveFileDialog.Filter = "PDF|*.pdf;";

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            string filepath = saveFileDialog.FileName;
                            LocalReportExtensions.ExportToPDF(rep, filepath);
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

        private void printRowinDatagrid(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        CashTransferSts itemrowsts = (CashTransferSts)dgPayments.SelectedItems[0];
                        clsReports clsr = new clsReports();

                        CashTransfer itemrow = clsr.JsonCashTransfer(itemrowsts);
                        BuildvoucherReport(itemrow);
                        if (MainWindow.docPapersize == "A4")
                        {
                            LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

                        }
                        else //A5
                        {
                            LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name,2);

                        }
                      //  LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

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

        public void BuildvoucherReport(CashTransfer cashtrans)
        {
            string addpath;
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {

                if (MainWindow.docPapersize == "A4")
                {
                    addpath = @"\Reports\Account\Ar\ArReciveReportA4.rdlc";
                }
                else
                {
                    addpath = @"\Reports\Account\Ar\ArReciveReport.rdlc";
                }
            }
            else
            {
                if (MainWindow.docPapersize == "A4")
                {
                    addpath = @"\Reports\Account\En\ReciveReportA4.rdlc";
                }
                else
                {
                    addpath = @"\Reports\Account\En\ReciveReport.rdlc";
                }


            }

            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            rep.ReportPath = reppath;
            rep.DataSources.Clear();
            rep.EnableExternalImages = true;
            rep.SetParameters(reportclass.fillPayReport(cashtrans));

            rep.Refresh();
        }
        #endregion

        #region reports
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        public void BuildReport()
        {

            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            string firstTitle = "recipientReport";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string startDate = "";
            string endDate = "";
            string vendorval = "";
            string Accountantval = "";
            string paymentval = "";
            //  string cardval = "";
            string searchval = "";

            string trVendor = "";
            List<string> invTypelist = new List<string>();
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
          
            if (isArabic)
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\Ar\ArVendor.rdlc";
                    secondTitle = "vendors";
                    trVendor = MainWindow.resourcemanagerreport.GetString("trVendor");
                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\Ar\ArCustomer.rdlc";
                    secondTitle = "customers";
                    trVendor = MainWindow.resourcemanagerreport.GetString("trCustomer");
                }
                else if (selectedTab == 2)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\Ar\ArUser.rdlc";
                    secondTitle = "users";
                    trVendor = MainWindow.resourcemanagerreport.GetString("trUser");
                }
                else if (selectedTab == 3)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\Ar\ArAdministrativeDeposit.rdlc";
                    secondTitle = "AdministrativeDeposit";

                }
                else if (selectedTab == 4)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\Ar\ArShipping.rdlc";
                    secondTitle = "shipping";
                    trVendor = MainWindow.resourcemanagerreport.GetString("trShippingCompanynohint");
                }
            }
            else
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\En\Vendor.rdlc";
                    secondTitle = "vendors";
                    trVendor = MainWindow.resourcemanagerreport.GetString("trVendor");
                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\En\Customer.rdlc";
                    secondTitle = "customers";
                    trVendor = MainWindow.resourcemanagerreport.GetString("trCustomer");
                }
                else if (selectedTab == 2)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\En\User.rdlc";
                    secondTitle = "users";
                    trVendor = MainWindow.resourcemanagerreport.GetString("trUser");
                }
                else if (selectedTab == 3)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\En\AdministrativeDeposit.rdlc";
                    secondTitle = "AdministrativeDeposit";
                }
                else if (selectedTab == 4)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\En\Shipping.rdlc";
                    secondTitle = "shipping";
                    trVendor = MainWindow.resourcemanagerreport.GetString("trShippingCompanynohint");
                }
            }
            //filter
            startDate = dp_vendorStartDate.SelectedDate != null ? SectionData.DateToString(dp_vendorStartDate.SelectedDate) : "";

            endDate = dp_vendorEndDate.SelectedDate != null ? SectionData.DateToString(dp_vendorEndDate.SelectedDate) : "";
            //startTime = dt_startTime.SelectedTime != null ? dt_startTime.Text : "";
            //endTime = dt_endTime.SelectedTime != null ? dt_endTime.Text : "";
            vendorval = cb_vendors.SelectedItem != null
               && (chk_allVendors.IsChecked == false || chk_allVendors.IsChecked == null)
               ? cb_vendors.Text : (chk_allVendors.IsChecked == true ? all : "");

            paymentval = cb_vendorPayType.SelectedItem != null
               && (chk_allVendorsPaymentType.IsChecked == false || chk_allVendorsPaymentType.IsChecked == null)
               && vendorval != ""
               ? clsReports.PaymentComboConvert(cb_vendorPayType.SelectedValue.ToString()) : (chk_allVendorsPaymentType.IsChecked == true && vendorval != "" ? all : "");

            Accountantval = cb_vendorAccountant.SelectedItem != null
            && (chk_allVendorsAccountant.IsChecked == false || chk_allVendorsAccountant.IsChecked == null)
            && vendorval != ""
            ? cb_vendorAccountant.Text : (chk_allVendorsAccountant.IsChecked == true && vendorval != "" ? all : "");
            paramarr.Add(new ReportParameter("trVendor", trVendor));
            paramarr.Add(new ReportParameter("paymentval", paymentval));
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("VendorVal", vendorval));
            paramarr.Add(new ReportParameter("AccountantVal", Accountantval));
            paramarr.Add(new ReportParameter("trPaymentType", MainWindow.resourcemanagerreport.GetString("trPaymentType")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            paramarr.Add(new ReportParameter("trAccoutant", MainWindow.resourcemanagerreport.GetString("trAccoutant")));

            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter

            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trAccounting") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            paramarr.Add(new ReportParameter("totalValue", tb_total.Text));
            clsReports.cashTransferStsRecipient(temp, rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
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
                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

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
                //    Thread t1 = new Thread(() =>
                //{
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
                //    t1.Start();

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
