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
    /// Interaction logic for wd_unitConversion.xaml
    /// </summary>
    public partial class wd_unitConversion : Window
    {
        public wd_unitConversion()
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
        //IEnumerable<Item> items;
        Item item = new Item();
        ItemUnit itemUnit = new ItemUnit();
        ItemLocation ItemLocation = new ItemLocation();
        List<ItemLocation> locations;
        public int _itemId;
        public int _itemUnitId;
        List<ItemUnit> units;
        List<ItemUnit> smallUnits;
        ItemUnit isSmall = null;
        public string itemName;
        public string unitName;
        private static string _FromUnit = "";
        private static string _ToUnit = "";
        private static int _ToQuantity = 0;
        private static int _FromQuantity = 0;
        private static int _ConversionQuantity = 0;
        /*
        private void Cb_item_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                cb_item.ItemsSource = items.Where(x => x.name.Contains(cb_item.Text));
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */
        /*
        private async Task fillParentItemCombo()
        {
            //إنشاء إجراء من API يعيد فقط الحزم
            items = await item.GetAllItems();
            var listCa = items.Where(x => x.isActive == 1).ToList();
            cb_item.ItemsSource = listCa;
            cb_item.SelectedValuePath = "itemId";
            cb_item.DisplayMemberPath = "name";
        }
        */
        private async Task setToquantityMessage()
        {
            int quantity = 0;
            int remain = 0;

            if (tb_fromQuantity.Text != "")
                quantity = int.Parse(tb_fromQuantity.Text);
            //if (quantity != 0 && tb_fromQuantity.Text != "" && cb_fromUnit.SelectedIndex != -1 && cb_toUnit.SelectedIndex != -1)
            if (quantity != 0 && tb_fromQuantity.Text != ""   && cb_toUnit.SelectedIndex != -1)
            {
                isSmall = smallUnits.Find(x => x.itemUnitId == (int)cb_toUnit.SelectedValue);
                if (isSmall != null) // from-unit is bigger than to-unit
                {
                    _ConversionQuantity = itemUnit.getUnitConversionQuan(_itemUnitId, (int)cb_toUnit.SelectedValue, units);
                    //_ConversionQuantity = (int)await itemUnit.largeToSmallUnitQuan(_itemUnitId, (int)cb_toUnit.SelectedValue);
                    _ToQuantity = quantity * _ConversionQuantity;
                    _FromUnit = "";
                    _FromQuantity = quantity;

                }
                else
                {
                    _ConversionQuantity =  itemUnit.getLargeUnitConversionQuan(_itemUnitId, (int)cb_toUnit.SelectedValue,units);
                    //_ConversionQuantity = (int)await itemUnit.smallToLargeUnit(_itemUnitId, (int)cb_toUnit.SelectedValue);

                    if (_ConversionQuantity != 0)
                    {
                        _ToQuantity = quantity / _ConversionQuantity;
                        remain = quantity - (_ToQuantity * _ConversionQuantity); // get remain quantity which cannot be changeed
                    }
                    _FromUnit = remain.ToString() + " " + unitName;
                    _FromQuantity = quantity - remain;
                }
                _ToUnit = _ToQuantity.ToString() + " " + cb_toUnit.Text;
            }

           
            txt_toQuantity.Text = _FromUnit;
            txt_toQuantityRemainder.Text = _ToUnit;
        }
        private void clearConversionInputs()
        {
            //cb_item.SelectedIndex = -1;
            //cb_fromUnit.SelectedIndex = -1;
            cb_toUnit.SelectedIndex = -1;
            cb_sectionLocation.SelectedIndex = -1;
            tb_fromQuantity.Clear();
            txt_toQuantity.Text = "";
            txt_toQuantityRemainder.Text = "";
            _ToQuantity = 0;
            _ConversionQuantity = 0;
            _FromUnit = "";
            _ToUnit = "";
            isSmall = null;
            SectionData.clearValidate(tb_fromQuantity, p_errorFromQuantity);
        }
        private async void Tb_validateEmptyTextChange(object sender, TextChangedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
                var txb = sender as TextBox;
                if ((sender as TextBox).Name == "tb_fromQuantity")
                    SectionData.InputJustNumber(ref txb);
                checkLocationQuantity();
                await setToquantityMessage();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_validateEmptyLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void validateEmpty(string name, object sender)
        {
            try
            {
                if (name == "TextBox")
                {
                    if ((sender as TextBox).Name == "tb_fromQuantity")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorFromQuantity, tt_errorFromQuantity, "trEmptyQuantityToolTip");
                }
                else if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_toUnit")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorToUnit, tt_errorToUnit, "trErrorEmptyDesUnitToToolTip");
                    //else if ((sender as ComboBox).Name == "cb_fromUnit")
                    //    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorFromUnit, tt_errorFromUnit, "trErrorEmptySrcUnitToolTip");
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }


        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
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
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (e.Key == Key.Return)
                {
                    Btn_save_Click(null, null);
                }
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
        private async Task<bool> validateInputs()
        {
            bool valid = true;
            SectionData.validateEmptyTextBox(tb_fromQuantity, p_errorFromQuantity, tt_errorFromQuantity, "trEmptyQuantityToolTip");
            SectionData.validateEmptyComboBox(cb_toUnit, p_errorToUnit, tt_errorToUnit, "trErrorEmptyDesUnitToToolTip");
            //SectionData.validateEmptyComboBox(cb_fromUnit, p_errorFromUnit, tt_errorFromUnit, "trErrorEmptySrcUnitToolTip");
            //if (tb_fromQuantity.Text.Equals("") || cb_fromUnit.SelectedIndex == -1 || cb_toUnit.SelectedIndex == -1)
            if (tb_fromQuantity.Text.Equals("")   || cb_toUnit.SelectedIndex == -1)
            {
                valid = false;
                return valid;
            }
            int quantity = int.Parse(tb_fromQuantity.Text);
            if (cb_sectionLocation.SelectedIndex == -1)
            {
                int branchQuantity = (int)await ItemLocation.getUnitAmount(_itemUnitId, MainWindow.branchID.Value);

                if (branchQuantity < quantity)
                {
                    tb_fromQuantity.Text = branchQuantity.ToString();

                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableFromToolTip"), animation: ToasterAnimation.FadeIn);
                    valid = false;
                    return valid;
                }
            }
            if (isSmall == null && _ConversionQuantity > quantity)
            {
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorNoEnoughQuantityToolTip"), animation: ToasterAnimation.FadeIn);
                valid = false;
                return valid;
            }
            return valid;
        }
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                bool valid = await validateInputs();
                if (valid)
                {
                    int res = 0;
                    if (cb_sectionLocation.SelectedIndex != -1)
                    {
                        var locationId = locations.Find(x => x.itemsLocId == (int)cb_sectionLocation.SelectedValue).locationId;
                       res = (int)await ItemLocation.transferAmountbetweenUnits((int)locationId, (int)cb_sectionLocation.SelectedValue, (int)cb_toUnit.SelectedValue, _FromQuantity, _ToQuantity, MainWindow.userID.Value);
                       
                    }
                    else
                    {
                        res = (int)await ItemLocation.unitsConversion(MainWindow.branchID.Value, _itemUnitId, (int)cb_toUnit.SelectedValue, _FromQuantity, _ToQuantity, MainWindow.userID.Value, isSmall);
                       
                    }

                    if (res > 0)
                    {
                        //clearConversionInputs();
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                        this.Close();
                    }
                    else if (res == -3)
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableFromToolTip"), animation: ToasterAnimation.FadeIn);

                    else
                        Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                }
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
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isActive = false;
                this.Close();
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
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                }

                translate();

                #region key up
                cb_sectionLocation.IsTextSearchEnabled = false;
                cb_sectionLocation.IsEditable = true;
                cb_sectionLocation.StaysOpenOnEdit = true;
                cb_sectionLocation.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_sectionLocation.Text = "";

                cb_toUnit.IsTextSearchEnabled = false;
                cb_toUnit.IsEditable = true;
                cb_toUnit.StaysOpenOnEdit = true;
                cb_toUnit.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_toUnit.Text = "";
                #endregion

                txt_itemUnitName.Text = itemName + " - " + unitName;
                clearConversionInputs();

                //await fillItemCombo();
                await item_SelectionChanged();
                await fromUnit_SelectionChanged();
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
        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trUnitConversion");
            //MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_item, MainWindow.resourcemanager.GetString("trItemHint")); 
            //MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_fromUnit , MainWindow.resourcemanager.GetString("trFromUnitHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_toUnit, MainWindow.resourcemanager.GetString("trToUnitHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_sectionLocation, MainWindow.resourcemanager.GetString("trFromLocationHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_fromQuantity , MainWindow.resourcemanager.GetString("trQuantityHint"));
            txt_FromTitle.Text = MainWindow.resourcemanager.GetString("trFrom")+":"; 
            btn_save.Content = MainWindow.resourcemanager.GetString("trConvert");
        }
        /*
        private async Task fillItemCombo()
        {
            if (items is null)
                await RefrishItems();
            cb_item.ItemsSource = items.ToList();
            cb_item.SelectedValuePath = "itemId";
            cb_item.DisplayMemberPath = "name";
        }
        */
        /*
        async Task<IEnumerable<Item>> RefrishItems()
        {
            items = await item.GetItemsWichHasUnits();
            items = items.Where(x => x.type != "sr").ToList();

            return items;
        }
        */
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

        /*
        private async void Cb_item_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cb_item.SelectedIndex != -1)
                {
                    units = await itemUnit.GetItemUnits(int.Parse(cb_item.SelectedValue.ToString()));

                    cb_fromUnit.ItemsSource = units;
                    cb_fromUnit.SelectedValuePath = "itemUnitId";
                    cb_fromUnit.DisplayMemberPath = "mainUnit";
                    cb_fromUnit.SelectedIndex = 0;

                    cb_toUnit.ItemsSource = units;
                    cb_toUnit.SelectedValuePath = "itemUnitId";
                    cb_toUnit.DisplayMemberPath = "mainUnit";
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */
        /*
        private async void Cb_fromUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_fromUnit.SelectedIndex != -1)
                {
                    smallUnits = await itemUnit.getSmallItemUnits((int)cb_item.SelectedValue, (int)cb_fromUnit.SelectedValue);

                    string itemUnitStr = cb_fromUnit.SelectedValue.ToString();
                    locations = await ItemLocation.getSpecificItemLocation(itemUnitStr, MainWindow.branchID.Value);
                    foreach (var l in locations)
                    {
                        if (l.location.Equals("000"))
                            l.note = MainWindow.resourcemanager.GetString("trFreeZone");
                        else l.note = l.section + "-" + l.location;
                    }
                    cb_sectionLocation.ItemsSource = locations;
                    cb_sectionLocation.SelectedValuePath = "itemsLocId";
                    cb_sectionLocation.DisplayMemberPath = "note";

                    await setToquantityMessage();
                }
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
        */
        async Task item_SelectionChanged()
        {
          
                units = await itemUnit.GetItemUnits(_itemId);
              
                cb_toUnit.ItemsSource = units;
                cb_toUnit.SelectedValuePath = "itemUnitId";
                cb_toUnit.DisplayMemberPath = "mainUnit";
        }

        async Task fromUnit_SelectionChanged()
        {
            //if (cb_fromUnit.SelectedIndex != -1)
            {
                smallUnits = await itemUnit.getSmallItemUnits(_itemId, _itemUnitId);

                string itemUnitStr = _itemUnitId.ToString();
                locations = await ItemLocation.getSpecificItemLocation(itemUnitStr, MainWindow.branchID.Value);
                foreach (var l in locations)
                {
                    if (l.location.Equals("000"))
                        l.note = MainWindow.resourcemanager.GetString("trFreeZone");
                    else l.note = l.section + "-" + l.location;
                }
                cb_sectionLocation.ItemsSource = locations;
                cb_sectionLocation.SelectedValuePath = "itemsLocId";
                cb_sectionLocation.DisplayMemberPath = "note";

                await setToquantityMessage();
            }
        }
        private  async void Cb_toUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                await setToquantityMessage();
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
        private void checkLocationQuantity()
        {
            if (cb_sectionLocation.SelectedIndex != -1)
            {
                var locationQuantity = locations.Find(x => x.itemsLocId == (int)cb_sectionLocation.SelectedValue).quantity;
                int quantity = 0;
                if (!tb_fromQuantity.Text.Equals(""))
                    quantity = int.Parse(tb_fromQuantity.Text);
                if (locationQuantity < quantity)
                {
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableFromToolTip"), animation: ToasterAnimation.FadeIn);
                    tb_fromQuantity.Text = locationQuantity.ToString();

                }
            }
        }
        private void Cb_sectionLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                checkLocationQuantity();
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

        private void Cb_sectionLocation_KeyUp(object sender, KeyEventArgs e)
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

        private void Cb_toUnit_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = units.Where(p => p.mainUnit.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
