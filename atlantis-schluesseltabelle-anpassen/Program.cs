using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace atlantis_schluesseltabelle_anpassen
{
    class Program
    {
        public const string ConnectionStringAtlantis = @"Dsn=Atlantis9;uid=DBA";
        public static string schluesseltabellePfad = "Schluesseltabelle_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
        public static string insertPfad = "INSERT_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".sql";

        static void Main(string[] args)
        {
            Console.WriteLine("Atlantis2Webuntis (Version 20190914)");
            Console.WriteLine("====================================");
            Console.WriteLine("");

            int sj = (DateTime.Now.Month >= 8 ? DateTime.Now.Year : DateTime.Now.Year - 1);            
            string aktSjAtlantis = sj.ToString() + "/" + (sj + 1 - 2000);
           
            Schluesseltabelle istSchluesseltabelle = new Schluesseltabelle(ConnectionStringAtlantis, aktSjAtlantis);
            DataSet dataSetAsdtabs = AccessDbLoader.LoadFromFile(@"C:\ASDPC32\Hilfstabellen\" + "ASDTABS.MDB");
            DataSet dataSetSchulver = AccessDbLoader.LoadFromFile(@"C:\ASDPC32\Hilfstabellen\" + "schulver.MDB");

            Schulformen schulformen = new Schulformen(dataSetSchulver);
                    
            Schluesseltabelle sollSchulSchluesseltabelle = new Schluesseltabelle(dataSetSchulver, schulformen);

            // Schulen, die in Atlantis fehlen

            Schluesseltabelle neueSchulschluesseltabelle = new Schluesseltabelle();

            sollSchulSchluesseltabelle.FehlendeEinträgeErgänzen(istSchluesseltabelle, insertPfad);
            
            Schluesseltabelle updateSchulschluesseltabelle = new Schluesseltabelle();

            // Für alle Schulen, ...

            foreach (var st in (from s in sollSchulSchluesseltabelle where s.Kennzeichen == "PS-SCHULE" select s).ToList())
            {
                // ... für die eine ID bereits existert, ...

                if ((from u in istSchluesseltabelle where u.Kennzeichen == "PS-SCHULE" where u.Wert == st.Wert select u).Any())
                {
                    // ... wird geprüft, ob ein Update stattfinden muss.    

                    if (!(from x in istSchluesseltabelle
                         where x.Kennzeichen == "PS-SCHULE"
                         where x.Wert == st.Wert
                         where x.Steuerung == (st.Steuerung == "SK"? "SE":st.Steuerung) // Schulform
                          select x
                         ).Any())
                    {
                        // Falls die Schulform abweicht, muss ein Update stattfinden.

                        Console.WriteLine(st.Wert + " Neue Schulform: " + st.Steuerung);

                    }

                    neueSchulschluesseltabelle.Add(st);
                }
            }



            istSchluesseltabelle.ausgeben(schluesseltabellePfad);
            Console.ReadKey();
        }
    }
}
