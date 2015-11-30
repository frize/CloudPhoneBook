using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

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

        public static void HandleClient(object obj)
        {
            // retrieve client from parameter passed to thread
            TcpClient client = (TcpClient)obj;

            Console.WriteLine("New Client Connected");

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
                Console.WriteLine("Received Search Request: {0}", data);

                // Process the data sent by the client.
                String searchString = data;

                if (File.Exists(Searcher.dbFileName))
                {
                    List<Contact> contacts = new List<Contact>();
                    int nbContacts = Int32.Parse(File.ReadLines(Searcher.dbFileName).Skip(0).Take(1).ElementAt(0));
                    int nbClusters = 10;
                    int szClusters = nbContacts / nbClusters;

                    for (int cl = 0; cl < nbClusters; cl++)
                    {
                        int index = cl * szClusters;
                        int nbLines = szClusters;
                        if (cl == nbClusters - 1)
                        {
                            nbLines = nbClusters - index;
                        }
                        Searcher searcher = new Searcher();
                        List<Contact> cache = searcher.search(searchString, index, nbLines);
                        contacts.AddRange(cache);
                    }
                    byte[] msg = ObjectToByteArray(contacts.GetRange(0, 100));
                    byte[] msgSize = System.Text.Encoding.ASCII.GetBytes(msg.Length + "");
                    stream.Write(msgSize, 0, msgSize.Length);
                    stream.Write(msg, 0, msg.Length);
                    Console.WriteLine("Sent Result: {0}", contacts.Count);
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
        
        static void Main(string[] args)
        {
            Contact.GenerateContactFile(Searcher.dbFileName);

            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();
                Console.WriteLine("Server started");

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
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}
