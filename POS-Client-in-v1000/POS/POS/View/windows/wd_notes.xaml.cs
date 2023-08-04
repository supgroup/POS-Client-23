using POS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for wd_notes.xaml
    /// </summary>
    public partial class wd_notes : Window
    {
        public bool isOk;
        public string note;
        public int maxLength=500;
        public wd_notes()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            isOk = false;
            this.Close();
        }


        private  void Window_Loaded(object sender, RoutedEventArgs e)
        {//load

            try
            {
                    SectionData.StartAwait(grid_main);
                tb_notes.MaxLength = maxLength;

                #region translate

                if (AppSettings.lang.Equals("en"))
                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                else
                    grid_main.FlowDirection = FlowDirection.RightToLeft;

                translate();
                #endregion
                tb_notes.Text = note;

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

            txt_title.Text = MainWindow.resourcemanager.GetString("trNote_");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_notes, MainWindow.resourcemanager.GetString("trNote_")+"...");
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");

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
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
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



        private  void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.StartAwait(grid_main);

                note = tb_notes.Text;
                isOk = true;
                this.Close();

                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
