using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using netoaster;
using POS.Classes;
using POS.controlTemplate;
using POS.View.catalog;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Resources;
using System.Windows.Shapes;

namespace POS.View
{
    /// <summary>
    /// Interaction logic for uc_categorie.xaml
    /// </summary>
    public partial class uc_categorie : UserControl
    {
        Category categoryModel = new Category();
        Category category = new Category();
        int? categoryParentId = 0;
        IEnumerable<Category> categories;
        IEnumerable<Category> categoriesQuery;
        CatigoriesAndItemsView catigoriesAndItemsView = new CatigoriesAndItemsView();
        int parentCategorieSelctedValue = 0;
        public byte tglCategoryState;
        public string txtCategorySearch;
        OpenFileDialog openFileDialog = new OpenFileDialog();
        ImageBrush brush = new ImageBrush();
        BrushConverter bc = new BrushConverter();
        string imgFileName = "pic/no-image-icon-125x125.png";
        bool isImgPressed = false;
        string basicsPermission = "categories_basics";
        private static uc_categorie _instance;
        public static uc_categorie Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_categorie();
                return _instance;
            }
        }
        public uc_categorie()
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
        private async Task fillCategories()
        {
            if (categories is null)
                await RefrishCategories();
            var listCa = categories.ToList();

            var cat = new Category();
            cat.categoryId = 0;
            cat.name = "-";
            listCa.Insert(0, cat);

            cb_parentCategorie.ItemsSource = listCa;
            cb_parentCategorie.SelectedValuePath = "categoryId";
            cb_parentCategorie.DisplayMemberPath = "name";

        }
        private void translate()
        {
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trBaseInformation");
            txt_activeSearch.Text = MainWindow.resourcemanager.GetString("trActive");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_name, MainWindow.resourcemanager.GetString("trNameHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_categoryCode, MainWindow.resourcemanager.GetString("trCodeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_details, MainWindow.resourcemanager.GetString("trDetailsHint"));
            // txt_secondaryInformation.Text = MainWindow.resourcemanager.GetString("trSecondaryInformation");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_parentCategorie, MainWindow.resourcemanager.GetString("trParentCategorieHint"));
            //MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_taxes, MainWindow.resourcemanager.GetString("trTaxHint"));
            // MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_categorie, MainWindow.resourcemanager.GetString("trCategorie"));
            txt_categorie.Text = MainWindow.resourcemanager.GetString("trCategorie");
            //txt_tax.Text = MainWindow.resourcemanager.GetString("trParentTax");
            txt_isTaxExempt.Text = MainWindow.resourcemanager.GetString("taxExempt");

            txt_addButton.Text = MainWindow.resourcemanager.GetString("trAdd");
            txt_updateButton.Text = MainWindow.resourcemanager.GetString("trUpdate");
            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");

            tt_name.Content = MainWindow.resourcemanager.GetString("trName");
            tt_code.Content = MainWindow.resourcemanager.GetString("trCode");
            tt_details.Content = MainWindow.resourcemanager.GetString("trDetails");
            tt_parentCategory.Content = MainWindow.resourcemanager.GetString("trParentCategory");
            tt_search.Content = MainWindow.resourcemanager.GetString("trSearch");
            //tt_taxes.Content = MainWindow.resourcemanager.GetString("trTax");
            tt_grid.Content = MainWindow.resourcemanager.GetString("trViewGrid");
            tt_items.Content = MainWindow.resourcemanager.GetString("trViewItems");

            tt_add_Button.Content = MainWindow.resourcemanager.GetString("trAdd");
            tt_update_Button.Content = MainWindow.resourcemanager.GetString("trUpdate");
            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

            tt_clear.Content = MainWindow.resourcemanager.GetString("trClear");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");

        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

                ///////// on Top Always
                btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                //CreateGridCardContainer();
                catigoriesAndItemsView.ucCategorie = this;
                ////////////////
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


                await fillCategories();
                RefrishCategoriesDatagrid(categories);
                if (AppSettings.itemsTax_bool == false)
                    gd_tax.Visibility = Visibility.Collapsed;
                else
                    gd_tax.Visibility = Visibility.Visible;


                if (sender != null)
                    SectionData.EndAwait(grid_main);
                Keyboard.Focus(tb_categoryCode);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_parentCategorie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                /*

                if (cb_parentCategorie.SelectedIndex > 0)
                {
                    int catId = (int)cb_parentCategorie.SelectedValue;
                    var cat = categories.Where(x => x.categoryId == catId).FirstOrDefault();
                    tb_taxes.Text = SectionData.PercentageDecTostring(cat.taxes);
                    if (cat.fixedTax == 1)
                    {
                        gd_tax.IsEnabled = false;
                        tgl_tax.IsChecked = true;
                    }
                    else
                    {
                        gd_tax.IsEnabled = true;
                        tgl_tax.IsChecked = false;
                    }
                }
                else
                {
                    gd_tax.IsEnabled = true;
                    tgl_tax.IsChecked = false;
                }
                */
                if (cb_parentCategorie.Text.Equals("")) parentCategorieSelctedValue = 0;
                else parentCategorieSelctedValue = Convert.ToInt32(cb_parentCategorie.SelectedValue);
                if (parentCategorieSelctedValue != category.categoryId)
                {
                    cb_parentCategorie.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                    p_errorParentCategory.Visibility = Visibility.Collapsed;

                }
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        #region


        private void Tb_categoryCode_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {

                SectionData.validateEmptyTextBox(tb_categoryCode, p_errorCode, tt_errorCode, "trEmptyCodeToolTip");
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        private void Tb_categoryCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {

                SectionData.validateEmptyTextBox(tb_categoryCode, p_errorCode, tt_errorCode, "trEmptyCodeToolTip");
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_name_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {

                SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");

            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {

                SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
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
        #endregion
        #region Add - Update - Delete _ Clear
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {//add 
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {

                    category = new Category();
                    category.categoryId = 0;
                    //duplicate
                    bool iscodeExist = await SectionData.isCodeExist(tb_categoryCode.Text, "", "Category", 0);
                    //chk empty name
                    SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
                    //chk empty code
                    SectionData.validateEmptyTextBox(tb_categoryCode, p_errorName, tt_errorName, "trEmptyCodeToolTip");
                    //decimal tax;
                    //if (string.IsNullOrEmpty(tb_taxes.Text))
                    //    tax = 0;
                    //else tax = decimal.Parse(tb_taxes.Text);

                    //if (tgl_tax.IsChecked == true)
                    //    category.fixedTax = 1;
                    //else
                    //    category.fixedTax = 0;

                  
                    if (cb_parentCategorie.Text.Equals("")) parentCategorieSelctedValue = 0;
                    else parentCategorieSelctedValue = Convert.ToInt32(cb_parentCategorie.SelectedValue);

                    if ((!tb_name.Text.Equals("")) && (!tb_categoryCode.Text.Equals("")))
                    {
                        if (iscodeExist)
                            SectionData.validateDuplicateCode(tb_categoryCode, p_errorCode, tt_errorCode, "trDuplicateCodeToolTip");
                        else
                        {
                            category.categoryCode = tb_categoryCode.Text;
                            category.name = tb_name.Text;
                            category.details = tb_details.Text;
                            category.isTaxExempt = (bool)tgl_isTaxExempt.IsChecked;
                            //category.taxes = tax;
                            category.parentId = parentCategorieSelctedValue;
                            category.createUserId = MainWindow.userID;
                            category.updateUserId = MainWindow.userID;
                            category.isActive = 1;

                            int s = (int)await categoryModel.save(category);

                            if (s > 0)  //{SectionData.popUpResponse("", MainWindow.resourcemanager.GetString("trPopAdd")); Btn_clear_Click(null, null);  }
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                                if (openFileDialog.FileName != "")
                                {
                                    int categoryId = s;
                                    bool b = await categoryModel.uploadImage(imgFileName, Md5Encription.MD5Hash("Inc-m" + categoryId.ToString()), categoryId);
                                    isImgPressed = false;
                                    if (b)
                                    {
                                        brush.ImageSource = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.Relative));
                                        img_category.Background = brush;
                                    }
                                }
                                Btn_clear_Click(null, null);
                            }
                            else //SectionData.popUpResponse("", MainWindow.resourcemanager.GetString("trPopError"));
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);



                            //if (isImgPressed)
                            //{
                            //    int categoryId =s;
                            //    bool b = await categoryModel.uploadImage(imgFileName, Md5Encription.MD5Hash("Inc-m" + categoryId.ToString()), categoryId);
                            //    isImgPressed = false;
                            //    if (b)
                            //    {
                            //        brush.ImageSource = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.Relative));
                            //        img_category.Background = brush;
                            //    }
                            //    else
                            //    {
                            //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trThereWasAnErrorLoadingTheImage"), animation: ToasterAnimation.FadeIn);
                            //    }

                            //}

                            ///

                            await RefrishCategories();
                            Txb_searchcategories_TextChanged(null, null);
                            await fillCategories();
                            await FillCombo.RefreshCategories();
                        }
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
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update") || SectionData.isAdminPermision())
                {
                    if (category.categoryId > 0)
                    {
                        //duplicate
                        bool iscodeExist = await SectionData.isCodeExist(tb_categoryCode.Text, "", "Category", category.categoryId);
                        //chk empty name
                        SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
                        //chk empty code
                        SectionData.validateEmptyTextBox(tb_categoryCode, p_errorName, tt_errorName, "trEmptyCodeToolTip");
                        //decimal tax;
                        //if (string.IsNullOrEmpty(tb_taxes.Text))
                        //    tax = 0;
                        //else tax = decimal.Parse(tb_taxes.Text);

                        //if (tgl_tax.IsChecked == true)
                        //    category.fixedTax = 1;
                        //else
                        //    category.fixedTax = 0;

                        if (cb_parentCategorie.Text.Equals("")) parentCategorieSelctedValue = 0;
                        else parentCategorieSelctedValue = Convert.ToInt32(cb_parentCategorie.SelectedValue);

                        if ((!tb_name.Text.Equals("")) && (!tb_categoryCode.Text.Equals("")))
                        {
                            if (parentCategorieSelctedValue != category.categoryId)
                            {
                                if (iscodeExist)
                                    SectionData.validateDuplicateCode(tb_categoryCode, p_errorCode, tt_errorCode, "trDuplicateCodeToolTip");
                                else
                                {
                                    category.categoryCode = tb_categoryCode.Text;
                                    category.name = tb_name.Text;
                                    category.details = tb_details.Text;
                                    category.isTaxExempt = (bool)tgl_isTaxExempt.IsChecked;
                                    //category.taxes = tax;
                                    category.parentId = parentCategorieSelctedValue;
                                    category.updateUserId = MainWindow.userID;

                                    int s = (int)await categoryModel.save(category);

                                    if (s > 0) //SectionData.popUpResponse("", MainWindow.resourcemanager.GetString("trPopUpdate")); 
                                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                                    else //SectionData.popUpResponse("", MainWindow.resourcemanager.GetString("trPopError"));
                                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);



                                    if (isImgPressed)
                                    {
                                        int categoryId = s;
                                        bool b = await categoryModel.uploadImage(imgFileName, Md5Encription.MD5Hash("Inc-m" + categoryId.ToString()), categoryId);
                                        isImgPressed = false;
                                        if (b)
                                        {
                                            brush.ImageSource = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.Relative));
                                            img_category.Background = brush;
                                        }
                                        else
                                        {
                                            SectionData.clearImg(img_category);
                                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trThereWasAnErrorLoadingTheImage"), animation: ToasterAnimation.FadeIn);
                                        }
                                    }

                                    await RefrishCategories();

                                    Txb_searchcategories_TextChanged(null, null);
                                    await FillCombo.RefreshCategories();


                                }
                            }
                            else
                            {
                                p_errorParentCategory.Visibility = Visibility.Visible;
                                tt_errorParentCategorie.Content = MainWindow.resourcemanager.GetString("trCategorieParentError");
                                cb_parentCategorie.Background = (Brush)bc.ConvertFrom("#15FF0000");
                            }
                        }

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

        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                category = new Category();
                dg_categories.SelectedIndex = -1;
                openFileDialog.FileName = "";
                tb_name.Clear();
                //tb_taxes.Clear();
                tgl_isTaxExempt.IsChecked = false;
                tb_details.Clear();
                tb_categoryCode.Clear();
                cb_parentCategorie.SelectedIndex = -1;

                //clear img
                Uri resourceUri = new Uri("pic/no-image-icon-125x125.png", UriKind.Relative);
                StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
                BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                brush.ImageSource = temp;
                img_category.Background = brush;

                p_errorName.Visibility = Visibility.Collapsed;
                p_errorCode.Visibility = Visibility.Collapsed;

                tb_name.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_categoryCode.Background = (Brush)bc.ConvertFrom("#f8f8f8");
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
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                    if (category.categoryId != 0)
                    {
                        if ((!category.canDelete) && (category.isActive == 0))
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
                            if (category.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                            if (!category.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDeactivate");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion
                            if (w.isOk)
                            {
                                string popupContent = "";
                                if (category.canDelete) popupContent = MainWindow.resourcemanager.GetString("trPopDelete");
                                if ((!category.canDelete) && (category.isActive == 1)) popupContent = MainWindow.resourcemanager.GetString("trPopInActive");

                                int b = (int)await categoryModel.delete(category.categoryId, MainWindow.userID.Value, category.canDelete);

                                if (b > 0) //SectionData.popUpResponse("", popupContent);
                                {
                                    category.categoryId = 0;
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: popupContent, animation: ToasterAnimation.FadeIn);
                                }
                                else //SectionData.popUpResponse("", MainWindow.resourcemanager.GetString("trPopError"));
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            }
                        }
                        await RefrishCategories();

                        Txb_searchcategories_TextChanged(null, null);
                        await FillCombo.RefreshCategories();

                    }
                    //clear textBoxs
                    Btn_clear_Click(null, null);

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

            category.isActive = 1;

            int s = (int)await categoryModel.save(category);

            if (s > 0) //SectionData.popUpResponse("", MainWindow.resourcemanager.GetString("trPopActive"));
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopActive"), animation: ToasterAnimation.FadeIn);
            else //SectionData.popUpResponse("", MainWindow.resourcemanager.GetString("trPopError"));
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
            await RefrishCategories();
            Txb_searchcategories_TextChanged(null, null);

        }
        #endregion


        #region Categor and Item
        #region Refrish Y
        async Task<IEnumerable<Category>> RefrishCategories()
        {
            categories = await categoryModel.GetAllCategories();
            return categories;
        }


        void RefrishCategoriesDatagrid(IEnumerable<Category> _categories)
        {
            dg_categories.ItemsSource = _categories;
        }

        void RefrishCategoriesCard(IEnumerable<Category> _categories)
        {
            //catigoriesAndItemsView.gridCatigorieItems = grid_itemCard;
            //catigoriesAndItemsView.FN_refrishCatalogItem(_items.ToList(), AppSettings.lang, "sale");

            catigoriesAndItemsView.gridCatigories = grid_categorieContainerCard;
            catigoriesAndItemsView.FN_refrishCatalogCard(_categories.ToList(), 5);
        }
        #endregion
        #region Get Id By Click  Y
        int datagridSelectedItemId;
        private async void dg_categories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (dg_categories.SelectedItem as Category == null || dg_categories.SelectedIndex == -1)
                    return;
                if (datagridSelectedItemId == (dg_categories.SelectedItem as Category).categoryId)
                    return;

                p_errorName.Visibility = Visibility.Collapsed;
                p_errorCode.Visibility = Visibility.Collapsed;
                var bc = new BrushConverter();
                tb_name.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_categoryCode.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tt_errorParentCategorie.Background = (Brush)bc.ConvertFrom("#f8f8f8");

                if (dg_categories.SelectedIndex != -1)
                {
                    category = dg_categories.SelectedItem as Category;
                    datagridSelectedItemId = (dg_categories.SelectedItem as Category).categoryId;
                    this.DataContext = category;
                    cb_parentCategorie.SelectedValue = category.parentId;

                    tgl_isTaxExempt.IsChecked = category.isTaxExempt;

                    //tb_taxes.Text = SectionData.PercentageDecTostring(category.taxes);
                    //if (category.fixedTax == 1)
                    //    tgl_tax.IsChecked = true;
                    //else
                    //    tgl_tax.IsChecked = false;
                    getImg();

                    #region delete
                    if (category.canDelete)
                    {
                        txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                        txt_delete_Icon.Kind =
                                 MaterialDesignThemes.Wpf.PackIconKind.Delete;
                        tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

                    }

                    else
                    {
                        if (category.isActive == 0)
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
                if (categories.Where(x => (x.categoryCode.Contains(txtCategorySearch) ||
                                        x.name.Contains(txtCategorySearch) ||
                                        x.details.Contains(txtCategorySearch)
                                        ) && x.isActive == tglCategoryState && x.parentId == category.categoryId).Count() != 0)
                {
                    categoryParentId = category.categoryId;
                    Txb_searchcategories_TextChanged(null, null);
                }
                //grid_categoryControlPath.Children.Clear();
                await generateTrack(category.categoryId);
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

        public async void ChangeCategorieIdEvent(int categoryId)
        {

            //////////////
            p_errorName.Visibility = Visibility.Collapsed;
            p_errorCode.Visibility = Visibility.Collapsed;
            var bc = new BrushConverter();
            tb_name.Background = (Brush)bc.ConvertFrom("#f8f8f8");
            tb_categoryCode.Background = (Brush)bc.ConvertFrom("#f8f8f8");
            tt_errorParentCategorie.Background = (Brush)bc.ConvertFrom("#f8f8f8");
            //////////////
            //_categorieId = categoryId;

            category = categories.ToList().Find(c => c.categoryId == categoryId);
            this.DataContext = category;
            cb_parentCategorie.SelectedValue = category.parentId;
            tgl_isTaxExempt.IsChecked = category.isTaxExempt;

            //tb_taxes.Text = SectionData.PercentageDecTostring(category.taxes);
            //if (category.fixedTax == 1)
            //    tgl_tax.IsChecked = true;
            //else
            //    tgl_tax.IsChecked = false;
            if (categories.Where(x => (x.categoryCode.Contains(txtCategorySearch) ||
             x.name.Contains(txtCategorySearch) ||
             x.details.Contains(txtCategorySearch)
             ) && x.isActive == tglCategoryState && x.parentId == category.categoryId).Count() != 0)
            {
                categoryParentId = category.categoryId;
                Txb_searchcategories_TextChanged(null, null);
            }

            await generateTrack(category.categoryId);

            //await Img.getImg(category.image , "category");
            getImg();

            #region delete
            if (category.canDelete)
            {
                txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                txt_delete_Icon.Kind =
                         MaterialDesignThemes.Wpf.PackIconKind.Delete;
                tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

            }

            else
            {
                if (category.isActive == 0)
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

        #endregion

        #region Toggle Button Y

        private void Tgl_categoryIsActive_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                tglCategoryState = 1;

                Txb_searchcategories_TextChanged(null, null);
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
        private void Tgl_categorIsActive_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                tglCategoryState = 0;

                Txb_searchcategories_TextChanged(null, null);
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
        #region Switch Card/DataGrid Y

        private void Btn_categoriesInCards_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                grid_categoriesDatagrid.Visibility = Visibility.Collapsed;
                grid_categoryCards.Visibility = Visibility.Visible;
                path_categoriesInCards.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                path_categoriesInGrid.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4e4e4e"));

                tgl_categoryIsActive.IsChecked = (tglCategoryState == 1) ? true : false;
                Txb_searchcategories_TextChanged(null, null);
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

        private void Btn_categoriesInGrid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                grid_categoryCards.Visibility = Visibility.Collapsed;
                grid_categoriesDatagrid.Visibility = Visibility.Visible;
                path_categoriesInGrid.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                path_categoriesInCards.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4e4e4e"));

                tgl_categoryIsActive.IsChecked = (tglCategoryState == 1) ? true : false;
                Txb_searchcategories_TextChanged(null, null);
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
        #region Search Y
        private async void Txb_searchcategories_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    if (categories is null)
                        await RefrishCategories();

                    txtCategorySearch = tb_search.Text.ToLower();

                    categoriesQuery = categories.Where(x => (x.categoryCode.ToLower().Contains(txtCategorySearch) ||
                    x.name.ToLower().Contains(txtCategorySearch) ||
                    x.details.ToLower().Contains(txtCategorySearch)
                    ) && x.isActive == tglCategoryState && x.parentId == categoryParentId);
                    txt_count.Text = categoriesQuery.Count().ToString();


                    RefrishCategoriesDatagrid(categoriesQuery);
                    if (btns is null)
                        btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                    RefrishCategoriesCard(pagination.refrishPagination(categoriesQuery, pageIndex, btns));
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

        public int pageIndex = 1;
        Pagination pagination = new Pagination();
        Button[] btns;
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

                categoriesQuery = categories.Where(x => x.isActive == tglCategoryState);

                if (tb_pageNumberSearch.Text.Equals(""))
                {
                    pageIndex = 1;
                }
                else if (((categoriesQuery.Count() - 1) / 15) + 1 < int.Parse(tb_pageNumberSearch.Text))
                {
                    pageIndex = ((categoriesQuery.Count() - 1) / 15) + 1;
                }
                else
                {
                    pageIndex = int.Parse(tb_pageNumberSearch.Text);
                }

                #region
                categoriesQuery = categories.Where(x => (x.categoryCode.ToLower().Contains(txtCategorySearch) ||
                 x.name.ToLower().Contains(txtCategorySearch) ||
                 x.details.ToLower().Contains(txtCategorySearch)
                 ) && x.isActive == tglCategoryState && x.parentId == categoryParentId);
                txt_count.Text = categoriesQuery.Count().ToString();
                RefrishCategoriesCard(pagination.refrishPagination(categoriesQuery, pageIndex, btns));
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
                categoriesQuery = categories.Where(x => (x.categoryCode.ToLower().Contains(txtCategorySearch) ||
                 x.name.ToLower().Contains(txtCategorySearch) ||
                 x.details.ToLower().Contains(txtCategorySearch)
                 ) && x.isActive == tglCategoryState && x.parentId == categoryParentId);
                txt_count.Text = categoriesQuery.Count().ToString();
                RefrishCategoriesCard(pagination.refrishPagination(categoriesQuery, pageIndex, btns));
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
                categoriesQuery = categories.Where(x => (x.categoryCode.ToLower().Contains(txtCategorySearch) ||
                 x.name.ToLower().Contains(txtCategorySearch) ||
                 x.details.ToLower().Contains(txtCategorySearch)
                 ) && x.isActive == tglCategoryState && x.parentId == categoryParentId);
                txt_count.Text = categoriesQuery.Count().ToString();
                RefrishCategoriesCard(pagination.refrishPagination(categoriesQuery, pageIndex, btns));
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
                categoriesQuery = categories.Where(x => (x.categoryCode.ToLower().Contains(txtCategorySearch) ||
                 x.name.ToLower().Contains(txtCategorySearch) ||
                 x.details.ToLower().Contains(txtCategorySearch)
                 ) && x.isActive == tglCategoryState && x.parentId == categoryParentId);
                txt_count.Text = categoriesQuery.Count().ToString();
                RefrishCategoriesCard(pagination.refrishPagination(categoriesQuery, pageIndex, btns));
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
                categoriesQuery = categories.Where(x => (x.categoryCode.ToLower().Contains(txtCategorySearch) ||
                 x.name.ToLower().Contains(txtCategorySearch) ||
                 x.details.ToLower().Contains(txtCategorySearch)
                 ) && x.isActive == tglCategoryState && x.parentId == categoryParentId);
                txt_count.Text = categoriesQuery.Count().ToString();
                RefrishCategoriesCard(pagination.refrishPagination(categoriesQuery, pageIndex, btns));
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

                categoriesQuery = categories.Where(x => x.isActive == tglCategoryState);
                pageIndex = ((categoriesQuery.Count() - 1) / 15) + 1;
                #region
                categoriesQuery = categories.Where(x => (x.categoryCode.ToLower().Contains(txtCategorySearch) ||
                 x.name.ToLower().Contains(txtCategorySearch) ||
                 x.details.ToLower().Contains(txtCategorySearch)
                 ) && x.isActive == tglCategoryState && x.parentId == categoryParentId);
                txt_count.Text = categoriesQuery.Count().ToString();
                RefrishCategoriesCard(pagination.refrishPagination(categoriesQuery, pageIndex, btns));
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
                        addpath = @"\Reports\Catalog\Ar\ArCategoryReport.rdlc";
                    }
                    else addpath = @"\Reports\Catalog\En\CategoryReport.rdlc";
                    string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

                    ReportCls.checkLang();
                    foreach (var r in categoriesQuery)
                    {
                        r.taxes = decimal.Parse(SectionData.DecTostring(r.taxes));
                    }
                    clsReports.categoryReport(categoriesQuery, rep, reppath, paramarr);
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

            var QueryExcel = categoriesQuery.AsEnumerable().Select(x => new
            {
                Code = x.categoryCode,
                Name = x.name,
                Details = x.details,
                parentId = x.parentId,
                Taxes = x.taxes,

            });
            var DTForExcel = QueryExcel.ToDataTable();
            DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trCodeHint");
            DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trNameHint");
            DTForExcel.Columns[2].Caption = MainWindow.resourcemanager.GetString("trDetailsHint");
            DTForExcel.Columns[3].Caption = MainWindow.resourcemanager.GetString("trParentCategorieHint");
            DTForExcel.Columns[4].Caption = MainWindow.resourcemanager.GetString("trTaxHint");


            ExportToExcel.Export(DTForExcel);

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
                    //if (count + 1 == categoriesPath.Count())
                    //    b.FontFamily = Application.Current.Resources["Font-cairo-bold"] as FontFamily;
                    //else
                    b.FontFamily = Application.Current.Resources["Font-cairo-light"] as FontFamily;
                    b.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#6e6e6e"));
                    //b.FontWeight = FontWeights.Bold;
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
        }
        private async void getCategoryIdFromPath(object sender, RoutedEventArgs e)
        {
            try
            {
                Button b = (Button)sender;

                if (!string.IsNullOrEmpty(b.Tag.ToString()))
                    await generateTrack(int.Parse(b.Tag.ToString()));

                if (categories.Where(x => (x.categoryCode.Contains(txtCategorySearch) ||
                 x.name.Contains(txtCategorySearch) ||
                 x.details.Contains(txtCategorySearch)
                 ) && x.isActive == tglCategoryState && x.parentId == int.Parse(b.Tag.ToString())).Count() != 0)
                {
                    categoryParentId = int.Parse(b.Tag.ToString());
                    Txb_searchcategories_TextChanged(null, null);

                }
                datagridSelectedItemId = 0;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_getAllCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                categoryParentId = 0;
                Txb_searchcategories_TextChanged(null, null);
                grid_categoryControlPath.Children.Clear();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion

        #endregion

        private void Img_calegorieImg_Click(object sender, RoutedEventArgs e)
        {//select image
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                isImgPressed = true;
                openFileDialog.Filter = "Images|*.png;*.jpg;*.bmp;*.jpeg;*.jfif";
                if (openFileDialog.ShowDialog() == true)
                {
                    brush.ImageSource = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.Relative));
                    imgFileName = openFileDialog.FileName;
                    img_category.Background = brush;
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

        private void getImg()
        {
            try
            {


                if (string.IsNullOrEmpty(category.image))
                {
                    SectionData.clearImg(img_category);
                }
                else
                {
                    // byte[] imageBuffer = await categoryModel.downloadImage(category.image); // read this as BLOB from your DB
                    byte[] imageBuffer = SectionData.readLocalImage(category.image, Global.TMPFolder);
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
                        img_category.Background = new ImageBrush(bitmapImage);
                    }
                    else
                        SectionData.clearImg(img_category);

                }
            }
            catch
            {
                SectionData.clearImg(img_category);
            }

        }

        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    await RefrishCategories();
                    Txb_searchcategories_TextChanged(null, null);
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
        private void Tb_categoryCode_PreviewKeyDown(object sender, KeyEventArgs e)
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

        private void Tb_categoryCode_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {//only english and digits
            try
            {
                Regex regex = new Regex("^[a-zA-Z0-9. -_?]*$");
                if (!regex.IsMatch(e.Text))
                    e.Handled = true;

            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_taxes_PreviewKeyDown(object sender, KeyEventArgs e)
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


        private void btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    /////////////////////////////////////
                    Window.GetWindow(this).Opacity = 0.2;
                    win_lvcCatalog win = new win_lvcCatalog(categoriesQuery, 1);
                    win.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                    //////////////////////////////////////
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

        private void Cb_parentCategorie_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                cb_parentCategorie.ItemsSource = categories.Where(x => x.name.Contains(cb_parentCategorie.Text));
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_taxes_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {

                var txb = sender as TextBox;
                if ((sender as TextBox).Name == "tb_taxes")
                    SectionData.InputJustNumber(ref txb);
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
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
                    #region
                    //List<ReportParameter> paramarr = new List<ReportParameter>();

                    //string addpath;
                    //bool isArabic = ReportCls.checkLang();
                    //if (isArabic)
                    //{
                    //    addpath = @"\Reports\Catalog\Ar\ArCategoryReport.rdlc";
                    //}
                    //else addpath = @"\Reports\Catalog\En\CategoryReport.rdlc";
                    //string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

                    //ReportCls.checkLang();

                    //clsReports.categoryReport(categoriesQuery, rep, reppath, paramarr);
                    //clsReports.setReportLanguage(paramarr);
                    //clsReports.Header(paramarr);

                    //rep.SetParameters(paramarr);

                    //rep.Refresh();

                    //saveFileDialog.Filter = "PDF|*.pdf;";

                    //if (saveFileDialog.ShowDialog() == true)
                    //{
                    //    string filepath = saveFileDialog.FileName;
                    //    LocalReportExtensions.ExportToPDF(rep, filepath);
                    //}
                    #endregion
                    Thread t1 = new Thread(() =>
                    {
                        pdfcategory();
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
        public void pdfcategory()
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
        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
        
            string searchval = "";
            string stateval = "";
            //  List<string> invTypelist = new List<string>();
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            string addpath;
         
            if (isArabic)
            {
                addpath = @"\Reports\Catalog\Ar\ArCategoryReport.rdlc";
            }
            else
            {
                addpath = @"\Reports\Catalog\En\CategoryReport.rdlc";
            }
            //filter   
            stateval = tgl_categoryIsActive.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trActive_")
              : MainWindow.resourcemanagerreport.GetString("trNotActive");
            paramarr.Add(new ReportParameter("stateval", stateval));
            paramarr.Add(new ReportParameter("trActiveState", MainWindow.resourcemanagerreport.GetString("trState")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            searchval = tb_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            clsReports.categoryReport(categoriesQuery, rep, reppath, paramarr);
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
                    //Window.GetWindow(this).Opacity = 0.2;
                    //string pdfpath = "";

                    //List<ReportParameter> paramarr = new List<ReportParameter>();

                    ////
                    //pdfpath = @"\Thumb\report\temp.pdf";
                    //pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                    //string addpath;
                    //bool isArabic = ReportCls.checkLang();
                    //if (isArabic)
                    //{
                    //    addpath = @"\Reports\Catalog\Ar\ArCategoryReport.rdlc";
                    //}
                    //else addpath = @"\Reports\Catalog\En\CategoryReport.rdlc";
                    //string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

                    //ReportCls.checkLang();

                    //clsReports.categoryReport(categoriesQuery, rep, reppath, paramarr);
                    //clsReports.setReportLanguage(paramarr);
                    //clsReports.Header(paramarr);

                    //rep.SetParameters(paramarr);

                    //rep.Refresh();

                    //LocalReportExtensions.ExportToPDF(rep, pdfpath);
                    //wd_previewPdf w = new wd_previewPdf();
                    //w.pdfPath = pdfpath;
                    //if (!string.IsNullOrEmpty(w.pdfPath))
                    //{
                    //    w.ShowDialog();
                    //    w.wb_pdfWebViewer.Dispose();
                    //}
                    //Window.GetWindow(this).Opacity = 1;
                    #endregion
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
                    #region
                    //List<ReportParameter> paramarr = new List<ReportParameter>();

                    //string addpath;
                    //bool isArabic = ReportCls.checkLang();
                    //if (isArabic)
                    //{
                    //    addpath = @"\Reports\Catalog\Ar\ArCategoryReport.rdlc";
                    //}
                    //else addpath = @"\Reports\Catalog\En\CategoryReport.rdlc";
                    //string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

                    //ReportCls.checkLang();

                    //clsReports.categoryReport(categoriesQuery, rep, reppath, paramarr);
                    //clsReports.setReportLanguage(paramarr);
                    //clsReports.Header(paramarr);

                    //rep.SetParameters(paramarr);
                    //rep.Refresh();
                    //LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(MainWindow.rep_print_count));
                    #endregion
                    /////////////////////////////////////
                    Thread t1 = new Thread(() =>
                    {
                        printcategory();
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
        public void printcategory()
        {
            BuildReport();

            this.Dispatcher.Invoke(() =>
            {
                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
            });
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GC.Collect();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
