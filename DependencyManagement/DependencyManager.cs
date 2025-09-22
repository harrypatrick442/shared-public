using System.Data;
namespace DependencyManagement
{
    public static class DependencyManager
    {
        private static Dictionary<string, object> _MapNameToDependency =
            new Dictionary<string, object>();
        private static Dictionary<Type, object> _MapTypeToDependency =
            new Dictionary<Type, object>();
        public static void Add(string name, object dependency)
        {
            if (_MapNameToDependency.ContainsKey(name))
                throw new DuplicateNameException(name);
            _MapNameToDependency[name] = dependency;
        }
        public static void AddByNames(List<Tuple<string, object>> dependencies)
        {
            foreach (Tuple<string, object> dependency in dependencies)
                Add(dependency.Item1, dependency.Item2);
        }
        public static void Add<TDependency>(TDependency dependency)
        {
            if (dependency == null) 
                throw new ArgumentNullException(typeof(TDependency).Name);
            if (_MapTypeToDependency.ContainsKey(typeof(TDependency)))
                throw new Exception($"Type [{typeof(TDependency).Name}] already added ");
            _MapTypeToDependency[typeof(TDependency)] = dependency;
        }
        public static TDependency Get<TDependency>() {
            return (TDependency)_MapTypeToDependency[typeof(TDependency)];
        }
        public static TDependency Get<TDependency>(string name)
        {
            if (!_MapNameToDependency.TryGetValue(name, out object? dependency))
            {
                throw new NullReferenceException($"No dependency named \"{name}\"");
            }
            return (TDependency)dependency;
        }
        public static string GetString(string name) {
            string str = Get<string>(name);
            return str;
        }
    }
}
