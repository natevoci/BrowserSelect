namespace BrowserSelect {
    partial class BrowserUCCompact {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.alwaysUseThisBrowserForThisDomainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.alwaysUseThisBrowserForThisDomainToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(284, 48);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // alwaysUseThisBrowserForThisDomainToolStripMenuItem
            // 
            this.alwaysUseThisBrowserForThisDomainToolStripMenuItem.Name = "alwaysUseThisBrowserForThisDomainToolStripMenuItem";
            this.alwaysUseThisBrowserForThisDomainToolStripMenuItem.Size = new System.Drawing.Size(283, 22);
            this.alwaysUseThisBrowserForThisDomainToolStripMenuItem.Text = "Always use this browser for this domain";
            this.alwaysUseThisBrowserForThisDomainToolStripMenuItem.Click += new System.EventHandler(this.alwaysUseThisBrowserForThisDomainToolStripMenuItem_Click);
            // 
            // BrowserUCCompact
            // 
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "BrowserUCCompact";
            this.Size = new System.Drawing.Size(256, 52);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem alwaysUseThisBrowserForThisDomainToolStripMenuItem;
    }
}
