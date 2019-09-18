using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace atlantis_schluesseltabelle_anpassen
{
    internal class Schulen:List<Schule>
    {
        private DataSet dataSetSchulver;

        public Schulen(DataSet dataSetSchulver, Schulformen schulformen)
        {
            foreach (var ds in dataSetSchulver.Tables["[Schulen]"].AsEnumerable())
            {
                var schule = new Schule();
                schule.Kennzeichen = "PS-SCHULE";
                schule.Wert = ds.Field<string>("Schulnr");                   // Schulnummer
                schule.Aufloesung = (ds.Field<string>("ABez1") + ", " + ds.Field<string>("ABez2")).Replace("'", "''"); ; // Name der Schule               
                schule.Aufloesung2 = (ds.Field<string>("Strasse") + ", " + ds.Field<string>("Plz") + " " + ds.Field<string>("Ort") + ", " + ds.Field<string>("TelVorw") + " " + ds.Field<string>("Telefon")).Replace("'", "''");
                schule.Prioritaet = "9";  // 0 = veraltet
                schule.Steuerung = (from s in schulformen where s.ID == ds.Field<string>("SF") select s.Kürzel).FirstOrDefault();
                this.Add(schule);
            }
        }
    }
}