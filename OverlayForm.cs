using System;
using System.Drawing;
using System.Windows.Forms;

namespace coppy
{
    public partial class OverlayForm : Form
    {
        private Point startPoint;
        private Rectangle selection;
        private bool isDragging = false;

        public Rectangle SelectedBounds => selection;

        public OverlayForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.Cursor = Cursors.Cross;
            this.BackColor = Color.Black;
            this.Opacity = 0.4;
            this.DoubleBuffered = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                startPoint = e.Location;
                selection = new Rectangle(e.Location, new Size(0, 0));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDragging)
            {
                int x = Math.Min(startPoint.X, e.X);
                int y = Math.Min(startPoint.Y, e.Y);
                int width = Math.Abs(startPoint.X - e.X);
                int height = Math.Abs(startPoint.Y - e.Y);
                selection = new Rectangle(x, y, width, height);
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (selection.Width > 0 && selection.Height > 0)
            {
                e.Graphics.FillRectangle(Brushes.White, selection);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}