using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
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

namespace POS.View.reports.deliveryReports
{
    /// <summary>
    /// Interaction logic for uc_deliveryOrderStatus.xaml
    /// </summary>
    public partial class uc_deliveryOrderStatus : UserControl
    {
        #region variabls
        IEnumerable<Invoice> orders;
        Statistics statisticsModel = new Statistics();
        IEnumerable<Invoice> ordersQuery;
        string searchText = "";
        int selectedTab = 0;
        //prin & pdf
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        bool showChart = true;
        double col_reportChartWidth = 0;
        #endregion

        public uc_deliveryOrderStatus()
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

        private static uc_deliveryOrderStatus _instance;
        public static uc_deliveryOrderStatus Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_deliveryOrderStatus();
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

                await RefreshOrderStatusList();

                chk_allStatus.IsChecked = true;
                chk_allBranches.IsChecked = true;

                await Search();

                #region key up
                // key_up branch
                cb_branches.IsTextSearchEnabled = false;
                cb_branches.IsEditable = true;
                cb_branches.StaysOpenOnEdit = true;
                cb_branches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branches.Text = "";
                #endregion

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), btn_orderStatus.Tag.ToString());

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
        List<Branch> branches = new List<Branch>();
        private void fillBranches()
        {
            cb_branches.SelectedValuePath = "branchId";
            cb_branches.DisplayMemberPath = "name";
            branches = orders.GroupBy(i => i.branchId).Select(i => new Branch { name = i.FirstOrDefault().branchCreatorName, branchId = i.FirstOrDefault().branchCreatorId.Value }).ToList();
            cb_branches.ItemsSource = branches;
        }
        private void fillStatus()
        {
            var typelist = new[] {
                //new { Text = MainWindow.resourcemanager.GetString("trListed")     , Value = "Listed" },
                //new { Text = MainWindow.resourcemanager.GetString("trPreparing")  , Value = "Preparing" },
                new { Text = MainWindow.resourcemanager.GetString("trReady")      , Value = "Ready" },
                new { Text = MainWindow.resourcemanager.GetString("withDelivery") , Value = "Collected" },
                new { Text = MainWindow.resourcemanager.GetString("onTheWay")     , Value = "InTheWay" },
                new { Text = MainWindow.resourcemanager.GetString("trDone")       , Value = "Done" },
                 };
            cb_status.DisplayMemberPath = "Text";
            cb_status.SelectedValuePath = "Value";
            cb_status.ItemsSource = typelist;
        }
        private void translate()
        {
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranch") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_status, MainWindow.resourcemanager.GetString("trStatus") + "...");

            chk_allBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allStatus.Content = MainWindow.resourcemanager.GetString("trAll");

            tt_orderStatus.Content = MainWindow.resourcemanager.GetString("orderStatus");

            col_InvNo.Header = MainWindow.resourcemanager.GetString("trInvoiceCharp");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_status.Header = MainWindow.resourcemanager.GetString("trStatus");
            col_updateDate.Header = MainWindow.resourcemanager.GetString("trDate");

            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
         }
        async Task<IEnumerable<Invoice>> RefreshOrderStatusList()
        {
            orders = await statisticsModel.GetDeliveryStat(MainWindow.branchID.Value, MainWindow.userID.Value);

            fillBranches();
            fillStatus();

            return orders;
        }
        async Task Search()
        {
            if (orders is null)
                await RefreshOrderStatusList();

            searchText = txt_search.Text.ToLower();
            ordersQuery = orders
                .Where(s =>
            (
            s.invNumber.ToLower().Contains(searchText)
            )
            &&
            //branchID
            (
            chk_allBranches.IsChecked.Value ?
                true :
                (cb_branches.SelectedIndex != -1 ? s.branchCreatorId == Convert.ToInt32(cb_branches.SelectedValue) : false)
            )
            &&
            //status
            (
            chk_allStatus.IsChecked.Value ?
                true :
                (cb_status.SelectedIndex != -1 ? s.status == cb_status.SelectedValue.ToString() : false)
            )
            &&
            //start date
            (dp_startDate.SelectedDate != null ? s.updateDate.Value.Date >= dp_startDate.SelectedDate.Value.Date : true)
            &&
            //end date
            (dp_endDate.SelectedDate != null ? s.updateDate.Value.Date <= dp_endDate.SelectedDate.Value.Date : true)
            );

            RefreshSerialsView();
            fillColumnChart();
            fillPieChart();
            fillRowChart();
        }
        void RefreshSerialsView()
        {
            dg_orders.ItemsSource = ordersQuery;
            txt_count.Text = dg_orders.Items.Count.ToString();
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
        private void RefreshViewCheckbox(object sender, RoutedEventArgs e)
        {

        }
        private async void RefreshView_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {//select date
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                await RefreshOrderStatusList();
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
        private async void Cb_branches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select branch
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
        private async void Chk_allBranches_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_branches.SelectedIndex = -1;
                cb_branches.Text = "";
                cb_branches.IsEnabled = false;

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
        private void Cb_branches_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = branches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private async void Cb_status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select status
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
        private async void Chk_allStatus_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_status.SelectedIndex = -1;
                cb_status.IsEnabled = false;
                cb_status.Text = "";

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
        private async void Chk_allStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_status.IsEnabled = true;
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

                await RefreshOrderStatusList();
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
            //string subTitle = "";
            string Title = "";
            string startDate = "";
            string endDate = "";
            string branchVal = "";
            string statusVal = "";
            string searchval = "";
 
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            if (isArabic)
            {
                addpath = @"\Reports\StatisticReport\Delivery\Ar\ArOrderStat.rdlc";
            }
            else
            {
                addpath = @"\Reports\StatisticReport\Delivery\En\OrderStat.rdlc";
            }

            //filter
            startDate = dp_startDate.SelectedDate != null ? SectionData.DateToString(dp_startDate.SelectedDate) : "";
            endDate = dp_endDate.SelectedDate != null ? SectionData.DateToString(dp_endDate.SelectedDate) : "";
            branchVal = cb_branches.SelectedItem != null
                && (chk_allBranches.IsChecked == false || chk_allBranches.IsChecked == null)
                ? cb_branches.Text : (chk_allBranches.IsChecked == true ? all : "");
            statusVal = cb_status.SelectedItem != null
             && (chk_allStatus.IsChecked == false || chk_allStatus.IsChecked == null)
             ? clsReports.preparingOrderStatusConvert(cb_status.SelectedValue.ToString()) : (chk_allStatus.IsChecked == true ? all : "");
        
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));         
            paramarr.Add(new ReportParameter("branchVal", branchVal));
            paramarr.Add(new ReportParameter("statusVal", statusVal));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            //secondTitle = "";
            // subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);          
           secondTitle = MainWindow.resourcemanagerreport.GetString("orderStatus");
            Title = MainWindow.resourcemanagerreport.GetString("deliveryReport") + " / " + secondTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            clsReports.DeliveryOrderStatReport(ordersQuery.ToList(), rep, reppath, paramarr);
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

        #region charts
        private void fillRowChart()
        {
        }
        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            List<int> resultList = new List<int>();
            titles.Clear();

            var result = orders
                .GroupBy(s => new { s.status })
                .Select(s => new 
                {
                    statusCount = s.Count(),
                    status = s.FirstOrDefault().status,
                });
            resultList = result.Select(m => m.statusCount).ToList();
            titles = result.Select(m => m.status).ToList();
            for (int t = 0; t < titles.Count; t++)
            {
                string s = "";
                switch (titles[t])
                {
                    case "Ready": s = MainWindow.resourcemanager.GetString("trReady"); break;
                    case "Collected": s = MainWindow.resourcemanager.GetString("withDelivery"); break;
                    case "InTheWay": s = MainWindow.resourcemanager.GetString("onTheWay"); break;
                    case "Done": s = MainWindow.resourcemanager.GetString("trDone"); break;
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
            //List<CashTransferSts> resultList = new List<CashTransferSts>();

            var resultList = orders.GroupBy(x => x.branchCreatorId).Select(x => new 
            {
                readyTotal = x.Where(g => g.status == "Ready").Count(),
                collectedTotal = x.Where(g => g.status == "Collected").Count(),
                inTheWayTotal = x.Where(g => g.status == "InTheWay").Count(),
                doneTotal = x.Where(g => g.status == "Done").Count(),
                branchName = x.FirstOrDefault().branchCreatorName,
                branchId = x.FirstOrDefault().branchCreatorId,
            }
            ).ToList();

            names.AddRange(resultList.Select(nn => nn.branchName));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<int> ready = new List<int>();
            List<int> collected = new List<int>();
            List<int> inTheWay = new List<int>();
            List<int> done = new List<int>();

            int xCount = 6;
            if (resultList.Count() <= 6)
                xCount = resultList.Count();
            for (int i = 0; i < xCount; i++)
            {
                ready.Add(resultList.ToList().Skip(i).FirstOrDefault().readyTotal);
                collected.Add(resultList.ToList().Skip(i).FirstOrDefault().collectedTotal);
                inTheWay.Add(resultList.ToList().Skip(i).FirstOrDefault().inTheWayTotal);
                done.Add(resultList.ToList().Skip(i).FirstOrDefault().doneTotal);

                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (resultList.Count() > 6)
            {
                int readySum = 0, collectedSum = 0, inTheWaySum = 0, doneSum = 0;
                for (int i = 6; i < resultList.Count; i++)
                {
                    readySum = readySum + resultList.ToList().Skip(i).FirstOrDefault().readyTotal;
                    collectedSum = collectedSum + resultList.ToList().Skip(i).FirstOrDefault().collectedTotal;
                    inTheWaySum = inTheWaySum + resultList.ToList().Skip(i).FirstOrDefault().inTheWayTotal;
                    doneSum = doneSum + resultList.ToList().Skip(i).FirstOrDefault().doneTotal;
                }
                if (!((readySum == 0) && (collectedSum == 0) && (inTheWaySum == 0) && (doneSum == 0)))
                {
                    ready.Add(readySum);
                    collected.Add(collectedSum);
                    inTheWay.Add(inTheWaySum);
                    done.Add(doneSum);

                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = ready.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("trReady")
            });
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = collected.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("withDelivery")
            });
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = inTheWay.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("onTheWay")
            });
            columnChartData.Add(
             new StackedColumnSeries
             {
                 Values = done.AsChartValues(),
                 DataLabels = true,
                 Title = MainWindow.resourcemanager.GetString("trDone")
             });

            DataContext = this;
            cartesianChart.Series = columnChartData;
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

        #region datagrid events
        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    if (cb_payments.SelectedIndex != -1 && !cb_payments.SelectedValue.ToString().Equals("multiple"))
            //    {
            //        Button btnPayments = sender as Button;

            //        ItemTransferInvoice iTI = itemTrasferInvoicesQuery.Where(i => i.invNumber == btnPayments.Tag.ToString()).FirstOrDefault();
            //        if (iTI.cachTransferList.Count > 0 && iTI.cachTransferList.Count(i => i.processType == cb_payments.SelectedValue.ToString()) > 1)
            //            btnPayments.Visibility = Visibility.Visible;
            //        else
            //            btnPayments.Visibility = Visibility.Collapsed;

            //        if (iTI.cachTransferList.Count > 0 && iTI.cachTransferList.Count(i => i.cardName == cb_card.SelectedValue.ToString()) > 1)
            //            btnPayments.Visibility = Visibility.Visible;
            //        else
            //            btnPayments.Visibility = Visibility.Collapsed;

            //    }
            //    else
            //    {
            //        this.DataContext = itemTrasferInvoicesQuery;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            //}
        }

        private async void showStatus(object sender, RoutedEventArgs e)
        {//status
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        Invoice row = (Invoice)dg_orders.SelectedItems[0];

                        Statistics statisticsModel = new Statistics();

                        Window.GetWindow(this).Opacity = 0.2;
                        wd_orderStatus w = new wd_orderStatus();
                        w.orders = await statisticsModel.GetStatByInvId(row.invoiceId);
                        w.invoice = row;
                        w.ShowDialog();
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
