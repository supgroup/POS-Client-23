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
    /// Interaction logic for wd_sliceItemUnits.xaml
    /// </summary>
    public partial class wd_sliceItemUnits : Window
    {
        #region variables
        Price priceModel = new Price();
        Price price = new Price();
        IEnumerable<Price> pricesQuery;
        IEnumerable<Price> prices;
        public int sliceId;
        bool tgl_priceState = true;
        string searchText = "";
        BrushConverter bc = new BrushConverter();
      public  Slice slice = new Slice();
        #endregion

        public wd_sliceItemUnits()
        {
            try
            {
                InitializeComponent();

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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_sliceMain);

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

                await FillCombo.fillItemCombo(cb_item);

                if (FillCombo.itemunitsList is null)
                    await FillCombo.RefreshItemUnits();

                await Search();

                #region key up
                // key_up item
                cb_item.IsTextSearchEnabled = false;
                cb_item.IsEditable = true;
                cb_item.StaysOpenOnEdit = true;
                cb_item.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_item.Text = "";
                // key_up item
                cb_unit.IsTextSearchEnabled = false;
                cb_unit.IsEditable = true;
                cb_unit.StaysOpenOnEdit = true;
                cb_unit.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_unit.Text = "";
                #endregion


                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        #region methods
        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("pricing");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trBaseInformation");
            txt_unitCost.Text = MainWindow.resourcemanager.GetString("trCost");
            txt_itemUnitPrice.Text = MainWindow.resourcemanager.GetString("basePrice");
            txt_activeSearch.Text = MainWindow.resourcemanager.GetString("trActive");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_item, MainWindow.resourcemanager.GetString("trItem")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_unit, MainWindow.resourcemanager.GetString("trUnit") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_price, MainWindow.resourcemanager.GetString("trPriceHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_notes, MainWindow.resourcemanager.GetString("trNoteHint"));

            txt_addButton.Text = MainWindow.resourcemanager.GetString("trAdd");
            txt_updateButton.Text = MainWindow.resourcemanager.GetString("trUpdate");
            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");

            dg_items.Columns[0].Header = MainWindow.resourcemanager.GetString("trItem");
            dg_items.Columns[1].Header = MainWindow.resourcemanager.GetString("trPrice");
            dg_items.Columns[2].Header = MainWindow.resourcemanager.GetString("trCost");
            dg_items.Columns[3].Header = MainWindow.resourcemanager.GetString("basePrice");

            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");

            tt_add_Button.Content = MainWindow.resourcemanager.GetString("trAdd");
            tt_update_Button.Content = MainWindow.resourcemanager.GetString("trUpdate");
            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

            tt_clear.Content = MainWindow.resourcemanager.GetString("trClear");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
        }
        async Task Search()
        {
            if (prices is null)
                await RefreshPricesList();

            searchText = tb_search.Text.ToLower();

            pricesQuery = prices.Where(s => (
            (
            s.itemName.ToString() + " - " + s.unitName.ToString()) .ToLower().Contains(searchText) ||
            s.price.ToString().ToLower().Contains(searchText)
            )
            && s.isActive == tgl_priceState
            );
            RefreshPriceView();
        }
        async Task<IEnumerable<Price>> RefreshPricesList()
        {
            prices = await priceModel.getBySliceId(sliceId);
            foreach(var p in prices)
            {
                p.isSelect = true;
            }
            return prices;
        }
        void RefreshPriceView()
        {
            dg_items.ItemsSource = pricesQuery;
            txt_count.Text = dg_items.Items.Count.ToString();
        }
        private void Clear()
        {
            this.DataContext = new Price();

            SectionData.clearComboBoxValidate(cb_unit, p_errorUnit);
            SectionData.clearComboBoxValidate(cb_item, p_errorItem);
            SectionData.clearValidate(tb_price, p_errorPrice);

            cb_item.SelectedIndex = -1;
            cb_unit.SelectedIndex = -1;
        }
        private void validateEmpty(string name, object sender)
        {
            try
            {
                if (name == "TextBox")
                {
                    if ((sender as TextBox).Name == "tb_price")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorPrice, tt_errorPrice, "trIsRequired");
                }
                else if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_item")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorItem, tt_errorItem, "trIsRequired");
                    else if ((sender as ComboBox).Name == "cb_unit")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorUnit, tt_errorUnit, "trIsRequired");
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async Task activate()
        {//activate

            price.isActive = true;

            int s = (int)await price.Save(price);

            if (s > 0)
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopActive"), animation: ToasterAnimation.FadeIn);
            else
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

            await RefreshPricesList();
            await Search();
        }
        private bool chkDuplicateItemUnit()
        {
            bool b = false;

            if (prices.Any(c => c.itemUnitId == (int)cb_unit.SelectedValue))
                b = true;
           
            return b;
        }
        private async Task<bool> chkDuplicate(Nullable<int> itemUnitId)
        {
            await RefreshPricesList();

            if (prices.Where(x => x.itemUnitId == itemUnitId && x.priceId != price.priceId).Count() > 0)
                return true;
            else
                return false;
        }
        #endregion

        #region events
        private void Cb_item_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = FillCombo.itemsList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_unit_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = units.Where(p => p.unitName.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Dg_items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                //if (sender != null)
                //    SectionData.StartAwait(grid_sliceMain);
                
                SectionData.clearComboBoxValidate(cb_item, p_errorItem);
                SectionData.clearComboBoxValidate(cb_unit, p_errorUnit);
                SectionData.clearValidate(tb_price, p_errorPrice);
                cb_item.SelectedIndex = -1;

                if (dg_items.SelectedIndex != -1)
                {
                    price = dg_items.SelectedItem as Price;
                    this.DataContext = price;
                    cb_item.SelectedValue = price.itemId;
                }
                else
                {
                    cb_unit.SelectedIndex = -1;
                }

                if (price != null)
                {
                    #region delete
                    if (price.canDelete)
                    {
                        txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                        txt_delete_Icon.Kind =
                                 MaterialDesignThemes.Wpf.PackIconKind.Delete;
                        tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");
                    }
                    else
                    {
                        if (price.isActive == false)
                        {
                            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trActive");
                            txt_delete_Icon.Kind =
                             MaterialDesignThemes.Wpf.PackIconKind.Check;
                            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trActive");

                        }
                        else
                        {
                            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trInActive");
                            txt_delete_Icon.Kind =
                                 MaterialDesignThemes.Wpf.PackIconKind.Cancel;
                            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trInActive");
                        }
                    }
                    #endregion
                }
                //if (sender != null)
                //    SectionData.EndAwait(grid_sliceMain);
            }
            catch (Exception ex)
            {
                //if (sender != null)
                //    SectionData.EndAwait(grid_sliceMain);
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
                    SectionData.StartAwait(grid_sliceMain);

                await Search();

                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_sliceMain);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    tb_search.Text = "";
                    searchText = "";

                    await Search();
                }
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Tgl_sliceIsActive_Checked(object sender, RoutedEventArgs e)
        {//active
            try
            {
                if (tgl_sliceIsActive.IsFocused)
                {
                    if (sender != null)
                        SectionData.StartAwait(grid_sliceMain);

                    tgl_priceState = true;
                    await Search();

                    if (sender != null)
                        SectionData.EndAwait(grid_sliceMain);
                }

            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Tgl_sliceIsActive_Unchecked(object sender, RoutedEventArgs e)
        {//inactive
            try
            {
                if (tgl_sliceIsActive.IsFocused)
                {
                    if (sender != null)
                        SectionData.StartAwait(grid_sliceMain);

                    tgl_priceState = false;
                    await Search();

                    if (sender != null)
                        SectionData.EndAwait(grid_sliceMain);
                }
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_sliceMain);

                Clear();

                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
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
        List<ItemUnit> units = new List<ItemUnit>();
        private async void Cb_item_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select item
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_sliceMain);

                if (cb_item.SelectedIndex != -1)
                {
                    SectionData.clearComboBoxValidate(cb_item, p_errorItem);
                    //await FillCombo.fillItemUnitCombo(cb_unit, (int)cb_item.SelectedValue);
                    if (FillCombo.itemunitsList is null)
                        await FillCombo.RefreshItemUnits();
                    units = FillCombo.itemunitsList.Distinct().ToList();
                    units = units.Where(u => u.itemId == (int)cb_item.SelectedValue).ToList();
                    cb_unit.ItemsSource = units;
                    cb_unit.DisplayMemberPath = "unitName";
                    cb_unit.SelectedValuePath = "itemUnitId";
                    cb_unit.SelectedValue = price.itemUnitId;
                }

                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Cb_unit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select item
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_sliceMain);

                if (cb_unit.SelectedIndex != -1)
                {
                    SectionData.clearComboBoxValidate(cb_unit, p_errorUnit);
                    grd_cost.DataContext = units.Where(i => i.itemUnitId == (int)cb_unit.SelectedValue);
                    grd_price.DataContext = units.Where(i => i.itemUnitId == (int)cb_unit.SelectedValue);
                }
                else
                {
                    grd_cost.DataContext = new ItemUnit();
                    grd_price.DataContext = new ItemUnit();
                }

                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
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
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string searchval = "";
            string stateval = "";
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
            //filter   
            stateval = tgl_sliceIsActive.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trActive_")
              : MainWindow.resourcemanagerreport.GetString("trNotActive");
            paramarr.Add(new ReportParameter("stateval", stateval));
            paramarr.Add(new ReportParameter("trActiveState", MainWindow.resourcemanagerreport.GetString("trState")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            searchval = tb_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            
            ReportCls.checkLang();
            //    IEnumerable<Slice> slicesQuery;
            clsReports.PriceReport(pricesQuery, rep, reppath, paramarr);
            paramarr.Add(new ReportParameter("invoiceClass", slice.name));
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

        #region add - update - delete
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {//add
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_sliceMain);
                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    price.priceId = 0;
                    Nullable<int> itemUnitId = null;

                    #region validate
                    SectionData.validateEmptyTextBox(tb_price, p_errorPrice, tt_errorPrice, "trIsRequired");
                    SectionData.validateEmptyComboBox(cb_item, p_errorItem, tt_errorItem, "trIsRequired");
                    SectionData.validateEmptyComboBox(cb_unit, p_errorUnit, tt_errorUnit, "trIsRequired");
                    //bool chkIsItemUnitExist = chkDuplicateItemUnit();
                    //if(chkIsItemUnitExist)
                    //{
                    //    SectionData.showComboBoxValidate(cb_item, p_errorItem , tt_errorItem , "trDuplicateCodeToolTip");
                    //    SectionData.showComboBoxValidate(cb_unit, p_errorUnit , tt_errorUnit , "trDuplicateCodeToolTip");
                    //}
                    #endregion

                    if (!cb_item.Text.Equals("") && !cb_unit.Text.Equals("") && !tb_price.Text.Equals(""))
                    {

                        if (cb_unit.SelectedIndex != -1)
                            itemUnitId = (int)cb_unit.SelectedValue;

                        bool isExist = await chkDuplicate(itemUnitId);
                        if (isExist)
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUnitExist"), animation: ToasterAnimation.FadeIn);
                            p_errorUnit.Visibility = Visibility.Visible;
                            tt_errorUnit.Content = MainWindow.resourcemanager.GetString("trPopUnitExist");
                            cb_unit.Background = (Brush)bc.ConvertFrom("#15FF0000");
                            
                        }
                        else
                        {
                            decimal _price = 0;
                            if (tb_price.Text != "")
                                _price = decimal.Parse(tb_price.Text);

                            price.itemUnitId = itemUnitId;
                            price.sliceId = sliceId;
                            price.price = _price;
                            price.notes = tb_notes.Text;
                            price.createUserId = MainWindow.userID;
                            price.updateUserId = MainWindow.userID;
                            price.isActive = true;

                            int s = (int)await priceModel.Save(price);

                            if (s > 0)
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                                Clear();
                                await RefreshPricesList();
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                            await Search();
                        }
                    }
                }
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_update_Click(object sender, RoutedEventArgs e)
        {//update
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_sliceMain);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update") || SectionData.isAdminPermision())
                {
                    if (price.priceId > 0)
                    {
                        Nullable<int> itemUnitId = null;
                        #region validate
                        //chk empty name
                        SectionData.validateEmptyTextBox(tb_price, p_errorPrice, tt_errorPrice, "trIsRequired");
                        SectionData.validateEmptyComboBox(cb_item, p_errorItem, tt_errorItem, "trIsRequired");
                        SectionData.validateEmptyComboBox(cb_unit, p_errorUnit, tt_errorItem, "trIsRequired");

                        #endregion
                        if (!cb_item.Text.Equals("") && !cb_unit.Text.Equals("") && !tb_price.Text.Equals(""))
                        {

                            if (cb_unit.SelectedIndex != -1)
                                itemUnitId = (int)cb_unit.SelectedValue;

                            bool isExist = await chkDuplicate(itemUnitId);
                            if (isExist)
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUnitExist"), animation: ToasterAnimation.FadeIn);
                                p_errorUnit.Visibility = Visibility.Visible;
                                tt_errorUnit.Content = MainWindow.resourcemanager.GetString("trPopUnitExist");
                                cb_unit.Background = (Brush)bc.ConvertFrom("#15FF0000");
                            }
                            else
                            {
                                decimal _price = 0;
                                if (tb_price.Text != "")
                                    _price = decimal.Parse(tb_price.Text);

                                price.itemUnitId = itemUnitId;
                                price.sliceId = sliceId;
                                price.price = _price;
                                price.notes = tb_notes.Text;
                                price.createUserId = MainWindow.userID;
                                price.updateUserId = MainWindow.userID;

                                int s = (int)await price.Save(price);

                                if (s > 0)
                                {
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                                    await RefreshPricesList();
                                }
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                                await Search();

                            }
                        }
                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectItemFirst"), animation: ToasterAnimation.FadeIn);

                }
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_sliceMain);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                    if (price.priceId != 0)
                    {
                        if ((!price.canDelete) && (price.isActive == false))
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
                            if (price.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                            if (!price.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDeactivate");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion

                            if (w.isOk)
                            {
                                string popupContent = "";
                                if (price.canDelete)
                                    popupContent = MainWindow.resourcemanager.GetString("trPopDelete");
                                if ((!price.canDelete) && (price.isActive == true)) popupContent = MainWindow.resourcemanager.GetString("trPopInActive");

                                int b = (int)await price.Delete(price.priceId, MainWindow.userID.Value, price.canDelete);

                                if (b > 0)
                                {
                                    price.priceId = 0;
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: popupContent, animation: ToasterAnimation.FadeIn);
                                }
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            }

                        }
                        await RefreshPricesList();
                        await Search();
                    }

                    Clear();
                }
            //else
            //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

            if (sender != null)
                SectionData.EndAwait(grid_sliceMain);
        }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_sliceMain);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

    }
        #endregion

       
    }
}
