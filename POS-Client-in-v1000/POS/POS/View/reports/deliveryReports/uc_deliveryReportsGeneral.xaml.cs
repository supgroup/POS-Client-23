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

namespace POS.View.reports.deliveryReports
{
    /// <summary>
    /// Interaction logic for uc_deliveryReportsGeneral.xaml
    /// </summary>
    public partial class uc_deliveryReportsGeneral : UserControl
    {
        private static uc_deliveryReportsGeneral _instance;

        public static uc_deliveryReportsGeneral Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_deliveryReportsGeneral();
                return _instance;
            }
        }
        public uc_deliveryReportsGeneral()
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

        private void Btn_invoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_deliveryReports uc = new uc_deliveryReports();
                sc_main.Visibility = Visibility.Collapsed;
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_invoicesStatuses_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_deliveryOrderStatus uc = new uc_deliveryOrderStatus();
                sc_main.Visibility = Visibility.Collapsed;
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_bank_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_promotion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_salePromotion uc = new uc_salePromotion();
                sc_main.Visibility = Visibility.Collapsed;
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_order_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_saleOrders uc = new uc_saleOrders();
                sc_main.Visibility = Visibility.Collapsed;
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_quotation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_saleQuotation uc = new uc_saleQuotation();
                sc_main.Visibility = Visibility.Collapsed;
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_dailySales_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                us_dailySalesStatistic uc = new us_dailySalesStatistic();
                sc_main.Visibility = Visibility.Collapsed;
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
            }
            catch (Exception ex)
            {
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_ucSales.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_ucSales.FlowDirection = FlowDirection.RightToLeft;
                }
                translate();
                #endregion



            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void translate()
        {
            txt_invoiceInfo.Text = MainWindow.resourcemanager.GetString("trInvoice");
            txt_invoiceHint.Text = MainWindow.resourcemanager.GetString("trShippingCompanies") + ", " +
                                   MainWindow.resourcemanager.GetString("trCustomer") + "...";
            txt_invoicesStatusesInfo.Text = MainWindow.resourcemanager.GetString("orderStatus");
            txt_invoicesStatusesHint.Text = MainWindow.resourcemanager.GetString("trReady") + ", " +
                                   MainWindow.resourcemanager.GetString("onTheWay") + "...";
        }
    }
}
