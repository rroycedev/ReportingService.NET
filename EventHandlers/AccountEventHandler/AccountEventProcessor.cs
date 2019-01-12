using System;
using System.Threading;

using ReportingServiceDatabase.DataSets;
using ReportingServiceDatabase.DataAdapters;
using ReportingServiceDatabase.Logging;
using ReportingServiceDatabase.Classes.Database;

namespace AccountEventHandler
{
    public class AccountEventProcessor
    {
        public AccountEventProcessor()
        {
        }

        public void Start(ReportEvent evt, ConnectionManager cm, int sleepTime)
        {
            TuDbConnection dbConnection = null;

            try
            {
                dbConnection = cm.GetConnection("Main");
            }
            catch(Exception ex)
            {
                Logger.Debug(String.Format("Error connecting to database: {0}", ex.Message));
                return;
            }

            EventLogDataAdapter eventLogDataAdapter = new EventLogDataAdapter(dbConnection);

            try
            {
                eventLogDataAdapter.Insert(evt.EventId, String.Format("Event has been processed for event id {0}  event data [{1}]  event date [{2}]", evt.EventId, evt.EventData, evt.EventDate));

                Logger.Debug(String.Format("Connection " + dbConnection.ConnectionId + " - has been processed for event id {0}  event data [{1}]  event date [{2}]", evt.EventId, evt.EventData, evt.EventDate));

                Thread.Sleep(sleepTime * 1000);

                cm.Release(dbConnection);

            }
            catch (Exception ex)
            {
                Logger.Debug(String.Format("Error starting threads: {0}", ex.Message));
            }
        }
    }
}
