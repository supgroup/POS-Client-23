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
    /// Interaction logic for wd_dragdrop.xaml
    /// </summary>
    public partial class wd_dragdrop : Window
    {
        public wd_dragdrop()
        {
            InitializeComponent();
        }

        string s = "";
      
        private void Txt1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                s = "txt1";
                DragDrop.DoDragDrop(txt1, txt1.Text, DragDropEffects.All);
            }
        }

        private void Txt2_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }
         
        private void Txt2_Drop(object sender, DragEventArgs e)
        {
            string x = txt2.Text;
            txt2.Text = e.Data.GetData(DataFormats.Text , true).ToString();
            switch(s)
            {
                case "txt1": txt1.Text = x;  break;
                case "txt3": txt3.Text = x; break;
            }
        }

        private void Txt2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                s = "txt2";
                DragDrop.DoDragDrop(txt2, txt2.Text, DragDropEffects.All);
            }
        }

        private void Txt1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void Txt1_Drop(object sender, DragEventArgs e)
        {
            string x = txt1.Text;
            txt1.Text = e.Data.GetData(DataFormats.Text, true).ToString();
            switch (s)
            {
                case "txt2": txt2.Text = x; break;
                case "txt3": txt3.Text = x; break;
            }
        }

        private void Txt3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                s = "txt3";
                DragDrop.DoDragDrop(txt3, txt3.Text, DragDropEffects.All);
            }
        }

        private void Txt3_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void Txt3_Drop(object sender, DragEventArgs e)
        {
            string x = txt3.Text;
            txt3.Text = e.Data.GetData(DataFormats.Text, true).ToString();
            switch (s)
            {
                case "txt1": txt1.Text = x; break;
                case "txt2": txt2.Text = x; break;
            }
        }

        private void Grid_drag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(grid_drag, grid_drag.Children, DragDropEffects.All);
            }
        }

        private void Grid_drop_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void Grid_drop(object sender, DragEventArgs e)
        {
            //grid_drag.Children.Clear();
            // grid_drop.Children.Add(e.Data.GetData(DataFormats.GetDataFormat(), true));
            grid_drop.Background = grid_drag.Background;
        }
    }
}
