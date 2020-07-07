using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace atlantis_schluesseltabelle_anpassen
{
    class Program
    {
        public const string ConnectionStringAtlantis = @"Dsn=Atlantis9;uid=DBA";
        public static string SchluesseltabellePfad = "Schluesseltabelle_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
        public static string Pfad = DateTime.Now.ToString("yyyyMMddHHmmss") + ".sql";
        public static string AktSjAtlantis = (DateTime.Now.Month >= 8 ? DateTime.Now.Year : DateTime.Now.Year - 1).ToString() + "/" + ((DateTime.Now.Month >= 8 ? DateTime.Now.Year : DateTime.Now.Year - 1) + 1 - 2000);

        static void Main(string[] args)
        {
            Console.WriteLine("atlantis_schluesseltabelle_anpassen (Version 20200707)");
            Console.WriteLine("====================================");
            Console.WriteLine("");
            Console.WriteLine("");

            System.Diagnostics.Process.Start("https://www.svws.nrw.de/download/schild-nrw/hilfstabellen");

            Process.Start("explorer.exe", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                        
            Console.WriteLine("1. Laden Sie die Hilfstabellen herunter:");
            Console.WriteLine("2. ASDTAB nach " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + " entpacken.");
            Console.WriteLine("3. schulver nach " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + " entpacken.");
            Console.WriteLine("4. statkue nach " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + " entpacken.");
            Console.WriteLine("4. ENTER");

            Console.ReadKey();

            Schluesseltabelle istSchluesseltabelle = new Schluesseltabelle(ConnectionStringAtlantis, AktSjAtlantis);
            DataSet dataSetAsdtabs = AccessDbLoader.LoadFromFile(@"ASDTABS.MDB");
            DataSet dataSetSchulver = AccessDbLoader.LoadFromFile(@"schulver.MDB");
            
            // Schulformen

            Schluesseltabelle SchulformenSoll = new Schluesseltabelle(dataSetSchulver, dataSetAsdtabs, "PS-SCHULART");
            SchulformenSoll.PrepareINSERT(istSchluesseltabelle, "INSERT_Schulform_" + Pfad, "PS-SCHULART");
            SchulformenSoll.PrepareUPDATE(istSchluesseltabelle, "UPDATE_Schulform_" + Pfad, "PS-SCHULART");
            istSchluesseltabelle.PrepareDELETE(SchulformenSoll, "DELETE_Schulform_" + Pfad, "PS-SCHULART");

            // Schulen

            Schluesseltabelle SchulenSoll = new Schluesseltabelle(dataSetSchulver, dataSetSchulver, "PS-SCHULE");
            SchulenSoll.PrepareINSERT(istSchluesseltabelle, "INSERT_Schule_" + Pfad, "PS-SCHULE");
            SchulenSoll.PrepareUPDATE(istSchluesseltabelle, "UPDATE_Schule_" + Pfad, "PS-SCHULE");
                        
            istSchluesseltabelle.ausgeben(SchluesseltabellePfad);
            Console.ReadKey();
        }
    }
}
