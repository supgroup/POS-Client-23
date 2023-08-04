using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using netoaster;
using POS.Classes;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_generatePackage.xaml
    /// </summary>
    public partial class wd_generatePackage : Window
    {
        public wd_generatePackage()
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
        IEnumerable<Item> items;
        Item item = new Item();
        IEnumerable<Location> locations;
        Location location = new Location();
        ItemLocation itemLocation = new ItemLocation();
        Package package = new Package();
        public int _itemUnitId = 0;
        public List<Package> packages;
       
        private async Task fillLocationCombo()
        {
            locations = await location.GetLocsByBranchId(MainWindow.branchID.Value);
            locations = locations.Where(l => l.sectionId != null);
            cb_location.ItemsSource = locations;
            foreach(var l in locations)
            {
                if (l.name.Equals("000"))
                    l.note = MainWindow.resourcemanager.GetString("trFreeZone");
                else l.note = l.sectionName + "-" + l.name;
            }
            cb_location.SelectedValuePath = "locationId";
            cb_location.DisplayMemberPath = "note";
        }
        private void Tb_validateEmptyLostFocus(object sender, RoutedEventArgs e)
        {
            try
            { 
                SectionData.validateEmptyTextBox(tb_quantity, p_errorQuantity, tt_errorQuantity, "trEmptyQuantityToolTip");
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private async void tb_quantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_generatePackage);

                SectionData.validateEmptyTextBox(tb_quantity, p_errorQuantity, tt_errorQuantity, "trEmptyQuantityToolTip");
               // bool validQan = await checkAmount();
                var txb = sender as TextBox;
                if ((sender as TextBox).Name == "tb_quantity")
                    SectionData.InputJustNumber(ref txb);

                if (sender != null)
                    SectionData.EndAwait(grid_generatePackage);

                txb.Focus();
                txb.SelectionStart = txb.Text.Length;
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_generatePackage);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private async Task<bool> checkAmount()
        {
            bool valid = true;
            int quantity = 0;
            if(!tb_quantity.Text.Equals(""))
                quantity= int.Parse(tb_quantity.Text);
            if(quantity > 0)
            //if(cb_item.SelectedIndex != -1 && quantity > 0)
            {
                
                int itemUnitId = 0;
                 foreach(Package p in packages)
                {
                    if (!p.type.Equals("sr"))
                    {
                        int branchQuantity = 0;
                        int itemQuanP = p.quantity;
                        itemUnitId = (int)p.childIUId;
                        int requireQuan = itemQuanP * quantity;

                        branchQuantity = (int)await itemLocation.getAmountInBranch(itemUnitId, MainWindow.branchID.Value);

                        if (requireQuan > branchQuantity)
                        {
                            valid = false;
                            tb_quantity.Text = "0";
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorItemAmntNotAvailableToolTip") + " " + p.citemName, animation: ToasterAnimation.FadeIn);
                            return valid;
                        }
                    }

                }
            }
            return valid;
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
        private bool validateInputs()
        {
            bool valid = true;
            SectionData.validateEmptyTextBox(tb_quantity, p_errorQuantity, tt_errorQuantity, "trEmptyQuantityToolTip");
            //SectionData.validateEmptyComboBox(cb_item, p_errorParentItem, tt_errorParentItem, "trErrorEmptyItemToolTip");
            SectionData.validateEmptyComboBox(cb_location, p_errorLocation, tt_errorLocation, "trErrorEmptyLocationToolTip");
            //if (tb_quantity.Text.Equals("") || cb_item.SelectedIndex == -1 || cb_location.SelectedIndex == -1)
            if (tb_quantity.Text.Equals("") || cb_location.SelectedIndex == -1)
            {
                valid = false;
                return valid;
            }
            if (int.Parse(tb_quantity.Text) == 0)
            {
                valid = false;
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorQuantIsZeroToolTip"), animation: ToasterAnimation.FadeIn);

            }
            return valid;
        }
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_generatePackage);

                bool valid = validateInputs();
                if (valid)
                {
                    int quantity = int.Parse(tb_quantity.Text);
                    int res = 0;
                    if (cb_process.SelectedValue.ToString() == "generate")//compose package
                    {
                        res = (int)await itemLocation.generatePackage(_itemUnitId, quantity, (int)cb_location.SelectedValue, MainWindow.branchID.Value, MainWindow.userID.Value);
                    }
                    else //decompose Package
                    {
                        res = (int)await itemLocation.decomposePackage(_itemUnitId, quantity, (int)cb_location.SelectedValue, MainWindow.branchID.Value, MainWindow.userID.Value);

                    }
                    if (res > 0)
                    {
                        //clearGenerateInputs();
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                        await Task.Delay(0500);
                        this.Close();
                    }
                    else if (res.Equals(-3))
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableFromToolTip"), animation: ToasterAnimation.FadeIn);

                    else
                        Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                }
                if (sender != null)
                    SectionData.EndAwait(grid_generatePackage);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_generatePackage);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
      
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_generatePackage);

                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                }

                translate();
                #endregion
                configureProcessType();
                //await fillItemCombo();
                await fillLocationCombo();

                #region key up
                cb_location.IsTextSearchEnabled = false;
                cb_location.IsEditable = true;
                cb_location.StaysOpenOnEdit = true;
                cb_location.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_location.Text = "";
                #endregion

                BuildExtraOrdersDesign(packages);

                if (sender != null)
                    SectionData.EndAwait(grid_generatePackage);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_generatePackage);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trPackage");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_process, MainWindow.resourcemanager.GetString("trProcessType")); 
            //MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_item, MainWindow.resourcemanager.GetString("trItemHint")); 
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_location, MainWindow.resourcemanager.GetString("trLocationt"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_quantity, MainWindow.resourcemanager.GetString("trQuantityHint"));
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
        }
        private void configureProcessType()
        {
            var processList = new[] {
            new { Text = MainWindow.resourcemanager.GetString("trGenerate"), Value = "generate" },
            new { Text = MainWindow.resourcemanager.GetString("trPackageDestroy"), Value = "destroy"},
             };

            cb_process.DisplayMemberPath = "Text";
            cb_process.SelectedValuePath = "Value";
            cb_process.ItemsSource = processList;
            cb_process.SelectedIndex = 0;
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

        private async void Cb_location_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_generatePackage);
                if (cb_location.SelectedIndex != -1)
                {
                   await checkAmount();
                }
                if (sender != null)
                    SectionData.EndAwait(grid_generatePackage);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_generatePackage);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        
        #region extraOrders
        
        void BuildExtraOrdersDesign(List<Package> itemsPackageList)
        {
            sp_itemsPackage.Children.Clear();

            int sequence = 0;
            foreach (var item in itemsPackageList)
            {
                sequence++;
                #region Grid Container
                Grid gridContainer = new Grid();
                int colCount = 3;
                ColumnDefinition[] cd = new ColumnDefinition[colCount];
                for (int i = 0; i < colCount; i++)
                {
                    cd[i] = new ColumnDefinition();
                }
                cd[0].Width = new GridLength(1, GridUnitType.Auto);
                cd[1].Width = new GridLength(1, GridUnitType.Star);
                cd[2].Width = new GridLength(1, GridUnitType.Auto);
                for (int i = 0; i < colCount; i++)
                {
                    gridContainer.ColumnDefinitions.Add(cd[i]);
                }
                /////////////////////////////////////////////////////
                #region   sequence

                var itemSequenceText = new TextBlock();
                itemSequenceText.Text = sequence + ".";
                itemSequenceText.Margin = new Thickness(5);
                itemSequenceText.Foreground = Application.Current.Resources["MainColorGrey"] as SolidColorBrush;
                itemSequenceText.FontWeight = FontWeights.SemiBold;
                itemSequenceText.VerticalAlignment = VerticalAlignment.Center;
                itemSequenceText.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetColumn(itemSequenceText, 0);

                gridContainer.Children.Add(itemSequenceText);

                #endregion
                #region   name
                var itemNameText = new TextBlock();
                itemNameText.Text = item.notes;
                itemNameText.Margin = new Thickness(5);
                itemNameText.Foreground = Application.Current.Resources["MainColorGrey"] as SolidColorBrush;
                //itemNameText.FontWeight = FontWeights.SemiBold;
                itemNameText.VerticalAlignment = VerticalAlignment.Center;
                itemNameText.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetColumn(itemNameText, 1);

                gridContainer.Children.Add(itemNameText);
                #endregion
                #region   count
                var itemCountText = new TextBlock();
                itemCountText.Text = item.quantity.ToString();
                itemCountText.Margin = new Thickness(5, 5, 10, 5);
                itemCountText.Foreground = Application.Current.Resources["MainColorGrey"] as SolidColorBrush;
                //itemCountText.FontWeight = FontWeights.SemiBold;
                itemCountText.VerticalAlignment = VerticalAlignment.Center;
                itemCountText.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetColumn(itemCountText, 2);

                gridContainer.Children.Add(itemCountText);
                #endregion
                #endregion
                sp_itemsPackage.Children.Add(gridContainer);
            }
        }

        #endregion

        private void Cb_location_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = locations.Where(p => p.note.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
