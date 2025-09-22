using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Atomics
{
    public class Atomic<T>
    {
        public Atomic() { }
        public Atomic(T initialValue) { _Value = initialValue; }
        private T _Value;
        public T Value { set { _Value = value; } get { return _Value; } }
    }
}
