using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Net;
using ArchBench.PlugIns;
using ArchBench.Server.Kernel;

namespace ArchBench.Server
{
    public partial class ArchServerForm : Form
    {

        public ArchServerForm()
        {
            InitializeComponent();
            Logger = new TextBoxLogger( mOutput );
            Server = new PlugInsServer( Logger );
        }

        private IArchBenchLogger Logger { get; }
        public  PlugInsServer    Server { get; }

        #region Toolbar Double Click problem

        private bool HandleFirstClick { get; set; } = false;

        protected override void OnActivated( EventArgs e )
        {
            base.OnActivated( e );
            if (HandleFirstClick)
            {
                var position = Cursor.Position;
                var point = this.PointToClient(position);
                var child = this.GetChildAtPoint(point);
                while ( HandleFirstClick && child != null )
                {
                    if (child is ToolStrip toolStrip)
                    {
                        HandleFirstClick = false;
                        point = toolStrip.PointToClient(position);
                        foreach (var item in toolStrip.Items)
                        {
                            if (item is ToolStripItem toolStripItem && toolStripItem.Bounds.Contains(point))
                            {
                                if (item is ToolStripMenuItem tsMenuItem)
                                {
                                    tsMenuItem.ShowDropDown();
                                }
                                else
                                {
                                    toolStripItem.PerformClick();
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        child = child.GetChildAtPoint(point);
                    }
                }
                HandleFirstClick = false;
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_ACTIVATE = 0x0006;
            const int WA_CLICKACTIVE = 0x0002;
            if (m.Msg == WM_ACTIVATE && Low16(m.WParam) == WA_CLICKACTIVE)
            {
                HandleFirstClick = true;
            }
            base.WndProc( ref m );
        }

        private static int GetIntUnchecked(IntPtr value)
        {
            return IntPtr.Size == 8 ? unchecked((int)value.ToInt64()) : value.ToInt32();
        }

        private static int Low16(IntPtr value)
        {
            return unchecked((short)GetIntUnchecked(value));
        }

        private static int High16(IntPtr value)
        {
            return unchecked((short)(((uint)GetIntUnchecked(value)) >> 16));
        }

        #endregion
        
        private void OnExit(object sender, EventArgs e)
        {
            Server?.Stop();
            Application.Exit();
        }

        private void OnConnect(object sender, EventArgs e)
        {
            mConnectTool.Checked = ! mConnectTool.Checked;
            if ( mConnectTool.Checked )
            {
                if ( int.TryParse( mPort.Text, out int port ) )
                {
                    Server.Start( port );
                    Logger.WriteLine("Server online on port {0}", port );
                    mConnectTool.Image = Properties.Resources.connect;
                }
                else
                {
                    Logger.WriteLine("Invalid port '{0}' specification", mPort.Text );
                }
            }
            else
            {
                Server.Stop();
                mConnectTool.Image = Properties.Resources.disconnect;
            }
        }

        private void OnPlugIn( object sender, EventArgs e )
        {
            new PlugInsForm( Server.Manager ).ShowDialog();
        }
    }
}
