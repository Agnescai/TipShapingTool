using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;


using System.Windows.Controls;
using System.Windows.Data;


namespace TipShaping
{
    public class CrosshairImage : Image
    {
        public static readonly DependencyProperty CrosshairVisibilityProperty = DependencyProperty.Register("CrosshairVisibility", typeof(bool), typeof(CrosshairImage), new PropertyMetadata(true));
        public bool CrosshairVisibility
        {
            get { return (bool)GetValue(CrosshairVisibilityProperty); }
            set { SetValue(CrosshairVisibilityProperty, value); }
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (!CrosshairVisibility)
            {
                return;
            }

            // Calculate the center of the control
            double centerX = this.ActualWidth / 2;
            double centerY = this.ActualHeight / 2;

            // Create a pen for drawing the crosshair
            Pen pen = new Pen(Brushes.Blue, 1);

            // Draw the crosshair lines
            drawingContext.DrawLine(pen, new Point(centerX, 0), new Point(centerX, this.ActualHeight));
            drawingContext.DrawLine(pen, new Point(0, centerY), new Point(this.ActualWidth, centerY));
        }
    }
}
