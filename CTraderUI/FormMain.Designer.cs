namespace CTraderUI
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBoxTop = new System.Windows.Forms.GroupBox();
            this.groupBoxAccount = new System.Windows.Forms.GroupBox();
            this.textBoxFairPrice = new System.Windows.Forms.TextBox();
            this.labelFairPrice = new System.Windows.Forms.Label();
            this.groupBoxTarget = new System.Windows.Forms.GroupBox();
            this.textBoxTarget = new System.Windows.Forms.TextBox();
            this.contextMenuStripTarget = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelTarget = new System.Windows.Forms.Label();
            this.textBoxAvailable = new System.Windows.Forms.TextBox();
            this.labelAvailable = new System.Windows.Forms.Label();
            this.textBoxTotal = new System.Windows.Forms.TextBox();
            this.labelTotal = new System.Windows.Forms.Label();
            this.dataGridViewXX = new System.Windows.Forms.DataGridView();
            this.dataGridViewETH = new System.Windows.Forms.DataGridView();
            this.dataGridViewXBT = new System.Windows.Forms.DataGridView();
            this.groupBoxButtons = new System.Windows.Forms.GroupBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonRebalance = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.dataGridViewTrades = new System.Windows.Forms.DataGridView();
            this.dataGridViewPositions = new System.Windows.Forms.DataGridView();
            this.textBoxTimeLeft = new System.Windows.Forms.TextBox();
            this.labelTimeLeft = new System.Windows.Forms.Label();
            this.groupBoxTop.SuspendLayout();
            this.groupBoxAccount.SuspendLayout();
            this.groupBoxTarget.SuspendLayout();
            this.contextMenuStripTarget.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewXX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewETH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewXBT)).BeginInit();
            this.groupBoxButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTrades)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPositions)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxTop
            // 
            this.groupBoxTop.Controls.Add(this.groupBoxAccount);
            this.groupBoxTop.Controls.Add(this.dataGridViewXX);
            this.groupBoxTop.Controls.Add(this.dataGridViewETH);
            this.groupBoxTop.Controls.Add(this.dataGridViewXBT);
            this.groupBoxTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxTop.Location = new System.Drawing.Point(0, 0);
            this.groupBoxTop.Name = "groupBoxTop";
            this.groupBoxTop.Size = new System.Drawing.Size(927, 184);
            this.groupBoxTop.TabIndex = 0;
            this.groupBoxTop.TabStop = false;
            // 
            // groupBoxAccount
            // 
            this.groupBoxAccount.Controls.Add(this.textBoxFairPrice);
            this.groupBoxAccount.Controls.Add(this.labelFairPrice);
            this.groupBoxAccount.Controls.Add(this.groupBoxTarget);
            this.groupBoxAccount.Controls.Add(this.textBoxAvailable);
            this.groupBoxAccount.Controls.Add(this.labelAvailable);
            this.groupBoxAccount.Controls.Add(this.textBoxTotal);
            this.groupBoxAccount.Controls.Add(this.labelTotal);
            this.groupBoxAccount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxAccount.Location = new System.Drawing.Point(723, 16);
            this.groupBoxAccount.Name = "groupBoxAccount";
            this.groupBoxAccount.Size = new System.Drawing.Size(201, 165);
            this.groupBoxAccount.TabIndex = 3;
            this.groupBoxAccount.TabStop = false;
            // 
            // textBoxFairPrice
            // 
            this.textBoxFairPrice.Location = new System.Drawing.Point(76, 65);
            this.textBoxFairPrice.Name = "textBoxFairPrice";
            this.textBoxFairPrice.ReadOnly = true;
            this.textBoxFairPrice.Size = new System.Drawing.Size(115, 20);
            this.textBoxFairPrice.TabIndex = 6;
            // 
            // labelFairPrice
            // 
            this.labelFairPrice.AutoSize = true;
            this.labelFairPrice.Location = new System.Drawing.Point(6, 68);
            this.labelFairPrice.Name = "labelFairPrice";
            this.labelFairPrice.Size = new System.Drawing.Size(51, 13);
            this.labelFairPrice.TabIndex = 5;
            this.labelFairPrice.Text = "Fair Price";
            // 
            // groupBoxTarget
            // 
            this.groupBoxTarget.Controls.Add(this.textBoxTimeLeft);
            this.groupBoxTarget.Controls.Add(this.labelTimeLeft);
            this.groupBoxTarget.Controls.Add(this.textBoxTarget);
            this.groupBoxTarget.Controls.Add(this.labelTarget);
            this.groupBoxTarget.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBoxTarget.Location = new System.Drawing.Point(3, 90);
            this.groupBoxTarget.Name = "groupBoxTarget";
            this.groupBoxTarget.Size = new System.Drawing.Size(195, 72);
            this.groupBoxTarget.TabIndex = 4;
            this.groupBoxTarget.TabStop = false;
            // 
            // textBoxTarget
            // 
            this.textBoxTarget.ContextMenuStrip = this.contextMenuStripTarget;
            this.textBoxTarget.Location = new System.Drawing.Point(74, 17);
            this.textBoxTarget.Name = "textBoxTarget";
            this.textBoxTarget.ReadOnly = true;
            this.textBoxTarget.Size = new System.Drawing.Size(115, 20);
            this.textBoxTarget.TabIndex = 6;
            // 
            // contextMenuStripTarget
            // 
            this.contextMenuStripTarget.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem});
            this.contextMenuStripTarget.Name = "contextMenuStripTarget";
            this.contextMenuStripTarget.Size = new System.Drawing.Size(95, 26);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(94, 22);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // labelTarget
            // 
            this.labelTarget.AutoSize = true;
            this.labelTarget.Location = new System.Drawing.Point(4, 20);
            this.labelTarget.Name = "labelTarget";
            this.labelTarget.Size = new System.Drawing.Size(38, 13);
            this.labelTarget.TabIndex = 5;
            this.labelTarget.Text = "Target";
            // 
            // textBoxAvailable
            // 
            this.textBoxAvailable.Location = new System.Drawing.Point(76, 39);
            this.textBoxAvailable.Name = "textBoxAvailable";
            this.textBoxAvailable.ReadOnly = true;
            this.textBoxAvailable.Size = new System.Drawing.Size(115, 20);
            this.textBoxAvailable.TabIndex = 3;
            // 
            // labelAvailable
            // 
            this.labelAvailable.AutoSize = true;
            this.labelAvailable.Location = new System.Drawing.Point(6, 42);
            this.labelAvailable.Name = "labelAvailable";
            this.labelAvailable.Size = new System.Drawing.Size(50, 13);
            this.labelAvailable.TabIndex = 2;
            this.labelAvailable.Text = "Available";
            // 
            // textBoxTotal
            // 
            this.textBoxTotal.Location = new System.Drawing.Point(76, 13);
            this.textBoxTotal.Name = "textBoxTotal";
            this.textBoxTotal.ReadOnly = true;
            this.textBoxTotal.Size = new System.Drawing.Size(115, 20);
            this.textBoxTotal.TabIndex = 1;
            // 
            // labelTotal
            // 
            this.labelTotal.AutoSize = true;
            this.labelTotal.Location = new System.Drawing.Point(6, 16);
            this.labelTotal.Name = "labelTotal";
            this.labelTotal.Size = new System.Drawing.Size(31, 13);
            this.labelTotal.TabIndex = 0;
            this.labelTotal.Text = "Total";
            // 
            // dataGridViewXX
            // 
            this.dataGridViewXX.AllowUserToAddRows = false;
            this.dataGridViewXX.AllowUserToDeleteRows = false;
            this.dataGridViewXX.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridViewXX.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridViewXX.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewXX.Dock = System.Windows.Forms.DockStyle.Left;
            this.dataGridViewXX.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewXX.Location = new System.Drawing.Point(483, 16);
            this.dataGridViewXX.Name = "dataGridViewXX";
            this.dataGridViewXX.Size = new System.Drawing.Size(240, 165);
            this.dataGridViewXX.TabIndex = 2;
            // 
            // dataGridViewETH
            // 
            this.dataGridViewETH.AllowUserToAddRows = false;
            this.dataGridViewETH.AllowUserToDeleteRows = false;
            this.dataGridViewETH.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridViewETH.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridViewETH.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewETH.Dock = System.Windows.Forms.DockStyle.Left;
            this.dataGridViewETH.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewETH.Location = new System.Drawing.Point(243, 16);
            this.dataGridViewETH.Name = "dataGridViewETH";
            this.dataGridViewETH.Size = new System.Drawing.Size(240, 165);
            this.dataGridViewETH.TabIndex = 1;
            // 
            // dataGridViewXBT
            // 
            this.dataGridViewXBT.AllowUserToAddRows = false;
            this.dataGridViewXBT.AllowUserToDeleteRows = false;
            this.dataGridViewXBT.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridViewXBT.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridViewXBT.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewXBT.Dock = System.Windows.Forms.DockStyle.Left;
            this.dataGridViewXBT.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewXBT.Location = new System.Drawing.Point(3, 16);
            this.dataGridViewXBT.Name = "dataGridViewXBT";
            this.dataGridViewXBT.Size = new System.Drawing.Size(240, 165);
            this.dataGridViewXBT.TabIndex = 0;
            // 
            // groupBoxButtons
            // 
            this.groupBoxButtons.Controls.Add(this.buttonConnect);
            this.groupBoxButtons.Controls.Add(this.buttonRebalance);
            this.groupBoxButtons.Controls.Add(this.buttonStop);
            this.groupBoxButtons.Controls.Add(this.buttonStart);
            this.groupBoxButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBoxButtons.Location = new System.Drawing.Point(0, 444);
            this.groupBoxButtons.Name = "groupBoxButtons";
            this.groupBoxButtons.Size = new System.Drawing.Size(927, 66);
            this.groupBoxButtons.TabIndex = 1;
            this.groupBoxButtons.TabStop = false;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(172, 19);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(91, 37);
            this.buttonConnect.TabIndex = 3;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // buttonRebalance
            // 
            this.buttonRebalance.Location = new System.Drawing.Point(682, 19);
            this.buttonRebalance.Name = "buttonRebalance";
            this.buttonRebalance.Size = new System.Drawing.Size(91, 37);
            this.buttonRebalance.TabIndex = 2;
            this.buttonRebalance.Text = "Rebalance";
            this.buttonRebalance.UseVisualStyleBackColor = true;
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(500, 19);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(91, 37);
            this.buttonStop.TabIndex = 1;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(325, 19);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(91, 37);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            // 
            // dataGridViewTrades
            // 
            this.dataGridViewTrades.AllowUserToAddRows = false;
            this.dataGridViewTrades.AllowUserToDeleteRows = false;
            this.dataGridViewTrades.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridViewTrades.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridViewTrades.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTrades.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dataGridViewTrades.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewTrades.Location = new System.Drawing.Point(0, 304);
            this.dataGridViewTrades.Name = "dataGridViewTrades";
            this.dataGridViewTrades.Size = new System.Drawing.Size(927, 140);
            this.dataGridViewTrades.TabIndex = 3;
            // 
            // dataGridViewPositions
            // 
            this.dataGridViewPositions.AllowUserToAddRows = false;
            this.dataGridViewPositions.AllowUserToDeleteRows = false;
            this.dataGridViewPositions.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridViewPositions.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridViewPositions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPositions.Dock = System.Windows.Forms.DockStyle.Top;
            this.dataGridViewPositions.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewPositions.Location = new System.Drawing.Point(0, 184);
            this.dataGridViewPositions.Name = "dataGridViewPositions";
            this.dataGridViewPositions.Size = new System.Drawing.Size(927, 114);
            this.dataGridViewPositions.TabIndex = 4;
            // 
            // textBoxTimeLeft
            // 
            this.textBoxTimeLeft.ContextMenuStrip = this.contextMenuStripTarget;
            this.textBoxTimeLeft.Location = new System.Drawing.Point(74, 43);
            this.textBoxTimeLeft.Name = "textBoxTimeLeft";
            this.textBoxTimeLeft.ReadOnly = true;
            this.textBoxTimeLeft.Size = new System.Drawing.Size(115, 20);
            this.textBoxTimeLeft.TabIndex = 8;
            // 
            // labelTimeLeft
            // 
            this.labelTimeLeft.AutoSize = true;
            this.labelTimeLeft.Location = new System.Drawing.Point(4, 46);
            this.labelTimeLeft.Name = "labelTimeLeft";
            this.labelTimeLeft.Size = new System.Drawing.Size(47, 13);
            this.labelTimeLeft.TabIndex = 7;
            this.labelTimeLeft.Text = "Time left";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(927, 510);
            this.Controls.Add(this.dataGridViewPositions);
            this.Controls.Add(this.dataGridViewTrades);
            this.Controls.Add(this.groupBoxButtons);
            this.Controls.Add(this.groupBoxTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormMain";
            this.Text = "CTrader";
            this.groupBoxTop.ResumeLayout(false);
            this.groupBoxAccount.ResumeLayout(false);
            this.groupBoxAccount.PerformLayout();
            this.groupBoxTarget.ResumeLayout(false);
            this.groupBoxTarget.PerformLayout();
            this.contextMenuStripTarget.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewXX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewETH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewXBT)).EndInit();
            this.groupBoxButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTrades)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPositions)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxTop;
        private System.Windows.Forms.GroupBox groupBoxAccount;
        private System.Windows.Forms.GroupBox groupBoxTarget;
        private System.Windows.Forms.TextBox textBoxTarget;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTarget;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.Label labelTarget;
        private System.Windows.Forms.TextBox textBoxAvailable;
        private System.Windows.Forms.Label labelAvailable;
        private System.Windows.Forms.TextBox textBoxTotal;
        private System.Windows.Forms.Label labelTotal;
        private System.Windows.Forms.DataGridView dataGridViewXX;
        private System.Windows.Forms.DataGridView dataGridViewETH;
        private System.Windows.Forms.DataGridView dataGridViewXBT;
        private System.Windows.Forms.GroupBox groupBoxButtons;
        private System.Windows.Forms.Button buttonRebalance;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.DataGridView dataGridViewTrades;
        private System.Windows.Forms.DataGridView dataGridViewPositions;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.TextBox textBoxFairPrice;
        private System.Windows.Forms.Label labelFairPrice;
        private System.Windows.Forms.TextBox textBoxTimeLeft;
        private System.Windows.Forms.Label labelTimeLeft;
    }
}

