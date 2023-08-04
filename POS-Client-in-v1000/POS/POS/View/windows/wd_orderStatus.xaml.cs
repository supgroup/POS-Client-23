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
using System.Windows.Shapes;

using System.IO;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_orderStatus.xaml
    /// </summary>
    public partial class wd_orderStatus : Window
    {
        #region variables
        public List<invoiceStatus> orders;
        public Invoice invoice;
        //prin & pdf
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        #endregion

        public wd_orderStatus()
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
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

                dg_status.ItemsSource = orders;
                txt_count.Text = dg_status.Items.Count.ToString();

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
            txt_status.Text = MainWindow.resourcemanager.GetString("invoiceStatus");

            dg_status.Columns[0].Header = MainWindow.resourcemanager.GetString("trStatus");
            dg_status.Columns[1].Header = MainWindow.resourcemanager.GetString("trDate");
            dg_status.Columns[2].Header = MainWindow.resourcemanager.GetString("trTime");

            btn_colse.ToolTip = MainWindow.resourcemanager.GetString("trClose");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
        }
        #endregion

        #region events
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch //(Exception ex)
            {
                //SectionData.ExceptionMessage(ex, this);
            }
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
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
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {//close
            this.Close();
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

            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\StatisticReport\Delivery\Ar\ArOrderAllStat.rdlc";
            }
            else
            {
                addpath = @"\Reports\StatisticReport\Delivery\En\OrderAllStat.rdlc";
            }
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            //secondTitle = "";
            // subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);

            secondTitle = MainWindow.resourcemanagerreport.GetString("orderStatus");
            Title = MainWindow.resourcemanagerreport.GetString("deliveryReport") + " / " + secondTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            paramarr.Add(new ReportParameter("invNum", invoice.invNumber));
            
            clsReports.DeliveryAllStatReport(orders.ToList(), rep, reppath, paramarr);
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
    }
}
