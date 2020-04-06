using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using ArchBench.PlugIns;

namespace ArchBench.Server
{
    public partial class MainForm : Form
    {
        #region Fields
        private HttpServer.HttpServer mServer;

        #endregion

        public MainForm()
        {
            InitializeComponent();

            Logger  = new TextBoxLogger( mOutput );
            Manager = new PlugInManager { Logger = Logger };
        }

        public IArchBenchLogger Logger { get; }
        public PlugInManager Manager { get; }

        private void OnExit(object sender, EventArgs e)
        {
            mServer?.Stop();
            Manager.Clear();

            Application.Exit();
        }

        private void OnClearConsole(object sender, EventArgs e)
        {
            mOutput.Clear();
        }

        private void OnConnect(object sender, EventArgs e)
        {
            mConnectTool.Checked = ! mConnectTool.Checked;
            if (mConnectTool.Checked)
            {
                mServer = new HttpServer.HttpServer();
                mServer.Add( Manager );
                mServer.Start( IPAddress.Any, int.Parse( mPort.Text ) );

                Logger.WriteLine($"Server online on port { mPort.Text }" );
                mConnectTool.Image = Properties.Resources.connect;
            }
            else
            {
                mServer.Stop();
                mServer = null;
                Logger.WriteLine( $"Server offline on port { mPort.Text }" );
                mConnectTool.Image = Properties.Resources.disconnect;
            }
        }

        private void OnAddPlugIn( object sender, EventArgs e )
        {
            var dialog = new OpenFileDialog {
                Multiselect = true, 
                Filter      = @"Arch.Bench PlugIn File (*.dll)|*.dll"
            };
            if ( dialog.ShowDialog() != DialogResult.OK ) return;
            
            foreach ( var filename in dialog.FileNames )
            {
                Manager.AddPlugIn( filename );
                Logger.WriteLine( $"Added PlugIn from { Path.GetFileName( filename ) }" );
            }
        }
    }
}
