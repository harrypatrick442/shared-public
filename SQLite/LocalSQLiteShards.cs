using Core;
using Database;
using JSON;
using KeyValuePairDatabases;
using Microsoft.Data.Sqlite;
namespace SQLite
{
    public class LocalSQLiteShards : IDisposable
    {
        private const int MAX_N_CONNECTIONS_DEFAULT = 10;
        private const int MAX_WAITING_BEFORE_BACKED_UP_DEFAULT= 20;
        private const int TIMEOUT_MILLISECONDS_TAKE_CONNECTION_FROM_POOL_DEFAULT = 1000;


        private long _CurrentIdToExclusive;
        private int _ShardSize;
        private string _DirectoryPath;
        private bool _UseUTF16;
        private int _MaxNConnections;
        private int _MaxWaitingBeforeBackedUp;
        private int _TimeoutMillisecondsTakeConnectionFromPool;

        private string _IdentiferRangeShardIdsPath;
        private bool _Disposed = false;
        private readonly object _LockObject = new object();
        private IdentifierRangeShardId[] _IdentifierRangeShardIds;
        private KeyValuePairInMemoryDatabase<int, LocalSQLite> _MapShardIdToLocalSqlite;
        private Action<SqliteConnection> _Prepare;
        public LocalSQLiteShards(string directoryPath, bool useUTF16, int shardSize,
            Action<SqliteConnection> prepare,
            int maxNConnections = MAX_N_CONNECTIONS_DEFAULT, 
            int maxWaitingBeforeBackedUp = MAX_WAITING_BEFORE_BACKED_UP_DEFAULT,
            int timeoutMillisecondsTakeConnectionFromPool = TIMEOUT_MILLISECONDS_TAKE_CONNECTION_FROM_POOL_DEFAULT) {
            
            _DirectoryPath = directoryPath;
            _UseUTF16 = useUTF16;
            _ShardSize = shardSize;
            _Prepare = prepare;
            _MaxNConnections = maxNConnections;
            _MaxWaitingBeforeBackedUp= maxWaitingBeforeBackedUp;
            _TimeoutMillisecondsTakeConnectionFromPool= timeoutMillisecondsTakeConnectionFromPool;

            _IdentiferRangeShardIdsPath = Path.Combine(directoryPath, "ranges.txt");
            if (File.Exists(_IdentiferRangeShardIdsPath))
            {
                string[] lines = File.ReadAllLines(_IdentiferRangeShardIdsPath);
                _IdentifierRangeShardIds = lines.Select(l => Json.Deserialize<IdentifierRangeShardId>(l)).ToArray();
            }
            else { 
                _IdentifierRangeShardIds = new IdentifierRangeShardId[0];
            }
            _MapShardIdToLocalSqlite = new KeyValuePairInMemoryDatabase<int, LocalSQLite>(
                new OverflowParameters<int, LocalSQLite>(
                    overflowToNowhere:false,
                    new NoIdentifierLock<int>(),
                    OverflowEntry
                    ), new NoIdentifierLock<int>());
        }
        ~LocalSQLiteShards() {
            Dispose();
        }
        public void UsingConnection(long identifier, Action<SqliteConnection> callback)
        {
            LocalSQLite shard = GetOrCreateShard(identifier);
            shard.UsingConnection(callback);
        }
        public void UsingConnectionForWrite(long identifier, Action<SqliteConnection> callback)
        {
            LocalSQLite shard = GetOrCreateShard(identifier);
            shard.UsingConnectionForWrite(callback);
        }
        public TReturn UsingConnection<TReturn>(long identifier, Func<SqliteConnection, TReturn> callback)
        {
            LocalSQLite shard = GetOrCreateShard(identifier);
            return shard.UsingConnection(callback);
        }
        public TReturn UsingConnectionForWrite<TReturn>(long identifier, Func<SqliteConnection, TReturn> callback)
        {
            LocalSQLite shard = GetOrCreateShard(identifier);
            return shard.UsingConnectionForWrite(callback);
        }
        private LocalSQLite GetOrCreateShard(long identifier) {
            int shardId = GetShardId(identifier);
            LocalSQLite shard = _MapShardIdToLocalSqlite.Get(shardId);
            if (shard == null)
            {
                lock (_LockObject)
                {
                    if (_Disposed)
                        throw new ObjectDisposedException(nameof(LocalSQLiteShards));
                    shard = _MapShardIdToLocalSqlite.Get(shardId);
                    if (shard == null)
                    {
                        shard = SpinUpShard(shardId);
                        _MapShardIdToLocalSqlite.Set(shardId, shard);
                    }
                }
            }
            return shard;
        }
        private LocalSQLite SpinUpShard(int shardId) {

            string path = _DirectoryPath;
            if (path.Last() != Path.DirectorySeparatorChar)
                path += Path.DirectorySeparatorChar;
            path += DatabasePathsHelper.SplitIdentifierIntoHundredsPathSegments(shardId);
            path += ".sqlite";
            LocalSQLite localSqlite = new LocalSQLite(path, _UseUTF16, _MaxNConnections, _MaxWaitingBeforeBackedUp, _TimeoutMillisecondsTakeConnectionFromPool);
            localSqlite.UsingConnection(_Prepare);
            return localSqlite;
        }
        private int GetShardId(long identifier) {
            IdentifierRangeShardId[] dentifierRangeShardIds;
            lock (_LockObject)
            {
                if (_Disposed)
                {
                    throw new ObjectDisposedException(nameof(LocalSQLiteShards));
                }
                dentifierRangeShardIds = _IdentifierRangeShardIds;
            }
            if (!GetShardId(identifier, dentifierRangeShardIds, out int shardId))
            {
                shardId = AppendNewShards(identifier);
            }
            return shardId;
        }
        private bool GetShardId(long identifier, IdentifierRangeShardId[] identifierRangeShardIds, out int shardId) {
            shardId = 0;
            if (identifierRangeShardIds == null || !identifierRangeShardIds.Any())
                return false;
            int? index = BinarySearchHelper.BinarySearchWithOverflowEachEnd(identifierRangeShardIds,
            (i) => i.FromInclusive > identifier ? -1 : (i.ToExclusive <= identifier ? 1 : 0),
            out bool wasExactMatch, roundUpOnEquals: false, exactMatch: true);
            if (index == null||!wasExactMatch) {
                return false;
            }
            shardId = identifierRangeShardIds[(int)index].ShardId;
            return true;
        }
        private int AppendNewShards(long identifier) {

            lock (_LockObject)
            {
                if (_Disposed)
                    throw new ObjectDisposedException(nameof(LocalSQLiteShards));
                if (GetShardId(identifier, _IdentifierRangeShardIds, out int shardId))
                    return shardId;
                long identifierFromInclusive;
                int nextShardId;
                if (_IdentifierRangeShardIds.Any())
                {
                    IdentifierRangeShardId last = _IdentifierRangeShardIds.Last();
                    identifierFromInclusive = last.ToExclusive;
                    nextShardId = last.ShardId + 1;
                }
                else {
                    identifierFromInclusive = 0;
                    nextShardId = 1;
                }
                List<IdentifierRangeShardId> newRanges = new List<IdentifierRangeShardId>();
                IdentifierRangeShardId newRange;
                do
                {
                    long toExclusive = identifierFromInclusive + _ShardSize;
                    newRange = new IdentifierRangeShardId(
                        identifierFromInclusive, toExclusive, nextShardId++);
                    identifierFromInclusive = toExclusive;
                } while (identifierFromInclusive < identifier);
                File.AppendAllLines(_IdentiferRangeShardIdsPath, newRanges.Select(n => Json.Serialize(n)).ToArray());
                _IdentifierRangeShardIds = _IdentifierRangeShardIds.Concat(newRanges).ToArray();
                return newRange.ShardId;
            }
        }
        private void OverflowEntry(int shardId, LocalSQLite shard) {
            shard.Dispose();
        }
        public void Dispose() {
            lock (_LockObject) {
                if (_Disposed) return;
                _Disposed = true;
            }
            _MapShardIdToLocalSqlite.OverflowAllToExternal();
        }
    }
}