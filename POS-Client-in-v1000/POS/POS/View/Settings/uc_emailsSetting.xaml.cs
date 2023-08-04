using netoaster;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Resources;
using System.IO;


using Microsoft.Reporting.WinForms;
namespace POS.View.Settings
{
    /// <summary>
    /// Interaction logic for uc_emailsSetting.xaml
    /// </summary>
    public partial class uc_emailsSetting : UserControl
    {
        private static uc_emailsSetting _instance;
        public static uc_emailsSetting Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_emailsSetting();
                return _instance;
            }
        }
        public uc_emailsSetting()
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

        Branch branchModel = new Branch();
        SysEmails sysEmail = new SysEmails();
        BrushConverter bc = new BrushConverter();
        IEnumerable<SysEmails> sysEmailQuery;
        IEnumerable<SysEmails> sysEmails;
        byte tgl_sysEmailState;
        string searchText = "";
        string basicsPermission = "emailsSetting_basics";

        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    if (sysEmails is null)
                        await RefreshSysEmailList();
                    searchText = tb_search.Text.ToLower();
                    sysEmailQuery = sysEmails.Where(s => (s.name.ToLower().Contains(searchText) ||
                    s.email.ToString().ToLower().Contains(searchText)
                    ) && s.isActive == tgl_sysEmailState);
                    RefreshSysEmailView();
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

        async Task<IEnumerable<SysEmails>> RefreshSysEmailList()
        {
             sysEmails = await sysEmail.Get();
             return sysEmails;
        }
        void RefreshSysEmailView()
        {
            dg_sysEmail.ItemsSource = sysEmailQuery;
            txt_count.Text = sysEmailQuery.Count().ToString();
        }
        private async void Tgl_isActive_Checked(object sender, RoutedEventArgs e)
        {//active
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (sysEmails is null)
                    await RefreshSysEmailList();
                tgl_sysEmailState = 1;
                Tb_search_TextChanged(null, null);
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

        private async void Tgl_isActive_Unchecked(object sender, RoutedEventArgs e)
        {//inactive
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (sysEmails is null)
                    await RefreshSysEmailList();
                tgl_sysEmailState = 0;
                Tb_search_TextChanged(null, null);
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

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    await RefreshSysEmailList();
                    Tb_search_TextChanged(null, null);
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
            var QueryExcel = sysEmailQuery.AsEnumerable().Select(x => new
            {
                name = x.name,
                email = x.email,
                port = x.port,
                isSSL = x.isSSL,
                smtpClient = x.smtpClient,
                side = x.side,
                branchName = x.branchName,
                isMajor = x.isMajor,
                notes = x.notes
            });
            var DTForExcel = QueryExcel.ToDataTable();
            DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trName");
            DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("sssssssssssss");
            DTForExcel.Columns[2].Caption = MainWindow.resourcemanager.GetString("sssssssssssssss");
            DTForExcel.Columns[3].Caption = MainWindow.resourcemanager.GetString("sssssssssssssss");
            DTForExcel.Columns[4].Caption = MainWindow.resourcemanager.GetString("sssssssssssssss");
            DTForExcel.Columns[5].Caption = MainWindow.resourcemanager.GetString("sssssssssssssss");
            DTForExcel.Columns[6].Caption = MainWindow.resourcemanager.GetString("sssssssssssssss");
            DTForExcel.Columns[7].Caption = MainWindow.resourcemanager.GetString("sssssssssssssss");
            DTForExcel.Columns[8].Caption = MainWindow.resourcemanager.GetString("trNote");

            ExportToExcel.Export(DTForExcel);
        }

        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                sysEmail = new SysEmails();
                this.DataContext = sysEmail;

                //tb_name.Clear();
                //tb_email.Clear();
                //tb_password.Clear();
                pb_password.Clear();
                //tb_smtpClient.Clear();
                //tb_port.Clear();
                //pb_password.Clear();
                //tgl_isSSL.IsChecked =
                // tgl_isMajor.IsChecked = false;
                cb_branchId.SelectedIndex = -1;
                cb_side.SelectedIndex = -1;
                //tb_notes.Clear();
                SectionData.clearValidate(tb_name, p_errorName);
                SectionData.clearValidate(tb_email, p_errorEmail);
                SectionData.clearValidate(tb_port, p_errorPort);
                SectionData.clearValidate(tb_smtpClient, p_errorSmtpClient);
                SectionData.clearComboBoxValidate(cb_side, p_errorSide);
                SectionData.clearComboBoxValidate(cb_branchId, p_errorBranchId);
                SectionData.clearPasswordValidate(pb_password, p_errorPassword);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void validationControl_LostFocus(object sender, RoutedEventArgs e)
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

        private void validationTextbox_TextChanged(object sender, TextChangedEventArgs e)
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
        private void validateEmpty(string name, object sender)
        {
            try
            {
                if (name == "TextBox")
                {
                    if ((sender as TextBox).Name == "tb_name")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorName, tt_errorName, "trEmptyNameToolTip");
                    else if ((sender as TextBox).Name == "tb_email")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorEmail, tt_errorEmail, "ssssssssssssssssssssss");
                    else if ((sender as TextBox).Name == "tb_port")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorPort, tt_errorPort, "sssssssssssssssssssssssssssss");
                    else if ((sender as TextBox).Name == "tb_smtpClient")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorSmtpClient, tt_errorSmtpClient, "sssssssssssssssssssssssssssss");

                }
                else if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_side")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorSide, tt_errorSide, "sssssssssssssss");
                    else if ((sender as ComboBox).Name == "cb_branchId")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorBranchId, tt_errorBranchId, "sssssssssssssss");
                }
                else if (name == "PasswordBox")
                {
                    if ((sender as PasswordBox).Name == "pb_password")
                        SectionData.showPasswordValidate((PasswordBox)sender, p_errorPassword, tt_errorPassword, "ssssssssssssssssssssssss");
                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        bool isValid()
        {
            if (SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip") == false)
                return false;
            if (SectionData.validateEmptyTextBox(tb_email, p_errorEmail, tt_errorEmail, "trEmptyEmailToolTip") == false)
                return false;
            if (SectionData.validateEmail(tb_email, p_errorEmail, tt_errorEmail) == false)
                return false;
            if (SectionData.validateEmptyTextBox(tb_port, p_errorPort, tt_errorPort, "trEmptyError") == false)
                return false;
            if (SectionData.validateEmptyTextBox(tb_smtpClient, p_errorSmtpClient, tt_errorSmtpClient, "trEmptyError") == false)
                return false;
            if (SectionData.validateEmptyComboBox(cb_side, p_errorSide, tt_errorSide, "trEmptyError") == false)
                return false;
            if (SectionData.validateEmptyComboBox(cb_branchId, p_errorBranchId, tt_errorBranchId, "trEmptyError") == false ||
                cb_branchId.SelectedValue == null)
                return false;
            if (SectionData.validateEmptyPassword(pb_password, p_errorPassword, tt_errorPassword, "trEmptyError") == false)
                return false;
            return true;
        }
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {//add
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    if (isValid())
                    {
                        sysEmail.emailId = 0;
                        sysEmail.name = tb_name.Text;
                        sysEmail.email = tb_email.Text;
                        // sysEmail.password = pb_password.Password;
                        sysEmail.password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(pb_password.Password));

                        sysEmail.port = int.Parse(tb_port.Text);
                        sysEmail.isSSL = tgl_isSSL.IsChecked;
                        sysEmail.isMajor = tgl_isMajor.IsChecked;
                        sysEmail.smtpClient = tb_smtpClient.Text;
                        sysEmail.side = cb_side.SelectedValue.ToString();
                        sysEmail.branchId = (int)cb_branchId.SelectedValue;
                        sysEmail.notes = tb_notes.Text;
                        sysEmail.createUserId = MainWindow.userID;
                        sysEmail.updateUserId = MainWindow.userID;
                        sysEmail.isActive = 1;

                        int s = (int)await sysEmail.Save(sysEmail);
                        if (s.Equals(-4))
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trMajorEmaillAlreadyExists"), animation: ToasterAnimation.FadeIn);
                        else if (!s.Equals(0))
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            Btn_clear_Click(null, null);
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                        await RefreshSysEmailList();
                        Tb_search_TextChanged(null, null);
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
                    if (sysEmail.emailId > 0)
                    {
                        if (isValid())
                    {
                        sysEmail.email = tb_email.Text;
                        //   sysEmail.password = pb_password.Password;
                        sysEmail.password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(pb_password.Password));
                        sysEmail.name = tb_name.Text;
                        sysEmail.port = int.Parse(tb_port.Text);
                        sysEmail.isSSL = tgl_isSSL.IsChecked;
                        sysEmail.isMajor = tgl_isMajor.IsChecked;
                        sysEmail.smtpClient = tb_smtpClient.Text;
                        sysEmail.side = cb_side.SelectedValue.ToString();
                        sysEmail.branchId = (int)cb_branchId.SelectedValue;
                        sysEmail.notes = tb_notes.Text;
                        sysEmail.createUserId = MainWindow.userID;
                        sysEmail.updateUserId = MainWindow.userID;
                        //sysEmail.isActive = 1;
                     //  string s = await sysEmail.Save(sysEmail);
                        int s = (int)await sysEmail.Save(sysEmail);
                        if (s.Equals(-4))
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trMajorEmaillAlreadyExists"), animation: ToasterAnimation.FadeIn);
                        else if (!s.Equals(0))
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                            await RefreshSysEmailList();
                            Tb_search_TextChanged(null, null);
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
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
        private async void Btn_delete_Click(object sender, RoutedEventArgs e)
        {//delete
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                    if (sysEmail.emailId != 0)
                    {
                        if ((!sysEmail.canDelete) && (sysEmail.isActive == 0))
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
                            if (sysEmail.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                            if (!sysEmail.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDeactivate");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion

                            if (w.isOk)
                            {
                                string popupContent = "";
                                if (sysEmail.canDelete) popupContent = MainWindow.resourcemanager.GetString("trPopDelete");
                                if ((!sysEmail.canDelete) && (sysEmail.isActive == 1)) popupContent = MainWindow.resourcemanager.GetString("trPopInActive");

                               int b = (int)await sysEmail.Delete(sysEmail.emailId, MainWindow.userID.Value, sysEmail.canDelete);

                                if (!b.Equals(0))
                                {
                                    sysEmail.emailId = 0;
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);
                                }
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            }
                        }

                        await RefreshSysEmailList();
                        Tb_search_TextChanged(null, null);

                        //clear textBoxs
                        Btn_clear_Click(null, null);
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
        private async Task activate()
        {//activate
            sysEmail.isActive = 1;

          //  string s = await sysEmail.Save(sysEmail);
           int s = (int)await sysEmail.Save(sysEmail);
            if (!s.Equals(0))
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopActive"), animation: ToasterAnimation.FadeIn);
            else
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

            await RefreshSysEmailList();
            Tb_search_TextChanged(null, null);

        }

        private void Dg_sysEmail_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (dg_sysEmail.SelectedIndex != -1)
                {
                    sysEmail = dg_sysEmail.SelectedItem as SysEmails;
                    this.DataContext = sysEmail;

                    cb_branchId.SelectedValue = sysEmail.branchId;
                    cb_side.SelectedValue = sysEmail.side;
                    //   pb_password.Password = sysEmail.password;
                    pb_password.Password = Encoding.UTF8.GetString(Convert.FromBase64String(sysEmail.password));

                    if (sysEmail != null)
                    {

                        #region delete
                        if (sysEmail.canDelete)
                        {
                            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                            txt_delete_Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Delete;
                            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");
                        }

                        else
                        {
                            if (sysEmail.isActive == 0)
                            {
                                txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trActive");
                                txt_delete_Icon.Kind =
                                MaterialDesignThemes.Wpf.PackIconKind.Check;
                                tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trActive");
                            }
                            else
                            {
                                txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trInActive");
                                txt_delete_Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Cancel;
                                tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trInActive");
                            }
                        }
                        #endregion

                    }
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

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);
                if (AppSettings.lang.Equals("en"))
                { MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.LeftToRight; }
                else
                { MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.RightToLeft; }

                translat();

                await fillBranches();
                FillSideCombo();
                await RefreshSysEmailList();
                Tb_search_TextChanged(null, null);
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
        private async Task fillBranches()
        {
            var branches = await branchModel.GetAllWithoutMain("all");
            cb_branchId.ItemsSource = branches;
            cb_branchId.SelectedValuePath = "branchId";
            cb_branchId.DisplayMemberPath = "name";
        }
        void FillSideCombo()
        {
            #region fill deposit to combo
            var list = new[] {
         //   new { Text = MainWindow.resourcemanager.GetString("trMedia")     , Value = "md" },
          //  new { Text = MainWindow.resourcemanager.GetString("trHR")   , Value = "hr" },
          //  new { Text = MainWindow.resourcemanager.GetString("trManager")       , Value = "mg" },
            //new { Text = MainWindow.resourcemanager.GetString("trMarket")     , Value = "mk" },
            //new { Text = MainWindow.resourcemanager.GetString("trSupport")     , Value = "sp" },
            //new { Text = MainWindow.resourcemanager.GetString("trInfo")  , Value = "if" },
  new { Text = MainWindow.resourcemanager.GetString("trAccounting")  , Value = "accounting" },
            new { Text = MainWindow.resourcemanager.GetString("trSales")  , Value = "sales" },
            new { Text = MainWindow.resourcemanager.GetString("trPurchases")  , Value = "purchase" },
              
             };
            cb_side.DisplayMemberPath = "Text";
            cb_side.SelectedValuePath = "Value";
            cb_side.ItemsSource = list;
            #endregion

        }
        private void translat()
        {
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_side, MainWindow.resourcemanager.GetString("trDepartmentHent"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branchId, MainWindow.resourcemanager.GetString("trSelectPosBranchHint"));
            txt_active.Text = MainWindow.resourcemanager.GetString("trActive");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            txt_sysEmail.Text = MainWindow.resourcemanager.GetString("trSysEmails");
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trBaseInformation");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_name, MainWindow.resourcemanager.GetString("trNameHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_notes, MainWindow.resourcemanager.GetString("trNoteHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_port, MainWindow.resourcemanager.GetString("trPort")+"...");
            txt_isMajor.Text = MainWindow.resourcemanager.GetString("trMajor");

            dg_sysEmail.Columns[0].Header = MainWindow.resourcemanager.GetString("trName");
            dg_sysEmail.Columns[1].Header = MainWindow.resourcemanager.GetString("trEmail");
            dg_sysEmail.Columns[2].Header = MainWindow.resourcemanager.GetString("trBranch");
            dg_sysEmail.Columns[3].Header = MainWindow.resourcemanager.GetString("trMajor");

            tt_add_Button.Content = MainWindow.resourcemanager.GetString("trAdd");
            tt_update_Button.Content = MainWindow.resourcemanager.GetString("trUpdate");
            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

            tt_clear.Content = MainWindow.resourcemanager.GetString("trClear");
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            //tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_search.Content = MainWindow.resourcemanager.GetString("trSearch");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(pb_password, MainWindow.resourcemanager.GetString("trPasswordHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_password, MainWindow.resourcemanager.GetString("trPasswordHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_email, MainWindow.resourcemanager.GetString("trEmailHint"));

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

        private void Tb_Numbers_PreviewTextInput(object sender, TextCompositionEventArgs e)
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

        private void P_showPassword_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                tb_password.Text = pb_password.Password;
                tb_password.Visibility = Visibility.Visible;
                pb_password.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void P_showPassword_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                tb_password.Visibility = Visibility.Collapsed;
                pb_password.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Pb_password_PasswordChanged(object sender, RoutedEventArgs e)
        {

            try
            {
                if (pb_password.Password.Equals(""))
                {
                    SectionData.showPasswordValidate(pb_password, p_errorPassword, tt_errorPassword, "trEmptyPasswordToolTip");
                    p_showPassword.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SectionData.clearPasswordValidate(pb_password, p_errorPassword);
                    p_showPassword.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_password_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pb_password.Password.Equals(""))
                {
                    p_errorPassword.Visibility = Visibility.Visible;
                    tt_errorPassword.Content = MainWindow.resourcemanager.GetString("trEmptyPasswordToolTip");
                    pb_password.Background = (Brush)bc.ConvertFrom("#15FF0000");
                    p_showPassword.Visibility = Visibility.Collapsed;
                }
                else
                {
                    pb_password.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                    p_errorPassword.Visibility = Visibility.Collapsed;
                    p_showPassword.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_email_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyTextBox(tb_email, p_errorEmail, tt_errorEmail, "trEmptyEmailToolTip");
                SectionData.validateEmail(tb_email, p_errorEmail, tt_errorEmail);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #region report
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string addpath;
            string searchval = "";
            string stateval = "";
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\Setting\Ar\SysEmail.rdlc";
            }
            else
            {
                addpath = @"\Reports\Setting\En\SysEmail.rdlc";
            }
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
           
            clsReports.SysEmailReport(sysEmailQuery.ToList(), rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            rep.SetParameters(paramarr);
            rep.Refresh();
        }
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
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
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    #region
                    BuildReport();
                    LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
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
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//export
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
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
        #endregion
    }
}
