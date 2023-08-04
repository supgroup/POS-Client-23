using netoaster;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for wd_invoice.xaml
    /// </summary>
    public partial class wd_invoice : Window
    {
        public wd_invoice()
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
        /// <summary>
        /// for filtering store
        /// </summary>
        public Invoice invoice = new Invoice();
        //IEnumerable<Invoice> FillCombo.invoices;
        public int posId { get; set; }
        public int branchId { get; set; }
        public int branchCreatorId { get; set; }
        public int userId { get; set; }
        /// <summary>
        /// for filtering invoice type
        /// </summary>
        public string invoiceType { get; set; }
        public string invoiceStatus { get; set; }
        public string title { get; set; }
        public string condition { get; set; }
        public bool fromOrder = false;
        public int duration { get; set; }
        public int invoiceId { get; set; }
        private void Btn_select_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                invoice = dg_Invoice.SelectedItem as Invoice;
                DialogResult = true;
                this.Close();
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

        private void Txb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            { 
                dg_Invoice.ItemsSource = FillCombo.invoices.Where(x => x.invNumber.ToLower().Contains(txb_search.Text.ToLower())).ToList();
                txt_count.Text = dg_Invoice.Items.Count.ToString() ;

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
                txt_Invoices.Text = title;
                translat();
                #endregion
                dg_Invoice.Columns[0].Visibility = Visibility.Collapsed;
                List<string> invTypeL = invoiceType.Split(',').ToList();
                bool inCommen = false;
                #region hide Total column in grid if invoice is import/export order/purchase order

                string[] invTypeArray = new string[] { "imd" ,"exd","im","ex" ,"exw","pod","po" };
                var invTypes = invTypeArray.ToList();
               
                inCommen = invTypeL.Any(s => invTypes.Contains(s));
                if(inCommen)
                    col_total.Visibility = Visibility.Collapsed; //make total column unvisible
                #endregion
               
                #region display branch & user columns in grid if invoice is sales order and purchase orders
                invTypeArray = new string[] { "or" };
                invTypes = invTypeArray.ToList();
                inCommen = invTypeL.Any(s => invTypes.Contains(s));
                if (inCommen)
                {
                    col_agent.Header = MainWindow.resourcemanager.GetString("trCustomer");                                      
                    col_agent.Visibility = Visibility.Visible;
                    if (fromOrder == false)
                    {
                        col_branch.Visibility = Visibility.Visible; //make branch column visible
                        col_user.Visibility = Visibility.Visible; //make user column visible
                    }
                    //dg_Invoice.Columns[7].Visibility = Visibility.Visible; //make user column visible
                }
                #endregion
                #region display branch, vendor & user columns in grid if invoice is  purchase orders
                if (invoiceType == "po" && fromOrder == false)
                {
                    col_agent.Header = MainWindow.resourcemanager.GetString("trVendor");
                    col_branch.Visibility = Visibility.Visible; //make branch column visible
                    col_user.Visibility = Visibility.Visible; //make user column visible
                    col_agent.Visibility = Visibility.Visible;
                }
                #endregion
                #region display branch if invoice is export or import process
                invTypeArray = new string[] { "exw" ,"im","ex" };
                invTypes = invTypeArray.ToList();
                inCommen = invTypeL.Any(s => invTypes.Contains(s));
                if (inCommen)
                    col_branch.Visibility = Visibility.Visible; //make total column unvisible
                #endregion

                await refreshInvoices();
                Txb_search_TextChanged(null, null);

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
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txb_search, MainWindow.resourcemanager.GetString("trSearchHint"));

            col_num.Header = MainWindow.resourcemanager.GetString("trCharp");
            col_updateDate.Header = MainWindow.resourcemanager.GetString("UpdateDate");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_user.Header = MainWindow.resourcemanager.GetString("trUser");
            col_count.Header = MainWindow.resourcemanager.GetString("trCount");
            col_total.Header = MainWindow.resourcemanager.GetString("trTotal");
            col_type.Header = MainWindow.resourcemanager.GetString("trType");
            col_agent.Header = MainWindow.resourcemanager.GetString("trVendor");

            txt_countTitle.Text = MainWindow.resourcemanager.GetString("trCount") + ":";

            btn_select.Content = MainWindow.resourcemanager.GetString("trSelect");

            if(condition == "exportImport")
                col_branch.Header = MainWindow.resourcemanager.GetString("trToBranch");
            else if (condition == "export")
                col_branch.Header = MainWindow.resourcemanager.GetString("trFromBranch");

        }
        private async Task refreshInvoices()
        {
            if (condition == "salesOrders") 
            {
                FillCombo.invoices = await invoice.getWaitingOrders(invoiceType,branchCreatorId, branchId,duration,userId);
            }
            else if (condition == "orders") 
            {
                FillCombo.invoices = await invoice.getUnHandeldOrders(invoiceType,branchCreatorId, branchId,duration,userId);
            }
            else if (condition == "export")
            {
                FillCombo.invoices = await invoice.getExportInvoices(invoiceType, branchId);
            }
            else if (condition == "exportImport")
            {
                FillCombo.invoices = await invoice.getExportImportInvoices(invoiceType, branchId,duration);
            }
            else if(condition == "return")
                FillCombo.invoices = await invoice.getInvoicesToReturn(invoiceType, userId);
            else if(condition == "salesInv")
            {
                var isAdmin = SectionData.isAdminPermision();
                FillCombo.invoices = await invoice.GetSalesInvoices(invoiceType, duration,isAdmin,userId);
            }
            else if(condition == "invoiceArchive")
            {
                FillCombo.invoices = await invoice.GetInvoiceArchive( invoiceId);
            }
            else if(condition == "admin")
                FillCombo.invoices = await invoice.GetInvoicesForAdmin(invoiceType, duration);
            else
            {
                if (userId != 0 && (invoiceStatus == "" || invoiceStatus == null))/// to display draft invoices /
                    FillCombo.invoices = await invoice.GetInvoicesByCreator(invoiceType, userId, duration);
                else if (branchId != 0 && branchCreatorId != 0) // to get FillCombo.invoices to make return from it
                    FillCombo.invoices = await invoice.getBranchInvoices(invoiceType, branchCreatorId, branchId);
                else if (branchCreatorId != 0)
                    FillCombo.invoices = await invoice.getBranchInvoices(invoiceType, branchCreatorId);
                else if (invoiceStatus != "" && branchId != 0) // get return invoice in storage
                    FillCombo.invoices = await invoice.getBranchInvoices(invoiceType, branchCreatorId, branchId,duration);
                else if (branchId != 0) // get export/ import orders
                    FillCombo.invoices = await invoice.GetOrderByType(invoiceType, branchId);
                else if (invoiceStatus != "" && userId != 0) // get sales FillCombo.invoices to get deliver accept on it
                    FillCombo.invoices = await invoice.getDeliverOrders(invoiceType, invoiceStatus, userId);
                else
                    FillCombo.invoices = await invoice.GetInvoicesByType(invoiceType, branchId);
            }
            

        }
        private void Dg_Invoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                invoice = dg_Invoice.SelectedItem as Invoice;
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
                Btn_select_Click(null,null);
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
        private async void deleteRowFromInvoiceItems(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucInvoice);
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        #region
                        Window.GetWindow(this).Opacity = 0.2;
                        wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                        w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                        w.ShowDialog();
                        Window.GetWindow(this).Opacity = 1;
                        #endregion
                        if (w.isOk)
                        {
                            Invoice row = (Invoice)dg_Invoice.SelectedItems[0];
                            int res = 0;
                            //if (row.invType == "or")
                            //    res = await invoice.deleteOrder(row.invoiceId);
                            //else
                            //    res = await invoice.deleteInvoice(row.invoiceId);
                            //if (res > 0)
                            //{
                            //    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);
                            //    await refreshInvoices();
                            //    Txb_search_TextChanged(null,null);
                            //}
                            //else
                            //    Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        }
                    }

                if (sender != null)
                    SectionData.EndAwait(grid_ucInvoice);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucInvoice);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
