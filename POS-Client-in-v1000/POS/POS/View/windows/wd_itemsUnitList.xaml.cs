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
    /// Interaction logic for wd_itemsUnitList.xaml
    /// </summary>
    public partial class wd_itemsUnitList : Window
    {
        public int itemId = 0 , itemUnitId = 0;

        public bool isActive;
        public string CallerName;//"IUList"

        ItemUnit itemUnit = new ItemUnit();
        ItemUnit itemUnitModel = new ItemUnit();
        List<ItemUnit> allItemUnitsSource = new List<ItemUnit>();
        List<ItemUnit> allItemUnits = new List<ItemUnit>();

        Package package = new Package();
        Package packageModel = new Package();
        List<Package> allIPackagesSource = new List<Package>();
        List<Package> allPackages = new List<Package>();

        ItemUnitUser itemUnitUser = new ItemUnitUser();
        ItemUnitUser itemUnitUserModel = new ItemUnitUser();
        List<ItemUnitUser> selectedItemUnitsSource = new List<ItemUnitUser>();
        public List<ItemUnitUser> selectedItemUnits = new List<ItemUnitUser>();

        string searchText = "";

        public string txtItemSearch;

        IEnumerable<ItemUnit> itemUnitQuery;

        public wd_itemsUnitList()
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
                if (sender != null)
                    SectionData.StartAwait(grid_offerList);

                #region translate
                    if (AppSettings.lang.Equals("en"))
                { MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly()); grid_offerList.FlowDirection = FlowDirection.LeftToRight; }
                else
                { MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly()); grid_offerList.FlowDirection = FlowDirection.RightToLeft; }

                translat();
                #endregion

                allItemUnitsSource = await itemUnitModel.Getall();
                allItemUnits.AddRange(allItemUnitsSource);
                for (int i = 0; i < allItemUnits.Count; i++)
                {
                    //remove parent package itemunit
                    if (allItemUnits[i].itemUnitId == itemUnitId)
                    { allItemUnits.Remove(allItemUnits[i]); break; }

                }
                foreach (var iu in allItemUnits)
                {
                    iu.itemName = iu.itemName + "-" + iu.unitName;
                }

                if (CallerName.Equals("IUList"))
                {
                    allItemUnits = allItemUnits.Where(i => i.unitName != "service").ToList();
                    dg_selectedItems.Columns[1].Visibility = Visibility.Collapsed;

                    selectedItemUnitsSource = await itemUnitUserModel.GetByUserId(MainWindow.userID.Value);
                    //selectedItemUnitsSource = selectedItemUnitsSource.Where(i => i.);

                    //remove selected itemunits from source itemunits
                    foreach (var p in selectedItemUnitsSource)
                    {
                        for (int i = 0; i < allItemUnits.Count; i++)
                        {
                            //remove saved itemunits
                            if (p.itemUnitId == allItemUnits[i].itemUnitId)
                            {
                                allItemUnits.Remove(allItemUnits[i]);
                            }
                        }
                    }
                    selectedItemUnits.AddRange(selectedItemUnitsSource);
                    foreach (var p in selectedItemUnits)
                    {
                        foreach (var iu in allItemUnits)
                            if (p.itemUnitId == iu.itemUnitId)
                                p.notes = iu.itemName + "-" + iu.unitName;
                    }

                    dg_selectedItems.ItemsSource = selectedItemUnits;
                    dg_selectedItems.SelectedValuePath = "id";
                    dg_selectedItems.DisplayMemberPath = "notes";
                }
                else
                {
                    allIPackagesSource = await packageModel.GetChildsByParentId(itemUnitId);

                    //remove selected itemunits from source itemunits
                    foreach (var p in allIPackagesSource)
                    {
                        for (int i = 0; i < allItemUnits.Count; i++)
                        {
                            //remove saved itemunits
                            if (p.childIUId == allItemUnits[i].itemUnitId)
                            {
                                allItemUnits.Remove(allItemUnits[i]);
                            }
                        }
                    }
                    allPackages.AddRange(allIPackagesSource);
                    foreach (var p in allPackages)
                    {
                        foreach (var iu in allItemUnits)
                            if (p.parentIUId == iu.itemUnitId)
                                p.notes = iu.itemName + "-" + iu.unitName;
                    }

                    dg_selectedItems.ItemsSource = allPackages;
                    dg_selectedItems.SelectedValuePath = "packageId";
                    dg_selectedItems.DisplayMemberPath = "notes";

                    allItemUnits = allItemUnits.Where(i => i.isActive == 1).ToList();
                }
                dg_allItems.ItemsSource = allItemUnits;
                dg_allItems.SelectedValuePath = "itemUnitId";
                dg_allItems.DisplayMemberPath = "itemName";

                if (sender != null)
                    SectionData.EndAwait(grid_offerList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_offerList);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void translat()
        {
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txb_searchitems, MainWindow.resourcemanager.GetString("trSearchHint"));

            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");

            dg_allItems.Columns[0].Header = MainWindow.resourcemanager.GetString("trItem");
            dg_selectedItems.Columns[0].Header = MainWindow.resourcemanager.GetString("trItem");
            dg_selectedItems.Columns[1].Header = MainWindow.resourcemanager.GetString("trQTR");

            txt_title.Text = MainWindow.resourcemanager.GetString("trItems");
            txt_items.Text = MainWindow.resourcemanager.GetString("trItems");
            txt_selectedItems.Text = MainWindow.resourcemanager.GetString("trSelectedItems");

            tt_search.Content = MainWindow.resourcemanager.GetString("trSearch");
            tt_selectAllItem.Content = MainWindow.resourcemanager.GetString("trSelectAllItems");
            tt_unselectAllItem.Content = MainWindow.resourcemanager.GetString("trUnSelectAllItems");
            tt_selectItem.Content = MainWindow.resourcemanager.GetString("trSelectOneItem");
            tt_unselectItem.Content = MainWindow.resourcemanager.GetString("trUnSelectOneItem");

        }

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            isActive = false;
            this.Close();
        }

        private void Txb_searchitems_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                txtItemSearch = txb_searchitems.Text.ToLower();

                searchText = txb_searchitems.Text;
                itemUnitQuery = allItemUnits.Where(s => s.itemName.ToLower().Contains(searchText.ToLower()) || s.unitName.ToLower().Contains(searchText.ToLower()));
                dg_allItems.ItemsSource = itemUnitQuery;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
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
                int x = allItemUnits.Count;
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
                itemUnit = dg_allItems.SelectedItem as ItemUnit;
                if (itemUnit != null)
                {
                    if (CallerName.Equals(""))
                    {
                        Package p = new Package();

                        p.parentIUId = itemUnitId;
                        p.childIUId = itemUnit.itemUnitId;
                        p.quantity = 1;
                        p.isActive = 1;
                        p.notes = itemUnit.itemName;
                        p.createUserId = MainWindow.userID;

                        allItemUnits.Remove(itemUnit);
                        allPackages.Add(p);

                        dg_allItems.ItemsSource = allItemUnits;
                        dg_selectedItems.ItemsSource = allPackages;

                    }
                    else
                    {
                        ItemUnitUser iu = new ItemUnitUser();

                        iu.itemUnitId = itemUnit.itemUnitId;
                        iu.userId = MainWindow.userID;
                        iu.isActive = 1;
                        iu.notes = itemUnit.itemName;
                        iu.createUserId = MainWindow.userID;

                        allItemUnits.Remove(itemUnit);
                        selectedItemUnits.Add(iu);

                        dg_allItems.ItemsSource = allItemUnits;
                        dg_selectedItems.ItemsSource = selectedItemUnits;

                        
                    }

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
                ItemUnit i = new ItemUnit();

                if (CallerName.Equals(""))
                {
                    package = dg_selectedItems.SelectedItem as Package;
                    if (package != null)
                    {
                        i = allItemUnitsSource.Where(s => s.itemUnitId == package.childIUId.Value).FirstOrDefault();

                        allItemUnits.Add(i);

                        allPackages.Remove(package);

                        dg_selectedItems.ItemsSource = allPackages;
                    }
                }
                else
                {
                    itemUnitUser = dg_selectedItems.SelectedItem as ItemUnitUser;
                    if(itemUnitUser != null)
                    {
                        i = allItemUnitsSource.Where(s => s.itemUnitId == itemUnitUser.itemUnitId.Value).FirstOrDefault();

                        allItemUnits.Add(i);

                        selectedItemUnits.Remove(itemUnitUser);

                        dg_selectedItems.ItemsSource = selectedItemUnits;
                    }
                }

                dg_allItems.ItemsSource = allItemUnits;

                dg_allItems.Items.Refresh();
                // for solve problem
                //this.dg_selectedItems.CancelEdit();
                //this.dg_selectedItems.CancelEdit();
                ////////////
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
                if (CallerName.Equals(""))
                    x = allPackages.Count;
                else
                    x = selectedItemUnits.Count;
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

        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_offerList);

                if (CallerName.Equals("IUList"))
                {
                    foreach (var x in selectedItemUnits)
                    {
                        x.id = 0;
                        ItemUnit iu = new ItemUnit();
                        iu = await itemUnitModel.GetById(x.itemUnitId.Value);
                        x.itemId = iu.itemId;
                        x.unitId = iu.unitId;
                    }
                    await itemUnitUserModel.UpdateList(selectedItemUnits, MainWindow.userID.Value);
                    isActive = true;
                    this.Close();
                }
                else
                {
                    var res = await package.canUpdate(itemUnitId);
                    if (res)
                    {
                        await package.UpdatePackByParentId(itemUnitId, allPackages, MainWindow.userID.Value);
                        isActive = true;
                        this.Close();
                    }
                    else
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trCantDoProcess"), animation: ToasterAnimation.FadeIn);
                    }
                }
                //isActive = true;
                //this.Close();

                if (sender != null)
                    SectionData.EndAwait(grid_offerList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_offerList);
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

        private void Dg_allItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Dg_selectedItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }

        

        private void Dg_selectedItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            //// Have to do this in the unusual case where the border of the cell gets selected.
            //// and causes a crash 'EditItem is not allowed'
            //e.Cancel = true;
        }
    }
}
