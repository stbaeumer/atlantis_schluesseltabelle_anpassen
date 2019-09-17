using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;

namespace atlantis_schluesseltabelle_anpassen
{
    public class Schluesseltabelle : List<Schluessel>
    {
        private string connectionStringAtlantis;

        public Schluesseltabelle(string connectionStringAtlantis, string aktSjAtlantis)
        {
            Console.Write("Schluesseltabelleneinträge ".PadRight(75, '.'));

            using (OdbcConnection connection = new OdbcConnection(connectionStringAtlantis))
            {
                DataSet dataSet = new DataSet();
                OdbcDataAdapter schluesselAdapter = new OdbcDataAdapter(@"SELECT DBA.schluessel.sv_id,
DBA.schluessel.kennzeichen,
DBA.schluessel.wert,
DBA.schluessel.aufloesung,
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
                        Console.WriteLine((" " + this.Count.ToString()).PadLeft(30, '.'));
                    }                    
                }                
            }
        }

        internal void ausgeben(object schluesseltabellePfad)
        {
            throw new NotImplementedException();
        }

        internal void ausgeben(string pfad)
        {
            using (StreamWriter outputFile = new StreamWriter(pfad))
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