using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace atlantis_schluesseltabelle_anpassen
{
    public class Schluesseltabelle : List<Schluessel>
    {
        public Schluesseltabelle()
        {
        }

        public Schluesseltabelle(DataSet dataSetSchulver, DataSet dataSetAsdtabs, string kennzeichen)
        {   
            if (kennzeichen == "PS-SCHULE")
            {
                this.AddRange(GetSchulen(dataSetSchulver, dataSetAsdtabs));
            }
            if (kennzeichen == "PS-SCHULART")
            {
                this.AddRange(GetSchulart(dataSetSchulver, dataSetAsdtabs));
            }
        }

        private List<Schluessel> GetSchulen(DataSet dataSetSchulver, DataSet dataSetAsdtabs)
        {
            DataTable schulen = dataSetSchulver.Tables["[DBS]"];
            DataTable schulformen = dataSetAsdtabs.Tables["[Schulformen]"];

            List<Schluessel> x = new List<Schluessel>();

            x = (from schule in schulen.AsEnumerable()
                 join schulform in schulformen.AsEnumerable() on schule.Field<string>("SF") equals schulform.Field<string>("Schulform")
                 //where order.Field<bool>("OnlineOrderFlag") == true
                 //&& order.Field<DateTime>("OrderDate").Month == 8
                 select new Schluessel
                 {
                     Kennzeichen = "PS-SCHULE",
                     Wert = schule.Field<string>("SchulNr"),
                     Aufloesung = (schule.Field<string>("ABez1") + ", " + schule.Field<string>("ABez2")).Replace("'", "''"),
                     Aufloesung2 = (schule.Field<string>("Strasse") + ", " + schule.Field<string>("Plz") + " " + schule.Field<string>("Ort") + ", " + schule.Field<string>("TelVorw") + " " + schule.Field<string>("Telefon")).Replace("'", "''"),
                     Prioritaet = "9",
                     Steuerung = (schulform.Field<string>("SF") == "SK" ? "SE": schulform.Field<string>("SF"))
                 }).ToList();
            
            return getSchluesseltabelle(x);
        }
        
        private List<Schluessel> GetSchulart(DataSet dataSetSchulver, DataSet dataSetAsdtabs)
        {
            List<Schluessel> x = new List<Schluessel>();

            x = (from schulform in dataSetAsdtabs.Tables["[Schulformen]"].AsEnumerable()
                 select new Schluessel
                 {
                     Kennzeichen = "PS-SCHULART",
                     Wert = schulform.Field<string>("SF") == "SK" ? "SE" : schulform.Field<string>("SF"),
                     Aufloesung = (schulform.Field<string>("Bezeichnung")).Replace("'", "''"),
                     Aufloesung2 = ("").Replace("'", "''"),
                     Prioritaet = "9",
                     Steuerung = ""
                 }).ToList();

            return getSchluesseltabelle(x);
        }
        
        private Schluesseltabelle getSchluesseltabelle(List<Schluessel> x)
        {
            Schluesseltabelle schluesseltabelle = new Schluesseltabelle();

            foreach (var schl in x)
            {
                var schluessel = new Schluessel();
                schluessel.Kennzeichen = schl.Kennzeichen;
                schluessel.Wert = schl.Wert;
                schluessel.Aufloesung = schl.Aufloesung;
                schluessel.Aufloesung2 = schl.Aufloesung2;
                schluessel.Prioritaet = schl.Prioritaet;  // 0 = veraltet
                schluessel.Steuerung = schl.Steuerung;
                schluesseltabelle.Add(schluessel);
            }
            return schluesseltabelle;
        }

        public Schluesseltabelle(string connectionStringAtlantis, string aktSjAtlantis)
        {
            Console.Write("Schluesseltabelleneinträge in Atlantis ".PadRight(75, '.'));

            using (OdbcConnection connection = new OdbcConnection(connectionStringAtlantis))
            {
                DataSet dataSet = new DataSet();
                OdbcDataAdapter schluesselAdapter = new OdbcDataAdapter(@"SELECT DBA.schluessel.sv_id,
DBA.schluessel.kennzeichen,
DBA.schluessel.wert,
DBA.schluessel.aufloesung,
DBA.schluessel.aufloesung_2,
DBA.schluessel.prioritaet,
DBA.schluessel.steuerung
FROM DBA.schluessel", connection);

                connection.Open();
                schluesselAdapter.Fill(dataSet, "DBA.schluessel");

                foreach (DataRow theRow in dataSet.Tables["DBA.schluessel"].Rows)
                {
                    try
                    {
                        var schluessel = new Schluessel();
                        if (schluessel != null)
                        {
                            schluessel.SVID = theRow["sv_id"] == null ? -99 : Convert.ToInt32(theRow["sv_id"]);
                            schluessel.Kennzeichen = theRow["kennzeichen"] == null ? "" : theRow["kennzeichen"].ToString();
                            schluessel.Wert = theRow["wert"] == null ? "" : theRow["wert"].ToString();
                            schluessel.Aufloesung = theRow["aufloesung"] == null ? "" : theRow["aufloesung"].ToString();
                            schluessel.Aufloesung2 = theRow["aufloesung_2"] == null ? "" : theRow["aufloesung_2"].ToString();
                            schluessel.Steuerung = theRow["steuerung"] == null ? "" : theRow["steuerung"].ToString();
                            schluessel.Prioritaet = theRow["prioritaet"] == null ? "" : theRow["prioritaet"].ToString();
                            this.Add(schluessel);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        connection.Close();                        
                    }                    
                }
                Console.WriteLine((" " + this.Count.ToString()).PadLeft(30, '.'));
            }
        }

        internal void PrepareINSERT(Schluesseltabelle istSchluesseltabelle, string pfad, string kennzeichen)
        {
            using (StreamWriter outputFile = new StreamWriter(pfad, true, System.Text.Encoding.Default))
            {
                Console.Write(("Neu anzulegende " + kennzeichen + " in Atlantis ").PadRight(75, '.'));

                int i = 0;

                foreach (var st in (from s in this where s.Kennzeichen == kennzeichen select s).ToList())
                {
                    if (!(from u in istSchluesseltabelle where u.Kennzeichen == kennzeichen where u.Wert == st.Wert select u).Any())
                    {
                        string insert = "INSERT INTO " +
                                        "schluessel(" +
                                        "kennzeichen, " +
                                        "wert, " +
                                        "aufloesung, " +
                                        "aufloesung_2, " +
                                        "prioritaet, " +
                                        "steuerung) " +
                                        "VALUES('" +
                                        st.Kennzeichen + "', '" +
                                        st.Wert + "', '" +
                                        st.Aufloesung + "', '" +
                                        st.Aufloesung2 + "','" +
                                        st.Prioritaet + "','" +
                                        st.Steuerung + "');";
                        outputFile.WriteLine(insert);
                        i++;
                    }
                }

                Console.WriteLine((" " + i.ToString()).PadLeft(30, '.'));

                try
                {
                    System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Notepad++\Notepad++.exe", pfad);
                }
                catch (Exception)
                {
                    System.Diagnostics.Process.Start("Notepad.exe", pfad);
                }
            }
        }

        internal void PrepareUPDATE(Schluesseltabelle istSchluesseltabelle, string pfad, string kennzeichen)
        {
            using (StreamWriter outputFile = new StreamWriter(pfad, true, System.Text.Encoding.Default))
            {
                Console.Write((kennzeichen + " mit Updatebedarf ").PadRight(75, '.'));

                int i = 0;

                foreach (var st in (from s in this where s.Kennzeichen == kennzeichen select s).ToList())
                {
                    // ... für die eine ID bereits existert, ...

                    if ((from u in istSchluesseltabelle where u.Kennzeichen == kennzeichen where u.Wert == st.Wert select u).Any())
                    {
                        // ... wird geprüft, ob ein Update stattfinden muss.    

                        if (!(from x in istSchluesseltabelle
                              where x.Kennzeichen == kennzeichen
                              where x.Wert == st.Wert
                              where x.Steuerung == st.Steuerung
                              select x
                             ).Any())
                        {
                            // Falls die Schulform abweicht, muss ein Update stattfinden.
                                                        
                            string update = "UPDATE schluessel " +
                                            "SET steuerung = '" + st.Steuerung + "' " +
                                            "WHERE (kennzeichen = '" + st.Kennzeichen + "' AND wert = '" + st.Wert + "');";
                            outputFile.WriteLine(update);
                            i++;
                        }
                    }
                }

                Console.WriteLine((" " + i.ToString()).PadLeft(30, '.'));

                try
                {
                    System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Notepad++\Notepad++.exe", pfad);
                }
                catch (Exception)
                {
                    System.Diagnostics.Process.Start("Notepad.exe", pfad);
                }

                try
                {
                    Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }                
            }
        }

        internal void PrepareDELETE(Schluesseltabelle schulformenSoll, string pfad, string kennzeichen)
        {
            using (StreamWriter outputFile = new StreamWriter(pfad, true, System.Text.Encoding.Default))
            {
                Console.Write((kennzeichen + " bereit zum Löschen ").PadRight(75, '.'));

                int i = 0;

                foreach (var st in (from s in this where s.Kennzeichen == kennzeichen select s).ToList())
                {
                    if (!(from u in schulformenSoll where u.Kennzeichen == kennzeichen where u.Wert == st.Wert select u).Any())
                    {  
                        string delete = "UPDATE schluessel " +
                                        "SET prioritaet = '0' " +
                                        "WHERE (kennzeichen = '" + st.Kennzeichen + "' AND wert = '" + st.Wert + "');";
                        outputFile.WriteLine(delete);
                        i++;                     
                    }
                }

                Console.WriteLine((" " + i.ToString()).PadLeft(30, '.'));

                try
                {
                    System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Notepad++\Notepad++.exe", pfad);
                }
                catch (Exception)
                {
                    System.Diagnostics.Process.Start("Notepad.exe", pfad);
                }                
            }
        }
        
        internal void ausgeben(object schluesseltabellePfad)
        {
            throw new NotImplementedException();
        }

        internal void ausgeben(string pfad)
        {
            using (StreamWriter outputFile = new StreamWriter(pfad, true, System.Text.Encoding.Default))
            {
                outputFile.WriteLine("SVID Kennzeichen         Wert Aufloesung              Prioritaet Steuerung");
                
                foreach (var schluessel in this)
                {
                    outputFile.WriteLine(schluessel.SVID.ToString().PadLeft(6) + ". " + schluessel.Kennzeichen.PadRight(20) + schluessel.Wert.PadRight(8) + schluessel.Aufloesung.PadRight(30) + schluessel.Prioritaet.PadRight(5) + schluessel.Steuerung.PadRight(10));
                }

                try
                {
                    System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Notepad++\Notepad++.exe", pfad);
                }
                catch (Exception)
                {
                    System.Diagnostics.Process.Start("Notepad.exe", pfad);
                }
            }
        }
    }
}