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
using System.IO;
using POS.Classes;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_previewPdf.xaml
    /// </summary>
    public partial class wd_previewPdf : Window
    {
        public wd_previewPdf()
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
        public string pdfPath;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_branchList);
                translate();
                wb_pdfWebViewer.Navigate(new Uri(pdfPath));

                if (sender != null)
                    SectionData.EndAwait(grid_branchList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_branchList);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trPreview");
            
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
        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                //Btn_save_Click(null, null);
            }
        }

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
           
            DialogResult = true;
            this.Close();
        }

       
    }
}
