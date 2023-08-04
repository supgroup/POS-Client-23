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
    /// Interaction logic for wd_branchesList.xaml
    /// </summary>
    public partial class wd_branchesList : Window
    {
        public bool isActive;
        public int Id { get; set; }
        public char userOrBranch { get; set; }
        public string branchType { get; set; }

        Branch branchModel = new Branch();
        BranchesUsers branchUserModel = new BranchesUsers();
        BranchStore branchStoreModel = new BranchStore();

        List<Branch> allStoresSource = new List<Branch>();
        List<BranchesUsers> selectedStoresByUserSource = new List<BranchesUsers>();
        List<BranchStore> selectedStoresByBranchSource = new List<BranchStore>();

        List<Branch> allStores = new List<Branch>();
        List<BranchesUsers> selectedStoresByUser = new List<BranchesUsers>();
        List<BranchesUserstable> selectedStoresByUserTable = new List<BranchesUserstable>();
        List<BranchStore> selectedStoresByBranch = new List<BranchStore>();
        List<BranchStoretable> selectedStoresByBranchTable = new List<BranchStoretable>();

        Branch branch = new Branch();
        BranchesUsers branchUser = new BranchesUsers();
        BranchStore branchStore = new BranchStore();

        IEnumerable<Branch> storeQuery;

        string searchText = "";

        public string txtStoreSearch;
        public wd_branchesList()
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_branchList);

                #region translate
                if (AppSettings.lang.Equals("en"))
                { MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly()); grid_branchList.FlowDirection = FlowDirection.LeftToRight; }
                else
                { MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly()); grid_branchList.FlowDirection = FlowDirection.RightToLeft; }

                translat();
                #endregion

                allStoresSource = await branchModel.GetAll();////active branch and store 
                allStoresSource = allStoresSource.Where(x => x.type.Equals(branchType)).ToList();
                allStores.AddRange(allStoresSource.Where(x => x.branchId != 1 && x.isActive == 1 ));
                //chk user or branch
                var dgtc = dg_selectedStores.Columns[0] as DataGridTextColumn;

                if (userOrBranch == 'u')
                {
                    selectedStoresByUserSource = await branchUserModel.GetBranchesByUserId(Id,branchType);
                    selectedStoresByUser.AddRange(selectedStoresByUserSource);
                    //remove selected items from all items
                    foreach (var i in selectedStoresByUser)
                    {
                        branch = allStores.Where(s => s.branchId == i.branchId).FirstOrDefault<Branch>();
                        allStores.Remove(branch);
                    }
                    dgtc.Binding = new System.Windows.Data.Binding("bname");

                    dg_selectedStores.ItemsSource = selectedStoresByUser;
                    dg_selectedStores.SelectedValuePath = "branchId";
                    dg_selectedStores.DisplayMemberPath = "bname";
                }
                else
                {
                    allStores = allStores.Where(s => s.branchId != Id).ToList();
                    selectedStoresByBranchSource = await branchStoreModel.GetByBranchId(Id,branchType);
                    selectedStoresByBranchSource = selectedStoresByBranchSource.Where(s => s.branchId == Id).ToList();
                    selectedStoresByBranch.AddRange(selectedStoresByBranchSource);
                    //remove selected items from all items
                    foreach (var i in selectedStoresByBranch)
                    {
                        branch = allStores.Where(s => s.branchId == i.storeId).FirstOrDefault<Branch>();
                        allStores.Remove(branch);
                    }
               
                    dgtc.Binding = new System.Windows.Data.Binding("sname");

                    dg_selectedStores.ItemsSource = selectedStoresByBranch;
                    dg_selectedStores.SelectedValuePath = "branchId";
                    dg_selectedStores.DisplayMemberPath = "sname";

                    //remove selected branch/store from previous window
                    //branch = allStores.Where(s => s.branchId == Id).FirstOrDefault<Branch>();
                    //allStores.Remove(branch);
                }

                dg_allStores.ItemsSource = allStores;
                dg_allStores.SelectedValuePath = "branchId";
                dg_allStores.DisplayMemberPath = "name";
          
                if (sender != null)
                    SectionData.EndAwait(grid_branchList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_branchList);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void translat()
        {
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txb_search, MainWindow.resourcemanager.GetString("trSearchHint"));

            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");

          

            if(branchType == "b")
            {

                txt_title.Text = MainWindow.resourcemanager.GetString("trBranch");
                txt_stores.Text = MainWindow.resourcemanager.GetString("trBranch");
                txt_selectedStores.Text = MainWindow.resourcemanager.GetString("trSelectedBranchs");

                dg_allStores.Columns[0].Header = MainWindow.resourcemanager.GetString("trBranch");
                dg_selectedStores.Columns[0].Header = MainWindow.resourcemanager.GetString("trBranch");
            }
            else if (branchType == "s")
            {
                txt_title.Text = MainWindow.resourcemanager.GetString("trStore");
                txt_stores.Text = MainWindow.resourcemanager.GetString("trStore");
                txt_selectedStores.Text = MainWindow.resourcemanager.GetString("trSelectedStores");

                dg_allStores.Columns[0].Header = MainWindow.resourcemanager.GetString("trStore");
                dg_selectedStores.Columns[0].Header = MainWindow.resourcemanager.GetString("trStore");
            }
             
           

            tt_search.Content = MainWindow.resourcemanager.GetString("trSearch");
            tt_selectAllItem.Content = MainWindow.resourcemanager.GetString("trSelectAllItems");
            tt_unselectAllItem.Content = MainWindow.resourcemanager.GetString("trUnSelectAllItems");
            tt_selectItem.Content = MainWindow.resourcemanager.GetString("trSelectOneItem");
            tt_unselectItem.Content = MainWindow.resourcemanager.GetString("trUnSelectOneItem");
        }
        private void Dg_selectedStores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Btn_unSelectedStore_Click(null, null);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }


        private void Dg_allStores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Btn_selectedStore_Click(null, null);
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
                int x = allStores.Count;
                for (int i = 0; i < x; i++)
                {
                    dg_allStores.SelectedIndex = 0;
                    Btn_selectedStore_Click(null, null);
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_selectedStore_Click(object sender, RoutedEventArgs e)
        {//select one 
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_branchList);

                branch = dg_allStores.SelectedItem as Branch;

            if (branch != null)
            {
                allStores.Remove(branch);
                if(userOrBranch == 'u')
                {
                    BranchesUsers bu = new BranchesUsers();
                    bu.branchsUsersId = 0;
                    bu.branchId = branch.branchId;
                    bu.userId = Id;
                    bu.bname = branch.name;
                    bu.createUserId = MainWindow.userID;

                    selectedStoresByUser.Add(bu);
                    
                    dg_selectedStores.ItemsSource = selectedStoresByUser;

                }
                else
                {
                    BranchStore bs = new BranchStore();
                    bs.id = 0;
                    bs.branchId = branch.branchId;
                    bs.storeId = branch.branchId;
                    bs.createUserId = MainWindow.userID;
                    bs.sname = branch.name;
                    bs.bname = branch.name;
                    bs.isActive = 1;
                    selectedStoresByBranch.Add(bs);
                    dg_selectedStores.ItemsSource = selectedStoresByBranch;
                }

                dg_allStores.ItemsSource = allStores;

                dg_allStores.Items.Refresh();
                dg_selectedStores.Items.Refresh();
            }
                if (sender != null)
                    SectionData.EndAwait(grid_branchList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_branchList);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_unSelectedAll_Click(object sender, RoutedEventArgs e)
        {//unselect all
            try
            {
                int x = 0;
                if (userOrBranch == 'u')
                    x = selectedStoresByUser.Count;
                else
                    x = selectedStoresByBranch.Count;

                for (int i = 0; i < x; i++)
                {
                    dg_selectedStores.SelectedIndex = 0;
                    Btn_unSelectedStore_Click(null, null);
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private  void Btn_unSelectedStore_Click(object sender, RoutedEventArgs e)
        {//unselect one
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_branchList);

                if (userOrBranch == 'u')
                {
                    branchUser = dg_selectedStores.SelectedItem as BranchesUsers;

                    if (branchUser != null)
                    {
                        branch = allStoresSource.Where(s => s.branchId == branchUser.branchId.Value).FirstOrDefault();
                        //branch = await branchModel.getBranchById(branchUser.branchId.Value);

                        selectedStoresByUser.Remove(branchUser);

                        dg_selectedStores.ItemsSource = selectedStoresByUser;
                    }
                }
                else
                {
                    branchStore = dg_selectedStores.SelectedItem as BranchStore;

                    if (branchStore != null)
                    {
                        branch = allStoresSource.Where(s => s.branchId == branchStore.storeId.Value).FirstOrDefault();
                        //branch = await branchModel.getBranchById(branchStore.branchId.Value);
                        selectedStoresByBranch.Remove(branchStore);

                        dg_selectedStores.ItemsSource = selectedStoresByBranch;
                    }

                }

                allStores.Add(branch);
                dg_allStores.ItemsSource = allStores;
                dg_allStores.Items.Refresh();
                dg_selectedStores.Items.Refresh();

                if (sender != null)
                    SectionData.EndAwait(grid_branchList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_branchList);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_branchList);
                int s = 0;
                if (userOrBranch == 'u')
                {
                    foreach(var v in selectedStoresByUser)
                    {
                        BranchesUserstable but = new BranchesUserstable();
                        but.branchsUsersId = v.branchsUsersId;
                        but.branchId = v.branchId;
                        but.userId = v.userId;
                        but.createUserId = v.createUserId;
                        but.userId = v.updateUserId;
                        but.createDate = v.createDate;
                        but.updateDate = v.updateDate;
                        selectedStoresByUserTable.Add(but);
                    }
                    s = (int)await branchUserModel.UpdateBranchByUserId(selectedStoresByUserTable, Id,MainWindow.userID.Value, branchType);
                }
                else
                {
                    foreach(var v in selectedStoresByBranch)
                    {
                        BranchStoretable bst = new BranchStoretable();
                        bst.id = v.id;
                        bst.branchId = v.branchId;
                        bst.storeId = v.storeId;
                        bst.createDate = v.createDate;
                        bst.updateDate = v.updateDate;
                        bst.createUserId = v.createUserId;
                        bst.updateUserId = v.updateUserId;
                        bst.isActive = 1;
                        selectedStoresByBranchTable.Add(bst);
                    }
                    s = (int)await branchStoreModel.UpdateStoresById(selectedStoresByBranchTable, Id, MainWindow.userID.Value, branchType);

                }
                isActive = true;
                this.Close();
                if (sender != null)
                    SectionData.EndAwait(grid_branchList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_branchList);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception)
            {

            }
        }

        private void Txb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_branchList);
                txtStoreSearch = txb_search.Text.ToLower();

                searchText = txb_search.Text.ToLower();
                storeQuery = allStores.Where(s => s.name.ToLower().Contains(searchText) );
                dg_allStores.ItemsSource = storeQuery;
                if (sender != null)
                    SectionData.EndAwait(grid_branchList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_branchList);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {//close
            isActive = false;
            this.Close();
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
        private void Grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            try
            {
                //// Have to do this in the unusual case where the border of the cell gets selected.
                //// and causes a crash 'EditItem is not allowed'
                e.Cancel = true;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
