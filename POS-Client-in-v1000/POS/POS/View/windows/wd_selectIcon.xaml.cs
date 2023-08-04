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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_selectIcon.xaml
    /// </summary>
    public partial class wd_selectIcon : Window
    {

        public bool isOk;
        public string _selectedIcon = "website";
        public List<string> socialMediaIcons = new List<string>();
        public wd_selectIcon()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            { SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isOk = false;
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
                socialMediaIcons = new List<string>() { "website", "facebook", "instagram", "messenger", "skype",
                    "snapchat", "telegram","tikTok","twitter","whatsapp", "youtube" };

                buildListIconsDesign(socialMediaIcons);
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

        void buildListIconsDesign(List<string> listIcons)
        {
            wp_main.Children.Clear();
            foreach (var item in listIcons)
            {
                #region Button
                Button button = new Button();
                button.Padding = new Thickness(0, 0, 0, 0);
                button.Background = null;
                button.BorderBrush = null;
                button.Name ="btn_"+ item;
                button.Tag = item;
                button.Margin = new Thickness(5);
                button.Height = 60;
                button.Width = 60;
                button.Click += icon_Click;
                #region border
                Border border1 = new Border();
                border1.CornerRadius = new CornerRadius(100);
                border1.BorderThickness = new Thickness(2);
                if (item == _selectedIcon)
                {
                    border1.BorderBrush = Application.Current.Resources["MainColor"] as SolidColorBrush;
                }
                else
                {
                    border1.BorderBrush = null;
                }
                #region border
                Border border = new Border();
                border.CornerRadius = new CornerRadius(100);
                border.BorderThickness = new Thickness(1);
                 border.BorderBrush = Application.Current.Resources["LightGrey"] as SolidColorBrush; 
                #region Path
                Path path = new Path();
                path.Stretch = Stretch.Fill;
                path.FlowDirection = FlowDirection.LeftToRight;
                path.Fill = Application.Current.Resources[item] as SolidColorBrush;
                path.Data = Application.Current.Resources[item + "Icon"] as Geometry;
                #endregion
                border.Child = path;
                #endregion
                border1.Child = border;
                #endregion
                button.Content = border1;
                #endregion
                wp_main.Children.Add(button);

            }

        }
        void icon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                _selectedIcon = button.Tag.ToString();
                buildListIconsDesign(socialMediaIcons);

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region methods
        private void translate()
        {
            //txt_title.Text = MainWindow.resourcemanager.GetString("trTax");

            //btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
        }
      
        #endregion

        #region events
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

        private  void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                //if (sender != null)
                //    SectionData.StartAwait(grid_main);

                isOk = true;
                this.Close();


                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }



    }
}
