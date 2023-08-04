using POS.Classes;
using netoaster;
using POS.controlTemplate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static POS.View.uc_categorie;
using Microsoft.Win32;
using System.Windows.Resources;
using System.Threading;
using System.Windows.Media.Animation;
using Zen.Barcode;
using POS.View.windows;
using POS.View.catalog;
using Microsoft.Reporting.WinForms;

namespace POS.View
{
    /// <summary>
    /// Interaction logic for UC_item.xaml
    /// </summary>
    public partial class UC_item : UserControl
    {
        //public int ItemId;

        Item itemModel = new Item();
        Category categoryModel = new Category();
        Unit unitModel = new Unit();
        Property propertyModel = new Property();
        PropertiesItems propItemsModel = new PropertiesItems();
        ItemsProp itemsPropModel = new ItemsProp();
        // Serial serialModel = new Serial();
        ItemUnit itemUnitModel = new ItemUnit();
        Service serviceModel = new Service();
        IEnumerable<Category> categoriesQuery;
        List<Item> items;
        IEnumerable<Item> allItems;
        IEnumerable<Item> itemsQuery;
        Category category = new Category();
        // item object
        Item item = new Item();
        //item property value object
        ItemsProp itemProp = new ItemsProp();
        // serial object
        // Serial serial = new Serial();
        // item unit object
        ItemUnit itemUnit = new ItemUnit();
        // service object
        Service service = new Service();
        StorageCost storageCost = new StorageCost();

        string selectedType = "";
        BrushConverter bc = new BrushConverter();

        DataGrid dt = new DataGrid();

        List<int> categoryIds = new List<int>();
        List<string> categoryNames = new List<string>();
        // List<Property> properties;
        List<Category> categories;
        //List<StorageCost> storageCosts;
        List<PropertiesItems> propItems;
        List<Unit> units;
        List<ItemsProp> itemsProp;
        // List<Serial> itemSerials;
        List<ItemUnit> itemUnits;
        List<ItemUnit> barcodesList;
        public byte tglCategoryState = 1;
        public byte tglItemState;
        int? categoryParentId = 0;
        public string txtItemSearch;

        List<int> unitIds = new List<int>();
        List<string> unitNames = new List<string>();

        static private int _InternalCounter = 0;


        private static UC_item _instance;

        OpenFileDialog openFileDialog = new OpenFileDialog();
        ImageBrush brush = new ImageBrush();

        //bool StateClosed = false;
        #region //to handle barcode characters
        DateTime _lastKeystroke = new DateTime(0);
        static private string _BarcodeStr = "";
        static private object _Sender;
        #endregion
        string basicsPermission = "item_basics";
        string unitBasicsPermission = "unit_basics";

