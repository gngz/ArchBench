using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using ArchBench.PlugIns;
using ArchBench.Server.Kernel;

namespace ArchBench.Server.CLI
{
    class ArchBenchExecutor : ArchBenchBaseVisitor<bool>
    {
        private PlugInsServer Server { get; }

        public ArchBenchExecutor( PlugInsServer aServer )
        {
            Server = aServer;
        }

        public override bool VisitErrorNode( IErrorNode node )
        {
            return true;
        }

        public override bool VisitCommand( ArchBenchParser.CommandContext aContext )
        {
            if ( aContext.ChildCount < 1 ) return false;

            switch ( aContext.GetChild(0).GetText() )
            {
                case "help":
                    //Help();
                    break;
                case "start":
                    Start( aContext.GetChild(1) );
                    break;
                case "stop":
                    Stop();
                    break;
                case "install":
                    Install( aContext.GetChild( 1 ) );
                    break;
                case "with":
                    //Set(parts);
                    break;
                case "show":
                    //Show(parts);
                    break;
                case "enable":
                    //Enable(parts[1], true);
                    break;
                case "disable":
                    //Enable(parts[1], false);
                    break;
                case "exit":
                    return Exit();
                default:
                    //Console.WriteLine($"Unknown Command: '{ command }'");
                    return true;
            }

            return true;
        }


        private bool Exit()
        {
            return false;
        }

        private void Start( IParseTree aNode )
        {
            if ( aNode is IErrorNode )
            {
                Console.WriteLine("The <port> is required. Example: ");
                Console.WriteLine("start 8081");
            }
            else if ( int.TryParse( aNode.GetText(), out int port))
            {
                Server.Start(port);
                Console.WriteLine($"Server started on port '{ port }'");
            }
        }

        private void Stop()
        {
            Server.Stop();
        }

        private void Install( IParseTree aNode )
        {
            if ( aNode == null )
            {
            }
            else if ( aNode is IErrorNode )
            {
                Console.WriteLine( aNode.GetText() );
            }
            else
            {
                var plugins = Server.Install( aNode.GetText() );
                Console.WriteLine();
                Console.WriteLine("Installed plugins:");
                Console.WriteLine();
                foreach (var plugin in plugins)
                {
                    Show( plugin );
                }
                Console.WriteLine();
            }
        }

        private void Show( IArchBenchPlugIn aPlugIn )
        {
            if (aPlugIn == null) return;

            var index = Server.Manager.IndexOf(aPlugIn);
            Console.Write($"[{index + 1}]");
            Console.WriteLine($" :\t{ aPlugIn.Name } ({ GetStatus(aPlugIn) })");
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

        private static string GetStatus(IArchBenchPlugIn aPlugIn)
        {
            return aPlugIn.Enabled ? "enabled" : "disabled";
        }
    }
}
