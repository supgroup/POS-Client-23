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

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_inoicesForReports.xaml
    /// </summary>
    public partial class wd_inoicesForReports : Window
    {
        public List<ItemTransferInvoice> invoiceLst;
        public List<Invoice> returnLst { get; set; }
        public string invType;
        public wd_inoicesForReports()
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

                RefreshView();

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
        private void RefreshView()
        {
            if (invType == "s" || invType == "p" || invType == "pw")
            {
                dg_invoices.ItemsSource = returnLst;
                dg_invoices.Columns[0].Header = MainWindow.resourcemanager.GetString("trNo.");
                txt_invoices.Text = MainWindow.resourcemanager.GetString("trReturns");
            }
            else if (invType == "sb" || invType == "pb")
            {
                dg_invoices.ItemsSource = invoiceLst;
                dg_invoices.Columns[0].Header = MainWindow.resourcemanager.GetString("trRefNo.");
                txt_invoices.Text = MainWindow.resourcemanager.GetString("ref");
            }
            tb_count.Text = dg_invoices.Items.Count.ToString();
        }
        private void translate()
        {
            txt_count.Text = MainWindow.resourcemanager.GetString("trCount");

            btn_colse.ToolTip = MainWindow.resourcemanager.GetString("trClose");
        }
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
    }
}
