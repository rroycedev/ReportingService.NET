using System;
using System.Collections.Generic;
using System.Threading;

using ReportingServiceDatabase.DataSets;
using ReportingServiceDatabase.DataAdapters;
using ReportingServiceDatabase.Logging;
using ReportingServiceDatabase.Classes.Database;
using ReportingServiceDatabase.Classes.Exceptions;
using System.Linq;

namespace ReportingService.Classes
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

        public void LoadEvents(string threadId, ulong eventId, ConnectionManager connectionManager)
        {
           // Logger.Debug("Wating for queue list mutex for event id " + eventId);

            bool isOwned = _queueListMutex.WaitOne(15000);

            if (!isOwned)
            {
                Logger.Debug("Unable to get queue list mutex for event id " + eventId);
                return;
            }

            TuDbConnection dbConnection;

            try
            {
            //    Logger.Debug(threadId + " - Getting connection for event id " + eventId);

                dbConnection = connectionManager.GetConnection();

            }
            catch (DbNoConnections dbn)
            {
                _queueListMutex.ReleaseMutex();
                throw dbn;
            }
            catch (MySql.Data.MySqlClient.MySqlException mex)
            {
                _queueListMutex.ReleaseMutex();
                throw mex;
            }
            catch (Exception ex)
            {
                _queueListMutex.ReleaseMutex();
                throw ex;
            }

            ReportEventsDataAdapter reportEventsDataAdapter = new ReportEventsDataAdapter(dbConnection);

            try
            {
           //     Logger.Debug("Getting event by id for event id " + eventId);

                List<ReportEvent> events = reportEventsDataAdapter.GetByEventId(eventId, 50);

           //     Logger.Debug("Retrieved " + events.Count + " events for event id " + eventId);

                foreach (ReportEvent e in events)
                {
                    this._eventQueues[eventId].AddEvent(e);
                }

                connectionManager.Release(dbConnection);

         //      Logger.Debug(threadId + " -  Releasing queue list mutex");

                _queueListMutex.ReleaseMutex();

        //       Logger.Debug(threadId + " -  Releasing queue list mutex again");

                try
                {
                    _queueListMutex.ReleaseMutex();
                }
                catch(Exception)
                {
                  //  Logger.Debug(threadId + " -  2nd Releasing queue list mutex again failed");
                }

               // Logger.Debug(threadId + " -  Done Releasing queue list mutex again");
            }
            catch (Exception ex)
            {
                connectionManager.Release(dbConnection);
                _queueListMutex.ReleaseMutex();
                throw ex;
            }
        }
    }
}



