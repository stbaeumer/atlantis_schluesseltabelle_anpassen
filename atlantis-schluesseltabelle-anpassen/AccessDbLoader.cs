using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace atlantis_schluesseltabelle_anpassen
{
    public class AccessDbLoader
    {
        // http://sizious.com/2015/05/11/how-to-load-an-access-database-into-a-dataset-object-in-c/

        /// <summary>
        /// Loads a Microsoft Access Database file into a DataSet object.
        /// The file can be the in the newer ACCDB format or MDB legacy format.
        /// </summary>
        /// <param name="fileName">The file name to load.</param>
        /// <returns>A DataSet object with the Tables object populated with the contents of the specified Microsoft Access Database.</returns>
        public static DataSet LoadFromFile(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                    throw new Exception("Die Datei " + fileName + " existiert nicht.");

                Console.WriteLine(Path.GetFileName(fileName) + " ...");
                DataSet result = new DataSet()
                {
                    // For convenience, the DataSet is identified by the name of the loaded file (without extension).
                    DataSetName = Path.GetFileNameWithoutExtension(fileName).Replace(" ", "_")
                };

                // Compute the ConnectionString (using the OLEDB v12.0 driver compatible with ACCDB and MDB files)
                fileName = Path.GetFullPath(fileName);
                string connString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};User Id=Admin;Password=", fileName);

                //string connString = Global.ConnectionAsdpc + fileName;

                // Opening the Access connection
                using (OleDbConnection conn = new OleDbConnection(connString))
                {
                    conn.Open();

                    // Getting all user tables present in the Access file (Msys* tables are system thus useless for us)
                    DataTable dt = conn.GetSchema("Tables");
                    List<string> tablesName = dt.AsEnumerable().Select(dr => dr.Field<string>("TABLE_NAME")).Where(dr => !dr.StartsWith("MSys")).Where(dr => !dr.StartsWith("~")).ToList();

                    // Getting the data for every user tables
                    foreach (string tableName in tablesName)
                    {
                        using (OleDbCommand cmd = new OleDbCommand(string.Format("SELECT * FROM [{0}]", tableName), conn))
                        {
                            using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                            {
                                // Saving all tables in our result DataSet.
                                DataTable buf = new DataTable("[" + tableName + "]");
                                adapter.Fill(buf);
                                result.Tables.Add(buf);

                            } // adapter
                        } // cmd
                    } // tableName
                } // conn
                
                return result;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return null;
            }
        }

        public static void DumpDataSet(DataSet ds)
        {
            Console.Out.WriteLine("DataSet: {0}", ds.DataSetName);

            // For every tables in the DataSet ...
            foreach (DataTable dt in ds.Tables)
            {
                Console.Out.WriteLine("\tTableName: {0}", dt.TableName);

                // ... Write the table schema
                foreach (DataColumn col in dt.Columns)
                {
                    Console.Out.Write("\t\t" + col.ColumnName + " ");
                }
                Console.Out.WriteLine("\t\t");

                // ... Write the table contents
                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        Console.Out.Write("\t\t" + row[i]);
                    }
                    Console.Out.WriteLine("");
                    Console.ReadKey();
                }
            }
        }
    }
}