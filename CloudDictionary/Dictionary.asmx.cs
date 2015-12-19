using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using Models;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Sockets;

namespace CloudDictionary
{
    /// <summary>
    /// Summary description for Dictionary
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]

    public class Dictionary : System.Web.Services.WebService
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

        static string fileName = HttpContext.Current.Request.MapPath("PhoneBook.txt");

         [WebMethod]
        public List<Contact> Search(string search)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 13000;
                string server = "127.0.0.1";
                TcpClient client = new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(search);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent Search Request: {0}", search);

                // Receive the TcpServer.response.
                byte[] resData;

                // Read the first batch of the TcpServer response bytes.
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    int fileSize = Int32.Parse(System.Text.Encoding.ASCII.GetString(buffer, 0, buffer.Length));
                    while (fileSize > 0 && (read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                        fileSize -= read;
                    }
                    resData = ms.ToArray();
                }
                List<Contact> contacts = ByteArrayToObject(resData) as List<Contact>;
                Console.WriteLine("Received Search Result: {0}", search);

                // Close everything.
                stream.Close();
                client.Close();

                return contacts;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            return new List<Contact>(); //in case error;
        }
        
    }
}
