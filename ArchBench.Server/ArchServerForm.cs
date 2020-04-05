using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using ArchBench.PlugIns;

namespace ArchBench.Server
{
    public partial class ArchServerForm : Form
    {
        #region Fields
        private HttpServer.HttpServer mServer;
        private readonly IArchBenchLogger       mLogger;
        private readonly ArchBenchPlugInsModule mPlugInsModule;
        #endregion

        public ArchServerForm()
        {
            InitializeComponent();
            mLogger = new TextBoxLogger( mOutput );
            mPlugInsModule = new ArchBenchPlugInsModule( mLogger );
        }

        private void OnExit(object sender, EventArgs e)
        {
            if ( mServer != null ) mServer.Stop();
            mPlugInsModule.PlugInManager.ClosePlugIns();
            Application.Exit();
        }

        private void OnConnect(object sender, EventArgs e)
        {
            mConnectTool.Checked = ! mConnectTool.Checked;
            if (mConnectTool.Checked)
            {
                mServer = new HttpServer.HttpServer();
                mServer.Add( new ArchServerModule( mLogger ) );
                mServer.Add( mPlugInsModule );
                mServer.Start( IPAddress.Any, int.Parse( mPort.Text ) );

                mLogger.WriteLine( String.Format( "Server online on port {0}", mPort.Text ) );
                mConnectTool.Image = Properties.Resources.connect;
            }
            else
            {
                mServer.Stop();
                mServer = null;
                mLogger.WriteLine( String.Format( "Server offline on port {0}", mPort.Text ) );
                mConnectTool.Image = Properties.Resources.disconnect;
            }
        }

        private void OnPlugIn( object sender, EventArgs e )
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "Arch.Bench PlugIn File (*.dll)|*.dll";

            if ( dialog.ShowDialog() == DialogResult.OK )
            {
                foreach ( var filename in dialog.FileNames )
                {
                    mPlugInsModule.PlugInManager.AddPlugIn( filename );
                    mLogger.WriteLine( String.Format( "Added PlugIn from {0}",
                        System.IO.Path.GetFileName( filename ) ) );
                }
            }
        }

        public static string GetLocalIP()
        {
            // Resolves a host name or IP address to an IPHostEntry instance.
            // IPHostEntry - Provides a container class for Internet host address information. 
            IPHostEntry IPHostEntry = Dns.GetHostEntry( Dns.GetHostName() );

            // IPAddress class contains the address of a computer on an IP network. 
            foreach ( IPAddress IPAddress in IPHostEntry.AddressList)
            {
                // InterNetwork indicates that an IP version 4 address is expected 
                // when a Socket connects to an endpoint
                if ( IPAddress.AddressFamily.ToString() != "InterNetwork" ) continue;
                return IPAddress.ToString();
            }
            return @"127.0.0.1";
        }

        private void OnRegistServer( object sender, EventArgs evt )
        {
            try 
            {
                TcpClient client = new TcpClient( mServerAddress.Text, 9000 );

                string address = GetLocalIP();
                mLogger.WriteLine("LocalIP: {0}", address );
                string message = String.Format( "{0}{1}{2}{3}{4}", mServerName.Text, '@', address, ':', mPort.Text );

                Byte[] data = Encoding.ASCII.GetBytes( message );         

                NetworkStream stream = client.GetStream();
                stream.Write( data, 0, data.Length );
                stream.Close();         
                client.Close();

                mRegisterServer.Image = Properties.Resources.link;
            } 
            catch ( SocketException e ) 
            {
               mLogger.WriteLine( String.Format( "SocketException: {0}", e ) );
            }
        }

        private void OnEraseOutput( object sender, EventArgs e )
        {
            mOutput.Clear();
        }
    }
}
