using netoaster;
using POS.Classes;
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
using static POS.Classes.FillCombo;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_serialNum.xaml
    /// </summary>
    public partial class wd_serialNum : Window
    {
        public wd_serialNum()
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
        BrushConverter bc = new BrushConverter();
        public Item item { get; set; }
        public ItemTransfer itemTransfer { get; set; }
        public int itemCount { get; set; }
        public string invType { get; set; }
       
        public List<Serial> itemsSerials { get; set; }
        public List<Serial> returnedSerials { get; set; }
        public int mainInvoiceId { get; set; }
        public bool valid { get; set; }
        public bool isOk { get; set; }
        public bool serialsSave { get; set; }
        public bool serialsSkip { get; set; }
        public bool propertiesSave { get; set; }
        public bool propertiesSkip { get; set; }
        public UserControls sourceUserControls;

        bool hasProp = false;
        bool hasSerial = false;

        #region properties params
        public List<StoreProperty> ItemProperties { get; set; } // item unit properties
        public List<StoreProperty> StoreProperties { get; set; }//saved with item transfer
        public List<StoreProperty> ReturnedProperties { get; set; }//saved with item transfer

       public List<StoreProperty> propertyList = new List<StoreProperty>();//keep selected properties

        List<StoreProperty> propValuesConcat = new List<StoreProperty>();//list to fill properties combo
        private static int _propertiesCount = 0;
        #endregion
       

        public List<Serial> serialList = new List<Serial>();
        public List<StoreProperty> propertiesList = new List<StoreProperty>();

        private static int _serialCount = 0;
        private  int _allSerialCount = 0;

        Serial serialModel = new Serial();
        ItemUnit itemUnit = new ItemUnit();
        ItemUnit propItemUnit = new ItemUnit();
        Invoice invoiceModel = new Invoice();
        List<Item> gridItems = new List<Item>();
        List<Item> propGridItems = new List<Item>();
        List<Item> packageItems = new List<Item>();
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            isOk = false;
            this.Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_serialNum);
                #region translate

                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_serialNum.FlowDirection = FlowDirection.LeftToRight;

                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_serialNum.FlowDirection = FlowDirection.RightToLeft;

                }
                translate();
                #endregion
                if(sourceUserControls == UserControls.payInvoice || sourceUserControls== UserControls.receiptOfPurchaseInvoice)
                {
                    grid_minUnit.Visibility = Visibility.Visible;
                    grid_minUnitProperties.Visibility = Visibility.Visible;
                }
                else
                {
                    grid_minUnit.Visibility = Visibility.Collapsed;
                    grid_minUnitProperties.Visibility = Visibility.Collapsed;


                }

                #region canSkipProperties
                if (AppSettings.canSkipProperties ||sourceUserControls == UserControls.payInvoice
                    || sourceUserControls == UserControls.receiptOfPurchaseInvoice || sourceUserControls == UserControls.itemsExport)
                    cd_btnSkipProperties.Width = new GridLength(1, GridUnitType.Star);
                else
                    cd_btnSkipProperties.Width = new GridLength(0, GridUnitType.Pixel);
                #endregion

                #region canSkipSerialsNum
                if (AppSettings.canSkipSerialsNum || sourceUserControls == UserControls.payInvoice 
                    || sourceUserControls == UserControls.receiptOfPurchaseInvoice || sourceUserControls == UserControls.itemsExport)
                    cd_btnSkipSerialsNum.Width = new GridLength(1, GridUnitType.Star);
                else
                    cd_btnSkipSerialsNum.Width = new GridLength(0, GridUnitType.Pixel);
                #endregion

                #region item info
                txt_itemCode.Text = item.code;
                txt_itemName.Text = item.name;
                txt_itemDetails.Text = item.details;
                if(itemTransfer is null)
                {
                    if (!string.IsNullOrWhiteSpace( item.warrantyName))
                        txt_warrantyName.Text = item.warrantyName;
                    if (!string.IsNullOrWhiteSpace(item.warrantyDescription))
                        txt_warrantyDescription.Text = item.warrantyDescription;

                    if (!string.IsNullOrWhiteSpace(item.offerName))
                        txt_offerName.Text = item.offerName;
                    if (item.discountType == "1")
                        txt_offerDescription.Text = item.discountValue + " " + AppSettings.Currency; 
                    else if (item.discountType == "2")
                        txt_offerDescription.Text = item.discountValue +" %";

                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(itemTransfer.warrantyName))
                        txt_warrantyName.Text = itemTransfer.warrantyName;
                    if (!string.IsNullOrWhiteSpace(itemTransfer.warrantyDescription))
                        txt_warrantyDescription.Text = itemTransfer.warrantyDescription;


                    if (!string.IsNullOrWhiteSpace(itemTransfer.offerName))
                        txt_offerName.Text = itemTransfer.offerName;
                    if (itemTransfer.offerType == 1)
                        txt_offerDescription.Text = itemTransfer.offerValue + " " + AppSettings.Currency;
                    else if (itemTransfer.offerType == 2)
                        txt_offerDescription.Text = itemTransfer.offerValue + " %";
                }
                #region
                if (item.type == "p" && item.packageItems.Count > 0)
                {
                    //grid_package.Visibility = Visibility.Visible;
                    brd_itemsPackage.Visibility = Visibility.Visible;
                    txt_notPackageType.Visibility = Visibility.Collapsed;
                    //this.Height = 375;
                    BuildExtraOrdersDesign(item.packageItems);
                }
                else
                {
                    //grid_package.Visibility = Visibility.Collapsed;
                    brd_itemsPackage.Visibility = Visibility.Collapsed;
                    txt_notPackageType.Visibility = Visibility.Visible;
                }
                #endregion
                #endregion
                #region hasSerial
                List<string> invTypeList = new List<string>() { "s","sd","sb","p","pd","po","pb","pbd","exd","exw","ex","isd","is","or","qt","po"};
                if ((item.type == "sn" ||(item.type.Equals("p")&& item.packageItems != null && item.packageItems.Where(x => x.type == "sn").FirstOrDefault() != null)) 
                && (invTypeList.Contains(invType) || (invType.Equals("sbd") && itemsSerials != null)))
                {
                    Btn_tab_Click(btn_serialInfo, null);

                    hasSerial = true;
                }
                else
                {
                    Btn_tab_Click(btn_itemInfo, null);
                    hasSerial = false;


                }
                #endregion

                #region has properties
                await checkUnitHasProp();
                
                #endregion
                fillSerialItemsGrid();
                inputEditable();

                hideTabs();
                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void translate()
        {
            txt_itemTitle.Text = MainWindow.resourcemanager.GetString("itemInfo");
            txt_serialTitle.Text = MainWindow.resourcemanager.GetString("trSerialNum");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_serialNum, MainWindow.resourcemanager.GetString("trSerialNumHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_minUnit, MainWindow.resourcemanager.GetString("trSelectUnitHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_minUnitProperties, MainWindow.resourcemanager.GetString("trSelectUnitHint"));
            tt_clear.Content = MainWindow.resourcemanager.GetString("trClear");
            tt_clear1.Content = MainWindow.resourcemanager.GetString("trClear");
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
            btn_save1.Content = MainWindow.resourcemanager.GetString("trSave");
            btn_skip.Content = MainWindow.resourcemanager.GetString("trSkip");
            btn_skip1.Content = MainWindow.resourcemanager.GetString("trSkip");
            btn_delete.Content = MainWindow.resourcemanager.GetString("trDelete");
            btn_deleteProperties.Content = MainWindow.resourcemanager.GetString("trDelete");
            txt_serials.Text = MainWindow.resourcemanager.GetString("trSerials");
            txt_properties.Text = MainWindow.resourcemanager.GetString("trProperties");
            txt_propertiess.Text = MainWindow.resourcemanager.GetString("trProperties");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_itemProps, MainWindow.resourcemanager.GetString("trProperties"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_itemPropsCount, MainWindow.resourcemanager.GetString("trCount"));
            

            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trBaseInformation");
            txt_packageItem.Text = MainWindow.resourcemanager.GetString("packageItems");

            txt_itemCodeTitle.Text = MainWindow.resourcemanager.GetString("trCode") + ":";
            txt_itemNameTitle.Text = MainWindow.resourcemanager.GetString("trName") + ":";
            txt_itemDetailsTitle.Text = MainWindow.resourcemanager.GetString("trDetails") + ":";
            txt_offer.Text = MainWindow.resourcemanager.GetString("trOffer") + ":";
            txt_offerNameTitle.Text = MainWindow.resourcemanager.GetString("trName") + ":";
            txt_offerDescriptionTitle.Text = MainWindow.resourcemanager.GetString("trDiscount") + ":";
            txt_warranty.Text = MainWindow.resourcemanager.GetString("trWarranty");
            txt_warrantyNameTitle.Text = MainWindow.resourcemanager.GetString("trPeriod") + ":";
            txt_warrantyDescriptionTitle.Text = MainWindow.resourcemanager.GetString("trDescription") + ":";
            txt_notPackageType.Text = MainWindow.resourcemanager.GetString("notPackageType") + " !";

            txt_from.Text = MainWindow.resourcemanager.GetString("trFrom");
            txt_from1.Text = MainWindow.resourcemanager.GetString("trFrom");

            col_itemName.Header = MainWindow.resourcemanager.GetString("trItem");
            col_itemName1.Header = MainWindow.resourcemanager.GetString("trItem");
            col_itemCount.Header = MainWindow.resourcemanager.GetString("trQTR");
            col_itemCount1.Header = MainWindow.resourcemanager.GetString("trQTR");
            col_serialNum.Header = MainWindow.resourcemanager.GetString("trSerialNum");


            col_properties.Header = MainWindow.resourcemanager.GetString("trProperties");
            col_propertiesCount.Header = MainWindow.resourcemanager.GetString("trCount");
        }
        void hideTabs()
        {
            if (hasSerial)
                col_serial.Width = new GridLength(1, GridUnitType.Star);
            else
            {
                col_serial.Width = new GridLength(0, GridUnitType.Pixel);
                serialsSkip = true;
                serialsSave = true;
            }

            if (hasProp)
                col_props.Width = new GridLength(1, GridUnitType.Star);
            else
            {
                col_props.Width = new GridLength(0, GridUnitType.Pixel);
                propertiesSkip = true;
                propertiesSave = true;
            }

            if(hasProp && !hasSerial)
            {
                btn_properties.BorderThickness = new Thickness(0, 1, 1, 1);
                MaterialDesignThemes.Wpf.ButtonAssist.SetCornerRadius(btn_properties, new CornerRadius(0, 7, 0, 0));
            }
            else if(!hasProp && hasSerial)
            {

            }
            else if(!hasProp && !hasSerial)
            {
                btn_itemInfo.Background = Application.Current.Resources["MainColor"] as SolidColorBrush;
                btn_itemInfo.BorderThickness = new Thickness(0);
                path_itemInfo.Fill = Application.Current.Resources["White"] as SolidColorBrush;
                txt_itemTitle.Foreground = Application.Current.Resources["White"] as SolidColorBrush;
                MaterialDesignThemes.Wpf.ButtonAssist.SetCornerRadius(btn_itemInfo, new CornerRadius(7, 7, 0, 0));
            }
        }
        private async Task checkUnitHasProp()
        {
            #region fill small item units if come from purchase
            if (sourceUserControls == UserControls.payInvoice || sourceUserControls == UserControls.receiptOfPurchaseInvoice)
            {
                await fillItemUnitCombo();
            }
            #endregion

            if (invType == "pd"|| invType =="isd")
            {
                //check unit and sub units if has properties
                
                foreach(var row in itemUnits)
                {
                    if (row.ItemProperties != null && row.ItemProperties.Count > 0)
                    {
                        hasProp = true;
                        break;
                    }
                }

            }
            else if(item.type.Equals("p") && item.packageItems != null)
            {
                foreach(var p in item.packageItems)
                {
                    if(p.ItemProperties != null && p.ItemProperties.Count() > 0)
                    {
                        hasProp = true;
                        break;
                    }
                }
            }
            else if((ItemProperties != null && ItemProperties.Count() > 0) || (StoreProperties != null && StoreProperties.Count() > 0))
            {
                hasProp = true;
               
            }
            if (hasProp)
                fillPropItemsGrid();
            hideTabs();

        }
        private void fillSerialItemsGrid()
        {
            gridItems = new List<Item>();
            if (itemsSerials == null)
                itemsSerials = new List<Serial>();
            if (StoreProperties == null)
                StoreProperties = new List<StoreProperty>();

            serialList = itemsSerials.ToList();
            if (item.type.Equals("p"))
            {
                packageItems = item.packageItems.Select(x => new Item()
                {itemCount = x.itemCount, name = x.name,type = x.type,itemUnitId=x.itemUnitId,itemId = x.itemId,unitName = x.unitName }).ToList();
                foreach (var it in packageItems)
                {
                    if (it.type.Equals("sn"))
                   {
                        it.itemCount *= itemCount;
                        _allSerialCount += (int)it.itemCount;
                        if (invType == "sd" || invType =="exw" || invType == "exd")
                        {
                            if (itemsSerials != null)
                            {
                               
                                var serialCount = itemsSerials.Where(x => x.itemUnitId == it.itemUnitId).Count();
                                if (serialCount.Equals(it.itemCount))
                                    it.valid = true;
                                else
                                    it.valid = false;
                            }
                        }
                        else if(invType == "sbd" || invType == "pbd")
                        {
                            if (returnedSerials != null)
                            {
                                var serialCount = returnedSerials.Where(x => x.itemUnitId == it.itemUnitId).Count();
                                if (serialCount.Equals(it.itemCount))
                                    it.valid = true;
                                else
                                    it.valid = false;
                            }
                            else
                            {
                                var serialCount = itemsSerials.Where(x => x.itemUnitId == it.itemUnitId).Count();
                                if (serialCount.Equals(it.itemCount))
                                    it.valid = true;
                                else
                                    it.valid = false;

                                returnedSerials = itemsSerials.ToList();

                            }
                        }
                        gridItems.Add(it);
                    }
                }
            }
            else
            {
                item.itemCount = itemCount;
                _allSerialCount = itemCount;
                if (invType == "sd" ||  invType == "pd" || invType == "exd" || invType == "exw" )
                {
                    if (itemsSerials != null && itemsSerials.Count().Equals(item.itemCount))
                        item.valid = true;
                    else
                        item.valid = false;
                }
                else
                {
                    if (returnedSerials != null)
                    {
                        if (returnedSerials != null && returnedSerials.Count().Equals(item.itemCount))
                            item.valid = true;
                        else
                            item.valid = false;
                    }
                    else
                    {
                        
                        if (itemsSerials != null && itemsSerials.Count().Equals(item.itemCount))
                            item.valid = true;
                        else
                            item.valid = false;

                        returnedSerials = itemsSerials.ToList();
                    }
                }
                gridItems.Add(item);

            }

            dg_items.ItemsSource = gridItems;
            if (dg_items.Items.Count > 0)
                dg_items.SelectedIndex = 0;

            //dg_items1.ItemsSource = dg_items.ItemsSource;
            //if (dg_items1.Items.Count > 0)
            //    dg_items1.SelectedIndex = 0;
        }
        private void fillPropItemsGrid()
        {
            propGridItems = new List<Item>();
            if (itemsSerials == null)
                itemsSerials = new List<Serial>();
            if (StoreProperties == null)
                StoreProperties = new List<StoreProperty>();

            if (item.type.Equals("p"))
            {
                packageItems = item.packageItems.Select(x => new Item()
                {itemCount = x.itemCount, name = x.name,type = x.type,itemUnitId=x.itemUnitId,itemId = x.itemId,unitName = x.unitName, ItemProperties = x.ItemProperties }).ToList();
                foreach (var it in packageItems)
                {
                    if (it.ItemProperties != null && it.ItemProperties.Count > 0)
                    {
                        it.itemCount *= itemCount;

                        propGridItems.Add(it);
                    }
                }
            }
            else
            {
                item.itemCount = itemCount;

                propGridItems.Add(item);
            }
            dg_items1.ItemsSource = null;
            dg_items1.ItemsSource = propGridItems;
 
            if (dg_items1.Items.Count > 0)
                dg_items1.SelectedIndex = 0;
        }
        private void fillSerialList()
        {
            dg_serials.ItemsSource = null;
            _serialCount = 0;
            if (serialList != null)
            {
                _serialCount = serialList.Count;
                dg_serials.ItemsSource = serialList;

            }

            refreshValidIcon();
        }

        private void fillPropertiesList()
        {
            dg_properties.ItemsSource = null;
            _propertiesCount = 0;
            if (propertyList != null)
            {
                switch(invType)
                {
                    case "sd":
                    case "s":
                    case "exd":
                    case "exw":
                    case "ex":
                    case "sb":
                    
                        dg_properties.ItemsSource = StoreProperties.Where(x => x.itemUnitId == it.itemUnitId).ToList();
                        break;

                    case "sbd":
                        dg_properties.ItemsSource = ReturnedProperties.Where(x => x.itemUnitId == it.itemUnitId).ToList();

                        break;
                    case "pd":
                    case "p":
                    case "pb":
                    case "isd":
                    case "is":
                    case "pbw":
                        dg_properties.ItemsSource = StoreProperties.Where(x => x.itemUnitId == propItemUnit.itemUnitId).ToList();

                        break;
                    case "pbd":
                        dg_properties.ItemsSource = ReturnedProperties.Where(x => x.itemUnitId == propItemUnit.itemUnitId).ToList();

                        break;
                }
                //dg_properties.ItemsSource = propertyList.Where(x => x.itemUnitId == propItemUnit.itemUnitId).ToList();

            }

            refreshPropertyValidIcon();
        }

        List<ItemUnit> itemUnits = new List<ItemUnit>();
        private async Task fillItemUnitCombo()
        {
            try
            {
                 itemUnits = await itemUnit.getSmallItemUnits(item.itemId,(int) item.itemUnitId);

                var currentUnit = new ItemUnit() { itemUnitId = (int)item.itemUnitId,ItemProperties=ItemProperties, mainUnit = item.unitName,unitValue=1};
                itemUnits.Insert(0, currentUnit);

                cb_minUnit.ItemsSource = itemUnits;
                cb_minUnit.DisplayMemberPath = "mainUnit";
                cb_minUnit.SelectedValuePath = "itemUnitId";
                cb_minUnit.SelectedValue = item.itemUnitId;

                cb_minUnitProperties.ItemsSource = itemUnits;
                cb_minUnitProperties.DisplayMemberPath = "mainUnit";
                cb_minUnitProperties.SelectedValuePath = "itemUnitId";
                cb_minUnitProperties.SelectedValue = item.itemUnitId;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private void inputEditable()
        {
            List<string> lockedInvType = new List<string>() { "s","sb","p","pb","pbw","ex","im","imd","is"};
            if(lockedInvType.Contains(invType))
            {
                btn_enter.IsEnabled = false;
                btn_clearSerial.IsEnabled = false;
                btn_save.IsEnabled = false;
                btn_skip.IsEnabled = false;
                tb_serialNum.IsEnabled = false;
                col_chk.Visibility = Visibility.Collapsed;
                col_activate.Visibility = Visibility.Collapsed;

                btn_enter1.IsEnabled = false;
                btn_clearProperties.IsEnabled = false;
                btn_save1.IsEnabled = false;
                btn_skip1.IsEnabled = false;
                tb_itemPropsCount.IsEnabled = false;
                col_chk1.Visibility = Visibility.Collapsed;
                col_activate1.Visibility = Visibility.Collapsed;

                cb_itemProps.IsEnabled = false;
                tb_itemPropsCount.IsEnabled = false;
            }
            else
            {

                btn_enter.IsEnabled = true;
                btn_clearSerial.IsEnabled = true;
                btn_save.IsEnabled = true;
                //btn_skip.IsEnabled = true;
                if (item.skipSerialsNum || sourceUserControls == UserControls.payInvoice 
                    || sourceUserControls == UserControls.receiptOfPurchaseInvoice || sourceUserControls == UserControls.itemsExport)
                    btn_skip.IsEnabled = true;
                else
                    btn_skip.IsEnabled = false;


                tb_serialNum.IsEnabled = true;
                col_chk.Visibility = Visibility.Visible;
                col_activate.Visibility = Visibility.Visible;

                btn_enter1.IsEnabled = true;
                btn_clearProperties.IsEnabled = true;
                btn_save1.IsEnabled = true;
                //btn_skip1.IsEnabled = true;
                if (item.skipProperties || sourceUserControls == UserControls.payInvoice 
                    || sourceUserControls == UserControls.receiptOfPurchaseInvoice || sourceUserControls == UserControls.itemsExport)
                    btn_skip1.IsEnabled = true;
                else
                    btn_skip1.IsEnabled = false;


                tb_itemPropsCount.IsEnabled = true;
                col_chk1.Visibility = Visibility.Visible;
                col_activate1.Visibility = Visibility.Visible;

                cb_itemProps.IsEnabled = true;
                tb_itemPropsCount.IsEnabled = true;

            }

            if (invType.Equals("pd") || invType.Equals("isd"))
            {
                btn_delete.IsEnabled = true;
                btn_deleteProperties.IsEnabled = true;
            }
            else
            {
                btn_delete.IsEnabled = false;
                btn_deleteProperties.IsEnabled = false;
            }
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {

                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_serialNum);

                var serials = (List<Serial>)dg_serials.ItemsSource;

                int validSr = 1;
                if (serials == null)
                    _serialCount = 0;
                else
                switch (invType)
                {
                    case "sd":
                        case "exd":
                        case "exw":
                            itemsSerials = itemsSerials.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                            itemsSerials.AddRange(serials.Where(x => x.isSold == true));
                            _serialCount = itemsSerials.Where(x => x.isSold == true).Count();

                            validSr = await serialModel.SerialCanAdded(itemsSerials);
                            if(validSr != 1)
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("NotValidSerial"), animation: ToasterAnimation.FadeIn);
                           break;

                        case "sbd":
                            returnedSerials = returnedSerials.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                            returnedSerials.AddRange(serials.Where(x => x.isSold == true));
                            _serialCount = returnedSerials.Where(x => x.isSold == true).Count();
                            break;

                        case "pd":
                        case "isd":
                            itemsSerials = itemsSerials.Where(x => x.itemUnitId != itemUnit.itemUnitId).ToList();
                            itemsSerials.AddRange(serials);
                            _serialCount = itemsSerials.Count();

                            validSr = await serialModel.SerialCanAdded(itemsSerials);
                            if (validSr != 1)
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("NotValidSerial"), animation: ToasterAnimation.FadeIn);
                            break;
                        case "pbd":
                            returnedSerials = returnedSerials.Where(x => x.itemUnitId != itemUnit.itemUnitId).ToList();
                            returnedSerials.AddRange(serials.Where(x => x.isSold == true));
                            _serialCount = returnedSerials.Where(x => x.isSold == true).Count();

                            break;
                    }
                

                if ((_serialCount <= _allSerialCount || invType == "pd")|| invType == "pbd") 
                {
                    if (validSr == 1 &&( _serialCount.Equals(_allSerialCount) || invType == "pd"  ||invType == "isd"  || invType == "pbd"))
                    {
                        serialsSave = true;
                        serialsSkip = false;
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("SerialsSaved"), animation: ToasterAnimation.FadeIn);
                        //_serialCount = 0;
                      
                        if((serialsSave || serialsSkip) && (propertiesSave || propertiesSkip))
                        {
                            valid = true;
                            isOk = true;
                            await Task.Delay(1000);
                            this.Close();

                        }
                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSerialNumbersEqualItemsNumber"), animation: ToasterAnimation.FadeIn);
                }
                else
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorSerialMoreItemCountToolTip"), animation: ToasterAnimation.FadeIn);

                setHintText();
                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

       
        private async void Btn_save1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_serialNum);


                var properties = (List<StoreProperty>)dg_properties.ItemsSource;

                if (properties == null)
                    _propertiesCount = 0;
                else
                switch (invType)
                {
                    case "sd":
                        case "exd":
                        case "exw":

                            StoreProperties = StoreProperties.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                            StoreProperties.AddRange(properties.Where(x => x.isSold == true));
                            _propertiesCount = (int)StoreProperties.Where(x => x.isSold == true).Select(x => x.count).Sum();                           
                           break;

                        case "sbd":
                            ReturnedProperties = ReturnedProperties.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                            ReturnedProperties.AddRange(properties.Where(x => x.isSold == true));
                            break;

                        case "pd":
                        case "isd":
                            StoreProperties = StoreProperties.Where(x => x.itemUnitId != propItemUnit.itemUnitId).ToList();
                            StoreProperties.AddRange(properties);
                            //_propertiesCount = (int)StoreProperties.Select(x => x.count).Sum();

                            break;

                        case "pbd":
                            ReturnedProperties = ReturnedProperties.Where(x => x.itemUnitId != propItemUnit.itemUnitId).ToList();
                            ReturnedProperties.AddRange(properties.Where(x => x.isSold == true));
                            break;
                    }

                if (invType != "pbd" || invType != "sbd")
                {
                    var propsIU = StoreProperties.Select(x =>
                                    new
                                    {
                                        x.itemUnitId,
                                        count = StoreProperties.Where(m => m.itemUnitId == x.itemUnitId).Select(m => m.count).Sum()
                                    }).Distinct().ToList();

                    propertiesSave = true;
                    if (propsIU.Count == 0)
                    {
                        propertiesSave = false;
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPropNumbersEqualItemsNumber"), animation: ToasterAnimation.FadeIn);
                    }
                    foreach (var p in propsIU)
                    {
                        if (invType == "pd" || invType == "isd")
                        {
                            var unit = itemUnits.Where(x => x.itemUnitId == p.itemUnitId).FirstOrDefault();
                            if (p.count < (it.itemCount * unit.unitValue))
                            {
                                propertiesSave = false;
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPropNumbersEqualItemsNumber"), animation: ToasterAnimation.FadeIn);
                                break;
                            }
                        }
                        else if(p.count < it.itemCount)
                        {
                            propertiesSave = false;
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPropNumbersEqualItemsNumber"), animation: ToasterAnimation.FadeIn);
                            break;
                        }
                    }
                    if (propertiesSave)
                    {
                        propertiesSkip = false;
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("PropertiesSaved"), animation: ToasterAnimation.FadeIn);
                    }
                }
                else
                {
                    var propsIU = ReturnedProperties.Select(x =>
                                    new
                                    {
                                        x.itemUnitId,
                                        count = ReturnedProperties.Where(m => m.itemUnitId == x.itemUnitId).Select(m => m.count).Sum()
                                    }).Distinct().ToList();

                    propertiesSave = true;
                    if (propsIU.Count == 0)
                    {
                        propertiesSave = false;
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPropNumbersEqualItemsNumber"), animation: ToasterAnimation.FadeIn);
                    }
                    foreach (var p in propsIU)
                    {
                        if (p.count < it.itemCount)
                        {
                            propertiesSave = false;
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPropNumbersEqualItemsNumber"), animation: ToasterAnimation.FadeIn);
                            break;
                        }
                    }
                    if (propertiesSave)
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("PropertiesSaved"), animation: ToasterAnimation.FadeIn);
                }

                if ((serialsSave || serialsSkip) && (propertiesSave || propertiesSkip))
                {
                    valid = true;
                    isOk = true;
                    await Task.Delay(1000);
                    this.Close();

                }
                setHintText();
                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }




        private void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var serials = new List<Serial>();

                if (dg_serials.ItemsSource != null)
                    serials = (List<Serial>)dg_serials.ItemsSource;

                itemsSerials = itemsSerials.Where(x => x.itemUnitId != itemUnit.itemUnitId).ToList();
                itemsSerials.AddRange(serials.Where(x => x.isSold == false).ToList());

                serials = itemsSerials.Where(x => x.itemUnitId == itemunit.itemUnitId).ToList();
                dg_serials.ItemsSource = null;
                dg_serials.ItemsSource = serials;

                setSerialCountText();
                refreshValidIcon();

            }
            catch (Exception ex)
            {

            }
        }

       private void setHintText()
        {
            string serialHint = "";
            if(hasSerial)
            {
                if(serialsSave) 
                     serialHint += MainWindow.resourcemanager.GetString("SerialsSaved");
                else if(serialsSkip)
                    serialHint += MainWindow.resourcemanager.GetString("SerialsSkipped");
            }
            if(hasProp)
            {
                if (serialHint != "")
                    serialHint += " - ";
                if (propertiesSave)
                    serialHint += MainWindow.resourcemanager.GetString("PropertiesSaved");
                else if(propertiesSkip)
                    serialHint += MainWindow.resourcemanager.GetString("PropertiesSkipped");

            }
            txt_serialsHint.Text = serialHint;
            txt_propertiesHint.Text = serialHint;
        }
        private void Btn_skip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_serialNum);

                valid = true;
                if (dg_serials.ItemsSource == null)
                    serialList = new List<Serial>();
                else
                {
                    var serials = (List<Serial>)dg_serials.ItemsSource;

                    switch (invType)
                    {
                        case "sd":
                            serials = serials.Where(x => x.isSold == true).ToList();
                            break;
                        case "sbd":
                            break;
                        case "pd":
                            break;
                    }

                    if (serials.Count > 0)
                    {
                        serialList = serials.ToList();

                        _serialCount = 0;
                    }
                }
                serialsSkip = true;
                serialsSave = false;
                if ((serialsSave || serialsSkip) && (propertiesSave || propertiesSkip))
                {
                    valid = true;
                    isOk = true;
                    this.Close();

                }
                setHintText();
                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
      
        private void Btn_skip1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_serialNum);

                valid = true;
                propertiesSkip = true;
                propertiesSave = false;
                if ((serialsSave || serialsSkip) && (propertiesSave || propertiesSkip))
                {
                    valid = true;
                    isOk = true;
                    this.Close();

                }
                setHintText();
                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void preview_TextBox(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text == "," )
                    e.Handled = true;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void space_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            SectionData.InputJustNumber(ref textBox);
            e.Handled = e.Key == Key.Space;
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
        private  void Tb_serialNum_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                     keyEnterProcess();
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        bool keyEnterProcess()
        {
            try
            {
                if (it != null)
                {
                    string s = tb_serialNum.Text;
                    if (!s.Equals(""))
                    {
                        var serials = new List<Serial>();

                        if (dg_serials.ItemsSource != null)
                        serials = (List<Serial>)dg_serials.ItemsSource;
                        
                        var found = serials.Where(x => x.serialNum == tb_serialNum.Text).FirstOrDefault();

                        if (invType == "sd" || invType == "exd" || invType == "exw")
                        {
                            _serialCount = serials.Where(x => x.isSold == true).Count();
                            if (_serialCount == it.itemCount)
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + it.itemCount, animation: ToasterAnimation.FadeIn);
                            }
                            else if (found == null)
                            {
                                serials.Add(new Serial() { serialNum = tb_serialNum.Text, itemUnitId = it.itemUnitId, isSold = true, isManual = true });
                                serials.OrderByDescending(x => x.isSold).ThenBy(x => x.serialNum).ToList();
                                dg_serials.ItemsSource = null;
                                dg_serials.ItemsSource = serials;

                                itemsSerials = itemsSerials.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                                itemsSerials.AddRange(serials);
                                refreshValidIcon();
                                setSerialCountText();
                                tb_serialNum.Clear();

                                return true;
         
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningSerialExists"), animation: ToasterAnimation.FadeIn);
                        }
                        else if(invType == "sbd")
                        {
                            var valid = itemsSerials.Where(x => x.serialNum == s).FirstOrDefault();
                            if (_serialCount == it.itemCount)
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + itemCount, animation: ToasterAnimation.FadeIn);
                            }
                            else if (found == null && valid != null)
                            {
                                returnedSerials = returnedSerials.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                                returnedSerials.AddRange(serials);

                                returnedSerials.Add(new Serial() { serialNum = tb_serialNum.Text, itemUnitId = it.itemUnitId });

                                dg_serials.ItemsSource = null;
                                dg_serials.ItemsSource = returnedSerials;

                                refreshValidIcon();
                                setSerialCountText();
                                tb_serialNum.Clear();

                                return true;

                            }
                            else if(valid == null)
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("NotValidSerial"), animation: ToasterAnimation.FadeIn);
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningSerialExists"), animation: ToasterAnimation.FadeIn);

                        }
                        else if(invType == "pd" || invType == "isd")
                        {
                            _serialCount = serials.Count();
                            int validCount = (int)it.itemCount * (int)itemUnit.unitValue;
                            if (_serialCount == validCount)
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + it.itemCount, animation: ToasterAnimation.FadeIn);
                            }
                            else if (found == null)
                            {
                                serials.Add(new Serial() { serialNum = tb_serialNum.Text, itemUnitId =itemUnit.itemUnitId, isManual = true });
                                dg_serials.ItemsSource = null;
                                dg_serials.ItemsSource = serials;

                                itemsSerials = itemsSerials.Where(x => x.itemUnitId != itemUnit.itemUnitId).ToList();
                                itemsSerials.AddRange(serials);
                                refreshValidIcon();

                                setSerialCountText();

                                tb_serialNum.Clear();

                                return true;
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningSerialExists"), animation: ToasterAnimation.FadeIn);
                        }
                        else if (invType == "pbd")
                        {
                            var valid = itemsSerials.Where(x => x.serialNum == s).FirstOrDefault();
                            if (_serialCount == it.itemCount)
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + itemCount, animation: ToasterAnimation.FadeIn);
                            }
                            else if (found == null && valid != null)
                            {
                                returnedSerials = returnedSerials.Where(x => x.itemUnitId != itemUnit.itemUnitId).ToList();
                                returnedSerials.AddRange(serials);

                                returnedSerials.Add(new Serial() { serialNum = tb_serialNum.Text, itemUnitId = itemUnit.itemUnitId });

                                dg_serials.ItemsSource = null;
                                dg_serials.ItemsSource = returnedSerials;

                                refreshValidIcon();
                                setSerialCountText();
                                tb_serialNum.Clear();

                                return true;

                            }
                            else if (valid == null)
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("NotValidSerial"), animation: ToasterAnimation.FadeIn);
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningSerialExists"), animation: ToasterAnimation.FadeIn);

                        }
                    }
                    tb_serialNum.Clear();
                    tb_serialNum.Focus();
                }
                else
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectItemFirst"), animation: ToasterAnimation.FadeIn);

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                
            }
            return false;
        }
       
        private void Chb_isSoldRow_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox chk = sender as CheckBox;
                if (chk.IsFocused)
                {
                    var serials = new List<Serial>();

                    if (dg_serials.ItemsSource != null)
                        serials = (List<Serial>)dg_serials.ItemsSource;

                    switch (invType)
                    {
                        case "sd":
                        case "exd":
                        case "exw":
                            _serialCount = serials.Where(x => x.isSold == true).Count();
                            if (_serialCount > it.itemCount)
                            {
                                chk.IsChecked = false;
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + it.itemCount, animation: ToasterAnimation.FadeIn);
                            }
                            else
                            {
                                itemsSerials = itemsSerials.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                                itemsSerials.AddRange(serials.Where(x => x.isSold == true).ToList());
                            }
                            break;
                        case "sbd":
                            _serialCount = serials.Where(x => x.isSold == true).Count();
                            if (_serialCount > it.itemCount)
                            {
                                chk.IsChecked = false;
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + it.itemCount, animation: ToasterAnimation.FadeIn);
                            }
                            else
                            {
                                returnedSerials = returnedSerials.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                                returnedSerials.AddRange(serials.Where(x => x.isSold == true).ToList());
                            }
                            break;
                        case "pd":
                            itemsSerials = itemsSerials.Where(x => x.itemUnitId != itemUnit.itemUnitId).ToList();
                            itemsSerials.AddRange(serials.Where(x => x.isSold == false).ToList());
                            break;
                        case "pbd":
                            _serialCount = serials.Where(x => x.isSold == true).Count();
                            if (_serialCount > it.itemCount * itemUnit.unitValue)
                            {
                                chk.IsChecked = false;
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + it.itemCount, animation: ToasterAnimation.FadeIn);
                            }
                            else
                            {
                                returnedSerials = returnedSerials.Where(x => x.itemUnitId != itemUnit.itemUnitId).ToList();
                                returnedSerials.AddRange(serials.Where(x => x.isSold == true).ToList());
                            }
                            break;
                    }
                    dg_serials.ItemsSource = null;
                    dg_serials.ItemsSource = serials;
                    setSerialCountText();
                    refreshValidIcon();
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);

            }
        }

        private void Chb_isSoldRow_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {

                CheckBox chk = sender as CheckBox;
                if (chk.IsFocused)
                {
                    var serials = new List<Serial>();

                    if (dg_serials.ItemsSource != null)
                        serials = (List<Serial>)dg_serials.ItemsSource;

                    switch (invType)
                    {
                        case "sd":
                        case "exd":
                        case "exw":
                            _serialCount = serials.Where(x => x.isSold == true).Count();
                            itemsSerials = itemsSerials.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                            itemsSerials.AddRange(serials.Where(x => x.isSold == true).ToList());

                            break;
                        case "sbd":
                            _serialCount = serials.Where(x => x.isSold == true).Count();
                            returnedSerials = returnedSerials.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                            returnedSerials.AddRange(serials.Where(x => x.isSold == true).ToList());

                            break;
                        case "pd":
                        case "isd":
                            if (_serialCount > it.itemCount * itemUnit.unitValue)
                            {
                                chk.IsChecked = true;
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + it.itemCount, animation: ToasterAnimation.FadeIn);
                            }
                            else
                            {
                                itemsSerials = itemsSerials.Where(x => x.itemUnitId != itemUnit.itemUnitId).ToList();
                                itemsSerials.AddRange(serials.Where(x => x.isSold == false).ToList());
                            }
                            break;
                        case "pbd":
                            _serialCount = serials.Where(x => x.isSold == true).Count();

                            returnedSerials = returnedSerials.Where(x => x.itemUnitId != itemUnit.itemUnitId).ToList();
                            returnedSerials.AddRange(serials.Where(x => x.isSold == true).ToList());

                            break;
                    }
                    dg_serials.ItemsSource = null;
                    dg_serials.ItemsSource = serials;
                    setSerialCountText();
                    refreshValidIcon();
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);

            }
        }

        private void refreshValidIcon()
        {
            switch (invType) {

                case "sd":
                case "exd":
                case "exw":
                    foreach (var t in gridItems)
                    {
                        var serialCount = itemsSerials.Where(x => x.itemUnitId == t.itemUnitId && x.isSold == true).Count();
                        if (serialCount.Equals(it.itemCount))
                            t.valid = true;
                        else
                            t.valid = false;
                    }
                    break;
                case "pd":
                case "isd":
                    foreach (var t in gridItems)
                    {
                        var serialCount = itemsSerials.Where(x => x.itemUnitId == t.itemUnitId && x.isSold == false).Count();
                        if (serialCount.Equals(it.itemCount))
                            t.valid = true;
                        else
                            t.valid = false;
                    }
                    break;

                case "sbd":
                case "pbd":
                    foreach (var t in gridItems)
                    {
                        var serialCount = returnedSerials.Where(x => x.itemUnitId == t.itemUnitId && x.isSold == true).Count();
                        if (serialCount.Equals(it.itemCount))
                            t.valid = true;
                        else
                            t.valid = false;
                    }

                    break;

                    
        };
            if (item.packageItems != null)
                foreach (var it in item.packageItems)
                {
                    if (it.type.Equals("sn"))
                    {

                        if (itemsSerials != null && returnedSerials == null)
                        {
                            var serialCount = itemsSerials.Where(x => x.itemUnitId == it.itemUnitId).Count();
                            if (serialCount.Equals(it.itemCount))
                                it.valid = true;
                            else
                                it.valid = false;
                        }
                        else if (returnedSerials != null)
                        {
                            var serialCount = returnedSerials.Where(x => x.itemUnitId == it.itemUnitId).Count();
                            if (serialCount.Equals(it.itemCount))
                                it.valid = true;
                            else
                                it.valid = false;
                        }

                    }
                }
            dg_items.ItemsSource = gridItems;
            dg_items.Items.Refresh();
        }
        private void Btn_clearSerial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _serialCount = 0;
                dg_serials.ItemsSource = null;
                serialList = new List<Serial>();
                if (invType.Equals("sbd") || invType.Equals("pbd"))
                {
                    returnedSerials = new List<Serial>();
                }
                else
                {
                    itemsSerials = new List<Serial>();
                }


                refreshValidIcon();

                setSerialCountText();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void setSerialCountText()
        {
            switch (invType) {
                case "sd":
                case "s":
                case "exd":
                case "exw":
                case "ex":
                
                    #region serials count inf
                    txt_sum2.Text = (it.itemCount).ToString();
                    txt_sum.Text = itemsSerials.Where(x => x.isSold == true).Count().ToString();
                    #endregion
                    break;
                    case "sbd":
                    case "sb":
                    #region serials count inf
                    txt_sum2.Text = (it.itemCount).ToString();
                    txt_sum.Text = returnedSerials.Where(x => x.isSold == true).Count().ToString();
                    #endregion
                    break;

                case "pd":
                case "p":
                case "isd":
                case "is":
                    txt_sum2.Text = (it.itemCount * itemUnit.unitValue).ToString();
                    txt_sum.Text = itemsSerials.Where(x => x.itemUnitId == itemUnit.itemUnitId && x.isSold == false).Count().ToString();
                    break;
                case "pbd":
                case "pb":
               
                    txt_sum2.Text = (it.itemCount * itemUnit.unitValue).ToString();
                    txt_sum.Text = returnedSerials.Where(x => x.itemUnitId == itemUnit.itemUnitId && x.isSold == true).Count().ToString();
                    break;
            };
        }
        private async void Btn_enter_Click(object sender, RoutedEventArgs e)
        {         
            try
            {
              keyEnterProcess();
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
      

   
        Item it;
        private async void Dg_items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dg_items.SelectedIndex != -1)
                {
                    if (sender != null)
                        SectionData.StartAwait(grid_serialNum);

                    it = dg_items.SelectedItem as Item;

                    //#region fill small item units if come from purchase
                    //if (sourceUserControls == UserControls.payInvoice || sourceUserControls == UserControls.receiptOfPurchaseInvoice)
                    //{
                    //    await fillItemUnitCombo();
                    //}
                    //#endregion
                        if (itemsSerials != null && returnedSerials == null)
                        serialList = itemsSerials.Where(x => x.itemUnitId == it.itemUnitId).ToList();
                    else if(returnedSerials != null)
                        serialList = returnedSerials.Where(x => x.itemUnitId == it.itemUnitId).ToList();

                    List<Serial> availableSerials = new List<Serial>(); ;
                    switch (invType)
                    {
                        case "sd":
                        case "exw":
                        case "exd":
                            availableSerials =await serialModel.GetSerialsByIsSold(false,(int)it.itemUnitId, MainWindow.branchID.Value);

                            foreach (var sr in serialList)
                            {
                                sr.isSold = true;
                            }
                            if(availableSerials.ToList().Count > 0)
                            foreach (var s in itemsSerials)
                            {
                                if(s.itemUnitId == it.itemUnitId)
                                    availableSerials.Remove(availableSerials.Where(x => x.serialNum == s.serialNum).FirstOrDefault());
                            }
                            serialList.AddRange(availableSerials);
                            break;                       

                        case "sbd":
                            foreach (var sr in serialList)
                                sr.isSold = true;

                            var mainInvSerials = await serialModel.GetMainInvoiceSerials(mainInvoiceId, (int)it.itemUnitId);
                            foreach (var sr in returnedSerials)
                                mainInvSerials.Remove(mainInvSerials.Where(x => x.serialNum == sr.serialNum).FirstOrDefault());
                            break;
                        case "pbd":

                            foreach (var sr in serialList)
                                sr.isSold = true;

                           mainInvSerials = await serialModel.GetMainInvoiceSerials(mainInvoiceId, (int)itemUnit.itemUnitId);
                            foreach (var sr in returnedSerials)
                                mainInvSerials.Remove(mainInvSerials.Where(x => x.serialNum == sr.serialNum).FirstOrDefault());

                            serialList.AddRange(mainInvSerials);
                            break;
                    };

                    setSerialCountText();
                    if (sender != null)
                        SectionData.EndAwait(grid_serialNum);
                }
                fillSerialList();
            }
            catch (Exception ex){

                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }

        }

         private async void Dg_items1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dg_items1.SelectedIndex != -1)
                {
                    if (sender != null)
                        SectionData.StartAwait(grid_serialNum);

                    it = dg_items1.SelectedItem as Item;

                    if (invType == "pbd" || (sourceUserControls != UserControls.payInvoice && sourceUserControls != UserControls.receiptOfPurchaseInvoice))
                    {
                        fillPropertiesCombo();
                    }
                   
                  if (StoreProperties != null && ReturnedProperties == null)
                        propertyList = StoreProperties.Where(x => x.itemUnitId == it.itemUnitId).ToList();
                    else if(ReturnedProperties != null)
                        propertyList = ReturnedProperties.Where(x => x.itemUnitId == it.itemUnitId).ToList();

                    List<Serial> availableSerials = new List<Serial>(); ;
                    switch (invType)
                    {
                        case "pbd":
                        case "sbd":
                            propertyList = ReturnedProperties.Where(x => x.itemUnitId == it.itemUnitId).ToList();

                            break;
                        default:
                            propertyList = StoreProperties.Where(x => x.itemUnitId == it.itemUnitId).ToList();
                            break;
                    };

                    setPropertiesCountText();
                    if (sender != null)
                        SectionData.EndAwait(grid_serialNum);
                }
                fillPropertiesList();
            }
            catch (Exception ex){

                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }

        }

        #region properties methods

        #region properties combo
        private async void fillPropertiesCombo()
        {
            if (ItemProperties == null)
            {
                propertiesSkip = true;
                propertiesSave = true;
                ItemProperties = new List<StoreProperty>();
            }
            Dictionary<int, List<StoreProperty>> tags = new Dictionary<int, List<StoreProperty>>();

            switch (invType)
            {
                case "pd":
                case "isd":
                    //var properties = ItemProperties.Select(x => new { propertyId = x.propertyId, name = x.propName, index = x.propertyIndex }).Distinct().OrderBy(x => x.index).ToList();
                    if (propItemUnit.ItemProperties != null && propItemUnit.ItemProperties.Count > 0)
                    {
                        var properties = propItemUnit.ItemProperties.Select(x => new { propertyId = x.propertyId, name = x.propName, index = x.propertyIndex }).ToList().Distinct().OrderBy(x => x.index).ToList();

                        int i = 1;
                        foreach (var pr in properties)
                        {
                            var propValues = propItemUnit.ItemProperties.Where(x => x.propertyId == pr.propertyId).ToList();
                            tags.Add(i, propValues);

                            i++;
                        }
                        propValuesConcat = GetCombos(tags);
                    }
                    else
                        propValuesConcat = new List<StoreProperty>();
                    break;

                case "sd":
                case "exw":
                case "exd":
                    propValuesConcat = await invoiceModel.GetAvailableProperties((int)it.itemUnitId,MainWindow.branchID.Value); 
                    break;

                case "pbd":
                    propValuesConcat = StoreProperties.Where(x => x.itemUnitId == propItemUnit.itemUnitId).ToList();
                    break;
                case "sbd":
                    propValuesConcat = StoreProperties.ToList();
                    break;
            };
            cb_itemProps.ItemsSource = propValuesConcat.ToList();
            cb_itemProps.DisplayMemberPath = "propValue";
            cb_itemProps.SelectedValuePath = "notes";
        }
        private void Cb_minUnitProperties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (propItemUnit.itemUnitId != 0)
                {
                    var properties = (List<StoreProperty>)dg_properties.ItemsSource;
                    StoreProperties = StoreProperties.Where(x => x.itemUnitId != propItemUnit.itemUnitId).ToList();
                    if (properties != null) 
                        StoreProperties.AddRange(properties);

                }
                int itemUnitId = (int)cb_minUnitProperties.SelectedValue;
                propItemUnit = (ItemUnit)cb_minUnitProperties.SelectedItem;
                fillPropertiesCombo();

                //if (StoreProperties != null && ReturnedProperties == null)
                //    propertiesList = StoreProperties.Where(x => x.itemUnitId == itemUnitId).ToList();
                //else if (ReturnedProperties != null)
                //    propertiesList = ReturnedProperties.Where(x => x.itemUnitId == itemUnitId).ToList();


                setPropertiesCountText();

                fillPropertiesList();
            }
            catch { }
        }
        List<StoreProperty> GetCombos(IEnumerable<KeyValuePair<int, List<StoreProperty>>> remainingTags)
        {
            if (remainingTags.Count() == 1)
            {
                var current = remainingTags.First();
                foreach (var tagPart in current.Value)
                {
                    tagPart.notes = tagPart.propertyItemId.ToString();
                }
               //return remainingTags.First().Value;
               return current.Value;
            }
            else
            {
                var current = remainingTags.First();
                List<StoreProperty> outputs = new List<StoreProperty>();
                List<StoreProperty> combos = GetCombos(remainingTags.Where(tag => tag.Key != current.Key));

                foreach (var tagPart in current.Value)
                {
                    foreach (var combo in combos)
                    {
                        StoreProperty stp = new StoreProperty();
                        stp.propValue = tagPart.propName+": " +tagPart.propValue +", " +combo.propName+": "+ combo.propValue;
                        stp.notes = tagPart.propertyItemId.ToString() +"," + combo.propertyItemId.ToString();
                        outputs.Add(stp);
                    }
                }

                return outputs;
            }       
        }
        private void Cb_itemsProps_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var tb = cb_itemProps.Template.FindName("PART_EditableTextBox", cb_itemProps) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                cb_itemProps.ItemsSource = propValuesConcat.Where(p => p.propValue.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        StoreProperty sp;
        private void Cb_itemProps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                sp = null;
                if (cb_itemProps.SelectedIndex != -1 && !invType.Equals("pd") && !invType.Equals("isd"))
                {
                    sp = (StoreProperty)cb_itemProps.SelectedItem;
                    tb_itemPropsCount.Text = sp.count.ToString();
                }
            }
            catch { }
        }
        #endregion

        #region count changed
        private void Tb_itemPropsCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox tb = (TextBox)sender;
                if(tb.IsFocused)
                if(sp != null &&( invType.Equals("sd") || invType.Equals("exw")|| invType.Equals("exd") || invType.Equals("pbd")))
                {
                    if (int.Parse(tb_itemPropsCount.Text) > sp.count)
                    {
                        tb_itemPropsCount.Text = sp.count.ToString();
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("PropertiesNotAvailable") , animation: ToasterAnimation.FadeIn);                     
                    }
                }
            }
            catch
            {

            }
        }
        #endregion
        #region enter button

        private bool validateProperty()
        {
            bool valid = true;

            if (!SectionData.validateEmptyComboBox(cb_itemProps, p_errorItemProps, tt_errorItemProps, "trEmptyProperty"))
            {
                valid = false;
            }
            if (!SectionData.validateEmptyTextBox(tb_itemPropsCount, p_errorItemPropsCount, tt_errorItemPropsCount, "trEmptyCount") )
                valid = false;
            else if(!tb_itemPropsCount.Text.Equals("") && int.Parse(tb_itemPropsCount.Text).Equals(0))
            {
                SectionData.validateEmptyTextBox(tb_itemPropsCount, p_errorItemPropsCount, tt_errorItemPropsCount, "itMustBeGreaterThanZero");
                valid = false;
            }

            return valid;
        }
        private void Btn_enter1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(validateProperty())
                    PropertyEnterProcess();
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        bool PropertyEnterProcess()
        {
            try
            {
                if (it != null)
                {
                   string s = cb_itemProps.Text;
                    var prop = (StoreProperty) cb_itemProps.SelectedItem;

                    var properties = new List<StoreProperty>();

                    if (dg_properties.ItemsSource != null)
                        properties = (List<StoreProperty>)dg_properties.ItemsSource;


                    if (invType == "sd" || invType == "exd" || invType == "exw" )
                    {
                        var found = properties.Where(x => x.propValue == s && x.itemUnitId == it.itemUnitId).FirstOrDefault();

                        _propertiesCount = (int)properties.Where(x => x.isSold == true).Sum(x => x.count);
                        int toValidCount = (int)it.itemCount - _propertiesCount;
                        _propertiesCount += int.Parse(tb_itemPropsCount.Text);
                        int addedCount = 0;
                        if (toValidCount > int.Parse(tb_itemPropsCount.Text))
                            addedCount = int.Parse(tb_itemPropsCount.Text);
                        else
                            addedCount = toValidCount;

                        if (addedCount > (int)it.itemCount || addedCount==0)
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + it.itemCount, animation: ToasterAnimation.FadeIn);
                        }
                        else
                        {
                                if (found == null)
                                {
                                    properties.Add(new StoreProperty()
                                    {
                                        propValue = s,
                                        notes = cb_itemProps.SelectedValue.ToString(),
                                        itemUnitId = it.itemUnitId,
                                        //count = int.Parse(tb_itemPropsCount.Text),
                                        count = addedCount,
                                        isSold = true,
                                        isManual = true
                                    });
                                }
                                else
                                {
                                    if (found.isSold == false)
                                    {
                                        //found.count = int.Parse(tb_itemPropsCount.Text);
                                        found.count = addedCount;
                                        found.isSold = true;
                                    }
                                    else
                                    {
                                        //if((found.count + int.Parse(tb_itemPropsCount.Text) ) > sp.count)
                                        if((found.count + addedCount ) > sp.count)
                                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("PropertiesNotAvailable"), animation: ToasterAnimation.FadeIn);
                                        else
                                            found.count += addedCount;
                                            //found.count += int.Parse(tb_itemPropsCount.Text);
                                    }

                                }

                                dg_properties.ItemsSource = null;
                                dg_properties.ItemsSource = properties;

                                StoreProperties = StoreProperties.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                                StoreProperties.AddRange(properties);

                                refreshPropertyValidIcon();
                                setPropertiesCountText();

                            }
                        }
                        
                        else if (invType == "pd" || invType == "isd")
                        {
                            var found = properties.Where(x => x.propValue == s && x.itemUnitId == propItemUnit.itemUnitId).FirstOrDefault();

                            _propertiesCount = (int) properties.Sum(x => x.count);
                        _propertiesCount += int.Parse(tb_itemPropsCount.Text);


                            if (_propertiesCount > ((int)it.itemCount * propItemUnit.unitValue))
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + (it.itemCount * propItemUnit.unitValue), animation: ToasterAnimation.FadeIn);
                            }
                            else 
                            {
                                if (found == null)
                                {
                                    properties.Add(new StoreProperty()
                                    {
                                        propValue = s,
                                        notes = cb_itemProps.SelectedValue.ToString(),
                                        itemUnitId = propItemUnit.itemUnitId,
                                        count = int.Parse(tb_itemPropsCount.Text),
                                        isManual = true
                                    });
                                }
                                else
                                {
                                    found.count += int.Parse(tb_itemPropsCount.Text);
                                }
                                dg_properties.ItemsSource = null;
                                dg_properties.ItemsSource = properties;

                                StoreProperties = StoreProperties.Where(x => x.itemUnitId != propItemUnit.itemUnitId).ToList();
                                StoreProperties.AddRange(properties);

                                refreshPropertyValidIcon();
                                setPropertiesCountText();

                            }
                            
                        }
                        else if(invType == "pbd")
                        {
                            var found = properties.Where(x => x.propValue == s && x.itemUnitId == it.itemUnitId).FirstOrDefault();

                            if (ReturnedProperties == null)
                                ReturnedProperties = new List<StoreProperty>();
                            _propertiesCount = (int)properties.Where(x => x.isSold == true).Sum(x => x.count);
                        _propertiesCount += int.Parse(tb_itemPropsCount.Text);
                        
                            if (_propertiesCount > ((int)it.itemCount * propItemUnit.unitValue))
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + it.itemCount, animation: ToasterAnimation.FadeIn);
                            }
                            else
                            {
                                if (found == null)
                                {
                                    properties.Add(new StoreProperty()
                                    {
                                        propValue = s,
                                        notes = cb_itemProps.SelectedValue.ToString(),
                                        itemUnitId = propItemUnit.itemUnitId,
                                        count = int.Parse(tb_itemPropsCount.Text),
                                        isSold = true,
                                        isManual = true
                                    });
                                }
                                else
                                {
                                    if (found.isSold == false)
                                    {
                                        found.count = int.Parse(tb_itemPropsCount.Text);
                                        found.isSold = true;
                                    }
                                    else
                                        found.count += int.Parse(tb_itemPropsCount.Text);

                                }

                                dg_properties.ItemsSource = null;
                                dg_properties.ItemsSource = properties;

                                ReturnedProperties = ReturnedProperties.Where(x => x.itemUnitId != propItemUnit.itemUnitId).ToList();
                                ReturnedProperties.AddRange(properties);

                                refreshPropertyValidIcon();
                                setPropertiesCountText();

                            }
                        }
                    else if (invType == "sbd")
                    {
                        var found = properties.Where(x => x.propValue == s && x.itemUnitId == it.itemUnitId).FirstOrDefault();

                        if (ReturnedProperties == null)
                            ReturnedProperties = new List<StoreProperty>();
                        _propertiesCount = (int)properties.Where(x => x.isSold == true).Sum(x => x.count);
                        int toValidCount = (int)it.itemCount - _propertiesCount;

                        _propertiesCount += int.Parse(tb_itemPropsCount.Text);

                        int addedCount = 0;
                        if (toValidCount > int.Parse(tb_itemPropsCount.Text))
                            addedCount = int.Parse(tb_itemPropsCount.Text);
                        else
                            addedCount = toValidCount;

                        if (addedCount > (int)it.itemCount || addedCount == 0)
                            //if (_propertiesCount > (int)it.itemCount)
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + it.itemCount, animation: ToasterAnimation.FadeIn);
                        }
                        else
                        {
                            if (found == null)
                            {
                                properties.Add(new StoreProperty()
                                {
                                    propValue = s,
                                    notes = cb_itemProps.SelectedValue.ToString(),
                                    itemUnitId = it.itemUnitId,
                                    count = int.Parse(tb_itemPropsCount.Text),
                                    isSold = true,
                                    isManual = true
                                });
                            }
                            else
                            {
                                if (found.isSold == false)
                                {
                                    found.count = int.Parse(tb_itemPropsCount.Text);
                                    found.isSold = true;
                                }
                                else
                                    found.count += int.Parse(tb_itemPropsCount.Text);

                            }

                            dg_properties.ItemsSource = null;
                            dg_properties.ItemsSource = properties;

                            ReturnedProperties = ReturnedProperties.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                            ReturnedProperties.AddRange(properties);

                            refreshPropertyValidIcon();
                            setPropertiesCountText();

                        }

                    }

                    tb_itemPropsCount.Text ="1";
                    cb_itemProps.SelectedIndex = -1;
                }
                else
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectItemFirst"), animation: ToasterAnimation.FadeIn);

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);

            }
            return false;
        }
        #endregion

        #region refresh valid icon - properties count
        private void refreshPropertyValidIcon()
        {
            switch (invType)
            {

                case "sd":
                case "exd":
                case "exw":
                    foreach (var t in propGridItems)
                    {
                        var propertyCount = StoreProperties.Where(x => x.itemUnitId == t.itemUnitId && x.isSold == true).Select(x => x.count).Sum();
                        if (propertyCount == it.itemCount)
                            t.validProperty = true;
                        else
                            t.validProperty = false;
                    }
                    break;
                case "pd":
                case "isd":
                    foreach (var t in propGridItems)
                    {
                        var propertyCount = StoreProperties.Where(x => x.itemUnitId == propItemUnit.itemUnitId && x.isSold == false).Select(x => x.count).Sum();
                        if (propertyCount==it.itemCount)
                            t.validProperty = true;
                        else
                            t.validProperty = false;
                    }
                    break;

                case "sbd":
                    foreach (var t in propGridItems)
                    {
                        var propertyCount = ReturnedProperties.Where(x => x.itemUnitId == t.itemUnitId && x.isSold == true).Select(x => x.count).Sum();
                        if (propertyCount == it.itemCount)
                            t.validProperty = true;
                        else
                            t.validProperty = false;
                    }
                    break;
                case "pbd":
                    foreach (var t in propGridItems)
                    {
                        var propertyCount = ReturnedProperties.Where(x => x.itemUnitId == propItemUnit.itemUnitId && x.isSold == true).Select(x => x.count).Sum();
                        if (propertyCount == it.itemCount)
                            t.validProperty = true;
                        else
                            t.validProperty = false;
                    }

                    break;


            };
            //if (item.packageItems != null)
            //    foreach (var it in item.packageItems)
            //    {
            //        if (it.type.Equals("sn"))
            //        {

            //            if (StoreProperties != null && ReturnedProperties == null)
            //            {
            //                var propertiesCount = StoreProperties.Where(x => x.itemUnitId == it.itemUnitId).Select(x => x.count).Sum();
            //                if (propertiesCount == it.itemCount)
            //                    it.validProperty = true;
            //                else
            //                    it.validProperty = false;
            //            }
            //            else if (ReturnedProperties != null)
            //            {
            //                var propertiesCount = ReturnedProperties.Where(x => x.itemUnitId == it.itemUnitId).Select(x => x.count).Sum();
            //                if (propertiesCount == it.itemCount)
            //                    it.validProperty = true;
            //                else
            //                    it.validProperty = false;
            //            }

            //        }
            //    }
            dg_items1.ItemsSource = propGridItems;
            dg_items1.Items.Refresh();
        }

        private void setPropertiesCountText()
        {
            switch (invType)
            {
                case "sd":
                case "s":
                case "sbd":
                case "sb":
                case "exd":
                case "exw":
                case "ex":                
                    #region properties count count inf    
                    txt_totalPropCount.Text = it.itemCount.ToString();
                    txt_propCount.Text = StoreProperties.Where(x => x.isSold == true && x.itemUnitId == it.itemUnitId).Select(x => x.count).Sum().ToString();
                    #endregion
                    break;
                case "pbd":
                    txt_totalPropCount.Text = (it.itemCount * propItemUnit.unitValue).ToString();
                    txt_propCount.Text = ReturnedProperties.Where(x => x.isSold == true && x.itemUnitId == propItemUnit.itemUnitId).Select(x => x.count).Sum().ToString();
                    break;

                case "pb":
                    txt_totalPropCount.Text = (it.itemCount * propItemUnit.unitValue).ToString();
                    txt_propCount.Text = StoreProperties.Where(x => x.isSold == true && x.itemUnitId == propItemUnit.itemUnitId).Select(x => x.count).Sum().ToString();
                    break;
                case "pd":
                case "p":
                case "isd":
                case "is":
                    txt_totalPropCount.Text = (it.itemCount * propItemUnit.unitValue).ToString();

                    txt_propCount.Text = StoreProperties.Where(x => x.itemUnitId == propItemUnit.itemUnitId && x.isSold == false).Select(x => x.count).Sum().ToString();
                    break;
 
            };
        }
        #endregion

        #region delete
        private void deleteProperty_click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_serialNum);

                var row = (StoreProperty)dg_properties.SelectedItem;
                var properties = (List<StoreProperty>)dg_properties.ItemsSource;

                #region
                Window.GetWindow(this).Opacity = 0.2;
                wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;
                #endregion

                if (w.isOk)
                {
                    properties.Remove(row);

                    dg_properties.ItemsSource = null;
                    dg_properties.ItemsSource = properties;

                    switch(invType)
                    {
                        case "pbd":
                            ReturnedProperties.Remove(ReturnedProperties.Where(x => x.itemUnitId == propItemUnit.itemUnitId && x.propValue == row.propValue).FirstOrDefault());
                            break;
                        case "sbd":
                            ReturnedProperties.Remove(ReturnedProperties.Where(x => x.itemUnitId == it.itemUnitId && x.propValue == row.propValue).FirstOrDefault());

                            break;
                        case "sd":
                        case "exd":
                        case "exw":
                            StoreProperties.Remove(StoreProperties.Where(x => x.itemUnitId == it.itemUnitId && x.propValue == row.propValue).FirstOrDefault());
                            break;
                        default:
                            StoreProperties.Remove(StoreProperties.Where(x => x.itemUnitId == propItemUnit.itemUnitId && x.propValue == row.propValue).FirstOrDefault());

                            break;
                    };
                    
                    refreshPropertyValidIcon();
                    setPropertiesCountText();
                }


                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_deleteProperties_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var properties = new List<StoreProperty>();

                if (dg_properties.ItemsSource != null)
                    properties = (List<StoreProperty>)dg_properties.ItemsSource;

                StoreProperties = StoreProperties.Where(x => x.itemUnitId != propItemUnit.itemUnitId).ToList();
                StoreProperties.AddRange(properties.Where(x => x.isSold == false).ToList());

                properties = StoreProperties.Where(x => x.itemUnitId == propItemUnit.itemUnitId).ToList();
                dg_properties.ItemsSource = null;
                dg_properties.ItemsSource = properties;

                setPropertiesCountText();
                refreshPropertyValidIcon();

            }
            catch (Exception ex)
            {

            }
        }

        private void Btn_clearProperties_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _propertiesCount = 0;
                dg_properties.ItemsSource = null;

                if (invType.Equals("sbd") || invType.Equals("pbd"))
                {
                    ReturnedProperties = new List<StoreProperty>();
                }
                else
                {
                    StoreProperties = new List<StoreProperty>();
                }


                refreshPropertyValidIcon();

                setPropertiesCountText();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion


        #region property grid check
        private void Chb_isPropRow_Checked(object sender, RoutedEventArgs e)
        {
            try
            { 

            CheckBox chk = sender as CheckBox;
            if (chk.IsFocused)
            {
                var properties = new List<StoreProperty>();

                if (dg_properties.ItemsSource != null)
                    properties = (List<StoreProperty>)dg_properties.ItemsSource;

                switch (invType)
                {
                    case "sd":
                    case "exd":
                    case "exw":
                        _propertiesCount =(int) properties.Where(x => x.isSold == true).Select(x => x.count).Sum();
                        if (_propertiesCount > it.itemCount)
                        {
                            chk.IsChecked = false;
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + it.itemCount, animation: ToasterAnimation.FadeIn);
                        }
                        else
                        {
                            StoreProperties = StoreProperties.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                            StoreProperties.AddRange(properties.Where(x => x.isSold == true).ToList());
                        }
                        break;
                    case "sbd":
                        _propertiesCount = (int)StoreProperties.Where(x => x.isSold == true).Select(x => x.count).Sum();
                        if (_propertiesCount > it.itemCount)
                        {
                            chk.IsChecked = false;
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + it.itemCount, animation: ToasterAnimation.FadeIn);
                        }
                        else
                        {
                            ReturnedProperties = ReturnedProperties.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                            ReturnedProperties.AddRange(properties.Where(x => x.isSold == true).ToList());
                        }
                        break;
                    case "pd":
                        StoreProperties = StoreProperties.Where(x => x.itemUnitId != itemUnit.itemUnitId).ToList();
                        StoreProperties.AddRange(properties.Where(x => x.isSold == false).ToList());
                        break;
                    case "pbd":
                        _propertiesCount = (int)properties.Where(x => x.isSold == true).Select(x => x.count).Sum(); ;
                        if (_propertiesCount > it.itemCount * itemUnit.unitValue)
                        {
                            chk.IsChecked = false;
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + it.itemCount, animation: ToasterAnimation.FadeIn);
                        }
                        else
                        {
                            ReturnedProperties = ReturnedProperties.Where(x => x.itemUnitId != itemUnit.itemUnitId).ToList();
                            ReturnedProperties.AddRange(properties.Where(x => x.isSold == true).ToList());
                        }
                        break;
                }
                dg_properties.ItemsSource = null;
                dg_properties.ItemsSource = properties;
                setPropertiesCountText();
                refreshPropertyValidIcon();
            }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);

            }
        }

        private void Chb_isPropRow_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            { 
            CheckBox chk = sender as CheckBox;
            if (chk.IsFocused)
            {
                var properties = new List<StoreProperty>();

                if (dg_properties.ItemsSource != null)
                    properties = (List<StoreProperty>)dg_properties.ItemsSource;

                switch (invType)
                {
                    case "sd":
                    case "exd":
                    case "exw":
                        _propertiesCount = (int)properties.Where(x => x.isSold == true).Select(x => x.count).Sum(); ;
                        StoreProperties = StoreProperties.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                        StoreProperties.AddRange(properties.Where(x => x.isSold == true).ToList());

                        break;
                    case "sbd":
                        _propertiesCount = (int)properties.Where(x => x.isSold == true).Select(x => x.count).Sum();
                        ReturnedProperties = ReturnedProperties.Where(x => x.itemUnitId != it.itemUnitId).ToList();
                        ReturnedProperties.AddRange(properties.Where(x => x.isSold == true).ToList());

                        break;
                    case "pd":
                    case "isd":
                        _propertiesCount = (int)properties.Where(x => x.isSold == false).Select(x => x.count).Sum();

                        if (_serialCount > it.itemCount * itemUnit.unitValue)
                        {
                            chk.IsChecked = true;
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + it.itemCount, animation: ToasterAnimation.FadeIn);
                        }
                        else
                        {
                            StoreProperties = StoreProperties.Where(x => x.itemUnitId != itemUnit.itemUnitId).ToList();
                            StoreProperties.AddRange(properties.Where(x => x.isSold == false).ToList());
                        }
                        break;
                    case "pbd":
                        _propertiesCount = (int)properties.Where(x => x.isSold == true).Select(x => x.count).Sum();

                        ReturnedProperties = ReturnedProperties.Where(x => x.itemUnitId != itemUnit.itemUnitId).ToList();
                        ReturnedProperties.AddRange(properties.Where(x => x.isSold == true).ToList());

                        break;
                }
                dg_properties.ItemsSource = null;
                dg_properties.ItemsSource = properties;
                setPropertiesCountText();
                refreshPropertyValidIcon();
            }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
        #endregion
        private void Cb_minUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if(propItemUnit.itemUnitId != 0)
                {
                    var serials = (List<Serial>)dg_serials.ItemsSource;
                    itemsSerials = itemsSerials.Where(x => x.itemUnitId != propItemUnit.itemUnitId).ToList();
                    itemsSerials.AddRange(serials);             
                }
                int itemUnitId = (int)cb_minUnit.SelectedValue;
                itemUnit = (ItemUnit)cb_minUnit.SelectedItem;
                if (itemsSerials != null && returnedSerials == null)
                    serialList = itemsSerials.Where(x => x.itemUnitId == itemUnitId).ToList();
                else if (returnedSerials != null)
                    serialList = returnedSerials.Where(x => x.itemUnitId == itemUnitId).ToList();


                setSerialCountText();

                fillSerialList();
            }
            catch { }
        }
        void switchTab(bool status, Button button)
        {

            if (status)
            {
                // open
                button.BorderBrush = Application.Current.Resources["MainColorlightGrey"] as SolidColorBrush;
                button.Background = Application.Current.Resources["White"] as SolidColorBrush;
                Path path = FindControls.FindVisualChildren<Path>(button).FirstOrDefault();
                path.Fill = Application.Current.Resources["Grey"] as SolidColorBrush;
                TextBlock textBlock = FindControls.FindVisualChildren<TextBlock>(button).FirstOrDefault();
                textBlock.Foreground = Application.Current.Resources["Grey"] as SolidColorBrush;

            }
            else
            {
                // close
                button.BorderBrush = Application.Current.Resources["MainColorlightGrey"] as SolidColorBrush;
                button.Background = Application.Current.Resources["LightGrey"] as SolidColorBrush;
                Path path = FindControls.FindVisualChildren<Path>(button).FirstOrDefault();
                path.Fill = Application.Current.Resources["MainColorlightGrey"] as SolidColorBrush;
                TextBlock textBlock = FindControls.FindVisualChildren<TextBlock>(button).FirstOrDefault();
                textBlock.Foreground = Application.Current.Resources["MainColorlightGrey"] as SolidColorBrush;

            }


        }
        /*
        private void Btn_itemInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switchTab(true, btn_itemInfo);
                brd_ItemInfo.Visibility = Visibility.Visible;

                switchTab(false, btn_serialInfo);
                brd_serialInfo.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */
        private void Btn_tab_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                switch (button.Name)
                {
                    case "btn_itemInfo":
                        {

                            if (hasProp || hasSerial)
                            {

                            switchTab(true, btn_itemInfo);
                            brd_ItemInfo.Visibility = Visibility.Visible;


                            switchTab(false, btn_properties);
                            brd_properties.Visibility = Visibility.Collapsed;

                            switchTab(false, btn_serialInfo);
                            brd_serialInfo.Visibility = Visibility.Collapsed;
                            }


                            break;
                        }
                    case "btn_properties":
                        {
                            switchTab(false, btn_itemInfo);
                            brd_ItemInfo.Visibility = Visibility.Collapsed;

                            switchTab(true, btn_properties);
                            brd_properties.Visibility = Visibility.Visible;

                            switchTab(false, btn_serialInfo);
                            brd_serialInfo.Visibility = Visibility.Collapsed;
                            break;
                        }
                     case "btn_serialInfo":
                        {
                            switchTab(false, btn_itemInfo);
                            brd_ItemInfo.Visibility = Visibility.Collapsed;

                            switchTab(false, btn_properties);
                            brd_properties.Visibility = Visibility.Collapsed;

                            switchTab(true, btn_serialInfo);
                            brd_serialInfo.Visibility = Visibility.Visible;
                            break;
                        }
                    default:
                            break;
                }



            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        /*
        private void Btn_serialInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switchTab(false, btn_itemInfo);
                brd_ItemInfo.Visibility = Visibility.Collapsed;

                switchTab(true, btn_serialInfo);
                brd_serialInfo.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */

        #region extraOrders
        void BuildExtraOrdersDesign(List<Item> itemsPackageList)
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
                itemNameText.Text = item.name + " - " + item.unitName;
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
                itemCountText.Text = item.itemCount.ToString();
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

        private void Lst_serials_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
               // if (lst_serials.SelectedItem != null)
                {
                    Serial serial = null;
                    if (invType.Equals("sbd"))
                    {
                       // serial = returnedSerials.Where(x => x.serialNum == lst_serials.SelectedItem.ToString()).FirstOrDefault();
                        returnedSerials.Remove(serial);

                        foreach (var t in gridItems)
                        {
                            var serialCount = returnedSerials.Where(x => x.itemUnitId == t.itemUnitId).Count();
                            if (serialCount.Equals(it.itemCount))
                                t.valid = true;
                            else
                                t.valid = false;
                        }
                    }
                    else
                    {
                       // serial = itemsSerials.Where(x => x.serialNum == lst_serials.SelectedItem.ToString()).FirstOrDefault();
                        itemsSerials.Remove(serial);

                        foreach (var t in gridItems)
                        {
                            var serialCount = itemsSerials.Where(x => x.itemUnitId == t.itemUnitId).Count();
                            if (serialCount.Equals(it.itemCount))
                                t.valid = true;
                            else
                                t.valid = false;
                        }
                        
                    }
                    dg_items.ItemsSource =null;
                    dg_items.ItemsSource = gridItems;
                   // serialList.Remove(lst_serials.SelectedItem.ToString());
                    _serialCount--;
                    fillSerialList();
                }
            }
            catch
            {

            }
        }

        private void delete_click(object sender, RoutedEventArgs e)
        {        
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_serialNum);

                var row = (Serial)dg_serials.SelectedItem;
                var serials = (List<Serial>)dg_serials.ItemsSource;

                #region
                Window.GetWindow(this).Opacity = 0.2;
                wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;
                #endregion

                if (w.isOk)
                {
                    serials.Remove(row);

                    dg_serials.ItemsSource = null;
                    dg_serials.ItemsSource = serials;

                    switch (invType)
                    {

                        case "sd":
                        case "exd":
                        case "exw":
                            
                            itemsSerials.Remove(itemsSerials.Where(x => x.itemUnitId == it.itemUnitId && x.serialNum == row.serialNum).FirstOrDefault());
                            break;
                        case "pd":
                        case "isd":
                            itemsSerials.Remove(itemsSerials.Where(x => x.itemUnitId == itemUnit.itemUnitId && x.serialNum == row.serialNum).FirstOrDefault());
                         break;

                        case "sbd":
                            returnedSerials.Remove(returnedSerials.Where(x => x.itemUnitId == it.itemUnitId && x.serialNum == row.serialNum).FirstOrDefault());
                            break;
                        case "pbd":
                            returnedSerials.Remove(returnedSerials.Where(x => x.itemUnitId == itemUnit.itemUnitId && x.serialNum == row.serialNum).FirstOrDefault());
                            break;


                    };
                    refreshValidIcon();
                    setSerialCountText();
                }
                    

                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            
        }

        private async void Btn_addItemProps_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if(itemUnitId >0)
                {

                //if (sender != null)
                //    SectionData.StartAwait(grid_propertiesMainGrid);
                this.Opacity = 0;
                wd_itemUnitPropertiesSelection w = new wd_itemUnitPropertiesSelection();
                w.itemUnitId = 0;
                w.ShowDialog();

                // refreshPropertiesCombo
                //await FillCombo.RefreshCustomers();
                //await RefrishCustomers();

                this.Opacity = 1;

                //if (sender != null)
                //    SectionData.EndAwait(grid_propertiesMainGrid);
                }

            }
            catch (Exception ex)
            {
                this.Opacity = 1;
                //if (sender != null)
                //    SectionData.EndAwait(grid_propertiesMainGrid);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_itemPropsCount_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    if (validateProperty())
                        PropertyEnterProcess();
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Cb_minUnitProperties_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void Cb_minUnit_KeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}
