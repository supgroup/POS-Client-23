using netoaster;
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
    /// Interaction logic for wd_submitOrder.xaml
    /// </summary>
    public partial class wd_submitOrder : Window
    {
        public wd_submitOrder()
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
        public Invoice invoice = new Invoice();
        private ItemLocation itemLocation = new ItemLocation();
        private List<ItemTransfer> invoiceItems;
        private async void Btn_select_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               int res = (int)await itemLocation.reReserveItems(invoiceItems, invoice.invoiceId, (int)invoice.branchId, MainWindow.userID.Value);
                if (res > 0)
                {
                    await fillOrderItems();
                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                }
                else
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
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
                if (e.Key == Key.Return)
                {
                    Btn_select_Click(null, null);
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucInvoice);

                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_ucInvoice.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_ucInvoice.FlowDirection = FlowDirection.RightToLeft;
                }
                translat();
                #endregion
                await fillOrderItems();
                if (sender != null)
                    SectionData.EndAwait(grid_ucInvoice);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucInvoice);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void translat()
        {
           txt_title.Text = MainWindow.resourcemanager.GetString("trReservation");

           dg_itemOrder.Columns[0].Header = MainWindow.resourcemanager.GetString("trItemUnit");        
            dg_itemOrder.Columns[1].Header = MainWindow.resourcemanager.GetString("trQTR");
            dg_itemOrder.Columns[2].Header = MainWindow.resourcemanager.GetString("trReserved");
            dg_itemOrder.Columns[3].Header = MainWindow.resourcemanager.GetString("trAvailable");

           btn_select.Content = MainWindow.resourcemanager.GetString("trSave");
        }
        private async Task fillOrderItems()
        {
           invoiceItems = await invoice.getOrderItems(invoice.invoiceId, (int)invoice.branchId);
            dg_itemOrder.ItemsSource = invoiceItems;
        }
        private void Dg_itemOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //invoice = dg_itemOrder.SelectedItem as Invoice;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Dg_Invoice_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Btn_select_Click(null, null);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
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

        private void Dg_itemOrder_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                TextBox t = e.EditingElement as TextBox;  // Assumes columns are all TextBoxes
                var columnName = e.Column.Header.ToString();

                ItemTransfer row = e.Row.Item as ItemTransfer;
                int index = invoiceItems.IndexOf(invoiceItems.Where(p => p.itemUnitId == row.itemUnitId).FirstOrDefault());
              
                long oldReserved = 0;
                long newReserved = 0;

                if(!t.Text.Equals(""))
                    newReserved = int.Parse(t.Text);
                oldReserved = (long) invoiceItems[index].lockedQuantity;
                if( newReserved > oldReserved && (newReserved - oldReserved) > row.availableQuantity)
                {
                    t.Text = oldReserved.ToString();
                    invoiceItems[index].newLocked = oldReserved;
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableFromToolTip"), animation: ToasterAnimation.FadeIn);
                }
                else if (newReserved > row.quantity)
                {
                    t.Text = oldReserved.ToString();
                    invoiceItems[index].newLocked = oldReserved;
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountIncreaseToolTip"), animation: ToasterAnimation.FadeIn);
                }
                else
                {
                    t.Text = newReserved.ToString();
                    invoiceItems[index].newLocked = newReserved;
                }
                refrishDataGridItems();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        void refrishDataGridItems()
        {
            dg_itemOrder.ItemsSource = null;
            dg_itemOrder.ItemsSource = invoiceItems;
            dg_itemOrder.Items.Refresh();
        }
    }
}
