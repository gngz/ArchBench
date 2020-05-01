using System;
using System.Net;
using System.Net.Sockets;

namespace ArchBench.PlugIns.BrokerRegister
{
    public class BrokerRegister : IArchBenchPlugIn
    {
        #region PlugIn Metadata
        public string Name => "BrokerRegister";

        public string Description => "";

        public string Author => "Gonçalo Passos";

        public string Version => "0.9";

        #endregion

        public bool OnService { get; set; }

        public bool Enabled { get => OnService; set => Registration(value); }
        public IArchBenchPlugInHost Host { get; set; }
        public IArchBenchSettings Settings { get; set; } = new ArchBenchSettings();

        private string _ip;

        public void Dispose()
        {

        }

        public void Initialize()
        {
            Settings["BrokerAddress"] = "0.0.0.0";
            Settings["ServerPort"] = "8000";
            _ip = GetIP();

        }
        private void Registration(bool aOnService)
        {
            if (aOnService == OnService) return;
            OnService = aOnService;

            try
            {
                WebClient wb = new WebClient();



                if (OnService)
                {
                    wb.UploadString(@"http://" + Settings["BrokerAddress"] + "/broker/register", $"{_ip}:{Settings["ServerPort"]}");
                    Host.Logger.WriteLine($"Registering {_ip}:{Settings["ServerPort"]}");
                }
                else
                {
                    wb.UploadString(@"http://" + Settings["BrokerAddress"] + "/broker/unregister", $"{_ip}:{Settings["ServerPort"]}");
                    Host.Logger.WriteLine($"Unregistering {_ip}:{Settings["ServerPort"]}");

                }
            }
            catch (Exception ex)
            {
                // TODO some work here
            }
        }

        private static string GetIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) return ip.ToString();
            }
            return "0.0.0.0";
        }
    }
}
