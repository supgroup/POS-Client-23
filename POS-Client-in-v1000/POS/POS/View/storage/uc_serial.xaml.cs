using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using netoaster;
using POS.Classes;
using POS.View.catalog;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
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

namespace POS.View.storage
{
    /// <summary>
    /// Interaction logic for uc_serial.xaml
    /// </summary>
    public partial class uc_serial : UserControl
    {
        #region variables
        ItemUnit itemUnit = new ItemUnit();
        IEnumerable<ItemUnit> itemUnits;
        IEnumerable<ItemUnit> itemUnitsLst;
        IEnumerable<ItemUnit> itemUnitsQuery;
        string searchText = "";
        string basicsPermission = "serial_basics";
        string packagePermission = "serial_package";
        string unitConversionPermission = "serial_unitConversion";

        Package package = new Package();
        List<Package> packages = new List<Package>();
        //int _itemUnitId = 0;
        //int _itemId = 0;
        //155
        //156
        #endregion

        public uc_serial()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            { SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        private static uc_serial _instance;
        public static uc_serial Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_serial();
                return _instance;
            }
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

                clear();

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

                await RefreshItemUnitsList();
                await Search();
                chk_all.IsChecked = true;

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

        #region events
        void clear()
        {
            btn_serials.IsEnabled = false;
            btn_properties.IsEnabled = false;
            btn_package.IsEnabled = false;
            btn_unitConversion.IsEnabled = false;

            brd_itemsPackage.Visibility = Visibility.Collapsed;
            txt_notPackageType.Visibility = Visibility.Visible;
            this.DataContext = new ItemUnit();

        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }
        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                await Search();

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
        private async void Tgl_search_event(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                await Search();

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
        private void search_Checking(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                CheckBox cb = sender as CheckBox;
                if (cb != null)
                {
                    if (cb.Name == "chk_all")
                    {
                        chk_isSerial.IsChecked = false;
                        chk_isPackage.IsChecked = false;
                    }
                    else if (cb.Name == "chk_isSerial")
                    {
                        chk_all.IsChecked = false;
                        chk_isPackage.IsChecked = false;
                    }
                    else if (cb.Name == "chk_isPackage")
                    {
                        chk_all.IsChecked = false;
                        chk_isSerial.IsChecked = false;
                    }
                }

                Tb_search_TextChanged(tb_search, null);

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

        private void chk_uncheck(object sender, RoutedEventArgs e)
        {

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                CheckBox cb = sender as CheckBox;
                if (cb.IsFocused)
                {
                    if (cb.Name == "chk_all")
                        chk_all.IsChecked = true;
                    else if (cb.Name == "chk_isSerial")
                        chk_isSerial.IsChecked = true;
                    else if (cb.Name == "chk_isPackage")
                        chk_isPackage.IsChecked = true;
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
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    searchText = "";
                    tb_search.Text = "";
                    await RefreshItemUnitsList();
                    await Search();
                }
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
        private async void Dg_itemUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                if (dg_itemUnit.SelectedIndex != -1)
                {
                    btn_serials.IsEnabled = true;
                    //btn_properties.IsEnabled = true;

                    btn_unitConversion.IsEnabled = true;

                    itemUnit = dg_itemUnit.SelectedItem as ItemUnit;
                    this.DataContext = itemUnit;
                    //_itemUnitId = ;
                    //_itemId = ;
                    if (itemUnit.itemType == "p")
                    {
                        brd_itemsPackage.Visibility = Visibility.Visible;
                        txt_notPackageType.Visibility = Visibility.Collapsed;
                        btn_package.IsEnabled = true;
                        packages = await package.GetChildsByParentId(itemUnit.itemUnitId);
                        BuildPackageItemsDesign(packages);
                        btn_properties.IsEnabled = false;
                        btn_unitConversion.IsEnabled = false;
                    }
                    else
                    {
                        brd_itemsPackage.Visibility = Visibility.Collapsed;
                        txt_notPackageType.Visibility = Visibility.Visible;
                        btn_package.IsEnabled = false;
                        btn_properties.IsEnabled = true;
                        btn_unitConversion.IsEnabled = true;
                    }

                    if (itemUnit.itemType == "sn")
                        btn_serials.IsEnabled = true;
                    else
                        btn_serials.IsEnabled = false;
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

        #region methods
        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("features");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trBaseInformation");
            txt_packageItem.Text = MainWindow.resourcemanager.GetString("packageItems");

            txt_itemTitle.Text = MainWindow.resourcemanager.GetString("trItem");
            txt_itemUnitTitle.Text = MainWindow.resourcemanager.GetString("trUnit");
            txt_quantityTitle.Text = MainWindow.resourcemanager.GetString("trQuantity");
            txt_serialsCountTitle.Text = MainWindow.resourcemanager.GetString("trSerials");
            txt_propertiesCountTitle.Text = MainWindow.resourcemanager.GetString("trProperties");

            chk_all.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_isSerial.Content = MainWindow.resourcemanager.GetString("trSerials");
            chk_isPackage.Content = MainWindow.resourcemanager.GetString("trPackages");

            dg_itemUnit.Columns[0].Header = MainWindow.resourcemanager.GetString("trItem");
            dg_itemUnit.Columns[1].Header = MainWindow.resourcemanager.GetString("trQTR");
            dg_itemUnit.Columns[2].Header = MainWindow.resourcemanager.GetString("trProperties");
            dg_itemUnit.Columns[3].Header = MainWindow.resourcemanager.GetString("trSerials");

            txt_packageButton.Text = MainWindow.resourcemanager.GetString("trPackage");
            txt_unitConversionButton.Text = MainWindow.resourcemanager.GetString("trUnitConversion");
            txt_serialsButton.Text = MainWindow.resourcemanager.GetString("trSerials");
            txt_propertiesButton.Text = MainWindow.resourcemanager.GetString("trProperties");
            txt_notPackageType.Text = MainWindow.resourcemanager.GetString("notPackageType") + "!";

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
        }
        async Task Search()
        {
            if (itemUnits is null)
                await RefreshItemUnitsList();

            searchText = tb_search.Text.ToLower();

            itemUnitsQuery = itemUnits
                .Where(s =>
            (s.itemName.ToString() + " - " + s.unitName.ToString()).ToLower().Contains(searchText)
             && (chk_isSerial.IsChecked.Value ? s.itemType.Equals("sn") : true)
             && (chk_isPackage.IsChecked.Value ? s.itemType.Equals("p") : true)
            );

            RefreshItemUnitView();
        }
        async Task<IEnumerable<ItemUnit>> RefreshItemUnitsList()
        {

            //itemUnits = await itemUnit.GetIUbyBranch(MainWindow.branchID.Value);
            itemUnits = await itemUnit.GetIUBranchWithCount(MainWindow.loginBranch.branchId);
            return itemUnits;
        }
        void RefreshItemUnitView()
        {

            dg_itemUnit.ItemsSource = itemUnitsQuery;
            txt_count.Text = itemUnitsQuery.Count().ToString();

        }

        #endregion

        #region repots
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        private void pdf()
        {
            BuildReport();


            saveFileDialog.Filter = "PDF|*.pdf;";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filepath = saveFileDialog.FileName;
                LocalReportExtensions.ExportToPDF(rep, filepath);
            }

        }

        private void BuildReport()
        {
            string searchval = "";
            string stateval = "";
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string addpath;
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\Store\Ar\ArSerialMain.rdlc";
            }
            else
            {
                addpath = @"\Reports\Store\En\SerialMain.rdlc";
            }
            //filter  
            if (chk_all.IsChecked == true)
            {
                stateval = MainWindow.resourcemanagerreport.GetString("trAll");
            }
            else if (chk_isSerial.IsChecked == true)
            {
                stateval = MainWindow.resourcemanagerreport.GetString("trSerials");
            }
            else if (chk_isPackage.IsChecked == true)
            {
                stateval = MainWindow.resourcemanagerreport.GetString("trPackages");
            }
            paramarr.Add(new ReportParameter("stateval", stateval));            
            paramarr.Add(new ReportParameter("trItemType", MainWindow.resourcemanagerreport.GetString("trItemType")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            searchval = tb_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            //    IEnumerable<Slice> slicesQuery;
            clsReports.SerialMainReport(itemUnitsQuery, rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            rep.SetParameters(paramarr);
            rep.Refresh();

        }
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    pdf();
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
        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    /////////////////////////////////////
                    //Thread t1 = new Thread(() =>
                    //{
                    print();
                    //});
                    //t1.Start();
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


        private void btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie

        }
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    #region
                    excel();
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
        private void print()
        {
            BuildReport();


            LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

        }
        private void excel()
        {
            BuildReport();

            saveFileDialog.Filter = "EXCEL|*.xls;";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filepath = saveFileDialog.FileName;
                LocalReportExtensions.ExportToExcel(rep, filepath);
            }



        }



        #endregion

        private async void Btn_serials_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (
                    MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") ||
                    MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update") ||
                    MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") ||
                    MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") ||
                    MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report")
                    )
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_serialNumData w = new wd_serialNumData();
                    w.itemunitId = itemUnit.itemUnitId;
                    w.count = itemUnit.serialsCount.Value;
                    w.quantity = itemUnit.quantity.Value;
                    w.item = itemUnit.itemName;
                    w.unit = itemUnit.unitName;
                    w.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;

                    if (w.isOk)
                    {
                        await RefreshItemUnitsList();
                        await Search();
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

        private async void Btn_properties_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (
                   MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") ||
                   MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update") ||
                   MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") ||
                   MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") ||
                   MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report")
                   )
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_itemUnitPropertiesSelection w = new wd_itemUnitPropertiesSelection();
                    w.itemUnitId = itemUnit.itemUnitId;
                    w.ItemProperties = itemUnit.ItemProperties;
                    w.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;

                    {
                        await RefreshItemUnitsList();
                        await Search();
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

        private async void Btn_package_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(packagePermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_generatePackage w = new wd_generatePackage();
                    w._itemUnitId = itemUnit.itemUnitId;
                    w.packages = packages;

                    if (w.ShowDialog() == true)
                    {

                    }
                    {
                        await RefreshItemUnitsList();
                        await Search();
                    }
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
        private async void Btn_unitConversion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(unitConversionPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;

                    wd_unitConversion w = new wd_unitConversion();
                    w._itemId = itemUnit.itemId.Value;
                    w._itemUnitId = itemUnit.itemUnitId;
                    w.itemName = itemUnit.itemName;
                    w.unitName = itemUnit.unitName;
                    if (w.ShowDialog() == true)
                    {

                    }
                    {
                        await RefreshItemUnitsList();
                        await Search();
                    }
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

        #region packageItems
        void BuildPackageItemsDesign(List<Package> itemsPackageList)
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


    }
}
