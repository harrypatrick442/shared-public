using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Linq;
using Core.Geometry;
using Core.FileSystem;
using System.IO;
using JSON;
namespace Core.Multimedias
{
    [DataContract]
    public enum MultimediaType
    {
        Video=1,
        Image=2,
        Audio=3
    }
}
