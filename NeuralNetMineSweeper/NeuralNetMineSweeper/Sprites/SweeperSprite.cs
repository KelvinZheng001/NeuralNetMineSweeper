using Microsoft.Expression.Shapes;
using NeuralNetMineSweeper.NeuronClasses;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeuralNetMineSweeper.Sprites
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:NeuralNetMineSweeper.Sprites"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:NeuralNetMineSweeper.Sprites;assembly=NeuralNetMineSweeper.Sprites"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:SweeperSprite/>
    ///
    /// </summary>
    public class SweeperSprite : Control
    {
        static SweeperSprite()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SweeperSprite), new FrameworkPropertyMetadata(typeof(SweeperSprite)));
        }


        private double _Fitness;

        public double Fitness
        {
            get { return _Fitness; }
            set
            {
                _Fitness = value;
                if (_arrow != null)
                {
                    _arrow.StrokeThickness = 1 + 0.05 * _Fitness;
                }
            }
        }
        BlockArrow _arrow;
        Line _targetLine;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _arrow = GetTemplateChild("Arrow") as BlockArrow;
            _targetLine = GetTemplateChild("targetLine") as Line;

        }

        public void Render(MineSweeper sweeper)
        {
            TranslateTransform translateTransform = new TranslateTransform(sweeper.Position.X, sweeper.Position.Y);
            RotateTransform rotateTransform = new RotateTransform(sweeper.directionAngle);

            TransformGroup transformGroup = new TransformGroup();

            transformGroup.Children.Add(rotateTransform);
            transformGroup.Children.Add(translateTransform);

            _arrow.RenderTransform = transformGroup;
            Fitness = sweeper.Fitness;
            Vector targetVector = new Vector();
            if (sweeper.Target != null)
            {
                targetVector = sweeper.Position - sweeper.Target.Postion;
            }

            var scallTransform = new ScaleTransform(targetVector.Length, 1);
            double angle = Vector.AngleBetween(targetVector, new Vector(0, 1));
            rotateTransform = new RotateTransform(-(angle - 90) + 180);

            transformGroup = new TransformGroup();
            transformGroup.Children.Add(scallTransform);
            transformGroup.Children.Add(rotateTransform);
            transformGroup.Children.Add(translateTransform);

            _targetLine.RenderTransform = transformGroup;


        }
    }
}
