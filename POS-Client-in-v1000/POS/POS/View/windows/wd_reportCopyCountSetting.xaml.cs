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
using System.Windows.Shapes;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_reportCopyCountSetting.xaml
    /// </summary>
    public partial class wd_reportCopyCountSetting : Window
    {

        public wd_reportCopyCountSetting()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        BrushConverter bc = new BrushConverter();

        //print

        string sale_copy_count;
        string pur_copy_count;
        string rep_copy_count;
        string directentry_copy_count;
        SetValues setvalueModel = new SetValues();
        List<SetValues> printList = new List<SetValues>();

        public SetValues sale_copy_countrow = new SetValues();
        public SetValues pur_copy_countrow = new SetValues();
        public SetValues rep_copy_countrow = new SetValues();
        public SetValues directentry_copy_countrow = new SetValues();

        async Task refreshWindow()
        {
            printList = await setvalueModel.GetBySetvalNote("print");

            sale_copy_countrow = printList.Where(X => X.name == "sale_copy_count").FirstOrDefault();
            sale_copy_count = sale_copy_countrow.value;
            pur_copy_countrow = printList.Where(X => X.name == "pur_copy_count").FirstOrDefault();
            pur_copy_count = pur_copy_countrow.value;
            rep_copy_countrow = printList.Where(X => X.name == "rep_copy_count").FirstOrDefault();
            rep_copy_count = rep_copy_countrow.value;
            directentry_copy_countrow = printList.Where(X => X.name == "directentry_copy_count").FirstOrDefault();
            directentry_copy_count = directentry_copy_countrow.value;

            tb_purCopyCount.Text = pur_copy_count;

            tb_saleCopyCount.Text = sale_copy_count;
            tb_repPrintCount.Text = rep_copy_count;
            tb_directEntry.Text = directentry_copy_count;
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

                //
                await refreshWindow();


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
            txt_title.Text = winLogIn.resourcemanager.GetString("trCopyCount");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_purCopyCount, MainWindow.resourcemanager.GetString("trPurchasesCopyCount"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_saleCopyCount, MainWindow.resourcemanager.GetString("trSalesCopyCount"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_repPrintCount, MainWindow.resourcemanager.GetString("trReportsCopyCount"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_directEntry, MainWindow.resourcemanager.GetString("trDirectEntry"));

            btn_save.Content = winLogIn.resourcemanager.GetString("trSave");

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


                int msg;
                sale_copy_countrow.value = (string)tb_saleCopyCount.Text;
                pur_copy_countrow.value = (string)tb_purCopyCount.Text;
                rep_copy_countrow.value = (string)tb_repPrintCount.Text;
                directentry_copy_countrow.value = (string)tb_directEntry.Text;
                if (int.Parse(sale_copy_countrow.value) <= 0 || int.Parse(pur_copy_countrow.value) <= 0 || int.Parse(rep_copy_countrow.value) <= 0 || int.Parse(directentry_copy_countrow.value) <= 0)
                {
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trMustBeMoreThanZero"), animation: ToasterAnimation.FadeIn);
                }
                else
                {

                    //msg = (int)await setvalueModel.Save(sale_copy_countrow);
                    //msg = (int)await setvalueModel.Save(pur_copy_countrow);
                    //msg = (int)await setvalueModel.Save(rep_copy_countrow);
                    //msg = (int)await setvalueModel.Save(directentry_copy_countrow);
                    List<SetValues> savelist = new List<SetValues>();
                    savelist.Add(sale_copy_countrow);
                    savelist.Add(pur_copy_countrow);
                    savelist.Add(rep_copy_countrow);
                    savelist.Add(directentry_copy_countrow);
                    msg = (int)await setvalueModel.SaveList(savelist);


                    await refreshWindow();
                    await MainWindow.Getprintparameter();
                    if (msg > 0)
                    {
                        AppSettings.sale_copy_count = sale_copy_countrow.value;
                        AppSettings.pur_copy_count = pur_copy_countrow.value;
                        AppSettings.rep_print_count = rep_copy_countrow.value;
                        AppSettings.directentry_copy_count = directentry_copy_countrow.value;
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        await Task.Delay(1500);
                        this.Close();
                    }
                    else
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
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
                    if ((sender as TextBox).Name == "tb_purCopyCount")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorPurCopyCount, tt_errorPurCopyCount, "trEmptyError");
                    else if ((sender as TextBox).Name == "tb_saleCopyCount")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorSaleCopyCount, tt_errorSaleCopyCount, "trEmptyError");
                    else if ((sender as TextBox).Name == "tb_repPrintCount")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorRepPrintCount, tt_errorRepPrintCount, "trEmptyError");
                 else if ((sender as TextBox).Name == "tb_directEntry")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorDirectEntry, tt_errorDirectEntry, "trEmptyError");
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }


        private void Tb_count_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox textBox = sender as TextBox;
                if (textBox == null)
                {
                    return;
                }



                if (textBox.Name == "tb_purCopyCount" || textBox.Name == "tb_saleCopyCount" || textBox.Name == "tb_repPrintCount"|| textBox.Name == "tb_directEntry")
                    SectionData.InputJustNumber(ref textBox);

                if (textBox.Name == "tb_purCopyCount")
                {
                    if (int.TryParse(textBox.Text, out _numPurCopyCount))
                        numPurCopyCount = int.Parse(textBox.Text);
                }
                else if (textBox.Name == "tb_saleCopyCount")
                {
                    if (int.TryParse(textBox.Text, out _numSaleCopyCount))
                        numSaleCopyCount = int.Parse(textBox.Text);
                }
                else if (textBox.Name == "tb_repPrintCount")
                {
                    if (int.TryParse(textBox.Text, out _numRepPrintCount))
                        numRepPrintCount = int.Parse(textBox.Text);
                }
                else if (textBox.Name == "tb_directEntry")
                {
                    if (int.TryParse(textBox.Text, out _numDirectEntry))
                        numDirectEntry = int.Parse(textBox.Text);
                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_count_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
        #region NumericCount

        private int _numPurCopyCount = 1;
        public int numPurCopyCount
        {
            get { return _numPurCopyCount; }
            set
            {
                _numPurCopyCount = value;
                tb_purCopyCount.Text = value.ToString();
            }
        }


        private int _numSaleCopyCount = 1;
        public int numSaleCopyCount
        {
            get { return _numSaleCopyCount; }
            set
            {
                _numSaleCopyCount = value;
                tb_saleCopyCount.Text = value.ToString();
            }
        }


        private int _numRepPrintCount = 1;
        public int numRepPrintCount
        {
            get { return _numRepPrintCount; }
            set
            {
                _numRepPrintCount = value;
                tb_repPrintCount.Text = value.ToString();
            }
        }

        private int _numDirectEntry = 1;
        public int numDirectEntry
        {
            get { return _numDirectEntry; }
            set
            {
                _numDirectEntry = value;
                tb_directEntry.Text = value.ToString();
            }
        }

        private void Btn_countUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button.Tag.ToString() == "purCopyCount")
                    numPurCopyCount++;
                else if (button.Tag.ToString() == "saleCopyCount")
                    numSaleCopyCount++;
                else if (button.Tag.ToString() == "repPrintCount")
                    numRepPrintCount++;
                else if (button.Tag.ToString() == "directEntry")
                    numDirectEntry++;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_countDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;

                if (button.Tag.ToString() == "purCopyCount" && numPurCopyCount > 1)
                    numPurCopyCount--;
                else if (button.Tag.ToString() == "saleCopyCount" && numSaleCopyCount > 1)
                    numSaleCopyCount--;
                else if (button.Tag.ToString() == "repPrintCount" && numRepPrintCount > 1)
                    numRepPrintCount--;
                else if (button.Tag.ToString() == "directEntry" && numDirectEntry > 1)
                    numDirectEntry--;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
    }
}
