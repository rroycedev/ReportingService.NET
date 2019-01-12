using System;
using System.Collections.Generic;
using System.Data;

using MySql.Data.MySqlClient;

using ReportingServiceDatabase.Classes.Database;
using ReportingServiceDatabase.DataSets;

namespace ReportingServiceDatabase.DataAdapters
{
    public class EventLogDataAdapter
    {
        internal MySqlDataAdapter _dataAdapter;
        internal TuDbConnection _connection;

        public EventLogDataAdapter(TuDbConnection conn)
        {
            this._dataAdapter = new MySqlDataAdapter();
            this._connection = conn;
        }

        public List<EventLog> GetAll()
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM reporting.event_log order by event_date", this._connection.Handle);

            DataTable datatable = new DataTable();

            this._dataAdapter.SelectCommand = cmd;
            this._dataAdapter.Fill(datatable);

            List<EventLog> l = new List<EventLog>();

           
            foreach (DataRow row in datatable.Rows)
            {
                EventLog e = new EventLog
                {
                    EventLogId = Convert.ToUInt64(row["event_log_id"]),
                    EventId = Convert.ToUInt64(row["event_id"]),
                    EventDate = row["event_date"].ToString(),
                    LogMessage = row["log_message"].ToString()
                };

                l.Add(e);
            }

            return l;
        }

        public int Insert(UInt64 eventId, String msg)
        {
            EventLog e = new EventLog
            {
                EventId = eventId,
                LogMessage = msg
            };

            MySqlCommand insertCommand = new MySqlCommand(String.Format("INSERT INTO reporting.event_log (event_id, log_message) VALUES ({0}, '{1}')",
                        eventId, MySqlHelper.EscapeString(msg)), this._connection.Handle);

            return insertCommand.ExecuteNonQuery();
        }
    }
}
