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
using System.Resources;
using System.Reflection;

namespace POS.View.reports
{

    public partial class uc_destroied : UserControl
    {
        #region variables
        List<ItemTransferInvoice> Destroied;

        Statistics statisticModel = new Statistics();

        List<DestroiedCombo> comboDestroiedItemmsUnits;

        IEnumerable<ItemTransferInvoice> temp = null;
        #endregion

        private static uc_destroied _instance;
        public static uc_destroied Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_destroied();
                return _instance;
            }
        }
        public uc_destroied()
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
                cb_destroiedBranch.IsTextSearchEnabled = false;
                cb_destroiedBranch.IsEditable = true;
                cb_destroiedBranch.StaysOpenOnEdit = true;
                cb_destroiedBranch.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_destroiedBranch.Text = "";

                cb_destroiedItemsUnits.IsTextSearchEnabled = false;
                cb_destroiedItemsUnits.IsEditable = true;
                cb_destroiedItemsUnits.StaysOpenOnEdit = true;
                cb_destroiedItemsUnits.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_destroiedItemsUnits.Text = "";
                #endregion

                col_reportChartWidth = col_reportChart.ActualWidth;

                Destroied = await statisticModel.GetDesItems((int)MainWindow.branchID, (int)MainWindow.userID);
               
                await SectionData.fillBranchesWithoutMain(cb_destroiedBranch);

                Btn_destroied_Click(btn_destroied , null);

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
        private void fillEventsCall(object sender)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillDestroidEvents();

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
        private void translate()
        {
            tt_destroied.Content = MainWindow.resourcemanager.GetString("trDestructives");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_destroiedBranch, MainWindow.resourcemanager.GetString("trBranchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_destroiedItemsUnits, MainWindow.resourcemanager.GetString("trItemUnitHint"));

            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_destroiedStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_destroiedEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));

            chk_destroiedAllBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_destroiedAllItemsUnits.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_use.Header = MainWindow.resourcemanager.GetString("trDescription");
            col_destroiedNumber.Header = MainWindow.resourcemanager.GetString("trNom");
            col_destroiedDate.Header = MainWindow.resourcemanager.GetString("trDate");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_destroiedItemsUnits.Header = MainWindow.resourcemanager.GetString("trItemUnit");
            col_destroiedReason.Header = MainWindow.resourcemanager.GetString("trReason");
            col_destroiedAmount.Header = MainWindow.resourcemanager.GetString("trQTR");

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
        public void paint()
        {
            grid_detroied.Visibility = Visibility.Visible;

            bdr_destroied.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_destroied.Fill = Brushes.White;
        }
        private void fillDestroidEvents()
        {
            temp = fillListDestroied(cb_destroiedBranch, cb_destroiedItemsUnits, dp_destroiedStartDate, dp_destroiedEndDate);
            dgStock.ItemsSource = temp;
            txt_count.Text = temp.Count().ToString();

            fillDestroyColumnChart();
            fillDestroyRowChart();
            fillDestroyPieChart();
        }
        private void fillEmptyDestroidEvents()
        {
            temp = new List<ItemTransferInvoice>();
          //  temp = fillListDestroied(cb_destroiedBranch, cb_destroiedItemsUnits, dp_destroiedStartDate, dp_destroiedEndDate);
            dgStock.ItemsSource = temp;
            txt_count.Text = temp.Count().ToString();
            lst= new List<ItemTransferInvoice>();
            fillDestroyColumnChart();
            fillDestroyRowChart();
            fillDestroyPieChart();
        }
        List<DestroiedCombo> items = new List<DestroiedCombo>();
        private void fillComboItemsUnits()
        {
            var temp = cb_destroiedBranch.SelectedItem as Branch;
            cb_destroiedItemsUnits.SelectedValuePath = "ItemsUnitsId";
            cb_destroiedItemsUnits.DisplayMemberPath = "ItemsUnits";
            if (temp == null)
            {
                items = comboDestroiedItemmsUnits
                    .GroupBy(x => x.ItemsUnitsId)
                    .Select(g => new DestroiedCombo
                    {
                        ItemsUnits = g.FirstOrDefault().ItemsUnits,
                        BranchId = g.FirstOrDefault().BranchId,
                        ItemsUnitsId = g.FirstOrDefault().ItemsUnitsId
                    }).ToList();
                cb_destroiedItemsUnits.ItemsSource = items;
            }
            else
            {
                items = comboDestroiedItemmsUnits
                   .Where(x => x.BranchId == temp.branchId)
                    .GroupBy(x => x.ItemsUnitsId)
                    .Select(g => new DestroiedCombo
                    {
                        ItemsUnits = g.FirstOrDefault().ItemsUnits,
                        BranchId = g.FirstOrDefault().BranchId,
                        ItemsUnitsId = g.FirstOrDefault().ItemsUnitsId
                    }).ToList();
                cb_destroiedItemsUnits.ItemsSource = items;
            }
        }
        IEnumerable<ItemTransferInvoice> lst;
        private IEnumerable<ItemTransferInvoice> fillListDestroied(ComboBox branch, ComboBox cb, DatePicker startDate, DatePicker endDate)
        {
            var selectedBranch = branch.SelectedItem as Branch;
            var selectedType = cb.SelectedItem as DestroiedCombo;
            var result = Destroied.Where(x => (

                         (branch.SelectedItem != null ? (x.branchId == selectedBranch.branchId) : true)
                        && (cb.SelectedItem != null ? (x.itemUnitId == selectedType.ItemsUnitsId) : true)
                        && (startDate.SelectedDate != null ? (x.IupdateDate.Value.Date >= startDate.SelectedDate.Value.Date) : true)
                        && (endDate.SelectedDate != null ? (x.IupdateDate.Value.Date <= endDate.SelectedDate.Value.Date) : true)
          ));

            lst = result;
            return result;
        }
        #endregion

        #region events
        private void Cb_destroiedBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_destroiedBranch.SelectedItem != null)
                {
                    chk_destroiedAllItemsUnits.IsEnabled = true;
                    chk_destroiedAllItemsUnits.IsChecked = true;
                }
               
                fillComboItemsUnits();
                fillDestroidEvents();

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
        private void Chk_destroiedAllBranches_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_destroiedBranch.IsEnabled = true;

                cb_destroiedBranch.SelectedItem = null;
            

                cb_destroiedItemsUnits.IsEnabled = false;
                chk_destroiedAllItemsUnits.IsEnabled = false;
                chk_destroiedAllItemsUnits.IsChecked = false;
                fillEmptyDestroidEvents();

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
        private void Chk_destroiedAllBranches_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_destroiedBranch.SelectedItem = null;
                cb_destroiedBranch.IsEnabled = false;

                cb_destroiedItemsUnits.IsEnabled = false;
                chk_destroiedAllItemsUnits.IsEnabled = true;
                chk_destroiedAllItemsUnits.IsChecked = true;
                cb_destroiedBranch.Text = "";
                cb_destroiedBranch.ItemsSource = SectionData.BranchesAllWithoutMainList;

                fillDestroidEvents();
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
        private void Chk_destroiedAllItemsUnits_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_destroiedItemsUnits.SelectedItem = null;
                cb_destroiedItemsUnits.IsEnabled = false;
                fillDestroidEvents();
                cb_destroiedItemsUnits.Text = "";
                cb_destroiedItemsUnits.ItemsSource = items;
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
        private void Chk_destroiedAllItemsUnits_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_destroiedItemsUnits.IsEnabled = true;
                fillEmptyDestroidEvents();
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
        private void Btn_destroied_Click(object sender, RoutedEventArgs e)
        {//destroid
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                comboDestroiedItemmsUnits = statisticModel.getDestroiedCombo(Destroied);
                txt_search.Text = "";

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_destroied);
                path_destroied.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                dp_destroiedStartDate.SelectedDate = null;
                dp_destroiedEndDate.SelectedDate = null;
                chk_destroiedAllBranches.IsEnabled = true;
                chk_destroiedAllBranches.IsChecked = true;
             //   chk_destroiedAllItemsUnits.IsChecked = true;

              //  fillDestroidEvents();

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
            fillEventsCall(sender);
        }
        private void Dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            fillEventsCall(sender);
        }
        private void Txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                temp = temp
                    .Where(obj => (
                obj.branchName.ToLower().Contains(txt_search.Text.ToLower())
                ||
                obj.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                ||
                obj.quantity.ToString().ToLower().Contains(txt_search.Text.ToLower())
                ));

                dgStock.ItemsSource = temp;
                txt_count.Text = dgStock.Items.Count.ToString();

                fillDestroyColumnChart();
                fillDestroyRowChart();
                fillDestroyPieChart();

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

                Destroied = await statisticModel.GetDesItems((int)MainWindow.branchID, (int)MainWindow.userID);

                txt_search.Text = "";

                cb_destroiedBranch.SelectedItem = null;
                cb_destroiedItemsUnits.SelectedItem = null;

              
               // chk_destroiedAllItemsUnits.IsChecked = true;

                dp_destroiedStartDate.SelectedDate = null;
                dp_destroiedEndDate.SelectedDate = null;
                chk_destroiedAllBranches.IsChecked = true;
                chk_destroiedAllItemsUnits.IsChecked = true;
                fillListDestroied(cb_destroiedBranch, cb_destroiedItemsUnits, dp_destroiedStartDate, dp_destroiedEndDate);

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
        private void Cb_destroiedBranch_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = SectionData.BranchesAllWithoutMainList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_destroiedItemsUnits_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = items.Where(p => p.ItemsUnits.ToLower().Contains(tb.Text.ToLower())).ToList();
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
            //string firstTitle = "destroied";
            //string secondTitle = "";
            //string subTitle = "";
            string Title = "";

            string startDate = "";
            string endDate = "";
            string branchval = "";
            string itemval = "";

            string searchval = "";

            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");

            if (isArabic)
            {
                addpath = @"\Reports\StatisticReport\Storage\Destructive\Ar\ArDes.rdlc";
            }
            else
            {
                addpath = @"\Reports\StatisticReport\Storage\Destructive\En\Des.rdlc";
            }
            startDate = dp_destroiedStartDate.SelectedDate != null ? SectionData.DateToString(dp_destroiedStartDate.SelectedDate) : "";

            endDate = dp_destroiedEndDate.SelectedDate != null ? SectionData.DateToString(dp_destroiedStartDate.SelectedDate) : "";

            branchval = cb_destroiedBranch.SelectedItem != null
       && (chk_destroiedAllBranches.IsChecked == false || chk_destroiedAllBranches.IsChecked == null)
       ? cb_destroiedBranch.Text : (chk_destroiedAllBranches.IsChecked == true ? all : "");

            itemval = cb_destroiedItemsUnits.SelectedItem != null
               && (chk_destroiedAllItemsUnits.IsChecked == false || chk_destroiedAllItemsUnits.IsChecked == null)
               && branchval != ""
               ? cb_destroiedItemsUnits.Text : (chk_destroiedAllItemsUnits.IsChecked == true && branchval != "" ? all : "");


            // secondTitle = "destroied";
            //  subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trStorageReport") + " / " + MainWindow.resourcemanagerreport.GetString("trDestructives");
            paramarr.Add(new ReportParameter("trTitle", Title));


            paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItemUnit")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("BranchVal", branchval));
            paramarr.Add(new ReportParameter("ItemVal", itemval));

            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDateHint", MainWindow.resourcemanagerreport.GetString("trStartDate")));

            paramarr.Add(new ReportParameter("trEndDateHint", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            // end filter
            paramarr.Add(new ReportParameter("trDescription", MainWindow.resourcemanagerreport.GetString("trDescription")));
            paramarr.Add(new ReportParameter("trNom", MainWindow.resourcemanagerreport.GetString("trNom")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trReason", MainWindow.resourcemanagerreport.GetString("trReason")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();

            clsReports.itemTransferInvoiceDestroied(temp, rep, reppath, paramarr);
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

        #region charts
        private void fillDestroyRowChart()
        {
            List<long> cP = new List<long>();

            MyAxis.Labels = new List<string>();

            List<string> names = new List<string>();

            //var temp = fillListDestroied(cb_destroiedBranch, cb_destroiedItemsUnits, dp_destroiedStartDate, dp_destroiedEndDate);

            var result = temp.GroupBy(s => new { s.itemUnitId }).Select(s => new ItemTransferInvoice
            //var result = lst.GroupBy(s => new { s.itemUnitId }).Select(s => new ItemTransferInvoice
            {
                branchId = s.FirstOrDefault().branchId,
                branchName = s.FirstOrDefault().branchName,
                quantity = s.Sum(x => x.quantity),
                ItemUnits = s.FirstOrDefault().ItemUnits,
                itemUnitId = s.FirstOrDefault().itemUnitId,
                itemName = s.FirstOrDefault().itemName,
                unitName = s.FirstOrDefault().unitName
            });
            var tempName = result.GroupBy(s => new { s.itemUnitId }).Select(s => new
            {
                itemName = s.FirstOrDefault().itemName + s.FirstOrDefault().unitName,
            });
            names.AddRange(tempName.Select(nn => nn.itemName));
            for (int i = 0; i < result.Count(); i++)
            {
                cP.Add(long.Parse(result.ToList().Skip(i).FirstOrDefault().quantity.ToString()));
                MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            SeriesCollection rowChartData = new SeriesCollection();

            rowChartData.Add(
             new LineSeries
             {
                 Values = cP.AsChartValues(),

                 DataLabels = true,
             });
            DataContext = this;
            rowChart.Series = rowChartData;

        }
        private void fillDestroyColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();

            //var temp = fillListDestroied(cb_destroiedBranch, cb_destroiedItemsUnits, dp_destroiedStartDate, dp_destroiedEndDate);

            var result = temp.GroupBy(s => new { s.branchId }).Select(s => new ItemTransferInvoice
            //var result = lst.GroupBy(s => new { s.branchId }).Select(s => new ItemTransferInvoice
            {
                branchId = s.FirstOrDefault().branchId,
                branchName = s.FirstOrDefault().branchName,
                quantity = s.Sum(x => x.quantity),
            });

            var tempName = result.GroupBy(s => new { s.branchId }).Select(s => new
            {
                itemName = s.FirstOrDefault().branchName,
            });
            names.AddRange(tempName.Select(nn => nn.itemName));

            SeriesCollection columnChartData = new SeriesCollection();
            List<long> cPa = new List<long>();

            int xCount = 6;
            if (result.Count() <= 6) xCount = result.Count();

            for (int i = 0; i < xCount; i++)
            {
                cPa.Add(long.Parse(result.ToList().Skip(i).FirstOrDefault().quantity.ToString()));
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            if (result.Count() > 6)
            {
                long c = 0;
                for (int i = 6; i < result.Count(); i++)
                {
                    c = c + long.Parse(result.ToList().Skip(i).FirstOrDefault().quantity.ToString());
                }
                if (c != 0)
                {
                    cPa.Add(c);
                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cPa.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("trAmount")
            });


            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillDestroyPieChart()
        {
            List<string> titles = new List<string>();
            List<long> cP = new List<long>();

            titles.Clear();
            //var temp = fillListDestroied(cb_destroiedBranch, cb_destroiedItemsUnits, dp_destroiedStartDate, dp_destroiedEndDate);

            var result = temp.GroupBy(s => new { s.itemUnitId }).Select(s => new ItemTransferInvoice
            //var result = lst.GroupBy(s => new { s.itemUnitId }).Select(s => new ItemTransferInvoice
            {
                branchId = s.FirstOrDefault().branchId,
                branchName = s.FirstOrDefault().branchName,
                quantity = s.Sum(x => x.quantity),
                ItemUnits = s.FirstOrDefault().ItemUnits,
                itemUnitId = s.FirstOrDefault().itemUnitId,
                itemName = s.FirstOrDefault().itemName,
                unitName = s.FirstOrDefault().unitName
            });
            var tempName = result.GroupBy(s => new { s.itemUnitId }).Select(s => new
            {
                itemName = s.FirstOrDefault().itemName + s.FirstOrDefault().unitName,
            });
            titles.AddRange(tempName.Select(nn => nn.itemName));
            cP = result.Select(m => (long)m.quantity).ToList();
            int count = cP.Count();
            SeriesCollection piechartData = new SeriesCollection();
            for (int i = 0; i < count; i++)
            {
                List<decimal> final = new List<decimal>();
                List<string> lable = new List<string>();
                if (i < 5)
                {
                    final.Add(cP.Max());
                    lable.Add(titles.Skip(i).FirstOrDefault());
                    piechartData.Add(
                      new PieSeries
                      {
                          Values = final.AsChartValues(),
                          Title = lable.FirstOrDefault(),
                          DataLabels = true,
                      }
                  );
                    cP.Remove(cP.Max());
                }
                else
                {
                    final.Add(cP.Sum());
                    piechartData.Add(
                      new PieSeries
                      {
                          Values = final.AsChartValues(),
                          Title = MainWindow.resourcemanager.GetString("trOthers"),
                          DataLabels = true,
                      }
                  );
                    break;
                }

            }
            chart1.Series = piechartData;
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
