﻿using POS.Classes;
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
    /// Interaction logic for wd_messageBox.xaml
    /// </summary>
    public partial class wd_messageBox : Window
    {
        public bool isOk;

        public wd_messageBox()
        {
            try
            {
                InitializeComponent();
                this.DataContext = this;
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
                {
                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    grid_main.FlowDirection = FlowDirection.RightToLeft;
                }
                #endregion
                translate();


            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void translate()
        {
            if (string.IsNullOrWhiteSpace(titleText2))
                txt_title.Text = MainWindow.resourcemanager.GetString("trMessage");
            btn_ok.Content = MainWindow.resourcemanager.GetString("trOK");
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
        

        #region contentText2
        public static readonly DependencyProperty contentText2DependencyProperty = DependencyProperty.Register("contentText2",
            typeof(string),
            typeof(wd_acceptCancelPopup),
            new PropertyMetadata("DEFAULT"));
        public string contentText2
        {
            set
            { SetValue(contentText2DependencyProperty, value); }
            get
            { return (string)GetValue(contentText2DependencyProperty); }
        }
        #endregion
        #region titleText2
        public static readonly DependencyProperty titleText2DependencyProperty = DependencyProperty.Register("titleText2",
            typeof(string),
            typeof(wd_messageBox),
            new PropertyMetadata(""));
        public string titleText2
        {
            set
            { SetValue(titleText2DependencyProperty, value); }
            get
            { return (string)GetValue(titleText2DependencyProperty); }
        }
        #endregion

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception)
            {

            }
        }


        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            
        }

       
    }
}
