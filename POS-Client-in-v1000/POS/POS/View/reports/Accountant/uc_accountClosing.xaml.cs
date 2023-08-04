using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using netoaster;
using POS.Classes;
using POS.View.windows;
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


namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_accountClosing.xaml
    /// </summary>
    public partial class uc_accountClosing : UserControl
    {
        #region variables
        IEnumerable<POSOpenCloseModel> closings;
        IEnumerable<POSOpenCloseModel> closingTemp = null;
        Statistics statisticsModel = new Statistics();
        string searchText = "";
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        private static uc_accountClosing _instance;
        #endregion

        public static uc_accountClosing Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_accountClosing();
                return _instance;
            }
        }
        public uc_accountClosing()
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
                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    grid_main.FlowDirection = FlowDirection.RightToLeft;
                }
                translate();
                #endregion

                await RefreshClosingList();

                col_reportChartWidth = col_reportChart.ActualWidth;

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), btn_closing.Tag.ToString());

                Btn_closing_Click(btn_closing, null); 

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
        private async void callSearch(object sender)
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
        List<Branch> branches = new List<Branch>();
        void fillBranches()
        {
            //var iulist = closings.GroupBy(g => g.branchId).Select(g => new { branchId = g.FirstOrDefault().branchId, branchName = g.FirstOrDefault().branchName }).ToList();
            branches = closings.GroupBy(g => g.branchId).Select(g => new Branch{ branchId = g.FirstOrDefault().branchId.Value, name = g.FirstOrDefault().branchName }).ToList();
            cb_closingBranches.SelectedValuePath = "branchId";
            cb_closingBranches.DisplayMemberPath = "name";
            cb_closingBranches.ItemsSource = branches;
        }
        private void translate()
        {
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_closingStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_closingEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_closingBranches, MainWindow.resourcemanager.GetString("trBranchHint"));

            chk_closingBranches.Content = MainWindow.resourcemanager.GetString("trAll");

            tt_closing.Content = MainWindow.resourcemanager.GetString("trCash_");

            col_Num.Header = MainWindow.resourcemanager.GetString("trNo");
            col_pos.Header = MainWindow.resourcemanager.GetString("trPOS");
            col_openDate.Header = MainWindow.resourcemanager.GetString("trOpenDate");
            col_openCash.Header = MainWindow.resourcemanager.GetString("trOpenCash");
            col_closeDate.Header = MainWindow.resourcemanager.GetString("trCloseDate");
            col_closeCash.Header = MainWindow.resourcemanager.GetString("trCloseCash");
            col_operation.Header = MainWindow.resourcemanager.GetString("trOperations");

            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
        }
        async Task Search()
        {
            if (closings is null)
                await RefreshClosingList();

            searchText = txt_search.Text.ToLower();

            closingTemp = closings.Where(t =>
            (t.transNum.ToLower().Contains(searchText)
            || t.posName.ToLower().Contains(searchText)
            )
            &&
            //closing start date
            (dp_closingStartDate.SelectedDate != null ? t.updateDate.Value.Date >= dp_closingStartDate.SelectedDate.Value.Date : true)
            &&
            //closing end date
            (dp_closingEndDate.SelectedDate != null ? t.updateDate.Value.Date <= dp_closingEndDate.SelectedDate.Value.Date : true)
            &&
            //branchID
            (cb_closingBranches.SelectedIndex != -1 ? t.branchId == Convert.ToInt32(cb_closingBranches.SelectedValue) : true)
            );

            RefreshClosingView();
            //fillBranches();
            fillColumnChart();
            fillPieChart();
        }
        private void SearchEmpty()
        {
            searchText = txt_search.Text.ToLower();

            closingTemp = new List<POSOpenCloseModel>();

            RefreshClosingView();
            fillBranches();
            fillColumnChart();
            fillPieChart();
        }
        private void RefreshClosingView()
        {
            dgClosing.ItemsSource = closingTemp;
            txt_count.Text = closingTemp.Count().ToString();

        }
        async Task<IEnumerable<POSOpenCloseModel>> RefreshClosingList()
        {
            closings = await statisticsModel.GetPosCashOpenClose(MainWindow.branchID.Value, MainWindow.userID.Value);
            return closings;
        }
        #endregion

        #region charts

        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            IEnumerable<int> x = null;
            IEnumerable<decimal> cashes = null;

            titles.Clear();

            var cashTemp = closingTemp.GroupBy(m => m.posId).Select(
                g => new
                {
                    posId = g.Key,
                    branchName = g.FirstOrDefault().branchName,
                    branchId = g.FirstOrDefault().branchId,
                    cash = g.LastOrDefault().cash
                });
            titles.AddRange(cashTemp.Select(jj => jj.branchName));

            var result = cashTemp.GroupBy(m => m.branchId)
                        .Select(
                            g => new
                            {
                                branchId = g.Key,
                                cash = g.Sum(s => s.cash),
                            });
            cashes = result.Select(m => decimal.Parse(SectionData.DecTostring(m.cash.Value)));

            SeriesCollection piechartData = new SeriesCollection();
            for (int i = 0; i < cashes.Count(); i++)
            {
                List<decimal> final = new List<decimal>();
                List<string> lable = new List<string>();
                final.Add(cashes.ToList().Skip(i).FirstOrDefault());
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

        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            List<decimal> cashes = new List<decimal>();

            var result = closingTemp.GroupBy(s => s.posId).Select(s => new
            {
                posId = s.Key,
                cash = s.LastOrDefault().cash
            });

            var tempName = closingTemp.GroupBy(s => s.posName + "/" + s.branchName).Select(s => new
            {
                posName = s.Key
            });
            names.AddRange(tempName.Select(nn => nn.posName));

            cashes.AddRange(result.Select(nn => decimal.Parse(SectionData.DecTostring(nn.cash.Value))));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> cS = new List<decimal>();

            List<string> titles = new List<string>()
            {
               MainWindow.resourcemanager.GetString("trCloseCash")
            };
            int x = 6;
            if (names.Count() <= 6) x = names.Count();

            for (int i = 0; i < x; i++)
            {
                cS.Add(cashes.ToList().Skip(i).FirstOrDefault());
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            if (names.Count() > 6)
            {
                decimal balanceSum = 0;
                for (int i = 6; i < names.Count(); i++)
                    balanceSum = balanceSum + cashes.ToList().Skip(i).FirstOrDefault();

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

        #endregion

        #region events
        private void Cb_closingBranches_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = branches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {//unload
            try
            {
                GC.Collect();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Cb_closingBranches_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
        private void Dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {//select date
            callSearch(sender);
        }
        private void Txt_search_SelectionChanged(object sender, RoutedEventArgs e)
        {//search
            callSearch(sender);
        }
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                closings = await statisticsModel.GetPosCashOpenClose(MainWindow.branchID.Value, MainWindow.userID.Value);
             
                txt_search.Text = "";
                searchText = "";
                dp_closingStartDate.SelectedDate = null;
                dp_closingEndDate.SelectedDate = null;
                chk_closingBranches.IsChecked = true;
              //  await Search();

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
        private async void Chk_closingBranches_Checked(object sender, RoutedEventArgs e)
        {//select all branches
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_closingBranches.SelectedIndex = -1;
                cb_closingBranches.IsEnabled = false;
                cb_closingBranches.Text = "";
                cb_closingBranches.ItemsSource = branches;
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
        private void Chk_closingBranches_Unchecked(object sender, RoutedEventArgs e)
        {//unselect all branches
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_closingBranches.IsEnabled = true;

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
        #endregion


        #region reports

        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            string firstTitle = "closing";//trDailyClosing
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string startDate = "";
            string endDate = "";
            string branchVal = "";

            string searchval = "";
            string trBranch = "";



            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");

            if (isArabic)
            {
                addpath = @"\Reports\StatisticReport\Accounts\Closing\Ar\ArClosing.rdlc";

            }
            else
            {
                //english
                addpath = @"\Reports\StatisticReport\Accounts\Closing\En\EnClosing.rdlc";
            }
            //filter
            startDate = dp_closingStartDate.SelectedDate != null ? SectionData.DateToString(dp_closingStartDate.SelectedDate) : "";

            endDate = dp_closingEndDate.SelectedDate != null ? SectionData.DateToString(dp_closingEndDate.SelectedDate) : "";
            //startTime = dt_startTime.SelectedTime != null ? dt_startTime.Text : "";
            //endTime = dt_endTime.SelectedTime != null ? dt_endTime.Text : "";
            branchVal = cb_closingBranches.SelectedItem != null
                && (chk_closingBranches.IsChecked == false || chk_closingBranches.IsChecked == null)
                ? cb_closingBranches.Text : (chk_closingBranches.IsChecked == true ? all : "");
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("branchVal", branchVal));


            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            secondTitle = "cash";
            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trAccounting") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));

            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            //  getpuritemcount
            //paramarr.Add(new ReportParameter("totalBalance", tb_total.Text));

            clsReports.ClosingStsReport(closingTemp, rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);

            rep.SetParameters(paramarr);

            rep.Refresh();
        }
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            //pdf
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
        #endregion

        #region dataGrid events
        private async void moveRowinDatagrid(object sender, RoutedEventArgs e)
        {//move
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        POSOpenCloseModel row = (POSOpenCloseModel)dgClosing.SelectedItems[0];

                        Statistics statisticsModel = new Statistics();
                        IEnumerable<OpenClosOperatinModel> cashesQuery;
                        cashesQuery = await statisticsModel.GetTransBetweenOpenClose(row.openCashTransId.Value, row.cashTransId);
                        //cashesQuery = cashesQuery.Where(c => c.transType != "c" && c.transType != "o");
                        cashesQuery = cashesQuery.Where(c => c.transType != "c");
                        if (cashesQuery.Count() == 0)
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoChange"), animation: ToasterAnimation.FadeIn);
                        else
                        {
                            Window.GetWindow(this).Opacity = 0.2;
                            wd_transBetweenOpenClose w = new wd_transBetweenOpenClose();
                            w.openCashTransID = row.openCashTransId.Value;
                            w.closeCashTransID = row.cashTransId;
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                        }
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
        int cashTransID = 0, openCashTransID = 0;
        IEnumerable<OpenClosOperatinModel> opquery;
        POSOpenCloseModel openclosrow = new POSOpenCloseModel();
        public async Task<IEnumerable<OpenClosOperatinModel>> getopquery(POSOpenCloseModel ocrow)
        {

            Statistics statisticsModel = new Statistics();
            opquery = await statisticsModel.GetTransBetweenOpenClose((int)ocrow.openCashTransId, ocrow.cashTransId);
            //  opquery = opquery.Where(c => c.transType != "c" && c.transType != "o");
            Boxquery = opquery;
            openclosrow = ocrow;
            return Boxquery;
        }
        // ReportCls reportclass = new ReportCls();
        // LocalReport rep = new LocalReport();
        // SaveFileDialog saveFileDialog = new SaveFileDialog();
        IEnumerable<OpenClosOperatinModel> Boxquery;
        Statistics stsModel = new Statistics();
        List<CardsSts> cardtransList = new List<CardsSts>();
        private async Task fillCashquery()
        {
            List<CardsSts> tmpcard = new List<CardsSts>();
            //     Boxquery = await stsModel.GetTransfromOpen(posId);
            tmpcard = await clsReports.calctotalCards(Boxquery);
            cardtransList = new List<CardsSts>();
            // open cash
            CardsSts cardcashrow = new CardsSts();
            cardcashrow = clsReports.BoxOpenCashCalc(Boxquery.ToList());
            // cardcashrow.name = MainWindow.resourcemanager.GetString("trOpenCash");
            //add cash row
            cardtransList.Add(cardcashrow);
            // cash
            cardcashrow = new CardsSts();
            cardcashrow = clsReports.BoxCashCalc(Boxquery.ToList());
            //  cardcashrow.name = MainWindow.resourcemanager.GetString("trCash");
            //add cash row
            cardtransList.Add(cardcashrow);
            //add card list
            cardtransList.AddRange(tmpcard);
        }
        private async Task BuildOperationReport(int posId)
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            string firstTitle = "closing";//trDailyClosing
            string secondTitle = "";
            string subTitle = "";
            string Title = "";

            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\StatisticReport\Accounts\box\Ar\ArBox.rdlc";

            }
            else
            {
                //english
                addpath = @"\Reports\StatisticReport\Accounts\box\En\EnBox.rdlc";
            }

            secondTitle = "operations";// trOperations
            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));

            string reppath = reportclass.PathUp(System.IO.Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            //  getpuritemcount
            //paramarr.Add(new ReportParameter("totalBalance", tb_total.Text));
            //  OpenClosOperatinModel
            // openclosrow= opquery.ToList().Where(x => x.processType == "box").ToList();
            string posName = "";
            string branchName = "";
            if (FillCombo.branchsAllList is null)
            { await FillCombo.RefreshBranchsAll(); }
            if (FillCombo.posAllReport is null)
            { await FillCombo.RefreshPosAllReport(); }
            Pos postemp = FillCombo.posAllReport.Where(p => p.posId == posId).FirstOrDefault();
            posName = postemp.name;

            branchName = FillCombo.branchsAllList.Where(p => p.branchId == (int)postemp.branchId).FirstOrDefault().name;
            List<CardsSts> cardList = new List<CardsSts>();
            cardList = cardtransList.Skip(1).ToList();
            decimal posBalance = (decimal)Boxquery.FirstOrDefault().posBalance;
            clsReports.BoxStateReport(Boxquery.ToList(), rep, reppath, paramarr, cardList, posBalance.ToString(), branchName, posName);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);

            rep.SetParameters(paramarr);

            rep.Refresh();
        }
        //private void BuildOperationReport()
        //{
        //    List<ReportParameter> paramarr = new List<ReportParameter>();

        //    string addpath = "";
        //    string firstTitle = "closing";//trDailyClosing
        //    string secondTitle = "";
        //    string subTitle = "";
        //    string Title = "";

        //    bool isArabic = ReportCls.checkLang();
        //    if (isArabic)
        //    {
        //        addpath = @"\Reports\StatisticReport\Accounts\Closing\Ar\ArClosOp.rdlc";

        //    }
        //    else
        //    {
        //        //english
        //        addpath = @"\Reports\StatisticReport\Accounts\Closing\En\EnClosOp.rdlc";
        //    }

        //    secondTitle = "operations";// trOperations
        //    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
        //    Title = MainWindow.resourcemanagerreport.GetString("trAccounting") + " / " + subTitle;
        //    paramarr.Add(new ReportParameter("trTitle", Title));

        //    string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

        //    ReportCls.checkLang();
        //    //  getpuritemcount
        //    //paramarr.Add(new ReportParameter("totalBalance", tb_total.Text));
        //    //  OpenClosOperatinModel
        //    clsReports.ClosingOpStsReport(opquery, rep, reppath, paramarr, openclosrow);
        //    clsReports.setReportLanguage(paramarr);
        //    clsReports.Header(paramarr);

        //    rep.SetParameters(paramarr);

        //    rep.Refresh();
        //}
        private async void printRowinDatagrid(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        POSOpenCloseModel row = (POSOpenCloseModel)dgClosing.SelectedItems[0];
                        cashTransID = row.cashTransId;
                        openCashTransID = row.openCashTransId.Value;
                        await getopquery(row);
                        await fillCashquery();
                        if (Boxquery.Count() == 0)
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoChange"), animation: ToasterAnimation.FadeIn);

                        }
                        else
                        {
                            await BuildOperationReport((int)row.posId);

                            LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

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
        private async void pdfRowinDatagrid(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        POSOpenCloseModel row = (POSOpenCloseModel)dgClosing.SelectedItems[0];
                        cashTransID = row.cashTransId;
                        openCashTransID = row.openCashTransId.Value;
                        await getopquery(row);
                        await fillCashquery();
                        if (Boxquery.Count() == 0)
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoChange"), animation: ToasterAnimation.FadeIn);

                        }
                        else
                        {
                            await BuildOperationReport((int)row.posId);

                            saveFileDialog.Filter = "PDF|*.pdf;";

                            if (saveFileDialog.ShowDialog() == true)
                            {
                                string filepath = saveFileDialog.FileName;
                                LocalReportExtensions.ExportToPDF(rep, filepath);
                            }
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
        private async void previewRowinDatagrid(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        POSOpenCloseModel row = (POSOpenCloseModel)dgClosing.SelectedItems[0];
                        cashTransID = row.cashTransId;
                        openCashTransID = row.openCashTransId.Value;
                        await getopquery(row);
                        await fillCashquery();
                        if (Boxquery.Count() == 0)
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoChange"), animation: ToasterAnimation.FadeIn);

                        }
                        else
                        {
                            string pdfpath = "";

                            pdfpath = @"\Thumb\report\temp.pdf";
                            pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                            await BuildOperationReport((int)row.posId);
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
        bool showChart = true;
        double col_reportChartWidth = 0;
        private async void excelRowinDatagrid(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        POSOpenCloseModel row = (POSOpenCloseModel)dgClosing.SelectedItems[0];
                        cashTransID = row.cashTransId;
                        openCashTransID = row.openCashTransId.Value;
                        await getopquery(row);
                        await fillCashquery();
                        //   await getopquery(row);
                        if (Boxquery.Count() == 0)
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoChange"), animation: ToasterAnimation.FadeIn);

                        }
                        else
                        {
                            await BuildOperationReport((int)row.posId);

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
        #endregion

        private void Btn_closing_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                txt_search.Text = "";
                searchText = "";
                dp_closingStartDate.SelectedDate = null;
                dp_closingEndDate.SelectedDate = null;
                chk_closingBranches.IsChecked = true;

                // key_up branch
                cb_closingBranches.IsTextSearchEnabled = false;
                cb_closingBranches.IsEditable = true;
                cb_closingBranches.StaysOpenOnEdit = true;
                cb_closingBranches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_closingBranches.Text = "";

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
