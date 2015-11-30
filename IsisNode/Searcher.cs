using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using System.IO;

namespace IsisNode
{
    class Searcher
    {
        public static string dbFileName = "PhoneBook.txt";
        public List<Contact> search(String search, int index, int nbLines)
        {
            List<Contact> contacts = new List<Contact>();
            IEnumerable<string> fileLines = File.ReadLines(dbFileName).Skip(index + 1).Take(nbLines);
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
