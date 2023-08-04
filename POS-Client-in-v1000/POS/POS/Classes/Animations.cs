using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace POS.Classes
{
    class Animations
    {
        public static System.Windows.Media.Color ColorToColor(SolidColorBrush br)
        {
            return System.Windows.Media.Color.FromArgb(br.Color.A, br.Color.R, br.Color.G, br.Color.B);
        }
        public static TranslateTransform borderAnimation(int anim, Border control, Boolean Property)
        {
            Storyboard storyboard = new Storyboard();
            control.Opacity = 0;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = anim;
            myDoubleAnimation.Duration = TimeSpan.FromMilliseconds(500);

            TranslateTransform translateTransform = new TranslateTransform();
            if (Property)
            {
                translateTransform.BeginAnimation(TranslateTransform.XProperty, myDoubleAnimation);
            }
            else
                translateTransform.BeginAnimation(TranslateTransform.YProperty, myDoubleAnimation);
            control.BeginAnimation(UIElement.OpacityProperty, myDoubleAnimation);
            return translateTransform;

        }
        //         <TextBlock.RenderTransform>
        //             <TranslateTransform />
        //         </TextBlock.RenderTransform>
        public static void shakingControl(TextBlock control)
        {

            Storyboard blinkStoryboard = new Storyboard();


            var Animation1 = new DoubleAnimation
            {
                //From = 0,
                To = 7.5,
                Duration = TimeSpan.FromSeconds(0.1),
            };

            var Animation2 = new DoubleAnimation
            {
                //From = 7.5,
                To = -7.5,
                BeginTime = TimeSpan.FromSeconds(0.1),
                Duration = TimeSpan.FromSeconds(0.1)
            };
            var Animation3 = new DoubleAnimation
            {
                //From = -7.5,
                To = 5,
                BeginTime = TimeSpan.FromSeconds(0.2),
                Duration = TimeSpan.FromSeconds(0.1),
            };

            var Animation4 = new DoubleAnimation
            {
                //From =5,
                To = -5,
                BeginTime = TimeSpan.FromSeconds(0.3),
                Duration = TimeSpan.FromSeconds(0.1)
            };

            var Animation5 = new DoubleAnimation
            {
                //From = -2.5,
                To = 2.5,
                BeginTime = TimeSpan.FromSeconds(0.4),
                Duration = TimeSpan.FromSeconds(0.1)
            };
            var Animation6 = new DoubleAnimation
            {
                //From = 2.5,
                To = -2.5,
                BeginTime = TimeSpan.FromSeconds(0.5),
                Duration = TimeSpan.FromSeconds(0.1)
            };
            var Animation7 = new DoubleAnimation
            {
                //From = -2.5,
                To = 0,
                BeginTime = TimeSpan.FromSeconds(0.6),
                Duration = TimeSpan.FromSeconds(0.2)
            };

            Storyboard.SetTarget(Animation1, control);
            Storyboard.SetTarget(Animation2, control);
            Storyboard.SetTarget(Animation3, control);
            Storyboard.SetTarget(Animation4, control);
            Storyboard.SetTarget(Animation5, control);
            Storyboard.SetTarget(Animation6, control);
            Storyboard.SetTarget(Animation7, control);

            PropertyPath TargetPath =
            new PropertyPath("(0).(1)", TextBlock.RenderTransformProperty, TranslateTransform.XProperty);

            Storyboard.SetTargetProperty(Animation1, TargetPath);
            Storyboard.SetTargetProperty(Animation2, TargetPath);
            Storyboard.SetTargetProperty(Animation3, TargetPath);
            Storyboard.SetTargetProperty(Animation4, TargetPath);
            Storyboard.SetTargetProperty(Animation5, TargetPath);
            Storyboard.SetTargetProperty(Animation6, TargetPath);
            Storyboard.SetTargetProperty(Animation7, TargetPath);

            blinkStoryboard.Children.Add(Animation1);
            blinkStoryboard.Children.Add(Animation2);
            blinkStoryboard.Children.Add(Animation3);
            blinkStoryboard.Children.Add(Animation4);
            blinkStoryboard.Children.Add(Animation5);
            blinkStoryboard.Children.Add(Animation6);
            blinkStoryboard.Children.Add(Animation7);

            blinkStoryboard.Begin();
        }
        public static void shakingControl(ComboBox control)
        {

            Storyboard blinkStoryboard = new Storyboard();


            var Animation1 = new DoubleAnimation
            {
                //From = 0,
                To = 7.5,
                Duration = TimeSpan.FromSeconds(0.1),
            };

            var Animation2 = new DoubleAnimation
            {
                //From = 7.5,
                To = -7.5,
                BeginTime = TimeSpan.FromSeconds(0.1),
                Duration = TimeSpan.FromSeconds(0.1)
            };
            var Animation3 = new DoubleAnimation
            {
                //From = -7.5,
                To = 5,
                BeginTime = TimeSpan.FromSeconds(0.2),
                Duration = TimeSpan.FromSeconds(0.1),
            };

            var Animation4 = new DoubleAnimation
            {
                //From =5,
                To = -5,
                BeginTime = TimeSpan.FromSeconds(0.3),
                Duration = TimeSpan.FromSeconds(0.1)
            };

            var Animation5 = new DoubleAnimation
            {
                //From = -2.5,
                To = 2.5,
                BeginTime = TimeSpan.FromSeconds(0.4),
                Duration = TimeSpan.FromSeconds(0.1)
            };
            var Animation6 = new DoubleAnimation
            {
                //From = 2.5,
                To = -2.5,
                BeginTime = TimeSpan.FromSeconds(0.5),
                Duration = TimeSpan.FromSeconds(0.1)
            };
            var Animation7 = new DoubleAnimation
            {
                //From = -2.5,
                To = 0,
                BeginTime = TimeSpan.FromSeconds(0.6),
                Duration = TimeSpan.FromSeconds(0.2)
            };

            Storyboard.SetTarget(Animation1, control);
            Storyboard.SetTarget(Animation2, control);
            Storyboard.SetTarget(Animation3, control);
            Storyboard.SetTarget(Animation4, control);
            Storyboard.SetTarget(Animation5, control);
            Storyboard.SetTarget(Animation6, control);
            Storyboard.SetTarget(Animation7, control);

            PropertyPath TargetPath =
            new PropertyPath("(0).(1)", TextBlock.RenderTransformProperty, TranslateTransform.XProperty);

            Storyboard.SetTargetProperty(Animation1, TargetPath);
            Storyboard.SetTargetProperty(Animation2, TargetPath);
            Storyboard.SetTargetProperty(Animation3, TargetPath);
            Storyboard.SetTargetProperty(Animation4, TargetPath);
            Storyboard.SetTargetProperty(Animation5, TargetPath);
            Storyboard.SetTargetProperty(Animation6, TargetPath);
            Storyboard.SetTargetProperty(Animation7, TargetPath);

            blinkStoryboard.Children.Add(Animation1);
            blinkStoryboard.Children.Add(Animation2);
            blinkStoryboard.Children.Add(Animation3);
            blinkStoryboard.Children.Add(Animation4);
            blinkStoryboard.Children.Add(Animation5);
            blinkStoryboard.Children.Add(Animation6);
            blinkStoryboard.Children.Add(Animation7);

            blinkStoryboard.Begin();
        }

    }
}
