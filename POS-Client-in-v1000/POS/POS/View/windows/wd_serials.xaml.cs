using netoaster;
using POS.Classes;
using System;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_serials.xaml
    /// </summary>
    public partial class wd_serials : Window
    {
        public wd_serials()
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
        public string activationCode = "";
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
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

                tb_serial.Text = activationCode;

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
            txt_title.Text = MainWindow.resourcemanager.GetString("trSerial");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_serial, MainWindow.resourcemanager.GetString("trSerial") + "...");
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
            tt_close.Content = MainWindow.resourcemanager.GetString("trClose");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {//closing
            
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

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {//unload
            try
            {
                GC.Collect();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {//close
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {

               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        AvtivateServer asModel = new AvtivateServer(); 
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            int res = 0;
            SectionData.validateEmptyTextBox(tb_serial , p_errorSerial , tt_errorSerial , "trEmptySerial");
            if (!tb_serial.Text.Equals(""))
            {
                res = (int)await asModel.updatesalecode(tb_serial.Text);
            }
            if (res > 0)
            {
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                await Task.Delay(2000);
                this.Close();
            }
            else
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

        }
    }
}
