namespace KansasState.Ssw.AdapterApp
{
    partial class FormHome
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
            this.buttonTestMatlab = new System.Windows.Forms.Button();
            this.buttonTestPython = new System.Windows.Forms.Button();
            this.buttonTestScilab = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonTestMatlab
            // 
            this.buttonTestMatlab.Location = new System.Drawing.Point(36, 17);
            this.buttonTestMatlab.Name = "buttonTestMatlab";
            this.buttonTestMatlab.Size = new System.Drawing.Size(134, 40);
            this.buttonTestMatlab.TabIndex = 0;
            this.buttonTestMatlab.Text = "Test Matlab";
            this.buttonTestMatlab.UseVisualStyleBackColor = true;
            this.buttonTestMatlab.Click += new System.EventHandler(this.buttonTestMatlabClick);
            // 
            // buttonTestPython
            // 
            this.buttonTestPython.Location = new System.Drawing.Point(36, 63);
            this.buttonTestPython.Name = "buttonTestPython";
            this.buttonTestPython.Size = new System.Drawing.Size(134, 40);
            this.buttonTestPython.TabIndex = 1;
            this.buttonTestPython.Text = "Test Python";
            this.buttonTestPython.UseVisualStyleBackColor = true;
            this.buttonTestPython.Click += new System.EventHandler(this.buttonTestPythonClick);
            // 
            // buttonTestScilab
            // 
            this.buttonTestScilab.Location = new System.Drawing.Point(36, 109);
            this.buttonTestScilab.Name = "buttonTestScilab";
            this.buttonTestScilab.Size = new System.Drawing.Size(134, 40);
            this.buttonTestScilab.TabIndex = 2;
            this.buttonTestScilab.Text = "Test Scilab";
            this.buttonTestScilab.UseVisualStyleBackColor = true;
            this.buttonTestScilab.Click += new System.EventHandler(this.buttonTestScilabClick);
            // 
            // FormHome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(203, 167);
            this.Controls.Add(this.buttonTestScilab);
            this.Controls.Add(this.buttonTestPython);
            this.Controls.Add(this.buttonTestMatlab);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormHome";
            this.Text = "Adapter Test App";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonTestMatlab;
        private System.Windows.Forms.Button buttonTestPython;
        private System.Windows.Forms.Button buttonTestScilab;
    }
}

