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
    /// Interaction logic for wd_reportSystmSetting.xaml
    /// </summary>
    public partial class wd_reportSystmSetting : Window
    {
        
        public wd_reportSystmSetting()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        BrushConverter bc = new BrushConverter();
        // r/e
        public string windowType = "";
        // print
        SetValues print_on_save_salerow = new SetValues();
        SetValues print_on_save_purrow = new SetValues();
        SetValues email_on_save_salerow = new SetValues();
        SetValues email_on_save_purrow = new SetValues();
        SetValues show_header_row = new SetValues();
        SetValues setvalueModel = new SetValues();
        SetValues print_on_save_directentryrow = new SetValues();
        string print_on_save_sale;
        string print_on_save_pur;
        string email_on_save_sale;
        string email_on_save_pur;
        string show_header;
        string print_on_save_directentry;
        List<SetValues> printList = new List<SetValues>();

        async Task Getprintparameter()
        {

            printList = await setvalueModel.GetBySetvalNote("print");
      

          

            print_on_save_salerow = printList.Where(X => X.name == "print_on_save_sale").FirstOrDefault();
            print_on_save_sale = print_on_save_salerow.value;

            print_on_save_purrow = printList.Where(X => X.name == "print_on_save_pur").FirstOrDefault();
            print_on_save_pur = print_on_save_purrow.value;

            email_on_save_salerow = printList.Where(X => X.name == "email_on_save_sale").FirstOrDefault();
            email_on_save_sale = email_on_save_salerow.value;

            email_on_save_purrow = printList.Where(X => X.name == "email_on_save_pur").FirstOrDefault();
            email_on_save_pur = email_on_save_purrow.value;


            show_header_row = printList.Where(X => X.name == "show_header").FirstOrDefault();
            show_header = show_header_row.value;

            print_on_save_directentryrow = printList.Where(X => X.name == "print_on_save_directentry").FirstOrDefault();
            print_on_save_directentry = print_on_save_directentryrow.value;

            if (print_on_save_pur == "1")
            {
                tgl_printOnSavePur.IsChecked = true;
            }
            else
            {
                tgl_printOnSavePur.IsChecked = false;
            }
            //
            if (print_on_save_sale == "1")
            {
                tgl_printOnSaveSale.IsChecked = true;
            }
            else
            {
                tgl_printOnSaveSale.IsChecked = false;
            }
            //
            if (email_on_save_pur == "1")
            {
                tgl_emailOnSavePur.IsChecked = true;
            }
            else
            {
                tgl_emailOnSavePur.IsChecked = false;
            }
            //////////////////
            if (email_on_save_sale == "1")
            {
                tgl_emailOnSaveSale.IsChecked = true;
            }
            else
            {
                tgl_emailOnSaveSale.IsChecked = false;
            }

            if (show_header == "1")
            {
                tgl_showHeader.IsChecked = true;
            }
            else
            {
                tgl_showHeader.IsChecked = false;
            }
            if (print_on_save_directentry == "1")
            {
              tgl_directEntry.IsChecked = true;
            }
            else
            {
                tgl_directEntry.IsChecked = false;
            }
            /*
            tgl_printOnSaveSale
            tgl_emailOnSavePur
            tgl_emailOnSaveSale
            */

        }

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
                if(windowType == "r")
                {
                    //txt_showHeader.Visibility =
                    //tgl_showHeader.Visibility =
                     txt_printOnSavePur.Visibility =
                    tgl_printOnSavePur.Visibility =
                     txt_directEntry.Visibility =
                    tgl_directEntry.Visibility =
                    txt_printOnSaveSale.Visibility =
                    tgl_printOnSaveSale.Visibility = Visibility.Visible;

                    txt_showHeader.Visibility =
                    tgl_showHeader.Visibility =
                    txt_emailOnSavePur.Visibility =
                    tgl_emailOnSavePur.Visibility =
                    txt_emailOnSaveSale.Visibility =
                    tgl_emailOnSaveSale.Visibility = Visibility.Collapsed;
                }
                else if (windowType == "e")
                {
                    txt_showHeader.Visibility =
                   tgl_showHeader.Visibility =
                   txt_printOnSavePur.Visibility =
                     tgl_printOnSavePur.Visibility =
                     txt_directEntry.Visibility =
                    tgl_directEntry.Visibility =
                     txt_printOnSaveSale.Visibility =
                     tgl_printOnSaveSale.Visibility = Visibility.Collapsed;

                    txt_emailOnSavePur.Visibility =
                    tgl_emailOnSavePur.Visibility =
                    txt_emailOnSaveSale.Visibility =
                    tgl_emailOnSaveSale.Visibility = Visibility.Visible;

                }
                //code
                await Getprintparameter();


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
            if (windowType == "r")
            {
                txt_title.Text = MainWindow.resourcemanager.GetString("trDirectPrinting");
            }
            else if (windowType == "e")
            {
                txt_title.Text = MainWindow.resourcemanager.GetString("trDirectEmail");
            }
            txt_printOnSavePur.Text = MainWindow.resourcemanager.GetString("trPrintOnSavePurchase");
            txt_emailOnSavePur.Text = MainWindow.resourcemanager.GetString("trEmailOnSavePurchase");
            txt_printOnSaveSale.Text = MainWindow.resourcemanager.GetString("trPrintOnSaveSale");
            txt_emailOnSaveSale.Text = MainWindow.resourcemanager.GetString("trEmailOnSaveSale");
            txt_directEntry.Text= MainWindow.resourcemanager.GetString("trDirectEntry");
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
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
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        List<SettingCls> set = new List<SettingCls>();

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

        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //  string msg = "";
                int msg = 0;
                if ((bool)tgl_printOnSavePur.IsChecked)
                {
                    print_on_save_purrow.value = "1";

                }
                else
                {
                    print_on_save_purrow.value = "0";
                }
                if ((bool)tgl_printOnSaveSale.IsChecked)
                {
                    print_on_save_salerow.value = "1";

                }
                else
                {
                    print_on_save_salerow.value = "0";
                }
                if ((bool)tgl_emailOnSavePur.IsChecked)
                {
                    email_on_save_purrow.value = "1";

                }
                else
                {
                    email_on_save_purrow.value = "0";
                }
                if ((bool)tgl_emailOnSaveSale.IsChecked)
                {
                    email_on_save_salerow.value = "1";

                }
                else
                {
                    email_on_save_salerow.value = "0";
                }
                if ((bool)tgl_showHeader.IsChecked)
                {
                    show_header_row.value = "1";

                }
                else
                {
                    show_header_row.value = "0";
                }
                if ((bool)tgl_directEntry.IsChecked)
                {
                    print_on_save_directentryrow.value = "1";

                }
                else
                {
                    print_on_save_directentryrow.value = "0";
                }
                //   tgl_showHeader
                List<SetValues> savelist = new List<SetValues>();
                savelist.Add(print_on_save_purrow);
                savelist.Add(print_on_save_salerow);
                savelist.Add(email_on_save_purrow);
                savelist.Add(email_on_save_salerow);
                savelist.Add(print_on_save_directentryrow);

                //msg = (int)await setvalueModel.Save(print_on_save_purrow);
                //msg = (int)await setvalueModel.Save(print_on_save_salerow);
                //msg = (int)await setvalueModel.Save(email_on_save_purrow);
                //msg = (int)await setvalueModel.Save(email_on_save_salerow);
                //msg = (int)await setvalueModel.Save(print_on_save_directentryrow);
                
                msg =(int)await setvalueModel.SaveList(savelist);
                //   msg = await setvalueModel.Save(show_header_row);

                await Getprintparameter();
                await MainWindow.Getprintparameter();
                if (msg > 0)
                {
                    AppSettings.print_on_save_pur = print_on_save_purrow.value;
                    AppSettings.print_on_save_sale = print_on_save_salerow.value;
                    AppSettings.email_on_save_pur = email_on_save_purrow.value;
                    AppSettings.email_on_save_sale = email_on_save_salerow.value;
                    AppSettings.print_on_save_directentry = print_on_save_directentryrow.value;

                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                    await Task.Delay(1500);
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
