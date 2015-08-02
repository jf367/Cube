using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Cube
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const float DEFAULT_LENGTH = 200.0f;
        private static readonly Color DEFAULT_COLOR = Colors.Black;
        private const float DEFAULT_X = 200.0f;
        private const float DEFAULT_Y = 100.0f;

        private Matrix4x4 currentTransformation;

        private float startPointerX;
        private float startPointerY;
        private Matrix4x4 startTransformation;

        public MainPage()
        {
            currentTransformation = Matrix4x4.Identity;

            this.InitializeComponent();
        }


        private void MainCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Center();
        }

        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            CubeHelper helper = new CubeHelper(GetLength());
            foreach (Tuple<Vector2, Vector2> edge in helper.GetEdges(currentTransformation))
            {
                ColorValue colorValue = ColorComboBox.SelectedValue as ColorValue;
                Color color = (colorValue == null ? Colors.Black : colorValue.Color);
                args.DrawingSession.DrawLine(edge.Item1, edge.Item2, color);
            }
        }

        private void LengthTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            LengthTextBox.Text = DEFAULT_LENGTH.ToString(CultureInfo.CurrentCulture);
        }

        private void LengthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainCanvas.Invalidate();
        }

        private void ColorComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<ColorValue> colors = new List<ColorValue>(typeof(Colors).GetRuntimeProperties().Select(p => new ColorValue((Color)p.GetValue(null), p.Name)).Cast<ColorValue>());
            ColorComboBox.ItemsSource = colors;
            foreach (ColorValue color in colors)
            {
                if (color.Color == DEFAULT_COLOR)
                {
                    ColorComboBox.SelectedValue = color;
                    break;
                }
            }
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainCanvas.Invalidate();
        }

        private void TransformationComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<String> transforms = new List<String>(Enum.GetNames(typeof(Transformation)));
            TransformationComboBox.ItemsSource = transforms;
            TransformationComboBox.SelectedValue = transforms[0];
        }

        private void MainCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            StartTracking(e);
        }

        private void MainCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            StopTracking(e);
        }

        private void MainCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact)
            {
                Transformation transformation;
                if (!Enum.TryParse((string)TransformationComboBox.SelectedValue, out transformation))
                {
                    return;
                }

                Point p = e.GetCurrentPoint(MainCanvas).Position;
                switch (transformation)
                {
                    case Transformation.Translate:
                        Translate(p);
                        break;

                    case Transformation.Rotate:
                        Rotate(p);
                        break;

                    case Transformation.Scale:
                        Scale(p);
                        break;

                    default:
                        Debug.Assert(false, "Unknown transformation: " + TransformationComboBox.SelectedValue);
                        break;
                }

                MainCanvas.Invalidate();
            }
        }

        private void MainCanvas_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            StartTracking(e);
        }

        private void MainCanvas_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            StopTracking(e);
        }

        private void MainCanvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Center();
        }

        private void StartTracking(PointerRoutedEventArgs e)
        {
            Point p = e.GetCurrentPoint(MainCanvas).Position;
            startPointerX = (float)p.X;
            startPointerY = (float)p.Y;
            startTransformation = currentTransformation;
        }

        private void StopTracking(PointerRoutedEventArgs e)
        {
        }

        private float GetLength()
        {
            float result;
            if (float.TryParse(LengthTextBox.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out result) &&
                result > 0.0f)
            {
                return result;
            }

            return DEFAULT_LENGTH;
        }

        private void Center()
        {
            currentTransformation = new Matrix4x4(
                1.0f, 0.0f, 0.0f, ((float)MainCanvas.ActualWidth - GetLength()) / 2,
                0.0f, 1.0f, 0.0f, ((float)MainCanvas.ActualHeight - GetLength()) / 2,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );
            MainCanvas.Invalidate();
        }

        private void Translate(Point cursorPoint)
        {
            Matrix4x4 translate = new Matrix4x4(
                1.0f, 0.0f, 0.0f, (float)cursorPoint.X - startPointerX,
                0.0f, 1.0f, 0.0f, (float)cursorPoint.Y - startPointerY,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );

            currentTransformation = Matrix4x4.Multiply(translate, startTransformation);
        }

        private void Rotate(Point cursorPoint)
        {
            Vector2 origin = new Vector2(
                (float)MainCanvas.ActualWidth / 2,
                (float)MainCanvas.ActualHeight / 2
            );

            double startNorm = Math.Sqrt((startPointerX - origin.X) * (startPointerX - origin.X) + (startPointerY - origin.Y) * (startPointerY - origin.Y));
            double cursorNorm = Math.Sqrt((cursorPoint.X - origin.X) * (cursorPoint.X - origin.X) + (cursorPoint.Y - origin.Y) * (cursorPoint.Y - origin.Y));
            if (startNorm < 1e-6 || cursorNorm < 1e-6)
            {
                return;
            }

            double startCos = (startPointerX - origin.X) / startNorm;
            double startSin = (startPointerY - origin.Y) / startNorm;
            double cursorCos = (cursorPoint.X - origin.X) / cursorNorm;
            double cursorSin = (cursorPoint.Y - origin.Y) / cursorNorm;

            float cosDiff = (float)(cursorCos * startCos + cursorSin * startSin);
            float sinDiff = (float)(cursorSin * startCos - startSin * cursorCos);
            Matrix4x4 rotate = new Matrix4x4(
               cosDiff, -sinDiff, 0.0f, 0.0f,
               sinDiff, cosDiff, 0.0f, 0.0f,
               0.0f, 0.0f, 1.0f, 0.0f,
               0.0f, 0.0f, 0.0f, 1.0f 
            );

            Matrix4x4 reverseTranslate = new Matrix4x4(
                1.0f, 0.0f, 0.0f, -origin.X,
                0.0f, 1.0f, 0.0f, -origin.Y,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );

            Matrix4x4 translate = new Matrix4x4(
                1.0f, 0.0f, 0.0f, origin.X,
                0.0f, 1.0f, 0.0f, origin.Y,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );

            currentTransformation = Matrix4x4.Multiply(translate, Matrix4x4.Multiply(rotate, Matrix4x4.Multiply(reverseTranslate, startTransformation)));
        }

        private void Scale(Point cursorPoint)
        {
            Vector2 origin = new Vector2(
                (float)MainCanvas.ActualWidth / 2,
                (float)MainCanvas.ActualHeight / 2
            );

            double startNorm = Math.Sqrt((startPointerX - origin.X) * (startPointerX - origin.X) + (startPointerY - origin.Y) * (startPointerY - origin.Y));
            double cursorNorm = Math.Sqrt((cursorPoint.X - origin.X) * (cursorPoint.X - origin.X) + (cursorPoint.Y - origin.Y) * (cursorPoint.Y - origin.Y));
            if (startNorm < 1e-6 || cursorNorm < 1e-6)
            {
                return;
            }

            Matrix4x4 scale = new Matrix4x4(
               (float)(cursorNorm / startNorm), 0.0f, 0.0f, 0.0f,
               0.0f, (float)(cursorNorm / startNorm), 0.0f, 0.0f,
               0.0f, 0.0f, 1.0f, 0.0f,
               0.0f, 0.0f, 0.0f, 1.0f
            );

            Matrix4x4 reverseTranslate = new Matrix4x4(
                1.0f, 0.0f, 0.0f, -origin.X,
                0.0f, 1.0f, 0.0f, -origin.Y,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );

            Matrix4x4 translate = new Matrix4x4(
                1.0f, 0.0f, 0.0f, origin.X,
                0.0f, 1.0f, 0.0f, origin.Y,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );

            currentTransformation = Matrix4x4.Multiply(translate, Matrix4x4.Multiply(scale, Matrix4x4.Multiply(reverseTranslate, startTransformation)));
        }
    }
}
