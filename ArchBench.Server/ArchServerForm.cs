using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using System.Net;
using ArchBench.PlugIns;
using ArchBench.Server.Kernel;
using ArchBench.Server.Configurations;


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

        private void OnClearConsole(object sender, EventArgs e)
        {
            mOutput.Text = string.Empty;
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

        private string GetIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) return ip.ToString();
            }
            return "0.0.0.0";
        }

        private int GetPort()
        {
            return int.TryParse( mPort.Text, out var port ) ? port : 8081;
        }

        private async void OnOpen(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog {
                Filter = @"Configuration Files (*.config)|*.config|All Files (*.*)|*.*"
            };

            if ( dialog.ShowDialog() != DialogResult.OK ) return;

            //var options = new JsonSerializerOptions();
            //options.MaxDepth = 4;

            using ( var stream = File.OpenRead( dialog.FileName ) )
            {
                var config = await JsonSerializer.DeserializeAsync<ServerConfig>( stream );
                if (config == null) return;

                mPort.Text = config.Port.ToString();
                ModulePugIns.Manager.Clear();
                foreach (var plugin in config.PlugIns)
                {
                    var instance = ModulePugIns.Manager.Add(plugin.FileName, plugin.FullName);
                    if (instance == null) continue;

                    foreach (var key in plugin.Settings.Keys)
                    {
                        instance.Settings[key] = plugin.Settings[key];
                    }
                }
            }

            //var stream = new StreamReader( dialog.FileName );
            //var config = JsonSerializer.Deserialize<ServerConfig>( stream.ReadToEnd(), options );
        }

        private async void OnSave(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog() {
                Filter = @"Configuration Files (*.config)|*.config"
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            var config = new ServerConfig { Port = GetPort() };
            foreach ( var plugin in ModulePugIns.Manager.PlugIns )
            {
                var filename = ModulePugIns.Manager.GetFileName( plugin );
                if ( config.PlugIns.Any( c => c.FileName.Equals( filename ) ) ) continue;

                var elem = new PlugInConfig {
                    FullName = plugin.GetType().FullName,
                    FileName = filename
                };

                foreach ( var setting in plugin.Settings )
                {
                    elem.Settings.Add(setting, plugin.Settings[setting]);
                }
                config.PlugIns.Add( elem  );
            }

            using ( var stream = File.Create( dialog.FileName ) )
            {
                await JsonSerializer.SerializeAsync( stream, config );
            }

            //using (var stream = new FileStream(dialog.FileName, FileMode.Create))
            //{
            //    var writer = new Utf8JsonWriter(stream);
            //    var options = new JsonSerializerOptions { WriteIndented = true };
            //    JsonSerializer.Serialize(writer, config, options);
            //    stream.Flush();
            //}
        }
    }
}
