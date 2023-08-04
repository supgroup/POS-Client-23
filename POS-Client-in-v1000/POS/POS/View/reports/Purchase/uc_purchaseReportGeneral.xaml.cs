using POS.Classes;
using POS.View.purchases;
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
    /// Interaction logic for uc_purchaseReportGeneral.xaml
    /// </summary>
    public partial class uc_purchaseReportGeneral : UserControl
    {

        private static uc_purchaseReportGeneral _instance;

        public static uc_purchaseReportGeneral Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_purchaseReportGeneral();
                return _instance;
            }
        }
        public uc_purchaseReportGeneral()
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
                uc_purchaseReport uc = new uc_purchaseReport();
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
                uc_purchaseItem uc = new uc_purchaseItem();
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
                uc_purchaseOrders uc = new uc_purchaseOrders();
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

        private void Btn_itemCost_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                uc_purchaseItemsCost uc = new uc_purchaseItemsCost();
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
        {
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
            txt_invoiceHint.Text = MainWindow.resourcemanager.GetString("trBranch")+", "+ MainWindow.resourcemanager.GetString("trPOS")+", "+ 
                                   MainWindow.resourcemanager.GetString("trCustomer")+"...";
            txt_itemInfo.Text = MainWindow.resourcemanager.GetString("trItems");
            txt_itemHint.Text = MainWindow.resourcemanager.GetString("trItems")+"...";
            txt_orderInfo.Text = MainWindow.resourcemanager.GetString("trOrders");
            txt_orderHint.Text = MainWindow.resourcemanager.GetString("trBranch") + ", " + MainWindow.resourcemanager.GetString("trPOS") + ", " +
                                 MainWindow.resourcemanager.GetString("trCustomer") + "...";
            txt_itemCostInfo.Text = MainWindow.resourcemanager.GetString("trItemsCost");
            txt_itemCostHint.Text = MainWindow.resourcemanager.GetString("trItem") + ", " + MainWindow.resourcemanager.GetString("trUnit") + "...";
        }

    }
}
