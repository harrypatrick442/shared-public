using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using Core.Exceptions;
using System;
using Core.Sorting;
namespace Core.Geometry
{
    public class SetOfLayersBeingOrdered<TPayload>
    {
        private LayerBeingOrdered<TPayload>[] _LayersBeingOrdered;
        private List<LayerPairOrder<TPayload>> _UnknownLayerPairOrders;
        private List<LayerPairOrder<TPayload>> _KnownLayerPairOrders = new List<LayerPairOrder<TPayload>>();
        public LayerPairOrder<TPayload>[] KnownLayerPairOrders { get { return _KnownLayerPairOrders.ToArray(); } }
        private Func<LayerBeingOrdered<TPayload>[], LayerBeingOrdered<TPayload>[]> _GetLayersLaserCanHit;
        private Func<LayerPairOrder<TPayload>, bool> _GetIsLayerPairOrderLayerAFirst;
        public bool OrderEstablished
        {
            get
            {
                return _UnknownLayerPairOrders.Count < 1;
            }
        }
        public string GetLayerPairOrdersString(){
            return string.Join(",\r\n", _KnownLayerPairOrders.Select(layerPairOrder=>layerPairOrder.ToString()));
        }          
        public TPayload[] GetPayloadsOrdered()
        {
            List<LayerBeingOrdered<TPayload>> layers = new List<LayerBeingOrdered<TPayload>>();
            SortWithUnknownsNode<TPayload>[] sortWithUnknownsNodes = GetFromLayerBeingOrdereds(_KnownLayerPairOrders.ToArray());
            SortWithUnknownsNode<TPayload>[] sortWithUnknownsNodesSorted = SortWithUnknowns.Sort<TPayload>(sortWithUnknownsNodes);
            return sortWithUnknownsNodesSorted.Select(
                SortWithUnknownsNode => SortWithUnknownsNode.Payload)
                .ToArray();
        }
        public LayerPairOrder<TPayload>[] GetLayerPairOrders() {
            return _KnownLayerPairOrders.ToArray();
        }
        private static SortWithUnknownsNode<TPayload>[] GetFromLayerBeingOrdereds(LayerPairOrder<TPayload>[] knownLayerPairOrders)
        {
            Dictionary<TPayload, SortWithUnknownsNode<TPayload>> mapGameObjectToSortWithUnknownsNode = new Dictionary<TPayload, SortWithUnknownsNode<TPayload>>();
            Func<LayerBeingOrdered<TPayload>, SortWithUnknownsNode<TPayload>> getSortWithUnknownsNodeFromPayload = Get_GetSortWithUnknownsNodeFromPayloadLexicalClosure(mapGameObjectToSortWithUnknownsNode);
            foreach (LayerPairOrder<TPayload> knownLayerPairOrder in knownLayerPairOrders)
            {
                SortWithUnknownsNode<TPayload> sortWithUnknownsNodeLayerA = getSortWithUnknownsNodeFromPayload(knownLayerPairOrder.LayerBeingOrderedA);
                SortWithUnknownsNode<TPayload> sortWithUnknownsNodeLayerB = getSortWithUnknownsNodeFromPayload(knownLayerPairOrder.LayerBeingOrderedB);
                sortWithUnknownsNodeLayerA.AddFromNode(sortWithUnknownsNodeLayerB);
                sortWithUnknownsNodeLayerB.AddToNode(sortWithUnknownsNodeLayerA);
            }
            return mapGameObjectToSortWithUnknownsNode.Values.ToArray();
        }
        private static Func<LayerBeingOrdered<TPayload>, SortWithUnknownsNode<TPayload>> Get_GetSortWithUnknownsNodeFromPayloadLexicalClosure(Dictionary<TPayload, SortWithUnknownsNode<TPayload>> mapGameObjectToSortWithUnknownsNode)
        {
            return (layerBeingOrdered) => {
                if (mapGameObjectToSortWithUnknownsNode.ContainsKey(layerBeingOrdered.Payload))
                    return mapGameObjectToSortWithUnknownsNode[layerBeingOrdered.Payload];
                SortWithUnknownsNode<TPayload> sortWithUnknownsNode = new SortWithUnknownsNode<TPayload>(layerBeingOrdered.Payload, layerBeingOrdered.GetName);
                mapGameObjectToSortWithUnknownsNode[layerBeingOrdered.Payload] = sortWithUnknownsNode;
                return sortWithUnknownsNode;
            };
        }
        public void ResolveUnknownLayerOrdersAtThisDirectionVector()
        {
            LayerBeingOrdered<TPayload>[] layersLaserCanHit = _GetLayersLaserCanHit(_LayersBeingOrdered);
            foreach (LayerPairOrder<TPayload> unknownLayerPairOrder in _UnknownLayerPairOrders.ToArray())
            {
                if ((!layersLaserCanHit.Contains(unknownLayerPairOrder.LayerBeingOrderedA)) ||
                    (!layersLaserCanHit.Contains(unknownLayerPairOrder.LayerBeingOrderedB)))
                {
                    continue;
                }
                bool layerPairOrderLayerAIsFirst = _GetIsLayerPairOrderLayerAFirst(unknownLayerPairOrder);
                if (!layerPairOrderLayerAIsFirst)
                {
                    unknownLayerPairOrder.Switch();
                }
                _KnownLayerPairOrders.Add(unknownLayerPairOrder);
                _UnknownLayerPairOrders.Remove(unknownLayerPairOrder);
            }
        }
        public SetOfLayersBeingOrdered(TPayload[] payloads,
            Func<LayerBeingOrdered<TPayload>[], LayerBeingOrdered<TPayload>[]> getLayersLaserCanHit,
            Func<LayerPairOrder<TPayload>, bool> getIsLayerPairOrderLayerAFirst, Func<TPayload, string> getName)
        {
            _LayersBeingOrdered = payloads.Select(payload => new LayerBeingOrdered<TPayload>(payload, () => getName(payload))).ToArray();
            _UnknownLayerPairOrders = GetAllLayersPairCombinations(_LayersBeingOrdered);
            _GetLayersLaserCanHit = getLayersLaserCanHit;
            _GetIsLayerPairOrderLayerAFirst = getIsLayerPairOrderLayerAFirst;
        }
        private static List<LayerPairOrder<TPayload>> GetAllLayersPairCombinations(LayerBeingOrdered<TPayload>[] layersBeingOrdered)
        {
            List<LayerPairOrder<TPayload>> layerPairOrderCombinations = new List<LayerPairOrder<TPayload>>();
            for (int i = 0; i < layersBeingOrdered.Length; i++)
            {
                for (int j = i + 1; j < layersBeingOrdered.Length; j++)
                {
                    LayerBeingOrdered<TPayload> layerBeingOrderedA = layersBeingOrdered[i];
                    LayerBeingOrdered<TPayload> layerBeingOrderedB = layersBeingOrdered[j];
                    layerPairOrderCombinations.Add(new LayerPairOrder<TPayload>(layerBeingOrderedA, layerBeingOrderedB));
                }
            }
            return layerPairOrderCombinations;
        }
    }
}
