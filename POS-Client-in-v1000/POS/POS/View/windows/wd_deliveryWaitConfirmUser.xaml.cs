using netoaster;
using POS.Classes;
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
using System.Windows.Shapes;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_deliveryWaitConfirmUser.xaml
    /// </summary>
    public partial class wd_deliveryWaitConfirmUser : Window
    {
        public wd_deliveryWaitConfirmUser()
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
        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                //if (e.Key == Key.Return)
                //{
                //    Btn_select_Click(null, null);
                //}
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
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
                translat();
                #endregion

                await fillDataGrid();

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
        private void translat()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trWaitConfirmUser");

            #region data grid
            col_invNum.Header = MainWindow.resourcemanager.GetString("trCharp");
            col_customer.Header = MainWindow.resourcemanager.GetString("trCustomer");
            col_itemsCount.Header = MainWindow.resourcemanager.GetString("trQTR");
            #endregion
        }

        public Invoice invoice = new Invoice();
        IEnumerable<Invoice> invoices;
        async Task fillDataGrid()
        {
            string invoiceType = "s";
            //string invoiceStatus = "ex";
            string invoiceStatus = "Ready";
            invoices = await invoice.getDeliverOrders(invoiceType, invoiceStatus, MainWindow.userID.Value);

           dg_invoice.ItemsSource = invoices;
        }
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

        #region Button In DataGrid

        async void confirmRowinDatagrid(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
              
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        #region
                        Window.GetWindow(this).Opacity = 0.2;
                        wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                        w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxConfirm");
                        w.ShowDialog();
                        Window.GetWindow(this).Opacity = 1;
                        #endregion
                        if (w.isOk)
                        {
                            // Pos posModel = new Pos();
                            Invoice row = (Invoice)dg_invoice.SelectedItems[0];
                            //await saveOrderStatus(row.invoiceId, "tr");
                            await saveOrderStatus(row.invoiceId, "Collected");
                            await fillDataGrid();
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
        private async Task saveOrderStatus(int invoiceId, string status)
        {
            invoiceStatus st = new invoiceStatus();
            st.status = status;
            st.invoiceId = invoiceId;
            st.createUserId = MainWindow.userLogin.userId;
            st.isActive = 1;
            int res = (int)await invoice.saveOrderStatus(st);
            if (res > 0)
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopConfirm"), animation: ToasterAnimation.FadeIn);
            else
                Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
        }
        #endregion
    }
}
