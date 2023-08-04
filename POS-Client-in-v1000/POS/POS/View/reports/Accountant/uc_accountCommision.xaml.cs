using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using POS.Classes;
using System;
using System.Collections.Generic;
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
using System.IO;
using POS.View.windows;

namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_accountCommision.xaml
    /// </summary>
    public partial class uc_accountCommision : UserControl
    {
        #region variabls
        IEnumerable<CashTransferSts> commissions;
        Statistics statisticsModel = new Statistics();
        IEnumerable<CashTransferSts> commissionsQuery;
        IEnumerable<CashTransferSts> commissionsCardQuery;
        IEnumerable<CashTransferSts> commissionsUserQuery;
        string searchText = "";
        int selectedTab = 0;
        //prin & pdf
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        bool showChart = true;
        double col_reportChartWidth = 0;
        #endregion

        public uc_accountCommision()
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

        private static uc_accountCommision _instance;
        public static uc_accountCommision Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_accountCommision();
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

                tb_totalCurrency.Text = AppSettings.Currency;
                rowToHide1.Height = new GridLength(0);
                rowToHide2.Height = new GridLength(0);

                col_reportChartWidth = col_reportChart.ActualWidth;

                await RefreshCommissionsList();
                await Search();

                chk_allCommission.IsChecked = true;

                #region key up
                cb_commission.IsTextSearchEnabled = false;
                cb_commission.IsEditable = true;
                cb_commission.StaysOpenOnEdit = true;
                cb_commission.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_commission.Text = "";
                #endregion

                Btn_company_Click(btn_company, null);

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
        private void paint()
        {
            bdr_company.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_user.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_company.Fill = Brushes.White;
            path_user.Fill = Brushes.White;
        }
        async Task Search()
        {
            if (commissions is null)
                await RefreshCommissionsList();

            if (selectedTab == 0)
                commissionsQuery = commissionsCardQuery;
            else
                commissionsQuery = commissionsUserQuery;

            searchText = txt_search.Text.ToLower();
            commissionsQuery = commissionsQuery
                .Where(s =>
            (
            s.invNumber != null ? s.invNumber.ToLower().Contains(searchText) : false
            ||
            s.transNumSource != null ? s.transNumSource.ToLower().Contains(searchText) : false
            )
            //&&
            //(//paid
            //selectedTab == 0 ? true :
            //    ((chk_isPaid.IsChecked == true ) ? s.isCommissionPaid == 1  : false)
            //    ||
            //    ((chk_isNotPaid.IsChecked == true ) ? s.isCommissionPaid == 0 : false)
            //)
            &&
            //card/user
            (
            chk_allCommission.IsChecked.Value ?
                true :
                cb_commission.SelectedIndex == -1 ?
                false :
                    selectedTab == 0 ? s.cardId == Convert.ToInt32(cb_commission.SelectedValue)
                                     : s.userId == Convert.ToInt32(cb_commission.SelectedValue)
            )
            &&
            //start date
            (dp_startDate.SelectedDate != null ? s.updateDate.Value.Date >= dp_startDate.SelectedDate.Value.Date : true)
            &&
            //end date
            (dp_endDate.SelectedDate != null ? s.updateDate.Value.Date <= dp_endDate.SelectedDate.Value.Date : true)
            );

            RefreshCommissionsView();
            fillColumnChart();
            fillPieChart();
            fillRowChart();
        }
        List<Card> cards = new List<Card>();
        private void fillCard()
        {
            cb_commission.SelectedValuePath = "cardId";
            cb_commission.DisplayMemberPath = "name";
            cards = commissionsCardQuery.GroupBy(i => i.cardId).Select(i => new Card { name = i.FirstOrDefault().cardName, cardId = i.FirstOrDefault().cardId.Value }).ToList();
            cb_commission.ItemsSource = cards;
        }
        List<User> users = new List<User>();
        private void fillUsers()
        {
            cb_commission.SelectedValuePath = "userId";
            cb_commission.DisplayMemberPath = "fullName";
            users = commissionsUserQuery.GroupBy(i => i.userId).Select(i => new User { fullName = i.FirstOrDefault().usersName, userId = i.FirstOrDefault().userId.Value }).ToList();
            cb_commission.ItemsSource = users;
        }
        async Task<IEnumerable<CashTransferSts>> RefreshCommissionsList()
        {
            commissions = await statisticsModel.GetCommissions();
            commissionsCardQuery = commissions.Where(c => c.processType == "commissionCard");
            commissionsUserQuery = commissions.Where(c => c.processType == "commissionAgent");
            foreach (var c in commissionsUserQuery)
            {
                c.usersName = c.usersName + " " + c.usersLName;
            }
            return commissions;
        }
        void RefreshCommissionsView()
        {
            dg_commission.ItemsSource = commissionsQuery;
            txt_count.Text = dg_commission.Items.Count.ToString();
            //decimal total = commissionsQuery.Sum(c => c.cash.Value);
            decimal total = 0;
            if (selectedTab == 0)
                total = commissionsQuery.Sum(c => c.cash.Value);
            else
                total = commissionsQuery.Sum(c => c.deserved.Value);
            tb_total.Text = total.ToString();
        }
        private void translate()
        {
            tt_company.Content = MainWindow.resourcemanager.GetString("paymentAgents");
            tt_user.Content = MainWindow.resourcemanager.GetString("salesEmployees");
            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");

            //chk_isPaid.Content = MainWindow.resourcemanager.GetString("trPaid_");
            //chk_isNotPaid.Content = MainWindow.resourcemanager.GetString("trUnPaid");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startDate, MainWindow.resourcemanager.GetString("trStartDate") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endDate, MainWindow.resourcemanager.GetString("trEndDate") + "...");

            chk_allCommission.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_commNo.Header = MainWindow.resourcemanager.GetString("trProcessCharp");
            col_InvNo.Header = MainWindow.resourcemanager.GetString("trInvoiceCharp");
            col_card.Header = MainWindow.resourcemanager.GetString("paymentAgent");
            col_user.Header = MainWindow.resourcemanager.GetString("salesEmployee");
            col_cashCard.Header = MainWindow.resourcemanager.GetString("trCash_");
            col_cashUser.Header = MainWindow.resourcemanager.GetString("trCash_");
            col_percentage.Header = MainWindow.resourcemanager.GetString("trPercentageDiscount");
            col_value.Header = MainWindow.resourcemanager.GetString("trValue");
            col_cash.Header = MainWindow.resourcemanager.GetString("commission");
            //col_isPaid.Header = MainWindow.resourcemanager.GetString("trStatus");
            col_Paid.Header = MainWindow.resourcemanager.GetString("trPaid");
            col_updateDate.Header = MainWindow.resourcemanager.GetString("trDate");

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
        #endregion

        #region tabs
        private async void Btn_company_Click(object sender, RoutedEventArgs e)
        {//agent
            try
            {
                searchText = "";
                txt_search.Text = "";
                selectedTab = 0;

                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_commission, MainWindow.resourcemanager.GetString("paymentAgent") + "...");
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_company);
                path_company.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                //show/hide columns
                col_commNo.Visibility = Visibility.Visible;
                col_card.Visibility = Visibility.Visible;
                col_user.Visibility = Visibility.Hidden;
                col_cashCard.Visibility = Visibility.Visible;
                col_cashUser.Visibility = Visibility.Hidden;
                col_Paid.Visibility = Visibility.Hidden;

                //chk_isNotPaid.Visibility = Visibility.Collapsed;
                //chk_isPaid.Visibility = Visibility.Collapsed;

                dp_startDate.SelectedDate = null;
                dp_endDate.SelectedDate = null;
                chk_allCommission.IsChecked = true;

                fillCard();
                await Search();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_user_Click(object sender, RoutedEventArgs e)
        {//employee
            try
            {
                searchText = "";
                txt_search.Text = "";
                selectedTab = 1;

                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_commission, MainWindow.resourcemanager.GetString("salesEmployee") + "...");
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_user);
                path_user.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                //show/hide columns
                col_commNo.Visibility = Visibility.Hidden;
                col_card.Visibility = Visibility.Hidden;
                col_user.Visibility = Visibility.Visible;
                col_cashCard.Visibility = Visibility.Hidden;
                col_cashUser.Visibility = Visibility.Visible;
                col_Paid.Visibility = Visibility.Visible;

                //chk_isNotPaid.Visibility = Visibility.Visible;
                //chk_isPaid.Visibility = Visibility.Visible;

                //chk_isPaid.IsChecked = true;
                //chk_isNotPaid.IsChecked = true;

                dp_startDate.SelectedDate = null;
                dp_endDate.SelectedDate = null;
                chk_allCommission.IsChecked = true;

                fillUsers();
                await Search();

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion

        #region events
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
        private async void RefreshViewCheckbox(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                await Search();

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
        private async void RefreshView_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {//select date
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                await RefreshCommissionsList();
                await Search();

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
        private async void Cb_commission_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                await Search();

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
        private void Cb_commission_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = cards.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private async void Chk_allCommission_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_commission.SelectedIndex = -1;
                cb_commission.IsEnabled = false;
                cb_commission.Text = "";
                if (selectedTab == 0) cb_commission.ItemsSource = cards;
                else cb_commission.ItemsSource = users;

                await Search();

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
        private async void Chk_allCommission_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_commission.IsEnabled = true;
                await Search();
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
        private async void Txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                await Search();

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

                await RefreshCommissionsList();
                txt_search.Text = "";
                searchText = "";
                dp_startDate.SelectedDate = null;
                dp_endDate.SelectedDate = null;
                await Search();

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

        #region reports
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            //string firstTitle = "PreparingOrders";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string startDate = "";
            string endDate = "";
            string paymentVal = "";

            string searchval = "";
            string trBranch = "";
            string type = "";
            string paidchk = "";
            string unPaidchk = "";



            string trPayment = "";
            List<string> invTypelist = new List<string>();
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");

            if (isArabic)
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Commission\Ar\ArComCard.rdlc";
                    secondTitle = MainWindow.resourcemanagerreport.GetString("paymentAgents");
                    trPayment = MainWindow.resourcemanagerreport.GetString("paymentAgent");
                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Commission\Ar\ArComUser.rdlc";

                    secondTitle = MainWindow.resourcemanagerreport.GetString("salesEmployees");
                    trPayment = MainWindow.resourcemanagerreport.GetString("salesEmployee");

                }
            }
            else
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Commission\En\EnComCard.rdlc";
                    secondTitle = MainWindow.resourcemanagerreport.GetString("paymentAgents");
                    trPayment = MainWindow.resourcemanagerreport.GetString("paymentAgent");
                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Commission\En\EnComUser.rdlc";
                    secondTitle = MainWindow.resourcemanagerreport.GetString("salesEmployees");
                    trPayment = MainWindow.resourcemanagerreport.GetString("salesEmployee");
                }



            }
            //filter
            startDate = dp_startDate.SelectedDate != null ? SectionData.DateToString(dp_startDate.SelectedDate) : "";
            endDate = dp_endDate.SelectedDate != null ? SectionData.DateToString(dp_endDate.SelectedDate) : "";
            paymentVal = cb_commission.SelectedItem != null
                && (chk_allCommission.IsChecked == false || chk_allCommission.IsChecked == null)
                ? cb_commission.Text : (chk_allCommission.IsChecked == true ? all : "");
            //paidchk = chk_isPaid.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trPaid_") : "";
            //unPaidchk = chk_isNotPaid.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trUnPaid") : "";
            invTypelist.Add(paidchk);
            invTypelist.Add(unPaidchk);
            int i = 0;
            foreach (string r in invTypelist)
            {
                if (r != null && r != "")
                {
                    if (i == 0)
                    {
                        type = r;
                    }
                    else
                    {
                        type = type + " , " + r;
                    }
                    i++;
                }

            }
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("trPayment", trPayment));
            paramarr.Add(new ReportParameter("paymentVal", paymentVal));
            paramarr.Add(new ReportParameter("type", type));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            //   tb_total.Text
            paramarr.Add(new ReportParameter("totalval", tb_total.Text));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));

            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            //secondTitle = "";
            // subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            subTitle = MainWindow.resourcemanagerreport.GetString("commission") + " / " + secondTitle;
            Title = MainWindow.resourcemanagerreport.GetString("trAccounting") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            clsReports.CommissionReport(commissionsQuery.ToList(), rep, reppath, paramarr);
            clsReports.Header(paramarr);
            rep.SetParameters(paramarr);
            rep.Refresh();
        }
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {

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


                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }


        }
        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {

                SectionData.StartAwait(grid_main);

                #region

                BuildReport();

                LocalReportExtensions.PrintToPrinter(rep);
                #endregion


                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }


        }
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                SectionData.StartAwait(grid_main);

                #region
                //Thread t1 = new Thread(() =>
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
                //t1.Start();

                #endregion


                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {

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
                //dg_orders.ItemsSource = ordersQuery;
                Window.GetWindow(this).Opacity = 1;
                #endregion


                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                Window.GetWindow(this).Opacity = 1;
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }


        }
        #endregion

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

        #region charts
        private void fillRowChart()
        {
        }

        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            List<decimal> x = new List<decimal>();

            var result = commissionsQuery;
            if (selectedTab == 0)
            {
                result = result
                    .GroupBy(s => new { s.cardId })
                    .Select(s => new CashTransferSts
                    {
                        cardId = s.FirstOrDefault().cardId,
                        cardName = s.FirstOrDefault().cardName,
                        cash = s.Sum(g => g.cash),
                    });
                x = result.Select(m => decimal.Parse(SectionData.DecTostring((decimal)m.cash))).ToList();
                titles = result.Select(m => m.cardName).ToList();
            }

            else if (selectedTab == 1)
            {
                result = result
                    .GroupBy(s => new { s.userId })
                    .Select(s => new CashTransferSts
                    {
                        userId = s.FirstOrDefault().userId,
                        usersName = s.FirstOrDefault().usersName,
                        cash = s.Sum(g => g.cash),
                    });
                x = result.Select(m => decimal.Parse(SectionData.DecTostring((decimal)m.cash))).ToList();
                titles = result.Select(m => m.usersName).ToList();
            }
            int count = x.Count();
            SeriesCollection piechartData = new SeriesCollection();
            int xCount = 6;
            if (x.Count() <= 6) xCount = x.Count();
            for (int i = 0; i < xCount; i++)
            {
                List<decimal> final = new List<decimal>();

                final.Add(x.ToList().Skip(i).FirstOrDefault());
                piechartData.Add(
                  new PieSeries
                  {
                      Values = final.AsChartValues(),
                      Title = titles.Skip(i).FirstOrDefault(),
                      DataLabels = true,
                  }
              );
            }
            if (x.Count() > 6)
            {
                decimal finalSum = 0;
                for (int i = 6; i < x.Count(); i++)
                {
                    finalSum = finalSum + x.ToList().Skip(i).FirstOrDefault();
                }
                if (finalSum != 0)
                {
                    List<decimal> final = new List<decimal>();

                    final.Add(finalSum);
                    piechartData.Add(
                      new PieSeries
                      {
                          Values = final.AsChartValues(),
                          Title = MainWindow.resourcemanager.GetString("trOthers"),
                          DataLabels = true,
                      }
                  );
                }
            }
            chart1.Series = piechartData;
        }

        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            string title = "";

            var res = commissionsQuery;
            if (selectedTab == 0)
            {
                res = commissionsQuery.GroupBy(x => new { x.cardId }).Select(x => new CashTransferSts
                {
                    cardId = x.FirstOrDefault().cardId,
                    cardName = x.FirstOrDefault().cardName,
                    cashSource = x.Sum(g => (decimal)(g.cashSource))
                });
                names.AddRange(res.Select(nn => nn.cardName));
                title = MainWindow.resourcemanager.GetString("paymentAgent");
            }
            else if (selectedTab == 1)
            {
                res = commissionsQuery.GroupBy(x => new { x.userId }).Select(x => new CashTransferSts
                {
                    userId = x.FirstOrDefault().userId,
                    usersName = x.FirstOrDefault().usersName,
                    totalNet = x.Sum(g => (decimal)(g.totalNet))
                });
                names.AddRange(res.Select(nn => nn.usersName));
                title = MainWindow.resourcemanager.GetString("salesEmployee");
            }
            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> cP = new List<decimal>();

            int xCount = 6;
            if (names.Count() <= 6) xCount = names.Count();

            for (int i = 0; i < xCount; i++)
            {
                if (selectedTab == 0)
                    cP.Add(decimal.Parse(SectionData.DecTostring(res.ToList().Skip(i).FirstOrDefault().cashSource.Value)));
                else if (selectedTab == 1)
                    cP.Add(decimal.Parse(SectionData.DecTostring(res.ToList().Skip(i).FirstOrDefault().totalNet.Value)));
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (names.Count() > 6)
            {
                decimal b = 0;
                for (int i = 6; i < names.Count(); i++)
                {
                    if (selectedTab == 0)
                        b = b + decimal.Parse(SectionData.DecTostring(res.ToList().Skip(i).FirstOrDefault().cashSource.Value));
                    else if (selectedTab == 1)
                        b = b + decimal.Parse(SectionData.DecTostring(res.ToList().Skip(i).FirstOrDefault().totalNet.Value));
                }
                if (!(b == 0))
                {
                    cP.Add(b);
                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cP.AsChartValues(),
                DataLabels = true,
                Title = title
            }); ;

            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        #endregion

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
