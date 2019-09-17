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
        public static string schluesseltabellepFad = "Schluesseltabelle_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";

        static void Main(string[] args)
        {
            Console.WriteLine("Atlantis2Webuntis (Version 20190914)");
            Console.WriteLine("====================================");
            Console.WriteLine("");

            int sj = (DateTime.Now.Month >= 8 ? DateTime.Now.Year : DateTime.Now.Year - 1);            
            string aktSjAtlantis = sj.ToString() + "/" + (sj + 1 - 2000);
           

        Schluesseltabelle schluesseltabelle = new Schluesseltabelle(ConnectionStringAtlantis, aktSjAtlantis);
        DataSet dataSetAsdtabs = AccessDbLoader.LoadFromFile(@"C:\ASDPC32\Hilfstabellen\" + "ASDTABS.MDB");
        DataSet dataSetSchulver = AccessDbLoader.LoadFromFile(@"C:\ASDPC32\Hilfstabellen\" + "schulver.MDB");



        


            schluesseltabelle.ausgeben(schluesseltabellepFad);
        Console.ReadKey();
        }
    }
}
