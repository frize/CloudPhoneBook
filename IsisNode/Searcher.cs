using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Models;

namespace IsisNode
{
    class Searcher
    {
        public static List<string> cache = new List<string>();
        public static void cacheList(){
            int nbContacts = Int32.Parse(File.ReadLines(Searcher.dbFileName).Skip(0).Take(1).ElementAt(0));
            IEnumerable<string> fileLines = File.ReadLines(dbFileName).Skip(1).Take(nbContacts);
            foreach (string line in fileLines)
            {
                cache.Add(line);
            }
        }

        public static string dbFileName = "PhoneBook.txt";
        public List<Contact> search(String search, int index, int nbLines)
        {
            if (cache.Count > 0)
            {
                List<Contact> contacts = new List<Contact>();
                for (int i = index; i < index + nbLines; i++)
                {
                    Contact contact = new Contact(cache[i]);
                    //Console.WriteLine(string.Format("{0}", contact.name));
                    if (contact.name.ToLower().Contains(search.ToLower()))
                    {
                        contacts.Add(contact);
                    }
                }
                return contacts;
            }
            else
            {
                List<Contact> contacts = new List<Contact>();
                IEnumerable<string> fileLines = File.ReadLines(dbFileName).Skip(index + 1).Take(10000000);
                foreach (string line in fileLines)
                {
                    Contact contact = new Contact(line);
                    if (contact.name.ToLower().Contains(search.ToLower()))
                    {
                        contacts.Add(contact);
                    }
                }
                return contacts;

            }            
        }
    }
}
