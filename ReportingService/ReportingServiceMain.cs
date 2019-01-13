using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using System.Reflection;

using ReportingServiceDatabase.DataSets;
using ReportingServiceDatabase.Logging;
using ReportingServiceDatabase.Classes.Database;
using ReportingServiceDatabase.Configuration;
using ReportingServiceDatabase.Classes.Exceptions;
using ReportingService.Reflection;
using ReportingService.Queue;
using ReportingService.DataAdapters;
using ReportingService.DataSets;

namespace ReportingService
{
    public static class ReportingServiceMain
    {
        public static EventQueueManager eventsQueueManager = new EventQueueManager();
        public static ConnectionManager connectionManager = null;

        public static Int32 Start()
        {
            Int32 maxEventWorkerThreads = Convert.ToInt32(ConfigurationReader.GetAppSetting("maxeventworkers"));

            connectionManager = new ConnectionManager("reportingwritemgr", maxEventWorkerThreads);

            TuDbConnection conn;

            try
            {
                conn = connectionManager.GetConnection();
            }
            catch (Exception ex)
            {
                Logger.Debug("Error connecting to reporting database: " + ex.Message);
                return 1;
            }

            List<Thread> threads = new List<Thread>();
            Dictionary<UInt64, EventQueue> eventQueues = new Dictionary<UInt64, EventQueue>();
            List<SupportedEventProcessor> supportedEvents = new List<SupportedEventProcessor>();
            SupportedEventProcessorDataAdapter supportedEventProcessorDataAdapter = new SupportedEventProcessorDataAdapter(conn);


            try
            {
                supportedEvents = supportedEventProcessorDataAdapter.GetAll();

                foreach (SupportedEventProcessor e in supportedEvents)
                {
                    eventsQueueManager.DefineEvent(e.EventId);
                }

                //  Spawn worker threads

                foreach (SupportedEventProcessor eventProcessorInfo in supportedEvents)
                {
              //      Logger.Debug("Starting next thread");

                    Thread t = new Thread(new ParameterizedThreadStart(ReportingServiceMain.Worker));

                    t.Start(eventProcessorInfo);

                    threads.Add(t);

                    Thread.Sleep(1000);
                }

                //   Logger.Debug(String.Format("Worker {0}: Releasing connection {1}", threadId, dbConnection.PoolId));

                connectionManager.Release(conn);

                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                Logger.Debug(String.Format("Error starting threads: {0}", ex.Message));
                Thread.Sleep(5000);
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }

            return 0;
        }


        private static void Worker(object cm)
        {
        //    Logger.Debug("Inside worker thread....");

            SupportedEventProcessor eventProcessorInfo = (SupportedEventProcessor)cm;

            ulong thisEventId = eventProcessorInfo.EventId;
            ulong sleepTime = thisEventId * 2;

            String threadId = String.Format("{0}({1})", eventProcessorInfo.Name, eventProcessorInfo.EventId);
            Assembly asm = null;

            try
            {
                asm = ReflectionManager.LoadAssembly(eventProcessorInfo.AssemblyFilename);

            }
            catch (Exception ex)
            {
                Logger.Debug(String.Format("Worker {0}: Exception in querying logic: {1}", threadId, ex.Message));
                Thread.Sleep(5000);
            }

            bool firstTime = true;

            while (true)
            {
                if (firstTime)
                {
                  //  Logger.Debug(String.Format("{0} - Loading events from database table...", threadId));

                    try
                    {
                        eventsQueueManager.LoadEvents(threadId, thisEventId, connectionManager);
                    }
                    catch(Exception ex)
                    {
                        Logger.Error("Unable to load events: " + ex.Message);
                        continue;
                    }

                    firstTime = false;
                }

                //  Get next event for this event id.

              // Logger.Debug(String.Format("{0} - Getting next event...", threadId));

                ReportEvent evt;

                try
                {
                    evt = eventsQueueManager.GetEvent(thisEventId, connectionManager, threadId);
                }
                catch(Exception ex)
                {
                    Logger.Debug(String.Format("{0} - Error getting next event: {1}", threadId, ex.Message));
                    Thread.Sleep(2000);
                    continue;
                }

                if (evt == null)
                {
                    Logger.Debug(String.Format("{0} - No events to process", threadId));

                    Thread.Sleep(5000);

                    firstTime = true;

                    continue;
                }

                //  Received an event for this event id.  Process the event

          //      Logger.Debug(String.Format("{0} - Processing event {1}", threadId, evt.Id));

                try
                {
                    ReflectionManager.ProcessEvent(asm, eventProcessorInfo.ClassName, eventProcessorInfo.MethodName, evt, connectionManager, (int)sleepTime);
                }
                catch (EventProcessorClassNotFound)
                {
                    Logger.Debug("Internal error: Class SharedLibrary.MyClass not found");
                    return;
                }
                catch (EventProcessorClassEntryNotImplemented)
                {
                    Logger.Debug("Internal error: Start method in class SharedLibrary.MyClass not found");
                    return;
                }
                catch (Exception ex3)
                {
                    Logger.Debug("Error: " + ex3.Message);
                    return;
                }




             //   Logger.Debug(String.Format("{0}:  Id: {1}  Event Id: {2}  Event Date {3}  Event Data {4}", 
              //                  threadId, evt.Id, evt.EventId, evt.EventDate, evt.EventData));

            }   // END while(true)

        }   //  END Worker

    }   //  END Class ReportingService Main
}

