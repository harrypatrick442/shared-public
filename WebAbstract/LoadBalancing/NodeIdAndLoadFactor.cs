using FilesRelayCore.TransferServers;
using Logging;
using Shutdown;
using Core;
using Core.Exceptions;
using Core.Machine;
using Core.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.LoadBalancing
{
    public class NodeIdAndLoadFactor
    {
        public  int NodeId { get; protected set; }
        public double? LoadFactor { get; protected set; }
        public NodeIdAndLoadFactor(int nodeId, double? loadFactor) {
            NodeId = nodeId;
            LoadFactor = loadFactor;
        }
    }
}
