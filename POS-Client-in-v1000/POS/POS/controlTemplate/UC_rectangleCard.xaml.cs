using System;
using System.Collections.Generic;
using System.Globalization;
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
using POS.Classes;

namespace POS.controlTemplate
{
    /// <summary>
    /// Interaction logic for UC_rectangleCard.xaml
    /// </summary>
    public partial class UC_rectangleCard : UserControl
    {
        public UC_rectangleCard()
        {
            InitializeComponent();
        }
        public int contentId { get; set; }
        public CardViewItems cardViewitem { get; set; }
        public UC_rectangleCard(CardViewItems _CardViewitems)
        {
            InitializeComponent();
            cardViewitem = _CardViewitems;
        }
        async void CreateItemCard()
        {

            #region Grid Container
            Grid gridContainer = new Grid();
            ColumnDefinition[] cd = new ColumnDefinition[2];
            for (int i = 0; i < 2; i++)
                cd[i] = new ColumnDefinition();
            cd[0].Width = new GridLength(1.2, GridUnitType.Star);
            cd[1].Width = new GridLength(1, GridUnitType.Star);
            for (int i = 0; i < 2; i++)
                gridContainer.ColumnDefinitions.Add(cd[i]);
            //int rowCount = 3;
            //if (cardViewitem.cardType == "sales")
            int rowCount = 5;
            RowDefinition[] rd = new RowDefinition[5];
            for (int i = 0; i < rowCount; i++)
            {
                rd[i] = new RowDefinition();
            }
            rd[0].Height = new GridLength(1, GridUnitType.Star);
            rd[1].Height = new GridLength(20, GridUnitType.Pixel);
            rd[2].Height = new GridLength(20, GridUnitType.Pixel);
            rd[3].Height = new GridLength(20, GridUnitType.Pixel);
            //if (cardViewitem.cardType == "sales")
                rd[4].Height = new GridLength(20, GridUnitType.Star);
            for (int i = 0; i < rowCount; i++)
            {
                gridContainer.RowDefinitions.Add(rd[i]);
            }
            

            /////////////////////////////////////////////////////
            if (this.ActualHeight != 0)
                gridContainer.Height = this.ActualHeight - 10;
            if (this.ActualHeight != 0)
                gridContainer.Width = this.ActualWidth - 10;
            /////////////////////////////////////////////////////
            if (double.IsNaN(gridContainer.Width) || double.IsNaN(gridContainer.Height))
            {
                //MessageBox.Show("");
                await Task.Delay(0050);
                /////////////////////////////////////////////////////
                if (this.ActualHeight != 0)
                    gridContainer.Height = this.ActualHeight - 10;
                if (this.ActualHeight != 0)
                    gridContainer.Width = this.ActualWidth - 10;
                /////////////////////////////////////////////////////
            }
            /////////////////////////////////////////////////////

            brd_main.Child = gridContainer;
            #endregion
            if (AppSettings.lang.Equals("en"))
                grid_main.FlowDirection = FlowDirection.LeftToRight ;
            else grid_main.FlowDirection = FlowDirection.RightToLeft;
            #region   Title
            var titleText = new TextBlock();
            titleText.Text = cardViewitem.item.name;
            titleText.FontSize = 13;
            titleText.Height = 16;
            titleText.FontFamily = App.Current.Resources["Font-cairo-bold"] as FontFamily;
            titleText.Margin = new Thickness(5, 4, 5, 0);
            titleText.FontWeight = FontWeights.Bold;
            titleText.VerticalAlignment = VerticalAlignment.Top;
            titleText.HorizontalAlignment = HorizontalAlignment.Left;
            titleText.TextWrapping = TextWrapping.Wrap;
            titleText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#6e6e6e"));
            Grid.SetRow(titleText, 1);
            /////////////////////////////////

            #endregion
            #region  subTitle
            var subTitleText = new TextBlock();
            subTitleText.Text = cardViewitem.item.details;
            subTitleText.Margin = new Thickness(5, 0, 5, 0);
            subTitleText.FontWeight = FontWeights.Regular;
            subTitleText.HorizontalAlignment = HorizontalAlignment.Left;
            subTitleText.FontSize = 10;
            subTitleText.TextWrapping = TextWrapping.Wrap;
            subTitleText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#6e6e6e"));
            if (cardViewitem.item.itemCount == null)
                Grid.SetRowSpan(subTitleText, 2);
            Grid.SetRow(subTitleText, 2);
            /////////////////////////////////

            #endregion
            #region  itemCount
            /*
            var countText = new TextBlock();
            countText.Foreground = Application.Current.Resources["MainColor"] as SolidColorBrush;
            if (cardViewitem.item.itemCount == null)
                countText.Text = "";
            else
            {
                if (cardViewitem.item.type != "sr" && cardViewitem.item.itemCount > 0)
                {
                    countText.Text = cardViewitem.item.itemCount + " " + cardViewitem.item.unitName;

                    if (cardViewitem.item.reservedCount != null && cardViewitem.item.reservedCount != 0)
                    {
                        //countText.Text = cardViewitem.item.itemCount + " + "+ cardViewitem.item.reservedCount + " " + cardViewitem.item.unitName;
                        countText.Text += $" + {cardViewitem.item.reservedCount} { MainWindow.resourcemanager.GetString("trReserved")}";
                    }

                }
                else if (cardViewitem.item.type != "sr" && cardViewitem.item.itemCount == 0)
                {
                    countText.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                    if (cardViewitem.item.reservedCount != null && cardViewitem.item.reservedCount != 0)
                    {
                        countText.Text = $"{cardViewitem.item.reservedCount} { MainWindow.resourcemanager.GetString("trReserved}")}";
                    }
                    else
                        countText.Text = MainWindow.resourcemanager.GetString("trOutOfStock");
                }
                else
                {
                    countText.Text = MainWindow.resourcemanager.GetString("trService");
                }
            }


            //countText.FlowDirection = FlowDirection.LeftToRight;
            countText.Margin = new Thickness(5, 0, 5, 0);
            countText.FontWeight = FontWeights.Regular;
            countText.VerticalAlignment = VerticalAlignment.Center;
            countText.HorizontalAlignment = HorizontalAlignment.Left;
            countText.Height = 18;
            countText.FontSize = 9;
            //countText.TextWrapping = TextWrapping.Wrap;
            countText.FontWeight = FontWeights.Bold;
            Grid.SetRow(countText, 3);
            /////////////////////////////////
            */
            StackPanel stackPanelCount = new StackPanel();
            stackPanelCount.Orientation = Orientation.Horizontal;

            var countText = new TextBlock();
            var reservedCountText = new TextBlock();
            countText.Margin = new Thickness(5, 0, 0, 0);
            reservedCountText.FontWeight =countText.FontWeight = FontWeights.Regular;
            reservedCountText.VerticalAlignment =countText.VerticalAlignment = VerticalAlignment.Center;
            reservedCountText.HorizontalAlignment =countText.HorizontalAlignment = HorizontalAlignment.Left;
            reservedCountText.Height =countText.Height = 18;
            reservedCountText.FontSize =countText.FontSize = 9;
            reservedCountText.FontWeight =countText.FontWeight = FontWeights.Bold;
            reservedCountText.Margin = new Thickness(0, 0, 0, 0);


            countText.Foreground = Application.Current.Resources["MainColor"] as SolidColorBrush;
            reservedCountText.Foreground = countText.Foreground;
            if (cardViewitem.item.itemCount == null)
                countText.Text = "";
            else
            {
                if (cardViewitem.item.type != "sr" && cardViewitem.item.itemCount > 0)
                {
                    countText.Text = cardViewitem.item.itemCount + " " + cardViewitem.item.unitName;

                    if (cardViewitem.item.reservedCount != null && cardViewitem.item.reservedCount != 0)
                    {
                        reservedCountText.Text = $" + {cardViewitem.item.reservedCount} { MainWindow.resourcemanager.GetString("trReserved")}";
                    }

                }
                else if (cardViewitem.item.type != "sr" && cardViewitem.item.itemCount == 0)
                {
                    countText.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                    if (cardViewitem.item.reservedCount != null && cardViewitem.item.reservedCount != 0)
                    {
                        countText.Text = $"{cardViewitem.item.reservedCount} { MainWindow.resourcemanager.GetString("trReserved")}";
                    }
                    else
                        countText.Text = MainWindow.resourcemanager.GetString("trOutOfStock");
                }
                else
                {
                    countText.Text = MainWindow.resourcemanager.GetString("trService");
                }
            }

            reservedCountText.Foreground = countText.Foreground;

            stackPanelCount.Children.Add(countText);
            if(!string.IsNullOrWhiteSpace(reservedCountText.Text))
                stackPanelCount.Children.Add(reservedCountText);


            Grid.SetRow(stackPanelCount, 3);
            /////////////////////////////////
            #endregion
            #region Price
            if (cardViewitem.cardType == "sales" || cardViewitem.cardType == "order")
            {
                Grid gridPrice = new Grid();
                Grid.SetRow(gridPrice, 4);
                //70 
                gridPrice.Width = gridContainer.Width / 2;
                //25
                gridPrice.Height = gridContainer.Height / 4;
                gridPrice.HorizontalAlignment = HorizontalAlignment.Left;
                gridPrice.Margin = new Thickness(5,2.5,5,5);
                /////////////////////////////
                Rectangle rectanglePrice = new Rectangle();

                if(cardViewitem.item.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).Count() > 0)
                rectanglePrice.Fill = Application.Current.Resources["MainColor"] as SolidColorBrush;
                else
                rectanglePrice.Fill = Application.Current.Resources["Grey"] as SolidColorBrush;

                rectanglePrice.RadiusX = 4;
                rectanglePrice.RadiusY = 4;
                gridPrice.Children.Add(rectanglePrice);
                ////////////////////////////////
                var priceText = new TextBlock();

                //priceText.Text = cardViewitem.item.priceTax.ToString();
                try
                {
                    //if (AppSettings.itemsTax_bool == true)
                    //    priceText.Text = SectionData.DecTostring(cardViewitem.item.priceTax) ; 
                    //else
                    //    priceText.Text = SectionData.DecTostring(cardViewitem.item.price);

                    if (cardViewitem.item.SalesPrices == null || (cardViewitem.item.SalesPrices != null && cardViewitem.item.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault() == null))
                    {
                        //if (AppSettings.itemsTax_bool == true)
                        //    priceText.Text = ((decimal)cardViewitem.item.priceTax).ToString();
                        //else
                            priceText.Text = ((decimal)cardViewitem.item.price).ToString();
                    }
                    else
                    {
                        var slice = cardViewitem.item.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault();

                        //if (AppSettings.itemsTax_bool == true)
                        //    priceText.Text = ((decimal)slice.priceTax).ToString();
                        //else
                            priceText.Text = ((decimal)slice.price).ToString();
                    }
                }
                catch
                {
                    priceText.Text = "";
                }

                priceText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
                priceText.FontWeight = FontWeights.Bold;
                priceText.VerticalAlignment = VerticalAlignment.Center;
                priceText.HorizontalAlignment = HorizontalAlignment.Center;
                priceText.FontSize = 14;
                gridPrice.Children.Add(priceText);
                /////////////////////////////////

                gridContainer.Children.Add(gridPrice);
            }
            #endregion
            #region Image
            Item item = new Item();
            Button buttonImage = new Button();
            buttonImage.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            //buttonImage.Height = (gridContainer.Height) - 7.5;
            //buttonImage.Width = (gridContainer.Width / 2.2) - 7.5;
            buttonImage.Height = (gridContainer.Height / 1.1) - 7.5;
            buttonImage.Width = ((gridContainer.Width / 2.2) / 1.2) - 7.5;
            buttonImage.BorderThickness = new Thickness(0);
            buttonImage.Padding = new Thickness(0);
            buttonImage.FlowDirection = FlowDirection.LeftToRight;
            MaterialDesignThemes.Wpf.ButtonAssist.SetCornerRadius(buttonImage, (new CornerRadius(10)));
            bool isModified = SectionData.chkImgChng(cardViewitem.item.image, (DateTime)cardViewitem.item.updateDate, Global.TMPItemsFolder);
            if (isModified)
                SectionData.getImg("Item", cardViewitem.item.image, buttonImage);
            else
                SectionData.getLocalImg("Item", cardViewitem.item.image, buttonImage);
            Grid grid_image = new Grid();
            grid_image.Height = buttonImage.Height - 2;
            grid_image.Width = buttonImage.Width - 1;
            Grid.SetRowSpan(grid_image, 5);
            Grid.SetColumn(grid_image, 1);
            grid_image.Children.Add(buttonImage);

