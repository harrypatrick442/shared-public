
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System;
using Core.Comparisons.Delegaets;
namespace Core.Comparisons
{
    public class ComparisonChain<TBeingCompared>
    {
        private DelegateTest<TBeingCompared>[] _Tests;
        private DelegateFinalComparison<TBeingCompared> _FinalComparison;
        public ComparisonChain(
            DelegateFinalComparison<TBeingCompared> finalComparison,
            params DelegateTest<TBeingCompared>[] tests)
        {
            _Tests = tests;
            _FinalComparison = finalComparison;
        }
        public Tuple<TBeingCompared, bool> Run(TBeingCompared a, TBeingCompared b)
        {
            return Run(a, b, 0, notFailedTestsYet:true, null);
        }
        private Tuple<TBeingCompared, bool> Run(
            TBeingCompared a,
            TBeingCompared b,
            int index, 
            bool notFailedTestsYet, 
            DelegateTest<TBeingCompared>? highestLevelTestPassed
            ) {
            if(index>=_Tests.Length)
            {
                return new Tuple<TBeingCompared, bool>(_FinalComparison(a, b, highestLevelTestPassed!), notFailedTestsYet);
            }
            DelegateTest<TBeingCompared> test = 
                _Tests[index++];
            if (test(a)) {
                if (test(b))    
                {
                    return Run(a, b, index, notFailedTestsYet:true, 
                        highestLevelTestPassed??test);
                }
                if (notFailedTestsYet)
                {
                    return new Tuple<TBeingCompared, bool>(a, false);
                }
                return RunTests(a, index);
            }
            if(test(b))
            {
                if (notFailedTestsYet)
                {
                    return new Tuple<TBeingCompared, bool>(b, false);
                }
                return RunTests(b, index);
            }
            return Run(a, b, index, notFailedTestsYet: false, highestLevelTestPassed);
        }
        public bool RunTests(TBeingCompared t) {
            return _RunTests(t, 0);
        }
        private Tuple<TBeingCompared, bool> RunTests(TBeingCompared t, int index) {
            return new Tuple<TBeingCompared, bool>(t, _RunTests(t, index));
        }
        private bool _RunTests(TBeingCompared t, int index)
        {
            while (index < _Tests.Length)
            {
                DelegateTest<TBeingCompared> testAndComparison =
                    _Tests[index++];
                if (!testAndComparison(t))
                {
                    return false;
                }
            }
            return true;
        }
    }
}