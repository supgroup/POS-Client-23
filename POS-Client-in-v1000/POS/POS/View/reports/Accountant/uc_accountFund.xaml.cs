using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System.Threading;
using POS.View.windows;
using System.Resources;
using System.Reflection;
using static POS.Classes.Statistics;

namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_accountFund.xaml
    /// </summary>
    public partial class uc_accountFund : UserControl
    {
        #region variables
        IEnumerable<BalanceSTS> balances;
        Statistics statisticsModel = new Statistics();
        IEnumerable<BalanceSTS> balancesQuery;
        string searchText = "";
        //int selectedTab = 0;
        //prin & pdf
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        #endregion

        private static uc_accountFund _instance;
        public static uc_accountFund Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_accountFund();
                return _instance;
            }
        }
        public uc_accountFund()
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

                #region key up
                // key_up branch
                cb_branches.IsTextSearchEnabled = false;
                cb_branches.IsEditable = true;
                cb_branches.StaysOpenOnEdit = true;
                cb_branches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branches.Text = "";
                // key_up section
                cb_pos.IsTextSearchEnabled = false;
                cb_pos.IsEditable = true;
                cb_pos.StaysOpenOnEdit = true;
                cb_pos.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_pos.Text = "";
                #endregion

                await RefreshBalanceSTSList();

                chk_allBranches.IsChecked = true;
                chk_allPos.IsChecked = true;

                fillBranches();

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), btn_branch.Tag.ToString());

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region methods
        async Task Search()
        {

            if (balances is null)
                await RefreshBalanceSTSList();

            searchText = txt_search.Text.ToLower();
            balancesQuery = balances
                .Where(s =>
            (
            s.branchName.ToLower().Contains(searchText)
            ||
            s.posName.ToString().ToLower().Contains(searchText)
            )
            &&
            //branchID
            (cb_branches.SelectedIndex != -1 ? s.branchId == Convert.ToInt32(cb_branches.SelectedValue) : true)
            &&
            //posID
            (cb_pos.SelectedIndex != -1 ? s.posId == Convert.ToInt32(cb_pos.SelectedValue) : true)
            );

            //balancesQueryExcel = balancesQuery.ToList();
            RefreshBalancesView();
            fillColumnChart();
            fillPieChart();

        }
        private void SearchEmpty()
        {
            searchText = txt_search.Text.ToLower();
            balancesQuery = new List<BalanceSTS>();

            //balancesQueryExcel = balancesQuery.ToList();
            RefreshBalancesView();
            fillColumnChart();
            fillPieChart();

        }
        void RefreshBalancesView()
        {
            dgFund.ItemsSource = balancesQuery;
            txt_count.Text = balancesQuery.Count().ToString();
            decimal total = balancesQuery.Select(b => b.balance.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);
        }
        async Task<IEnumerable<BalanceSTS>> RefreshBalanceSTSList()
        {
            balances = await statisticsModel.GetBalance(MainWindow.branchID.Value, MainWindow.userID.Value);
            return balances;

        }
        private void translate()
        {
            tt_branch.Content = MainWindow.resourcemanager.GetString("trBranches");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tt_branch, MainWindow.resourcemanager.GetString("trBranches"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranch"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_pos, MainWindow.resourcemanager.GetString("trPOS"));

            chk_allBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allPos.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_branchName.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_posName.Header = MainWindow.resourcemanager.GetString("trPOS");
            col_posBalance.Header = MainWindow.resourcemanager.GetString("trBalance");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
        }
        List<Branch> branches = new List<Branch>();
        private void fillBranches()
        {
            cb_branches.SelectedValuePath = "branchId";
            cb_branches.DisplayMemberPath = "name";
            branches = balances.GroupBy(i => i.branchId).Select(i => new Branch { name = i.FirstOrDefault().branchName, branchId = i.FirstOrDefault().branchId }).ToList();
            //cb_branches.ItemsSource = balances.Select(i => new { i.branchName, i.branchId }).Distinct();
            cb_branches.ItemsSource = branches;
        }
        List<Pos> poss = new List<Pos>();
        private void fillPos(int bID)
        {
            cb_pos.SelectedValuePath = "posId";
            cb_pos.DisplayMemberPath = "name";
            //cb_pos.ItemsSource = balances.Where(b => b.branchId == bID).Select(i => new{i.posName,i.posId }).Distinct();
            poss = balances.GroupBy(i => i.posId).Where(b => b.FirstOrDefault().branchId == bID).Select(i => new Pos { name = i.FirstOrDefault().posName, posId = i.FirstOrDefault().posId }).ToList();
            cb_pos.ItemsSource = poss;
        }
        #endregion

        #region charts
        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            List<decimal> balances = new List<decimal>();

            //var temp = balancesQuery;
            var result = balancesQuery.GroupBy(s => s.posId).Select(s => new
            {
                posId = s.Key,
            });

            var tempName = balancesQuery.GroupBy(s => s.posName + "/" + s.branchName).Select(s => new
            {
                posName = s.Key
            });
            names.AddRange(tempName.Select(nn => nn.posName));

            var tempBalance = balancesQuery.GroupBy(s => s.balance).Select(s => new
            {
                balance = s.Key
            });
            balances.AddRange(tempBalance.Select(nn => decimal.Parse(SectionData.DecTostring(nn.balance.Value))));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> cS = new List<decimal>();

            List<string> titles = new List<string>()
            {
               MainWindow.resourcemanager.GetString("tr_Balance")
            };
            int x = 6;
            if (names.Count() <= 6) x = names.Count();

            for (int i = 0; i < x; i++)
            {
                cS.Add(balances.ToList().Skip(i).FirstOrDefault());
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            if (names.Count() > 6)
            {
                decimal balanceSum = 0;
                for (int i = 6; i < names.Count(); i++)
                    balanceSum = balanceSum + balances.ToList().Skip(i).FirstOrDefault();

                if (balanceSum != 0)
                    cS.Add(balanceSum);

                axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
            }

            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cS.AsChartValues(),
                Title = titles[0],
                DataLabels = true,
            });

            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            IEnumerable<int> x = null;
            IEnumerable<decimal> balances = null;

            titles.Clear();

            //var temp = balancesQuery;
            var titleTemp = balancesQuery.GroupBy(m => m.branchName);
            titles.AddRange(titleTemp.Select(jj => jj.Key));
            var result = balancesQuery.GroupBy(s => s.branchId)
                        .Select(
                            g => new
                            {
                                branchId = g.Key,
                                balance = g.Sum(s => s.balance),
                                count = g.Count()
                            });
            balances = result.Select(m => decimal.Parse(SectionData.DecTostring(m.balance.Value)));

            SeriesCollection piechartData = new SeriesCollection();
            for (int i = 0; i < balances.Count(); i++)
            {
                List<decimal> final = new List<decimal>();
                List<string> lable = new List<string>();
                final.Add(balances.ToList().Skip(i).FirstOrDefault());
                piechartData.Add(
                  new PieSeries
                  {
                      Values = final.AsChartValues(),
                      Title = titles.Skip(i).FirstOrDefault(),
                      DataLabels = true,
                  }
              );
            }
            chart1.Series = piechartData;
        }
        private void fillRowChart()
        {

        }
        #endregion

        #region events
        private async void cb_branches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select branch
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_branches.SelectedItem == null)
                {
                    chk_allPos.IsEnabled = false;
                    cb_pos.SelectedItem = null;
                    cb_pos.IsEnabled = false;
                }
                else
                {
                    chk_allPos.IsEnabled = true;
                    chk_allPos.IsChecked = true;
                    cb_pos.SelectedItem = null;
                    cb_pos.IsEnabled = false;
                }

                await Search();
                fillPos(Convert.ToInt32(cb_branches.SelectedValue));

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
        private async void Chk_allBranches_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_branches.SelectedIndex = -1;
                cb_branches.IsEnabled = false;
                cb_branches.Text = "";
                cb_branches.ItemsSource = branches;

                chk_allPos.IsEnabled = false;
                chk_allPos.IsChecked = true;
                cb_pos.SelectedItem = null;
                cb_pos.IsEnabled = false;
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
        private async void Chk_allBranches_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_branches.IsEnabled = true;
                chk_allPos.IsEnabled = false;
                cb_pos.SelectedItem = null;
                cb_pos.IsEnabled = false;
                SearchEmpty();

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
        private async void Cb_pos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select pos
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
        private async void Chk_allPos_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_pos.SelectedIndex = -1;
                cb_pos.IsEnabled = false;
                cb_pos.Text = "";
                cb_pos.ItemsSource = poss;

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
        private async void Chk_allPos_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_pos.IsEnabled = true;

                SearchEmpty();

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
        private void Cb_branches_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = branches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_pos_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = poss.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region reports
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            string firstTitle = "fund";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string startDate = "";
            string vendorval = "";
            string searchval = "";
            string branchVal = "";
            string posVal = "";
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");

            if (isArabic)
            {
                addpath = @"\Reports\StatisticReport\Accounts\Fund\Ar\ArFund.rdlc";

            }
            else
            {
                //english
                addpath = @"\Reports\StatisticReport\Accounts\Fund\En\Fund.rdlc";
            }
            //filter
            branchVal = cb_branches.SelectedItem != null
                       && (chk_allBranches.IsChecked == false || chk_allBranches.IsChecked == null)
                       ? cb_branches.Text : (chk_allBranches.IsChecked == true ? all : "");
            posVal = cb_pos.SelectedItem != null
               && (chk_allPos.IsChecked == false || chk_allPos.IsChecked == null)
               && branchVal != ""
               ? cb_pos.Text : (chk_allPos.IsChecked == true && branchVal != "" ? all : "");
            paramarr.Add(new ReportParameter("branchVal", branchVal));
            paramarr.Add(new ReportParameter("posVal", posVal));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trPOS", MainWindow.resourcemanagerreport.GetString("trPOS")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            paramarr.Add(new ReportParameter("trBalance", MainWindow.resourcemanagerreport.GetString("trBalance")));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            
            secondTitle = "branch";
            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trAccounting") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));

            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            //  getpuritemcount
            paramarr.Add(new ReportParameter("totalBalance", tb_total.Text));

            clsReports.FundStsReport(balancesQuery, rep, reppath, paramarr);
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
                List<ItemTransferInvoice> query = new List<ItemTransferInvoice>();

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
                List<ItemTransferInvoice> query = new List<ItemTransferInvoice>();

                #region
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
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                await RefreshBalanceSTSList();
                chk_allBranches.IsChecked = true;
                // await Search();

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
