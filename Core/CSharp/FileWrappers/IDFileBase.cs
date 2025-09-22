using Core.Assets;
using System.IO;
public abstract class IdFileBase<TDerived> where TDerived:IdFileBase<TDerived>, new()
{
    private static readonly object _LockObjectInstance = new object();
    private static TDerived _Instance;
    public static TDerived Instance { get {
            lock (_LockObjectInstance)
            {
                if (_Instance == null)
                    _Instance = new TDerived();
                return _Instance;
            }
        } 
    }
    private string _FilePath;
    protected IdFileBase(string relativePathFromRoot) {
        _FilePath = GetFilePath(relativePathFromRoot);
    }
    public int Id { 
        get
        {
            return int.Parse(File.ReadAllText(_FilePath));
        }
        set {
            File.WriteAllText(GetFilePath(_FilePath), value.ToString());
        }
    }
    private static string GetFilePath(string relativePathFromRoot) {
        return Path.Combine(DependencyManager.GetString(CoreDependencyNames.RootDirectory), relativePathFromRoot);
    }
}