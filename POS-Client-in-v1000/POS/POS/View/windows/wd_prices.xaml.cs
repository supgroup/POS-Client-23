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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_prices.xaml
    /// </summary>
    public partial class wd_prices : Window
    {
        public wd_prices()
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
        public int itemUnitId;
        BrushConverter bc = new BrushConverter();
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

                #region key up
                cb_sliceId.IsTextSearchEnabled = false;
                cb_sliceId.IsEditable = true;
                cb_sliceId.StaysOpenOnEdit = true;
                cb_sliceId.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_sliceId.Text = "";
                #endregion

                await FillCombo.FillComboSlices(cb_sliceId);
                
                if (prices is null)
                    await RefreshPricesList();

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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
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
                if ((sender as TextBox).Name == "tb_taxes" || (sender as TextBox).Name == "tb_price")
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

        Price priceModel = new Price();
        Price price = new Price();
        IEnumerable<Price> pricesQuery;
        IEnumerable<Price> prices;

        bool tgl_priceState = true;
        string searchText = "";
        //string basicsPermission = "price_basics";
       
       
        #region events
       
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
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
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
        private async void Tgl_priceIsActive_Checked(object sender, RoutedEventArgs e)
        {//active
            try
            {
                if(tgl_priceIsActive.IsFocused)
                {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                tgl_priceState = true;
                await Search();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
                }

            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private async void Tgl_priceIsActive_Unchecked(object sender, RoutedEventArgs e)
        {//inactive
            try
            {
                if (tgl_priceIsActive.IsFocused)
                {
                    if (sender != null)
                        SectionData.StartAwait(grid_main);

                    tgl_priceState = false;
                    await Search();

                    if (sender != null)
                        SectionData.EndAwait(grid_main);
                }
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Dg_price_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                SectionData.clearComboBoxValidate(cb_sliceId, p_errorSliceId);
                SectionData.clearValidate(tb_price, p_errorPrice);

                if (dg_price.SelectedIndex != -1)
                {
                    price = dg_price.SelectedItem as Price;
                    this.DataContext = price;
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
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {//add
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    price.priceId = 0;

                    #region validate
                    //chk empty name
                    SectionData.validateEmptyTextBox(tb_price, p_errorPrice, tt_errorPrice, "trIsRequired");
                    SectionData.validateEmptyComboBox(cb_sliceId, p_errorSliceId, tt_errorSliceId, "trIsRequired");
                    #endregion

                    if (!cb_sliceId.Text.Equals("") && !tb_price.Text.Equals(""))
                    {
                        Nullable<int> sliceId = null;
                        if (cb_sliceId.SelectedIndex != -1)
                            sliceId = (int)cb_sliceId.SelectedValue;

                        bool isExist = await chkDuplicate(sliceId);
                        if (isExist)
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSliceExist"), animation: ToasterAnimation.FadeIn);
                            p_errorSliceId.Visibility = Visibility.Visible;
                            tt_errorSliceId.Content = MainWindow.resourcemanager.GetString("trPopSliceExist");
                            cb_sliceId.Background = (Brush)bc.ConvertFrom("#15FF0000");

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

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update") || SectionData.isAdminPermision())
                {
                    if (price.priceId > 0)
                    {
                        #region validate
                        //chk empty name
                        SectionData.validateEmptyTextBox(tb_price, p_errorPrice, tt_errorPrice, "trIsRequired");
                        SectionData.validateEmptyComboBox(cb_sliceId, p_errorSliceId, tt_errorSliceId, "trIsRequired");

                        #endregion

                        if (!cb_sliceId.Text.Equals("") && !tb_price.Text.Equals(""))
                        {

                            Nullable<int> sliceId = null;
                            if (cb_sliceId.SelectedIndex != -1)
                                sliceId = (int)cb_sliceId.SelectedValue;
                            bool isExist = await chkDuplicate(sliceId);
                            if (isExist)
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSliceExist"), animation: ToasterAnimation.FadeIn);
                                p_errorSliceId.Visibility = Visibility.Visible;
                                tt_errorSliceId.Content = MainWindow.resourcemanager.GetString("trPopSliceExist");
                                cb_sliceId.Background = (Brush)bc.ConvertFrom("#15FF0000");

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
        private async Task<bool> chkDuplicate(Nullable<int> sliceId)
        {
            await RefreshPricesList();

            if (prices.Where(x => x.sliceId == sliceId && x.priceId != price.priceId).Count() > 0)
                return true;
            else
                return false;
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

        #region methods
        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("pricing");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trBaseInformation");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_sliceId, MainWindow.resourcemanager.GetString("slice"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_price, MainWindow.resourcemanager.GetString("trPriceHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_notes, MainWindow.resourcemanager.GetString("trNoteHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_sliceId, MainWindow.resourcemanager.GetString("invoiceClass"));

            txt_addButton.Text = MainWindow.resourcemanager.GetString("trAdd");
            txt_updateButton.Text = MainWindow.resourcemanager.GetString("trUpdate");
            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");

            dg_price.Columns[0].Header = MainWindow.resourcemanager.GetString("slice");
            dg_price.Columns[1].Header = MainWindow.resourcemanager.GetString("trPrice");
            dg_price.Columns[2].Header = MainWindow.resourcemanager.GetString("trNote");

            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");

            tt_add_Button.Content = MainWindow.resourcemanager.GetString("trAdd");
            tt_update_Button.Content = MainWindow.resourcemanager.GetString("trUpdate");
            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

            tt_clear.Content = MainWindow.resourcemanager.GetString("trClear");
        }
        async Task Search()
        {
            if (prices is null)
                await RefreshPricesList();

            searchText = tb_search.Text.ToLower();

            pricesQuery = prices.Where(s => (
            s.sliceName.ToLower().Contains(searchText)
            )
            && s.isActive == tgl_priceState);
            RefreshPriceView();
        }
        async Task<IEnumerable<Price>> RefreshPricesList()
        {
            prices = await priceModel.getByitemUnitId(itemUnitId);
            return prices;
        }
        void RefreshPriceView()
        {

            dg_price.ItemsSource = pricesQuery;

        }
        private void Clear()
        {
            this.DataContext = new Price();
            

            SectionData.clearComboBoxValidate(cb_sliceId, p_errorSliceId);
            SectionData.clearValidate(tb_price, p_errorPrice);
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
                    if ((sender as ComboBox).Name == "cb_sliceId")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorSliceId, tt_errorSliceId, "trIsRequired");
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




        #endregion

        private void Cb_sliceId_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                List<Slice> sLst = new List<Slice>();
                sLst = FillCombo.slicesList.Where(s => s.isActive == true).ToList();
                combo.ItemsSource = sLst.ToList().Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
