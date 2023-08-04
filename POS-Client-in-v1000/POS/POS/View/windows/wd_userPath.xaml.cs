using netoaster;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_userPath.xaml
    /// </summary>
    public partial class wd_userPath : Window
    {
        public wd_userPath()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            { SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        IEnumerable<Classes.Object> objects = new List<Classes.Object>();
        IEnumerable<Classes.Object> firstLevel;
        IEnumerable<Classes.Object> secondLevel;
        List<Classes.Object> newlist = new List<Classes.Object>();
        List<Classes.Object> newlist2 = new List<Classes.Object>();
        UserSetValues userSetValuesModel = new UserSetValues();
        UserSetValues firstUserSetValue, secondUserSetValue;
        int firstId = 0, secondId = 0;
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

                if (winLogIn.lang.Equals("en"))
                {
                    winLogIn.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    winLogIn.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.RightToLeft;
                }

                translate();
                #endregion

                await RefreshObjects();

                cb_secondLevel.IsEnabled = false;

                fillFirstLevel();

                await getUserPath();

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
        async Task RefreshObjects()
        {
            if (FillCombo.objectsList.Count() == 0)
                await FillCombo.RefreshObjectsList();
            //var objectsLst = await _object.GetAll();
            var objectsLst = FillCombo.objectsList;
            objectsLst = objectsLst.Where(x => x.name != "storageStatistic" && x.name != "usersReports"
            && x.name != "purchaseStatistic" && x.name != "accountsStatistic"
            && x.name != "medals" && x.name != "membership" && x.name != "subscriptions").ToList();
            if (!SectionData.isAdminPermision())
            {
                var list = new List<Classes.Object>();
                foreach (var obj in objectsLst)
                {
                    if (MainWindow.groupObject.HasPermission(obj.name, MainWindow.groupObjects))
                    {
                        list.Add(obj);
                    }
                }
                objects = list;
            }
            else
                objects = objectsLst;

        }

        private async Task getUserPath()
        {
            #region get user path

            if (FillCombo.userSetValuesLst is null)
                await FillCombo.RefreshUserSetValues();

            //SetValues setValueModel = new SetValues();

            if (FillCombo.settingsValues is null)
                await FillCombo.RefreshSettingsValues();
            //List<SetValues> setVLst = await setValueModel.GetBySetName("user_path");
            //List<SetValues> setVLst = FillCombo.settingsValues.Where(s => s.name == "user_path").ToList();
            //firstId = setVLst[0].valId;
            //secondId = setVLst[1].valId;
            firstId = FillCombo.settingsValues.Where(s => s.value == "first").FirstOrDefault().valId;
            secondId = FillCombo.settingsValues.Where(s => s.value == "second").FirstOrDefault().valId;
            try
            {
                firstUserSetValue = FillCombo.userSetValuesLst.Where(u => u.valId == firstId && u.userId == MainWindow.userID).FirstOrDefault();
                secondUserSetValue = FillCombo.userSetValuesLst.Where(u => u.valId == secondId && u.userId == MainWindow.userID).FirstOrDefault();

                foreach (var o in newlist)
                {
                    if (o.name.Equals(SectionData.translate(firstUserSetValue.note)))
                    {
                        cb_firstLevel.SelectedValue = o.objectId;
                        break;
                    }
                }
                foreach (var o in newlist2)
                {
                    if (o.name.Equals(SectionData.translate(secondUserSetValue.note)))
                    {
                        cb_secondLevel.SelectedValue = o.objectId;
                        break;
                    }
                }
            }
            catch
            {
                firstUserSetValue = new UserSetValues();
                secondUserSetValue = new UserSetValues();
                cb_firstLevel.SelectedIndex = -1;
            }

            #endregion

        }

        private void fillFirstLevel()
        {
            #region fill FirstLevel
            firstLevel = objects.Where(x => string.IsNullOrEmpty(x.parentObjectId.ToString()) && x.objectType == "basic");
            newlist = new List<Classes.Object>();

            foreach (var row in firstLevel)
            {
                Classes.Object newrow = new Classes.Object();
                newrow.objectId = row.objectId;
                newrow.name = SectionData.translate(row.name);
                newrow.parentObjectId = row.parentObjectId;
                newlist.Add(newrow);
            }
            cb_firstLevel.DisplayMemberPath = "name";
            cb_firstLevel.SelectedValuePath = "objectId";
            cb_firstLevel.ItemsSource = newlist.OrderBy(x => x.name);

            #endregion


        }
        private void Cb_firstLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox combo = sender as ComboBox;
                secondLevel = objects.Where(x => x.parentObjectId == (int)cb_firstLevel.SelectedValue);

                if (secondLevel.Count() > 0)
                {
                    cb_secondLevel.IsEnabled = true;

                    #region fill secondLevel

                    newlist2 = new List<Classes.Object>();
                    foreach (var row in secondLevel)
                    {
                        Classes.Object newrow = new Classes.Object();
                        newrow.objectId = row.objectId;
                        newrow.name = SectionData.translate(row.name);
                        newrow.parentObjectId = row.parentObjectId;
                        newlist2.Add(newrow);
                    }
                    cb_secondLevel.DisplayMemberPath = "name";
                    cb_secondLevel.SelectedValuePath = "objectId";
                    cb_secondLevel.ItemsSource = newlist2.OrderBy(x => x.name);

                    #endregion
                }
                else
                    cb_secondLevel.IsEnabled = false;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void translate()
        {
            txt_title.Text = winLogIn.resourcemanager.GetString("trUserPath");
            btn_save.Content = winLogIn.resourcemanager.GetString("trSave");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_firstLevel, MainWindow.resourcemanager.GetString("trFirstLevel"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_secondLevel, MainWindow.resourcemanager.GetString("trSecondLevel"));
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    Btn_save_Click(null, null);
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
            if (name == "ComboBox")
            {
                if ((sender as ComboBox).Name == "cb_firstLevel")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorFirstLevel, tt_errorFirstLevel, "trFirstPath");
                else if ((sender as ComboBox).Name == "cb_secondLevel")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorSecondLevel, tt_errorSecondLevel, "trSecondPath");
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

        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region validate
                SectionData.validateEmptyComboBox(cb_firstLevel, p_errorFirstLevel, tt_errorFirstLevel, "trFirstPath");
                SectionData.validateEmptyComboBox(cb_secondLevel, p_errorSecondLevel, tt_errorSecondLevel, "trSecondPath");
                #endregion

                #region save
                if ((!cb_firstLevel.Text.Equals("")) && (!cb_firstLevel.Text.Equals("")))
                {
                    string first = objects.Where(x => x.objectId == (int)cb_firstLevel.SelectedValue).FirstOrDefault().name.ToString();
                    string second = objects.Where(x => x.objectId == (int)cb_secondLevel.SelectedValue).FirstOrDefault().name.ToString();

                    //save first path
                    if (firstUserSetValue.id == 0)
                        firstUserSetValue = new UserSetValues();

                    firstUserSetValue.userId = MainWindow.userID;
                    firstUserSetValue.valId = firstId;
                    firstUserSetValue.note = first;
                    firstUserSetValue.createUserId = MainWindow.userID;
                    firstUserSetValue.updateUserId = MainWindow.userID;
                    int res1 = (int)await userSetValuesModel.Save(firstUserSetValue);

                    //save second path
                    if (secondUserSetValue.id == 0)
                        secondUserSetValue = new UserSetValues();

                    secondUserSetValue.userId = MainWindow.userID;
                    secondUserSetValue.valId = secondId;
                    secondUserSetValue.note = second;
                    secondUserSetValue.createUserId = MainWindow.userID;
                    secondUserSetValue.updateUserId = MainWindow.userID;
                    int res2 = (int)await userSetValuesModel.Save(secondUserSetValue);

                    if ((res1 > 0) && (res2 > 0))
                    {
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        AppSettings.firstPath = first;
                        AppSettings.secondPath = second;
                        AppSettings.firstPathId = res1;
                        AppSettings.secondPathId = res2;
                        await FillCombo.RefreshUserSetValues();
                        await Task.Delay(1000);
                        this.Close();
                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                }
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
            //+ $"Second: {objects.Where(x => x.objectId == (int)cb_secondLevel.SelectedValue).FirstOrDefault().name}");

        }

    }
}
