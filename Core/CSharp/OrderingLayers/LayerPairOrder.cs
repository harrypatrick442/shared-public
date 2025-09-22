using System;
using System.Collections.Generic;
using System.Linq;
using JSON;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Interfaces;

namespace Core.Geometry
{
    [DataContract]
    public class LayerPairOrder<TPayload>{
        private LayerBeingOrdered<TPayload> _LayerBeingOrderedA;
        private LayerBeingOrdered<TPayload> _LayerBeingOrderedB;
        public LayerBeingOrdered<TPayload> LayerBeingOrderedA { get { return _LayerBeingOrderedA; } }
        public LayerBeingOrdered<TPayload> LayerBeingOrderedB { get { return _LayerBeingOrderedB; } }
        public LayerPairOrder(LayerBeingOrdered<TPayload> layerBeingOrderedA, LayerBeingOrdered<TPayload> layerBeingOrderedB) {
            _LayerBeingOrderedA = layerBeingOrderedA;
            _LayerBeingOrderedB = layerBeingOrderedB;
        }
        protected LayerPairOrder() { }
        public void Switch() {
            LayerBeingOrdered<TPayload> temp = _LayerBeingOrderedA;
            _LayerBeingOrderedA = _LayerBeingOrderedB;
            _LayerBeingOrderedB=temp;
        }
        public override string ToString()
        {
            return $"{ _LayerBeingOrderedA.GetName()} ->{_LayerBeingOrderedB.GetName()}";
        }
    }
}
