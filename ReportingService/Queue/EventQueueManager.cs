using System;
using System.Collections.Generic;
using System.Threading;

using ReportingServiceDatabase.DataSets;
using ReportingServiceDatabase.DataAdapters;
using ReportingServiceDatabase.Logging;
using ReportingServiceDatabase.Classes.Database;
using ReportingServiceDatabase.Classes.Exceptions;
using System.Linq;

namespace ReportingService.Queue
{
    public class EventQueueManager
    {
        internal Dictionary<ulong, EventQueue> _eventQueues;
        private static Mutex _queueListMutex = new Mutex(false, "queuelistmutex");

        public EventQueueManager()
        {
            this._eventQueues = new Dictionary<ulong, EventQueue>();
        }

        public void AddEvent(ulong eventId, ReportEvent e)
        {
            _queueListMutex.WaitOne(15000);

            this._eventQueues[eventId].AddEvent(e);

            _queueListMutex.ReleaseMutex();
        }

        public ReportEvent GetEvent(ulong eventId, ConnectionManager cm, String threadId)
        {
            var isOwned = _queueListMutex.WaitOne(15000);

            if (!isOwned)
            {
                Logger.Debug("Unable to get queue list mutex for event id " + eventId);
                return null;
            }

            ReportEvent evt = this._eventQueues[eventId].GetEvent();

            if (evt == null)
            {
                _queueListMutex.ReleaseMutex();
                return null;
            }

            try
            {
                this.DeleteEventFromTable(evt, cm);
            }
            catch (DbNoConnections dbn)
            {
                this._eventQueues[eventId].AddEvent(evt);
                _queueListMutex.ReleaseMutex();
                throw dbn;
            }
            catch (MySql.Data.MySqlClient.MySqlException mex)
            {
                this._eventQueues[eventId].AddEvent(evt);
                _queueListMutex.ReleaseMutex();
                throw mex;
            }
            catch (Exception ex)
            {
                this._eventQueues[eventId].AddEvent(evt);
                _queueListMutex.ReleaseMutex();
                throw ex;
            }
            finally
            {
                // Logger.Debug(threadId + " - Releasing queue list mutext for event id " + eventId);

                _queueListMutex.ReleaseMutex();

              //   Logger.Debug(String.Format(threadId + " - Found event for event id {0}", eventId));
            }

            return evt;
        }


        public void DefineEvent(ulong eventId)
        {
            _queueListMutex.WaitOne(15000);

            this._eventQueues[eventId] = new EventQueue();

            _queueListMutex.ReleaseMutex();
        }

        private void DeleteEventFromTable(ReportEvent evt, ConnectionManager cm)
        {
            TuDbConnection dbConnection;

            try
            {

                dbConnection = cm.GetConnection();

            }
            catch (DbNoConnections dbn)
            {
                throw dbn;
            }
            catch (MySql.Data.MySqlClient.MySqlException mex)
            {
                throw mex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            ReportEventsDataAdapter reportEventsDataAdapter = new ReportEventsDataAdapter(dbConnection);

            try
            {
                reportEventsDataAdapter.DeleteEvent(evt);

                cm.Release(dbConnection);
            }
            catch (Exception ex)
            {
                cm.Release(dbConnection);
                throw ex;
            }

        }

        private void ReleaseQueueListMutex(bool wantException = true)
        {
            if (wantException)
            {
                _queueListMutex.ReleaseMutex();
            }
            else
            {
                try
                {
                    _queueListMutex.ReleaseMutex();
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                {
                }
            }
        }

        private void ReleaseDbConnection(ConnectionManager connectionManager, TuDbConnection dbConnection, bool wantException = true)
        {
            if (wantException)
            {
                connectionManager.Release(dbConnection);
            }
            else
            {
                try
                {
                    connectionManager.Release(dbConnection);
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                {

                }
            }
        }

        public void LoadEvents(string threadId, ulong eventId, ConnectionManager connectionManager)
        {
            //  Get access to the "Queue List" mutex

            bool isOwned = _queueListMutex.WaitOne(15000);

            if (!isOwned)
            {
                Logger.Debug("Unable to get queue list mutex for event id " + eventId);
                return;
            }

            //  Get connection to database from pool

            TuDbConnection dbConnection;

            try
            {
                dbConnection = connectionManager.GetConnection();
            }
            catch (DbNoConnections dbn)
            {
                //  Since throwing an exception, ignore any exceptions along the way
                ReleaseQueueListMutex(false);
                throw dbn;
            }
            catch (MySql.Data.MySqlClient.MySqlException mex)
            {
                //  Since throwing an exception, ignore any exceptions along the way
                ReleaseQueueListMutex(false);
                throw mex;
            }
            catch (Exception ex)
            {
                //  Since throwing an exception, ignore any exceptions along the way
                ReleaseQueueListMutex(false);
                throw ex;
            }

            //  Create the "Report Events" data adapter

            ReportEventsDataAdapter reportEventsDataAdapter = new ReportEventsDataAdapter(dbConnection);

            try
            {
                //  Query the "report_events" table for events with id = "eventId"

                UInt32 maxEventsToQueue = Convert.ToUInt32(ReportingServiceDatabase.Configuration.ConfigurationReader.GetAppSetting("maxeventstoqueue"));

                List<ReportEvent> events = reportEventsDataAdapter.GetByEventId(eventId, maxEventsToQueue);

                ReleaseDbConnection(connectionManager, dbConnection);

                foreach (ReportEvent e in events)
                {
                    this._eventQueues[eventId].AddEvent(e);
                }

                ReleaseQueueListMutex();
            }
            catch (Exception ex)
            {
                //  Since throwing an exception, ignore any exceptions along the way

                ReleaseDbConnection(connectionManager, dbConnection, false);
                ReleaseQueueListMutex(false);
                throw ex;
            }
            finally
            {

            }
        }
    }
}