            gridContainer.Children.Add(grid_image);

            //////////////
            #endregion
            if (cardViewitem.item.isNew == 1)
            {
                /*
                #region Path Star
                //string dataStar = "";
                Path pathStar = new Path();
                Grid.SetRowSpan(pathStar, 5);
                pathStar.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA926"));
                pathStar.Stretch = Stretch.Fill;
                Grid.SetColumnSpan(pathStar, 2);
                pathStar.Height = 18;
                pathStar.Width = 54;
                pathStar.VerticalAlignment = VerticalAlignment.Bottom;
                pathStar.HorizontalAlignment = HorizontalAlignment.Right;
                pathStar.FlowDirection = FlowDirection.LeftToRight;
                pathStar.Margin = new Thickness(7.5);
                pathStar.Data = App.Current.Resources["NewIconGeometry"] as Geometry;
                #endregion
                gridContainer.Children.Add(pathStar);
                */
                #region Path newLabel
                Path pathNewLabel = new Path();
                Grid.SetRowSpan(pathNewLabel, 5);
                pathNewLabel.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D20707"));
                pathNewLabel.Stretch = Stretch.Fill;
                Grid.SetColumnSpan(pathNewLabel, 2);
                //pathStar.Height = 18;
                //pathStar.Width = 54;
                //pathNewLabel.VerticalAlignment = VerticalAlignment.Bottom;
                //pathNewLabel.HorizontalAlignment = HorizontalAlignment.Right;
                pathNewLabel.FlowDirection = FlowDirection.LeftToRight;
                //pathNewLabel.Margin = new Thickness(7.5);
                pathNewLabel.Data = App.Current.Resources["newBlock"] as Geometry;
                 pathNewLabel.Width = gridContainer.Width / 6.5;
                pathNewLabel.Height = pathNewLabel.Width / 3;
                #region Text
                Path pathNewLabelText = new Path();
                Grid.SetRowSpan(pathNewLabelText, 5);
                pathNewLabelText.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFD00"));
                pathNewLabelText.Stretch = Stretch.Fill;
                Grid.SetColumnSpan(pathNewLabelText, 2);
                
                //pathNewLabelText.VerticalAlignment = VerticalAlignment.Bottom;
                //pathNewLabelText.HorizontalAlignment = HorizontalAlignment.Right;
                pathNewLabelText.FlowDirection = FlowDirection.LeftToRight;
                //pathNewLabelText.Margin = new Thickness(7.5);
                //pathNewLabelText.Margin = new Thickness(0, 0, 12.5, 10);
                pathNewLabelText.Data = App.Current.Resources["newText"] as Geometry;
                //pathStar.Height = 18;
                //pathStar.Width = 54;
                pathNewLabelText.Width = gridContainer.Width / 10;
                pathNewLabelText.Height = pathNewLabelText.Width /3;
                #endregion
                #endregion

                Grid gridNewContainer = new Grid();
                Grid.SetRowSpan(gridNewContainer, 5);
                Grid.SetColumnSpan(gridNewContainer, 2);
                gridNewContainer.VerticalAlignment = VerticalAlignment.Bottom;
                gridNewContainer.HorizontalAlignment = HorizontalAlignment.Right;
                gridNewContainer.Margin = new Thickness(7.5);

                gridNewContainer.Children.Add(pathNewLabel);
                gridNewContainer.Children.Add(pathNewLabelText);

                gridContainer.Children.Add(gridNewContainer);

                //gridContainer.Children.Add(pathNewLabel);
                //gridContainer.Children.Add(pathNewLabelText);
            }
            if (cardViewitem.item.isOffer == 1)
            {
                #region Path offerLabel
                //string dataStar = "";
                Path pathOfferLabel = new Path();
                Grid.SetColumnSpan(pathOfferLabel, 2);
                Grid.SetRowSpan(pathOfferLabel, 5);
                pathOfferLabel.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D20707"));
                pathOfferLabel.VerticalAlignment = VerticalAlignment.Top;
                pathOfferLabel.Stretch = Stretch.Fill;
                //   Height = "16" Width = "86" 
                pathOfferLabel.Height = pathOfferLabel.Width = gridContainer.Width / 4.5;
                pathOfferLabel.FlowDirection = FlowDirection.LeftToRight;
                pathOfferLabel.HorizontalAlignment = HorizontalAlignment.Right;

                if (AppSettings.lang.Equals("ar"))
                {
                    pathOfferLabel.Data = App.Current.Resources["offerLabelArTopLeft"] as Geometry;
                }
                else
                {
                    pathOfferLabel.Data = App.Current.Resources["offerLabelEnTopRight"] as Geometry;
                }

                #region Text
                Path pathOfferLabelText = new Path();
                Grid.SetColumnSpan(pathOfferLabelText, 2);
                Grid.SetRowSpan(pathOfferLabelText, 5);
                pathOfferLabelText.FlowDirection = FlowDirection.LeftToRight;
                pathOfferLabelText.VerticalAlignment = VerticalAlignment.Top;
                pathOfferLabelText.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFD00"));
                pathOfferLabelText.Stretch = Stretch.Fill;
                pathOfferLabelText.HorizontalAlignment = HorizontalAlignment.Right;
                if (AppSettings.lang.Equals("ar"))
                {
                    pathOfferLabelText.Height = pathOfferLabelText.Width = gridContainer.Width / 7;
                    pathOfferLabelText.Margin = new Thickness(0,4, 4, 0);
                    pathOfferLabelText.Data = App.Current.Resources["offerLabelArTopLeft_Text"] as Geometry;
                }
                else

                {
                    pathOfferLabelText.Height = pathOfferLabelText.Width = gridContainer.Width / 6.5;
                    pathOfferLabelText.Margin = new Thickness(0, 2.5, 2.5, 0);
                    pathOfferLabelText.Data = App.Current.Resources["offerLabelEnTopRight_Text"] as Geometry;
                }

                #endregion
                #endregion
                gridContainer.Children.Add(pathOfferLabel);
                gridContainer.Children.Add(pathOfferLabelText);
            }
            if (cardViewitem.item.itemCount > 0)
            {
                this.ToolTip = MainWindow.resourcemanager.GetString("trCount: ") + cardViewitem.item.itemCount + " " + cardViewitem.item.unitName;
                //tt_name.Content = "Count" + cardViewitem.item.itemCount;
            }
            gridContainer.Children.Add(titleText);
            gridContainer.Children.Add(subTitleText);
            gridContainer.Children.Add(stackPanelCount);
        }

