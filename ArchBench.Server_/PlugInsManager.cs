using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ArchBench.PlugIns;
using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;

namespace ArchBench.Server
{
    /// <summary>
    /// Summary description for PlugInManager.
    /// </summary>
    public class PlugInManager : HttpModule, IArchBenchPlugInHost
    {
        #region Fields
        private readonly IList<IArchBenchPlugIn> mPlugIns = new List<IArchBenchPlugIn>();
        #endregion

        public IArchBenchLogger Logger { get; set; }

        /// <summary>
        /// A Collection of all Plugins
        /// </summary>
        public IEnumerable<IArchBenchPlugIn> PlugIns => mPlugIns;

        public override bool Process( IHttpRequest aRequest, IHttpResponse aResponse, IHttpSession aSession )
        {
            foreach ( var plugin in PlugIns )
            {
                var module = plugin as IArchBenchHttpPlugIn;
                if ( module == null ) continue;

                if (! module.Process( aRequest, aResponse, aSession ) ) continue;

                Logger.WriteLine("Plugin {0} processed request {1}", plugin.Name, aRequest.Uri.ToString());
                return true;
            }
            return false;
        }
        
        public void AddPlugIn( string aFileName )
        {
            // Create a new assembly from the plugin file we're adding..
            Assembly assembly = Assembly.LoadFrom( aFileName );

            //Next we'll loop through all the Types found in the assembly
            foreach ( Type type in assembly.GetTypes() )
            {
                if ( ! type.IsPublic ) continue;
                if ( type.IsAbstract ) continue;

                // Gets a type object of the interface we need the plugins to match
                Type typeInterface = type.GetInterface($"ArchBench.PlugIns.{nameof(IArchBenchPlugIn)}", true);

                // Make sure the interface we want to use actually exists
                if ( typeInterface == null ) continue;

                // Create a new instance and store the instance in the collection for later use
                IArchBenchPlugIn instance = (IArchBenchPlugIn) Activator.CreateInstance( assembly.GetType( type.ToString() ) );

                // Set the Plug-in's host to this class which inherited IPluginHost
                instance.Host = this;

                // Call the initialization sub of the plugin
                instance.Initialize();

                //Add the new plugin to our collection here
                mPlugIns.Add( instance );

                // Cleanup a bit
                instance = null;
                typeInterface = null; 
            }

            assembly = null; // More cleanup
        }

        /// <summary>
        /// Unloads and Closes all Plug-ins
        /// </summary>
        public void Clear()
        {
            foreach ( var plugin in PlugIns )
            {
                // Close all plugin instances
                plugin.Dispose();
            }

            //Finally, clear our collection of available plugins
            mPlugIns.Clear();
        }
    }
}