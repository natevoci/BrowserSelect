﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BrowserSelect {
    public partial class BrowserUC : UserControl, IBrowserUC {
        public Browser _browser;

        public bool Always { get; set; } = false;
        public Browser Browser { get => _browser; }

        public BrowserUC(Browser b,int index) {
            InitializeComponent();

            this._browser = b;

            name.Text = b.name;
            shortcuts.Text = "( " + Convert.ToString(index+1) + "," + String.Join(",", b.shortcuts) + " )";
            shortcuts.ForeColor = Color.FromKnownColor(KnownColor.GrayText);
            icon.Image = b.string2Icon();//.ToBitmap();
            icon.SizeMode = PictureBoxSizeMode.Zoom;
        }
        public new event EventHandler Click {
            add {
                base.Click += value;
                foreach (Control control in Controls) {
                    control.Click += value;
                }
            }
            remove {
                base.Click -= value;
                foreach (Control control in Controls) {
                    control.Click -= value;
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Always = true;
        }
    }
}
