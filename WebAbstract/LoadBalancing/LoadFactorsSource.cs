using Core.Exceptions;
namespace Core.LoadBalancing
{
    public sealed class LoadFactorsSource
    {
        private static LoadFactorsSource _Instance;
        private Dictionary<LoadFactorType, ILoadFactorSource> _MapLoadFactorTypeToSource;
        public static LoadFactorsSource Initialize(params ILoadFactorSource[] sources) {
            if (_Instance != null) throw new AlreadyInitializedException(nameof(LoadFactorsSource));
            _Instance = new LoadFactorsSource(sources);
            return _Instance;
        }
        public static LoadFactorsSource Instance { get { 
                if (_Instance == null) 
                    throw new NotInitializedException(nameof(LoadFactorsSource));
                return _Instance;
            } 
        }
        private LoadFactorsSource(ILoadFactorSource[] sources) {
            Dictionary<LoadFactorType, ILoadFactorSource> mapLoadFactorTypeToSource = new Dictionary<LoadFactorType, ILoadFactorSource>();
            foreach (ILoadFactorSource source in sources) {
                if (mapLoadFactorTypeToSource.ContainsKey(source.LoadFactorType))
                    throw new ArgumentException($"Already had a mapping for {nameof(LoadFactorType)} {source.LoadFactorType}");
                mapLoadFactorTypeToSource[source.LoadFactorType] = source;
            }
            _MapLoadFactorTypeToSource = mapLoadFactorTypeToSource;
        }
        public double GetLoadFactor(LoadFactorType type) {
            ILoadFactorSource source;
            lock (_MapLoadFactorTypeToSource) {
                _MapLoadFactorTypeToSource.TryGetValue(type, out source);
            }
            if (source == null) throw new NullReferenceException($"Had no {nameof(ILoadFactorSource)} for {nameof(LoadFactorType)} {type}");
            return source.GetLoadFactor();
        }
    }
}