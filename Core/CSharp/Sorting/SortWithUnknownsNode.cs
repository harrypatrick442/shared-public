using System;

using System.Collections.Generic;
namespace Core.Sorting
{
    public class SortWithUnknownsNode<TPayload>
    {
        private TPayload _Payload;
        public TPayload Payload { get { return _Payload; } }
        private Func<string> _GetName;
        public string GetName() {
            return _GetName();
        }
        private List<SortWithUnknownsNode<TPayload>> _ToNodes = new List<SortWithUnknownsNode<TPayload>>();
        private List<SortWithUnknownsNode<TPayload>> _FromNodes = new List<SortWithUnknownsNode<TPayload>>();
        public SortWithUnknownsNode<TPayload>[] ToNodes { get { return _ToNodes.ToArray(); } }
        public SortWithUnknownsNode<TPayload>[] FromNodes { get { return _FromNodes.ToArray(); } }
        public void AddToNode(SortWithUnknownsNode<TPayload> toNode) {
            _ToNodes.Add(toNode);
        }
        public void AddFromNode(SortWithUnknownsNode<TPayload> fromNode) {
            _FromNodes.Add(fromNode);
        }
        public SortWithUnknownsNode(TPayload payload, Func<string> getName) {

            _Payload = payload;
            _GetName = getName;
        }
    }
}