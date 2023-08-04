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
    public partial class uc_accountant : UserControl
    {
        private static uc_accountant _instance;

        public static uc_accountant Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_accountant();
                return _instance;
            }
        }
        public uc_accountant()
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
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_ucAccountant.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_ucAccountant.FlowDirection = FlowDirection.RightToLeft;
                }
                translate();
                #endregion

                if ((!AppSettings.invoiceTax_bool.Value) && (!AppSettings.itemsTax_bool.Value))
                    bdr_taxSales.Visibility = Visibility.Collapsed;
                else
                    bdr_taxSales.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void translate()
        {
            txt_paymentsInfo.Text = MainWindow.resourcemanager.GetString("trPayments");
            txt_paymentsHint.Text = MainWindow.resourcemanager.GetString("trVendorCustomerUserHint");
            txt_recipientInfo.Text = MainWindow.resourcemanager.GetString("trReceived");
            txt_recipientHint.Text = MainWindow.resourcemanager.GetString("trVendorCustomerUserHint");
            txt_bankInfo.Text = MainWindow.resourcemanager.GetString("trBanks");
            txt_bankHint.Text = MainWindow.resourcemanager.GetString("trPaymentsRecipientHint");
            txt_posInfo.Text = MainWindow.resourcemanager.GetString("trTransfers");
            txt_posHint.Text = MainWindow.resourcemanager.GetString("trFromToPosHint");
            txt_statementInfo.Text = MainWindow.resourcemanager.GetString("trAccountStatement");
            txt_statementHint.Text = MainWindow.resourcemanager.GetString("trVendorCustomerUserHint");
            ///////////////
            txt_fundInfo.Text = MainWindow.resourcemanager.GetString("trCashBalance");
            txt_fundHint.Text = MainWindow.resourcemanager.GetString("trBranchPosHint");
            txt_profitInfo.Text = MainWindow.resourcemanager.GetString("trProfits");
            txt_profitHint.Text = MainWindow.resourcemanager.GetString("trInvoiceItemHint");

            txt_taxSalesInfo.Text = MainWindow.resourcemanager.GetString("trTax");
            txt_taxSalesHint.Text = MainWindow.resourcemanager.GetString("trInvoice") + ", " + MainWindow.resourcemanager.GetString("trItems") + "...";

            txt_closingInfo.Text = MainWindow.resourcemanager.GetString("trDailyClosing");
            txt_closingHint.Text = MainWindow.resourcemanager.GetString("trBranchHint") + ", " + MainWindow.resourcemanager.GetString("trPosHint") + "...";

            txt_commisionInfo.Text = MainWindow.resourcemanager.GetString("commission");
            txt_commisionHint.Text = MainWindow.resourcemanager.GetString("paymentAgent") + ", " + MainWindow.resourcemanager.GetString("salesEmployee") + "...";
        }

        private void Btn_payments_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_paymentsReport uc = new uc_paymentsReport();
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

        private void Btn_recipient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_recipientReport uc = new uc_recipientReport();
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
            try
            {
                uc_banksReport uc = new uc_banksReport();
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

        private void Btn_pos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_posReports uc = new uc_posReports();
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

        private void Btn_statement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_accountStatement uc = new uc_accountStatement();
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

        private void Btn_fund_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_accountFund uc = new uc_accountFund();
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

        private void Btn_profit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_accountProfits uc = new uc_accountProfits();
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
        private void Btn_closing_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_accountClosing uc = new uc_accountClosing();
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


        private void Btn_taxSales_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                us_saleTax uc = new us_saleTax();
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
        private void Btn_commision_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_accountCommision uc = new uc_accountCommision();
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

       
    }
}
