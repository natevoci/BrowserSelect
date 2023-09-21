using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace BrowserSelect {
    public partial class BrowserUCCompact : UserControl, IBrowserUC
    {
        private Browser _browser;
        private int _index = 0;

        private int _mainTextWidth = 0;
        private int _shortcutTextWidth = 0;

        private bool _isHighlighted = false;
        
        public bool IsSelected { get; set; } = false;
        
        public bool Always { get; set; } = false;
        public Browser Browser { get => _browser; }
        public int Index { get => _index; }

        public int MeasuredWidth
        {
            get => ImageLeftOffset + Padding.Left + ImageWidth + Padding.Left + _mainTextWidth + _shortcutTextWidth;
        }
        public int ImageLeftOffset { get; set; } = 30;
        public int ImageWidth
        {
            get => Height - Padding.Top - Padding.Bottom;
        }

        public event EventHandler Selected;


        public BrowserUCCompact(Browser b,int index) {
            this._browser = b;
            this._index = index;

            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            // If no uri is loaded disable the the right click menu
            if (Program.uri == null)
            {
                this.ContextMenuStrip = null;
            }

            this.Padding = new Padding(2);

            // Measure texts
            using (var g = Graphics.FromHwnd(this.Handle))
            {
                DrawMainText(g, true);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var backColor = _isHighlighted ? Color.FromArgb(0x00, 0x6D, 0xB7) : IsSelected ? Color.FromArgb(0xCC, 0xE8, 0xFF) : Color.White;

            using (Brush br = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(br, this.ClientRectangle);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (var image = this._browser.string2Icon())
            {
                e.Graphics.DrawImage(image, new Rectangle(ImageLeftOffset, Padding.Top, ImageWidth, ImageWidth));
            }

            // Draw main text
            DrawMainText(e.Graphics);
        }

        private void DrawMainText(Graphics g, bool measure = false)
        {
            var textX = ImageLeftOffset + Padding.Left + ImageWidth + Padding.Left;
            
            var format = new StringFormat(StringFormatFlags.NoWrap);
            format.LineAlignment = StringAlignment.Center;

            using (var fontMain = new Font(this.Font, FontStyle.Regular))
            {
                using (var fontUser = new Font(this.Font.FontFamily, 9.0f))
                {
                    if (measure)
                    {
                        var size = g.MeasureString(this._browser.name, fontMain, new PointF(0, 0), format);
                        _mainTextWidth = (int)size.Width;

                        if (!string.IsNullOrEmpty(_browser.user))
                        {
                            size = g.MeasureString(_browser.user, fontUser, new PointF(0, 0), format);
                            if (_mainTextWidth < (int)size.Width)
                            {
                                _mainTextWidth = (int)size.Width;
                            }
                        }
                        _mainTextWidth += 10;
                    }
                    else
                    {
                        var foreColor = _isHighlighted ? SystemColors.HighlightText : SystemColors.ControlText;

                        using (Brush br = new SolidBrush(foreColor))
                        {
                            if (!string.IsNullOrEmpty(_browser.user))
                            {
                                var rectMain = new RectangleF(textX, Padding.Top, Width - textX, ImageWidth / 2);
                                format.LineAlignment = StringAlignment.Far;
                                g.DrawString(this._browser.name, fontMain, br, rectMain, format);

                                using (Brush brUser = new SolidBrush(Color.FromArgb(0xA0, foreColor)))
                                {
                                    var rectUser = new RectangleF(textX, Padding.Top + ImageWidth / 2, Width - textX, ImageWidth / 2);
                                    format.LineAlignment = StringAlignment.Near;
                                    g.DrawString(_browser.user, fontUser, brUser, rectUser, format);
                                }
                            }
                            else
                            {
                                var rectMain = new RectangleF(textX, Padding.Top, Width - textX, ImageWidth);
                                g.DrawString(this._browser.name, fontMain, br, rectMain, format);
                            }
                        }
                    }
                }
            }

        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Selected?.Invoke(this, e);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHighlighted = true;
            ForeColor = SystemColors.HighlightText;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHighlighted = false;
            ForeColor = SystemColors.ControlText;
        }

        private void alwaysUseThisBrowserForThisDomainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Always = true;
            Selected?.Invoke(this, e);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            alwaysUseThisBrowserForThisDomainToolStripMenuItem.Text = "Always use " + this._browser.name + " for " + Program.uri.Authority;
        }
    }
}
