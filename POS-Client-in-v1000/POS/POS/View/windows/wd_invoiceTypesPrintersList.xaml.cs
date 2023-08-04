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
    /// Interaction logic for wd_invoiceTypesPrintersList.xaml
    /// </summary>
    public partial class wd_invoiceTypesPrintersList : Window
    {
        public int printerId;
        public bool isActive;
        public string paperSizeValue;

        IEnumerable<InvoiceTypes> allInvoiceTypesSource;
        IEnumerable<InvoiceTypes> selectedInvoiceTypesSource;
        List<InvoiceTypes> allInvoiceTypes = new List<InvoiceTypes>();
        List<InvoiceTypes> invoiceTypesQuery = new List<InvoiceTypes>();
        public List<InvoiceTypes> selectedInvoiceTypes = new List<InvoiceTypes>();
        InvoiceTypes invoiceTypesModel = new InvoiceTypes();
        InvoiceTypes invoiceTypes = new InvoiceTypes();

        #region date parameters to get invoiceTypes satatus
        public DateTime invDate;
        #endregion

        string searchText = "";

        public wd_invoiceTypesPrintersList()
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
                if (e.Key == Key.Return)
                {
                    Btn_save_Click(null, null);
                }
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
                SectionData.StartAwait(grid_invoiceTypesList);

                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    grid_invoiceTypesList.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    grid_invoiceTypesList.FlowDirection = FlowDirection.RightToLeft;
                }

                translat();
                #endregion

                if (FillCombo.invoiceTypessList is null)
                   await FillCombo.RefreshInvoiceTypess();
                    allInvoiceTypesSource = FillCombo.invoiceTypessList.ToList();

                allInvoiceTypes.AddRange(allInvoiceTypesSource.ToList());

                selectedInvoiceTypesSource = await invoiceTypesModel.GetTypesOfPrinter(printerId);
                selectedInvoiceTypes.AddRange(selectedInvoiceTypesSource.ToList());

                #region


                //remove selected items from all items
                foreach (var i in selectedInvoiceTypes)
                {
                    invoiceTypes = allInvoiceTypesSource.Where(s => s.invoiceTypeId == i.invoiceTypeId).FirstOrDefault<InvoiceTypes>();
                    allInvoiceTypes.Remove(invoiceTypes);
                }

                dg_allItems.ItemsSource = allInvoiceTypes;
                dg_allItems.SelectedValuePath = "invoiceTypeId";
                dg_allItems.DisplayMemberPath = "translate";

                dg_selectedItems.ItemsSource = selectedInvoiceTypes;
                dg_selectedItems.SelectedValuePath = "invoiceTypeId";
                dg_selectedItems.DisplayMemberPath = "translate";
                #endregion

                SectionData.EndAwait(grid_invoiceTypesList);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_invoiceTypesList);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void translat()
        {
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txb_search, MainWindow.resourcemanager.GetString("trSearchHint"));

            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");

            dg_allItems.Columns[0].Header = MainWindow.resourcemanager.GetString("trItem");
            dg_selectedItems.Columns[0].Header = MainWindow.resourcemanager.GetString("trItem");

            txt_title.Text = MainWindow.resourcemanager.GetString("invoiceTypes");
            txt_items.Text = MainWindow.resourcemanager.GetString("invoiceTypes");
            txt_selectedItems.Text = MainWindow.resourcemanager.GetString("trSelectedInvoicesTypes");

            tt_selectAllItem.Content = MainWindow.resourcemanager.GetString("trSelectAllItems");
            tt_unselectAllItem.Content = MainWindow.resourcemanager.GetString("trUnSelectAllItems");
            tt_selectItem.Content = MainWindow.resourcemanager.GetString("trSelectOneItem");
            tt_unselectItem.Content = MainWindow.resourcemanager.GetString("trUnSelectOneItem");
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {//close
            DialogResult = false;
            this.Close();
        }

        private void Txb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                searchText = txb_search.Text.ToLower();
                invoiceTypesQuery = allInvoiceTypes.Where(s => s.translate.ToLower().Contains(searchText)).ToList();
                dg_allItems.ItemsSource = invoiceTypesQuery;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        private void Grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            //// Have to do this in the unusual case where the border of the cell gets selected.
            //// and causes a crash 'EditItem is not allowed'
            //e.Cancel = true;
        }

        private void Dg_allItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Btn_selectedItem_Click(null, null);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_selectedAll_Click(object sender, RoutedEventArgs e)
        {//select all
            try
            {
                int x = allInvoiceTypes.Count();
                for (int i = 0; i < x; i++)
                {
                    dg_allItems.SelectedIndex = 0;
                    Btn_selectedItem_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        private void Btn_selectedItem_Click(object sender, RoutedEventArgs e)
        {//select one
            try
            {
                invoiceTypes = dg_allItems.SelectedItem as InvoiceTypes;
                if (invoiceTypes != null)
                {

                    allInvoiceTypes.Remove(invoiceTypes);
                    selectedInvoiceTypes.Add(invoiceTypes);

                    dg_allItems.ItemsSource = allInvoiceTypes;
                    dg_selectedItems.ItemsSource = selectedInvoiceTypes;

                    dg_allItems.Items.Refresh();
                    dg_selectedItems.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        private void Btn_unSelectedItem_Click(object sender, RoutedEventArgs e)
        {//unselect one
            try
            {
                invoiceTypes = dg_selectedItems.SelectedItem as InvoiceTypes;


                allInvoiceTypes.Add(invoiceTypes);

                selectedInvoiceTypes.Remove(invoiceTypes);

                dg_allItems.ItemsSource = allInvoiceTypes;

                dg_allItems.Items.Refresh();
                dg_selectedItems.Items.Refresh();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        private void Btn_unSelectedAll_Click(object sender, RoutedEventArgs e)
        {//unselect all
            try
            {
                int x = 0;
                x = selectedInvoiceTypes.Count();
                for (int i = 0; i < x; i++)
                {
                    dg_selectedItems.SelectedIndex = 0;
                    Btn_unSelectedItem_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        private void Dg_selectedItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Btn_unSelectedItem_Click(null, null);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                //SectionData.StartAwait(grid_invoiceTypesList);
               
                //await invoiceTypesModel.AddInvoiceTypesToSection(selectedInvoiceTypes.ToList(), sectionId, MainWindow.userLogin.userId);

                DialogResult = true;
                this.Close();

                //SectionData.EndAwait(grid_invoiceTypesList);
            }
            catch (Exception ex)
            {
                //SectionData.EndAwait(grid_invoiceTypesList);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
