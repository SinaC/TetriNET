using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using CPF.GRV10;

namespace CPF.StandardTetris
{
    public partial class STForm : Form
    {
        public GRControl mGRControl;
        public STFormHandler mSTFormHandler;
        private System.Windows.Forms.Timer mTimer;


        private void PrivateTimerTickEventHandler ( object sender, EventArgs e )
        {
            if (false == DesignMode)
            {
                this.mGRControl.Invalidate( );
            }
        }

        private void PrivateActivatedEventHandler ( object sender, EventArgs e )
        {
            // When this form becomes activated, after some time of not
            // being active, set input focus to the main control on the form.
            this.mGRControl.Focus( );
        }

        public STForm ( )
        {
            InitializeComponent( );

            this.mSTFormHandler = new STFormHandler( );
            this.mGRControl.OpenGLStarted += new GRControl.DelegateOpenGLStarted( this.mSTFormHandler.OpenGLStarted );
            this.mGRControl.KeyDown += new KeyEventHandler( this.mSTFormHandler.KeyDown );
            this.mGRControl.KeyUp += new KeyEventHandler( this.mSTFormHandler.KeyUp );
            this.mGRControl.MouseDown += new MouseEventHandler( this.mSTFormHandler.MouseDown );
            this.mGRControl.MouseUp += new MouseEventHandler( this.mSTFormHandler.MouseUp );
            this.mGRControl.MouseMove += new MouseEventHandler( this.mSTFormHandler.MouseMove );
            this.mGRControl.MouseWheel += new MouseEventHandler( this.mSTFormHandler.MouseWheel );
            this.mGRControl.Paint += new PaintEventHandler( this.mSTFormHandler.Paint );

            this.mTimer = new System.Windows.Forms.Timer( );
            this.mTimer.Interval = 16; // 16-millisecond timer
            this.mTimer.Tick += new EventHandler( PrivateTimerTickEventHandler );
            this.mTimer.Start( );

            // Set focus to the control so that it can immediately accept input
            this.mGRControl.Focus( );

            // Also, whenever the form becomes activated, set focus to the main
            // control on the form.  The following sets up an event handler for
            // that purpose.
            this.Activated += new EventHandler( PrivateActivatedEventHandler );

            // We want to preview dialog keys (most importantly, the cursor
            // keys: up, down, right, left) so we can forward such events to
            // the appropriate child control.
            this.KeyPreview = true;
        }


        // Cursor keys (up,down,left,right) need to be specially captured
        // and forwarded to the control.
        // CAUTION: The KeyPreview property of this Form must be set to 'true' 
        // for the following method to be called.
        protected override bool ProcessDialogKey ( Keys keyData )
        {
            if 
                (
                   (keyData == Keys.Up)
                || (keyData == Keys.Down)
                || (keyData == Keys.Left)
                || (keyData == Keys.Right)
                )
            {
                KeyEventArgs e = new KeyEventArgs( keyData );
                this.mSTFormHandler.KeyDown( this, e );
                return (true);
            }

            return base.ProcessDialogKey( keyData );
        }

    }
}