using Core.Pool;
using Microsoft.Data.Sqlite;
using System.Text;
namespace Database
{
    public class LocalSQLite:IDisposable
    {
        private const int MAX_N_CONNECTIONS_DEFAULT = 100;
        private const int MAX_WAITING_BEFORE_BACKED_UP_DEFAULT= 120;
        private const int TIMEOUT_MILLISECONDS_TAKE_CONNECTION_FROM_POOL_DEFAULT = 1000;


        private int _MaxNConnections;
        private int _MaxWaitingBeforeBackedUp;
        private int _TimeoutMillisecondsTakeConnectionFromPool;
        private bool _Disposed = false;
        private readonly object _LockObjectDispose = new object(),
            _LockObjectWrite = new object();
        private readonly string _ConnectionString;
        private ObjectPool<SqliteConnection> _ConnectionsPool;
        public LocalSQLite(string filePath, bool useUTF16, 
            int maxNConnections = MAX_N_CONNECTIONS_DEFAULT, 
            int maxWaitingBeforeBackedUp = MAX_WAITING_BEFORE_BACKED_UP_DEFAULT,
            int timeoutMillisecondsTakeConnectionFromPool = TIMEOUT_MILLISECONDS_TAKE_CONNECTION_FROM_POOL_DEFAULT) {
            _MaxNConnections = maxNConnections;
            _MaxWaitingBeforeBackedUp= maxWaitingBeforeBackedUp;
            _TimeoutMillisecondsTakeConnectionFromPool= timeoutMillisecondsTakeConnectionFromPool;
            if (!filePath.EndsWith(".sqlite"))
                throw new ArgumentException($"Not an sqlite database file path: \"{filePath}\"");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            string encodingStr = useUTF16 ? "UseUTF16Encoding=True; " : "UseUTF16Encoding=False; ";
            _ConnectionString = $"Data Source=\"{filePath}\";";
            _ConnectionsPool = new ObjectPool<SqliteConnection>(
                generateObject:_GenerateConnection, maxWaitingBeforeBackedUp:_MaxWaitingBeforeBackedUp, maxSize:_MaxNConnections, 
                (connection)=>connection.Dispose());
        }
        ~LocalSQLite() {
            Dispose();
        }
        private SqliteConnection _GenerateConnection() {
            SqliteConnection connection = new SqliteConnection(_ConnectionString);
            try
            {
                connection.Open();
            }
            catch (Exception ex) {
                throw new Exception($"Error calling connection.Open with connection string \"{_ConnectionString}\"", ex);
            }
            using (var command = new SqliteCommand("PRAGMA journal_mode=WAL2", connection))
            {
                command.ExecuteNonQuery();
            }
            return connection;
        }
        public void UsingConnection(Action<SqliteConnection> callback)
        {
            using (ObjectPoolHandle<SqliteConnection> handle = _ConnectionsPool.TakeWhenCan(_TimeoutMillisecondsTakeConnectionFromPool))
            {
                callback(handle.Object);
            }
        }
        public void UsingConnectionForWrite(Action<SqliteConnection> callback)
        {
            using (ObjectPoolHandle<SqliteConnection> handle = _ConnectionsPool.TakeWhenCan(_TimeoutMillisecondsTakeConnectionFromPool))
            {
                lock (_LockObjectWrite)
                {
                    callback(handle.Object);
                }
            }
        }
        public ObjectPoolHandle<SqliteConnection> TakeConnectionDangerous()
        {
            return _ConnectionsPool.TakeWhenCan(_TimeoutMillisecondsTakeConnectionFromPool);
        }
        public TReturn UsingConnection<TReturn>(Func<SqliteConnection, TReturn> callback)
        {
            using (ObjectPoolHandle<SqliteConnection> handle = _ConnectionsPool
                .TakeWhenCan(_TimeoutMillisecondsTakeConnectionFromPool))
            {
                return callback(handle.Object);
            }
        }
        public TReturn UsingConnectionForWrite<TReturn>(Func<SqliteConnection, TReturn> callback)
        {
            using (ObjectPoolHandle<SqliteConnection> handle = _ConnectionsPool
                .TakeWhenCan(_TimeoutMillisecondsTakeConnectionFromPool))
            {
                lock (_LockObjectWrite)
                {
                    return callback(handle.Object);
                }
            }
        }
        public void Dispose() {
            lock (_LockObjectDispose) {
                if (_Disposed) return;
                _Disposed = true;
            }
            _ConnectionsPool.Dispose();
        }
    }
}
