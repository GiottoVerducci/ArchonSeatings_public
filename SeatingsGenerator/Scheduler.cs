namespace ConsoleApplication5
{
    using System.Collections.Generic;
    using System.Threading;

    public class Scheduler
    {
        private readonly object _lockObject = new object();

        private readonly List<int> _availableProcessors = new List<int>();
        private AutoResetEvent _waitHandle = new AutoResetEvent(false);

        public Scheduler(IEnumerable<int> availableProcessors)
        {
            _availableProcessors.AddRange(availableProcessors);
        }

        public int Wait()
        {
            do
            {
                lock (_lockObject)
                {
                    if (_availableProcessors.Count > 0)
                    {
                        var processor = _availableProcessors[0];
                        _availableProcessors.RemoveAt(0);
                        return processor;
                    }
                }

                _waitHandle.WaitOne();
            } 
            while (true);
        }

        public void Release(int processor)
        {
            lock (_lockObject)
            {
                _availableProcessors.Add(processor);
                _waitHandle.Set();
            }
        }
    }
}
