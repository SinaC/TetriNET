// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.


using System.Windows.Forms;

using CPF.GRV10;




namespace CPF.StandardTetris
{

    public class STFormHandler
    {



        public STFormHandler ( )
        {
        }



        public void OpenGLStarted ( GRControl grControl )
        {
            GR gr = grControl.GetGR( );
            if (null == gr) return;
            // ...
        }



        public void Paint ( object sender, PaintEventArgs e )
        {
            if (null == sender) return;
            if (false == (sender is GRControl)) return;

            GRControl grControl = (sender as GRControl);
            GR gr = grControl.GetGR( );

            int clientWidth = grControl.ClientRectangle.Width;
            int clientHeight = grControl.ClientRectangle.Height;
            if (clientWidth <= 0)
            {
                clientWidth = 1;
            }
            if (clientHeight <= 0)
            {
                clientHeight = 1;
            }


            // Viewport 
            gr.glViewport( 0, 0, clientWidth, clientHeight );


            // Clear the viewport
            gr.glClearColor( 0.0f, 0.0f, 0.0f, 0.0f );
            gr.glClear( GR.GL_COLOR_BUFFER_BIT | GR.GL_DEPTH_BUFFER_BIT );


            // Basic rendering conditions
            gr.glEnable( GR.GL_DEPTH_TEST );
            gr.glDepthFunc( GR.GL_LEQUAL );
            gr.glEnable( GR.GL_CULL_FACE );
            gr.glCullFace( GR.GL_BACK );
            gr.glFrontFace( GR.GL_CCW );






            gr.glMatrixMode( GR.GL_PROJECTION );
            gr.glLoadIdentity( );
            gr.gluOrtho2D( 0.0, (double)clientWidth, 0.0, (double)clientHeight );

            gr.glMatrixMode( GR.GL_MODELVIEW );
            gr.glLoadIdentity( );







            // TETRIS GAME ITERATION

            STUserInterface.PerformGameIterations
            (
                STEngine.GetGame( ),
                grControl.GetPreviousFrameDurationSeconds( )
            );


            // TETRIS GAME DRAWING

            System.Drawing.Point controlRelativePoint = grControl.PointToClient( Cursor.Position );
            int clientRelativeCursorX = controlRelativePoint.X;
            int clientRelativeCursorY = controlRelativePoint.Y;

            STGameDrawing.DrawScreen
            (
                clientWidth,
                clientHeight,
                clientRelativeCursorX,
                clientRelativeCursorY,
                gr,
                STEngine.GetGame( ),
                STEngine.GetConsole( )
            );




            // Flush all the current rendering and flip the back buffer to the front.
            gr.wglSwapBuffers( grControl.GetHDC( ) );
        }









        public void KeyDown ( object sender, KeyEventArgs e )
        {
            // NOTE: The only way for cursor key events (up,down,left,right)
            // to make it to this function is for the main form to implement 
            // the following:
            //
            //   protected override bool ProcessDialogKey ( Keys keyData )
            //
            // and explicitly invoke this KeyDown() method with the 
            // an appropriately formed KeyEventArgs instance.
            // ALSO, the KeyPreview property of the main Form must be set to
            // 'true' for that override method to be called.

            STUserInterface.HandleKeyPress
                (
                STEngine.GetMainForm( ).mGRControl.GetGR( ),
                STEngine.GetMainForm( ).Handle,
                0,
                0,
                STEngine.GetGame( ),
                e.KeyCode,
                e.Shift,
                e.Control
                );
        }




        public void KeyUp ( object sender, KeyEventArgs e )
        {
        }




        public void MouseDown ( object sender, MouseEventArgs e )
        {
            if (null == sender) return;
            if (false == (sender is GRControl)) return;
            // GRControl grControl = (sender as GRControl);

            // int clientWidth = grControl.ClientRectangle.Width;
            // int clientHeight = grControl.ClientRectangle.Height;
            // int clientX = grControl.PointToClient( Cursor.Position ).X;
            // int clientY = grControl.PointToClient( Cursor.Position ).Y;

            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
            }

            if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
            {
            }
        }




        public void MouseUp ( object sender, MouseEventArgs e )
        {
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
            {
            }
        }




        public void MouseMove ( object sender, MouseEventArgs e )
        {
            if (null == sender) return;
            if (false == (sender is GRControl)) return;
            // GRControl grControl = (sender as GRControl);

            // int clientWidth = grControl.ClientRectangle.Width;
            // int clientHeight = grControl.ClientRectangle.Height;
            // int clientX = grControl.PointToClient( Cursor.Position ).X;
            // int clientY = grControl.PointToClient( Cursor.Position ).Y;

            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
            }
        }



        public void MouseWheel ( object sender, MouseEventArgs e )
        {
        }



    }



}