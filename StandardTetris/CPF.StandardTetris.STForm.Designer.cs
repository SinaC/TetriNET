namespace CPF.StandardTetris
{
    partial class STForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose ( bool disposing )
        {
            if (disposing && (components != null))
            {
                components.Dispose( );
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent ( )
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( STForm ) );
            this.mGRControl = new CPF.GRV10.GRControl( );
            this.SuspendLayout( );
            // 
            // mGRControl
            // 
            this.mGRControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mGRControl.Location = new System.Drawing.Point( 12, 12 );
            this.mGRControl.Name = "mGRControl";
            this.mGRControl.Size = new System.Drawing.Size( 608, 429 );
            this.mGRControl.TabIndex = 0;
            this.mGRControl.TabStop = false;
            this.mGRControl.Text = "GRControl";
            // 
            // STForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 632, 453 );
            this.Controls.Add( this.mGRControl );
            this.Icon = ((System.Drawing.Icon)(resources.GetObject( "$this.Icon" )));
            this.Name = "STForm";
            this.Text = "Standard Tetris";
            this.ResumeLayout( false );

        }

        #endregion
    }
}

