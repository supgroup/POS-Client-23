using netoaster;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Group = POS.Classes.Group;
using Object = POS.Classes.Object;

namespace POS.View.Settings
{
    /// <summary>
    /// Interaction logic for uc_permissions.xaml
    /// </summary>
    public partial class uc_permissions : UserControl
    {
        private int isCHecked = 1;
        private int index;
        string basicsPermission = "permissions_basics";
        string usersPermission = "Permissions_users";
        private static uc_permissions _instance;
        public static uc_permissions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_permissions();
                return _instance;
            }
        }
        static private object _Sender;
        Group groupModel = new Group();
        Group group = new Group();
        IEnumerable<Group> groupsQuery;
        IEnumerable<Group> groups;
        byte tgl_groupState;
        string searchGroupText = "";
        string _parentObjectName = "categories";
        GroupObject groupObject = new GroupObject();
        IEnumerable<GroupObject> groupObjectsQuery;
        IEnumerable<GroupObject> groupObjects;
        List<GroupObject> groupObjectsList;
        string searchText = "";
        Object objectModel = new Object();
        IEnumerable<Object> objects;
        BrushConverter bc = new BrushConverter();
        public uc_permissions()
        {
            try
            {
                InitializeComponent();
                index = isCHecked;
                if (System.Windows.SystemParameters.PrimaryScreenWidth >= 1440)
                {
                    txt_deleteGroupButton.Visibility = Visibility.Visible;
                    txt_addGroupButton.Visibility = Visibility.Visible;
                    txt_updateGroupButton.Visibility = Visibility.Visible;
                    txt_addGroup_Icon.Visibility = Visibility.Visible;
                    txt_updateGroup_Icon.Visibility = Visibility.Visible;
                    txt_deleteGroup_Icon.Visibility = Visibility.Visible;
                }
                else if (System.Windows.SystemParameters.PrimaryScreenWidth >= 1360)
                {
                    txt_addGroup_Icon.Visibility = Visibility.Collapsed;
                    txt_updateGroup_Icon.Visibility = Visibility.Collapsed;
                    txt_deleteGroup_Icon.Visibility = Visibility.Collapsed;
                    txt_deleteGroupButton.Visibility = Visibility.Visible;
                    txt_addGroupButton.Visibility = Visibility.Visible;
                    txt_updateGroupButton.Visibility = Visibility.Visible;

                }
                else
                {
                    txt_deleteGroupButton.Visibility = Visibility.Collapsed;
                    txt_addGroupButton.Visibility = Visibility.Collapsed;
                    txt_updateGroupButton.Visibility = Visibility.Collapsed;
                    txt_addGroup_Icon.Visibility = Visibility.Visible;
                    txt_updateGroup_Icon.Visibility = Visibility.Visible;
                    txt_deleteGroup_Icon.Visibility = Visibility.Visible;

                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);
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


                Tb_searchGroup_TextChanged(null, null);

               



                if (FillCombo.groupObjectsList is null)
                    await RefreshGroupObjectList();
                else
                    groupObjects = FillCombo.groupObjectsList.ToList();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
                Keyboard.Focus(tb_name);
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
        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                tb_name.Clear();
                tb_notes.Clear();

                p_errorName.Visibility = Visibility.Collapsed;

                tb_name.Background = (Brush)bc.ConvertFrom("#f8f8f8");
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        bool validate(Group group = null)
        {
            //chk empty name
            SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "");

            if ((!tb_name.Text.Equals("")))
                return true;
            else return false;
        }
        private async void Btn_addGroup_Click(object sender, RoutedEventArgs e)
        {//add
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    group.groupId = 0;
                    if (validate(group))
                    {
                        group.name = tb_name.Text;
                        group.notes = tb_notes.Text;
                        group.createUserId = MainWindow.userID;
                        group.updateUserId = MainWindow.userID;
                        group.isActive = 1;
                        int s = (int)await groupModel.Save(group);
                        if (s>0)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            Btn_clear_Click(null, null);

                            //await addObjects(s);
                            group.groupId = s;
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);



                        if (FillCombo.groupsList is null)
                            await RefreshGroupList();
                        else
                            groups = FillCombo.groupsList.ToList();


                        Tb_searchGroup_TextChanged(null, null);
                        await RefreshGroupObjectList();
                        Tb_search_TextChanged(null, null);
                        Btn_refreshGroup_Click(null, null);

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



        #region groupObjects
      

        void RefreshGroupObjectsView()
        {
            dg_permissions.ItemsSource = groupObjectsQuery;
        }
        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (groupObjects is null)
                    await RefreshGroupObjectList();
                searchText = tb_searchGroup.Text;
                groupObjectsQuery = groupObjects.Where(s => s.groupId == group.groupId
                && s.objectType != "basic" && s.parentObjectName == _parentObjectName);
                RefreshGroupObjectsView();
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
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                await RefreshGroupObjectList();
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
        async Task<IEnumerable<GroupObject>> RefreshGroupObjectList()
        {

            await FillCombo.RefreshGroupObjectList();
            groupObjects = FillCombo.groupObjectsList.ToList();
            return groupObjects;
        }


        #endregion

     

        private async void Btn_updateGroup_Click(object sender, RoutedEventArgs e)
        {//update

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update") || SectionData.isAdminPermision())
                {
                    if (validate(group))
                    {
                        group.name = tb_name.Text;
                        group.notes = tb_notes.Text;
                        group.updateUserId = MainWindow.userID;

                        int s = (int)await groupModel.Save(group);

                        if (!s.Equals(0))
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                        await RefreshGroupList();
                        Tb_searchGroup_TextChanged(null, null);


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


        private async void Btn_deleteGroup_Click(object sender, RoutedEventArgs e)
        {//delete
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                    if (group.groupId > 4)
                    {
                        if (group.groupId != 0)
                        {
                            if ((!group.canDelete) && (group.isActive == 0))
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
                                if (group.canDelete)
                                    w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                                if (!group.canDelete)
                                    w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDeactivate");
                                w.ShowDialog();
                                Window.GetWindow(this).Opacity = 1;
                                #endregion
                                if (w.isOk)
                                {
                                    string popupContent = "";
                                    if (group.canDelete) popupContent = MainWindow.resourcemanager.GetString("trPopDelete");
                                    if ((!group.canDelete) && (group.isActive == 1)) popupContent = MainWindow.resourcemanager.GetString("trPopInActive");

                                    int b = (int)await groupModel.Delete(group.groupId, MainWindow.userID.Value, group.canDelete);

                                    if (b > 0)  
                                        Toaster.ShowSuccess(Window.GetWindow(this), message: popupContent, animation: ToasterAnimation.FadeIn);
                                    else  
                                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                                }
                            }
                            await RefreshGroupList();
                            Tb_searchGroup_TextChanged(null, null);
                        }
                    }
                    else
                        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trCannotDeleteTheMainGroup"), animation: ToasterAnimation.FadeIn);

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
            group.isActive = 1;

            int s = (int)await group.Save(group);

            if (!s.Equals(0))
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopActive"), animation: ToasterAnimation.FadeIn);
            else 
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

            await RefreshGroupList();
            Tb_searchGroup_TextChanged(null, null);
        }
        private void Dg_group_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                p_errorName.Visibility = Visibility.Collapsed;
                tb_name.Background = (Brush)bc.ConvertFrom("#f8f8f8");

                if (dg_group.SelectedIndex != -1)
                {
                    group = dg_group.SelectedItem as Group;
                    this.DataContext = group;
                }

                if (group != null)
                {

                    #region delete
                    if (group.canDelete)
                    {
                        txt_deleteGroupButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                        txt_deleteGroup_Icon.Kind =
                                 MaterialDesignThemes.Wpf.PackIconKind.Delete;
                        tt_deleteGroup_Button.Content = MainWindow.resourcemanager.GetString("trDelete");
                    }
                    else
                    {
                        if (group.isActive == 0)
                        {
                            txt_deleteGroupButton.Text = MainWindow.resourcemanager.GetString("trActive");
                            txt_deleteGroup_Icon.Kind =
                             MaterialDesignThemes.Wpf.PackIconKind.Check;
                            tt_deleteGroup_Button.Content = MainWindow.resourcemanager.GetString("trActive");

                        }
                        else
                        {
                            txt_deleteGroupButton.Text = MainWindow.resourcemanager.GetString("trInActive");
                            txt_deleteGroup_Icon.Kind =
                                 MaterialDesignThemes.Wpf.PackIconKind.Cancel;
                            tt_deleteGroup_Button.Content = MainWindow.resourcemanager.GetString("trInActive");

                        }
                    }
                    #endregion
                }
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
        private void validationControl_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as Control).Name == "tb_x")
                    //chk empty name
                    SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");

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
                if ((sender as TextBox).Name == "tb_x")
                    //chk empty x
                    SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");

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
        private async void Tgl_isActive_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleButton toggle = sender as ToggleButton;
                //if (toggle.IsFocused)
                {
                    if (sender != null)
                        SectionData.StartAwait(grid_main);

                    if (FillCombo.groupsList != null)
                        groups = FillCombo.groupsList.ToList();
                    if (groups is null)
                        await RefreshGroupList();
                    tgl_groupState = 1;
                    Tb_searchGroup_TextChanged(null, null);
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
        private async void Tgl_isActive_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleButton toggle = sender as ToggleButton;
                //if (toggle.IsFocused)
                {
                    if (sender != null)
                        SectionData.StartAwait(grid_main);
                    if (groups is null)
                        await RefreshGroupList();
                    tgl_groupState = 0;
                    Tb_searchGroup_TextChanged(null, null);
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
        async Task<IEnumerable<Group>> RefreshGroupList()
        {
            await FillCombo.RefreshGroupList();
            groups = FillCombo.groupsList.ToList();
            return groups;
        }
        void RefreshGroupView()
        {
            dg_group.ItemsSource = groupsQuery;

            //txt_count.Text = groupsQuery.Count().ToString();
        }
        private async void Tb_searchGroup_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {

                    if (groups is null)
                        await RefreshGroupList();
                    searchGroupText = tb_searchGroup.Text.ToLower();
                    groupsQuery = groups.Where(s => (s.name.ToLower().Contains(searchGroupText)) && s.isActive == tgl_groupState);
                    RefreshGroupView();
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

        private async void Btn_refreshGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    await RefreshGroupList();
                    Tb_searchGroup_TextChanged(null, null);

                    await RefreshGroupObjectList();
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
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                this.Dispatcher.Invoke(() =>
                    {
                        Thread t1 = new Thread(FN_ExportToExcel);
                        t1.SetApartmentState(ApartmentState.STA);
                        t1.Start();
                    });
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

            if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
            {
                var QueryExcel = groupsQuery.AsEnumerable().Select(x => new
                {
                    Name = x.name,
                    Notes = x.notes
                });
                var DTForExcel = QueryExcel.ToDataTable();
                DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trName");
                DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trNote");

                ExportToExcel.Export(DTForExcel);
            }
            else
                Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
        }
        private void input_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                if (name == "TextBox")
                {

                }
                else if (name == "ComboBox")
                {


                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _Sender = sender;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_usersList_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(usersPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (group.groupId > 0)
                    {
                        Window.GetWindow(this).Opacity = 0.2;
                        wd_usersList w = new wd_usersList();
                        w.groupId = group.groupId;

                        w.ShowDialog();

                        Window.GetWindow(this).Opacity = 1;
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

        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update") || SectionData.isAdminPermision())
                {
                    int s = 0;
                    foreach (var item in groupObjectsQuery)
                    {
                        s = (int)await groupObject.Save(item);
                    }
                    if (!s.Equals(0))
                    {
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                        Btn_clear_Click(null, null);
                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

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

        private void btn_secondLevelClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Button button = sender as Button;
                paintSecondLevel();
                foreach (Path path in FindControls.FindVisualChildren<Path>(this))
                {
                    // do something with tb here
                    if (path.Name == "path_" + button.Tag)
                    {
                        path.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                        break;
                    }
                }
                foreach (TextBlock textBlock in FindControls.FindVisualChildren<TextBlock>(this))
                {
                    if (textBlock.Name == "txt_" + button.Tag)
                    {
                        textBlock.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                        break;
                    }
                }

                _parentObjectName = button.Tag.ToString();
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

        public void paintSecondLevel()
        {
            paintHome();
            paintCatalog();
            paintStore();
            paintPurchase();
            paintSale();
            paintAccounts();
            paintSectionData();
            paintSettings();
            paintAlerts();
            paintReports();
            paintUserSetting();
        }
        public void paintHome()
        {
            path_dashboard.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));

            txt_dashboard.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
        }
        public void paintCatalog()
        {
            path_categories.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_item.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_package.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_service.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_properties.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_units.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_warranty.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_storageCost.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));

            txt_categories.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_item.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_package.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_service.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_properties.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_units.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_warranty.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_storageCost.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
        }
        public void paintStore()
        {
            path_locations.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_section.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_recipthOfInvoice.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_itemsStorage.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_importExport.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_serial.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_itemsDestroy.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_shortage.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_inventory.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            //path_storageStatistic.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));

            txt_locations.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_section.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_reciptOfInvoice.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_itemsStorage.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_importExport.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_serial.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_itemsDestroy.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_shortage.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_inventory.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            //txt_storageStatistic.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
        }
        public void paintPurchase()
        {
            path_payInvoice.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_purchaseOrder.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            //path_purchaseStatistic.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));

            txt_payInvoice.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_purchaseOrder.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            //txt_purchaseStatistic.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
        }
        public void paintSale()
        {
            path_reciptInvoice.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_coupon.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_offer.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_quotation.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_slice.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            //path_medals.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_salesStatistic.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            //path_membership.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_salesOrders.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));

            txt_reciptInvoice.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_coupon.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_offer.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_quotation.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_slice.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            //txt_medals.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_salesStatistic.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            //txt_membership.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_salesOrders.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
        }
        public void paintAccounts()
        {
            path_posAccounting.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_dailyClosing.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_payments.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_received.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_bonds.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_banksAccounting.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            //path_accountsStatistic.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_ordersAccounting.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            //path_subscriptions.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));

            txt_posAccounting.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_dailyClosing.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_payments.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_received.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_bonds.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_banksAccounting.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            //txt_accountsStatistic.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_ordersAccounting.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            //txt_subscriptions.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
        }
        public void paintReports()
        {
            path_storageReports.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_purchaseReports.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_salesReports.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_accountsReports.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_deliveryReports.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_usersReports.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));

            txt_storageReports.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_purchaseReports.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_salesReports.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_accountsReports.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_deliveryReports.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_usersReports.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
        }
        public void paintSectionData()
        {
            path_suppliers.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_customers.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_users.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_branches.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_stores.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_pos.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_banks.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_cards.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_taxes.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_shippingCompany.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_deliveryManagement.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_driversManagement.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));


            txt_suppliers.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_customers.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_users.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_branches.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_stores.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_pos.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_banks.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_cards.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_taxes.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_shippingCompany.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_deliveryManagement.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_driversManagement.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
        }
        public void paintSettings()
        {
            path_permissions.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_reportsSettings.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_general.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_emailsSetting.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_emailTemplates.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));

            txt_permissions.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_reportsSettings.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_general.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_emailsSetting.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_emailTemplates.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
        }

        public void paintAlerts()
        {
            path_storageAlerts.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_saleAlerts.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            path_accountsAlerts.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));

            txt_storageAlerts.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_saleAlerts.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
            txt_accountsAlerts.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
        }
        public void paintUserSetting()
        {
            path_setUserSetting.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));

            txt_setUserSetting.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686d"));
        }
        private void translate()
        {

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_searchGroup, MainWindow.resourcemanager.GetString("trSearchHint"));
            txt_activeSearch.Text = MainWindow.resourcemanager.GetString("trActive");
            btn_refreshGroup.ToolTip = MainWindow.resourcemanager.GetString("trRefresh");
            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");

            txt_addGroupButton.Text = MainWindow.resourcemanager.GetString("trAdd");
            txt_updateGroupButton.Text = MainWindow.resourcemanager.GetString("trUpdate");
            txt_deleteGroupButton.Text = MainWindow.resourcemanager.GetString("trDelete");
            tt_addGroup_Button.Content = MainWindow.resourcemanager.GetString("trAdd");
            tt_updateGroup_Button.Content = MainWindow.resourcemanager.GetString("trUpdate");
            tt_deleteGroup_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_name, MainWindow.resourcemanager.GetString("trName"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_notes, MainWindow.resourcemanager.GetString("trNoteHint"));
            //tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            //tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            //tt_count.Content = MainWindow.resourcemanager.GetString("trCount");

            dg_group.Columns[0].Header = MainWindow.resourcemanager.GetString("trName");
            dg_group.Columns[1].Header = MainWindow.resourcemanager.GetString("trNote");

            txt_usersList.Text = MainWindow.resourcemanager.GetString("trUsers");


            txt_dashboard.Text = MainWindow.resourcemanager.GetString("trDashBoard");
            

            txt_categories.Text = MainWindow.resourcemanager.GetString("trCategories");
            txt_properties.Text = MainWindow.resourcemanager.GetString("trProperties");
            txt_item.Text = MainWindow.resourcemanager.GetString("trItems");
            txt_package.Text = MainWindow.resourcemanager.GetString("trPackage");
            txt_service.Text = MainWindow.resourcemanager.GetString("trService");
            txt_units.Text = MainWindow.resourcemanager.GetString("trUnits");
            txt_warranty.Text = MainWindow.resourcemanager.GetString("warranty");
            txt_storageCost.Text = MainWindow.resourcemanager.GetString("trStorageCost");

            txt_locations.Text = MainWindow.resourcemanager.GetString("trLocation");
            txt_section.Text = MainWindow.resourcemanager.GetString("trSection");
            txt_reciptOfInvoice.Text = MainWindow.resourcemanager.GetString("trInvoice");
            txt_itemsStorage.Text = MainWindow.resourcemanager.GetString("trStorage");
            txt_importExport.Text = MainWindow.resourcemanager.GetString("trMovements");
            txt_serial.Text = MainWindow.resourcemanager.GetString("features");
            txt_itemsDestroy.Text = MainWindow.resourcemanager.GetString("trDestructive");
            txt_shortage.Text = MainWindow.resourcemanager.GetString("trShortage");
            txt_inventory.Text = MainWindow.resourcemanager.GetString("trStocktaking");
            //txt_storageStatistic.Text = MainWindow.resourcemanager.GetString("trStatistic");

            
            txt_posAccounting.Text = MainWindow.resourcemanager.GetString("trPOS");
            txt_dailyClosing.Text = MainWindow.resourcemanager.GetString("trDailyClosing");
            txt_banksAccounting.Text = MainWindow.resourcemanager.GetString("trBanks");
            txt_payments.Text = MainWindow.resourcemanager.GetString("trPayments");
            txt_received.Text = MainWindow.resourcemanager.GetString("trReceived");
            txt_bonds.Text = MainWindow.resourcemanager.GetString("trBonds");
            txt_ordersAccounting.Text = MainWindow.resourcemanager.GetString("trOrders");

            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trInvoice");
            txt_purchaseOrder.Text = MainWindow.resourcemanager.GetString("trOrders");
            //txt_storageStatistic.Text = MainWindow.resourcemanager.GetString("trStatistic");
            //txt_purchaseStatistic.Text = MainWindow.resourcemanager.GetString("trStatistic");
            txt_salesStatistic.Text = MainWindow.resourcemanager.GetString("trDaily");
            //txt_accountsStatistic.Text = MainWindow.resourcemanager.GetString("trStatistic");

            txt_reciptInvoice.Text = MainWindow.resourcemanager.GetString("trInvoice");
            txt_coupon.Text = MainWindow.resourcemanager.GetString("trCoupon");
            txt_offer.Text = MainWindow.resourcemanager.GetString("trOffer");

            txt_quotation.Text = MainWindow.resourcemanager.GetString("trQuotations");
            txt_slice.Text = MainWindow.resourcemanager.GetString("prices");
            txt_salesOrders.Text = MainWindow.resourcemanager.GetString("trOrders");

            txt_customers.Text = MainWindow.resourcemanager.GetString("trCustomers");
            txt_suppliers.Text = MainWindow.resourcemanager.GetString("trSuppliers");
            txt_users.Text = MainWindow.resourcemanager.GetString("trUsers");
            txt_branches.Text = MainWindow.resourcemanager.GetString("trBranches");
            txt_stores.Text = MainWindow.resourcemanager.GetString("trStores");
            txt_pos.Text = MainWindow.resourcemanager.GetString("trPOS");
            txt_banks.Text = MainWindow.resourcemanager.GetString("trBanks");
            txt_cards.Text = MainWindow.resourcemanager.GetString("trPayment1");
            txt_taxes.Text = MainWindow.resourcemanager.GetString("trTax");
            txt_shippingCompany.Text = MainWindow.resourcemanager.GetString("trShipping");
            txt_deliveryManagement.Text = MainWindow.resourcemanager.GetString("management");
            txt_driversManagement.Text = MainWindow.resourcemanager.GetString("deliveryList");

            txt_storageReports.Text = MainWindow.resourcemanager.GetString("trStore");
            txt_purchaseReports.Text = MainWindow.resourcemanager.GetString("trPurchases");
            txt_salesReports.Text = MainWindow.resourcemanager.GetString("trSales");
            txt_accountsReports.Text = MainWindow.resourcemanager.GetString("trAccounting");
            txt_deliveryReports.Text = MainWindow.resourcemanager.GetString("trDelivery");

            txt_storageAlerts.Text = MainWindow.resourcemanager.GetString("trStore");
            txt_saleAlerts.Text = MainWindow.resourcemanager.GetString("trSales");

            txt_general.Text = MainWindow.resourcemanager.GetString("trGeneral");
            txt_reportsSettings.Text = MainWindow.resourcemanager.GetString("trReports");
            txt_permissions.Text = MainWindow.resourcemanager.GetString("trPermission");
            txt_emailsSetting.Text = MainWindow.resourcemanager.GetString("trEmail");
            txt_emailTemplates.Text = MainWindow.resourcemanager.GetString("trEmailTemplates");



            txt_setUserSetting.Text = MainWindow.resourcemanager.GetString("setSettings");

            dg_permissions.Columns[0].Header = MainWindow.resourcemanager.GetString("trPermission");
            dg_permissions.Columns[1].Header = MainWindow.resourcemanager.GetString("trShow");
            dg_permissions.Columns[2].Header = MainWindow.resourcemanager.GetString("trAdd");
            dg_permissions.Columns[3].Header = MainWindow.resourcemanager.GetString("trUpdate");
            dg_permissions.Columns[4].Header = MainWindow.resourcemanager.GetString("trDelete");
            dg_permissions.Columns[5].Header = MainWindow.resourcemanager.GetString("trReports");


            txt_header.Text = MainWindow.resourcemanager.GetString("trPermission");
            txt_groupDetails.Text = MainWindow.resourcemanager.GetString("trDetails");
            txt_groups.Text = MainWindow.resourcemanager.GetString("trGroups");
            btn_save.Content = MainWindow.resourcemanager.GetString("trUpdate");

            btn_userSetting.ToolTip = MainWindow.resourcemanager.GetString("trPermission");
            btn_alerts.ToolTip = MainWindow.resourcemanager.GetString("trAlerts");
            btn_settings.ToolTip = MainWindow.resourcemanager.GetString("trSettings");
            btn_data.ToolTip = MainWindow.resourcemanager.GetString("trSectionData");
            btn_charts.ToolTip = MainWindow.resourcemanager.GetString("trReports");
            btn_account.ToolTip = MainWindow.resourcemanager.GetString("trAccounting");
            btn_delivery.ToolTip = MainWindow.resourcemanager.GetString("trDelivery");
            btn_sale.ToolTip = MainWindow.resourcemanager.GetString("trSales");
            btn_purchase.ToolTip = MainWindow.resourcemanager.GetString("trPurchase");
            btn_store.ToolTip = MainWindow.resourcemanager.GetString("trStore");
            btn_catalog.ToolTip = MainWindow.resourcemanager.GetString("trCatalog");
            btn_home.ToolTip = MainWindow.resourcemanager.GetString("trHome");

        }
        private void isEnabledButtons()
        {
            btn_home.IsEnabled = true;
            btn_catalog.IsEnabled = true;
            btn_store.IsEnabled = true;
            btn_sale.IsEnabled = true;
            btn_delivery.IsEnabled = true;
            btn_purchase.IsEnabled = true;
            btn_data.IsEnabled = true;
            btn_charts.IsEnabled = true;
            btn_settings.IsEnabled = true;
            btn_alerts.IsEnabled = true;
            btn_userSetting.IsEnabled = true;
            btn_account.IsEnabled = true;
        }
        #region Tab
        private void btn_home_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                paint();
                bdr_home.Background = Brushes.White;
                path_home.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_home.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_home.IsEnabled = false;
                btn_home.Opacity = 1;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void btn_catalog_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                paint();
                bdr_catalog.Background = Brushes.White;
                path_catalog.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_catalog.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_catalog.IsEnabled = false;
                btn_catalog.Opacity = 1;

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void btn_store_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                paint();
                bdr_store.Background = Brushes.White;
                path_storage.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_store.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_store.IsEnabled = false;
                btn_store.Opacity = 1;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void btn_sale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                paint();
                bdr_sale.Background = Brushes.White;
                path_sales.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_sales.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_sale.IsEnabled = false;
                btn_sale.Opacity = 1;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void btn_delivery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                paint();
                bdr_delivery.Background = Brushes.White;
                path_delivery.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_delivery.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_delivery.IsEnabled = false;
                btn_delivery.Opacity = 1;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void btn_purchase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                paint();
                bdr_purchase.Background = Brushes.White;
                path_purchases.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_purchase.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_purchase.IsEnabled = false;
                btn_purchase.Opacity = 1;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void btn_account_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                paint();
                bdr_accounts.Background = Brushes.White;
                path_accounting.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_account.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_account.IsEnabled = false;
                btn_account.Opacity = 1;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void btn_charts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                paint();
                bdr_charts.Background = Brushes.White;
                path_reports.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_charts.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_charts.IsEnabled = false;
                btn_charts.Opacity = 1;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void btn_data_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                paint();
                bdr_data.Background = Brushes.White;
                path_sectionData.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_data.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_data.IsEnabled = false;
                btn_data.Opacity = 1;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void btn_settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                paint();
                bdr_settings.Background = Brushes.White;
                path_settings.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_settings.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_settings.IsEnabled = false;
                btn_settings.Opacity = 1;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_alerts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                paint();
                bdr_alerts.Background = Brushes.White;
                path_alerts.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_alerts.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_alerts.IsEnabled = false;
                btn_alerts.Opacity = 1;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_userSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                paint();
                bdr_userSetting.Background = Brushes.White;
                path_userSetting.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_userSetting.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_userSetting.IsEnabled = false;
                btn_userSetting.Opacity = 1;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public void paint()
        {
            bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);

            bdr_accounts.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_catalog.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_charts.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_data.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_home.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_purchase.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_sale.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_delivery.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_settings.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_alerts.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_userSetting.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_store.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_home.Fill = Brushes.White;
            path_catalog.Fill = Brushes.White;
            path_storage.Fill = Brushes.White;
            path_purchases.Fill = Brushes.White;
            path_sales.Fill = Brushes.White;
            path_delivery.Fill = Brushes.White;
            path_accounting.Fill = Brushes.White;
            path_reports.Fill = Brushes.White;
            path_sectionData.Fill = Brushes.White;
            path_settings.Fill = Brushes.White;
            path_alerts.Fill = Brushes.White;
            path_userSetting.Fill = Brushes.White;

            grid_home.Visibility = Visibility.Hidden;
            //grid_bank.Visibility = Visibility.Hidden;
            grid_catalog.Visibility = Visibility.Hidden;
            grid_store.Visibility = Visibility.Hidden;
            grid_purchase.Visibility = Visibility.Hidden;
            grid_sales.Visibility = Visibility.Hidden;
            grid_delivery.Visibility = Visibility.Hidden;
            grid_charts.Visibility = Visibility.Hidden;
            grid_data.Visibility = Visibility.Hidden;
            grid_settings.Visibility = Visibility.Hidden;
            grid_alerts.Visibility = Visibility.Hidden;
            grid_userSetting.Visibility = Visibility.Hidden;
            grid_account.Visibility = Visibility.Hidden;
        }
        #endregion
        private void Grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            //// Have to do this in the unusual case where the border of the cell gets selected.
            //// and causes a crash 'EditItem is not allowed'
            try
            {
                e.Cancel = true;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.S:
                        //handle S key
                        Btn_save_Click(btn_save, null);
                        break;

                }
            }
        }

        
    }
}
