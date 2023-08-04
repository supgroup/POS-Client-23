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
    /// Interaction logic for wd_favorite.xaml
    /// </summary>
    public partial class wd_favorite : Window
    {
        public wd_favorite()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
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

                await getCategories();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this);
            }
        }

     
        Category categoryModel = new Category();
        Categoryuser categoryuserModel = new Categoryuser();
        Category category = new Category();
        List<Category> categoryLst = new List<Category>();
        List<Categoryuser> categoryuserLst = new List<Categoryuser>();
        List<TextBlock> categoryTextblocks = new List<TextBlock>();
        TextBlock tx;
        int index = 0;
        private async Task getCategories()
        {
           categoryLst = await categoryModel.GetAllCategories(MainWindow.userID.Value);

            foreach (var c in categoryLst)
            {
                tx = new TextBlock();
                tx.Width = 250; tx.Height = 50;
                tx.Margin = new Thickness(5);
                tx.FontSize = 12; tx.Foreground = new SolidColorBrush(Colors.Black);
                tx.Background = new SolidColorBrush(Colors.Pink);
                tx.HorizontalAlignment = HorizontalAlignment.Center;
                tx.VerticalAlignment = VerticalAlignment.Center;
                tx.TextAlignment = TextAlignment.Center;
                tx.Text = c.name;
                tx.Tag = c.categoryId;
                tx.Name = "tx"+c.categoryId;
                tx.AllowDrop = true;
                tx.MouseDown += this.wpMouseDown;
                tx.DragEnter += this.wpDragEnter;
                tx.Drop += this.wpDrop;

                pnl_categories.Children.Add(tx);
                categoryTextblocks.Add(tx);
            }
        }

        private async void wpDrop(object sender, DragEventArgs e)
        {
            categoryTextblocks[index].Text = (sender as TextBlock).Text;
            categoryTextblocks[index].Tag = (sender as TextBlock).Tag;
            categoryTextblocks[index].Name = (sender as TextBlock).Name;
            int id = int.Parse(e.Data.GetData(DataFormats.Text, true).ToString());
            category =  await categoryModel.getById(id);
            (sender as TextBlock).Text = category.name ;
            (sender as TextBlock).Tag = category.categoryId;
            
        }

        private void wpDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }
        private void wpMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (categoryTextblocks.Contains(sender as TextBlock))
                {
                    index = categoryTextblocks.FindIndex(c => c.Tag == (sender as TextBlock).Tag);
                }
                DragDrop.DoDragDrop(sender as TextBlock , (sender as TextBlock).Tag.ToString() , DragDropEffects.All);
            }
        }

        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trFavorite");
            txt_categories.Text = MainWindow.resourcemanager.GetString("trCategories");
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
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
                SectionData.ExceptionMessage(ex, this);
            }
        }

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {//close

        }

        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            categoryuserLst = await categoryuserModel.GetByUserId(MainWindow.userID.Value);
            MessageBox.Show(categoryuserLst.Count.ToString());
        }
    }
}
