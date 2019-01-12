using System;
using System.Collections.Generic;
using System.Data;

using MySql.Data.MySqlClient;

using ReportingServiceDatabase.Classes.Database;
using ReportingServiceDatabase.DataSets;

namespace ReportingServiceDatabase.DataAdapters
{
    public class ReportEventsDataAdapter
    {
        internal MySqlDataAdapter _dataAdapter;
        internal TuDbConnection _connection;

        public ReportEventsDataAdapter(TuDbConnection conn)
        {
            this._dataAdapter = new MySqlDataAdapter();
            this._connection = conn;
        }

        public List<ReportEvent> GetAll()
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM reporting.report_events order by event_id", this._connection.Handle);

            DataTable datatable = new DataTable();

            this._dataAdapter.SelectCommand = cmd;
            this._dataAdapter.Fill(datatable);

            List<ReportEvent> l = new List<ReportEvent>();


            foreach (DataRow row in datatable.Rows)
            {
                ReportEvent e = new ReportEvent
                {
                    Id = Convert.ToUInt64(row["id"]),
                    EventId = Convert.ToUInt64(row["event_id"]),
                    EventData = row["event_data"].ToString()
                };

                l.Add(e);
            }

            return l;
        }

        public List<ReportEvent> GetByEventId(UInt64 eventId, uint maxRows)
        {
            MySqlCommand cmd = new MySqlCommand(String.Format("SELECT * FROM reporting.report_events where event_id = {0} ORDER BY event_date", eventId),
                                                this._connection.Handle);

            DataTable datatable = new DataTable();

            this._dataAdapter.SelectCommand = cmd;
            this._dataAdapter.Fill(datatable);

            List<ReportEvent> l = new List<ReportEvent>();


            foreach (DataRow row in datatable.Rows)
            {
                ReportEvent e = new ReportEvent
                {
                    Id = Convert.ToUInt64(row["id"]),
                    EventId = Convert.ToUInt64(row["event_id"]),
                    EventData = row["event_data"].ToString(),
                    EventDate = row["event_date"].ToString()
                };

                l.Add(e);
            }

            return l;
        }

        public int DeleteEvent(ReportEvent evt)
        {
            MySqlCommand deleteCommand = new MySqlCommand(String.Format("DELETE FROM reporting.report_events where id = {0}", evt.Id), this._connection.Handle);

            return deleteCommand.ExecuteNonQuery();
        }
    }
}
