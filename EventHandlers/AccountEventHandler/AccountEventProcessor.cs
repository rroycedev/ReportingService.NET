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

        public void AddAccount(ReportEvent evt, ConnectionManager cm, int sleepTime)
        {
            TuDbConnection dbConnection = null;

            try
            {
                dbConnection = cm.GetConnection();
            }
            catch(Exception ex)
            {
                Logger.Debug(String.Format("Error connecting to database: {0}", ex.Message));
                return;
            }

            EventLogDataAdapter eventLogDataAdapter = new EventLogDataAdapter(dbConnection);

            try
            {
                String msg = String.Format("Add Account Event has been processed for event id {0}  event data [{1}]  event date [{2}]", evt.EventId, evt.EventData, evt.EventDate);

                eventLogDataAdapter.Insert(evt.EventId, msg);

                Logger.Debug("Connection " + dbConnection.ConnectionId + " - " + msg);

                Thread.Sleep(sleepTime * 1000);

                cm.Release(dbConnection);

            }
            catch (Exception ex)
            {
                Logger.Debug(String.Format("Error starting threads: {0}", ex.Message));
            }
        }

        public void UpdateAccount(ReportEvent evt, ConnectionManager cm, int sleepTime)
        {
            TuDbConnection dbConnection = null;

            try
            {
                dbConnection = cm.GetConnection();
            }
            catch (Exception ex)
            {
                Logger.Debug(String.Format("Error connecting to database: {0}", ex.Message));
                return;
            }

            EventLogDataAdapter eventLogDataAdapter = new EventLogDataAdapter(dbConnection);

            try
            {
                String msg = String.Format("Update Account Event has been processed for event id {0}  event data [{1}]  event date [{2}]", evt.EventId, evt.EventData, evt.EventDate);

                eventLogDataAdapter.Insert(evt.EventId, msg);

                Logger.Debug("Connection " + dbConnection.ConnectionId + " - " + msg);

                Thread.Sleep(sleepTime * 1000);

                cm.Release(dbConnection);

            }
            catch (Exception ex)
            {
                Logger.Debug(String.Format("Error starting threads: {0}", ex.Message));
            }
        }

        public void DeleteAccount(ReportEvent evt, ConnectionManager cm, int sleepTime)
        {
            TuDbConnection dbConnection = null;

            try
            {
                dbConnection = cm.GetConnection();
            }
            catch (Exception ex)
            {
                Logger.Debug(String.Format("Error connecting to database: {0}", ex.Message));
                return;
            }

            EventLogDataAdapter eventLogDataAdapter = new EventLogDataAdapter(dbConnection);

            try
            {
                String msg = String.Format("Delete Account Event has been processed for event id {0}  event data [{1}]  event date [{2}]", evt.EventId, evt.EventData, evt.EventDate);

                eventLogDataAdapter.Insert(evt.EventId, msg);

                Logger.Debug("Connection " + dbConnection.ConnectionId + " - " + msg);

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