        public static UC_item Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UC_item();
                return _instance;

            }
        }

        public UC_item()
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
        CatigoriesAndItemsView catigoriesAndItemsView = new CatigoriesAndItemsView();

        private void translate()
        {
            txt_itemData.Text = MainWindow.resourcemanager.GetString("trItemData");
            txt_activeSearch.Text = MainWindow.resourcemanager.GetString("trActive");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            txt_barcode.Text = MainWindow.resourcemanager.GetString("trUnits");
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trBaseInformation");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_details, MainWindow.resourcemanager.GetString("trDetailsHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_name, MainWindow.resourcemanager.GetString("trNameHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_code, MainWindow.resourcemanager.GetString("trCode"));

            txt_secondaryInformation.Text = MainWindow.resourcemanager.GetString("trSecondaryInformation");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_categorie, MainWindow.resourcemanager.GetString("trSelectCategorieHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_itemType, MainWindow.resourcemanager.GetString("trSelectItemTypeHint"));//
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_warranty, MainWindow.resourcemanager.GetString("trWarranty") + "...");
            //MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_taxes, MainWindow.resourcemanager.GetString("trTax") + "...");
            txt_isTaxExempt.Text = MainWindow.resourcemanager.GetString("taxExempt");
            txt_isExpired.Text = MainWindow.resourcemanager.GetString("trHaveExpirationDate");
            txt_day.Text = MainWindow.resourcemanager.GetString("trDay");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_alertDays, MainWindow.resourcemanager.GetString("alertDays"));

            txt_minAndMax.Text = MainWindow.resourcemanager.GetString("trMinAndMaxOfItem");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_min, MainWindow.resourcemanager.GetString("trMinHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_max, MainWindow.resourcemanager.GetString("trMaxHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_minUnit, MainWindow.resourcemanager.GetString("trSelectUnitHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_maxUnit, MainWindow.resourcemanager.GetString("trSelectUnitHint"));

            btn_add.Content = MainWindow.resourcemanager.GetString("trAdd");
            btn_update.Content = MainWindow.resourcemanager.GetString("trUpdate");
            btn_delete.Content = MainWindow.resourcemanager.GetString("trDelete");
            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");

            ///////////////////////////Barcode
            txt_unit.Text = MainWindow.resourcemanager.GetString("trUnit");
            dg_unit.Columns[0].Header = MainWindow.resourcemanager.GetString("trUnit");
            dg_unit.Columns[1].Header = MainWindow.resourcemanager.GetString("trBarcode");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_selectUnit, MainWindow.resourcemanager.GetString("trSelectUnitHint"));
            txt_isDefaultPurchases.Text = MainWindow.resourcemanager.GetString("trPurchase");
            tb_isDefaultSales.Text = MainWindow.resourcemanager.GetString("trSales");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_count, MainWindow.resourcemanager.GetString("trCountHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_unit, MainWindow.resourcemanager.GetString("trUnitHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_price, MainWindow.resourcemanager.GetString("trPriceHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_cost, MainWindow.resourcemanager.GetString("trCost"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_barcode, MainWindow.resourcemanager.GetString("trBarcodeHint"));

            txt_propertiesButton.Text = MainWindow.resourcemanager.GetString("trProperties");
            txt_pricesButton.Text = MainWindow.resourcemanager.GetString("pricing");
            btn_addBarcode.Content = MainWindow.resourcemanager.GetString("trAdd");
            btn_updateBarcode.Content = MainWindow.resourcemanager.GetString("trUpdate");
            btn_deleteBarcode.Content = MainWindow.resourcemanager.GetString("trDelete");
            btn_clearProperties.ToolTip = MainWindow.resourcemanager.GetString("trClear");

            txt_hasWarranty.Text = MainWindow.resourcemanager.GetString("hasWarranty");
            txt_skipProperties.Text = MainWindow.resourcemanager.GetString("skipProperties");
            txt_skipSerialsNum.Text = MainWindow.resourcemanager.GetString("skipSerialsNum");

            tt_clear.Content = MainWindow.resourcemanager.GetString("trClear");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");

        }
        private async void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 150)
                {
                    _BarcodeStr = "";
                }

                string digit = "";
                // record keystroke & timestamp 

                if (e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    //digit pressed!
                    digit = e.Key.ToString().Substring(1);
                    // = "1" when D1 is pressed
                }
                else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                {
                    digit = e.Key.ToString().Substring(6); // = "1" when NumPad1 is pressed

                }
                _BarcodeStr += digit;
                _lastKeystroke = DateTime.Now;

                // process barcode 

                if (_BarcodeStr.Length == 13)
                {
                    if (_Sender != null)
                    {
                        TextBox tb = _Sender as TextBox;
                        if (tb != null)
                        {
                            if (tb.Name == "tb_code" || tb.Name == "tb_name" || tb.Name == "tb_details" || tb.Name == "tb_taxes" || tb.Name == "tb_min" || tb.Name == "tb_max")// remove barcode from text box
                            {
                                string tbString = tb.Text;
                                string newStr = "";
                                int startIndex = tbString.IndexOf(_BarcodeStr);
                                if (startIndex != -1)
                                    newStr = tbString.Remove(startIndex, _BarcodeStr.Length);

                                tb.Text = newStr;
                            }
                        }
                    }
                    tb_barcode.Text = _BarcodeStr;

                    // get item matches barcode
                    if (barcodesList != null)
                    {
                        ItemUnit unit1 = barcodesList.ToList().Find(c => c.barcode == tb_barcode.Text);

                        if (unit1 != null)
                        {
                            if (grid_barcode.Visibility == Visibility.Visible)
                            {
                                if (!await checkBarcodeValidity(tb_barcode.Text))
                                {
                                    SectionData.validateDuplicateCode(tb_barcode, p_errorBarcode, tt_errorBarcode, "trErrorDuplicateBarcodeToolTip");
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorDuplicateBarcodeToolTip"), animation: ToasterAnimation.FadeIn);
                                }
                                else
                                    SectionData.clearValidate(tb_barcode, p_errorBarcode);
                            }
                            else
                            {
                                int itemId = (int)unit1.itemId;
                                if (unit1.itemId != 0)
                                    await ChangeItemIdEvent(itemId);
                            }
                        }
                    }
                    drawBarcode(tb_barcode.Text);
                    _BarcodeStr = "";
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
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                categoryParentId = 0;
                grid_categoryControlPath.Children.Clear();
                selectedItem = 0;
                btn_clear_Click(btn_clear, null);
                MainWindow.mainWindow.KeyDown -= HandleKeyPress;
                GC.Collect();

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
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);
                // for pagination onTop Always
                btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                catigoriesAndItemsView.ucItem = this;
                MainWindow.mainWindow.KeyDown += HandleKeyPress;


                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.RightToLeft;
                }

                await RefrishCategoriesCard();
                translate();
                await fillCategories();
                await fillUnits();
                await fillBarcodeList();

                cb_categorie.SelectedIndex = -1;
                cb_itemType.SelectedIndex = 0;
                cb_minUnit.SelectedIndex = 0;
                cb_maxUnit.SelectedIndex = 0;


                await FillCombo.FillComboWarranty(cb_warranty);
                cb_warranty.SelectedIndex = -1;
                cb_warranty.IsEnabled = false;
                tgl_hasWarranty.IsChecked = false;

                if (AppSettings.itemsTax_bool == false)
                {
                    grid_taxes.Visibility = Visibility.Collapsed;
                    //tb_taxes.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grid_taxes.Visibility = Visibility.Visible;
                    //tb_taxes.Visibility = Visibility.Visible;
                }

                //if (AppSettings.canSkipProperties)
                //{
                //    sp_skipProperties.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    sp_skipProperties.Visibility = Visibility.Collapsed;
                //}

                //if (AppSettings.canSkipSerialsNum)
                //{
                //    sp_skipSerialsNum.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    sp_skipSerialsNum.Visibility = Visibility.Collapsed;
                //}

                await RefrishItems();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
                tb_code.Focus();
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async Task<Boolean> checkBarcodeValidity(string barcode)
        {

            await fillBarcodeList();
            if (barcodesList != null)
            {
                var exist = barcodesList.Where(x => x.barcode == barcode && x.itemUnitId != itemUnit.itemUnitId).FirstOrDefault();
                if (exist != null)
                    return false;
                else
                    return true;
            }
            return true;

        }

        private async void generateBarcode(string barcodeString, Boolean defaultBarcode)
        {

            if (barcodeString == "" && defaultBarcode)
            {
                barcodeString = generateRandomBarcode();
                if (barcodesList != null)
                {
                    if (!await checkBarcodeValidity(barcodeString))
                        barcodeString = generateRandomBarcode();
                }
                tb_barcode.Text = barcodeString;
                SectionData.validateEmptyTextBox(tb_barcode, p_errorBarcode, tt_errorBarcode, "trErrorEmptyBarcodeToolTip");
            }
            drawBarcode(tb_barcode.Text);
        }
        private void drawBarcode(string barcodeStr)
        {

            // configur check sum metrics
            BarcodeSymbology s = BarcodeSymbology.CodeEan13;

            BarcodeDraw drawObject = BarcodeDrawFactory.GetSymbology(s);

            BarcodeMetrics barcodeMetrics = drawObject.GetDefaultMetrics(60);
            barcodeMetrics.Scale = 2;

            if (barcodeStr != "")
            {
                if (barcodeStr.Length == 13)
                    barcodeStr = barcodeStr.Substring(1);//remove check sum from barcode string
                var barcodeImage = drawObject.Draw(barcodeStr, barcodeMetrics);

                using (MemoryStream ms = new MemoryStream())
                {
                    barcodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] imageBytes = ms.ToArray();

                    img_barcode.Source = ImageProcess.ByteToImage(imageBytes);
                }
            }
            else
                img_barcode.Source = null;

        }

        static public string generateRandomBarcode()
        {


            var now = DateTime.Now;

            var days = (int)(now - new DateTime(2000, 1, 1)).TotalDays;
            var seconds = (int)(now - DateTime.Today).TotalSeconds;

            var counter = _InternalCounter++ % 100;
            string randomBarcode = days.ToString("00000") + seconds.ToString("00000") + counter.ToString("00");
            char[] barcodeData = randomBarcode.ToCharArray();
            char checkDigit = Mod10CheckDigit(barcodeData);
            return checkDigit + randomBarcode;

        }
        private void Btn_itemData_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                grid_barcode.Visibility = grid_properties.Visibility = Visibility.Collapsed;
                grid_itemData.Visibility = Visibility.Visible;
                brd_barcodeTab.BorderBrush =

                (SolidColorBrush)(new BrushConverter().ConvertFrom("#4e4e4e"));
                brd_itemDataTab.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                tb_code.Focus();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_barcode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (item.itemId > 0)
                {
                    grid_itemData.Visibility = grid_properties.Visibility = Visibility.Collapsed;
                    grid_barcode.Visibility = Visibility.Visible;
                    btn_prices.IsEnabled = false;
                    btn_properties.IsEnabled = false;
                    brd_itemDataTab.BorderBrush =

                    (SolidColorBrush)(new BrushConverter().ConvertFrom("#4e4e4e"));
                    brd_barcodeTab.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                    tb_barcode.Focus();
                    tgl_hasWarranty.IsChecked = false;
                    cb_warranty.IsEnabled = false;
                    cb_warranty.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
       
        //0Normal Item
        //1Have Expiration date
        //2Have Serial number
        //3Service
        //4Package items

        #region add update delete , validate
        private void validateItemValues()
        {
            SectionData.validateEmptyTextBox(tb_code, p_errorCode, tt_errorCode, "trEmptyCodeToolTip");
            SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
            SectionData.validateEmptyComboBox(cb_categorie, p_errorCategorie, tt_categorie, "trErrorEmptyCategoryToolTip");
            SectionData.validateEmptyComboBox(cb_itemType, p_errorType, tt_errorType, "trErrorEmptyTypeToolTip");
            SectionData.validateEmptyComboBox(cb_minUnit, p_errorMinUnit, tt_errorMinUnit, "trErrorEmptyUnitToolTip");
            SectionData.validateEmptyComboBox(cb_maxUnit, p_errorMaxUnit, tt_errorMaxUnit, "trErrorEmptyUnitToolTip");
            SectionData.validateEmptyTextBox(tb_min, p_errorMin, tt_errorMin, "trErrorEmptyMinToolTip");
            SectionData.validateEmptyTextBox(tb_max, p_errorMax, tt_errorMax, "trErrorEmptyMaxToolTip");
        }
        private async Task<Boolean> checkCodeAvailabiltiy(string oldCode = "")
        {

            List<string> itemsCodes = await itemModel.GetItemsCodes();
            string code = tb_code.Text;
            var match = "";
            if (code != oldCode && itemsCodes != null)
                match = itemsCodes.FirstOrDefault(stringToCheck => stringToCheck.Contains(code));

            if (match != "" && match != null)
            {
                SectionData.validateDuplicateCode(tb_code, p_errorCode, tt_errorCode, "trDuplicateCodeToolTip");
                return false;
            }
            else
            {
                SectionData.clearValidate(tb_code, p_errorCode);
                return true;
            }
        }
        // add item with basic information 
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {//add

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    //validate values
                    validateItemValues();
                    Boolean codeAvailable = await checkCodeAvailabiltiy();

                    if ((!tb_code.Text.Equals("") && !tb_name.Text.Equals("") && cb_categorie.SelectedIndex != -1 && cb_itemType.SelectedIndex != -1
                        && cb_minUnit.SelectedIndex != -1 && cb_maxUnit.SelectedIndex != -1 && !tb_min.Text.Equals("") && !tb_max.Text.Equals("")) && codeAvailable ||
                       (!tb_code.Text.Equals("") && !tb_name.Text.Equals("") && cb_categorie.SelectedIndex != -1 && cb_itemType.SelectedIndex != -1 && cb_itemType.SelectedIndex == 3 && codeAvailable))
                    {
                        Nullable<int> categoryId = null;
                        if (cb_categorie.SelectedIndex != -1)
                            categoryId = (int)cb_categorie.SelectedValue;

                        int alertDays = 0;
                        int min = 0;
                        int max = 0;
                        //decimal taxes = 0;

                        if (tb_alertDays.Text != "")
                            alertDays = int.Parse(tb_alertDays.Text);
                        if (tb_min.Text != "")
                            min = int.Parse(tb_min.Text);
                        if (tb_max.Text != "")
                            max = int.Parse(tb_max.Text);
                        //if (tb_taxes.Text != "")
                        //    taxes = decimal.Parse(tb_taxes.Text);



                        Nullable<int> minUnitId = null;
                        if (cb_minUnit.SelectedIndex != -1)
                            minUnitId = units[cb_minUnit.SelectedIndex].unitId;

                        Nullable<int> maxUnitId = null;
                        if (cb_maxUnit.SelectedIndex != -1)
                            maxUnitId = units[cb_maxUnit.SelectedIndex].unitId;
                        item = new Item();
                        item.code = tb_code.Text;
                        item.name = tb_name.Text;
                        item.details = tb_details.Text;
                        item.type = selectedType;
                        item.image = "";
                        //item.taxes = taxes;
                        item.isActive = 1;
                        item.min = min;
                        item.max = max;
                        item.categoryId = categoryId;
                        item.isTaxExempt = (bool)tgl_isTaxExempt.IsChecked;
                        item.isExpired = (bool)tgl_isExpired.IsChecked;
                        item.alertDays = alertDays;

                        item.createUserId = MainWindow.userID;
                        item.updateUserId = MainWindow.userID;
                        item.minUnitId = minUnitId;
                        item.maxUnitId = maxUnitId;
                        item.warrantyId = (int?)cb_warranty.SelectedValue;

                        int res = (int)await itemModel.saveItem(item);
                        if (res == -1)// إظهار رسالة الترقية
                            Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpgrade"), animation: ToasterAnimation.FadeIn);

                        else if (res == 0) // an error occure
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        else
                        {
                            item.itemId = res;
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            int itemId = res;

                            if (openFileDialog.FileName != "")
                                await itemModel.uploadImage(openFileDialog.FileName, Md5Encription.MD5Hash("Inc-m" + itemId.ToString()), itemId);
                        }

                        await RefrishItems();

                    }
                    tb_code.Focus();
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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

        //update item
        private async void Btn_update_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update") || SectionData.isAdminPermision())
                {
                    if (item.itemId > 0)
                    {
                        //validate values
                        validateItemValues();

                        // check if code available
                        Boolean codeAvailable = await checkCodeAvailabiltiy(item.code);

                        if ((!tb_code.Text.Equals("") && !tb_name.Text.Equals("") && cb_categorie.SelectedIndex != -1 && cb_itemType.SelectedIndex != -1
                           && cb_minUnit.SelectedIndex != -1 && cb_maxUnit.SelectedIndex != -1 && !tb_min.Text.Equals("") && !tb_max.Text.Equals("")) && codeAvailable ||
                          (!tb_code.Text.Equals("") && !tb_name.Text.Equals("") && cb_categorie.SelectedIndex != -1 && cb_itemType.SelectedIndex != -1 && cb_itemType.SelectedIndex == 3 && codeAvailable))
                        {
                            Nullable<int> categoryId = null;
                            if (cb_categorie.SelectedIndex != -1)
                                categoryId = (int)cb_categorie.SelectedValue;

                            int alertDays = 0;
                            int min = 0;
                            int max = 0;
                            //decimal taxes = 0;

                            if (tb_alertDays.Text != "")
                                alertDays = int.Parse(tb_alertDays.Text);
                            if (tb_min.Text != "")
                                min = int.Parse(tb_min.Text);
                            if (tb_max.Text != "")
                                max = int.Parse(tb_max.Text);
                            //if (tb_taxes.Text != "")
                            //    taxes = decimal.Parse(tb_taxes.Text);

                            Nullable<int> minUnitId = null;
                            if (cb_minUnit.SelectedIndex != -1)
                                minUnitId = units[cb_minUnit.SelectedIndex].unitId;

                            Nullable<int> maxUnitId = null;
                            if (cb_maxUnit.SelectedIndex != -1)
                                maxUnitId = units[cb_maxUnit.SelectedIndex].unitId;

                            item.itemId = item.itemId;
                            item.code = tb_code.Text;
                            item.name = tb_name.Text;
                            item.details = tb_details.Text;
                            item.type = selectedType;
                            //item.taxes = taxes;
                            item.min = min;
                            item.max = max;
                            item.categoryId = categoryId;
                            item.isTaxExempt = (bool)tgl_isTaxExempt.IsChecked;
                            item.isExpired = (bool)tgl_isExpired.IsChecked;
                            item.alertDays = alertDays;
                            item.updateUserId = MainWindow.userID;
                            item.minUnitId = minUnitId;
                            item.maxUnitId = maxUnitId;
                            item.warrantyId = (int?)cb_warranty.SelectedValue;

                            int itemId = (int)await itemModel.saveItem(item);
                            if (!itemId.Equals("0"))
                            {
                                if (openFileDialog.FileName != "")
                                {
                                    await itemModel.uploadImage(openFileDialog.FileName, Md5Encription.MD5Hash("Inc-m" + itemId.ToString()), itemId);
                                }
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                            }
                            else
                                Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);



                            await RefrishItems();
                        }
                        tb_code.Focus();
                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectItemFirst"), animation: ToasterAnimation.FadeIn);

                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
        /*
        async void Btn_deleteService_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                    if (service.costId != 0)
                    {
                        int res = await serviceModel.delete(service.costId);

                        if (res>0)
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);
                        else
                            Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                        await refreshServicesGrid(item.itemId);

                        //tb_serviceName.Clear();
                        //tb_costVal.Clear();
                    }
                    tb_barcode.Focus();
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
        private async void Btn_delete_Click(object sender, RoutedEventArgs e)
        {//delete

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                    if ((!item.canDelete) && (item.isActive == 0))
                    {
                        #region
                        Window.GetWindow(this).Opacity = 0.2;
                        wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                        w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxActivate");
                        w.ShowDialog();
                        Window.GetWindow(this).Opacity = 1;
                        #endregion
                        if (w.isOk)
                            await activate();
                    }
                    else
                    {
                        #region
                        Window.GetWindow(this).Opacity = 0.2;
                        wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                        if (item.canDelete)
                            w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                        if (!item.canDelete)
                            w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDeactivate");
                        w.ShowDialog();
                        Window.GetWindow(this).Opacity = 1;
                        #endregion
                        if (w.isOk)
                        {
                            string popupContent = "";
                            if (item.canDelete) popupContent = MainWindow.resourcemanager.GetString("trPopDelete");
                            if ((!item.canDelete) && (item.isActive == 1)) popupContent = MainWindow.resourcemanager.GetString("trPopInActive");
                            int userId = (int)MainWindow.userID;
                            int res = (int)await itemModel.deleteItem(item.itemId, userId, item.canDelete);

                            if (res > 0)
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: popupContent, animation: ToasterAnimation.FadeIn);
                                item.itemId = 0;
                            }
                            else
                                Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        }
                    }

                    await RefrishItems();
                    //Txb_searchitems_TextChanged(null, null);

                    //clear textBoxs
                    btn_clear_Click(null, null);
                    tb_code.Focus();
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async Task activate()
        {//activate


            item.isActive = 1;

            int s = (int)await itemModel.saveItem(item);

            if (s > 0)
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopActive"), animation: ToasterAnimation.FadeIn);
            else
                Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
            await RefrishItems();
            //Txb_searchitems_TextChanged(null, null);
            tb_barcode.Focus();

        }
        /*
        private void validatePropertyValues()
        {

            SectionData.validateEmptyComboBox(cb_selectProperties, p_errorProperty, tt_errorProperty, "trEmptyPropertyToolTip");
            SectionData.validateEmptyComboBox(cb_value, p_errorValue, tt_errorValue, "trEmptyValueToolTip");
        }
        async void Btn_addProperties_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    validatePropertyValues();

                    if (cb_selectProperties.SelectedValue != null && cb_value.SelectedValue != null && item.itemId > 0)
                    {
                        int propertyItemId = propItems[cb_value.SelectedIndex].propertyItemId;
                        int itemId = (int)item.itemId;

                        // check if property assigned previously to item
                        Boolean exist = itemsProp.Any(x => x.propertyItemId == propertyItemId && x.itemId == itemId);

                        if (exist)
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopPropertyExist"), animation: ToasterAnimation.FadeIn);

                            p_errorValue.Visibility = Visibility.Visible;
                            tt_errorValue.Content = MainWindow.resourcemanager.GetString("trPopPropertyExist");
                            cb_value.Background = (Brush)bc.ConvertFrom("#15FF0000");

                            p_errorProperty.Visibility = Visibility.Visible;
                            tt_errorProperty.Content = MainWindow.resourcemanager.GetString("trPopPropertyExist");
                            cb_selectProperties.Background = (Brush)bc.ConvertFrom("#15FF0000");
                        }
                        else
                        {
                            ItemsProp itemspropObj = new ItemsProp();
                            itemspropObj.propertyItemId = propItems[cb_value.SelectedIndex].propertyItemId;
                            itemspropObj.itemId = itemId;
                            itemspropObj.updateUserId = MainWindow.userID;
                            itemspropObj.createUserId = MainWindow.userID;

                            int res = (int)await itemsPropModel.Save(itemspropObj);
                            if (res > 0) //SectionData.popUpResponse("", MainWindow.resourcemanager.GetString("trPopAdd"));
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            else //SectionData.popUpResponse("", MainWindow.resourcemanager.GetString("trPopError"));
                                Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                            await refreshPropertiesGrid(itemId);
                        }
                        //cb_value.SelectedIndex = -1;
                        //cb_selectProperties.SelectedIndex = -1;
                    }
                    cb_selectProperties.Focus();
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
        async void Btn_deleteProperties_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                    if (itemProp.itemPropId != 0)
                    {
                        int propertyItemId = (int)itemProp.propertyItemId;
                        int itemId = item.itemId;

                        int res = (int)await itemsPropModel.Delete(itemProp.itemPropId);

                        if (res > 0) //SectionData.popUpResponse("", MainWindow.resourcemanager.GetString("trPopDelete"));
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);
                        else //SectionData.popUpResponse("", MainWindow.resourcemanager.GetString("trPopError"));
                            Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                        await refreshPropertiesGrid(item.itemId);
                    }
                    cb_selectProperties.Focus();
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
        #endregion

        #region barcode
        //*****************************************8
        private bool validateUnitValues()
        {
            bool valid = false;
            SectionData.validateEmptyComboBox(cb_selectUnit, p_errorSelectUnit, tt_errorSelectUnit, "trErrorEmptyUnitToolTip");
            //SectionData.validateEmptyComboBox(cb_storageCost, p_errorStorageCost, tt_errorStorageCost, "trEmptyStoreCost");
            SectionData.validateEmptyTextBox(tb_count, p_errorCount, tt_errorCount, "trErrorEmptyCountToolTip");
            SectionData.validateEmptyComboBox(cb_unit, p_errorUnit, tt_errorUnit, "trErrorEmptyUnitToolTip");
            SectionData.validateEmptyTextBox(tb_price, p_errorPrice, tt_errorPrice, "trErrorEmptyPriceToolTip");
            SectionData.validateEmptyTextBox(tb_barcode, p_errorBarcode, tt_errorBarcode, "trEmptyBarcodeToolTip");
            SectionData.validateEmptyTextBox(tb_cost, p_errorCost, tt_errorCost, "trEmptyValueToolTip");

            if ((cb_selectUnit.SelectedIndex != -1 && !tb_count.Text.Equals("") && cb_unit.SelectedIndex != -1 && !tb_price.Text.Equals("") && !tb_cost.Text.Equals("") && !tb_barcode.Text.Equals(""))
                       || (!tb_price.Text.Equals("") && !tb_barcode.Text.Equals("") && cb_itemType.SelectedIndex == 3))
                valid = true;
            else
                valid = false;
            return valid;
        }
        // add barcode to item
        async void Btn_addBarcode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(unitBasicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    //check mandatory values
                    bool validate = validateUnitValues();
                    //cb_storageCost.SelectedIndex != -1 &&
                    if (validate)
                    {
                        if (tb_barcode.Text.Length == 12 || tb_barcode.Text.Length == 13)
                        {
                            char[] barcodeData;
                            char checkDigit;
                            bool valid = true;
                            if (tb_barcode.Text.Length == 12)// generate checksum didit
                            {
                                barcodeData = tb_barcode.Text.ToCharArray();
                                checkDigit = Mod10CheckDigit(barcodeData);
                                tb_barcode.Text = checkDigit + tb_barcode.Text;
                            }
                            else if (tb_barcode.Text.Length == 13)
                            {
                                char cd = tb_barcode.Text[0];
                                string barCode = tb_barcode.Text.Substring(1);
                                barcodeData = barCode.ToCharArray();
                                checkDigit = Mod10CheckDigit(barcodeData);
                                if (checkDigit != cd)
                                {
                                    valid = false;
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorBarcodeToolTip"), animation: ToasterAnimation.FadeIn);
                                }
                            }
                            if (valid == true)
                            {
                                // check barcode value if assigned to any item
                                if (!await checkBarcodeValidity(tb_barcode.Text))
                                {
                                    SectionData.validateDuplicateCode(tb_barcode, p_errorBarcode, tt_errorBarcode, "trErrorDuplicateBarcodeToolTip");
                                }
                                else //barcode is available
                                {
                                    Nullable<int> unitId = null;
                                    if (cb_selectUnit.SelectedIndex != -1)
                                        unitId = units[cb_selectUnit.SelectedIndex].unitId;

                                    //Nullable<int> storageCostId = null;
                                    //if (cb_storageCost.SelectedValue != null && cb_storageCost.SelectedIndex != -1)
                                    //    storageCostId = (int)cb_storageCost.SelectedValue;

                                    short defaultBurchase = 0;
                                    if (tbtn_isDefaultPurchases.IsChecked == true)
                                        defaultBurchase = 1;

                                    short defaultSale = 0;
                                    if (tbtn_isDefaultSales.IsChecked == true)
                                        defaultSale = 1;

                                    itemUnit.itemUnitId = 0;
                                    itemUnit.itemId = item.itemId;
                                    itemUnit.unitId = unitId;
                                    //itemUnit.storageCostId = storageCostId;
                                    itemUnit.unitValue = int.Parse(tb_count.Text);
                                    itemUnit.subUnitId = (int)cb_unit.SelectedValue;
                                    itemUnit.defaultSale = defaultSale;
                                    itemUnit.defaultPurchase = defaultBurchase;
                                    itemUnit.price = decimal.Parse(tb_price.Text);
                                    itemUnit.cost = decimal.Parse(tb_cost.Text);
                                    itemUnit.barcode = tb_barcode.Text;
                                    itemUnit.createUserId = MainWindow.userID;
                                    itemUnit.updateUserId = MainWindow.userID;
                                    itemUnit.warrantyId = (int?)cb_warranty.SelectedValue;
                                    itemUnit.hasWarranty = tgl_hasWarranty.IsChecked.Value;
                                    itemUnit.skipProperties = tgl_skipProperties.IsChecked.Value;
                                    itemUnit.skipSerialsNum = tgl_skipSerialsNum.IsChecked.Value;

                                    int res = (int)await itemUnit.saveItemUnit(itemUnit);
                                    if (res > 0)
                                    {
                                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                                        MainWindow.mainWindow.GlobalItemUnitsList = await MainWindow.mainWindow.GlobalItemUnit.GetIU();
                                        await FillCombo.RefreshAllUnits();
                                        Btn_unitClear(sender, e);

                                    }
                                    else
                                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                                    await refreshItemUnitsGrid(item.itemId);
                                }
                            }

                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopErrorBarcodeLength"), animation: ToasterAnimation.FadeIn);
                    }
                    tb_barcode.Focus();
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
        //**********************************************
        //**************update barcode******************
        async void Btn_updateBarcode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(unitBasicsPermission, MainWindow.groupObjects, "update") || SectionData.isAdminPermision())
                {
                    if (itemUnit.itemUnitId > 0)
                    {
                        //check mandatory values
                        bool validate = validateUnitValues();
                        if (validate)
                        {
                            if (tb_barcode.Text.Length == 12 || tb_barcode.Text.Length == 13)
                            {
                                char[] barcodeData;
                                char checkDigit;
                                bool valid = true;
                                if (tb_barcode.Text.Length == 12)// generate checksum didit
                                {
                                    barcodeData = tb_barcode.Text.ToCharArray();
                                    checkDigit = Mod10CheckDigit(barcodeData);
                                    tb_barcode.Text = checkDigit + tb_barcode.Text;
                                }
                                else if (tb_barcode.Text.Length == 13)
                                {
                                    char cd = tb_barcode.Text[0];
                                    string barCode = tb_barcode.Text.Substring(1);
                                    barcodeData = barCode.ToCharArray();
                                    checkDigit = Mod10CheckDigit(barcodeData);
                                    if (checkDigit != cd)
                                    {
                                        valid = false;
                                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorBarcodeToolTip"), animation: ToasterAnimation.FadeIn);
                                    }
                                }
                                if (valid == true)
                                {
                                    // check barcode value if assigned to any item
                                    if (!await checkBarcodeValidity(tb_barcode.Text) && itemUnit.barcode != tb_barcode.Text)
                                    {
                                        SectionData.validateDuplicateCode(tb_barcode, p_errorBarcode, tt_errorBarcode, "trErrorDuplicateBarcodeToolTip");
                                    }
                                    else //barcode is available
                                    {
                                        Nullable<int> unitId = null;
                                        if (cb_selectUnit.SelectedIndex != -1)
                                            unitId = units[cb_selectUnit.SelectedIndex].unitId;

                                        //Nullable<int> storageCostId = null;
                                        //if (cb_storageCost.SelectedValue != null && cb_storageCost.SelectedIndex > -1)
                                        //    storageCostId = (int)cb_storageCost.SelectedValue;


                                        short defaultBurchase = 0;
                                        if (tbtn_isDefaultPurchases.IsChecked == true)
                                            defaultBurchase = 1;

                                        short defaultSale = 0;
                                        if (tbtn_isDefaultSales.IsChecked == true)
                                            defaultSale = 1;

                                        itemUnit.itemId = item.itemId;
                                        itemUnit.unitId = unitId;
                                        //itemUnit.storageCostId = storageCostId;
                                        itemUnit.unitValue = int.Parse(tb_count.Text);
                                        itemUnit.subUnitId = (int)cb_unit.SelectedValue;
                                        itemUnit.defaultSale = defaultSale;
                                        itemUnit.defaultPurchase = defaultBurchase;
                                        itemUnit.price = decimal.Parse(tb_price.Text);
                                        itemUnit.cost = decimal.Parse(tb_cost.Text);
                                        itemUnit.barcode = tb_barcode.Text;
                                        itemUnit.updateUserId = MainWindow.userID;
                                        itemUnit.warrantyId = (int?)cb_warranty.SelectedValue;
                                        itemUnit.hasWarranty = tgl_hasWarranty.IsChecked.Value;
                                        itemUnit.skipProperties = tgl_skipProperties.IsChecked.Value;
                                        itemUnit.skipSerialsNum = tgl_skipSerialsNum.IsChecked.Value;

                                        int res = (int)await itemUnit.saveItemUnit(itemUnit);
                                        if (res > 0)
                                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                                        else
                                            Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                                        await refreshItemUnitsGrid(item.itemId);
                                    }
                                }
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopErrorBarcodeLength"), animation: ToasterAnimation.FadeIn);
                        }
                        tb_barcode.Focus();
                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectItemFirst"), animation: ToasterAnimation.FadeIn);

                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
        //**********************************************
        //**************delete barcode******************
        private async Task activateBarcode()
        {//activate


            itemUnit.isActive = 1;
            itemUnit.updateUserId = MainWindow.userID.Value;

            int s = (int)await itemUnit.saveItemUnit(itemUnit);

            if (s > 0)
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopActive"), animation: ToasterAnimation.FadeIn);
            else
                Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

        }
        async void Btn_deleteBarcode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(unitBasicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                    if (itemUnit.itemUnitId != 0)
                    {
                        if ((!itemUnit.canDelete) && (itemUnit.isActive == 0))
                        {
                            #region
                            Window.GetWindow(this).Opacity = 0.2;
                            wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                            w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxActivate");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion
                            if (w.isOk)
                                await activateBarcode();

                        }
                        else
                        {
                            #region
                            Window.GetWindow(this).Opacity = 0.2;
                            wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                            if (itemUnit.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                            if (!itemUnit.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDeactivate");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion
                            if (w.isOk)
                            {
                                string popupContent = "";
                                if (itemUnit.canDelete) popupContent = MainWindow.resourcemanager.GetString("trPopDelete");
                                if ((!itemUnit.canDelete) && (itemUnit.isActive == 1)) popupContent = MainWindow.resourcemanager.GetString("trPopInActive");
                                int userId = (int)MainWindow.userID;
                                int res = (int)await itemUnit.Delete(itemUnit.itemUnitId, userId, itemUnit.canDelete);

                                if (res > 0)
                                {
                                    itemUnit.itemUnitId = 0;
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: popupContent, animation: ToasterAnimation.FadeIn);
                                }
                                else
                                    Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            }
                            //Boolean res = await itemUnit.Delete(itemUnit.itemUnitId);

                            //if (res)
                            //    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);
                            //else
                            //    Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);


                        }
                        Btn_unitClear(null, null);
                        await refreshItemUnitsGrid(item.itemId);
                        tb_barcode.Focus();
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }


        #endregion barcode

        #region validate

        private void input_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                if (name == "TextBox")
                {
                    if ((sender as TextBox).Name == "tb_code")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorCode, tt_errorCode, "trEmptyCodeToolTip");
                    else if ((sender as TextBox).Name == "tb_name")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorName, tt_errorName, "trEmptyNameToolTip");
                    else if ((sender as TextBox).Name == "tb_min")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorMin, tt_errorMin, "trErrorEmptyMinToolTip");
                    else if ((sender as TextBox).Name == "tb_max")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorMax, tt_errorMax, "trErrorEmptyMaxToolTip");
                    else if ((sender as TextBox).Name == "tb_count")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorCount, tt_errorCount, "trErrorEmptyCountToolTip");
                    else if ((sender as TextBox).Name == "tb_price")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorPrice, tt_errorPrice, "trErrorEmptyPriceToolTip");
                    else if ((sender as TextBox).Name == "tb_barcode")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorBarcode, tt_errorBarcode, "trErrorEmptyBarcodeToolTip");
                    //else if ((sender as TextBox).Name == "tb_serial")
                    //    SectionData.validateEmptyTextBox((TextBox)sender, p_errorSerial, tt_errorSerial, "trEmptyCodeToolTip");

                }
                else if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_categorie")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorCategorie, tt_categorie, "trErrorEmptyCategoryToolTip");
                    else if ((sender as ComboBox).Name == "cb_itemType")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorType, tt_errorType, "trErrorEmptyCategoryToolTip");
                    else if ((sender as ComboBox).Name == "cb_minUnit")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorMinUnit, tt_errorMinUnit, "trErrorEmptyUnitToolTip");
                    else if ((sender as ComboBox).Name == "cb_maxUnit")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorMaxUnit, tt_errorMaxUnit, "trErrorEmptyUnitToolTip");
                    else if ((sender as ComboBox).Name == "cb_selectUnit")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorSelectUnit, tt_errorSelectUnit, "trErrorEmptyUnitToolTip");
                    else if ((sender as ComboBox).Name == "cb_unit")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorUnit, tt_errorUnit, "trErrorEmptyUnitToolTip");
                    //else if ((sender as ComboBox).Name == "cb_selectProperties")
                    //    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorProperty, tt_errorProperty, "trEmptyPropertyToolTip");
                    //else if ((sender as ComboBox).Name == "cb_value")
                    //    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorValue, tt_errorValue, "trEmptyValueToolTip");

                }
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }


        private void cb_selectUnit_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyComboBox(cb_selectUnit, p_errorSelectUnit, tt_errorSelectUnit, "trErrorEmptyUnitToolTip");
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void tb_count_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyTextBox(tb_count, p_errorCount, tt_errorCount, "trErrorEmptyCountToolTip");
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void cb_barcode_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!await checkBarcodeValidity(tb_barcode.Text))
                {
                    SectionData.validateDuplicateCode(tb_barcode, p_errorBarcode, tt_errorBarcode, "trErrorDuplicateBarcodeToolTip");
                }
                else
                {
                    SectionData.validateEmptyTextBox(tb_barcode, p_errorBarcode, tt_errorBarcode, "trErrorEmptyBarcodeToolTip");
                }

            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        /*
        private void Tb_taxes_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var bc = new BrushConverter();

                if (tb_taxes.Text.Equals(""))
                {
                    p_errorTaxes.Visibility = Visibility.Visible;
                    tt_errorTaxes.Content = MainWindow.resourcemanager.GetString("trEmptyNameToolTip");
                    tb_taxes.Background = (Brush)bc.ConvertFrom("#15FF0000");
                }
                else
                {
                    p_errorTaxes.Visibility = Visibility.Collapsed;
                    tb_taxes.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                }
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */
        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            try
            {
                var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
                if (regex.IsMatch(e.Text) && !(e.Text == "." && ((TextBox)sender).Text.Contains(e.Text)))
                    e.Handled = false;

                else
                    e.Handled = true;
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        

        private void Tb_barcode_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return && tb_barcode.Text.Length == 13)
                {
                    char checkDigit;
                    char[] barcodeData;
                    TextBox tb = (TextBox)sender;
                    string barCode = tb_barcode.Text;
                    char cd = barCode[0];
                    barCode = barCode.Substring(1);
                    barcodeData = barCode.ToCharArray();
                    checkDigit = Mod10CheckDigit(barcodeData);

                    if (checkDigit != cd)
                    {
                        tb_barcode.Text = "";
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorBarcodeToolTip"), animation: ToasterAnimation.FadeIn);
                    }
                    else
                        drawBarcode(barCode);
                }
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public static char Mod10CheckDigit(char[] data)
        {
            // Start the checksum calculation from the right most position.
            int factor = 3;
            int weight = 0;
            int length = data.Length;

            for (int i = 0; i <= length - 1; i++)
            {
                weight += (data[i] - '0') * factor;
                factor = (factor == 3) ? 1 : 3;
            }

            return (char)(((10 - (weight % 10)) % 10) + '0');

        }
        private void space_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox textBox = sender as TextBox;
                SectionData.InputJustNumber(ref textBox);
                e.Handled = e.Key == Key.Space;
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
        #endregion

        #region fill
        async Task fillCategories()
        {
            if (FillCombo.categoriesList is null)
                await FillCombo.RefreshCategories();
            //categories = await categoryModel.GetAllCategories(MainWindow.userID.Value);
            //if (categories != null)
            cb_categorie.ItemsSource = FillCombo.categoriesList;
            cb_categorie.SelectedValuePath = "categoryId";
            cb_categorie.DisplayMemberPath = "name";
        }
        /*
        async Task fillStorageCost()
        {
            await FillCombo.FillComboStorageCosts(cb_storageCost);
        }
        */
        private async Task fillUnits()
        {
            if (FillCombo.specialUnitsList is null)
                await FillCombo.RefreshSpecialUnits();

            units = FillCombo.specialUnitsList.ToList();

            await FillCombo.FillComboSpecialUnits(cb_minUnit);

            await FillCombo.FillComboSpecialUnits(cb_maxUnit);

            await FillCombo.FillComboSpecialUnits(cb_selectUnit);
        }
        private async Task fillSmallUnits(int itemId, int unitId)
        {
            List<Unit> units = await unitModel.getSmallUnits(itemId, unitId);

            if (itemUnits.Count().Equals(0)
                || (itemUnit != null && itemUnit.itemUnitId != 0 && itemUnit.unitId.Equals(itemUnit.subUnitId)))
            {
                cb_unit.ItemsSource = units.Where(x => x.unitId == unitId).ToList();
            }
            else
            {
                if (itemUnit != null && itemUnit.itemUnitId != 0)
                    units.Remove(units.Where(x => x.unitId == itemUnit.unitId).FirstOrDefault());

                cb_unit.ItemsSource = units.Where(x => x.name != "package"
                                                        && x.name != "service"
                                                        && itemUnits.Select(s => s.unitId).ToList().Contains(x.unitId)).ToList();

            }
            cb_unit.SelectedValuePath = "unitId";
            cb_unit.DisplayMemberPath = "name";

        }
        //async Task fillProperties()
        //{
        //    await FillCombo.FillComboPropertys(cb_selectProperties);
        //}
        async Task fillBarcodeList()
        {
            barcodesList = await itemUnitModel.getAllBarcodes();
        }

        #endregion


        #region refresh

        private async void btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                //cb_selectProperties.Focus();
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                tb_code.Clear();
                tb_name.Clear();
                tb_details.Clear();
                openFileDialog.FileName = "";
                //cb_parentItem.SelectedIndex = -1;
                cb_categorie.SelectedIndex = -1;
                cb_itemType.SelectedIndex = -1;
                //tb_taxes.Clear();
                tb_min.Text = "0";
                tb_max.Text = "0";
                tb_alertDays.Text = "0";
                cb_minUnit.SelectedIndex = -1;
                cb_maxUnit.SelectedIndex = -1;
                cb_warranty.SelectedIndex = -1;
                cb_warranty.IsEnabled = false;
                tgl_hasWarranty.IsChecked = false;
                tgl_isTaxExempt.IsChecked = false;
                tgl_isExpired.IsChecked = false;
                selectedItem = 0;
                SectionData.clearValidate(tb_code, p_errorCode);
                SectionData.clearValidate(tb_name, p_errorName);
                //SectionData.clearValidate(tb_taxes, p_errorTaxes);
                SectionData.clearValidate(tb_min, p_errorMin);
                SectionData.clearValidate(tb_max, p_errorMax);
                //SectionData.clearComboBoxValidate(cb_parentItem, p_errorParentItem);
                SectionData.clearComboBoxValidate(cb_itemType, p_errorType);
                SectionData.clearComboBoxValidate(cb_categorie, p_errorCategorie);
                SectionData.clearComboBoxValidate(cb_minUnit, p_errorMinUnit);
                SectionData.clearComboBoxValidate(cb_maxUnit, p_errorMaxUnit);

                //clear img
                Uri resourceUri = new Uri("/pic/no-image-icon-125x125.png", UriKind.Relative);
                StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
                BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                brush.ImageSource = temp;
                img_item.Background = brush;
                await fillCategories();
                //create new  object of item
                item = new Item();
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
        /*
        private void btn_clearPropertiesClick(object sender, RoutedEventArgs e)
        {
            try
            {
                cb_selectProperties.SelectedIndex = -1;
                cb_value.SelectedIndex = -1;
                //tb_serial.Text = "";
                //tb_serial.Clear();
                tb_barcode.Focus();

                itemProp = new ItemsProp();
            }
            catch (Exception ex)
            {

               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */
        private void Btn_unitClear(object sender, RoutedEventArgs e)
        {
            try
            {
                btn_prices.IsEnabled = false;
                btn_properties.IsEnabled = false;


                cb_selectUnit.SelectedIndex = -1;
                tbtn_isDefaultPurchases.IsChecked = false;
                tbtn_isDefaultSales.IsChecked = false;
                tb_count.Text = "";
                cb_unit.SelectedIndex = -1;
                tb_cost.Text = "";
                //cb_storageCost.SelectedIndex = -1;
                tb_price.Text = "";
                tb_barcode.Clear();
                tb_barcode.Focus();
                tgl_hasWarranty.IsChecked = false;
                cb_warranty.IsEnabled = false;
                cb_warranty.SelectedIndex = -1;
                itemUnit = new ItemUnit();
                grid_itemUnit.DataContext = itemUnit;
                dg_unit.SelectedIndex = -1;


                SectionData.clearComboBoxValidate(cb_selectUnit, p_errorSelectUnit);
                SectionData.clearComboBoxValidate(cb_unit, p_errorUnit);
                SectionData.clearValidate(tb_count, p_errorCount);
                SectionData.clearValidate(tb_price, p_errorPrice);
                SectionData.clearValidate(tb_barcode, p_errorBarcode);
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        //async Task refreshSerials(int itemId)
        //{

        //    // get all item serials
        //    itemSerials = await serialModel.GetItemSerials(itemId);
        //    //dg_serials.ItemsSource = new List<Serial>();
        //    dg_serials.ItemsSource = itemSerials;

        //}
        /*
        async Task refreshPropertiesGrid(int itemId)
        {

            itemsProp = await itemsPropModel.Get(item.itemId);
            //dg_properties.ItemsSource = new List<ItemsProp>();
            dg_properties.ItemsSource = itemsProp.ToList();

        }
        */
        async Task refreshItemUnitsGrid(int itemId)
        {
            itemUnits = await itemUnitModel.GetAllItemUnits(itemId);
            // dg_unit.ItemsSource = new List<ItemUnit>();
            dg_unit.ItemsSource = itemUnits.ToList();
        }
        /*
        async Task refreshServicesGrid(int itemId)
        {
            services = await serviceModel.GetItemServices(item.itemId);
            //dg_service.ItemsSource = new List<Service>();
            dg_service.ItemsSource = services.ToList();

        }
        */
        #endregion

        #region SelectionChanged
        private void dg_unit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dg_unit.SelectedIndex != -1)
                {
                    if (sender != null)
                        SectionData.StartAwait(grid_main);
                    itemUnit = dg_unit.SelectedItem as ItemUnit;
                    grid_itemUnit.DataContext = itemUnit;

                    btn_prices.IsEnabled = true;
                    btn_properties.IsEnabled = true;
                    if (itemUnit.unitId != null)
                        cb_selectUnit.SelectedValue = (int)itemUnit.unitId;
                    else
                        cb_selectUnit.SelectedValue = -1;

                    if (itemUnit.smallUnit != null)
                        cb_unit.SelectedValue = (int)itemUnit.subUnitId;
                    else
                        cb_unit.SelectedValue = -1;

                    if (itemUnit.defaultPurchase == 1)
                        tbtn_isDefaultPurchases.IsChecked = true;
                    else tbtn_isDefaultPurchases.IsChecked = false;

                    if (itemUnit.defaultSale == 1)
                        tbtn_isDefaultSales.IsChecked = true;
                    else tbtn_isDefaultSales.IsChecked = false;

                    tb_count.Text = itemUnit.unitValue.ToString();
                    tb_price.Text = SectionData.DecTostring(itemUnit.price);
                    tb_cost.Text = SectionData.DecTostring(itemUnit.cost);
                    tb_barcode.Text = itemUnit.barcode;

                    drawBarcode(itemUnit.barcode.Substring(1));

                    tgl_hasWarranty.IsChecked = itemUnit.hasWarranty;
                    cb_warranty.SelectedValue = itemUnit.warrantyId;
                    if (itemUnit.canDelete)
                    {
                        txt_deleteUnitButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                        txt_deleteUnit_Icon.Kind =
                                 MaterialDesignThemes.Wpf.PackIconKind.Delete;
                        tt_deleteUnit_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

                    }

                    else
                    {
                        if (itemUnit.isActive == 0)
                        {
                            txt_deleteUnitButton.Text = MainWindow.resourcemanager.GetString("trActive");
                            txt_deleteUnit_Icon.Kind =
                             MaterialDesignThemes.Wpf.PackIconKind.Check;
                            tt_deleteUnit_Button.Content = MainWindow.resourcemanager.GetString("trActive");

                        }
                        else
                        {
                            txt_deleteUnitButton.Text = MainWindow.resourcemanager.GetString("trInActive");
                            txt_deleteUnit_Icon.Kind =
                                 MaterialDesignThemes.Wpf.PackIconKind.Cancel;
                            tt_deleteUnit_Button.Content = MainWindow.resourcemanager.GetString("trInActive");

                        }

                    }
                    if (sender != null)
                        SectionData.EndAwait(grid_main);
                }
                tb_barcode.Focus();
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /*
        private void dg_propertiesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                p_errorName.Visibility = Visibility.Collapsed;
                p_errorCode.Visibility = Visibility.Collapsed;
                var bc = new BrushConverter();
                tb_name.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_code.Background = (Brush)bc.ConvertFrom("#f8f8f8");

                if (dg_properties.SelectedIndex != -1)
                {
                    itemProp = dg_properties.SelectedItem as ItemsProp;
                }
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                tb_barcode.Focus();
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
         */
        //private void dg_serials_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        if (sender != null)
        //            SectionData.StartAwait(grid_main);

        //        p_errorSerial.Visibility = Visibility.Collapsed;

        //        var bc = new BrushConverter();
        //        tb_serial.Background = (Brush)bc.ConvertFrom("#f8f8f8");

        //        if (dg_serials.SelectedIndex != -1)
        //        {
        //            serial = dg_serials.SelectedItem as Serial;
        //        }
        //        if (sender != null)
        //            SectionData.EndAwait(grid_main);
        //        tb_barcode.Focus();
        //    }
        //    catch (Exception ex)
        //    {
        //        if (sender != null)
        //            SectionData.EndAwait(grid_main);
        //       SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    }
        //}
        /*
        private void dg_services_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                p_errorServiceName.Visibility = Visibility.Collapsed;
                p_errorCostVal.Visibility = Visibility.Collapsed;

                var bc = new BrushConverter();
                tb_serviceName.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_costVal.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                if (dg_service.SelectedIndex != -1)
                {
                    service = dg_service.SelectedItem as Service;
                    tb_serviceName.Text = service.name;
                    tb_costVal.Text = service.costVal.ToString();
                }
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                tb_barcode.Focus();
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */
        //private async void Cb_parentItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        if (sender != null)
        //            SectionData.StartAwait(grid_main);

        //        Item parentItem = new Item();
        //        int parentId = 0;
        //        if (cb_parentItem.SelectedIndex != -1 && item.itemId == 0)
        //        {
        //            parentItem = await itemModel.GetItemByID((int)cb_parentItem.SelectedValue);
        //            parentId = parentItem.itemId;

        //            if (parentItem.categoryId != null)
        //                cb_categorie.SelectedValue = (int)parentItem.categoryId;
        //            if (parentItem.type != null)
        //            {
        //                switch (parentItem.type)
        //                {
        //                    case "n": cb_itemType.SelectedIndex = 0; break;
        //                    case "d": cb_itemType.SelectedIndex = 1; break;
        //                    case "sn": cb_itemType.SelectedIndex = 2; break;
        //                    case "sr": cb_itemType.SelectedIndex = 3; break;
        //                    case "p": cb_itemType.SelectedIndex = 4; break;
        //                }
        //            }
        //            tb_taxes.Text = SectionData.PercentageDecTostring(parentItem.taxes);

        //            tb_min.Text = parentItem.min.ToString();
        //            tb_max.Text = parentItem.max.ToString();

        //            if (parentItem.minUnitId != null)
        //                cb_minUnit.SelectedValue = (int)parentItem.minUnitId;

        //            if (parentItem.maxUnitId != null)
        //                cb_maxUnit.SelectedValue = (int)parentItem.maxUnitId;
        //        }
        //        if (sender != null)
        //            SectionData.EndAwait(grid_main);
        //        tb_barcode.Focus();
        //    }
        //    catch (Exception ex)
        //    {
        //        if (sender != null)
        //            SectionData.EndAwait(grid_main);
        //       SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    }
        //}
        /*
        async void Cb_selectProperties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_selectProperties.SelectedIndex != -1)
                {
                    int propertyId = (int) cb_selectProperties.SelectedValue;
                    propItems = FillCombo.propertysList.Where(x => x.propertyId == propertyId).FirstOrDefault().PropertiesItems;
                   // propItems = await propertyModel.GetPropertyValues(propertyId);
                    cb_value.ItemsSource = propItems.ToList();
                    cb_value.SelectedValuePath = "propertyItemId";
                    cb_value.DisplayMemberPath = "propertyItemName";
                }
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                tb_barcode.Focus();
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */
        async void Cb_categorie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //if (sender != null)
                //    SectionData.StartAwait(grid_main);
                Category cat = new Category();
                if (cb_categorie.SelectedIndex != -1)
                {
                    int catId = (int)cb_categorie.SelectedValue;
                    //cat = await categoryModel.getById(catId);

                    if (FillCombo.categoriesList is null)
                        await FillCombo.RefreshCategories();
                    cat = FillCombo.categoriesList.Where(x => x.categoryId == catId).FirstOrDefault();
                    //tb_taxes.Text = SectionData.PercentageDecTostring(cat.taxes);
                    //if (cat.fixedTax == 1)
                    //    tb_taxes.IsEnabled = false;
                    //else
                    //    tb_taxes.IsEnabled = true;
                }
                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
                tb_barcode.Focus();
            }
            catch (Exception ex)
            {
                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void CB_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //if (cb_itemType.SelectedIndex == 2)
                //{
                //   // grid_serial.Visibility = Visibility.Visible;
                //    gd_minMaxUnit.Visibility = Visibility.Visible;
                //    gd_countUnit.Visibility = Visibility.Visible;

                //}
                //else if (cb_itemType.SelectedIndex == 3)
                //{
                //   // grid_serial.Visibility = Visibility.Collapsed;
                //    gd_minMaxUnit.Visibility = Visibility.Collapsed;
                //    gd_countUnit.Visibility = Visibility.Collapsed;
                //}
                //else
                //{
                //    //grid_serial.Visibility = Visibility.Collapsed;
                //    gd_minMaxUnit.Visibility = Visibility.Visible;
                //    gd_countUnit.Visibility = Visibility.Visible;
                //}

                switch (cb_itemType.SelectedIndex)
                {
                    case 0: selectedType = "n"; break;
                    //case 1: selectedType = "d"; break;
                    case 1: selectedType = "sn"; break;
                        //case 3: selectedType = "sr"; break;
                        //case 4: selectedType = "p"; break;
                }
                if(selectedType == "sn")
                {
                    sp_skipSerialsNum.Visibility = Visibility.Visible;
                }
                else
                {
                    sp_skipSerialsNum.Visibility = Visibility.Collapsed;
                }
                tb_barcode.Focus();
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion


        #region Categor and Item
        #region Refrish Y
        /// <summary>
        /// Category
        /// </summary>
        /// <returns></returns>
        async Task<IEnumerable<Category>> RefrishCategories()
        {
            if (FillCombo.categoriesList is null)
                await FillCombo.RefreshCategories();
            categories = FillCombo.categoriesList;
            return categories;
        }
        async Task RefrishCategoriesCard()
        {
            //if (categories is null)
            await RefrishCategories();
            categoriesQuery = categories.Where(x => x.isActive == tglCategoryState && x.parentId == categoryParentId).ToList();
            catigoriesAndItemsView.gridCatigories = grid_categoryCards;
            generateCoulmnCategoriesGrid(categoriesQuery.Count());
            catigoriesAndItemsView.FN_refrishCatalogCard(categoriesQuery.ToList(), -1);
        }
        void generateCoulmnCategoriesGrid(int column)
        {

            #region
            grid_categoryCards.ColumnDefinitions.Clear();
            ColumnDefinition[] cd = new ColumnDefinition[column];
            for (int i = 0; i < column; i++)
            {
                cd[i] = new ColumnDefinition();
                cd[i].Width = new GridLength(110, GridUnitType.Pixel);
                grid_categoryCards.ColumnDefinitions.Add(cd[i]);
            }
            #endregion

        }
        /// <summary>
        /// Item
        /// </summary>
        /// <returns></returns>

        async Task<IEnumerable<Item>> RefrishItems()
        {

            allItems = await itemModel.GetAllItems();
            allItems = allItems.Where(x => x.type != "p" && x.type != "sr").ToList();


            Txb_searchitems_TextChanged(null, null);
            return items;

        }

        void RefrishItemsDatagrid(IEnumerable<Item> _items)
        {
            dg_items.ItemsSource = _items;
            txt_count.Text = _items.ToList().Count.ToString();
        }


        void RefrishItemsCard(IEnumerable<Item> _items)
        {
            grid_itemContainerCard.Children.Clear();
            catigoriesAndItemsView.gridCatigorieItems = grid_itemContainerCard;
            catigoriesAndItemsView.FN_refrishCatalogItem(_items.ToList(), "en", "purchase");
        }
        private void Grid_containerCard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                RefrishItemsCard(pagination.refrishPagination(itemsQuery, pageIndex, btns));
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
        #endregion
        #region Get Id By Click  Y
        private async void dg_items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Btn_itemData_Click(null, null);
                p_errorName.Visibility = Visibility.Collapsed;
                p_errorCode.Visibility = Visibility.Collapsed;
                var bc = new BrushConverter();
                tb_name.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_code.Background = (Brush)bc.ConvertFrom("#f8f8f8");

                if (dg_items.SelectedIndex != -1)
                {
                    item = dg_items.SelectedItem as Item;
                    this.DataContext = item;

                    //await refreshPropertiesGrid(item.itemId);
                    //await refreshSerials(item.itemId);
                    await refreshItemUnitsGrid(item.itemId);

                }
                if (item != null)
                {
                    tb_code.Text = item.code;
                    tb_name.Text = item.name;
                    tb_details.Text = item.details;

                    //if (item.parentId != null && item.parentId != 0)
                    //{
                    //    cb_parentItem.SelectedValue = (int)item.parentId;
                    //}
                    //else
                    //    cb_parentItem.SelectedValue = -1;

                    if (item.categoryId != null)
                    {
                        cb_categorie.SelectedValue = (int)item.categoryId;
                    }
                    else
                        cb_categorie.SelectedValue = -1;

                    if (item.type != null)
                    {
                        cb_itemType.SelectedValue = item.type;
                        switch (item.type)
                        {
                            case "n": cb_itemType.SelectedIndex = 0; break;
                            //case "d": cb_itemType.SelectedIndex = 1; break;
                            case "sn": cb_itemType.SelectedIndex = 1; break;
                                //case "sr": cb_itemType.SelectedIndex = 3; break;
                                //case "p": cb_itemType.SelectedIndex = 4; break;
                        }
                    }
                    else
                        cb_itemType.SelectedValue = -1;

                    //tb_taxes.Text = SectionData.PercentageDecTostring(item.taxes);

                    tb_min.Text = item.min.ToString();
                    tb_max.Text = item.max.ToString();

                    tgl_isTaxExempt.IsChecked = item.isTaxExempt;
                    tgl_isExpired.IsChecked = item.isExpired;
                    tb_alertDays.Text = item.alertDays.ToString();

                    if (item.minUnitId != null)
                        cb_minUnit.SelectedValue = (int)item.minUnitId;
                    else
                        cb_minUnit.SelectedValue = -1;

                    if (item.maxUnitId != null)
                        cb_maxUnit.SelectedValue = (int)item.maxUnitId;
                    else
                        cb_minUnit.SelectedValue = -1;

                    if (item.canDelete) btn_delete.Content = MainWindow.resourcemanager.GetString("trDelete");

                    else
                    {
                        if (item.isActive == 0) btn_delete.Content = MainWindow.resourcemanager.GetString("trActive");
                        else btn_delete.Content = MainWindow.resourcemanager.GetString("trInActive");
                    }

                    getImg();



                }
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                tb_barcode.Focus();
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void getImg()
        {

            try
            {
                if (string.IsNullOrEmpty(item.image))
                {
                    SectionData.clearImg(img_item);
                }
                else
                {
                    //byte[] imageBuffer = await itemModel.downloadImage(item.image); // read this as BLOB from your DB
                    byte[] imageBuffer = SectionData.readLocalImage(item.image, Global.TMPItemsFolder);

                    if (imageBuffer != null)
                    {
                        var bitmapImage = new BitmapImage();
                        if (imageBuffer != null)
                        {
                            using (var memoryStream = new MemoryStream(imageBuffer))
                            {
                                bitmapImage.BeginInit();
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.StreamSource = memoryStream;
                                bitmapImage.EndInit();
                            }

                            img_item.Background = new ImageBrush(bitmapImage);
                        }
                        // configure trmporary path
                        string dir = Directory.GetCurrentDirectory();
                        string tmpPath = System.IO.Path.Combine(dir, Global.TMPItemsFolder);
                        tmpPath = System.IO.Path.Combine(tmpPath, item.image);
                    }
                    else
                    {
                        SectionData.clearImg(img_item);
                    }

                }
            }
            catch
            {
                SectionData.clearImg(img_item);
            }
        }
        int selectedCategory = 0;
        public async Task ChangeCategoryIdEvent(int categoryId)
        {
            if (!selectedCategory.Equals(categoryId))
            {
                selectedCategory = categoryId;
                allCategory = false;

                category = categories.ToList().Find(c => c.categoryId == categoryId);
                if (categories.Where(x =>
                x.isActive == tglCategoryState && x.parentId == category.categoryId).Count() != 0)
                {
                    categoryParentId = category.categoryId;
                    await RefrishCategoriesCard();
                }

                await generateTrack(categoryId);
                // await RefrishItems();
                pageIndex = 1;
                Txb_searchitems_TextChanged(null, null);
                tb_barcode.Focus();
            }

        }

        int selectedItem = 0;
        public async Task ChangeItemIdEvent(int itemId)
        {
            if (!selectedItem.Equals(itemId))
            {
                btn_clear_Click(null,null);
                try
                {
                    selectedItem = itemId;
                    SectionData.StartAwait(grid_main);

                    p_errorName.Visibility = Visibility.Collapsed;
                    p_errorCode.Visibility = Visibility.Collapsed;
                    var bc = new BrushConverter();
                    tb_name.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                    tb_code.Background = (Brush)bc.ConvertFrom("#f8f8f8");

                    item = allItems.ToList().Find(c => c.itemId == itemId);
                    if (item != null)
                    {
                        
                        this.DataContext = item;

                        Btn_unitClear(null, null);

                        //await refreshPropertiesGrid(item.itemId);
                        // await refreshSerials(item.itemId);
                        await refreshItemUnitsGrid(item.itemId);

                        tb_code.Text = item.code;
                        tb_name.Text = item.name;
                        tb_details.Text = item.details;

                        //if (item.parentId != null && item.parentId != 0)
                        //{
                        //    cb_parentItem.SelectedValue = (int)item.parentId;
                        //}
                        //else
                        //    cb_parentItem.SelectedValue = -1;

                        if (item.categoryId != null)
                        {
                            cb_categorie.SelectedValue = (int)item.categoryId;
                        }
                        else
                            cb_categorie.SelectedValue = -1;

                        if (item.type != null)
                        {
                            cb_itemType.SelectedValue = item.type;
                            switch (item.type)
                            {
                                case "n": cb_itemType.SelectedIndex = 0; btn_addBarcode.IsEnabled = true; break;
                                //case "d": cb_itemType.SelectedIndex = 1; btn_addBarcode.IsEnabled = true; break;
                                case "sn": cb_itemType.SelectedIndex = 1; btn_addBarcode.IsEnabled = true; break;
                                    //case "sr": cb_itemType.SelectedIndex = 3; btn_addBarcode.IsEnabled = true; break;
                                    //case "p":
                                    //    cb_itemType.SelectedIndex = 4;
                                    //    if (itemUnits != null && itemUnits.Count == 1) btn_addBarcode.IsEnabled = false;
                                    //    else btn_addBarcode.IsEnabled = true;
                                    //    break;
                            }
                        }
                        else
                            cb_itemType.SelectedValue = -1;

                        //tb_taxes.Text = SectionData.PercentageDecTostring(item.taxes);
                        tb_min.Text = item.min.ToString();
                        tb_max.Text = item.max.ToString();

                        tgl_isTaxExempt.IsChecked = item.isTaxExempt;
                        tgl_isExpired.IsChecked = item.isExpired;
                        tb_alertDays.Text = item.alertDays.ToString();

                        if (item.minUnitId != null)
                            cb_minUnit.SelectedValue = (int)item.minUnitId;
                        else
                            cb_minUnit.SelectedValue = -1;

                        if (item.maxUnitId != null)
                            cb_maxUnit.SelectedValue = (int)item.maxUnitId;
                        else
                            cb_minUnit.SelectedValue = -1;

                        if (item.canDelete) btn_delete.Content = MainWindow.resourcemanager.GetString("trDelete");

                        else
                        {
                            if (item.isActive == 0) btn_delete.Content = MainWindow.resourcemanager.GetString("trActive");
                            else btn_delete.Content = MainWindow.resourcemanager.GetString("trInActive");
                        }

                        //get item image
                        getImg();


                    }
                    tb_barcode.Focus();
                    SectionData.EndAwait(grid_main);
                }
                catch (Exception ex)
                {
                    SectionData.EndAwait(grid_main);
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
        }

        #endregion


        #region Toggle Button Y

        /// <summary>
        /// Item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tgl_itemIsActive_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                tglItemState = 1;
                pageIndex = 1;
                Txb_searchitems_TextChanged(null, null);
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
        private void Tgl_itemIsActive_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                tglItemState = 0;
                pageIndex = 1;
                Txb_searchitems_TextChanged(null, null);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                tb_barcode.Focus();
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
        #region Switch Card/DataGrid Y

        private void Btn_itemsInCards_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                grid_itemsDatagrid.Visibility = Visibility.Collapsed;
                grid_itemCards.Visibility = Visibility.Visible;
                grid_pagination.Visibility = Visibility.Visible;
                path_itemsInCards.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                path_itemsInGrid.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4e4e4e"));

                tgl_itemIsActive.IsChecked = (tglItemState == 1) ? true : false;
                pageIndex = 1;
                Txb_searchitems_TextChanged(null, null);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                tb_barcode.Focus();
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_itemsInGrid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                grid_itemCards.Visibility = Visibility.Collapsed;
                grid_pagination.Visibility = Visibility.Collapsed;
                grid_itemsDatagrid.Visibility = Visibility.Visible;
                path_itemsInGrid.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                path_itemsInCards.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4e4e4e"));

                tgl_itemIsActive.IsChecked = (tglItemState == 1) ? true : false;
                pageIndex = 1;
                Txb_searchitems_TextChanged(null, null);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                tb_barcode.Focus();
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
        #region Search Y
        /// <summary>
        /// Item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private async void Txb_searchitems_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    if (allItems is null)
                        await RefrishItems();
                    txtItemSearch = tb_search.Text.ToLower();
                    //pageIndex = 1;


                    if (category.categoryId != 0 && !allCategory)
                    {
                        // items = await itemModel.GetItemsInCategoryAndSub(category.categoryId);
                        var categoriesId = await itemModel.getCategoryAndSubs(category.categoryId);
                        items = allItems.Where(x => categoriesId.Contains((int)x.categoryId)).ToList();
                    }
                    else
                        items = allItems.ToList();

                    #region
                    itemsQuery = items.Where(x => (x.code.ToLower().Contains(txtItemSearch) ||
                    x.name.ToLower().Contains(txtItemSearch) ||
                    x.details.ToLower().Contains(txtItemSearch)
                    ) && x.isActive == tglItemState);
                    txt_count.Text = itemsQuery.Count().ToString();
                    if (btns is null)
                        btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                    RefrishItemsCard(pagination.refrishPagination(itemsQuery, pageIndex, btns));
                    #endregion
                    RefrishItemsDatagrid(itemsQuery);
                    tb_barcode.Focus();
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
        #endregion
        #region Pagination Y
        Pagination pagination = new Pagination();
        Button[] btns;
        public int pageIndex = 1;

        private void Tb_pageNumberSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                try
                {
                    if (int.Parse(tb_pageNumberSearch.Text) < 1)
                        return;
                }
                catch
                {
                    if (!string.IsNullOrWhiteSpace(tb_pageNumberSearch.Text))
                    {
                        return;
                    }
                }

                if (sender != null)
                    SectionData.StartAwait(grid_main);


                itemsQuery = items.Where(x => x.isActive == tglItemState);

                if (tb_pageNumberSearch.Text.Equals(""))
                {
                    pageIndex = 1;
                }
                else if (((itemsQuery.Count() - 1) / 9) + 1 < int.Parse(tb_pageNumberSearch.Text))
                {
                    pageIndex = ((itemsQuery.Count() - 1) / 9) + 1;
                }
                else
                {
                    pageIndex = int.Parse(tb_pageNumberSearch.Text);
                }

                #region
                itemsQuery = items.Where(x => (x.code.ToLower().Contains(txtItemSearch) ||
                x.name.ToLower().Contains(txtItemSearch) ||
                x.details.ToLower().Contains(txtItemSearch)
                ) && x.isActive == tglItemState);
                txt_count.Text = itemsQuery.Count().ToString();
                RefrishItemsCard(pagination.refrishPagination(itemsQuery, pageIndex, btns));
                #endregion
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


        private void Btn_firstPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                pageIndex = 1;
                #region
                itemsQuery = items.Where(x => (x.code.ToLower().Contains(txtItemSearch) ||
                x.name.ToLower().Contains(txtItemSearch) ||
                x.details.ToLower().Contains(txtItemSearch)
                ) && x.isActive == tglItemState);
                txt_count.Text = itemsQuery.Count().ToString();
                RefrishItemsCard(pagination.refrishPagination(itemsQuery, pageIndex, btns));
                #endregion
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
        private void Btn_prevPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                pageIndex = int.Parse(btn_prevPage.Content.ToString());
                #region
                itemsQuery = items.Where(x => (x.code.ToLower().Contains(txtItemSearch) ||
                x.name.ToLower().Contains(txtItemSearch) ||
                x.details.ToLower().Contains(txtItemSearch)
                ) && x.isActive == tglItemState);
                txt_count.Text = itemsQuery.Count().ToString();
                RefrishItemsCard(pagination.refrishPagination(itemsQuery, pageIndex, btns));
                #endregion
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
        private void Btn_activePage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                pageIndex = int.Parse(btn_activePage.Content.ToString());
                #region
                itemsQuery = items.Where(x => (x.code.ToLower().Contains(txtItemSearch) ||
                x.name.ToLower().Contains(txtItemSearch) ||
                x.details.ToLower().Contains(txtItemSearch)
                ) && x.isActive == tglItemState);
                txt_count.Text = itemsQuery.Count().ToString();
                RefrishItemsCard(pagination.refrishPagination(itemsQuery, pageIndex, btns));
                #endregion
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
        private void Btn_nextPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                pageIndex = int.Parse(btn_nextPage.Content.ToString());
                #region
                itemsQuery = items.Where(x => (x.code.ToLower().Contains(txtItemSearch) ||
                x.name.ToLower().Contains(txtItemSearch) ||
                x.details.ToLower().Contains(txtItemSearch)
                ) && x.isActive == tglItemState);
                txt_count.Text = itemsQuery.Count().ToString();
                RefrishItemsCard(pagination.refrishPagination(itemsQuery, pageIndex, btns));
                #endregion
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
        private void Btn_lastPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                itemsQuery = items.Where(x => x.isActive == tglCategoryState);
                pageIndex = ((itemsQuery.Count() - 1) / 9) + 1;
                #region
                itemsQuery = items.Where(x => (x.code.ToLower().Contains(txtItemSearch) ||
                x.name.ToLower().Contains(txtItemSearch) ||
                x.details.ToLower().Contains(txtItemSearch)
                ) && x.isActive == tglItemState);
                txt_count.Text = itemsQuery.Count().ToString();
                RefrishItemsCard(pagination.refrishPagination(itemsQuery, pageIndex, btns));
                #endregion
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
        #endregion
        #region categoryPathControl Y

        async Task generateTrack(int categorypaPathId)
        {

            grid_categoryControlPath.Children.Clear();
            IEnumerable<Category> categoriesPath = await
            categoryModel.GetCategoryTreeByID(categorypaPathId);

            int count = 0;
            foreach (var item in categoriesPath.Reverse())
            {
                if (categories.Where(x => x.parentId == item.categoryId).Count() != 0)
                {
                    Button b = new Button();
                    b.Content = " > " + item.name + " ";
                    b.Padding = new Thickness(0);
                    b.Margin = new Thickness(0);
                    b.Background = null;
                    b.BorderThickness = new Thickness(0);
                    b.FontFamily = Application.Current.Resources["Font-cairo-light"] as FontFamily;
                    b.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#6e6e6e"));
                    b.FontSize = 14;
                    Grid.SetColumn(b, count);
                    b.DataContext = item;
                    b.Name = "category" + item.categoryId;
                    b.Tag = item.categoryId;
                    b.Click += new RoutedEventHandler(getCategoryIdFromPath);
                    count++;
                    grid_categoryControlPath.Children.Add(b);
                }
            }
            tb_barcode.Focus();


        }
        private async void getCategoryIdFromPath(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Button b = (Button)sender;
                allCategory = false;

                if (!string.IsNullOrEmpty(b.Tag.ToString()))
                {
                    await generateTrack(int.Parse(b.Tag.ToString()));
                    categoryParentId = int.Parse(b.Tag.ToString());
                    await RefrishCategoriesCard();


                    //allItems = await itemModel.GetAllItems();
                    //items = await itemModel.GetItemsInCategoryAndSub(categoryParentId.Value);
                    //items = items.Where(x => x.type != "p" && x.type != "sr").ToList();
                }
                //await RefrishItems();
                pageIndex = 1;
                Txb_searchitems_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
                tb_barcode.Focus();
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        bool allCategory = true;
        private async void Btn_getAllCategory_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                allCategory = true;
                selectedCategory = 0;
                categoriesQuery = categories.Where(x => x.isActive == tglCategoryState && x.parentId == 0).ToList();
                catigoriesAndItemsView.gridCatigories = grid_categoryCards;
                generateCoulmnCategoriesGrid(categoriesQuery.Count());
                catigoriesAndItemsView.FN_refrishCatalogCard(categoriesQuery.ToList(), -1);
                grid_categoryControlPath.Children.Clear();
                #region 
                //allItems = await itemModel.GetAllItems();
                //items = allItems.ToList();
                //items = items.Where(x => x.type != "p" && x.type != "sr").ToList();
                #endregion
                pageIndex = 1;
                Txb_searchitems_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
                tb_barcode.Focus();
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion

        #endregion
        #region Excel
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    #region
                    //Thread t1 = new Thread(() =>
                    //{
                    List<ReportParameter> paramarr = new List<ReportParameter>();

                    string addpath;
                    bool isArabic = ReportCls.checkLang();
                    if (isArabic)
                    {
                        addpath = @"\Reports\Catalog\Ar\ArItemReport.rdlc";
                    }
                    else addpath = @"\Reports\Catalog\En\ItemReport.rdlc";
                    string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

                    ReportCls.checkLang();

                    clsReports.itemReport(itemsQuery, rep, reppath, paramarr);
                    clsReports.setReportLanguage(paramarr);
                    clsReports.Header(paramarr);

                    rep.SetParameters(paramarr);

                    rep.Refresh();
                    this.Dispatcher.Invoke(() =>
                    {
                        saveFileDialog.Filter = "EXCEL|*.xls;";
                        if (saveFileDialog.ShowDialog() == true)
                        {
                            string filepath = saveFileDialog.FileName;
                            LocalReportExtensions.ExportToExcel(rep, filepath);
                        }
                    });


                    //});
                    //t1.Start();
                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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

        void FN_ExportToExcel()
        {
            var QueryExcel = itemsQuery.AsEnumerable().Select(x => new
            {
                code = x.code,
                name = x.name,
                details = x.details,
                parentName = x.parentName,
                categoryName = x.categoryName,
                type = convertItemType(x.type),
                taxes = x.taxes,
                minUnit = x.min + " " + x.minUnitName,
                maxUnit = x.max + " " + x.maxUnitName,
            });
            var DTForExcel = QueryExcel.ToDataTable();
            DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trCode");
            DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trName");
            DTForExcel.Columns[2].Caption = MainWindow.resourcemanager.GetString("trDetails");
            DTForExcel.Columns[3].Caption = MainWindow.resourcemanager.GetString("trParentName");
            DTForExcel.Columns[4].Caption = MainWindow.resourcemanager.GetString("trCategorie");
            DTForExcel.Columns[5].Caption = MainWindow.resourcemanager.GetString("trItemType");
            DTForExcel.Columns[6].Caption = MainWindow.resourcemanager.GetString("trTax");
            DTForExcel.Columns[7].Caption = MainWindow.resourcemanager.GetString("trMinUnit");
            DTForExcel.Columns[8].Caption = MainWindow.resourcemanager.GetString("trMaxUnit");
            ExportToExcel.Export(DTForExcel);
        }
        string convertItemType(string s)
        {

            switch (s)
            {
                case "n":
                    s = MainWindow.resourcemanager.GetString("trNormalItem");
                    break;
                case "d":
                    s = MainWindow.resourcemanager.GetString("trHaveExpirationDate");
                    break;
                case "sn":
                    s = MainWindow.resourcemanager.GetString("trHaveSerialNumber");
                    break;
                case "sr":
                    s = MainWindow.resourcemanager.GetString("trService");
                    break;
                case "p":
                    s = MainWindow.resourcemanager.GetString("trPackageItems");
                    break;
            }
            return s;

        }
        #endregion

        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    await RefrishItems();
                    //Txb_searchitems_TextChanged(null, null);
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
        private void Img_item_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //select image
                openFileDialog.Filter = "Images|*.png;*.jpg;*.bmp;*.jpeg;*.jfif";
                if (openFileDialog.ShowDialog() == true)
                {
                    brush.ImageSource = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.Relative));
                    img_item.Background = brush;
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
        private void Tb_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var txb = sender as TextBox;
                if ((sender as TextBox).Name == "tb_taxes"
                    || (sender as TextBox).Name == "tb_min"
                    || (sender as TextBox).Name == "tb_max")
                    SectionData.InputJustNumber(ref txb);
                _Sender = sender;
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    win_lvcCatalog win = new win_lvcCatalog(itemsQuery, 2);
                    win.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }


        private async void Cb_selectUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cb_selectUnit.SelectedIndex != -1)
                {
                    if (sender != null)
                        SectionData.StartAwait(grid_main);

                    if (cb_unit.SelectedIndex != -1 && (int)cb_selectUnit.SelectedValue == (int)cb_unit.SelectedValue)
                    {
                        tb_count.Text = "1";
                    }
                    await fillSmallUnits(item.itemId, (int)cb_selectUnit.SelectedValue);
                    if (itemUnit.itemUnitId == 0)
                        generateBarcode("", true);
                    cb_unit.SelectedValue = itemUnit.subUnitId;
                    if (sender != null)
                        SectionData.EndAwait(grid_main);
                    tb_barcode.Focus();
                }
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Cb_unit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_selectUnit.SelectedIndex != -1 && cb_unit.SelectedIndex != -1)
                {
                    if ((int)cb_selectUnit.SelectedValue == (int)cb_unit.SelectedValue)
                        tb_count.Text = "1";
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
        //private void Cb_parentItem_KeyUp(object sender, KeyEventArgs e)
        //{
        //    try
        //    {
        //        cb_parentItem.ItemsSource = items.Where(x => x.name.Contains(cb_parentItem.Text));
        //    }
        //    catch (Exception ex)
        //    {

        //       SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    }
        //}
        private void Cb_categorie_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                cb_categorie.ItemsSource = categories.Where(x => x.name.Contains(cb_categorie.Text));

            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        //private async Task fillParentItemCombo()
        //{

        //    if (items is null)
        //        await RefrishItems();


        //    //var listCa = items.Where(x => x.isActive == 1).ToList();

        //    var cat = new Item();
        //    cat.itemId = 0;
        //    cat.name = "-";
        //    items.Insert(0, cat);
        //    cb_parentItem.ItemsSource = items;
        //    cb_parentItem.SelectedValuePath = "itemId";
        //    cb_parentItem.DisplayMemberPath = "name";

        //}
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    Thread t1 = new Thread(() =>
                    {
                        pdfitem();
                    });
                    t1.Start();
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

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

        public void pdfitem()
        {

            BuildReport();

            this.Dispatcher.Invoke(() =>
            {
                saveFileDialog.Filter = "PDF|*.pdf;";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;
                    LocalReportExtensions.ExportToPDF(rep, filepath);
                }
            });
        }

        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string searchval = "";
            string stateval = "";
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            string addpath;
            if (isArabic)
            {
                addpath = @"\Reports\Catalog\Ar\ArItemReport.rdlc";
            }
            else
            {
                addpath = @"\Reports\Catalog\En\ItemReport.rdlc";
            }
            //filter   
            stateval = tgl_itemIsActive.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trActive_")
              : MainWindow.resourcemanagerreport.GetString("trNotActive");
            paramarr.Add(new ReportParameter("stateval", stateval));
            paramarr.Add(new ReportParameter("trActiveState", MainWindow.resourcemanagerreport.GetString("trState")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            searchval = tb_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            clsReports.itemReport(itemsQuery, rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            rep.SetParameters(paramarr);
            rep.Refresh();
        }

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    #region
                    Window.GetWindow(this).Opacity = 0.2;
                    /////////////////////
                    string pdfpath = "";
                    pdfpath = @"\Thumb\report\temp.pdf";
                    pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                    BuildReport();
                    LocalReportExtensions.ExportToPDF(rep, pdfpath);
                    ///////////////////
                    wd_previewPdf w = new wd_previewPdf();
                    w.pdfPath = pdfpath;
                    if (!string.IsNullOrEmpty(w.pdfPath))
                    {
                        w.ShowDialog();
                        w.wb_pdfWebViewer.Dispose();
                    }
                    Window.GetWindow(this).Opacity = 1;
                    //////////////////////////////////////
                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    /////////////////////////////////////
                    Thread t1 = new Thread(() =>
                    {
                        printitem();
                    });
                    t1.Start();
                    //////////////////////////////////////
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

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

        private void printitem()
        {
            BuildReport();

            this.Dispatcher.Invoke(() =>
            {
                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
            });
        }

        private void Tgl_hasWarranty_Checked(object sender, RoutedEventArgs e)
        {
            cb_warranty.IsEnabled = true;

        }

        private void Tgl_hasWarranty_Unchecked(object sender, RoutedEventArgs e)
        {
            cb_warranty.IsEnabled = false;

        }

        private void Tb_count_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cb_selectUnit.SelectedIndex != -1 && cb_unit.SelectedIndex != -1 && (int)cb_selectUnit.SelectedValue == (int)cb_unit.SelectedValue)
            {
                tb_count.Text = "1";
            }
        }
        private void Btn_properties_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //items
                if (itemUnit.itemUnitId > 0)
                {

                    //if (MainWindow.groupObject.HasPermissionAction(itemsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                    {
                        Window.GetWindow(this).Opacity = 0.2;

                        wd_itemUnitProperties w = new wd_itemUnitProperties();
                        w.itemUnitId = itemUnit.itemUnitId;
                        w.ShowDialog();
                        Window.GetWindow(this).Opacity = 1;
                    }
                    //else
                    //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                }

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_prices_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //items
                if (itemUnit.itemUnitId > 0)
                {

                    //if (MainWindow.groupObject.HasPermissionAction(itemsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                    {
                        Window.GetWindow(this).Opacity = 0.2;

                        wd_prices w = new wd_prices();
                        w.itemUnitId = itemUnit.itemUnitId;
                        w.ShowDialog();
                        Window.GetWindow(this).Opacity = 1;
                    }
                    //else
                    //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                }

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tgl_isExpired_Checked(object sender, RoutedEventArgs e)
        {
            tb_alertDays.IsEnabled = true;
        }

        private void Tgl_isExpired_Unchecked(object sender, RoutedEventArgs e)
        {
            tb_alertDays.IsEnabled = false;
        }
    }
}
