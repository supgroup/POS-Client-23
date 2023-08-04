using POS.Classes;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;

namespace POS.controlTemplate
{
    /// <summary>
    /// Interaction logic for UC_squareCard.xaml
    /// </summary>
    public partial class UC_squareCard : UserControl
    {
        public int ContentId { get; set; }
        public CardViewItems categoryCardView { get; set; }
        public int rowCount { get; set; }
        public int columnCount { get; set; }
        public UC_squareCard()
        {
            InitializeComponent();
        }
        public UC_squareCard(CardViewItems _categoryCardView)
        {
            InitializeComponent();
            categoryCardView = _categoryCardView;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
            InitializeControls();
        }
        void InitializeControls()
        {
            #region   Title
            var titleText = new TextBlock();
            titleText.Text = categoryCardView.category.name;
            Grid.SetRow(titleText, 1);
            titleText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#6e6e6e"));
            titleText.VerticalAlignment = VerticalAlignment.Center;
            titleText.HorizontalAlignment = HorizontalAlignment.Center;
            titleText.FontWeight = FontWeights.Bold;
            titleText.FontSize = 10;
            titleText.FontFamily = App.Current.Resources["Font-cairo-bold"] as FontFamily;
            //titleText.TextWrapping = TextWrapping.Wrap;
            grid_main.Children.Add(titleText);

            if(!string.IsNullOrWhiteSpace(titleText.Text))
                this.ToolTip = titleText.Text;

            #endregion
            #region Image
            Button buttonImage = new Button();
            buttonImage.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            buttonImage.FlowDirection = FlowDirection.LeftToRight;
            buttonImage.Padding = new Thickness(0);
            //d: DesignHeight = "120" d: DesignWidth = "100"
            //  Height = "77" Width = "85"
            //buttonImage.Height = grid_main.Height* 0.625;
            buttonImage.Height = grid_main.ActualHeight* 0.625;
            //buttonImage.Width = grid_main.Width * 0.85;
            buttonImage.Width = grid_main.ActualWidth * 0.85;
            buttonImage.BorderThickness = new Thickness(0);
            if (categoryCardView.category.image == null)
                SectionData.clearImg(buttonImage);
            else
            {
                bool isModified = SectionData.chkImgChng(categoryCardView.category.image, (DateTime)categoryCardView.category.updateDate, Global.TMPFolder);
                if (isModified)
                    SectionData.getImg("Category", categoryCardView.category.image, buttonImage);
                else
                    SectionData.getLocalImg("Category", categoryCardView.category.image, buttonImage);
            }
            Grid grid_image = new Grid();
            if (buttonImage.Height != 0)
            grid_image.Height = buttonImage.Height - 2;
            if (buttonImage.Width != 0)
                grid_image.Width = buttonImage.Width - 1;
            grid_image.Children.Add(buttonImage);
            //////////////
            #endregion
            brd_image.Child = (grid_image);
        }
        #region ButtonBorderBrush
        public static readonly DependencyProperty squareCardBorderBrushDependencyProperty = DependencyProperty.Register("squareCardBorderBrush",
            typeof(string),
            typeof(UC_squareCard),
            new PropertyMetadata("DEFAULT"));
        public string squareCardBorderBrush
        {
            set
            { SetValue(squareCardBorderBrushDependencyProperty, value); }
            get
            { return (string)GetValue(squareCardBorderBrushDependencyProperty); }
        }
        #endregion

       
    }
}
