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
    /// Interaction logic for wd_sliceList.xaml
    /// </summary>
    public partial class wd_sliceList : Window
     {
        public bool isActive;
        public int Id { get; set; }
 
        Slice sliceModel = new Slice();
        SliceUser sliceUserModel = new SliceUser();

        Slice slice = new Slice();
        SliceUser sliceUser = new SliceUser();

        //List<Slice> allSliceSource = new List<Slice>();
        List<SliceUser> selectedSliceByUserSource = new List<SliceUser>();
        List<SliceUser> selectedSliceByUserTable = new List<SliceUser>();
 
        List<Slice> allSlice = new List<Slice>();
        List<SliceUser> selectedSliceByUser = new List<SliceUser>();
  
        
 
        IEnumerable<Slice> storeQuery;

        string searchText = "";

        public string txtStoreSearch;
        public wd_sliceList()
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
                    SectionData.StartAwait(grid_sliceList);

                #region translate
                if (AppSettings.lang.Equals("en"))
                { MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly()); grid_sliceList.FlowDirection = FlowDirection.LeftToRight; }
                else
                { MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly()); grid_sliceList.FlowDirection = FlowDirection.RightToLeft; }

                translat();
                #endregion
                if (FillCombo.slicesList is null)
                    await FillCombo.RefreshSlices();
                //allSliceSource = await sliceModel.GetAll();////active slice and store 

                allSlice.AddRange(FillCombo.slicesList.Where(x => x.isActive).ToList());
                //chk user or slice
                //var dgtc = dg_selectedSlice.Columns[0] as DataGridTextColumn;

              
                    selectedSliceByUserSource = await sliceUserModel.GetSlicesByUserId(Id);
                    selectedSliceByUser.AddRange(selectedSliceByUserSource);
                    //remove selected items from all items
                    foreach (var i in selectedSliceByUser)
                    {
                        slice = allSlice.Where(s => s.sliceId == i.sliceId).FirstOrDefault<Slice>();
                        allSlice.Remove(slice);
                    }
                    //dgtc.Binding = new System.Windows.Data.Binding("bname");

                    dg_selectedSlice.ItemsSource = selectedSliceByUser;
                    dg_selectedSlice.SelectedValuePath = "sliceId";
                    dg_selectedSlice.DisplayMemberPath = "name";
                

                dg_allSlice.ItemsSource = allSlice;
                dg_allSlice.SelectedValuePath = "sliceId";
                dg_allSlice.DisplayMemberPath = "name";

                if (sender != null)
                    SectionData.EndAwait(grid_sliceList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceList);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void translat()
        {
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txb_search, MainWindow.resourcemanager.GetString("trSearchHint"));

            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");



          

           txt_title.Text = MainWindow.resourcemanager.GetString("prices");
           txt_slice.Text = MainWindow.resourcemanager.GetString("prices");
           txt_selectedSlice.Text = MainWindow.resourcemanager.GetString("trSelectedPrices");

           dg_allSlice.Columns[0].Header = MainWindow.resourcemanager.GetString("prices");
           dg_selectedSlice.Columns[0].Header = MainWindow.resourcemanager.GetString("prices");
           



            tt_search.Content = MainWindow.resourcemanager.GetString("trSearch");
            tt_selectAllItem.Content = MainWindow.resourcemanager.GetString("trSelectAllItems");
            tt_unselectAllItem.Content = MainWindow.resourcemanager.GetString("trUnSelectAllItems");
            tt_selectItem.Content = MainWindow.resourcemanager.GetString("trSelectOneItem");
            tt_unselectItem.Content = MainWindow.resourcemanager.GetString("trUnSelectOneItem");
        }
        private void Dg_selectedSlice_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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


        private void Dg_allSlice_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
                int x = allSlice.Count;
                for (int i = 0; i < x; i++)
                {
                    dg_allSlice.SelectedIndex = 0;
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
                    SectionData.StartAwait(grid_sliceList);

                slice = dg_allSlice.SelectedItem as Slice;

                if (slice != null)
                {
                    allSlice.Remove(slice);
                    
                        SliceUser bu = new SliceUser();
                        bu.sliceUserId = 0;
                        bu.sliceId = slice.sliceId;
                        bu.userId = Id;
                        bu.name = slice.name;
                        bu.createUserId = MainWindow.userID;

                        selectedSliceByUser.Add(bu);

                        dg_selectedSlice.ItemsSource = selectedSliceByUser;


                    dg_allSlice.ItemsSource = allSlice;

                    dg_allSlice.Items.Refresh();
                    dg_selectedSlice.Items.Refresh();
                }
                if (sender != null)
                    SectionData.EndAwait(grid_sliceList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceList);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_unSelectedAll_Click(object sender, RoutedEventArgs e)
        {//unselect all
            try
            {
                int x = 0;
                x = selectedSliceByUser.Count;
            

                for (int i = 0; i < x; i++)
                {
                    dg_selectedSlice.SelectedIndex = 0;
                    Btn_unSelectedStore_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_unSelectedStore_Click(object sender, RoutedEventArgs e)
        {//unselect one
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_sliceList);

               
                    sliceUser = dg_selectedSlice.SelectedItem as SliceUser;

                    if (sliceUser != null)
                    {
                        slice = FillCombo.slicesList.Where(s => s.sliceId == sliceUser.sliceId.Value).FirstOrDefault();
 
                        selectedSliceByUser.Remove(sliceUser);

                        dg_selectedSlice.ItemsSource = selectedSliceByUser;
                    }

                allSlice.Add(slice);
                dg_allSlice.ItemsSource = allSlice;
                dg_allSlice.Items.Refresh();
                dg_selectedSlice.Items.Refresh();

                if (sender != null)
                    SectionData.EndAwait(grid_sliceList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceList);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_sliceList);
                int s = 0;
               
                    foreach (var v in selectedSliceByUser)
                    {
                        SliceUser but = new SliceUser();
                        but.sliceId = v.sliceId;
                        but.userId = Id;
                        but.createUserId = MainWindow.userLogin.userId;
                      
                        selectedSliceByUserTable.Add(but);
                    }
                    s = (int)await sliceUserModel.UpdateSliceByUserId(selectedSliceByUserTable, Id, MainWindow.userID.Value);
                
                isActive = true;
                this.Close();
                if (sender != null)
                    SectionData.EndAwait(grid_sliceList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceList);
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
                    SectionData.StartAwait(grid_sliceList);
                txtStoreSearch = txb_search.Text.ToLower();

                searchText = txb_search.Text.ToLower();
                storeQuery = allSlice.Where(s => s.name.ToLower().Contains(searchText));
                dg_allSlice.ItemsSource = storeQuery;
                if (sender != null)
                    SectionData.EndAwait(grid_sliceList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceList);
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
