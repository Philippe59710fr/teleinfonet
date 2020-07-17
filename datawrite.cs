using System;
using System.Collections.Generic;
using System.Timers;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;

using static teleinfonet.Display;

namespace teleinfonet
{
    partial class Program
    {
        public static List<Dictionary<string,string>> writebuffer;
        static object buflock = new object();
        static bool persistconnection;
        static Timer worker;
        static string bufferfile;

        static Backend be;  // The backend for storage.


        static void InitWorker( int wfrequency )
        {
            // Reload any pending writes
            try 
            {
                using( TextReader tr = new StreamReader( bufferfile ))
                    writebuffer = JsonConvert.DeserializeObject<List<Dictionary<string,string>>>(tr.ReadToEnd());
            }
            catch( Exception )
            {
                Verbose( String.Format("Unable to open {0} for reading, starting a new instance of the buffer.", bufferfile));
                writebuffer = new List<Dictionary<string,string>>();
            }

            be = SqlBuilder.Build( dbtype, cnxstr, persistconnection );

            // Initialize the Worker Thread
            worker = new Timer(wfrequency*60000);   // Timer in milliseconds, entry in minutes
            worker.Elapsed += WorkEvent;
            worker.AutoReset = true;
            worker.Enabled = true;

            // Does not handled Ctrl+C on .Net core 3.1 Raspberry pî
            //AppDomain.CurrentDomain.ProcessExit += ( s, e ) => StopWorker();
        }

        static void StopWorker()
        {
            Verbose("Stopping worker.");
            worker.Stop();
            worker.Dispose();
            PersistBuffer();
        }

        static void WorkEvent( Object source, ElapsedEventArgs e )
        {
            Verbose("Processing buffer on timer event.");
            PersistBuffer();
        }

        static void BufferAppend( Dictionary<string,string> values )
        {
            lock(buflock)
            {
                writebuffer.Add(values);

                SaveBuffer();
            }
        }

        static void SaveBuffer()
        {
            string json = JsonConvert.SerializeObject(writebuffer, Formatting.Indented);
            using( TextWriter tw = new StreamWriter( bufferfile ))
                tw.Write( json );
        }

        static void PersistBuffer()
        {
            // Write the buffer to the database
            lock(buflock)
            {
                Verbose("Writing to backend.");
                if(be.Write( writebuffer ))
                    writebuffer.Clear();
                SaveBuffer();
                Verbose("Done writing.");
            }
        }
    }
}