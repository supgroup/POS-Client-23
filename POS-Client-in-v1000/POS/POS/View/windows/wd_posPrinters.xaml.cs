using netoaster;
using POS.Classes;
 using System;
using System.Collections.Generic;
using System.Linq;
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
using WPFTabTip;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_posPrinters.xaml
    /// </summary>
    public partial class wd_posPrinters : Window
    {
        public wd_posPrinters()
        {
            try
            {
                InitializeComponent();
                //if (System.Windows.SystemParameters.PrimaryScreenWidth >= 1440)
                //{
                //    txt_deleteButton.Visibility = Visibility.Visible;
                //    txt_addButton.Visibility = Visibility.Visible;
                //    txt_updateButton.Visibility = Visibility.Visible;
                //    txt_add_Icon.Visibility = Visibility.Visible;
                //    txt_update_Icon.Visibility = Visibility.Visible;
                //    txt_delete_Icon.Visibility = Visibility.Visible;
                //}
                //else if (System.Windows.SystemParameters.PrimaryScreenWidth >= 1360)
                //{
                //    txt_add_Icon.Visibility = Visibility.Collapsed;
                //    txt_update_Icon.Visibility = Visibility.Collapsed;
                //    txt_delete_Icon.Visibility = Visibility.Collapsed;
                //    txt_deleteButton.Visibility = Visibility.Visible;
                //    txt_addButton.Visibility = Visibility.Visible;
                //    txt_updateButton.Visibility = Visibility.Visible;

                //}
                //else
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

          
            //string basicsPermission = "printer_basics";
            //string itemsPermission = "printer_items";
            PosPrinters posPrinters = new PosPrinters();
            //ItemsPrinters itemsPrinter = new ItemsPrinters();
            IEnumerable<PosPrinters> posPrinterssQuery;
            IEnumerable<PosPrinters> posPrinterss;
            byte tgl_posPrintersState;
            string searchText = "";
            public static List<string> requiredControlList;

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                SectionData.StartAwait(grid_main);

                requiredControlList = new List<string> { "name", "printerName", "sizeId", "copiesCount" };

                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    grid_main.FlowDirection = FlowDirection.RightToLeft;
                }
                translate();
                #endregion


                //btn_items.IsEnabled = false;
                btn_invoiceTypes.IsEnabled = false;
                #region
                cb_printerName.ItemsSource = SectionData.getsystemPrinters();
                #endregion
                Keyboard.Focus(tb_name);

                await FillCombo.FillComboSalePapersizes(cb_sizeId);
                await Search();
                Clear();
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

            private void translate()
            {
            //// Title
            //if (!string.IsNullOrWhiteSpace(FillCombo.objectsList.Where(x => x.name == this.Tag.ToString()).FirstOrDefault().translate))
            //    txt_title.Text = MainWindow.resourcemanager.GetString(
            //   FillCombo.objectsList.Where(x => x.name == this.Tag.ToString()).FirstOrDefault().translate
            //   );
            //// Icon
            //if (!string.IsNullOrWhiteSpace(FillCombo.objectsList.Where(x => x.name == this.Tag.ToString()).FirstOrDefault().icon))
            //    path_title.Data = App.Current.Resources[
            //  FillCombo.objectsList.Where(x => x.name == this.Tag.ToString()).FirstOrDefault().icon
            //     ] as Geometry;
            txt_title.Text = MainWindow.resourcemanager.GetString("printer");

            txt_addButton.Text = MainWindow.resourcemanager.GetString("trAdd");
                txt_updateButton.Text = MainWindow.resourcemanager.GetString("trUpdate");
                txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
                txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trBaseInformation");
                MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_name, MainWindow.resourcemanager.GetString("trNameHint"));
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_printerName, MainWindow.resourcemanager.GetString("printerName"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_sizeId, MainWindow.resourcemanager.GetString("paperSize"));
                MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_copiesCount, MainWindow.resourcemanager.GetString("copiesCount"));
                MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_notes, MainWindow.resourcemanager.GetString("trNoteHint"));


                dg_posPrinters.Columns[0].Header = MainWindow.resourcemanager.GetString("trName");
                dg_posPrinters.Columns[1].Header = MainWindow.resourcemanager.GetString("printerName");
                dg_posPrinters.Columns[2].Header = MainWindow.resourcemanager.GetString("copiesCount");

                btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");
                //tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
                //tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
                //tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
                //tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
                //tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
                //tt_count.Content = MainWindow.resourcemanager.GetString("trCount");


                tt_add_Button.Content = MainWindow.resourcemanager.GetString("trAdd");
                tt_update_Button.Content = MainWindow.resourcemanager.GetString("trUpdate");
                tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");
                txt_invoiceTypesButton.Text = MainWindow.resourcemanager.GetString("invoiceTypes");
            txt_invoiceTypesTitle.Text = MainWindow.resourcemanager.GetString("invoiceTypes");
            //txt_itemsButton.Text = MainWindow.resourcemanager.GetString("trItems");


            //txt_active.Text = MainWindow.resourcemanager.GetString("trActive_");
        }
        #region Add - Update - Delete - Search - Tgl - Clear - DG_SelectionChanged - refresh
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
            {//add
                try
                {
                    //if (FillCombo.groupObject.HasPermissionAction(basicsPermission, FillCombo.groupObjects, "add"))
                    //{
                        SectionData.StartAwait(grid_main);

                        posPrinters = new PosPrinters();
                        if (SectionData.validate(requiredControlList, this))
                        {

                            await isCodeExist(0);
                            if (isNameExist)
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trDuplicateCodeToolTip"), animation: ToasterAnimation.FadeIn);
                                SectionData.SetValidate( p_error_name, "trDuplicateCodeToolTip");
                            }
                            else if (isPrinterNameExist)
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trDuplicateCodeToolTip"), animation: ToasterAnimation.FadeIn);
                                SectionData.SetValidate(p_error_printerName, "trDuplicateCodeToolTip");
                            }
                            else
                            {
                                posPrinters.name = tb_name.Text;
                                posPrinters.printerName = SectionData.EncodePrinterName(cb_printerName.SelectedValue.ToString());

                        if (cb_sizeId.SelectedValue != null && (int)cb_sizeId.SelectedValue != 0)
                            posPrinters.sizeId = (int)cb_sizeId.SelectedValue;

                        posPrinters.copiesCount = numValue_copiesCount;
                                posPrinters.notes = tb_notes.Text;
                                posPrinters.createUserId = MainWindow.userLogin.userId;
                                posPrinters.updateUserId = MainWindow.userLogin.userId;
                                posPrinters.posId = MainWindow.posLogIn.posId;
                                posPrinters.isActive = 1;

                                var s = await posPrinters.Save(posPrinters);
                                if (s <= 0)
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                                else
                                {
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);



                                    Clear();
                                    await RefreshPosPrinterssList();
                                    await Search();
                            await FillCombo.invoiceTypesPrinters.refreshPrinters();
                        }
                    }
                        }
                        SectionData.EndAwait(grid_main);
                    //}
                    //else
                    //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                }
                catch (Exception ex)
                {

                    SectionData.EndAwait(grid_main);
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
            private async void Btn_update_Click(object sender, RoutedEventArgs e)
            {//update
                try
                {
                    //if (FillCombo.groupObject.HasPermissionAction(basicsPermission, FillCombo.groupObjects, "update"))
                    //{
                        SectionData.StartAwait(grid_main);
                        if (posPrinters.printerId > 0)
                        {
                            if (SectionData.validate(requiredControlList, this) )
                            {
                                await isCodeExist(posPrinters.printerId);
                                if (isNameExist)
                                {
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trDuplicateCodeToolTip"), animation: ToasterAnimation.FadeIn);
                                    SectionData.SetValidate(p_error_name, "trDuplicateCodeToolTip");
                                }
                                else if (isPrinterNameExist)
                                {
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trDuplicateCodeToolTip"), animation: ToasterAnimation.FadeIn);
                                    SectionData.SetValidate(p_error_printerName, "trDuplicateCodeToolTip");
                                }
                                else
                                {

                                    posPrinters.name = tb_name.Text;
                                    posPrinters.printerName = SectionData.EncodePrinterName(cb_printerName.SelectedValue.ToString());

                            if (cb_sizeId.SelectedValue != null && (int)cb_sizeId.SelectedValue != 0)
                                posPrinters.sizeId = (int)cb_sizeId.SelectedValue;

                            posPrinters.copiesCount = numValue_copiesCount;
                                    posPrinters.notes = tb_notes.Text;
                                    posPrinters.updateUserId = MainWindow.userLogin.userId;

                                    var s = await posPrinters.Save(posPrinters);
                                    if (s <= 0)
                                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                                    else
                                    {
                                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                                        await RefreshPosPrinterssList();
                                        await Search();
                                await FillCombo.invoiceTypesPrinters.refreshPrinters();


                            }
                        }
                            }
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectItemFirst"), animation: ToasterAnimation.FadeIn);
                        SectionData.EndAwait(grid_main);
                    //}
                    //else
                    //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                }
                catch (Exception ex)
                {
                    SectionData.EndAwait(grid_main);
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
            private async void Btn_delete_Click(object sender, RoutedEventArgs e)
            {
                try
                {//delete
                    //if (FillCombo.groupObject.HasPermissionAction(basicsPermission, FillCombo.groupObjects, "delete"))
                    //{
                        SectionData.StartAwait(grid_main);
                        if (posPrinters.printerId != 0)
                        {

                            if ((!posPrinters.canDelete) && (posPrinters.isActive == 0))
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
                                if (posPrinters.canDelete)
                                    w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                                if (!posPrinters.canDelete)
                                    w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDeactivate");
                                w.ShowDialog();
                                Window.GetWindow(this).Opacity = 1;
                                #endregion
                                if (w.isOk)
                                {
                                    string popupContent = "";
                                    if (posPrinters.canDelete) popupContent = MainWindow.resourcemanager.GetString("trPopDelete");
                                    if ((!posPrinters.canDelete) && (posPrinters.isActive == 1)) popupContent = MainWindow.resourcemanager.GetString("trPopInActive");

                                    var s = await posPrinters.Delete(posPrinters.printerId, MainWindow.userLogin.userId, posPrinters.canDelete);
                                    if (s < 0)
                                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                                    else
                                    {
                                        posPrinters.printerId = 0;
                                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);

                                        await RefreshPosPrinterssList();
                                        await Search();
                                        Clear();
                            await FillCombo.invoiceTypesPrinters.refreshPrinters();
                            }
                        }
                            }
                        }
                        SectionData.EndAwait(grid_main);
                    //}
                    //else
                    //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                }
                catch (Exception ex)
                {
                    Window.GetWindow(this).Opacity = 1;
                    SectionData.EndAwait(grid_main);
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
            private async Task activate()
            {//activate
                posPrinters.isActive = 1;
                var s = await posPrinters.Save(posPrinters);
                if (s <= 0)
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                else
                {
                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopActive"), animation: ToasterAnimation.FadeIn);
                    await RefreshPosPrinterssList();
                    await Search();
                }
            }
            #endregion
            #region events
            private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
            {
                try
                {
                    SectionData.StartAwait(grid_main);
                    await Search();
                    SectionData.EndAwait(grid_main);
                }
                catch (Exception ex)
                {
                    SectionData.EndAwait(grid_main);
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
            private async void Tgl_isActive_Checked(object sender, RoutedEventArgs e)
            {
                try
                {
                    SectionData.StartAwait(grid_main);

                    if (posPrinterss is null)
                        await RefreshPosPrinterssList();
                    tgl_posPrintersState = 1;
                    await Search();
                    SectionData.EndAwait(grid_main);
                }
                catch (Exception ex)
                {
                    SectionData.EndAwait(grid_main);
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
            private async void Tgl_isActive_Unchecked(object sender, RoutedEventArgs e)
            {
                try
                {
                    SectionData.StartAwait(grid_main);

                    if (posPrinterss is null)
                        await RefreshPosPrinterssList();
                    tgl_posPrintersState = 0;
                    await Search();
                    SectionData.EndAwait(grid_main);
                }
                catch (Exception ex)
                {

                    SectionData.EndAwait(grid_main);
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
            private void Btn_clear_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    SectionData.StartAwait(grid_main);
                    Clear();
                    SectionData.EndAwait(grid_main);
                }
                catch (Exception ex)
                {

                    SectionData.EndAwait(grid_main);
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
            private void Dg_posPrinters_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                try
                {
                    SectionData.StartAwait(grid_main);
                    //selection
                    if (dg_posPrinters.SelectedIndex != -1)
                    {
                        posPrinters = dg_posPrinters.SelectedItem as PosPrinters;
                        tb_copiesCount.Text = posPrinters.copiesCount.ToString();
                        //cb_printerName.SelectedValue = posPrinters.printerName
                        this.DataContext = posPrinters;
                        if (posPrinters != null)
                        {
                            //btn_items.IsEnabled = true;
                        btn_invoiceTypes.IsEnabled = true;
                        buildInvoiceTypesListDesign(posPrinters.invoiceTypesPrintersList);

                        #region delete
                        if (posPrinters.canDelete)
                            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                            else
                            {
                                if (posPrinters.isActive == 0)
                                txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trActive");
                                else
                                txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trInActive");
                            }
                            #endregion


                        }
                    }
                    SectionData.clearValidate(requiredControlList, this);
                    SectionData.EndAwait(grid_main);
                }
                catch (Exception ex)
                {
                    SectionData.EndAwait(grid_main);
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
            private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
            {//refresh
                try
                {
                    SectionData.StartAwait(grid_main);

                    searchText = "";
                    tb_search.Text = "";
                    await RefreshPosPrinterssList();
                    await Search();

                    SectionData.EndAwait(grid_main);
                }
                catch (Exception ex)
                {

                    SectionData.EndAwait(grid_main);
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
            #endregion
            #region Refresh & Search
            async Task Search()
            {
                //search
                if (posPrinterss is null)
                    await RefreshPosPrinterssList();
                searchText = tb_search.Text.ToLower();
                posPrinterssQuery = posPrinterss.Where(s => (s.name.ToLower().Contains(searchText) ||
                s.printerName.ToLower().Contains(searchText) ||
                s.copiesCount.ToString().ToLower().Contains(searchText)
                ));
                RefreshPosPrinterssView();
            }
            async Task<IEnumerable<PosPrinters>> RefreshPosPrinterssList()
            {
                //await FillCombo.RefreshResidentialSectors();
                //residentials = FillCombo.residentialSecsList.ToList();
                //posPrinterss = await posPrinters.GetAll();
                posPrinterss = await posPrinters.GetByPosId(MainWindow.posLogIn.posId);
                return posPrinterss;
            }
            void RefreshPosPrinterssView()
            {
                dg_posPrinters.ItemsSource = posPrinterssQuery;
                //txt_count.Text = posPrinterssQuery.Count().ToString();
            }
            #endregion
            #region validate - clearValidate - textChange - lostFocus - . . . . 
            void Clear()
            {
                this.DataContext = new PosPrinters();
                txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                tb_copiesCount.Text = "1";
                numValue_copiesCount = 1;

            sp_invoiceTypes.Children.Clear();

            //cb_sizeId.ItemsSource = new List<Papersize>();


            // last 
            SectionData.clearValidate(requiredControlList, this);
                //btn_items.IsEnabled = false;
            btn_invoiceTypes.IsEnabled = false;

            }
            string input;
            decimal _decimal = 0;
            private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
            {
                try
                {


                    //only  digits
                    TextBox textBox = sender as TextBox;
                    SectionData.InputJustNumber(ref textBox);
                    if (textBox.Tag.ToString() == "int")
                    {
                        Regex regex = new Regex("[^0-9]");
                        e.Handled = regex.IsMatch(e.Text);
                    }
                    else if (textBox.Tag.ToString() == "decimal")
                    {
                        input = e.Text;
                        e.Handled = !decimal.TryParse(textBox.Text + input, out _decimal);

                    }
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
            private void Code_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
            private void Spaces_PreviewKeyDown(object sender, KeyEventArgs e)
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
            private void ValidateEmpty_TextChange(object sender, TextChangedEventArgs e)
            {
                try
                {
                    SectionData.validate(requiredControlList, this);
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
            private void validateEmpty_LostFocus(object sender, RoutedEventArgs e)
            {
                try
                {
                    SectionData.validate(requiredControlList, this);
                    if (sender.GetType().Name == "TextBox")
                        if ((sender as TextBox).Name == "tb_copiesCount")
                            if ((sender as TextBox).Text == "0" || (sender as TextBox).Text == "00")
                                (sender as TextBox).Text = "1";


                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
            bool isNameExist = false, isPrinterNameExist = false;
            public async Task isCodeExist(long id)
            {
                try
                {
                    await RefreshPosPrinterssList();
                    if (posPrinterss.Any(a => a.name == tb_name.Text && a.printerId != id))
                        isNameExist = true;
                    else
                        isNameExist = false;

                    if (posPrinterss.Any(a => a.printerName == SectionData.EncodePrinterName(cb_printerName.SelectedValue.ToString()) && a.printerId != id))
                        isPrinterNameExist = true;
                    else
                        isPrinterNameExist = false;

                }
                catch { }
            }
            #endregion
        /*
            private async void Btn_items_Click(object sender, RoutedEventArgs e)
            { //extras
                try
                {
                    //SectionData.StartAwait(grid_main);

                    if (posPrinters.printerId > 0)
                    {

                        //if (FillCombo.groupObject.HasPermissionAction(itemsPermission, FillCombo.groupObjects, "one"))
                        //{
                            Window.GetWindow(this).Opacity = 0;
                            wd_itemsUnitList w = new wd_itemsUnitList();
                            w.CallerName = "printer";
                            w.printerId = posPrinters.printerId;
                            w.ShowDialog();
                            if (w.DialogResult == true)
                            {
                                List<long> itemsUnitsIds = new List<long>();
                                foreach (ItemUnit u in w.selectedItemUnits)
                                    itemsUnitsIds.Add(u.itemUnitId);
                                var s = await itemsPrinter.updateListByPrinterId(posPrinters.printerId, itemsUnitsIds, MainWindow.userLogin.userId);
                                if (s > 0)
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            }
                            Window.GetWindow(this).Opacity = 1;
                        //}
                        //else
                        //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                    }
                else
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectItemFirst"), animation: ToasterAnimation.FadeIn);

                //SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
                {
                    Window.GetWindow(this).Opacity = 1;
                    //SectionData.EndAwait(grid_main);
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
        */
        private async void Btn_invoiceTypes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //SectionData.StartAwait(grid_main);

                if (posPrinters.printerId > 0)
                {

                    //if (FillCombo.groupObject.HasPermissionAction(itemsPermission, FillCombo.groupObjects, "one"))
                    //{
                    Window.GetWindow(this).Opacity = 0;
                    wd_invoiceTypesPrintersList w = new wd_invoiceTypesPrintersList();
                    w.printerId = posPrinters.printerId;
                    w.paperSizeValue = posPrinters.sizeValue;
                    w.ShowDialog();
                    if (w.DialogResult == true)
                    {
                        var intList = new List<int>();

                        foreach (var item in w.selectedInvoiceTypes)
                        {
                            intList.Add(item.invoiceTypeId);
                        }
                        decimal s = await FillCombo.invoiceTypesPrinters
                        .updateListByPrinterId(posPrinters.printerId,
                         intList,
                         MainWindow.userLogin.userId);

                        if (s <= 0)
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        else
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                            await FillCombo.invoiceTypesPrinters.refreshPrinters();
                            await RefreshPosPrinterssList();
                            await Search();
                        }
                    }
                    Window.GetWindow(this).Opacity = 1;
                    //}
                    //else
                    //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                }
                else
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectItemFirst"), animation: ToasterAnimation.FadeIn);

                //SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                //SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /*
            #region report
            //report  parameters
            ReportCls reportclass = new ReportCls();
            LocalReport rep = new LocalReport();
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // end report parameters
            public void BuildReport()
            {
                List<ReportParameter> paramarr = new List<ReportParameter>();
                string addpath;
                bool isArabic = ReportCls.checkLang();
                if (isArabic)
                {
                    addpath = @"\Reports\SectionData\posPrintersesStores\Ar\ArPosPrinterses.rdlc";
                }
                else
                {
                    addpath = @"\Reports\SectionData\posPrintersesStores\En\EnPosPrinterses.rdlc";
                }
                string searchval = "";
                string stateval = "";
                //filter   
                stateval = tgl_isActive.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trActive_")
                  : MainWindow.resourcemanagerreport.GetString("trNotActive");
                paramarr.Add(new ReportParameter("stateval", stateval));
                paramarr.Add(new ReportParameter("trActiveState", MainWindow.resourcemanagerreport.GetString("trState")));
                paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
                searchval = tb_search.Text;
                paramarr.Add(new ReportParameter("searchVal", searchval));
                //end filter
                string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
                clsReports.PosPrintersesReport(posPrinterssQuery, rep, reppath, paramarr);
                clsReports.setReportLanguage(paramarr);
                clsReports.Header(paramarr);
                rep.SetParameters(paramarr);
                rep.Refresh();
            }
            private void Btn_pdf_Click(object sender, RoutedEventArgs e)
            {
                //pdf
                try
                {

                    SectionData.StartAwait(grid_main);

                    if (FillCombo.groupObject.HasPermissionAction(basicsPermission, FillCombo.groupObjects, "report"))
                    {
                        #region
                        BuildReport();

                        saveFileDialog.Filter = "PDF|*.pdf;";

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            string filepath = saveFileDialog.FileName;
                            LocalReportExtensions.ExportToPDF(rep, filepath);
                        }
                        #endregion
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

            private void Btn_print_Click(object sender, RoutedEventArgs e)
            {
                try
                {

                    SectionData.StartAwait(grid_main);
                    if (FillCombo.groupObject.HasPermissionAction(basicsPermission, FillCombo.groupObjects, "report"))
                    {

                        #region
                        BuildReport();
                        LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, AppSettings.rep_printer_name, AppSettings.rep_print_count == null ? short.Parse("1") : short.Parse(AppSettings.rep_print_count));
                        #endregion
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

            private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
            {
                try
                {

                    SectionData.StartAwait(grid_main);

                    if (FillCombo.groupObject.HasPermissionAction(basicsPermission, FillCombo.groupObjects, "report"))
                    {
                        #region
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
                        #endregion
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

            private void Btn_preview_Click(object sender, RoutedEventArgs e)
            {
                try
                {

                    SectionData.StartAwait(grid_main);
                    if (FillCombo.groupObject.HasPermissionAction(basicsPermission, FillCombo.groupObjects, "report"))
                    {
                        #region
                        Window.GetWindow(this).Opacity = 0.2;

                        string pdfpath = "";
                        //
                        pdfpath = @"\Thumb\report\temp.pdf";
                        pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                        BuildReport();

                        LocalReportExtensions.ExportToPDF(rep, pdfpath);
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


                    SectionData.EndAwait(grid_main);
                }
                catch (Exception ex)
                {

                    Window.GetWindow(this).Opacity = 1;
                    SectionData.EndAwait(grid_main);
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }

            }

            private void Btn_pieChart_Click(object sender, RoutedEventArgs e)
            { //pie
                try
                {
                    SectionData.StartAwait(grid_main);

                    if (FillCombo.groupObject.HasPermissionAction(basicsPermission, FillCombo.groupObjects, "report"))
                    {
                        #region
                        Window.GetWindow(this).Opacity = 0.2;
                        win_lvc win = new win_lvc(posPrinterssQuery, 2);
                        win.ShowDialog();
                        Window.GetWindow(this).Opacity = 1;
                        #endregion
                    }
                    else
                        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                    SectionData.EndAwait(grid_main);
                }
                catch (Exception ex)
                {
                    Window.GetWindow(this).Opacity = 1;
                    SectionData.EndAwait(grid_main);
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }


            }
            #endregion
        */
        #region NumericCount



        private int _numValue_copiesCount = 1;
            public int numValue_copiesCount
            {
                get
                {
                    if (int.TryParse(tb_copiesCount.Text, out _numValue_copiesCount))
                        _numValue_copiesCount = int.Parse(tb_copiesCount.Text);
                    return _numValue_copiesCount;
                }
                set
                {
                    _numValue_copiesCount = value;
                    tb_copiesCount.Text = value.ToString();
                }
            }



            private void Btn_countDown_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    Button button = sender as Button;
                    if (button.Tag.ToString() == "copiesCount")
                    {
                        if (numValue_copiesCount > 1)
                            numValue_copiesCount--;
                        else
                            numValue_copiesCount = 1;
                    }
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }

            private void Btn_countUp_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    Button button = sender as Button;
                    if (button.Tag.ToString() == "copiesCount")
                        numValue_copiesCount++;
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }
        #endregion


       
        private void btn_Keyboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TabTip.Close())
                {
                #pragma warning disable CS0436 // Type conflicts with imported type
                    TabTip.OpenUndockedAndStartPoolingForClosedEvent();
                #pragma warning restore CS0436 // Type conflicts with imported type
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        void buildInvoiceTypesListDesign(List<InvoiceTypesPrinters> invoiceTypesPrintersList)
        {
            sp_invoiceTypes.Children.Clear();
            foreach (var item in invoiceTypesPrintersList)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = item.invoiceTypeNameTranslate;
                textBlock.FontWeight = FontWeights.SemiBold;
                textBlock.Margin = new Thickness(10, 5, 10, 5);
                textBlock.Foreground = Application.Current.Resources["ThickGrey"] as SolidColorBrush;

                sp_invoiceTypes.Children.Add(textBlock);
            }
        }
    }
}
