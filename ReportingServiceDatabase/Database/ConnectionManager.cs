using System;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Threading;
using System.Collections.Generic;

using System.Net.Sockets;
using MySql.Data.MySqlClient;

using ReportingServiceDatabase.Classes.Exceptions;
using static System.Console;

namespace ReportingServiceDatabase.Classes.Database
{
    public class ConnectionManager
    {
        internal List<TuDbConnection> connectionPool = null;
        internal String currentHostIp = "";
        private  static Mutex connectionMutex = new Mutex(false, "dbconnectionpoolmutext");
        internal String connectionName = "";

        public ConnectionManager(String connectionName, int poolSize)
        {
            this.connectionName = connectionName;

            this.connectionPool = new List<TuDbConnection>(poolSize);

            var connectString = System.Configuration.ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;

            var f = DbProviderFactories.GetFactory("MySql.Data.MySqlClient"); //your provider
            var b = f.CreateConnectionStringBuilder();

            b.ConnectionString = connectString;

            String hostname = b["server"].ToString();

            this.currentHostIp = ConnectionManager.HostnameToIp(hostname);

            for (int i = 0; i < poolSize; i++)
            {
                TuDbConnection conn = new TuDbConnection(connectionName, i);

                this.connectionPool.Insert(i,  conn);
            }

        }

        public static String HostnameToIp(String hostname)
        {
            IPHostEntry hostEntry;

            hostEntry = Dns.GetHostEntry(hostname);

            //you might get more than one ip for a hostname since 
            //DNS supports more than one record

            if (hostEntry.AddressList.Length > 0)
            {
                var ip = hostEntry.AddressList[0];

                return ip.ToString();
            }

            return "";
        }


        public TuDbConnection GetConnection(String threadId)
        {
            connectionMutex.WaitOne();

//            Logger.Debug(threadId + " - Getting connection from " + this.connectionPool.Count + " slots ....");

            String connectString;
            DbProviderFactory f;
            DbConnectionStringBuilder b;
            String hostname;
            String hostIp;

            try
            {

                connectString = System.Configuration.ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;

                f = DbProviderFactories.GetFactory("MySql.Data.MySqlClient"); //your provider
                b = f.CreateConnectionStringBuilder();

                b.ConnectionString = connectString;

                hostname = b["server"].ToString();
                hostIp = ConnectionManager.HostnameToIp(hostname);
            }
            catch(Exception ex)
            {
                WriteLine("Internal error: " + ex.Message);
                return null;
            }

            TuDbConnection conn = null;

            try
            {
                for (int i = 0; i < this.connectionPool.Count; i++)
                {
                    if (!this.connectionPool[i].InUse)
                    {
                        this.connectionPool[i].InUse = true;

                        if (hostIp != this.currentHostIp)
                        {
                            WriteLine("ConnectionManager: IP of host has changed");

                            Int32 hostsCleared = 0;
                            Int32 hostsRemaining = 0;

                            for (int j = 0; j < this.connectionPool.Count; j++)
                            {
                                if (!this.connectionPool[j].InUse && this.connectionPool[j].HostIp != hostIp)
                                {
                                    hostsCleared++;
                                    WriteLine(String.Format("ConnectionManager: Clearing pool of connection {0}", j));

                                    connectionPool[j].ClearPool();

                                    this.connectionPool[j].HostIp = hostIp;
                                }
                                else if (this.connectionPool[j].InUse && this.connectionPool[j].HostIp != hostIp)
                                {
                                    WriteLine(String.Format("ConnectionManager: Remaining Poold Id {0}  Connection Id {1} Host Ip {2}", this.connectionPool[j].PoolId, this.connectionPool[j].ConnectionId, this.connectionPool[j].HostIp));

                                    hostsRemaining++;
                                }

                            }

                            if (hostsCleared == 0 && hostsRemaining == 0)
                            {
                                WriteLine("ConnectionManager: All connections have been switched over.  Setting hostIp");

                                this.currentHostIp = hostIp;
                            }
                            else
                            {
                                WriteLine(String.Format("ConnectionManager: {0} cleared {1} remaining", hostsCleared, hostsRemaining));
                            }
                        }

                        else
                        {
                            if (this.connectionPool[i].State == ConnectionState.Open)
                            {
                                this.connectionPool[i].Release();
                            }
                        }

                        this.connectionPool[i].Connect(this.connectionName);

                        conn = this.connectionPool[i];

//                        Logger.Debug(threadId + " - Got connection from " + this.connectionPool.Count + " slots ....");

                        break;
                    }
                }

            }
            catch(Exception ex)
            {
                WriteLine(string.Format("ConnectionManager: Exception while trying to get connection from connection pool: {0}", ex.Message));
                connectionMutex.ReleaseMutex();
                return null;
            }

            ConnectionManager.connectionMutex.ReleaseMutex();

            if (conn == null)
            {
                throw new DbNoConnections();
            }

  //          Logger.Debug(threadId + " - Returning connection from " + this.connectionPool.Count + " slots ....");

            return conn;

        }

        public void Release(TuDbConnection conn)
        {
  //          Logger.Debug(String.Format("ConnectionManager: Waiting for semaphore to release connection {0}", conn.PoolId));

            connectionMutex.WaitOne();


    //        Logger.Debug(String.Format("ConnectionManager: Releasing connection {0}", conn.PoolId));

            this.connectionPool[conn.PoolId].Release();

      //      Logger.Debug(String.Format("ConnectionManager: Releasing semaphore for connection {0}", conn.PoolId));

            connectionMutex.ReleaseMutex();
        }
    }
}
