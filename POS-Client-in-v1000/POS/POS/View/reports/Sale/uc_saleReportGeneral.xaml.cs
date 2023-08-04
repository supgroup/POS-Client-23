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

namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_saleReportGeneral.xaml
    /// </summary>
    public partial class uc_saleReportGeneral : UserControl
    {
        private static uc_saleReportGeneral _instance;

        public static uc_saleReportGeneral Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_saleReportGeneral();
                return _instance;
            }
        }
        public uc_saleReportGeneral()
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
                uc_saleReport uc = new uc_saleReport();
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

        private void Btn_item_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_saleItems uc = new uc_saleItems();
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
            txt_invoiceHint.Text = MainWindow.resourcemanager.GetString("trBranch") + ", " + MainWindow.resourcemanager.GetString("trPOS") + ", " +
                                   MainWindow.resourcemanager.GetString("trCustomer") + "...";
            txt_itemInfo.Text = MainWindow.resourcemanager.GetString("trItems");
            txt_itemHint.Text = MainWindow.resourcemanager.GetString("trItems") + "...";
            txt_promotionInfo.Text = MainWindow.resourcemanager.GetString("trPromotion");
            txt_promotionHint.Text = MainWindow.resourcemanager.GetString("trCoupon") + ", " + MainWindow.resourcemanager.GetString("trOffer") + ", " +
                                     MainWindow.resourcemanager.GetString("trPackage") + "...";
            txt_orderInfo.Text = MainWindow.resourcemanager.GetString("trOrders");
            txt_orderHint.Text = MainWindow.resourcemanager.GetString("trBranch") + ", " + MainWindow.resourcemanager.GetString("trPOS") + ", " +
                                 MainWindow.resourcemanager.GetString("trCustomer") + "...";
            txt_quotationInfo.Text = MainWindow.resourcemanager.GetString("trQuotation");
            txt_quotationHint.Text = MainWindow.resourcemanager.GetString("trBranch") + ", " + MainWindow.resourcemanager.GetString("trPOS") + ", " +
                                 MainWindow.resourcemanager.GetString("trCustomer") + "...";
            txt_dailySalesInfo.Text = MainWindow.resourcemanager.GetString("trDaily");
            txt_dailySalesHint.Text = MainWindow.resourcemanager.GetString("trDailySales")+"...";

        }

       
    }
}
