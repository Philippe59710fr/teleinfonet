using System;

using static System.Console;

namespace teleinfonet
{
    public class Display
    {
        public static bool verbose;
        public static string output;
        static bool bfirstjsonobject = true;


        public static void Verbose( string msg )
        {
            if( verbose )
                Console.WriteLine( "{0:yyyy-dd-MM H:mm:ss} : {1}", DateTime.Now, msg );
        }

        public static void Output( string[] frame )
        {
            switch( output )
            {
                case "rows" :
                    // WriteLine("# -- {0:yyyy-dd-MM H:mm:ss} --", DateTime.Now);
                    foreach( string field in frame )
                        if( field.Length>2 )
                            WriteLine( field.Substring( 0, field.Length-2) );
                    break;
                case "inline" :
                    // WriteLine("# -- {0:yyyy-dd-MM H:mm:ss} --", DateTime.Now);
                    foreach( string field in frame )
                        if( field.Length>2 )
                            Write( "{0} ", field.Substring( 0, field.Length-2) );
                    WriteLine();
                    break;
                case "json" :
                    bool bfirst = true;
                    
                    if( !bfirstjsonobject )
                        Write(",");
                    else
                    {
                        bfirstjsonobject = false;
                        WriteLine("[");
                        AppDomain.CurrentDomain.ProcessExit += (s, e) => { WriteLine("]"); };   // Output a ] upon exit
                    }

                    WriteLine("{");
                    // WriteLine("\t\"_datetime\" : \"{0:yyyy-dd-MM H:mm:ss}\",", DateTime.Now);
                    foreach( string field in frame ) // Per frame
                    {
                        string[] m = field.Split(" ");
                        if( m.Length>=2)
                        {
                            if( !bfirst )
                                WriteLine(",");
                            Write("\t\"{0}\" : \"{1}\"", m[0], m[1]);
                            bfirst = false;
                        }
                    }
                    WriteLine();
                    WriteLine("}");
                    break;
                default :
                    Error.WriteLine("unknown parameter value for output" );
                    Environment.Exit(1);
                    break;
            }
        }

        public static void WriteError( string message )
        {
            Console.Error.WriteLine("{0:yyyy-dd-MM H:mm:ss} : #Error# : {1}", DateTime.Now, message );
        }
    }
}