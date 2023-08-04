using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using Microsoft.Reporting.WinForms;
using netoaster;
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
using Microsoft.Win32;
using System.Threading;

namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_accountStatement.xaml
    /// </summary>
    public partial class uc_accountStatement : UserControl
    {
        #region variables
        Statistics statisticModel = new Statistics();

        List<CashTransferSts> statement;

        IEnumerable<VendorCombo> vendorCombo;
        IEnumerable<VendorCombo> customerCombo;
        IEnumerable<VendorCombo> userCombo;
        IEnumerable<ShippingCombo> ShippingCombo;

        //report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        public static string repTrRequires = "";

        int selectedTab = 0;
        #endregion
        public uc_accountStatement()
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

         private static uc_accountStatement _instance;
        public static uc_accountStatement Instance
        {
            get
            {
                if (_instance == null) _instance = new uc_accountStatement();
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

                statement = await statisticModel.GetStatement();
                //statement = statement.Where(s => s.processType != "inv").ToList();

                // key_up search Person name
                cb_vendors.IsTextSearchEnabled = false;
                cb_vendors.IsEditable = true;
                cb_vendors.StaysOpenOnEdit = true;
                cb_vendors.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_vendors.Text = "";

                Btn_vendor_Click(btn_vendor, null);

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
            tt_shipping.Content = MainWindow.resourcemanager.GetString("trShippingCompanies");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendors, MainWindow.resourcemanager.GetString("trVendorHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendorsDate, MainWindow.resourcemanager.GetString("trArchive"));
            //chk_allVendors.Content = MainWindow.resourcemanager.GetString("trAll");

            dgPayments.Columns[0].Header = MainWindow.resourcemanager.GetString("trNo");
            dgPayments.Columns[1].Header = MainWindow.resourcemanager.GetString("trDate");
            dgPayments.Columns[2].Header = MainWindow.resourcemanager.GetString("trDescription");
            dgPayments.Columns[3].Header = MainWindow.resourcemanager.GetString("trPayment");
            dgPayments.Columns[4].Header = MainWindow.resourcemanager.GetString("trCashTooltip");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_settings.Content = MainWindow.resourcemanager.GetString("trSettings");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_emailMessage.Content = MainWindow.resourcemanager.GetString("trSendEmail");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print3.Content = MainWindow.resourcemanager.GetString("trPrint");
        }
        private void fillVendorCombo(IEnumerable<VendorCombo> list, ComboBox cb)
        {
            cb.SelectedValuePath = "VendorId";
            cb.DisplayMemberPath = "VendorName";
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
        private void fillDateCombo(ComboBox cb)
        {
            cb.Items.Clear();
            if (statement.Count() > 0)
            {
                int firstYear = statement.Min(obj => obj.updateDate.Value.Year);
                int presentYear = DateTime.Now.Year;
                for (int i = firstYear; i <= presentYear; i++)
                {
                    cb.Items.Add(firstYear);
                    firstYear++;
                }
                cb.SelectedItem = presentYear;

            }
        }
        private List<CashTransferSts> fillList(List<CashTransferSts> payments, ComboBox vendor, ComboBox date)
        {
            var selectedItem1 = vendor.SelectedItem as VendorCombo;
            var selectedItem2 = vendor.SelectedItem as ShippingCombo;
            var selectedItem3 = date.SelectedItem;


            var result = payments.Where(x => (
                      (vendor.SelectedItem != null ? x.agentId == selectedItem1.VendorId : false)
                   && (date.SelectedItem != null ? x.updateDate.Value.Year == (int)selectedItem3 : true)));

            if (selectedTab == 2)
            {
                result = payments.Where(x => (
                         (vendor.SelectedItem != null ? x.userId == selectedItem1.UserId : false)
                      && (date.SelectedItem != null ? x.updateDate.Value.Year == (int)selectedItem3 : true)));
            }

            if (selectedTab == 3)
            {
                result = payments.Where(x => (
                                (vendor.SelectedItem != null ? x.invShippingCompanyId == selectedItem2.ShippingId : false)
                             && (date.SelectedItem != null ? x.updateDate.Value.Year == (int)selectedItem3 : true)));
            }

            return result.ToList();
        }
        public void paint()
        {
            //bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);

            bdr_customer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_vendor.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_user.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_shipping.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_customer.Fill = Brushes.White;
            path_vendor.Fill = Brushes.White;
            path_user.Fill = Brushes.White;
            path_shipping.Fill = Brushes.White;

        }
        private void hideAllColumn()
        {
            col_date.Visibility = Visibility.Hidden;
            col_amount.Visibility = Visibility.Hidden;
            col_proccesType.Visibility = Visibility.Hidden;
        }
        string selectedItem = "";
        private void FillbyComboValue()
        {
            if (cb_vendors.SelectedItem == null)
            {
                //chk_allVendors.IsEnabled = false;
                cb_vendorsDate.IsEnabled = false;
                cb_vendorsDate.SelectedItem = null;

                fillEmptyEvents();
            }
            else
            {
                //chk_allVendors.IsEnabled = true;
                cb_vendorsDate.IsEnabled = true;
                fillEvents();
            }
        }
        private void fillEmptyEvents()
        {
            temp = new List<CashTransferSts>();

            dgPayments.ItemsSource = temp;
            txt_count.Text = temp.Count().ToString();
            //decimal cashTotal = temp.Where(x => x.side != "shd").Select(x => x.cashTotal).LastOrDefault();
            decimal cashTotal = temp.Select(x => x.cashTotal).LastOrDefault();

            //bool worthy = false;
            //if (cashTotal >= 0) worthy = true;
            //if(selectedItem.Equals(MainWindow.resourcemanager.GetString("trCashCustomer")))
            //    worthy = true;
            if (cashTotal >= 0)
            //if (worthy)
            {
                txt_total.Text = SectionData.DecTostring(cashTotal);
                txt_for.Text = MainWindow.resourcemanager.GetString("trWorthy");

                repTrRequires = "trWorthy";
                tb_moneyIcon.Text = AppSettings.Currency;

                bdr_email.Visibility = Visibility.Collapsed;
            }
            else
            {
                cashTotal = -cashTotal;
                txt_total.Text = SectionData.DecTostring(cashTotal);
                txt_for.Text = MainWindow.resourcemanager.GetString("trRequired");

                repTrRequires = "trRequired";
                tb_moneyIcon.Text = AppSettings.Currency;
                //if (cb_vendors.SelectedItem != null)
                //{
                bdr_email.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //bdr_email.Visibility = Visibility.Collapsed;
                //}
            }
            fillRowChart();
            fillColumnChart();
            fillPieChart();
        }
        IEnumerable<CashTransferSts> temp = null;
        private void fillEvents()
        {
            temp = statisticModel.getstate(fillList(statement, cb_vendors, cb_vendorsDate), selectedTab, statement);

            dgPayments.ItemsSource = temp;
            txt_count.Text = temp.Count().ToString();
            //decimal cashTotal = temp.Where(x => x.side != "shd").Select(x => x.cashTotal).LastOrDefault();
            decimal cashTotal = temp.Select(x => x.cashTotal).LastOrDefault();

            //bool worthy = false;
            //if (cashTotal >= 0) worthy = true;
            //if(selectedItem.Equals(MainWindow.resourcemanager.GetString("trCashCustomer")))
            //    worthy = true;
            if (cashTotal >= 0)
            //if (worthy)
            {
                txt_total.Text = SectionData.DecTostring(cashTotal);
                txt_for.Text = MainWindow.resourcemanager.GetString("trWorthy");

                repTrRequires = "trWorthy";
                tb_moneyIcon.Text = AppSettings.Currency;

                bdr_email.Visibility = Visibility.Collapsed;
            }
            else
            {
                cashTotal = -cashTotal;
                txt_total.Text = SectionData.DecTostring(cashTotal);
                txt_for.Text = MainWindow.resourcemanager.GetString("trRequired");

                repTrRequires = "trRequired";
                tb_moneyIcon.Text = AppSettings.Currency;
                //if (cb_vendors.SelectedItem != null)
                //{
                bdr_email.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //bdr_email.Visibility = Visibility.Collapsed;
                //}
            }
            fillRowChart();
            fillColumnChart();
            fillPieChart();
        }

        #endregion

        #region events
        private void Cb_vendors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

               

                selectedItem = cb_vendors.SelectedItem.ToString();
                cb_vendorsDate.SelectedItem = DateTime.Now.Year;
                FillbyComboValue();

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
        private void Cb_vendorsDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_vendorsDate.SelectedItem != null && cb_vendorsDate.SelectedItem.ToString() == DateTime.Now.Year.ToString())
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendorsDate, MainWindow.resourcemanager.GetString("current"));
                else
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendorsDate, MainWindow.resourcemanager.GetString("trArchive"));

                FillbyComboValue();

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
        private void Chk_allVendors_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                  cb_vendorsDate.SelectedItem = null;
                cb_vendorsDate.IsEnabled = false;
               // cb_vendors.Text = "";

                if (selectedTab == 0)
                    cb_vendors.ItemsSource = vendorCombo;
                else if (selectedTab == 1)
                    cb_vendors.ItemsSource = customerCombo;
                if (selectedTab == 2)
                    cb_vendors.ItemsSource = userCombo;
                else if (selectedTab == 3)
                    cb_vendors.ItemsSource = ShippingCombo;

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
        private void Chk_allVendors_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_vendorsDate.IsEnabled = true;
                cb_vendorsDate.SelectedItem = null;
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
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                statement = await statisticModel.GetStatement();
                txt_search.Text = "";

                cb_vendors.SelectedItem = null;
                cb_vendorsDate.SelectedItem = null;
                //chk_allVendors.IsChecked = false;
                //chk_allVendors.IsEnabled = false;

                FillbyComboValue();

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

                temp = statisticModel.getstate(fillList(statement, cb_vendors, cb_vendorsDate), selectedTab, statement);

                temp = temp.Where(obj => obj.transNum.ToLower().Contains(txt_search.Text.ToLower()) ||
                obj.Description.ToLower().Contains(txt_search.Text.ToLower()) ||
                obj.Description1.ToLower().Contains(txt_search.Text.ToLower())).ToList();

                dgPayments.ItemsSource = temp;
                txt_count.Text = temp.Count().ToString();
                decimal cashTotal = temp.Select(x => x.cashTotal).LastOrDefault();

                if (cashTotal >= 0)
                {
                    txt_total.Text = SectionData.DecTostring(cashTotal);
                    txt_for.Text = MainWindow.resourcemanager.GetString("trWorthy");

                    repTrRequires = "trWorthy";
                    tb_moneyIcon.Text = AppSettings.Currency;

                    bdr_email.Visibility = Visibility.Collapsed;
                }
                else
                {
                    cashTotal = -cashTotal;
                    txt_total.Text = SectionData.DecTostring(cashTotal);
                    txt_for.Text = MainWindow.resourcemanager.GetString("trRequired");

                    repTrRequires = "trRequired";
                    tb_moneyIcon.Text = AppSettings.Currency;
                    bdr_email.Visibility = Visibility.Visible;
                }
                fillRowChart();
                fillColumnChart();
                fillPieChart();

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
                    combo.ItemsSource = vendorCombo.Where(p => p.VendorName.ToLower().Contains(tb.Text.ToLower()) || (p.AgentMobile != null && p.AgentMobile.Contains(tb.Text))).ToList();
                }
                else if (selectedTab == 1)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                    combo.ItemsSource = customerCombo.Where(p => p.VendorName.ToLower().Contains(tb.Text.ToLower()) || (p.AgentMobile != null && p.AgentMobile.Contains(tb.Text))).ToList();
                }
                if (selectedTab == 2)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                    combo.ItemsSource = userCombo.Where(p => p.UserAcc.ToLower().Contains(tb.Text.ToLower()) || (p.UserMobile != null && p.UserMobile.Contains(tb.Text))).ToList();
                }
                else if (selectedTab == 3)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                    combo.ItemsSource = ShippingCombo.Where(p => p.ShippingName.ToLower().Contains(tb.Text.ToLower()) || (p.ShippingMobile != null && p.ShippingMobile.Contains(tb.Text))).ToList();
                }

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
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

                cb_vendors.SelectedItem = null;
                cb_vendors.Text = "";
                selectedTab = 0;

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_vendor);
                path_vendor.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                //fillEvents();

                hideAllColumn();
                //show columns
                col_date.Visibility = Visibility.Visible;
                col_amount.Visibility = Visibility.Visible;
                col_proccesType.Visibility = Visibility.Visible;

                //chk_allVendors.IsChecked = true;
                fillDateCombo(cb_vendorsDate);
                vendorCombo = statisticModel.getVendorCombo(statement, "v").Where(x => x.VendorId != null);
                fillVendorCombo(vendorCombo, cb_vendors);

                FillbyComboValue();


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

                cb_vendors.SelectedItem = null;
                cb_vendors.Text = "";
                selectedTab = 1;

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_customer);
                path_customer.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                // fillEvents();

                hideAllColumn();
                //show columns
                col_date.Visibility = Visibility.Visible;
                col_amount.Visibility = Visibility.Visible;
                col_proccesType.Visibility = Visibility.Visible;

                //chk_allVendors.IsChecked = true;
                fillDateCombo(cb_vendorsDate);
                //customerCombo = statisticModel.getCustomerForStatementCombo(statement, "c");
                customerCombo = statisticModel.getVendorCombo(statement, "c").Where(x => x.VendorId != null);
                fillVendorCombo(customerCombo, cb_vendors);

                FillbyComboValue();
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

                cb_vendors.SelectedItem = null;
                cb_vendors.Text = "";
                selectedTab = 2;

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_user);
                path_user.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                //  fillEvents();

                hideAllColumn();
                //show columns
                col_date.Visibility = Visibility.Visible;
                col_amount.Visibility = Visibility.Visible;
                col_proccesType.Visibility = Visibility.Visible;

                //chk_allVendors.IsChecked = true;
                fillDateCombo(cb_vendorsDate);
                userCombo = statisticModel.getUserAcc(statement, "u");
                fillSalaryCombo(userCombo, cb_vendors);

                FillbyComboValue();

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
        private void Btn_shipping_Click(object sender, RoutedEventArgs e)
        {//shippings
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendors, MainWindow.resourcemanager.GetString("trShippingCompanyHint"));
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                cb_vendors.SelectedItem = null;
                cb_vendors.Text = "";
                selectedTab = 3;

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_shipping);
                path_shipping.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                //  fillEvents();

                hideAllColumn();
                //show columns
                col_date.Visibility = Visibility.Visible;
                col_amount.Visibility = Visibility.Visible;
                col_proccesType.Visibility = Visibility.Visible;

                //chk_allVendors.IsChecked = true;
                fillDateCombo(cb_vendorsDate);
                ShippingCombo = statisticModel.getShippingForStatementCombo(statement);
                //ShippingCombo = statisticModel.getShippingCombo(statement);
                fillShippingCombo(ShippingCombo, cb_vendors);

                FillbyComboValue();

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
        private void fillRowChart()
        {
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>(12);
            List<CashTransferSts> resultList = new List<CashTransferSts>();
            int year = DateTime.Now.Year;
            if (cb_vendorsDate.SelectedItem != null)
            {
                year = (int)cb_vendorsDate.SelectedItem;
            }

            SeriesCollection rowChartData = new SeriesCollection();

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> cash = new List<decimal>();

            LineSeries l = new LineSeries();
            for (int month = 1; month <= 12; month++)
            {
                var firstOfThisMonth = new DateTime(year, month, 1);
                var firstOfNextMonth = firstOfThisMonth.AddMonths(1);
                var drawCash = temp.Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth).Select(x => x.cashTotal).LastOrDefault();

                if (drawCash > 0)
                {
                    names.Add(MainWindow.resourcemanager.GetString("trWorthy"));

                    btn_emailMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    names.Add(MainWindow.resourcemanager.GetString("trRequired"));

                    btn_emailMessage.Visibility = Visibility.Visible;
                }
                cash.Add(drawCash);


                MyAxis.Labels.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + "/" + year);
            }
            //l.Values = cash.AsChartValues();
            //rowChartData.Add(l);
            rowChartData.Add(
           new LineSeries
           {
               Values = cash.AsChartValues(),
               Title = MainWindow.resourcemanager.GetString("trCashTooltip")

           });
            DataContext = this;
            rowChart.Series = rowChartData;
        }
        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> cash = new List<decimal>();
            List<decimal> card = new List<decimal>();
            List<decimal> doc = new List<decimal>();
            List<decimal> cheque = new List<decimal>();
            List<decimal> inv = new List<decimal>();

            cash.Add(temp.Where(x => x.processType == "cash").Select(x => x.cash.Value).Sum());
            card.Add(temp.Where(x => x.processType == "card").Select(x => x.cash.Value).Sum());
            doc.Add(temp.Where(x => x.processType == "doc").Select(x => x.cash.Value).Sum());
            cheque.Add(temp.Where(x => x.processType == "cheque").Select(x => x.cash.Value).Sum());
            inv.Add(temp.Where(x => x.processType == "inv").Select(x => x.cash.Value).Sum());

            columnChartData.Add(
            new ColumnSeries
            {
                Values = cash.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("trCash")
            });
            columnChartData.Add(
           new ColumnSeries
           {
               Values = card.AsChartValues(),
               DataLabels = true,
               Title = MainWindow.resourcemanager.GetString("trAnotherPaymentMethods")
           });
            columnChartData.Add(
           new ColumnSeries
           {
               Values = doc.AsChartValues(),
               DataLabels = true,
               Title = MainWindow.resourcemanager.GetString("trDocument")

           });
            columnChartData.Add(
           new ColumnSeries
           {
               Values = cheque.AsChartValues(),
               DataLabels = true,
               Title = MainWindow.resourcemanager.GetString("trCheque")
           });

            //columnChartData.Add(
            //new ColumnSeries
            // {
            //     Values = inv.AsChartValues(),
            //     DataLabels = true,
            //     Title = MainWindow.resourcemanager.GetString("tr_Invoice")
            // });

            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            List<int> resultList = new List<int>();
            titles.Clear();

            resultList.Add(temp.Where(x => x.processType != "inv" && x.transType == "p").Count());
            resultList.Add(temp.Where(x => x.processType != "inv" && x.transType == "d").Count());
            //resultList.Add(temp.Where(x => x.processType == "inv").Count());
            SeriesCollection piechartData = new SeriesCollection();
            for (int i = 0; i < resultList.Count(); i++)
            {
                List<int> final = new List<int>();
                List<string> lable = new List<string>()
                {
                    MainWindow.resourcemanager.GetString("trOnePayment"),
                    MainWindow.resourcemanager.GetString("trOneDeposit"),
                    //MainWindow.resourcemanager.GetString("tr_Invoice")
                };
                final.Add(resultList.Skip(i).FirstOrDefault());
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
        #endregion

        #region reports
        private void Btn_emailMessage_Click(object sender, RoutedEventArgs e)
        {//email
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //Thread t1 = new Thread(() =>
                //{
                sendEmail();
                //});
                //t1.Start();


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
        private async void sendEmail()
        {
            //if (txt_for.Text == "Required")
            if (txt_for.Text == MainWindow.resourcemanager.GetString("trRequired"))
            {
                string total = txt_total.Text;
                SysEmails email = new SysEmails();
                EmailClass mailtosend = new EmailClass();

                Agent toAgent = new Agent();
                User toUser = new User();
                ShippingCompanies toShipCompanies = new ShippingCompanies();
                string emailto = "";
                bool toemailexist = false;
                email = await email.GetByBranchIdandSide((int)MainWindow.branchID, "accounting");
                switch (selectedTab)
                {
                    case 0:
                        //vendor
                        var objct0 = cb_vendors.SelectedItem as VendorCombo;

                        int agentId = (int)objct0.VendorId;
                        toAgent = await toAgent.getAgentById(agentId);
                        emailto = toAgent.email;

                        if (emailto is null || emailto == "")
                        {
                            toemailexist = false;

                            this.Dispatcher.Invoke(() =>
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trTheVendorHasNoEmail"), animation: ToasterAnimation.FadeIn);
                            });
                        }
                        else
                        {
                            toemailexist = true;
                        }

                        break;

                    case 1:
                        var objct1 = cb_vendors.SelectedItem as VendorCombo;
                        agentId = (int)objct1.VendorId;
                        toAgent = await toAgent.getAgentById(agentId);
                        emailto = toAgent.email;

                        if (emailto is null || emailto == "")
                        {
                            toemailexist = false;
                            this.Dispatcher.Invoke(() =>
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trTheCustomerHasNoEmail"), animation: ToasterAnimation.FadeIn);
                            });
                        }
                        else
                        {
                            toemailexist = true;
                        }
                        break;
                    case 2:
                        var objct2 = cb_vendors.SelectedItem as VendorCombo;
                        int userId = (int)objct2.UserId;
                        toUser = await toUser.getUserById(userId);
                        emailto = toUser.email;

                        if (emailto is null || emailto == "")
                        {
                            toemailexist = false;
                            this.Dispatcher.Invoke(() =>
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trTheUserHasNoEmail"), animation: ToasterAnimation.FadeIn);
                            });
                        }
                        else
                        {
                            toemailexist = true;
                        }
                        break;
                    case 3:
                        var objct3 = cb_vendors.SelectedItem as ShippingCombo;
                        int shipId = (int)objct3.ShippingId;

                        toShipCompanies = await toShipCompanies.GetByID(shipId);
                        emailto = toShipCompanies.email;

                        if (emailto is null || emailto == "")
                        {
                            toemailexist = false;
                            this.Dispatcher.Invoke(() =>
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trTheShippingCompaniesHasNoEmail"), animation: ToasterAnimation.FadeIn);
                            });
                        }
                        else
                        {
                            toemailexist = true;
                        }

                        break;

                }

                if (email != null)
                {
                    if (email.emailId == 0)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoEmailForThisDept"), animation: ToasterAnimation.FadeIn);
                        });
                    }
                    else
                    {
                        if (toemailexist)
                        {
                            SetValues setvmodel = new SetValues();

                            List<SetValues> setvlist = new List<SetValues>();

                            setvlist = await setvmodel.GetBySetName("required_email_temp");

                            mailtosend = mailtosend.fillRequirdTempData(total, emailto, email, setvlist);

                            string msg = "";
                            Thread t1 = new Thread(() =>
                            {
                                msg = mailtosend.Sendmail();// temp comment
                                if (msg == "Failure sending mail.")
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoConnection"), animation: ToasterAnimation.FadeIn);
                                    });
                                }
                                else if (msg == "mailsent")
                                {

                                    this.Dispatcher.Invoke(() =>
                                    {
                                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trMailSent"), animation: ToasterAnimation.FadeIn);
                                    });

                                }
                                else
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trMailNotSent"), animation: ToasterAnimation.FadeIn);
                                    });
                                }
                            });
                            t1.Start();
                        }
                    }
                }
            }
            else
            {
                //this.Dispatcher.Invoke(() =>
                //{
                //});
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
                /////////////////////
                string pdfpath = "";
                pdfpath = @"\Thumb\report\temp.pdf";
                pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                BuildReport();
                LocalReportExtensions.ExportToPDF(rep, pdfpath);
                ///////////////////
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
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath;
            string firstTitle = "accountStatement";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string startDate = "";
            string vendorval = "";
            string searchval = "";
            string trVendor = "";
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");

            if (isArabic)
            {
                addpath = @"\Reports\StatisticReport\Accounts\AccountStatement\Ar\ArAccStatement.rdlc";

            }
            else
            {
                addpath = @"\Reports\StatisticReport\Accounts\AccountStatement\En\AccStatement.rdlc";

            }
            if (selectedTab == 0)
            {
                secondTitle = "vendors";
                trVendor = MainWindow.resourcemanagerreport.GetString("trVendor");
            }
            else if (selectedTab == 1)
            {
                secondTitle = "customers";
                trVendor = MainWindow.resourcemanagerreport.GetString("trCustomer");
            }
            else if (selectedTab == 2)
            {
                secondTitle = "users";
                trVendor = MainWindow.resourcemanagerreport.GetString("trUser");
            }
            else if (selectedTab == 3)
            {
                secondTitle = "shipping";
                trVendor = MainWindow.resourcemanagerreport.GetString("trShippingCompanynohint");
            }
            //filter
            //startDate = cb_vendorsDate.SelectedItem != null
            //  && (chk_allVendors.IsChecked == false || chk_allVendors.IsChecked == null)
            //  ? cb_vendorsDate.Text : (chk_allVendors.IsChecked == true ? all : "");
            //vendorval = cb_vendors.SelectedItem != null ? cb_vendors.Text : "";

            startDate = cb_vendorsDate.SelectedItem != null
              ? cb_vendorsDate.Text : "";
            vendorval = cb_vendors.SelectedItem != null ? cb_vendors.Text : "";

            paramarr.Add(new ReportParameter("VendorVal", vendorval));
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("trVendor", trVendor));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trDescription", MainWindow.resourcemanagerreport.GetString("trDescription")));
            paramarr.Add(new ReportParameter("trPayment", MainWindow.resourcemanagerreport.GetString("trPayment")));
            paramarr.Add(new ReportParameter("trCashTooltip", MainWindow.resourcemanagerreport.GetString("trCashTooltip")));
            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            Title = MainWindow.resourcemanagerreport.GetString("trAccounting") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            clsReports.cashTransferStsStatement(temp, rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            if (repTrRequires == "trRequired")
            {
                repTrRequires = MainWindow.resourcemanagerreport.GetString("trRequired");
            }
            else
            {
                repTrRequires = MainWindow.resourcemanagerreport.GetString("trWorthy");
            }
            paramarr.Add(new ReportParameter("trRequired", repTrRequires));
            paramarr.Add(new ReportParameter("finalAccount", txt_total.Text));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            rep.SetParameters(paramarr);
            rep.Refresh();
        }
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                /////////////////////////////////////
                Thread t1 = new Thread(() =>
                {
                    pdfStatement();
                });
                t1.Start();
                //////////////////////////////////////

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
        private void pdfStatement()
        {
            BuildReport();

            this.Dispatcher.Invoke(() =>
            {
                saveFileDialog.Filter = "PDF|*.pdf;";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;
                    LocalReportExtensions.ExportToPDF(rep, filepath);
                }
            });
        }
        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                /////////////////////////////////////
                Thread t1 = new Thread(() =>
                {
                    printStatement();
                });
                t1.Start();
                //////////////////////////////////////


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
        private void printStatement()
        {
            BuildReport();

            this.Dispatcher.Invoke(() =>
            {
                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
            });
        }
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //Thread t1 = new Thread(() =>
                //{
                ExcelStatement();

                //});
                //t1.Start();

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
        private void ExcelStatement()
        {
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