using System.Collections.Generic;

namespace ArchiveDiff.Logic
{
    public class EnumeratorWithHasCurrent<T>
    {
        private readonly IEnumerator<T> _enumerator;
        
        public bool HasCurrent { get; private set; }
        public T Current => _enumerator.Current;

        public EnumeratorWithHasCurrent(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
            HasCurrent = enumerator.MoveNext();
        }

        public bool MoveNext()
        {
            return HasCurrent = _enumerator.MoveNext();
        }
    }
}
