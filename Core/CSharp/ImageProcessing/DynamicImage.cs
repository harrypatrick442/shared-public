using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.IO;
namespace Snippets.Core.ImageProcessing
{
    [DataContract]
    [KnownType(typeof(DynamicImage))]
    public class DynamicImage{
        private DynamicImageEntry[] _Entries;
        [JsonPropertyName("entries")]
        [JsonInclude]
        [DataMember(Name = "entries")]
        public DynamicImageEntry[] Entries { get { return _Entries; } protected set { _Entries = value; } }
        private string _RootDirectoryAbsolutePath;
        [JsonPropertyName("rootDirectoryAbsolutePath")]
        [JsonInclude]
        [DataMember(Name = "rootDirectoryAbsolutePath")]
        public string RootDirectoryAbsolutePath { get { return _RootDirectoryAbsolutePath; } protected set { _RootDirectoryAbsolutePath = value; } }
        public DynamicImage(DynamicImageEntry[] entries, string rootDirectoryAbsolutePath) {
            _Entries = entries;
            _RootDirectoryAbsolutePath = rootDirectoryAbsolutePath;
        }
        public DynamicImage CopyToDirectory(string newRootDirectoryAbsolutePath) {
            foreach (DynamicImageEntry entry in _Entries) {
                string filePathTo = Path.Combine(newRootDirectoryAbsolutePath, entry.RelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(filePathTo));
                File.Copy(Path.Combine(_RootDirectoryAbsolutePath, entry.RelativePath), filePathTo, true);
            }
            return new DynamicImage(_Entries, newRootDirectoryAbsolutePath);
        }
    }
}
