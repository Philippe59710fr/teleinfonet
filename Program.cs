using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using static System.Console;
using static teleinfonet.Display;

namespace teleinfonet
{
    partial class Program
    {
        static Char[] buffer = new char[1];
        static StreamReader sr = null;
        static StringBuilder sb = new StringBuilder();
        static string dbtype, cnxstr;

        static string GetFrame()
        {
            sb.Clear();
            char[] buf = new char[1024];

            while( sr.Read(buffer, 0, 1 )>0 )
                if( buffer[0]==2 )  // end of frame
                    break;
                else
                    sb.Append( buffer[0] );

            Verbose( HexDump.Utils.HexDump( Encoding.ASCII.GetBytes(sb.ToString()) ));
            if( sb.Length>=2)   // In case of invald frame
            {
                sb.Remove(0, 1);
                sb.Remove( sb.Length-1, 1);
            }

            return sb.ToString();
        }

        static void Main(string[] args)
        {
            bool once = false;
            bool gotit = false; // got one frame

            var help = args.Where(a => a=="--help").Count();
            if( help==1 )
            {
                WriteLine(@"This app gathers measures from Teleinfo from your Linky device.
                
Usage and default values : 
dotnet run -- [port=/dev/ttyUSB0] [verbose=false] [output=none] [once=true] [bufferfile=~/teleinfo_buffer.json] [wfrequency=1] [afrequency=10] [persistconnection=true] [dbtype=mysql] [cnxstr=Database%3DTest\;Host%3Dlocalhost\;Port%3D3306\;User%20Id%3Dusername\;Password%3Dpass]

Options : 
  output= none         No output
          json         Output to stdin as json
          rows         Output as rows
          inline       Output raw elements inline

  once=   false        Keep reading input
          true         Read one input, then leave
        
  dbtype= mysql        Write to Mysql Database  --  see MySQL Connector/NET Reference
          none         Do not flush to a database.

  cnxstr= <Connection string>   The connection string for the database. Can be escaped using % encoding.
                                Sample for Mysql : Database%3DTest\;Data%20Source%3Dlocalhost\;User%20Id%3Dusername\;Password%3Dpass
                                equals to  Database=Test;Data Source=localhost;User Id=username;Password=pass
                                BEWARE TO PROPERTLY ESCAPE THE SEMICOLON !

  bufferfile :         The buffer file for pending writes
  port :               The port to read on
  verbose :            Enable verbose mode
  wfrequency :         The frequency to establish and write pending changes to database ( in minutes )
  afrequency :         The frequency to gather data ( in seconds )
  persistconnection :  Keep the connection to the database alive as possible. False=disconnect after each writes.


  Messages are :
    N° d’identification du compteur : ADCO (12 caractères)
    Option tarifaire (type d’abonnement) : OPTARIF (4 car.)
    Intensité souscrite : ISOUSC ( 2 car. unité = ampères)
    Index si option = base : BASE ( 9 car. unité = Wh)
    Index heures creuses si option = heures creuses : HCHC ( 9 car. unité = Wh)
    Index heures pleines si option = heures creuses : HCHP ( 9 car. unité = Wh)
    Index heures normales si option = EJP : EJP HN ( 9 car. unité = Wh)
    Index heures de pointe mobile si option = EJP : EJP HPM ( 9 car. unité = Wh)
    Index heures creuses jours bleus si option = tempo : BBR HC JB ( 9 car. unité = Wh)
    Index heures pleines jours bleus si option = tempo : BBR HP JB ( 9 car. unité = Wh)
    Index heures creuses jours blancs si option = tempo : BBR HC JW ( 9 car. unité = Wh)
    Index heures pleines jours blancs si option = tempo : BBR HP JW ( 9 car. unité = Wh)
    Index heures creuses jours rouges si option = tempo : BBR HC JR ( 9 car. unité = Wh)
    Index heures pleines jours rouges si option = tempo : BBR HP JR ( 9 car. unité = Wh)
    Préavis EJP si option = EJP : PEJP ( 2 car.) 30mn avant période EJP
    Période tarifaire en cours : PTEC ( 4 car.)
    Couleur du lendemain si option = tempo : DEMAIN
    Intensité instantanée : IINST ( 3 car. unité = ampères)
    Avertissement de dépassement de puissance souscrite : ADPS ( 3 car. unité = ampères) (message émis uniquement en cas de dépassement effectif, dans ce cas il est immédiat)
    Intensité maximale : IMAX ( 3 car. unité = ampères)
    Puissance apparente : PAPP ( 5 car. unité = Volt.ampères)
    Groupe horaire si option = heures creuses ou tempo : HHPHC (1 car.)
    Mot d’état (autocontrôle) : MOTDETAT (6 car.)


Table for MySql : 
 CREATE TABLE `teleinfo` (
  `dateinsert` datetime NOT NULL DEFAULT current_timestamp(),
  `IDDEVICE` char(14) NOT NULL,
  `OPTARIF` char(4) DEFAULT NOT NULL,
  `IDXBASE` int(11) DEFAULT NULL,
  `IDXHC` int(11) DEFAULT NULL,
  `IDXHP` int(11) DEFAULT NULL,
  `IDXEJPHN` int(11) DEFAULT NULL,
  `IDXEJPHPM` int(11) DEFAULT NULL,
  `BBRHCJB` int(11) DEFAULT NULL,
  `BBRHPJB` int(11) DEFAULT NULL,
  `BBRHCJW` int(11) DEFAULT NULL,
  `BBRHPJW` int(11) DEFAULT NULL,
  `BBRHCJR` int(11) DEFAULT NULL,
  `BBRHPJR` int(11) DEFAULT NULL,
  `PTEC` char(4) DEFAULT NULL,
  `IINST` tinyint(4) DEFAULT NULL,
  `PAPP` int(11) DEFAULT NULL,
  `HHPHC` char(1) DEFAULT NULL
) ENGINE=MyISAM DEFAULT CHARSET=ascii;

Type 'q' for clean exit.
");
            }
            else
            {
                var pars = args.Select(a => a.Split('=')).ToDictionary(a => a[0], a => a.Length == 2 ? a[1] : null);

                string tmpval;
                pars.TryGetValue("verbose", out tmpval );
                Display.verbose= (tmpval==null ? false : Convert.ToBoolean(tmpval));
                Verbose("verbose = "+Display.verbose );

                pars.TryGetValue("port", out tmpval );
                string port= tmpval ?? "/dev/ttyUSB0";
                Verbose("port = "+port );

                pars.TryGetValue("output", out tmpval );
                Display.output= (tmpval==null ? "none" : tmpval);
                Verbose("output = "+Display.output );

                pars.TryGetValue("once", out tmpval );
                once= (tmpval==null ? false : Convert.ToBoolean(tmpval));
                Verbose("once = "+once );

                pars.TryGetValue("bufferfile", out tmpval );
                bufferfile = tmpval ?? "~/teleinfo_buffer.json";
                bufferfile = Uri.UnescapeDataString( bufferfile );
                Verbose("bufferfile = "+bufferfile );

                int wfrequency;
                pars.TryGetValue("wfrequency", out tmpval );
                wfrequency = (tmpval==null) ? 1 : Math.Abs(Convert.ToInt32(tmpval));
                Verbose("wfrequency = "+wfrequency );

                int afrequency;
                pars.TryGetValue("afrequency", out tmpval );
                afrequency = (tmpval==null) ? 10 : Math.Abs(Convert.ToInt32(tmpval));
                Verbose("afrequency = "+afrequency );

                pars.TryGetValue("persistconnection", out tmpval );
                persistconnection = (tmpval==null) ? true : Convert.ToBoolean(tmpval);
                Verbose("persistconnection = "+persistconnection );

                pars.TryGetValue("dbtype", out tmpval );
                dbtype = tmpval ?? "mysql";
                Verbose("dbtype = "+dbtype );

                pars.TryGetValue("cnxstr", out tmpval );
                cnxstr = tmpval ?? "Database%3DTest;Host%3Dlocalhost;Port%3D3306;User%20Id%3Dusername;Password%3Dpass";
                cnxstr = Uri.UnescapeDataString( cnxstr );
                Verbose("cnxstr = "+cnxstr );

                bool bfine = true;
                if(wfrequency==0 || afrequency==0 || wfrequency*60<afrequency )
                {
                    WriteLine("Frequency mismatch.");
                    WriteLine("Frequencies cannot be zero nor afrequency in seconds being less than wfrequency in minutes.");
                    bfine = false;
                }

                if(bfine)
                    using (sr = new StreamReader(port))
                    {
                        Dictionary<string,string> values = new Dictionary<string,string>();
                        Verbose("Ignoring first frame :");
                        GetFrame(); // get the first ( broken ) frame, and drop it
                        
                        InitWorker( wfrequency );   // Initialize the background timer for the database
                        Stopwatch sw = new Stopwatch();
                        sw.Start();

                        ConsoleKeyInfo cki;
                        bool bleave = false;
                        bool bfirstwrite = true;
                        do
                        {
                            string frame = GetFrame();
                            string[] messages = frame.Split((char)10);

                            values = new Dictionary<string,string>();
                            values.Add( "_datetime", DateTime.Now.ToString("s") );

                            foreach( string field in messages ) // Per frame
                                if( field.Length>0 )
                                {
                                    gotit=true;
                                    string[] m = field.Split(" ");
                                    if( m.Length>=2)
                                        values.Add( m[0], m[1] );
                                    else
                                        WriteError("Got invalid data : --"+field+"--" );
                                }
                            Output( messages );

                            if(verbose)
                                foreach( var k in values.Keys )
                                    Verbose( String.Format("'{0}':'{1}'", k.ToString(), values[k]));

                            if( sw.Elapsed.Seconds+(60*sw.Elapsed.Minutes)+(1440*sw.Elapsed.Hours) >= afrequency || bfirstwrite )                        
                            {
                                Verbose("Appending");
                                BufferAppend(values);
                                sw.Restart();
                                bfirstwrite = false;
                            }

                            // Exit if q key pressed
                            if( Console.KeyAvailable )
                            {
                                cki = Console.ReadKey(true);
                                if( cki.KeyChar== 'q' || cki.KeyChar== 'Q' )
                                {
                                    bleave = true;
                                    StopWorker();
                                }
                            }
                        }
                        while(!bleave && (!once || !gotit));  // display one if asked, but until we got at least one frame
                    }
            }
        }
    }
}
