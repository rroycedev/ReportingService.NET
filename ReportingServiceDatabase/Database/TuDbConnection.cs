using System;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Net.Sockets;
using MySql.Data.MySqlClient;
using System.Threading;

namespace ReportingServiceDatabase.Classes.Database
{
    public class TuDbConnection
    {
        internal MySqlConnection connection = null;
        internal Boolean inUse = false;
        internal Int32 poolId = -1;
        internal String hostIp = "";

        public TuDbConnection(String connectionName, Int32 id)
        {
            this.poolId = id;

            var connectString = System.Configuration.ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;

            var f = DbProviderFactories.GetFactory("MySql.Data.MySqlClient"); //your provider
            var b = f.CreateConnectionStringBuilder();

            b.ConnectionString = connectString;

            String hostname = b["server"].ToString();

            this.hostIp = ConnectionManager.HostnameToIp(hostname);

          //  this.Connect(connectionName);
        }

        public MySqlConnection GetConnection()
        {
            return this.connection;
        }

        public Int32 PoolId
        {
            get
            {
                return this.poolId;
            }
        }

        internal void Connect(String connectionName)
        {
            var connectString = System.Configuration.ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;

            /*
            if (this.connection != null)
            {
                if (this.connection.State == System.Data.ConnectionState.Open)
                {
                    this.connection.Close();
                }

            }
            */

      //      Logger.Debug(String.Format("Connecting with {0}", connectString));

            this.connection = new MySqlConnection
            {
                ConnectionString = connectString
            };

            this.connection.Open();

            var f = DbProviderFactories.GetFactory("MySql.Data.MySqlClient"); //your provider
            var b = f.CreateConnectionStringBuilder();

            b.ConnectionString = connectString;

            String hostname = b["server"].ToString();

            this.hostIp = ConnectionManager.HostnameToIp(hostname);

        }

        public System.Data.ConnectionState State
        {
            get
            {
                if (this.connection == null)
                {
                    return ConnectionState.Closed;
                }

                return this.connection.State;
            }
        }

        public Boolean InUse
        {
            get
            {
                return this.inUse;
            }
            set
            {
                this.inUse = value;
            }
        }

        public String HostIp
        {
            get
            {
                return this.hostIp;
            }
            set
            {
                this.hostIp = value;
            }
        }

        public int ConnectionId
        {
            get
            {
                if (this.connection == null || this.connection.State != ConnectionState.Open)
                {
                    return -1;
                }

                return this.connection.ServerThread;
            }
        }


        public MySqlConnection Handle
        {
            get
            {
                if (this.connection == null)
                {
                    return null;
                }

                return this.connection;
            }
        }

        public void Release()
        {
            if (this.connection.State == ConnectionState.Open)
            {
                this.connection.Close();
            }

            this.InUse = false;
        }

        public void ClearPool()
        {
            if (this.connection != null)
            {
                MySqlConnection.ClearPool(this.connection);

                this.Release();
            }
        }
    }
}