        void CreateUserCard(string type , string title, string subTitle, string subTitle2, string Img)
        {
            #region Grid Container
            Grid gridContainer = new Grid();
            ColumnDefinition[] cd = new ColumnDefinition[3];
            for (int i = 0; i < 3; i++)
                cd[i] = new ColumnDefinition();
            cd[0].Width = new GridLength(1.2, GridUnitType.Star);
            cd[1].Width = new GridLength(1, GridUnitType.Star);
            for (int i = 0; i < 2; i++)
                gridContainer.ColumnDefinitions.Add(cd[i]);
            //int rowCount =3;
            //if (cardViewitem.cardType == "sales")
            int rowCount = 4;
            RowDefinition[] rd = new RowDefinition[4];
            for (int i = 0; i < rowCount; i++)
            {
                rd[i] = new RowDefinition();
            }
            rd[0].Height = new GridLength(5, GridUnitType.Star);
            rd[1].Height = new GridLength(16, GridUnitType.Star);
            rd[2].Height = new GridLength(13, GridUnitType.Star);
            //if (cardViewitem.cardType == "sales")
            rd[3].Height = new GridLength(20, GridUnitType.Star);
            for (int i = 0; i < rowCount; i++)
            {
                gridContainer.RowDefinitions.Add(rd[i]);
            }
            /////////////////////////////////////////////////////
            if (this.ActualHeight != 0)
                gridContainer.Height = this.ActualHeight - 10;
            if (this.ActualHeight != 0)
                gridContainer.Width = this.ActualWidth - 10;
            /////////////////////////////////////////////////////

            brd_main.Child = gridContainer;
            #endregion
            if (AppSettings.lang.Equals("en"))
                grid_main.FlowDirection = FlowDirection.LeftToRight;
            else grid_main.FlowDirection = FlowDirection.RightToLeft;
            #region   Title
            var titleText = new TextBlock();
            titleText.Text = title;
            titleText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#6e6e6e"));
            titleText.FontSize = 13;
            titleText.Height = 20;
            titleText.FontFamily = App.Current.Resources["Font-cairo-bold"] as FontFamily;
            titleText.Margin = new Thickness(5, 0, 5, 0);
            titleText.FontWeight = FontWeights.Bold;
            titleText.VerticalAlignment = VerticalAlignment.Top;
            titleText.HorizontalAlignment = HorizontalAlignment.Left;
            titleText.TextWrapping = TextWrapping.WrapWithOverflow;
            Grid.SetRow(titleText, 1);
            /////////////////////////////////

            #endregion
            #region  subTitle
            var subTitleText = new TextBlock();
            subTitleText.Text = subTitle;
            subTitleText.Margin = new Thickness(5, 0, 5, 0);
            subTitleText.FontWeight = FontWeights.Regular;
            subTitleText.VerticalAlignment = VerticalAlignment.Top;
            subTitleText.HorizontalAlignment = HorizontalAlignment.Left;
            subTitleText.FontSize = 10;
            subTitleText.Height = 18;
            subTitleText.TextWrapping = TextWrapping.Wrap;
            subTitleText.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#6e6e6e"));
            Grid.SetRow(subTitleText, 2);
            /////////////////////////////////

            #endregion
            #region  subTitle2
            var subTitleText2 = new TextBlock();
            subTitleText2.Text = subTitle2;
            subTitleText2.Margin = new Thickness(5, 0, 5, 0);
            subTitleText2.FontWeight = FontWeights.Regular;
            subTitleText2.VerticalAlignment = VerticalAlignment.Top;
            subTitleText2.HorizontalAlignment = HorizontalAlignment.Left;
            subTitleText2.FontSize = 10;
            subTitleText2.Height = 18;

            subTitleText2.TextWrapping = TextWrapping.Wrap;
            subTitleText2.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#6e6e6e"));
            Grid.SetRow(subTitleText2, 3);
            /////////////////////////////////

            #endregion
            #region Image
            Button buttonImage = new Button();
            buttonImage.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            buttonImage.Height = (gridContainer.Height/1.1) - 7.5;
            buttonImage.Width = ((gridContainer.Width / 2.2)/1.2) - 7.5;
            buttonImage.BorderThickness = new Thickness(0);
            buttonImage.Padding = new Thickness(0);
            buttonImage.FlowDirection = FlowDirection.LeftToRight;
            MaterialDesignThemes.Wpf.ButtonAssist.SetCornerRadius(buttonImage, (new CornerRadius(10)));
            SectionData.getImg(type, Img, buttonImage);
            Grid grid_image = new Grid();
            grid_image.Height = buttonImage.Height - 2;
            grid_image.Width = buttonImage.Width - 1;
            grid_image.VerticalAlignment = VerticalAlignment.Top;
            grid_image.HorizontalAlignment = HorizontalAlignment.Right;
            grid_image.Margin = new Thickness(0,10,10,0);
            Grid.SetRowSpan(grid_image, 5);
            Grid.SetColumn(grid_image, 1);
            grid_image.Children.Add(buttonImage);
            gridContainer.Children.Add(grid_image);

            //////////////
            #endregion

            gridContainer.Children.Add(titleText);
            gridContainer.Children.Add(subTitleText);
            gridContainer.Children.Add(subTitleText2);
        }
        void InitializeControls()
        {
            if (cardViewitem.cardType == "User")
                CreateUserCard(cardViewitem.cardType, cardViewitem.user.name, cardViewitem.user.job, cardViewitem.user.mobile, cardViewitem.user.image);
            else if (cardViewitem.cardType == "Agent")
                CreateUserCard(cardViewitem.cardType, cardViewitem.agent.name, cardViewitem.agent.company, cardViewitem.agent.mobile, cardViewitem.agent.image);
            else
                CreateItemCard();

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DataContext = this;
                InitializeControls();
               
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        #region rectangleCardBorderBrush
        public static readonly DependencyProperty rectangleCardBorderBrushDependencyProperty = DependencyProperty.Register("rectangleCardBorderBrush",
            typeof(string),
            typeof(UC_rectangleCard),
            new PropertyMetadata("DEFAULT"));
        public string rectangleCardBorderBrush
        {
            set
            { SetValue(rectangleCardBorderBrushDependencyProperty, value); }
            get
            { return (string)GetValue(rectangleCardBorderBrushDependencyProperty); }
        }
        #endregion

       

    }
}
