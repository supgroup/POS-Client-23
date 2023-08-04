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
    /// Interaction logic for wd_itemsList.xaml
    /// </summary>
    public partial class wd_itemsList : Window
    {
        public wd_itemsList()
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
        public bool isActive;
        Item item = new Item();
        List<Item> allItems = new List<Item>();
        public List<Item> selectedItems = new List<Item>();
        Item itemModel = new Item();
        public string txtItemSearch;
        /// <summary>
        /// Selcted Items if selectedItems Have Items At the beginning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucItemsList);

                #region translate
                if (AppSettings.lang.Equals("en"))
                { MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly()); grid_ucItemsList.FlowDirection = FlowDirection.LeftToRight; }
                else
                { MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly()); grid_ucItemsList.FlowDirection = FlowDirection.RightToLeft; }

                translat();
                #endregion

                allItems = (await itemModel.GetAllItems()).Where(x => x.isActive == 1).ToList();

                foreach (var item in selectedItems)
                {
                    allItems.Remove(item);

                }
                selectedItems.AddRange(selectedItems);

                lst_allItems.ItemsSource = allItems;
                lst_selectedItems.ItemsSource = selectedItems;

                lst_allItems.SelectedValuePath = "itemId";
                lst_selectedItems.SelectedValuePath = "itemId";
                lst_allItems.DisplayMemberPath = "name";
                lst_selectedItems.DisplayMemberPath = "name";

                if (sender != null)
                    SectionData.EndAwait(grid_ucItemsList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucItemsList);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void translat()
        {
            txt_items.Text = MainWindow.resourcemanager.GetString("trItems");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txb_searchitems, MainWindow.resourcemanager.GetString("trSearchHint"));
            txt_Items.Text = MainWindow.resourcemanager.GetString("trItems");
            txt_selectedItems.Text = MainWindow.resourcemanager.GetString("trSelectedItems");

            tt_selectAllItem.Content = MainWindow.resourcemanager.GetString("trSelectAllItems");
            tt_unselectAllItem.Content = MainWindow.resourcemanager.GetString("trUnSelectAllItems");
            tt_selectItem.Content = MainWindow.resourcemanager.GetString("trSelectOneItem");
            tt_unselectItem.Content = MainWindow.resourcemanager.GetString("trUnSelectOneItem");

            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
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
        private void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            isActive = true;
            this.Close();
        }
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            isActive = false;
            this.Close();
        }
        private void Lst_allItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
        private void Lst_selectedItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
        private async void Btn_selectedAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                selectedItems = (await itemModel.GetAllItems()).Where(x => x.isActive == 1).ToList();
                allItems.Clear();
                lst_allItems.ItemsSource = allItems;
                lst_selectedItems.ItemsSource = selectedItems;
                lst_allItems.Items.Refresh();
                lst_selectedItems.Items.Refresh();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_selectedItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                item = lst_allItems.SelectedItem as Item;
                if (item != null)
                {
                    allItems.Remove(item);
                    selectedItems.Add(item);

                    lst_allItems.ItemsSource = allItems;
                    lst_selectedItems.ItemsSource = selectedItems;

                    lst_allItems.Items.Refresh();
                    lst_selectedItems.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Btn_unSelectedItem_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                item = lst_selectedItems.SelectedItem as Item;
                if (item != null)
                {
                    selectedItems.Remove(item);
                    allItems.Add(item);

                    lst_allItems.ItemsSource = allItems;
                    lst_allItems.Items.Refresh();
                    lst_selectedItems.ItemsSource = selectedItems;
                    lst_selectedItems.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_unSelectedAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                allItems = (await itemModel.GetAllItems()).Where(x => x.isActive == 1).ToList();
                selectedItems.Clear();
                lst_allItems.ItemsSource = allItems;
                lst_allItems.Items.Refresh();
                lst_selectedItems.ItemsSource = selectedItems;
                lst_selectedItems.Items.Refresh();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Txb_searchitems_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            { 
                txtItemSearch = txb_searchitems.Text.ToLower();
                lst_allItems.ItemsSource = allItems.Where(x => (x.code.ToLower().Contains(txtItemSearch) ||
                x.name.ToLower().Contains(txtItemSearch) ||
                x.details.ToLower().Contains(txtItemSearch)
                ) && x.isActive == 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
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
         
    }
}
