using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Models;
using Vsync;
using System.Diagnostics;

namespace IsisNode
{
    class Program
    {
        // Convert an object to a byte array
        private static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        // Convert a byte array to an Object
        private static Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }

        private static void Log(string msg)
        {
            Console.WriteLine("{0:HH:mm:ss.ff} --- {1}", DateTime.Now, msg);
        }

        public static string GroupIdentifier = "Search";
        static TcpListener server = null;
        static Thread serverThread = null;
        public static readonly object serverLocker = new object();
        static Vsync.Group g;
        static int nbNodes;

        public const int SEARCH = 0;

        [Vsync.AutoMarshalled]
        public class QueryMessage
        {
            public string query;
            public int nbNodes;
        }

        static void Main(string[] args)
        {
            int MASTER_RANK = 0; //the first one enter is master
            Log("Start Vsync System");
            Vsync.VsyncSystem.Start();
            Vsync.Msg.RegisterType(typeof(QueryMessage), 0);
            Vsync.Msg.RegisterType(typeof(Contact), 1);
            g = new Vsync.Group(GroupIdentifier);
            int myRank = 0;
            g.ViewHandlers += (Vsync.ViewHandler)delegate(Vsync.View view)
            {
                //there is changes in View
                Vsync.VsyncSystem.WriteLine("New view: " + view);
                myRank = view.GetMyRank();
                nbNodes = view.members.Length;
                lock (serverLocker)
                {
                    //if this is Master --> start the server to receive request
                    if (myRank == MASTER_RANK)
                    {
                        if (serverThread == null || !serverThread.IsAlive)
                        {
                            Log("I'm the Master!");
                            serverThread = new Thread(new ParameterizedThreadStart(startServer));
                            serverThread.Start();
                        }
                    }
                    else
                    {
                        if (serverThread != null && serverThread.IsAlive)
                        {
                            server.Stop();
                        }
                    }
                }
            };
            g.Handlers[SEARCH] += (Action<QueryMessage>)delegate(QueryMessage queryMsg)
            {
                Log(string.Format("Received search request from Master: {0}", queryMsg.query));
                bool isFinalNode = (myRank == queryMsg.nbNodes);
                int nbContacts = Int32.Parse(File.ReadLines(Searcher.dbFileName).Skip(0).Take(1).ElementAt(0));
                int nbLines = nbContacts / queryMsg.nbNodes;
                int startIndex = nbLines * myRank;
                if (!isFinalNode)
                {
                    nbLines = nbContacts - startIndex;
                }
                Searcher searcher = new Searcher();
                List<Contact> localResult = searcher.search(queryMsg.query, startIndex, nbLines);
                g.Reply(localResult);
                Log(string.Format("Returned result for {0}", queryMsg.query));
            };
            g.Join();
            Log(string.Format("Joined with Rank {0}", myRank));
            Vsync.VsyncSystem.WaitForever();
            Console.ReadLine();
        }

        public static void HandleClient(object obj)
        {
            // retrieve client from parameter passed to thread
            TcpClient client = (TcpClient)obj;

            Log("New Client Connected");

            String data = null;
            // Buffer for reading data
            Byte[] bytes = new Byte[256];

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            int i;
            // Loop to receive all the data sent by the client.
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                Log(string.Format("Received Search Request: {0}", data));

                // Process the data sent by the client.
                String searchString = data;

                if (File.Exists(Searcher.dbFileName))
                {
                    List<List<Contact>> answers = new List<List<Contact>>();
                    QueryMessage queryMsg = new QueryMessage();
                    queryMsg.query = searchString;
                    queryMsg.nbNodes = nbNodes;
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    g.OrderedQuery(Vsync.Group.ALL, new Vsync.Timeout(120000, Vsync.Timeout.TO_ABORTREPLY), SEARCH, queryMsg, new Vsync.EOLMarker(), answers);
                    stopwatch.Stop();
                    List<Contact> contacts = new List<Contact>();
                    for (int ans = 0; ans < answers.Count; ans++)
                    {
                        contacts.AddRange(answers[ans]);
                    }
                    if (contacts.Count > 100)
                    {
                        contacts = contacts.GetRange(0, 100);
                    }
                    byte[] msg = ObjectToByteArray(contacts);
                    byte[] msgSize = System.Text.Encoding.ASCII.GetBytes(msg.Length + "");
                    stream.Write(msgSize, 0, msgSize.Length);
                    stream.Write(msg, 0, msg.Length);
                    Log(string.Format("Sent Result of \"{1}\": {0} - Search Completed In {2} (ms)", contacts.Count, searchString, stopwatch.ElapsedMilliseconds));
                }
                else
                {
                    String Error = "Error: database not ready";
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(Error);
                    stream.Write(msg, 0, msg.Length);
                }
            }

            // Shutdown and end connection
            client.Close();
        }

        static void startServer(object obj)
        {
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();
                Log("Server started");

                // Enter the listening loop.
                while (true)
                {
                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    // wait for client connection
                    TcpClient newClient = server.AcceptTcpClient();

                    // client found.
                    // create a thread to handle communication
                    Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                    t.Start(newClient);
                }
            }
            catch (SocketException e)
            {
                Log(string.Format("SocketException: {0}", e));
                Console.ReadLine();
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


            Log("Server Stop");

        }
    }
}
