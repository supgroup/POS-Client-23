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
using Microsoft.Reporting.WinForms;

namespace POS.View.catalog
{
    /// <summary>
    /// Interaction logic for uc_serviceItem.xaml
    /// </summary>
    public partial class uc_serviceItem : UserControl
    {

        string basicsPermission = "service_basics";
        private static uc_serviceItem _instance;
        public static uc_serviceItem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_serviceItem();
                return _instance;
            }
        }

        #region variables
        public uc_serviceItem()
        {
            try
            {
                InitializeComponent();
                if (System.Windows.SystemParameters.PrimaryScreenWidth >= 1440)
                {
                    txt_deleteButton.Visibility = Visibility.Visible;
                    txt_addButton.Visibility = Visibility.Visible;
                    txt_updateButton.Visibility = Visibility.Visible;
                    txt_add_Icon.Visibility = Visibility.Visible;
                    txt_update_Icon.Visibility = Visibility.Visible;
                    txt_delete_Icon.Visibility = Visibility.Visible;
                }
                else if (System.Windows.SystemParameters.PrimaryScreenWidth >= 1360)
                {
                    txt_add_Icon.Visibility = Visibility.Collapsed;
                    txt_update_Icon.Visibility = Visibility.Collapsed;
                    txt_delete_Icon.Visibility = Visibility.Collapsed;
                    txt_deleteButton.Visibility = Visibility.Visible;
                    txt_addButton.Visibility = Visibility.Visible;
                    txt_updateButton.Visibility = Visibility.Visible;

                }
                else
                {
                    txt_deleteButton.Visibility = Visibility.Collapsed;
                    txt_addButton.Visibility = Visibility.Collapsed;
                    txt_updateButton.Visibility = Visibility.Collapsed;
                    txt_add_Icon.Visibility = Visibility.Visible;
                    txt_update_Icon.Visibility = Visibility.Visible;
                    txt_delete_Icon.Visibility = Visibility.Visible;

                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        Category categoryModel = new Category();
        List<Category> categories;
        static private int _InternalCounter = 0;
        List<ItemUnit> barcodesList;
        ItemUnit itemUnitModel = new ItemUnit();
        // item object
        Item item = new Item();
        // item unit object
        ItemUnit itemUnit = new ItemUnit();
        OpenFileDialog openFileDialog = new OpenFileDialog();
        Item itemModel = new Item();
        Unit unitModel = new Unit();
        IEnumerable<Category> categoriesQuery;
        IEnumerable<Item> items;
        IEnumerable<Item> itemsQuery;
        Category category = new Category();
        ItemsProp itemProp = new ItemsProp();
        ImageBrush brush = new ImageBrush();
        DataGrid dt = new DataGrid();
        List<int> categoryIds = new List<int>();
        List<string> categoryNames = new List<string>();
        //List<Property> properties;
        //List<PropertiesItems> propItems;
        List<Unit> units = new List<Unit>();
        //List<ItemsProp> itemsProp;
        //List<Serial> itemSerials;
        List<ItemUnit> itemUnits;
        //List<Service> services;
        public byte tglCategoryState = 1;
        public byte tglItemState;
        int? categoryParentId = 0;
        public string txtItemSearch;
        CatigoriesAndItemsView catigoriesAndItemsView = new CatigoriesAndItemsView();
        List<int> unitIds = new List<int>();
        List<string> unitNames = new List<string>();
        int unitServiceId = 0;

        // report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        #endregion

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);
                //btn_items.IsEnabled = false;
                btn_prices.IsEnabled = false;

                btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                catigoriesAndItemsView.ucServiceItem = this;

                #region translate
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
                translate();
                #endregion

                #region key up
                cb_categorie.IsTextSearchEnabled = false;
                cb_categorie.IsEditable = true;
                cb_categorie.StaysOpenOnEdit = true;
                cb_categorie.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_categorie.Text = "";
                #endregion

                if (AppSettings.itemsTax_bool == false)
                {
                    grid_taxes.Visibility = Visibility.Collapsed;
                    tb_taxes.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grid_taxes.Visibility = Visibility.Visible;
                    tb_taxes.Visibility = Visibility.Visible;
                }
                await fillCategories();

                generateBarcode();

                await fillBarcodeList();

                SectionData.clearValidate(tb_code, p_errorCode);

                if (FillCombo.allUnitsList is null)
                    await FillCombo.RefreshAllUnits();
                units = FillCombo.allUnitsList.ToList();
                //units = await unitModel.Get();
                var uQuery = units.Where(u => u.name == "service").FirstOrDefault();
                unitServiceId = uQuery.unitId;

                await RefrishCategoriesCard();
                await RefrishItems();
                //Txb_searchitems_TextChanged(null, null);
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
        async Task fillBarcodeList()
        {
            barcodesList = await itemUnitModel.getAllBarcodes();
        }
        private void generateBarcode(string barcodeString = "")
        {
            if (barcodeString == "")
            {
                barcodeString = generateRandomBarcode();
                if (barcodesList != null)
                {
                    ItemUnit unit1 = barcodesList.ToList().Find(c => c.barcode == barcodeString);
                    if (unit1 != null)
                        barcodeString = generateRandomBarcode();
                }
                tb_barcode.Text = barcodeString;
            }
            tb_barcode.Text = barcodeString;
            // create encoding object
            Zen.Barcode.Code128BarcodeDraw barcode = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;

            System.Drawing.Bitmap serial_bitmap = (System.Drawing.Bitmap)barcode.Draw(barcodeString, 60);
            System.Drawing.ImageConverter ic = new System.Drawing.ImageConverter();
            //generate bitmap
            img_barcode.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(serial_bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //clear
                btn_prices.IsEnabled = false;

                tb_code.Clear();
                tb_name.Clear();
                tb_details.Clear();
                cb_categorie.SelectedIndex = -1;
                openFileDialog.FileName = "";
                selectedItem = 0;
                tb_taxes.Clear();
                item = new Item();
                tb_price.Clear();
                tb_cost.Clear();
                // set random barcode on image
                generateBarcode();

                SectionData.clearValidate(tb_code, p_errorCode);
                SectionData.clearValidate(tb_name, p_errorName);
                SectionData.clearComboBoxValidate(cb_categorie, p_errorCategorie);
                SectionData.clearValidate(tb_taxes, p_errorTaxes);
                SectionData.clearValidate(tb_price, p_errorPrice);
                SectionData.clearValidate(tb_cost, p_errorPrice);

                clearImg();

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
        private void clearImg()
        {
            //clear img
            Uri resourceUri = new Uri("pic/no-image-icon-125x125.png", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            brush.ImageSource = temp;
            img_item.Background = brush;
        }
        static public string generateRandomBarcode()
        {
            var now = DateTime.Now;

            var days = (int)(now - new DateTime(2000, 1, 1)).TotalDays;
            var seconds = (int)(now - DateTime.Today).TotalSeconds;

            var counter = _InternalCounter++ % 100;

            return days.ToString("00000") + seconds.ToString("00000") + counter.ToString("00");
        }
        async Task fillCategories()
        {
            if (FillCombo.categoriesList is null)
                await FillCombo.RefreshCategories();
            //categories = await categoryModel.GetAllCategories(MainWindow.userID.Value);
            //cb_categorie.ItemsSource = categories.ToList();
            cb_categorie.ItemsSource = FillCombo.categoriesList;
            cb_categorie.SelectedValuePath = "categoryId";
            cb_categorie.DisplayMemberPath = "name";
        }
        private void translate()
        {
            txt_service.Text = MainWindow.resourcemanager.GetString("trService");
            txt_activeSearch.Text = MainWindow.resourcemanager.GetString("trActive");
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trBaseInformation");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txb_searchitems, MainWindow.resourcemanager.GetString("trSearchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_details, MainWindow.resourcemanager.GetString("trDetailsHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_name, MainWindow.resourcemanager.GetString("trNameHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_code, MainWindow.resourcemanager.GetString("trCode"));

            txt_secondaryInformation.Text = MainWindow.resourcemanager.GetString("trSecondaryInformation");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_categorie, MainWindow.resourcemanager.GetString("trSelectCategorieHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_taxes, MainWindow.resourcemanager.GetString("trTaxHint"));

            txt_barcode.Text = MainWindow.resourcemanager.GetString("trBarCode");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_cost, MainWindow.resourcemanager.GetString("trCost"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_price, MainWindow.resourcemanager.GetString("trPrice"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_barcode, MainWindow.resourcemanager.GetString("trBarcodeHint"));

            //btn_items.Content = MainWindow.resourcemanager.GetString("trItems");
            btn_add.Content = MainWindow.resourcemanager.GetString("trAdd");
            btn_update.Content = MainWindow.resourcemanager.GetString("trUpdate");
            btn_delete.Content = MainWindow.resourcemanager.GetString("trDelete");

            txt_pricesButton.Text = MainWindow.resourcemanager.GetString("pricing");


            dg_items.Columns[0].Header = MainWindow.resourcemanager.GetString("trCode");
            dg_items.Columns[1].Header = MainWindow.resourcemanager.GetString("trName");
            dg_items.Columns[2].Header = MainWindow.resourcemanager.GetString("trDetails");
            dg_items.Columns[3].Header = MainWindow.resourcemanager.GetString("trCategorie");

            tt_clear.Content = MainWindow.resourcemanager.GetString("trClear");
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
            tt_search.Content = MainWindow.resourcemanager.GetString("trSearch");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_grid.Content = MainWindow.resourcemanager.GetString("trViewGrid");
            tt_items.Content = MainWindow.resourcemanager.GetString("trViewItems");
        }
        private void tb_barcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                string barCode = tb_barcode.Text;
                generateBarcode(barCode);
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
        private void tb_upperLimit_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                e.Handled = e.Key == Key.Space;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
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
        private void tb_barcode_Generate(object sender, KeyEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (e.Key.ToString() == "Return")
                {
                    string barCode = tb_barcode.Text;
                    generateBarcode(barCode);
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
            categories = FillCombo.categoriesList.ToList();
            //categories = await categoryModel.GetAllCategories(MainWindow.userID.Value);
            return categories;
        }

        async Task RefrishCategoriesCard()
        {
            if (categories is null)
                await RefrishCategories();
            categoriesQuery = categories.Where(x => x.isActive == tglCategoryState && x.parentId == categoryParentId);
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
            items = await item.GetAllSrItems();
            //if (category.categoryId != 0 && !allCategory)
            //{
            //    //items = await itemModel.GetSrItemsInCategoryAndSub(category.categoryId);
            //    var categoriesId = await itemModel.getCategoryAndSubs(category.categoryId);
            //    items = items.Where(x => categoriesId.Contains((int)x.categoryId)).ToList();
            //}
            // else
            //items = await item.GetAllSrItems();

            Txb_searchitems_TextChanged(null, null);
            return items;
        }

        void RefrishItemsDatagrid(IEnumerable<Item> _items)
        {
            dg_items.ItemsSource = _items;
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
        int itemUnitId = 0;
        private async void dg_items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { //selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (dg_items.SelectedIndex != -1)
                {
                    item = dg_items.SelectedItem as Item;
                    this.DataContext = item;
                }
                if (item != null)
                {
                    //btn_items.IsEnabled = true;
                    btn_prices.IsEnabled = true;
                    cb_categorie.SelectedValue = item.categoryId;
                    List<ItemUnit> itemUnits = new List<ItemUnit>();
                    //itemUnits = await itemUnitModel.Getall();
                    //var uQuery = itemUnits.Where(iu => iu.itemId == item.itemId && iu.unitId == unitServiceId).FirstOrDefault();
                    itemUnits = await itemUnitModel.GetAllItemUnits(item.itemId);
                    var uQuery = itemUnits.Where(iu => iu.unitId == unitServiceId).FirstOrDefault();

                    if (uQuery != null)
                    {
                        itemUnit = uQuery;
                        itemUnitId = uQuery.itemUnitId;
                        tb_taxes.Text = SectionData.PercentageDecTostring(item.taxes);
                        //tb_price.Text = uQuery.price.ToString();
                        tb_price.Text = SectionData.DecTostring(uQuery.price);
                        tb_cost.Text = SectionData.DecTostring(item.avgPurchasePrice);
                        tb_barcode.Text = uQuery.barcode;
                    }
                    else
                    {
                        tb_price.Text = "";
                        tb_cost.Text = "";
                        tb_barcode.Text = "";
                    }
                    getImg();
                }
                //tb_barcode.Focus();
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
        private async void getImg()
        {
            try
            {
                if (string.IsNullOrEmpty(item.image))
                {
                    SectionData.clearImg(img_item);
                }
                else
                {
                    //  byte[] imageBuffer = await itemModel.downloadImage(item.image); // read this as BLOB from your DB
                    byte[] imageBuffer = SectionData.readLocalImage(item.image, Global.TMPItemsFolder);

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
                    // string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                    string dir = Directory.GetCurrentDirectory();
                    string tmpPath = System.IO.Path.Combine(dir, Global.TMPItemsFolder);
                    tmpPath = System.IO.Path.Combine(tmpPath, item.image);
                    //  openFileDialog.FileName = tmpPath;
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
                tb_barcode.Focus();

                await generateTrack(categoryId);
                //await RefrishItems();
                Txb_searchitems_TextChanged(null, null);
            }
        }

        int selectedItem = 0;
        public async Task ChangeItemIdEvent(int itemId)
        {//change id
            if (!selectedItem.Equals(itemId))
            {
                try
                {
                    SectionData.StartAwait(grid_main);

                    selectedItem = itemId;
                    item = items.ToList().Find(c => c.itemId == itemId);
                    if (item != null)
                    {
                        this.DataContext = item;
                        tb_code.Text = item.code;
                        tb_name.Text = item.name;
                        tb_details.Text = item.details;
                        btn_prices.IsEnabled = true;

                        cb_categorie.SelectedValue = item.categoryId;
                        List<ItemUnit> itemUnits = new List<ItemUnit>();
                        //itemUnits = await itemUnitModel.Getall();
                        //var uQuery = itemUnits.Where(iu => iu.itemId == itemId && iu.unitId == unitServiceId).FirstOrDefault();
                        itemUnits = await itemUnitModel.GetAllItemUnits(itemId);
                        var uQuery = itemUnits.Where(iu => iu.unitId == unitServiceId).FirstOrDefault();
                        if (uQuery != null)
                        {
                            itemUnit = uQuery;
                            itemUnitId = uQuery.itemUnitId;
                            //tb_price.Text = uQuery.price.ToString();
                            tb_taxes.Text = SectionData.PercentageDecTostring(item.taxes);
                            tb_price.Text = SectionData.DecTostring(uQuery.price);
                            tb_cost.Text = SectionData.DecTostring(item.avgPurchasePrice);
                            tb_barcode.Text = uQuery.barcode;
                        }
                        else
                        {
                            tb_price.Text = "";
                            tb_cost.Text = "";
                            tb_barcode.Text = "";
                        }
                        getImg();
                    }
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
                path_itemsInCards.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                path_itemsInGrid.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4e4e4e"));

                tgl_itemIsActive.IsChecked = (tglItemState == 1) ? true : false;
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
                grid_itemsDatagrid.Visibility = Visibility.Visible;
                path_itemsInCards.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                path_itemsInCards.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4e4e4e"));

                tgl_itemIsActive.IsChecked = (tglItemState == 1) ? true : false;
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
        private async void Txb_searchitems_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //search
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    if (items is null)
                        await RefrishItems();
                    txtItemSearch = txb_searchitems.Text.ToLower();
                    pageIndex = 1;

                    #region search in category
                    if (category.categoryId != 0 && !allCategory)
                    {
                        //items = await itemModel.GetSrItemsInCategoryAndSub(category.categoryId);
                        var categoriesId = await itemModel.getCategoryAndSubs(category.categoryId);
                        itemsQuery = items.Where(x => categoriesId.Contains((int)x.categoryId)).ToList();
                    }
                    else
                        itemsQuery = items.ToList();
                    #endregion

                    #region
                    itemsQuery = itemsQuery.Where(x => (x.code.ToLower().Contains(txtItemSearch) ||
                    x.name.ToLower().Contains(txtItemSearch) ||
                    x.details.ToLower().Contains(txtItemSearch)
                    ) && x.isActive == tglItemState);
                    txt_count.Text = itemsQuery.Count().ToString();
                    if (btns is null)
                        btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                    RefrishItemsCard(pagination.refrishPagination(itemsQuery, pageIndex, btns));
                    #endregion
                    RefrishItemsDatagrid(itemsQuery);

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
                allCategory = false;

                Button b = (Button)sender;
                if (!string.IsNullOrEmpty(b.Tag.ToString()))
                {
                    await generateTrack(int.Parse(b.Tag.ToString()));
                    categoryParentId = int.Parse(b.Tag.ToString());
                    await RefrishCategoriesCard();
                    category.categoryId = int.Parse(b.Tag.ToString());
                }
                //await RefrishItems();
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
                categoriesQuery = categories.Where(x => x.isActive == tglCategoryState && x.parentId == 0);
                catigoriesAndItemsView.gridCatigories = grid_categoryCards;
                generateCoulmnCategoriesGrid(categoriesQuery.Count());
                catigoriesAndItemsView.FN_refrishCatalogCard(categoriesQuery.ToList(), -1);
                ///
                grid_categoryControlPath.Children.Clear();
                #region
                //items = await item.GetAllSrItems();
                #endregion
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

        public void ExcelService()
        {

            BuildReport();

            this.Dispatcher.Invoke(() =>
            {
                saveFileDialog.Filter = "EXCEL|*.xls;";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;
                    LocalReportExtensions.ExportToExcel(rep, filepath);
                }
            });
        }
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    //Thread t1 = new Thread(() =>
                    //{
                    ExcelService();

                    //});
                    //t1.Start();
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



        private void FN_ExportToExcel()
        {
            var QueryExcel = itemsQuery.AsEnumerable().Select(x => new
            {
                Code = x.code,
                Name = x.name,
                Details = x.details,
                Categoty = x.categoryName
            });
            var DTForExcel = QueryExcel.ToDataTable();
            DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trName");
            DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trCode");
            DTForExcel.Columns[2].Caption = MainWindow.resourcemanager.GetString("trDetails");
            DTForExcel.Columns[3].Caption = MainWindow.resourcemanager.GetString("trCategorie");

            ExportToExcel.Export(DTForExcel);
        }
        #endregion
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //refresh
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {

                    await RefrishItems();
                    // Txb_searchitems_TextChanged(null, null);
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
        private void Tb_validateEmptyTextChange(object sender, TextChangedEventArgs e)
        {
            try
            {

                string name = sender.GetType().Name;
                validateEmpty(name, sender);
                var txb = sender as TextBox;
                if ((sender as TextBox).Name == "tb_taxes" || (sender as TextBox).Name == "tb_price" || (sender as TextBox).Name == "tb_cost")
                    SectionData.InputJustNumber(ref txb);
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
        private void Tb_PreventSpaces(object sender, KeyEventArgs e)
        {
            try
            {
                e.Handled = e.Key == Key.Space;
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
                    if ((sender as TextBox).Name == "tb_code")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorCode, tt_errorCode, "trEmptyCodeToolTip");
                    else if ((sender as TextBox).Name == "tb_name")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorName, tt_errorName, "trEmptyNameToolTip");
                    //else if ((sender as TextBox).Name == "tb_taxes")
                    //    SectionData.validateEmptyTextBox((TextBox)sender, p_errorTaxes, tt_errorTaxes, "trEmptyTax");
                    else if ((sender as TextBox).Name == "tb_price")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorPrice, tt_errorPrice, "trEmptyPrice");
                    else if ((sender as TextBox).Name == "tb_cost")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorPrice, tt_errorPrice, "trEmptyError");
                }
                else if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_categorie")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorCategorie, tt_errorCategorie, "trErrorEmptyCategoryToolTip");
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_decimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                //decimal
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
        private void Tb_EnglishAndDigits_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                //only english and digits
                Regex regex = new Regex("^[a-zA-Z0-9. -_?]*$");
                if (!regex.IsMatch(e.Text))
                    e.Handled = true;

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {//add
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //add
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    validateEmptyEntries();

                    Boolean codeAvailable = await checkCodeAvailabiltiy(tb_code.Text);

                    decimal tax = 0;
                    if (tb_taxes.Text != "")
                        tax = decimal.Parse(tb_taxes.Text);

                    decimal price = 0;
                    if (tb_price.Text != "")
                        price = decimal.Parse(tb_price.Text);

                    decimal cost = 0;
                    if (tb_cost.Text != "")
                        cost = decimal.Parse(tb_cost.Text);

                    if ((!tb_code.Text.Equals("")) && (!tb_name.Text.Equals("")) && (!cb_categorie.Text.Equals("")) &&
                        //(!tb_taxes.Text.Equals("")) && 
                        (!tb_price.Text.Equals("")) &&
                        codeAvailable)
                    {
                        //item record
                        item = new Item();
                        item.code = tb_code.Text;
                        item.name = tb_name.Text;
                        item.details = tb_details.Text;
                        item.type = "sr";
                        item.image = "";
                        item.taxes = tax;
                        item.avgPurchasePrice = cost;
                        item.min = 0;
                        item.max = 0;
                        item.minUnitId = 1;
                        item.maxUnitId = 1;
                        item.isActive = 1;
                        item.categoryId = Convert.ToInt32(cb_categorie.SelectedValue);
                        item.createUserId = MainWindow.userID;

                        int res = (int)await itemModel.saveItem(item);
                        if (res > 0)
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                        else
                            Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                        int itemId = res;

                        if (openFileDialog.FileName != "")
                        {
                            await itemModel.uploadImage(openFileDialog.FileName, Md5Encription.MD5Hash("Inc-m" + itemId.ToString()), itemId);
                            openFileDialog.FileName = "";
                        }

                        //itemunit record
                        itemUnit = new ItemUnit();
                        itemUnit.itemId = itemId;
                        itemUnit.unitId = unitServiceId;
                        //itemUnit.avgPurchasePrice = cost;
                        itemUnit.price = price;
                        itemUnit.defaultPurchase = 1;
                        itemUnit.defaultSale = 1;
                        itemUnit.unitValue = 1;
                        itemUnit.purchasePrice = 0;
                        itemUnit.subUnitId = 1;
                        itemUnit.barcode = tb_barcode.Text;
                        itemUnit.createUserId = MainWindow.userID;

                        int s = (int)await itemUnitModel.saveItemUnit(itemUnit);

                        await RefrishItems();
                        //Txb_searchitems_TextChanged(null, null);
                        btn_clear_Click(null, null);
                        tb_code.Focus();
                        SectionData.clearValidate(tb_code, p_errorCode);
                    }



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
        private async void Btn_update_Click(object sender, RoutedEventArgs e)
        {//update
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //update
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update") || SectionData.isAdminPermision())
                {
                    if (item.itemId > 0)
                    {
                        validateEmptyEntries();

                        Boolean codeAvailable = await checkCodeAvailabiltiy(tb_code.Text);

                        decimal tax = 0;
                        if (tb_taxes.Text != "")
                            tax = decimal.Parse(tb_taxes.Text);

                        decimal price = 0;
                        if (tb_price.Text != "")
                            price = decimal.Parse(tb_price.Text);

                        decimal cost = 0;
                        if (tb_cost.Text != "")
                            cost = decimal.Parse(tb_cost.Text);

                        if ((!tb_code.Text.Equals("")) && (!tb_name.Text.Equals("")) && (!cb_categorie.Text.Equals("")) &&
                            //(!tb_taxes.Text.Equals("")) && 
                            (!tb_price.Text.Equals("")) &&
                            codeAvailable)
                        {
                            item.code = tb_code.Text;
                            item.name = tb_name.Text;
                            item.details = tb_details.Text;
                            item.type = "sr";
                            // item.image = "";
                            item.taxes = tax;
                            item.avgPurchasePrice = cost;
                            //item.isActive = 1;
                            item.categoryId = Convert.ToInt32(cb_categorie.SelectedValue);
                            item.createUserId = MainWindow.userID;

                            int res = (int)await itemModel.saveItem(item);

                            if (!res.Equals("0"))
                            {
                                int itemId = res;
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);

                                if (openFileDialog.FileName != "")
                                    await itemModel.uploadImage(openFileDialog.FileName, Md5Encription.MD5Hash("Inc-m" + itemId.ToString()), itemId);

                                List<ItemUnit> itemUnits = new List<ItemUnit>();
                                itemUnits = await itemUnitModel.Getall();
                                var uQuery = itemUnits.Where(iu => iu.itemId == itemId && iu.unitId == unitServiceId).FirstOrDefault();
                                int itemUnitId = 0;
                                if (uQuery != null)
                                {
                                    itemUnitId = uQuery.itemUnitId;
                                    itemUnit = uQuery;
                                }

                                //itemunit record
                                itemUnit.itemUnitId = itemUnitId;
                                itemUnit.itemId = itemId;
                                itemUnit.unitId = unitServiceId;
                                itemUnit.price = price;
                                itemUnit.barcode = tb_barcode.Text;
                                itemUnit.updateUserId = MainWindow.userID;

                                int s = (int)await itemUnitModel.saveItemUnit(itemUnit);

                                await RefrishItems();
                                //Txb_searchitems_TextChanged(null, null);
                            }
                            else
                                Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);


                        }

                        tb_code.Focus();
                        SectionData.clearValidate(tb_code, p_errorCode);
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
        private async void Btn_delete_Click(object sender, RoutedEventArgs e)
        {//delete
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //delete
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                    if (item.itemId != 0)
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

                                int b = (int)await itemModel.deleteItem(item.itemId, MainWindow.userID.Value, item.canDelete);

                                if (b > 0)
                                {
                                    item.itemId = 0;
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);
                                }
                                else if (b == -1)
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpgrade"), animation: ToasterAnimation.FadeIn);
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            }
                        }

                        await RefrishItems();
                        //Txb_searchitems_TextChanged(null, null);
                    }
                    //clear textBoxs
                    btn_clear_Click(null, null);
                    tb_code.Focus();
                    SectionData.clearValidate(tb_code, p_errorCode);

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
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

            await RefrishItems();
            //Txb_searchitems_TextChanged(null, null);
        }
        private async Task<Boolean> checkCodeAvailabiltiy(string code)
        {
            List<Item> items = await itemModel.GetAllItems();

            if (items != null)
                if (items.Any(i => i.code == code && i.itemId != item.itemId))
                {
                    SectionData.validateDuplicateCode(tb_code, p_errorCode, tt_errorCode, "trDuplicateCodeToolTip");
                    return false;
                }
                else
                {
                    SectionData.clearValidate(tb_code, p_errorCode);
                    return true;
                }
            else
            {
                SectionData.clearValidate(tb_code, p_errorCode);
                return true;
            }

        }
        private void validateEmptyEntries()
        {
            //chk empty code
            SectionData.validateEmptyTextBox(tb_code, p_errorCode, tt_errorCode, "trEmptyCodeToolTip");
            //chk empty name 
            SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
            //chk empty category
            SectionData.validateEmptyComboBox(cb_categorie, p_errorCategorie, tt_errorCategorie, "trErrorEmptyCategoryToolTip");
            //chk empty tax
            //SectionData.validateEmptyTextBox(tb_taxes, p_errorTaxes, tt_errorTaxes, "trEmptyTax");
            //chk empty price
            SectionData.validateEmptyTextBox(tb_price, p_errorPrice, tt_errorPrice, "trEmptyPrice");
            SectionData.validateEmptyTextBox(tb_cost, p_errorCost, tt_errorCost, "trEmptyError");
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
        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string searchval = "";
            string stateval = "";
            bool isArabic = ReportCls.checkLang();
           // string all = MainWindow.resourcemanagerreport.GetString("trAll");
            string addpath;
            if (isArabic)
            {
                addpath = @"\Reports\Catalog\Ar\ServiceReport.rdlc";
            }
            else
            {
                addpath = @"\Reports\Catalog\En\ServiceReport.rdlc";
            }
            //filter   
            stateval = tgl_itemIsActive.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trActive_")
              : MainWindow.resourcemanagerreport.GetString("trNotActive");
            paramarr.Add(new ReportParameter("stateval", stateval));
            paramarr.Add(new ReportParameter("trActiveState", MainWindow.resourcemanagerreport.GetString("trState")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            searchval = txb_searchitems.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            clsReports.serviceReport(itemsQuery, rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            rep.SetParameters(paramarr);
            rep.Refresh();
        }
        public void pdfService()
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
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {

                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    /////////////////////////////////////
                    Thread t1 = new Thread(() =>
                    {
                        pdfService();
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
        public void printService()
        {
            BuildReport();

            this.Dispatcher.Invoke(() =>
            {
                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
            });
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
                        printService();
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
        private void Btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    #region
                    Window.GetWindow(this).Opacity = 0.2;
                    win_lvcCatalog win = new win_lvcCatalog(itemsQuery, 3);
                    win.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                categoryParentId = 0;
                grid_categoryControlPath.Children.Clear();
                selectedItem = 0;
                btn_clear_Click(btn_clear, null);
                GC.Collect();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

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
                    tb_taxes.Text = SectionData.PercentageDecTostring(cat.taxes);
                    if (cat.fixedTax == 1)
                        tb_taxes.IsEnabled = false;
                    else
                        tb_taxes.IsEnabled = true;
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

        private void Cb_categorie_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = FillCombo.categoriesList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
