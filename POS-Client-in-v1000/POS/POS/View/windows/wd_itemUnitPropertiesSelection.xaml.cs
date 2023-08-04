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
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System.IO;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_itemUnitPropertiesSelection.xaml
    /// </summary>
    public partial class wd_itemUnitPropertiesSelection : Window
    {
        #region variables

        StoreProperty storePropertyModel = new StoreProperty();
        StoreProperty selectedProperty = new StoreProperty();
        Invoice invoice = new Invoice();
        List<StoreProperty> storeProperty;
        public List<StoreProperty> ItemProperties;
        List<StoreProperty> storePropertyQuery;
         public int itemUnitId;
        string searchText = "";
        BrushConverter bc = new BrushConverter();
        string basicsPermission = "serial_basics";
        string soldPermission = "serial_sold";
        #endregion

        public wd_itemUnitPropertiesSelection()
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
                    SectionData.StartAwait(grid_main);

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
                btn_sold.IsEnabled = false;
                btn_update.IsEnabled = false;

                fillPropertiesCombo();

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

        #region methods
        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trProperties");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_itemProps, MainWindow.resourcemanager.GetString("trPropertiesHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_itemPropsCount, MainWindow.resourcemanager.GetString("trCountHint"));
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trBaseInformation");

            col_propName.Header = MainWindow.resourcemanager.GetString("trProperties");
            col_count.Header = MainWindow.resourcemanager.GetString("trCount");

            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");
            btn_refresh.ToolTip = MainWindow.resourcemanager.GetString("trRefresh");
            txt_soldButton.Text = MainWindow.resourcemanager.GetString("sold_");
            txt_updateButton.Text = MainWindow.resourcemanager.GetString("trUpdate");
            txt_addButton.Text = MainWindow.resourcemanager.GetString("trAdd");


            tt_clear.Content = MainWindow.resourcemanager.GetString("trClear");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");

          

        }



        async Task Search()
        {
            if (storeProperty is null)
                await RefreshStorePropertyList();

            fillPropertiesCombo();
            searchText = tb_search.Text.ToLower();

            storePropertyQuery = storeProperty.Where(s => 
           (s.propValue.ToString().ToLower().Contains(searchText) )).ToList();
            RefreshStorePropertyView();
        }
        async Task<IEnumerable<StoreProperty>> RefreshStorePropertyList()
        {
            storeProperty = await invoice.GetAvailableProperties(itemUnitId, MainWindow.loginBranch.branchId);
            return storeProperty;
        }


        void RefreshStorePropertyView()
        {
            dg_storeProperty.ItemsSource = storePropertyQuery;
            txt_count.Text = dg_storeProperty.Items.Count.ToString();
        }

        private void Clear()
        {
            this.DataContext = new StoreProperty();
            tb_itemPropsCount.Text = "";
            btn_sold.IsEnabled = false;
            btn_update.IsEnabled = false;
            btn_add.IsEnabled = true;

            dg_storeProperty.SelectedIndex = -1;

            fillPropertiesCombo();
        }
        private void validateEmpty(string name, object sender)
        {
            try
            {
                if (name == "ComboBox")
                {
                    //if ((sender as ComboBox).Name == "cb_selectProperties")
                    //    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorProperty, tt_errorProperty, "trIsRequired");
                    //else if ((sender as ComboBox).Name == "cb_value")
                    //    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorValue, tt_errorValue, "trIsRequired");
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void fillPropertiesCombo()
        {
            try
            {
                List<StoreProperty> propValuesConcat = new List<StoreProperty>();
                if (dg_storeProperty.SelectedIndex == -1)
                {
                    if (ItemProperties != null)
                    {
                        Dictionary<int, List<StoreProperty>> tags = new Dictionary<int, List<StoreProperty>>();
                        var properties = ItemProperties.Select(x => new { propertyId = x.propertyId, name = x.propName, index = x.propertyIndex }).ToList().Distinct().OrderBy(x => x.index).ToList();

                        int i = 1;
                        foreach (var pr in properties)
                        {
                            var propValues = ItemProperties.Where(x => x.propertyId == pr.propertyId).ToList();
                            tags.Add(i, propValues);

                            i++;
                        }
                        propValuesConcat = GetCombos(tags);
                    }
                    else
                        propValuesConcat = new List<StoreProperty>();
                }
                else
                {
                    propValuesConcat = new List<StoreProperty>();
                    propValuesConcat.Add(new StoreProperty() { propValue = selectedProperty.propValue, notes = selectedProperty.notes });
                }

                cb_itemProps.ItemsSource = propValuesConcat.ToList();
                cb_itemProps.DisplayMemberPath = "propValue";
                cb_itemProps.SelectedValuePath = "notes";
                cb_itemProps.SelectedIndex = 0;
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
                        stp.propValue = tagPart.propName + ": " + tagPart.propValue + ", " + combo.propName + ": " + combo.propValue;
                        stp.notes = tagPart.propertyItemId.ToString() + "," + combo.propertyItemId.ToString();
                        outputs.Add(stp);
                    }
                }

                return outputs;
            }
        }

        #endregion

        #region events
        private void dg_propertiesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                selectedProperty = dg_storeProperty.SelectedItem as StoreProperty;
                tb_itemPropsCount.Text = selectedProperty.count.ToString();
                fillPropertiesCombo();
                btn_sold.IsEnabled = true;
                btn_update.IsEnabled = true;
                btn_add.IsEnabled = false;

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
        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    //Btn_save_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
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
        private async void Cb_propertiesSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    tb_search.Text = "";
                    searchText = "";

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
        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                Clear();

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
        private void Tb_TextChanged(object sender, TextChangedEventArgs e)
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
        private void Tb_LostFocus(object sender, RoutedEventArgs e)
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
            /*
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath;
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\Sale\Ar\Price.rdlc";
            }
            else
            {
                addpath = @"\Reports\Sale\En\Price.rdlc";
            }
            
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
             clsReports.PriceReport(propQuery, rep, reppath, paramarr);
            paramarr.Add(new ReportParameter("invoiceClass", slice.name));
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);

            rep.SetParameters(paramarr);

            rep.Refresh();
            */
        }
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                //{
                pdf();
                //}
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
        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                //{
                /////////////////////////////////////
                //Thread t1 = new Thread(() =>
                //{
                print();
                //});
                //t1.Start();
                //////////////////////////////////////
                //}
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
        private void btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    //win_lvcCatalog win = new win_lvcCatalog(slicesQuery, 6);
                    //win.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                }
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

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
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                //{
                #region
                excel();
                #endregion
                //}
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

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                //{
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
                //}
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

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
        private void delete_click(object sender, RoutedEventArgs e)
        {
            /*
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_serialNum);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete"))
                {
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
                            itemsSerials.Remove(itemsSerials.Where(x => x.itemUnitId == item.itemUnitId && x.serialNum == row.serialNum).FirstOrDefault());
                            break;
                        case "pd":
                        case "isd":
                            itemsSerials.Remove(itemsSerials.Where(x => x.itemUnitId == itemUnit.itemUnitId && x.serialNum == row.serialNum).FirstOrDefault());
                            break;

                        case "sbd":
                            returnedSerials.Remove(returnedSerials.Where(x => x.itemUnitId == item.itemUnitId && x.serialNum == row.serialNum).FirstOrDefault());
                            break;
                        case "pbd":
                            returnedSerials.Remove(returnedSerials.Where(x => x.itemUnitId == itemUnit.itemUnitId && x.serialNum == row.serialNum).FirstOrDefault());
                            break;


                    };
                    refreshValidIcon();
                    setSerialCountText();
                }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);


                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_serialNum);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            */
        }
        private bool validateSave()
        {
            bool valid = true;
            if (!SectionData.validateEmptyTextBox(tb_itemPropsCount, p_errorItemPropsCount, tt_errorItemPropsCount, "trEmptyCount"))
                valid = false;
            else if (int.Parse(tb_itemPropsCount.Text) == 0)
            {
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorQuantIsZeroToolTip:") , animation: ToasterAnimation.FadeIn);
                valid = false;
            }
            return valid;
        }
        private bool validateUpdate()
        {
            bool valid = true;

            if (!SectionData.validateEmptyTextBox(tb_itemPropsCount, p_errorItemPropsCount, tt_errorItemPropsCount, "trEmptyCount"))
                valid = false;

            return valid;
        }
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {
            //add
            try
            {
                SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add"))
                {
                    if (validateSave())
                    {
                        int count = int.Parse( tb_itemPropsCount.Text);
                        int res = (int)await storePropertyModel.AddProperties(itemUnitId, cb_itemProps.SelectedValue.ToString(), count, MainWindow.branchID.Value, MainWindow.userID.Value);
                        if (res > 0)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                            Clear();
                            await RefreshStorePropertyList();
                        }
                        else if(res == -1)
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("overQuantity"), animation: ToasterAnimation.FadeIn);
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
                    

                    await Search();
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private bool validateSold()
        {
            bool valid = true;
            if (!SectionData.validateEmptyTextBox(tb_itemPropsCount, p_errorItemPropsCount, tt_errorItemPropsCount, "trEmptyCount"))
                valid = false;
            else if (int.Parse(tb_itemPropsCount.Text) == 0)
            {
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorQuantIsZeroToolTip"), animation: ToasterAnimation.FadeIn);
                valid = false;
            }
            else if (int.Parse(tb_itemPropsCount.Text) > selectedProperty.count)
            {
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + selectedProperty.count, animation: ToasterAnimation.FadeIn);
                valid = false;
            }
                return valid;
        }
        private async void Btn_sold_Click(object sender, RoutedEventArgs e)
        {//sold
    
            try
            {
                SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(soldPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (validateSold())
                    {
                        int count = int.Parse(tb_itemPropsCount.Text);
                        int res = (int)await storePropertyModel.SoldProperties((int)selectedProperty.itemUnitId,selectedProperty.notes,count,MainWindow.branchID.Value,MainWindow.userID.Value);
                        if (res > 0)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSold"), animation: ToasterAnimation.FadeIn);
                            Clear();
                            await RefreshStorePropertyList();
                        }
                        else if(res == -10) 
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableToolTip"), animation: ToasterAnimation.FadeIn);

                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
                    

                    await Search();
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
       
        }

        #region properties combo
        /*
        private async void fillPropertiesCombo()
        {
            Dictionary<int, List<StoreProperty>> tags = new Dictionary<int, List<StoreProperty>>();

            switch (invType)
            {
                case "pd":
                case "isd":
                    var properties = ItemProperties.Select(x => new { propertyId = x.propertyId, name = x.propName, index = x.propertyIndex }).Distinct().OrderBy(x => x.index).ToList();

                    int i = 1;
                    foreach (var pr in properties)
                    {
                        var propValues = ItemProperties.Where(x => x.propertyId == pr.propertyId).ToList();
                        tags.Add(i, propValues);

                        i++;
                    }
                    propValuesConcat = GetCombos(tags);
                    break;

                case "sd":
                case "exw":
                case "exd":
                    propValuesConcat = await invoiceModel.GetAvailableProperties((int)it.itemUnitId, MainWindow.branchID.Value);
                    break;
            };
            cb_itemProps.ItemsSource = propValuesConcat.ToList();
            cb_itemProps.DisplayMemberPath = "propValue";
            cb_itemProps.SelectedValuePath = "notes";
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
                        stp.propValue = tagPart.propName + ": " + tagPart.propValue + ", " + combo.propName + ": " + combo.propValue;
                        stp.notes = tagPart.propertyItemId.ToString() + "," + combo.propertyItemId.ToString();
                        outputs.Add(stp);
                    }
                }

                return outputs;
            }
        }
        */
        private void Cb_storePropertys_KeyUp(object sender, KeyEventArgs e)
        {
            /*
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
            */
        }

        StoreProperty sp;
        private void Cb_itemProps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
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
            */
        }
        #endregion

        #region count changed
        private void Tb_itemPropsCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            try
            {
                if (sp != null && (invType.Equals("sd") || invType.Equals("exw") || invType.Equals("exd")))
                {
                    if (int.Parse(tb_itemPropsCount.Text) > sp.count)
                    {
                        tb_itemPropsCount.Text = sp.count.ToString();
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("PropertiesNotAvailable"), animation: ToasterAnimation.FadeIn);
                    }
                }
            }
            catch
            {

            }
        */

        }
        #endregion
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

        private async void Btn_update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(soldPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (validateUpdate())
                    {
                        int count = int.Parse(tb_itemPropsCount.Text);
                        int res = (int)await storePropertyModel.UpdateProperties((int)selectedProperty.itemUnitId, selectedProperty.notes, count, MainWindow.branchID.Value, MainWindow.userID.Value,selectedProperty.storeProbId);
                        if (res > 0)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                            Clear();
                            await RefreshStorePropertyList();
                        }
                        else if (res == -1)
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("overQuantity"), animation: ToasterAnimation.FadeIn);
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }


                    await Search();
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
