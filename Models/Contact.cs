using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable()]
    public class Contact
    {
        public string name;
        public string gender;
        public string age;
        public string number;
        public string city;

        static string[] FirstNames = { "Ton", "Ngo", "Nguyen", "Pham", "Kim", "Lee", "Ku", "Park", "Smith", "John", "Tom" };
        static string[] LastNames = { "Quang Thai", "Van Nam", "Huu Quoc", "Kim Loan", "Cruise", "Bond", "Sang-Ho", "Yunkon", "Woo-Sang", "Harward", "Dearman", "Baskin", "Bak", "Ortmann", "Trang" };
        static string[] Genders = { "Male", "Female" };
        static string[] NumberFirst = { "010", "011", "012", "013", "014", "015", "016", "017", "018", "019" };
        static string[] Cities = { "Guadalajara", "Monterrey", "Puebla", "Toluca", "Tijuana", "Chihuahua", "Juárez", "León", "Torreón", "San Luis Potosí", "Mérida", "Acapulco", "Cancún", "Los Cabos", "Cuernavaca", "Pachuca", "Querétaro", "San Salvador", "San Miguel", "Santa Ana", "Toronto", "Montreal", "Calgary", "Edmonton", "Ottawa", "Vancouver" };
        static Random rnd = new Random();

        public static void GenerateContactFile(string fileName){
            if (!File.Exists(fileName))
            { //if exist --> read data
                //Random data --> save to file
                StreamWriter sw = new StreamWriter(fileName);
                int nbContacts = 10000000;
                sw.WriteLine(nbContacts);
                for (int i = 0; i < nbContacts; i++)
                {
                    Contact randomContact = new Contact();
                    sw.WriteLine(randomContact.getString());
                }
                sw.Close();
            }
        }

        public Contact()
        {
            string firstName = FirstNames[rnd.Next(0, FirstNames.Length)];
            string lastName = LastNames[rnd.Next(0, LastNames.Length)];
            string firstNumber = NumberFirst[rnd.Next(0, NumberFirst.Length)];

            this.name = firstName + " " + lastName;
            this.gender = Genders[rnd.Next(0, Genders.Length)];
            this.age = "" + rnd.Next(5, 100);
            this.number = firstNumber + "-" + rnd.Next(1000, 9999) + "-" + rnd.Next(1000, 9999);
            this.city = Cities[rnd.Next(0, Cities.Length)];
        }
        public Contact(string data)
        {
            char[] seperator = { ';' };
            string[] info = data.Split(seperator);
            this.name = info[0];
            this.gender = info[1];
            this.age = info[2];
            this.number = info[3];
            this.city = info[4];
        }

        public string getString()
        {
            return this.name + ";" + this.gender + ";" + this.age + ";" + this.number + ";" + this.city;
        }
    }
}