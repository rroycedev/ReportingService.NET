using System;
using System.Collections.Generic;
using System.Data;

using MySql.Data.MySqlClient;

using ReportingServiceDatabase.Classes.Database;
using ReportingService.DataSets;

namespace ReportingService.DataAdapters
{
    public class SupportedEventProcessorDataAdapter
    {
        internal MySqlDataAdapter _dataAdapter;
        internal TuDbConnection _connection;

        public SupportedEventProcessorDataAdapter(TuDbConnection conn)
        {
            this._dataAdapter = new MySqlDataAdapter();
            this._connection = conn;
        }

        public List<SupportedEventProcessor> GetAll()
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM reporting.event_processors order by event_id", this._connection.Handle);

            DataTable datatable = new DataTable();

            this._dataAdapter.SelectCommand = cmd;
            this._dataAdapter.Fill(datatable);

            List<SupportedEventProcessor> l = new List<SupportedEventProcessor>();

           
            foreach (DataRow row in datatable.Rows)
            {
                SupportedEventProcessor e = new SupportedEventProcessor();

                e.AssemblyFilename = row["assembly_filename"].ToString();
                e.ClassName = row["class_name"].ToString();
                e.Description = row["description"].ToString();
                e.EventId = Convert.ToUInt64(row["event_id"]);
                e.MethodName = row["method_name"].ToString();
                e.Name = row["name"].ToString();

                l.Add(e);
            }

            return l;
        }
    }
}
