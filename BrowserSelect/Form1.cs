﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Windows.Forms;
using BrowserSelect.Properties;
using Newtonsoft.Json;
using SHDocVw;

namespace BrowserSelect
{
    public partial class Form1 : Form
    {
        // get the list of Borwsers from registry and remove the ones unchecked from settings
        List<Browser> browsers;
        List<BrowserUCCompact> _compactControls;

        private ButtonsUC _buc;
        private string _selectedBrowser = "";

        public Form1()
        {
            InitializeComponent();
            MaximizeBox = false;
        }

        public void updateBrowsers()
        {
            if (!Settings.Default.CompactVertical)
            {
                SuspendLayout();
                textBoxFilter.Visible = false;
                this.BackColor = SystemColors.Control;
                browsers = BrowserFinder.find().Where(b => !Settings.Default.HideBrowsers.Contains(b.Identifier)).ToList();
                int i = 0;
                int width = 0;
                for (int k = Controls.Count - 1; k >= 0; k--)
                {
                    Control c = Controls[k];
                    if (c is IBrowserUC)
                        Controls.RemoveAt(k);
                }
                // add browserUC objects to the form
                foreach (var browser in browsers)
                {
                    var bruc = new BrowserUC(browser, i);
                    width = bruc.Width;  // buc.Width = 128*dpi Scale
                    bruc.Left = width * i++;
                    bruc.Click += browser_click;
                    this.Controls.Add(bruc);
                }
                ResumeLayout();
                _buc.Top = 0;
                _buc.Left = i * width;
                btn_help.Left = i * width;
                btn_help.Top = _buc.Height - btn_help.Height;
            }
            else
            {
                SuspendLayout();
                textBoxFilter.Visible = true;
                this.BackColor = Color.White;

                browsers = BrowserFinder.find().Where(b => !Settings.Default.HideBrowsers.Contains(b.Identifier)).ToList();
                if (textBoxFilter.Text.Length > 0)
                {
                    browsers = browsers.Where(b => (
                        b.name.IndexOf(textBoxFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (b.user?.IndexOf(textBoxFilter.Text, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                    )).ToList();
                }
                int i = 0;
                int height = 0;
                for (int k = Controls.Count - 1; k >= 0; k--)
                {
                    Control c = Controls[k];
                    if (c is IBrowserUC)
                        Controls.RemoveAt(k);
                }

                this.AutoSize = true;

                var totalHeight = textBoxFilter.Height;
                bool selectedBrowserFound = false;

                var brucs = new List<BrowserUCCompact>();
                _compactControls = brucs;
                foreach (var browser in browsers)
                {
                    var bruc = new BrowserUCCompact(browser, i);
                    height = bruc.Height;
                    bruc.Top = height * i++ + textBoxFilter.Height;
                    bruc.Selected += browser_click;
                    bruc.BackColor = Color.White;
                    bruc.IsSelected = (browser.Identifier == _selectedBrowser);
                    selectedBrowserFound |= bruc.IsSelected;

                    this.Controls.Add(bruc);
                    brucs.Add(bruc);

                    totalHeight += bruc.Height;
                }

                var maxWidth = brucs.Select(b => b.MeasuredWidth).Max();

                this.Width = maxWidth;

                foreach (var bruc in brucs)
                {
                    bruc.Width = maxWidth;
                }

                if (!selectedBrowserFound && brucs.Count() > 0)
                {
                    var lastUsedBrowserIdentifier = string.Empty;
                    var domain = Program.uri?.Authority;
                    if (domain != null)
                    {
                        var history = string.IsNullOrEmpty(Settings.Default.History) ? new Dictionary<string, string>() : JsonConvert.DeserializeObject<Dictionary<string, string>>(Settings.Default.History);
                        history.TryGetValue(domain, out lastUsedBrowserIdentifier);
                    }

                    BrowserUCCompact lastUsedBrowser = string.IsNullOrEmpty(lastUsedBrowserIdentifier) ? null : brucs.Find(b => b.Browser.Identifier == lastUsedBrowserIdentifier);
                    if (lastUsedBrowser == null)
                        lastUsedBrowser = brucs[0];

                    _selectedBrowser = lastUsedBrowser.Browser.Identifier;
                    lastUsedBrowser.IsSelected = true;
                }

                ResumeLayout();
                _buc.Top = textBoxFilter.Height;
                _buc.Left = 0;
                _buc.BackColor = Color.White;
                btn_help.BackColor = Color.White;
                btn_help.Left = 0;
                btn_help.Top = textBoxFilter.Height + _buc.Height - btn_help.Height;
                this.AutoSize = false;

                var maxHeight = (int)(Screen.FromPoint(new Point(Cursor.Position.X, Cursor.Position.Y)).WorkingArea.Height * 0.8);
                var targetHeight = totalHeight + this.Height - this.ClientSize.Height + 10;
                targetHeight = Math.Max(Math.Min(targetHeight, maxHeight), 280);
                this.Height = targetHeight;

                this.Width = maxWidth + this.Width - this.ClientSize.Width;
            }
            center_me();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.KeyPreview = true;
            // set the form icon from .exe file icon
            this.Icon = IconExtractor.fromFile(Application.ExecutablePath);
            // create a wildcard rule for this domain (always button)
            if (Program.uri != null)
            {
                _alwaysRule = generate_rule(Program.uri);
                this.Text = Program.uri.AbsoluteUri;
            }
            // add vertical buttons to right of form
            _buc = new ButtonsUC(this);
            this.Controls.Add(_buc);
            this.updateBrowsers();
        }

        public void DisplayUpdate()
        {
            Debug.WriteLine("DisplayUpdate");
            btn_help.BackgroundImage = Resources.update_available;
            btn_help.Click -= btn_help_Click;
            btn_help.Click += btn_update_click;
        }

        // struct used to store patterns created for Always button
        public struct AmbiguousRule
        {
            public string tld_rule;
            public string second_rule;
            public int mode;
        }

        private AmbiguousRule _alwaysRule;
        private ContextMenu _alwaysAsk;
        public void add_rule(Browser b)
        {
            if (!Settings.Default.CompactVertical)
            {
                // check if desired pattern is ambiguous
                if (_alwaysRule.mode == 3)
                {
                    // generate a context menu with tld_rule and second_rule as options
                    // and call the overloaded add_rule with pattern as second arg on click
                    _alwaysAsk = new ContextMenu((new[] { _alwaysRule.tld_rule, _alwaysRule.second_rule })
                        .Select(x => new MenuItem(x, (s, e) => add_rule(b, x))).ToArray());
                    // display the context menu at mouse position
                    _alwaysAsk.Show(this, PointToClient(Cursor.Position));
                }
                else if (_alwaysRule.mode == 0)
                {
                    // in case ambiguousness of pattern was not determined, should not happen
                    MessageBox.Show(String.Format("Error while generating pattern from url." +
                        " Please include the following url in your bug report:\n{0}",
                        Program.uri.AbsoluteUri));
                }
                else
                {
                    // in case pattern was not ambiguous, just set the pattern as rule and open the url
                    var pat = (_alwaysRule.mode == 1) ? _alwaysRule.tld_rule : _alwaysRule.second_rule;
                    add_rule(b, pat);
                }
            }
            else
            {
                var domain = Program.uri?.Authority;
                add_rule(b, domain);
            }
        }

        public void add_rule(Browser b, string pattern)
        {
            save_rule(pattern, b);
            open_url(b);
        }

        private void save_rule(string pattern, Browser b)
        {
            // add a rule and save app settings
            DataTable rules;
            if (Settings.Default.Rules != null && Settings.Default.Rules != "" && Settings.Default.Rules != "[]")
                rules = (DataTable)JsonConvert.DeserializeObject(Settings.Default.Rules, (typeof(DataTable)));
            else
            {
                rules = new DataTable();
                rules.Columns.Add("Type");
                rules.Columns.Add("Match");
                rules.Columns.Add("Pattern");
                rules.Columns.Add("Browser");
            }
            if (pattern.StartsWith("*."))
                pattern = pattern.Substring(2);
            rules.Rows.Add("Ends With", "Domain", pattern, b.name);
            Settings.Default.Rules = JsonConvert.SerializeObject(rules);
            Settings.Default.Save();
        }

        public static AmbiguousRule generate_rule(Uri uri)
        {
            /*
            to solve issue #13
            there are a lot of second level domains, e.g. domain.info.au, domain.vic.au, ...
            so we check these rules:
            if url has only two parts (e.g. x.tld or www.x.tld) choose *.x.tld
            else if url has 3 parts or more(e.g. y.x.tld) and y!=www:
                check the following rules: (x = second part after tld)
                    1.(x is part of domain)
                        if len(x) > 4: assume that x is not part of extension, and choose  *.x.tld
                    2.(x is part of extension)
                        if len(x) <=2 (e.g. y.id.au) than choose *.y.x.tld
                        if x is in exceptions (com,net,org,edu,gov,asn.sch) choose *.y.x.tld
                            because many TLD's have second level domains on these, e.g. chap.sch.ir
                        if count(parts)==4 and first part is www: e.g. www.news.com.au, choose *.y.x.tld
                if none of the rules apply, the case is ambiguous, display both options in a context menu.
                    e.g. sealake.vic.au or something.fun.ir
            else if url has only one part (#27):
                add *.x
            */

            // needed variables
            var domain = uri.Authority;
            var parts = domain.Split('.');
            var count = parts.Length;
            var tld = parts.Last();
            var x = "";
            var y = "";
            try
            {
                x = parts[count - 2]; //second-level
                y = parts[count - 3]; //third-level
            }
            catch (IndexOutOfRangeException ex) {
                Debug.WriteLine(ex);
            } // in case domain did not have 3 parts.. (e.g. localhost, google.com)

            // creating the patterns
            var rule_tld = String.Format("*.{0}.{1}", x, tld);
            var rule_second = String.Format("*.{0}.{1}.{2}", y, x, tld);
            var mode = 0; // 0 = error, 1=use rule_tld (*.x.tld), 2=use rule_second (*.y.x.tld), 3=ambiguous

            // this conditions are based on the long comment above
            if (count == 2 || (count == 3 && y == "www"))
                mode = 1;
            else if (count >= 3)
            {
                if (x.Length > 4)
                    mode = 1;
                else if (
                    (x.Length <= 2) ||
                    ((new[] { "com", "net", "org", "edu", "gov", "asn", "sch" }).Contains(x)) ||
                    (count == 4 && parts[0] == "www")
                    )
                    mode = 2;
                else
                    mode = 3;
            }
            else if (count == 1)
            {
                mode = 1;
                rule_tld = "*." + tld;
            }

            return new AmbiguousRule()
            {
                tld_rule = rule_tld,
                second_rule = rule_second,
                mode = mode
            };
        }

        private void browser_click(object sender, EventArgs e)
        {
            // callback for click event inside the browserControls
            IBrowserUC uc;
            if (sender is IBrowserUC)
                uc = (IBrowserUC)sender;
            else if (((Control)sender).Parent is IBrowserUC)
                uc = (IBrowserUC)((Control)sender).Parent;
            else
                throw new Exception("this should not happen");

            // check if Always was clicked
            if (uc.Always)
                add_rule(uc.Browser);
            else if ((ModifierKeys & Keys.Shift) != 0 || (ModifierKeys & Keys.Alt) != 0)    // open in incognito
                open_url(uc.Browser, true, false);
            else
                open_url(uc.Browser);
        }

        public static void open_url(Browser b, bool incognito = false, bool saveHistory = true)
        {
            if (saveHistory)
            {
                var history = string.IsNullOrEmpty(Settings.Default.History) ? new Dictionary<string, string>() : JsonConvert.DeserializeObject<Dictionary<string, string>>(Settings.Default.History);
                var domain = Program.uri?.Authority;
                if (domain != null)
                {
                    history[domain] = b.Identifier;
                    Settings.Default.History = JsonConvert.SerializeObject(history);
                    Settings.Default.Save();
                }
            }

            var url = Program.uri?.AbsoluteUri ?? string.Empty;
            var args = new List<string>();
            if (!string.IsNullOrEmpty(b.additionalArgs))
                args.Add(b.additionalArgs);
            if (incognito)
                args.Add(b.private_arg);
            if (b.exec.ToLower().EndsWith("brave.exe"))
                args.Add("--");
            args.Add(url.Replace("\"", "%22"));

            if (b.exec.EndsWith("iexplore.exe") && !incognito)
            {
                // IE tends to open in a new window instead of a new tab
                // code borrowed from http://stackoverflow.com/a/3713470/1461004
                bool found = false;
                ShellWindows iExplorerInstances = new ShellWindows();
                foreach (InternetExplorer iExplorer in iExplorerInstances)
                {
                    if (iExplorer.Name.EndsWith("Internet Explorer"))
                    {
                        iExplorer.Navigate(url, 0x800);
                        // for issue #10 (bring IE to focus after opening link)
                        ForegroundAgent.RestoreWindow(iExplorer.HWND);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Process.Start(b.exec, Program.Args2Str(args));
                }
            }
            else
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(b.exec);
                // Clicking MS Edge takes more than 4 seconds to load, even with an existing window
                // Disabling UseShellExecute to create the process directly from the browser executable file
                startInfo.UseShellExecute = false;
                startInfo.Arguments = Program.Args2Str(args);
                Process.Start(startInfo);
            }
            Application.Exit();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Settings.Default.CompactVertical)
            {
                int i = 1;
                foreach (var browser in browsers)
                {
                    if (browser.shortcuts.Contains(e.KeyChar) || e.KeyChar == (Convert.ToString(i++))[0])
                    {
                        open_url(browser);
                        return;
                    }
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Application.Exit();

            if (Settings.Default.CompactVertical)
            {
                if ((e.KeyCode == Keys.Down) || (e.KeyCode == Keys.Up))
                {
                    var selectedIndex = _compactControls.FindIndex(bruc => bruc.IsSelected);

                    var targetIndex = (e.KeyCode == Keys.Down) ? selectedIndex + 1 : selectedIndex - 1;
                    targetIndex = Math.Min(targetIndex, _compactControls.Count - 1);
                    targetIndex = Math.Max(targetIndex, 0);

                    if (targetIndex != selectedIndex)
                    {
                        var selectedControl = _compactControls[selectedIndex];
                        var targetControl = _compactControls[targetIndex];
                        _selectedBrowser = targetControl.Browser.Identifier;
                        selectedControl.IsSelected = false;
                        targetControl.IsSelected = true;
                        selectedControl.Invalidate();
                        targetControl.Invalidate();
                        this.ScrollControlIntoView(targetControl);
                    }
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.Enter)
                {
                    var selectedIndex = _compactControls.FindIndex(bruc => bruc.IsSelected);
                    if (selectedIndex >= 0)
                    {
                        open_url(_compactControls[selectedIndex].Browser);
                    }
                    e.Handled = true;
                }
            }
        }

        private void center_me()
        {
            var wa = Screen.FromPoint(new Point(Cursor.Position.X, Cursor.Position.Y)).WorkingArea;
            var left = wa.Width / 2 + wa.Left - Width / 2;
            var top = wa.Height / 2 + wa.Top - Height / 2;

            this.Location = new Point(left, top);

            
            // Borrowed from https://stackoverflow.com/a/5853542
            // Get the window to the front
            this.TopMost = true;
            this.TopMost = false;

            // "Steal" the focus
            this.Activate();            
        }

        private void btn_help_Click(object sender, EventArgs e)
        {
            (new frm_help_main()).ShowDialog();
        }

        void btn_update_click(object sender, EventArgs e)
        {
            Program.UpdateDialog();
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e)
        {
            updateBrowsers();
        }
    }
}
