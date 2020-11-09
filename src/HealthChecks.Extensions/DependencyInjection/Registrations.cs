// ReSharper disable All
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// A list of names of well known community health checks.
    /// </summary>
    public static class Registrations
    {
        /// <summary>
        /// The name of ArangoDb health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string ArangoDB = "arangodb";

        /// <summary>
        /// The name of AzureBlobStorage health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string AzureBlobStorage = "azureblob";

        /// <summary>
        /// The name of AzureCosmosDb health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string AzureCosmosDb = "cosmosdb";

        /// <summary>
        /// The name of AzureEventHub health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string AzureEventHub = "azureeventhub";

        /// <summary>
        /// The name of AzureIoTHub health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string AzureIoTHub = "iothub";

        /// <summary>
        /// The name of AzureKeyVault health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string AzureKeyVault = "azurekeyvault";

        /// <summary>
        /// The name of AzureServiceBusQueue health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string AzureServiceBusQueue = "azurequeue";

        /// <summary>
        /// The name of AzureServiceBusTopic health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string AzureServiceBusTopic = "azuretopic";

        /// <summary>
        /// The name of AzureServiceBusSubscription health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string AzureServiceBusSubscription = "azuresubscription";

        /// <summary>
        /// The name of AzureTable health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string AzureTable = "azuretable";

        /// <summary>
        /// The name of AzureQueueStorage health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string AzureQueueStorage = "azurequeue";

        /// <summary>
        /// The name of CloudFirestore health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string CloudFirestore = "cloud firestore";

        /// <summary>
        /// The name of Consul health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Consul = "consul";

        /// <summary>
        /// The name of DiskStorage health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string DiskStorage = "diskstorage";

        /// <summary>
        /// The name of DnsResolve health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string DnsResolve = "dns";

        /// <summary>
        /// The name of DocumentDB health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string DocumentDB = "documentdb";

        /// <summary>
        /// The name of DynamoDB health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string DynamoDB = "dynamodb";

        /// <summary>
        /// The name of ElasticSearch health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string ElasticSearch = "elasticsearch";

        /// <summary>
        /// The name of Eventstore health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Eventstore = "eventstore";

        /// <summary>
        /// The name of FTP health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Ftp = "ftp";

        /// <summary>
        /// The name of Gremlin health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Gremlin = "gremlin";

        /// <summary>
        /// The name of Hangfire health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Hangfire = "hangfire";

        /// <summary>
        /// The name of IbmMQ health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string IbmMQ = "ibmmq";

        /// <summary>
        /// The name of IdentityServer health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string IdentityServer = "idsvr";

        /// <summary>
        /// The name of Imap health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Imap = "imap";

        /// <summary>
        /// The name of Kafka health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Kafka = "kafka";

        /// <summary>
        /// The name of Kubernetes health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Kubernetes = "k8s";

        /// <summary>
        /// The name of MongoDb health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string MongoDb = "mongodb";

        /// <summary>
        /// The name of MySql health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string MySql = "mysql";

        /// <summary>
        /// The name of NpgSql health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string NpgSql = "npgsql";

        /// <summary>
        /// The name of Oracle health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Oracle = "oracle";

        /// <summary>
        /// The name of Ping health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Ping = "ping";

        /// <summary>
        /// The name of PrivateMemory health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string PrivateMemory = "privatememory";

        /// <summary>
        /// The name of ProcessAllocatedMemory health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string ProcessAllocatedMemory = "process_allocated_memory";

        /// <summary>
        /// The name of ProcessHealth health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string ProcessHealth = "process";

        /// <summary>
        /// The name of RabbitMQ health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string RabbitMQ = "rabbitmq";

        /// <summary>
        /// The name of RavenDB health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string RavenDB = "ravendb";

        /// <summary>
        /// The name of Redis health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Redis = "redis";

        /// <summary>
        /// The name of S3 health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string S3 = "aws s3";

        /// <summary>
        /// The name of SendGrid health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string SendGrid = "sendgrid";

        /// <summary>
        /// The name of Seq health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Seq = "seq";

        /// <summary>
        /// The name of Sftp health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Sftp = "sftp";

        /// <summary>
        /// The name of SignalR health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string SignalR = "signalr";

        /// <summary>
        /// The name of Smtp health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Smtp = "smtp";

        /// <summary>
        /// The name of Solr health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Solr = "solr";

        /// <summary>
        /// The name of Sqlite health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Sqlite = "sqlite";

        /// <summary>
        /// The name of SqlServer health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string SqlServer = "sqlserver";

        /// <summary>
        /// The name of Tcp health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string Tcp = "tcp";

        /// <summary>
        /// The name of UrlGroup health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string UrlGroup = "uri-group";

        /// <summary>
        /// The name of VirtualMemorySize health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string VirtualMemorySize = "virtualmemory";

        /// <summary>
        /// The name of WindowsService health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string WindowsService = "windowsservice";

        /// <summary>
        /// The name of WorkingSet health check implemented by https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        /// </summary>
        public static readonly string WorkingSet = "workingset";
    }
}
