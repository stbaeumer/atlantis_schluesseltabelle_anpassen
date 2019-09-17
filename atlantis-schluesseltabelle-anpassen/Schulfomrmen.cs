using System.Collections.Generic;
using System.Data;

namespace atlantis_schluesseltabelle_anpassen
{
    public class Schulformen : List<Schulform>
    {
        public Schulformen(DataSet dataSetSchulver)
        {
            foreach (var ds in dataSetSchulver.Tables["[Schulformen]"].AsEnumerable())
            {
                var schulform = new Schulform();
                schulform.ID = ds.Field<string>("Schulform");
                schulform.Kürzel = ds.Field<string>("SF");
                schulform.Bezeichnung = ds.Field<string>("Bezeichnung"); // Name der Schule

                this.Add(schulform);
            }
        }
    }
}