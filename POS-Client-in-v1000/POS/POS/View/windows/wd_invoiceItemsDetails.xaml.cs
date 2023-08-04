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
    /// Interaction logic for wd_invoiceItemsDetails.xaml
    /// </summary>
    public partial class wd_invoiceItemsDetails : Window
     {
        #region variables
        SetValues setValuesModel = new SetValues();
        SetValues setVCanSkipProperties = new SetValues();
        SetValues setVIcanSkipSerialsNum = new SetValues();
        #endregion

        public wd_invoiceItemsDetails()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            { SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
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

                setVCanSkipProperties = await SectionData.getSetValueBySetName("canSkipProperties");
                setVIcanSkipSerialsNum = await SectionData.getSetValueBySetName("canSkipSerialsNum");

                if (setVCanSkipProperties != null)
                    tgl_skipProperties.IsChecked = Convert.ToBoolean(setVCanSkipProperties.value);
                else
                    tgl_skipProperties.IsChecked = false;
              
                if (setVIcanSkipSerialsNum != null)
                    tgl_skipSerialsNum.IsChecked = Convert.ToBoolean(setVIcanSkipSerialsNum.value);
                else
                    tgl_skipSerialsNum.IsChecked = false;

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


        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("skipFeatures");
            txt_skipProperties.Text = MainWindow.resourcemanager.GetString("skipProperties");
            txt_skipSerialsNum.Text = MainWindow.resourcemanager.GetString("skipSerialsNum");
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
        }

        #region events
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
            { SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
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
        #endregion

        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (setVCanSkipProperties == null)
                    setVCanSkipProperties = new SetValues();
                //save CanSkipProperties
                setVCanSkipProperties.value = tgl_skipProperties.IsChecked.ToString();
                setVCanSkipProperties.isSystem = 1;
                setVCanSkipProperties.settingId = setVCanSkipProperties.settingId;
                int canSkipProperties = (int)await setValuesModel.Save(setVCanSkipProperties);

                if (setVIcanSkipSerialsNum == null)
                    setVIcanSkipSerialsNum = new SetValues();
                //save bool item tax
                setVIcanSkipSerialsNum.value = tgl_skipSerialsNum.IsChecked.ToString();
                setVIcanSkipSerialsNum.isSystem = 1;
                setVIcanSkipSerialsNum.settingId = setVIcanSkipSerialsNum.settingId;
                int canSkipSerialsNum = (int)await setValuesModel.Save(setVIcanSkipSerialsNum);

                if ((canSkipProperties > 0) && (canSkipSerialsNum > 0))
                {
                    await FillCombo.RefreshSettingsValues();
                    //update in main window
                    AppSettings.canSkipProperties = bool.Parse(setVCanSkipProperties.value);
                    AppSettings.canSkipSerialsNum = bool.Parse(setVIcanSkipSerialsNum.value);

                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                    this.Close();
                }
                else
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

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
    }
}
