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
    /// Interaction logic for wd_inventory.xaml
    /// </summary>
    public partial class wd_inventory : Window
    {
        public wd_inventory()
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
        public Inventory inventory = new Inventory();
        IEnumerable<Inventory> inventories;
        public int branchId { get; set; }
        public int userId { get; set; }
        public string inventoryType { get; set; }
        /// <summary>
        /// for filtering inventory type
        /// </summary>
        public string title { get; set; }
        private void Btn_select_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                inventory = dg_Inventory.SelectedItem as Inventory;
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

        }
        private   void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucInventory);

                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_ucInventory.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_ucInventory.FlowDirection = FlowDirection.RightToLeft;
                }

                txt_title.Text = title;

                translat();
                #endregion
                  refreshInventories();
           
                if (sender != null)
                    SectionData.EndAwait(grid_ucInventory);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucInventory);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void translat()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trInventory");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txb_search, MainWindow.resourcemanager.GetString("trSearchHint"));

            dg_Inventory.Columns[0].Header = MainWindow.resourcemanager.GetString("trInventoryNum");
            dg_Inventory.Columns[1].Header = MainWindow.resourcemanager.GetString("trCreateDate");

            btn_select.Content = MainWindow.resourcemanager.GetString("trSelect");
        }

        private   void refreshInventories()
        {
            dg_Inventory.ItemsSource = inventories.ToList();
        }

        private void Dg_Inventory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                inventory = dg_Inventory.SelectedItem as Inventory;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Dg_Inventory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
    }
}
