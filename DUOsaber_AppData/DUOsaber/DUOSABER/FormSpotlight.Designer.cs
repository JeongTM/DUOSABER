namespace DUOsaber
{
    partial class FormSpotlight
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
            this.timerSpotlight = new System.Windows.Forms.Timer(this.components);
            this.ovalPictureBox3 = new DUOsaber.OvalPictureBox();
            this.ovalPictureBox2 = new DUOsaber.OvalPictureBox();
            this.ovalPictureBox1 = new DUOsaber.OvalPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.ovalPictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ovalPictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ovalPictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // timerSpotlight
            // 
            this.timerSpotlight.Interval = 10;
            this.timerSpotlight.Tick += new System.EventHandler(this.timerSpotlight_Tick);
            // 
            // ovalPictureBox3
            // 
            this.ovalPictureBox3.BackColor = System.Drawing.Color.Red;
            this.ovalPictureBox3.Enabled = false;
            this.ovalPictureBox3.Location = new System.Drawing.Point(473, 182);
            this.ovalPictureBox3.Name = "ovalPictureBox3";
            this.ovalPictureBox3.Size = new System.Drawing.Size(100, 100);
            this.ovalPictureBox3.TabIndex = 5;
            this.ovalPictureBox3.TabStop = false;
            this.ovalPictureBox3.Visible = false;
            // 
            // ovalPictureBox2
            // 
            this.ovalPictureBox2.BackColor = System.Drawing.Color.Red;
            this.ovalPictureBox2.Enabled = false;
            this.ovalPictureBox2.Location = new System.Drawing.Point(347, 182);
            this.ovalPictureBox2.Name = "ovalPictureBox2";
            this.ovalPictureBox2.Size = new System.Drawing.Size(100, 100);
            this.ovalPictureBox2.TabIndex = 4;
            this.ovalPictureBox2.TabStop = false;
            this.ovalPictureBox2.Visible = false;
            // 
            // ovalPictureBox1
            // 
            this.ovalPictureBox1.BackColor = System.Drawing.Color.Red;
            this.ovalPictureBox1.Enabled = false;
            this.ovalPictureBox1.Location = new System.Drawing.Point(218, 182);
            this.ovalPictureBox1.Name = "ovalPictureBox1";
            this.ovalPictureBox1.Size = new System.Drawing.Size(100, 100);
            this.ovalPictureBox1.TabIndex = 3;
            this.ovalPictureBox1.TabStop = false;
            this.ovalPictureBox1.Visible = false;
            // 
            // FormSpotlight
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ovalPictureBox3);
            this.Controls.Add(this.ovalPictureBox2);
            this.Controls.Add(this.ovalPictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSpotlight";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "FormSpotlight";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Red;
            ((System.ComponentModel.ISupportInitialize)(this.ovalPictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ovalPictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ovalPictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private OvalPictureBox ovalPictureBox1;
        private OvalPictureBox ovalPictureBox2;
        private OvalPictureBox ovalPictureBox3;
        public System.Windows.Forms.Timer timerSpotlight;
    }
}