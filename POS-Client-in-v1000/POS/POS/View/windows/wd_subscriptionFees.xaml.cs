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
    /// Interaction logic for wd_subscriptionFees.xaml
    /// </summary>
    public partial class wd_subscriptionFees : Window
    {
        public wd_subscriptionFees()
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
        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
                    try
                    {
                        if (e.Key == Key.Return)
            {
                //Btn_confirmation_Click(null, null);
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
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                           SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                        }
                    }

        private void Btn_add_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_update_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Dg_subscriptionFees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                #region translate
                if (AppSettings.lang.Equals("en"))
                { MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly()); grid_subscriptionFees.FlowDirection = FlowDirection.LeftToRight; }
                else
                { MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly()); grid_subscriptionFees.FlowDirection = FlowDirection.RightToLeft; }

                translat();
                #endregion
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void translat()
        {
            txt_Title.Text = MainWindow.resourcemanager.GetString("trSubscriptionFees");

            dg_subscriptionFees.Columns[0].Header = MainWindow.resourcemanager.GetString("trMonthCount");
            dg_subscriptionFees.Columns[1].Header = MainWindow.resourcemanager.GetString("trAmount");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_monthsCount, MainWindow.resourcemanager.GetString("trMonthCountHint"));//
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_amount, MainWindow.resourcemanager.GetString("trAmountHint"));//

            tt_add_Button.Content = MainWindow.resourcemanager.GetString("trAdd");
            tt_update_Button.Content = MainWindow.resourcemanager.GetString("trUpdate");
            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

        }
    }
}
