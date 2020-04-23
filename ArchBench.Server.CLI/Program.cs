using System;
using System.Linq;
using Antlr4.Runtime;
using ArchBench.PlugIns;
using ArchBench.Server.Kernel;

namespace ArchBench.Server.CLI
{
    class Program
    {
        private static PlugInsServer Server { get; } = new PlugInsServer();

        static void Main()
        {
            Console.WriteLine("ArchBench Server CLI (Command Line Interface) - version 1.0");
            Console.WriteLine();
            Console.WriteLine( "Use 'help' command for usage information.");
            Console.WriteLine();
            Serve();
        }

        private static void Serve()
        {
            while (true)
            {
                Console.Write( "? ");
                var command = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(command)) continue;

                var lexer  = new ArchBenchLexer(new CodePointCharStream( command ));
                var tokens = new CommonTokenStream( lexer );
                var parser = new ArchBenchParser( tokens );

                parser.RemoveErrorListeners();
                parser.AddErrorListener( new ArchBenchErrorListener() );

                var visitor = new ArchBenchExecutor( Server );
                if ( !visitor.VisitCommand( parser.command() ) ) break;
                continue;

                var parts = command?.Split(' ');
                switch ( parts[0].ToLower() )
                {
                    case "help": Help();
                        break;
                    case "start": Start( parts );
                        break;
                    case "stop": Stop();
                        break;
                    case "install": Install( parts[1] );
                        break;
                    case "with":
                        Set( parts );
                        break;
                    case "show": Show( parts );
                        break;
                    case "enable": Enable( parts[1], true );
                        break;
                    case "disable": Enable( parts[1], false );
                        break;
                    case "exit": 
                        Stop();
                        return;
                    default:
                        Console.WriteLine( $"Unknown Command: '{ command }'" );
                        break;
                }
            }
        }

        public static void Help()
        {
            Console.WriteLine("The ArchBench Server CLI is a simple command line utility that allows ");
            Console.WriteLine("running and instance of ArchBench Server.");
            Console.WriteLine();
            Console.WriteLine("Available Commands:");
            Console.WriteLine();
            Console.WriteLine("  start <port>                           Start the server on the specified port (<port>).");
            Console.WriteLine("  stop                                   Stops the server.");
            Console.WriteLine("  install <path>                         Install a plugin.");
            Console.WriteLine("  show                                   Show all plugins installed" );
            Console.WriteLine("  show <id>                              Show details of plugin with the given id");
            Console.WriteLine("  enable  <id>|<name>                    Enables the plugin.");
            Console.WriteLine("  disable <id>|<name>                    Disables the plugin.");
            Console.WriteLine("  for <name>|<id> set <property>=<value> Define the plugin's settings");
            Console.WriteLine("  help                                   Help about this application.");
            Console.WriteLine("  exit                                   Exits from server.");
            Console.WriteLine();
        }

        private static void Start( string[] aParts )
        {
            if ( aParts.Length < 2 )
            {
                Console.WriteLine( "The <port> is required. Example: " );
                Console.WriteLine( "start 8081" );
            }
            else if ( int.TryParse( aParts[1], out int port ) )
            {
                Server.Start( port );
                Console.WriteLine( $"Server started on port '{ port }'");
            }
        }

        private static void Stop()
        {
            Server.Stop();
        }

        private static void Install( string aFileName )
        {
            var plugins = Server.Install( aFileName );
            Console.WriteLine();
            Console.WriteLine( "Installed plugins:");
            Console.WriteLine();
            foreach ( var plugin in plugins )
            {
                Show( plugin );
            }
            Console.WriteLine();
        }

        private static void Show( string[] aParts )
        {
            if ( aParts.Length == 1 )
            {
                foreach (var plugin in Server.Manager.PlugIns)
                {
                    var index = Server.Manager.IndexOf(plugin);
                    Console.WriteLine($"[{ index }] :\t{ plugin.Name } ({ GetStatus(plugin)})");
                }
            }
            else
            {
                Show( aParts[1] );
            }
        }

        private static void Show( string aId )
        {
            if ( int.TryParse( aId, out var id ) )
            {
                Show( Server.Manager.Get( id - 1 ) ); 
            }
            else
            {
                Console.WriteLine($"Invalid id: '{ aId}'");
            }
        }

        private static void Set( string[] aParts )
        {
            if ( int.TryParse( aParts[1], out var id ) )
            {
                var plugin = Server.Manager.Get( id - 1 );
                if ( plugin == null ) return;

                var property = "";
                var value    = "";

                if ( aParts.Length == 4 )
                {
                    var parts = aParts[ 3 ].Split('=');
                    if ( parts.Length != 2 ) return;

                    property = parts[ 0 ];
                    value = parts[ 1 ];
                }
                else if (aParts.Length == 5)
                {
                    property = aParts[3].TrimStart('=').TrimEnd('=');
                    value = aParts[4].TrimStart('=').TrimStart('=');
                }
                else if ( aParts.Length > 5 )
                {
                    property = aParts[ 3 ];
                    value    = aParts[ 5 ];
                } 

                if (plugin.Settings.Contains( property ) )
                {
                    plugin.Settings[ property ] = value;
                }
            }
        }

        private static void Show( IArchBenchPlugIn aPlugIn )
        {
            if ( aPlugIn == null ) return;

            var index = Server.Manager.IndexOf( aPlugIn );
            Console.Write($"[{index + 1}]");
            Console.WriteLine($" :\t{ aPlugIn.Name } ({ GetStatus( aPlugIn ) })");
            Console.WriteLine($"\tAuthor: { aPlugIn.Author }");
            Console.WriteLine($"\tVersion: { aPlugIn.Version }");
            Console.WriteLine($"\tDescription: { aPlugIn.Description }");
            if (aPlugIn.Settings.Any())
            {
                Console.WriteLine("\tSettings:");
                foreach (var key in aPlugIn.Settings)
                {
                    Console.WriteLine($"\t\t{key} = { aPlugIn.Settings[key] }");
                }
            }
        }

        private static string GetStatus( IArchBenchPlugIn aPlugIn )
        {
            return aPlugIn.Enabled ? "enabled" : "disabled";
        }

        private static void Enable( string aPlugIn, bool aEnabled )
        {
            if ( int.TryParse( aPlugIn, out var id ) )
            {
                Server.Enable( Server.Manager.Get( id - 1 ), aEnabled );
            }
            else
            {
                Server.Enable( Server.Manager.Find( aPlugIn ), aEnabled );
            }
        }
    }
}
