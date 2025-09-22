using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Core.Pool
{
	public class CompositeProgressHandler : ProgressHandler
    {
        private Dictionary<ProgressHandler, double> _MapChildToCachedValue;
        private int? _NChildrenExpected;
        public CompositeProgressHandler(params ProgressHandler[] children)
        :this(children, null){

        }
        public CompositeProgressHandler(int nChildrenExpected)
        : this(null, nChildrenExpected)
        {

        }
        public CompositeProgressHandler(ProgressHandler[]? children, int? nChildrenExpected) {
            if (nChildrenExpected != null) {
                if ((int)nChildrenExpected <= 0) {
                    throw new ArgumentException($"{nameof(nChildrenExpected)} cannot have a value of {(int)nChildrenExpected}. It must be greater than zero or null");
                }
                _NChildrenExpected = nChildrenExpected;
            }
            if (children == null||children.Length < 1) { 
                _MapChildToCachedValue = new Dictionary<ProgressHandler, double>();
                _Proportion = 0;
                return;
            }
            _MapChildToCachedValue = children.ToDictionary(c=>c, c=>c.Proportion);
            foreach(var child in children)
            {
                child.Progressed += ChildProgressed;
            }
            _Proportion = children.Select(c => c.Proportion).Sum() 
                /Denominator();
        }
        public void AddChild(ProgressHandler child)
        {
            EventHandler<ProgressEventArgs>[]? eventHandlers;
            double proportion;
            lock (_LockObject)
            {
                if (_MapChildToCachedValue.ContainsKey(child))
                {
                    return;
                }
                _MapChildToCachedValue.Add(child, child.Proportion);
                child.Progressed += ChildProgressed;
                double oldProportion = _Proportion;
                _Proportion = _MapChildToCachedValue.Values.Sum()
                    / Denominator();
                if (oldProportion == _Proportion) return;
                proportion = _Proportion;
                eventHandlers = _ProgressedEventHandlers?.ToArray();
            }
            Dispatch(eventHandlers, proportion);
        }
        public void RemoveChild(ProgressHandler child)
        {
            EventHandler<ProgressEventArgs>[]? eventHandlers;
            double proportion;
            lock (_LockObject) {
                if (!_MapChildToCachedValue.Remove(child)) return;
                child.Progressed -= ChildProgressed;
                if (_MapChildToCachedValue.Count <= 0)
                {
                    if (_Proportion == 0) return;
                    _Proportion = (proportion = 0);
                }
                else
                {
                    double oldProportion = _Proportion;
                    _Proportion = _MapChildToCachedValue.Values.Sum()
                        / Denominator();
                    if (oldProportion == _Proportion) return;
                    proportion = _Proportion;
                }
                eventHandlers = _ProgressedEventHandlers?.ToArray();
            }
            Dispatch(eventHandlers, proportion);
        }
        private void ChildProgressed(object sender, ProgressEventArgs e) {

            EventHandler<ProgressEventArgs>[]? eventHandlers;
            ProgressHandler child = (ProgressHandler)sender;
            double proportion;
            lock (_LockObject)
            {
                double currentProportion = _MapChildToCachedValue[child];
                if (currentProportion == e.Proportion) return;
                _MapChildToCachedValue[child] = e.Proportion;
                _Proportion = _MapChildToCachedValue.Values.Sum() 
                    / Denominator();
                proportion = _Proportion;
                eventHandlers = _ProgressedEventHandlers?.ToArray();
            }
            Dispatch(eventHandlers, proportion);
        }
        private double Denominator() {
            if (_NChildrenExpected == null)
                return _MapChildToCachedValue.Count;
            if (_NChildrenExpected >= _MapChildToCachedValue.Count) {
                return (int)_NChildrenExpected;
            }
            return _MapChildToCachedValue.Count;
        }
    }
}
