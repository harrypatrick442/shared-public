using System.Collections.Generic;
using System.Linq;
namespace Core.Arguments
{
    public class ArgsBuilder {
        private List<Arg> _Args = new List<Arg>();
        public ArgsBuilder(params Arg[] args) {
            AddRange(args);
        }
        public void AddRange(Arg[] args)
        {
            foreach (Arg arg in args) Add(arg);
        }

        public void Add(Args args) {
            foreach (Arg arg in args.ToArray()) {
                Add(arg);
            }
        }

        public void Add(Arg arg) {
            if (_Args.Contains(arg)) return;
            _Args.Add(arg);
        }
        public override string ToString() {
            return string.Join(" ", _Args.Select(arg => arg.ToString()).ToArray());
        }
    }
}