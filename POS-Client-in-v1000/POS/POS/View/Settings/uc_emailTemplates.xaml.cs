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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POS.View.Settings
{
    /// <summary>
    /// Interaction logic for uc_emailTemplates.xaml
    /// </summary>
    public partial class uc_emailTemplates : UserControl
    {

        string savePermission = "emailTemplates_save";
        private static uc_emailTemplates _instance;
        public static uc_emailTemplates Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_emailTemplates();
                return _instance;
            }
        }

        public uc_emailTemplates()
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

        SetValues setValuesModel = new SetValues();
        SetValues setValues = new SetValues();
        IEnumerable<SetValues> setValuessQuery;
        IEnumerable<SetValues> setValuess;

        SettingCls setModel = new SettingCls();
        SettingCls sett = new SettingCls();
        IEnumerable<SettingCls> setQuery;
        IEnumerable<SettingCls> setQueryView;

        int tgl_setValuesState;

        string searchText = "";
        private void translate()
        {

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_title, MainWindow.resourcemanager.GetString("trTitle")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_text1, MainWindow.resourcemanager.GetString("trText1")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_text2, MainWindow.resourcemanager.GetString("trText2")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_link1text, MainWindow.resourcemanager.GetString("trLinkText1")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_link1url, MainWindow.resourcemanager.GetString("trUrlLink1")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_link2text, MainWindow.resourcemanager.GetString("trLinkText2") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_link2url, MainWindow.resourcemanager.GetString("trUrlLink1") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_link3text, MainWindow.resourcemanager.GetString("trLinkText3") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_link3url, MainWindow.resourcemanager.GetString("trUrlLink3") + "...");

            btn_refresh.ToolTip = MainWindow.resourcemanager.GetString("trRefresh");
            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");

            tt_search.Content = MainWindow.resourcemanager.GetString("trSearch");

            tt_clear.Content = MainWindow.resourcemanager.GetString("trClear");
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            txt_emailTemplates.Text = MainWindow.resourcemanager.GetString("trEmailTemplates");
            txt_infoTitle.Text = MainWindow.resourcemanager.GetString("trTitle");
            txt_infoBody.Text = MainWindow.resourcemanager.GetString("trBody");
            txt_infoEmailSupport.Text = MainWindow.resourcemanager.GetString("trEmailSupport");

            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");

            dg_setValues.Columns[0].Header = MainWindow.resourcemanager.GetString("trName");

        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

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

                RefreshSettingsList();
               // RefreshSetttingsView();

                SectionData.clearValidate(tb_title, p_errorTitle);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
                Keyboard.Focus(tb_title);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
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



        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//update
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(savePermission, MainWindow.groupObjects, "one"))
                {

                    //write here Mr.Naji
                    List<SetValues> setValuesList = new List<SetValues>();
                    /////
                   int msg = 0;
                    setValues = setValuessQuery.Where(x => x.notes == "title").FirstOrDefault();
                    setValues.value = tb_title.Text;

                  //  msg += (int)await setValuesModel.SaveValueByNotes(setValues);
                    setValuesList.Add(setValues);
                    //

                    setValues = setValuessQuery.Where(x => x.notes == "text1").FirstOrDefault();
                    setValues.value = tb_text1.Text;

                  //  msg += (int)await setValuesModel.SaveValueByNotes(setValues);
                    setValuesList.Add(setValues);
                    setValues = setValuessQuery.Where(x => x.notes == "text2").FirstOrDefault();
                    setValues.value = tb_text2.Text;
                 //   msg += (int)await setValuesModel.SaveValueByNotes(setValues);
                    setValuesList.Add(setValues);
                    setValues = setValuessQuery.Where(x => x.notes == "link1text").FirstOrDefault();
                    setValues.value = tb_link1text.Text;
                   // msg += (int)await setValuesModel.SaveValueByNotes(setValues);
                    setValuesList.Add(setValues);
                    setValues = setValuessQuery.Where(x => x.notes == "link2text").FirstOrDefault();
                    setValues.value = tb_link2text.Text;
                   // msg += (int)await setValuesModel.SaveValueByNotes(setValues);
                    setValuesList.Add(setValues);
                    setValues = setValuessQuery.Where(x => x.notes == "link3text").FirstOrDefault();
                    setValues.value = tb_link3text.Text;
                  //  msg += (int)await setValuesModel.SaveValueByNotes(setValues);
                    setValuesList.Add(setValues);
                    setValues = setValuessQuery.Where(x => x.notes == "link1url").FirstOrDefault();
                    setValues.value = tb_link1url.Text;
                 //   msg += (int)await setValuesModel.SaveValueByNotes(setValues);
                    setValuesList.Add(setValues);
                    setValues = setValuessQuery.Where(x => x.notes == "link2url").FirstOrDefault();
                    setValues.value = tb_link2url.Text;
                  //  msg += (int)await setValuesModel.SaveValueByNotes(setValues);
                    setValuesList.Add(setValues);
                    setValues = setValuessQuery.Where(x => x.notes == "link3url").FirstOrDefault();
                    setValues.value = tb_link3url.Text;
                   // msg += (int)await setValuesModel.SaveValueByNotes(setValues);
                    setValuesList.Add(setValues);
                    msg =(int) await setValuesModel.SaveList(setValuesList);
                    if (msg > 0)
                    {
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        await Task.Delay(1500);
                         
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
        private async void Dg_setValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (dg_setValues.SelectedIndex != -1)
                {
                    if (dg_setValues.SelectedIndex != -1)
                    {
                        sett = dg_setValues.SelectedItem as SettingCls;
                        setValuessQuery = await setValuesModel.GetBySetName(sett.name);

                        //List<SettingCls> settLst = await setModel.GetAll();
                        //SettingCls setting = settLst.Where(s => s.settingId == sett.settingId).FirstOrDefault();
                        //setValuessQuery = await setValuesModel.GetBySetName(setting.name);

                        tb_title.Text = setValuessQuery.Where(x => x.notes == "title").FirstOrDefault() is null ? ""
                        : setValuessQuery.Where(x => x.notes == "title").FirstOrDefault().value.ToString();
                        tb_text1.Text = setValuessQuery.Where(x => x.notes == "text1").FirstOrDefault() is null ? ""
                           : setValuessQuery.Where(x => x.notes == "text1").FirstOrDefault().value.ToString();
                        tb_text2.Text = setValuessQuery.Where(x => x.notes == "text2").FirstOrDefault() is null ? ""
                        : setValuessQuery.Where(x => x.notes == "text2").FirstOrDefault().value.ToString();
                        tb_link1text.Text = setValuessQuery.Where(x => x.notes == "link1text").FirstOrDefault() is null ? ""
                        : setValuessQuery.Where(x => x.notes == "link1text").FirstOrDefault().value.ToString();

                        tb_link2text.Text = setValuessQuery.Where(x => x.notes == "link2text").FirstOrDefault() is null ? ""
                         : setValuessQuery.Where(x => x.notes == "link2text").FirstOrDefault().value.ToString();
                        tb_link3text.Text = setValuessQuery.Where(x => x.notes == "link3text").FirstOrDefault() is null ? ""
                        : setValuessQuery.Where(x => x.notes == "link3text").FirstOrDefault().value.ToString();


                        tb_link1url.Text = setValuessQuery.Where(x => x.notes == "link1url").FirstOrDefault() is null ? ""
                             : setValuessQuery.Where(x => x.notes == "link1url").FirstOrDefault().value.ToString();
                        tb_link2url.Text = setValuessQuery.Where(x => x.notes == "link2url").FirstOrDefault() is null ? ""
                               : setValuessQuery.Where(x => x.notes == "link2url").FirstOrDefault().value.ToString();

                        tb_link3url.Text = setValuessQuery.Where(x => x.notes == "link3url").FirstOrDefault() is null ? ""
                               : setValuessQuery.Where(x => x.notes == "link3url").FirstOrDefault().value.ToString();

                        this.DataContext = setValues;
                    }

                }

                if (setValues != null)
                {
                    //btn_addRange.IsEnabled = true;


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
        private void validationControl_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {    //if ((sender as Control).Name == "tb_x")
                 //    //chk empty name
                 //    SectionData.validateEmptyTextBox(tb_x, p_errorX, tt_errorX, "trEmptyNameToolTip");
                 //else if ((sender as Control).Name == "tb_y")
                 //    //chk empty mobile
                 //    SectionData.validateEmptyTextBox(tb_y, p_errorY, tt_errorY, "trEmptyMobileToolTip");
                 //else if ((sender as Control).Name == "tb_z")
                 //    //chk empty phone
                 //    SectionData.validateEmptyTextBox(tb_z, p_errorZ, tt_errorZ, "trEmptyPhoneToolTip");
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void validationTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {   //if ((sender as TextBox).Name == "tb_x")
                //    //chk empty x
                //    SectionData.validateEmptyTextBox(tb_x, p_errorX, tt_errorX, "trEmptyNameToolTip");
                //else if ((sender as TextBox).Name == "tb_y")
                //    //chk empty y
                //    SectionData.validateEmptyTextBox(tb_y, p_errorY, tt_errorY, "trEmptyMobileToolTip");
                //else if ((sender as TextBox).Name == "tb_z")
                //    //chk empty z
                //    SectionData.validateEmptyTextBox(tb_z, p_errorZ, tt_errorZ, "trEmptyPhoneToolTip");
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        void handleSpace_PreviewKeyDown(object sender, KeyEventArgs e)
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
       
   
        async void RefreshSettingsList()
        {
            setQuery = await setModel.GetByNotes("emailtemp");
            foreach (SettingCls row in setQuery)
            {
                switch (row.name)
                {
                    case "pur_email_temp":
                        row.trName = MainWindow.resourcemanager.GetString("trPurchasesEmailTemplate");
                        break;
                    case "pur_order_email_temp":
                        row.trName = MainWindow.resourcemanager.GetString("trPurchaseOrdersEmailTemplate");
                        break;
                    case "sale_email_temp":
                        row.trName = MainWindow.resourcemanager.GetString("trSalesEmailTemplate");
                        break;
                    case "sale_order_email_temp":
                        row.trName = MainWindow.resourcemanager.GetString("trSalesOrdersEmailTemplate");
                        break;
                    case "quotation_email_temp":
                        row.trName = MainWindow.resourcemanager.GetString("trQuotationsEmailTemplate");
                        break;
                    case "required_email_temp":
                        row.trName = MainWindow.resourcemanager.GetString("trRequirementsEmailTemplate");
                        break;

                }
            }

            dg_setValues.ItemsSource = setQuery;

        }
        //void RefreshSetttingsView()
        //{
        //    setQuery.ToList()[0].name = MainWindow.resourcemanager.GetString("trPurchaseOrdersEmailTemplate");
        //    setQuery.ToList()[1].name = MainWindow.resourcemanager.GetString("trSalesEmailTemplate");
        //    setQuery.ToList()[2].name = MainWindow.resourcemanager.GetString("trSalesOrdersEmailTemplate");
        //    setQuery.ToList()[3].name = MainWindow.resourcemanager.GetString("trQuotationsEmailTemplate");
        //    setQuery.ToList()[4].name = MainWindow.resourcemanager.GetString("trRequirementsEmailTemplate");
        //    setQuery.ToList()[5].name = MainWindow.resourcemanager.GetString("trPurchasesEmailTemplate");
        //    dg_setValues.ItemsSource = setQuery;
        //}


        //private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        //{//search
        //    //try
        //    //{
        //    //    if (sender != null)
        //    //        SectionData.StartAwait(grid_main);

        //        if (setQuery is null)
        //            await RefreshSettingsList();

        //        searchText = tb_search.Text.ToLower();
        //        setQuery = setQuery.Where(s => (s.name.ToLower().Contains(searchText)));
               
        //        RefreshSetttingsView();

        //    //    if (sender != null)
        //    //        SectionData.EndAwait(grid_main);
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    if (sender != null)
        //    //        SectionData.EndAwait(grid_main);
        //    //   SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    //}

        //}

        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

              RefreshSettingsList();
               // RefreshSetttingsView();

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
        {
            try
            { 
                tb_title.Text = 
                tb_text1.Text =
                tb_text2.Text =
                tb_link1text.Text =
                tb_link2text.Text =
                tb_link3text.Text =
                tb_link1url.Text =
                tb_link2url.Text =
                tb_link3url.Text =  "";
                this.DataContext = new SetValues();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
