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
    /// Interaction logic for wd_locationAddRange.xaml
    /// </summary>
    public partial class wd_locationAddRange : Window
    {
        public wd_locationAddRange()
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
        public bool isOpend = false;
        List<Location> locations = new List<Location>();
        List<Location> AllLocations = new List<Location>();
        Location location = new Location();
        BrushConverter bc = new BrushConverter();
        Regex regex = new Regex("^[a-zA-Z0-9_]*$");
        Regex regexNumber = new Regex("^[0-9]");
        Regex regexAlpha = new Regex("^[A-Za-z]+$");
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        bool validate(Location location = null)
        {
            bool isValid = true;
            //chk empty x
            SectionData.validateEmptyTextBox(tb_fromX, p_errorFromX, tt_errorFromX, "");
            //chk empty Y
            SectionData.validateEmptyTextBox(tb_fromY, p_errorFromY, tt_errorFromY, "");
            //chk empty Z
            SectionData.validateEmptyTextBox(tb_fromZ, p_errorFromZ, tt_errorFromZ, "");

            //chk empty x
            SectionData.validateEmptyTextBox(tb_toX, p_errorToX, tt_errorToX, "");
            //chk empty Y
            SectionData.validateEmptyTextBox(tb_toY, p_errorToY, tt_errorToY, "");
            //chk empty Z
            SectionData.validateEmptyTextBox(tb_toZ, p_errorToZ, tt_errorToZ, "");

            /////////////////////////////////
            if (regexAlpha.IsMatch(tb_fromX.Text) && regexAlpha.IsMatch(tb_toX.Text) ||
                regexNumber.IsMatch(tb_fromX.Text) && regexNumber.IsMatch(tb_toX.Text))
            {
                if (regexAlpha.IsMatch(tb_fromX.Text))
                {
                    if (char.Parse(tb_fromX.Text.ToString().ToUpper()) > char.Parse(tb_toX.Text.ToString().ToUpper()))
                    {
                        SectionData.SetError(tb_fromX, p_errorFromX, tt_errorFromX, "trValidLocationToBigger");
                        SectionData.SetError(tb_toX, p_errorToX, tt_errorToX, "trValidLocationToBigger");
                        isValid = false;
                    }
                }
                else
                {
                    if (int.Parse(tb_fromX.Text.ToString().ToUpper()) > int.Parse(tb_toX.Text.ToString().ToUpper()))
                    {
                        SectionData.SetError(tb_fromX, p_errorFromX, tt_errorFromX, "trValidLocationToBigger");
                        SectionData.SetError(tb_toX, p_errorToX, tt_errorToX, "trValidLocationToBigger");
                        isValid = false;
                    }
                }
            }
            ///////
            if (regexAlpha.IsMatch(tb_fromY.Text) && regexAlpha.IsMatch(tb_toY.Text) ||
                regexNumber.IsMatch(tb_fromY.Text) && regexNumber.IsMatch(tb_toY.Text))
            {
                if (regexAlpha.IsMatch(tb_fromY.Text))
                {
                    if (char.Parse(tb_fromY.Text.ToString().ToUpper()) > char.Parse(tb_toY.Text.ToString().ToUpper()))
                    {
                        SectionData.SetError(tb_fromY, p_errorFromY, tt_errorFromY, "trValidLocationToBigger");
                        SectionData.SetError(tb_toY, p_errorToY, tt_errorToY, "trValidLocationToBigger");
                        isValid = false;
                    }
                }
                else
                {
                    if (int.Parse(tb_fromY.Text.ToString().ToUpper()) > int.Parse(tb_toY.Text.ToString().ToUpper()))
                    {
                        SectionData.SetError(tb_fromY, p_errorFromY, tt_errorFromY, "trValidLocationToBigger");
                        SectionData.SetError(tb_toY, p_errorToY, tt_errorToY, "trValidLocationToBigger");
                        isValid = false;
                    }
                }
            }
            ///////
            if (regexAlpha.IsMatch(tb_fromZ.Text) && regexAlpha.IsMatch(tb_toZ.Text) ||
                regexNumber.IsMatch(tb_fromZ.Text) && regexNumber.IsMatch(tb_toZ.Text))
            {
                if (regexAlpha.IsMatch(tb_fromZ.Text))
                {
                    if (char.Parse(tb_fromZ.Text.ToString().ToUpper()) > char.Parse(tb_toZ.Text.ToString().ToUpper()))
                    {
                        SectionData.SetError(tb_fromZ, p_errorFromZ, tt_errorFromZ, "trValidLocationToBigger");
                        SectionData.SetError(tb_toZ, p_errorToZ, tt_errorToZ, "trValidLocationToBigger");
                        isValid = false;
                    }
                }
                else
                {
                    if (int.Parse(tb_fromZ.Text.ToString().ToUpper()) > int.Parse(tb_toZ.Text.ToString().ToUpper()))
                    {
                        SectionData.SetError(tb_fromZ, p_errorFromZ, tt_errorFromZ, "trValidLocationToBigger");
                        SectionData.SetError(tb_toZ, p_errorToZ, tt_errorToZ, "trValidLocationToBigger");
                        isValid = false;
                    }
                }
            }
            /////////////////////////////////

            /////////////////////////////////
            if (regexAlpha.IsMatch(tb_fromX.Text) && !regexAlpha.IsMatch(tb_toX.Text) ||
                regexNumber.IsMatch(tb_fromX.Text) && !regexNumber.IsMatch(tb_toX.Text))
            {
                SectionData.SetError(tb_fromX, p_errorFromX, tt_errorFromX, "trValidLocationMatch");
                SectionData.SetError(tb_toX, p_errorToX, tt_errorToX, "trValidLocationMatch");
                isValid = false;
            }
            if (regexAlpha.IsMatch(tb_fromY.Text) && !regexAlpha.IsMatch(tb_toY.Text) ||
                regexNumber.IsMatch(tb_fromY.Text) && !regexNumber.IsMatch(tb_toY.Text))
            {
                SectionData.SetError(tb_fromY, p_errorFromY, tt_errorFromY, "trValidLocationMatch");
                SectionData.SetError(tb_toY, p_errorToY, tt_errorToY, "trValidLocationMatch");
                isValid = false;
            }
            if (regexAlpha.IsMatch(tb_fromZ.Text) && !regexAlpha.IsMatch(tb_toZ.Text) ||
                regexNumber.IsMatch(tb_fromZ.Text) && !regexNumber.IsMatch(tb_toZ.Text))
            {
                SectionData.SetError(tb_fromZ, p_errorFromZ, tt_errorFromZ, "trValidLocationMatch");
                SectionData.SetError(tb_toZ, p_errorToZ, tt_errorToZ, "trValidLocationMatch");
                isValid = false;
            }
            /////////////////////////////////

            if ((tb_fromX.Text.Equals("")) || (tb_fromX.Text.Equals("")) || (tb_fromZ.Text.Equals("")) ||
                (tb_toX.Text.Equals("")) || (tb_toX.Text.Equals("")) || (tb_toZ.Text.Equals("")))
             isValid = false;

            return isValid;
        }
        
        void generateLocationListX(Location location)
        {
            #region x
            if (regexAlpha.IsMatch(tb_fromX.Text)  && !regexNumber.IsMatch(tb_fromX.Text))
            {
                for (char x = char.Parse(tb_fromX.Text.ToString().ToUpper());
                  x <= char.Parse(tb_toX.Text.ToString().ToUpper()); x++)

                {
                    location.x = x.ToString();
                    generateLocationListY(location);
                }
            }
            else
            {
                for (int x = int.Parse(tb_fromX.Text.ToString());
                 x <= int.Parse(tb_toX.Text.ToString()); x++)
                {
                    location.x = x.ToString();
                    generateLocationListY(location);
                }
            }
            #endregion  
        }
        void generateLocationListY(Location location)
        {
            #region y
            if (regexAlpha.IsMatch(tb_fromY.Text) &&  !regexNumber.IsMatch(tb_fromY.Text))
            {
                for (char y = char.Parse(tb_fromY.Text.ToString().ToUpper());
                  y <= char.Parse(tb_toY.Text.ToString().ToUpper()); y++)

                {
                    location.y = y.ToString();
                    generateLocationListZ(location);
                }
            }
            else
            {
                for (int y = int.Parse(tb_fromY.Text.ToString());
                  y <= int.Parse(tb_toY.Text.ToString()); y++)
                {
                    location.y = y.ToString();
                    generateLocationListZ(location);
                }
            }
            #endregion  
        }
        void generateLocationListZ(Location location)
        {
            #region z
            if (regexAlpha.IsMatch(tb_fromZ.Text) && !regexNumber.IsMatch(tb_fromZ.Text))
            {
                for (char z = char.Parse(tb_fromZ.Text.ToString().ToUpper());
                  z <= char.Parse(tb_toZ.Text.ToString().ToUpper()); z++)

                {
                    location.z = z.ToString();
                    Location l = new Location();
                    l.x = location.x;
                    l.y = location.y;
                    l.z = location.z;
                    locations.Add(l);
                }
            }
            else
            {
                for (int z = int.Parse(tb_fromZ.Text.ToString());
                  z <= int.Parse(tb_toZ.Text.ToString()); z++)
                {
                    location.z = z.ToString();
                    Location l = new Location();
                    l.x = location.x;
                    l.y = location.y;
                    l.z = location.z;
                    locations.Add(l);
                }
            }

            #endregion z
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
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_locationRange);
                
                if (validate(location))
                {
                    int s = 0;
                    generateLocationListX(location);
                    foreach (var item in locations)
                    {
                        if (AllLocations.Where(x => x.name == item.name && x.branchId == MainWindow.branchID).Count() == 0)
                        {
                            item.createUserId = MainWindow.userID;
                            item.updateUserId = MainWindow.userID;
                            item.note = "";
                            item.isActive = 1;
                            item.sectionId = null;
                            item.branchId = MainWindow.branchID;

                            s = (int)await location.save(item);
                        }
                    }

                    if (!s.Equals("-1"))
                    {
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                        Btn_clear_Click(null, null);
                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                }

                if (sender != null)
                    SectionData.EndAwait(grid_locationRange);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_locationRange);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                tb_fromX.Clear();
                tb_fromY.Clear();
                tb_fromZ.Clear();

                p_errorFromX.Visibility = Visibility.Collapsed;
                p_errorFromY.Visibility = Visibility.Collapsed;
                p_errorFromZ.Visibility = Visibility.Collapsed;

                tb_fromX.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_fromY.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_fromZ.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                /////////////////////////////////////////////
                tb_toX.Clear();
                tb_toY.Clear();
                tb_toZ.Clear();

                p_errorToX.Visibility = Visibility.Collapsed;
                p_errorToY.Visibility = Visibility.Collapsed;
                p_errorToZ.Visibility = Visibility.Collapsed;

                tb_toX.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_toY.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_toZ.Background = (Brush)bc.ConvertFrom("#f8f8f8");
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void validationControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }
        
        private void validationControl_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                bool isValid = false;

                if (regex.IsMatch(e.Text))
                {
                    if (regexNumber.IsMatch(e.Text) && !(regexAlpha.IsMatch((sender as TextBox).Text)))
                    {
                        e.Handled = false;
                        isValid = true;
                    }
                    else if (regexAlpha.IsMatch(e.Text) && (sender as TextBox).Text.Count() == 0 && !regexNumber.IsMatch((sender as TextBox).Text))
                    {
                        e.Handled = false;
                        isValid = true;
                    }
                    else if (regexAlpha.IsMatch(e.Text) && (sender as TextBox).Text.Count() == 1 && !regexNumber.IsMatch((sender as TextBox).Text))
                    {
                        (sender as TextBox).Text = "";
                        e.Handled = false;
                        isValid = true;
                    }
                }
                if (!isValid)
                    e.Handled = true;
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
                if ((sender as Control).Name == "tb_fromX")
                    SectionData.validateEmptyTextBox(tb_fromX, p_errorFromX, tt_errorFromX, "");
                else if ((sender as Control).Name == "tb_fromY")
                    SectionData.validateEmptyTextBox(tb_fromY, p_errorFromY, tt_errorFromY, "");
                else if ((sender as Control).Name == "tb_fromZ")
                    SectionData.validateEmptyTextBox(tb_fromZ, p_errorFromZ, tt_errorFromZ, "");

                else if ((sender as Control).Name == "tb_toX")
                    SectionData.validateEmptyTextBox(tb_toX, p_errorToX, tt_errorToX, "");
                else if ((sender as Control).Name == "tb_toY")
                    SectionData.validateEmptyTextBox(tb_toY, p_errorToY, tt_errorToY, "");
                else if ((sender as Control).Name == "tb_toZ")
                    SectionData.validateEmptyTextBox(tb_toZ, p_errorToZ, tt_errorToZ, "");
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
                if ((sender as Control).Name == "tb_fromX")
                    SectionData.validateEmptyTextBox(tb_fromX, p_errorFromX, tt_errorFromX, "");
                else if ((sender as Control).Name == "tb_fromY")
                    SectionData.validateEmptyTextBox(tb_fromY, p_errorFromY, tt_errorFromY, "");
                else if ((sender as Control).Name == "tb_fromZ")
                    SectionData.validateEmptyTextBox(tb_fromZ, p_errorFromZ, tt_errorFromZ, "");
                ////////////////////////////////////
                else if ((sender as Control).Name == "tb_toX")
                    SectionData.validateEmptyTextBox(tb_toX, p_errorToX, tt_errorToX, "");
                else if ((sender as Control).Name == "tb_toY")
                    SectionData.validateEmptyTextBox(tb_toY, p_errorToY, tt_errorToY, "");
                else if ((sender as Control).Name == "tb_toZ")
                    SectionData.validateEmptyTextBox(tb_toZ, p_errorToZ, tt_errorToZ, "");
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
                    SectionData.StartAwait(grid_locationRange);

                #region translate
                if (AppSettings.lang.Equals("en"))
                { MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly()); grid_locationRange.FlowDirection = FlowDirection.LeftToRight; }
                else
                { MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly()); grid_locationRange.FlowDirection = FlowDirection.RightToLeft; }

                translate();
                #endregion

                AllLocations = await location.Get();

                if (sender != null)
                    SectionData.EndAwait(grid_locationRange);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_locationRange);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trLocation");
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_fromX, MainWindow.resourcemanager.GetString("trFromXHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_fromY, MainWindow.resourcemanager.GetString("trFromYHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_fromZ, MainWindow.resourcemanager.GetString("trFromZHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_toX, MainWindow.resourcemanager.GetString("trToXHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_toY, MainWindow.resourcemanager.GetString("trToYHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_toZ, MainWindow.resourcemanager.GetString("trToZHint"));

            tt_fromX.Content = MainWindow.resourcemanager.GetString("trFromX");
            tt_fromY.Content = MainWindow.resourcemanager.GetString("trFromY");
            tt_fromZ.Content = MainWindow.resourcemanager.GetString("trFromZ");
            tt_ToX.Content = MainWindow.resourcemanager.GetString("trToX");
            tt_toY.Content = MainWindow.resourcemanager.GetString("trToY");
            tt_toZ.Content = MainWindow.resourcemanager.GetString("trToZ");
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

       
    }
}
