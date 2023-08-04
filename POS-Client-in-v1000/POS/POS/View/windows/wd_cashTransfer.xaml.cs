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
using static POS.Classes.FillCombo;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_cashTransfer.xaml
    /// </summary>
    public partial class wd_cashTransfer : Window
    {
        public wd_cashTransfer()
        {
            try
            {
                InitializeComponent();
            }
            catch(Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public int invId;
        //public Invoice childInvoice;

        public string invType;
        public decimal invPaid;
        public decimal invTotal;
        public string title;
        public UserControls sourceUserControls;
        CashTransfer cashModel = new CashTransfer();
        IEnumerable<CashTransfer> cashes;
        IEnumerable<CashTransfer> cashesQuery; 
        string searchText = "";
        private  void Window_Loaded(object sender, RoutedEventArgs e)
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

                Txb_search_TextChanged(null, null);

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
            txt_accounts.Text = title;
            txt_paid.Text = MainWindow.resourcemanager.GetString("trCashPaid");
            txt_notPaid.Text = MainWindow.resourcemanager.GetString("trUnPaid");
            txt_total.Text = MainWindow.resourcemanager.GetString("trOf") ;
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            dg_accounts.Columns[0].Header = MainWindow.resourcemanager.GetString("trTransferNumberTooltip");
            dg_accounts.Columns[1].Header = MainWindow.resourcemanager.GetString("trDate");
            dg_accounts.Columns[2].Header = MainWindow.resourcemanager.GetString("trPaymentTypeTooltip");
            dg_accounts.Columns[3].Header = MainWindow.resourcemanager.GetString("trCashTooltip");
            btn_pay.Content = MainWindow.resourcemanager.GetString("trPay");
            btn_colse.ToolTip = MainWindow.resourcemanager.GetString("trClose");
        }

        async Task<IEnumerable<CashTransfer>> RefreshCashesList()
        {
            //if((sourceUserControls == FillCombo.UserControls.receiptInvoice && invType.Equals("s"))
            //    || (sourceUserControls == FillCombo.UserControls.payInvoice && invType.Equals("p")))
            //    cashes = await cashModel.GetInvAndReturn(invId);

            //else
             cashes = await cashModel.GetListByInvId(invId);

            cashes = cashes.Where(x => x.processType !="balance").ToList();
            return cashes;

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

        private async void Txb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cashes is null)
                    await RefreshCashesList();

                this.Dispatcher.Invoke(() =>
                {
                    searchText = txb_search.Text.ToLower();
                    cashesQuery = cashes.Where(s => (s.transNum.ToLower().Contains(searchText)
                                                  || s.cash.ToString().ToLower().Contains(searchText)));

                });

                RefreshCashView();

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

        void RefreshCashView()
        {
            dg_accounts.ItemsSource = cashesQuery;

            //if (((sourceUserControls == FillCombo.UserControls.receiptInvoice && invType.Equals("s"))
            //    || (sourceUserControls == FillCombo.UserControls.payInvoice && invType.Equals("p"))) 
            //    && childInvoice != null)
            //{
            //    txt_return.Visibility = Visibility.Visible;
            //    tb_return.Visibility = Visibility.Visible;
            //    decimal returnVal = 0;
            //    foreach (var cash in cashesQuery)
            //    {
            //        if (cash.notes.Equals("return"))
            //            returnVal += (decimal)cash.cash;
            //    }
            //    tb_return.Text = SectionData.DecTostring(returnVal * -1);

            //}
            //else
            //{
            //    txt_return.Visibility = Visibility.Collapsed;
            //    tb_return.Visibility = Visibility.Collapsed;
            //}

            //if(invType.Equals("sb") || invType.Equals("pb")|| invType.Equals("pbw"))
            //{
            //    invPaid = 0;
            //    foreach (var cash in cashes)
            //    {
            //        invPaid += (decimal)cash.cash;
            //    }
            //}

            invPaid = 0;
            foreach (var cash in cashes)
            {
                invPaid += (decimal)cash.cash;
            }

            if(invPaid < invTotal)
            {
                txt_notPaid.Visibility = Visibility.Visible;
                tb_notPaid.Visibility = Visibility.Visible;

                decimal notPaid = invTotal - invPaid;
                tb_notPaid.Text = SectionData.DecTostring(notPaid);
            }
            tb_paid.Text = SectionData.DecTostring( invPaid);
            tb_total.Text = SectionData.DecTostring(invTotal); 
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (e.Key == Key.Return)
                {
                    Btn_pay_Click(null, null);
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

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {//close
            this.Close();
        }

        private void Btn_pay_Click(object sender, RoutedEventArgs e)
        {//pay
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //
                //enter your code here
                //

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
    }
}
