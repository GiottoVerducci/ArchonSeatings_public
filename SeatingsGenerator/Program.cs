namespace ConsoleApplication5
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
#if USE_PARALLEL
    using System.Threading.Tasks;
#endif

    class Program
    {
        private static object _lockObject = new object();
        private volatile static int _count;
        private static DateTime _lastStatusDate;

        private static Dictionary<int, PartInfo> _partByProcessor = new Dictionary<int, PartInfo>();
        private static List<Range> _ranges = new List<Range>();
        private static bool _checkIfNotInRange = false;

        static int Main(string[] args)
        {
            Console.WriteLine("Press CTRL-C to exit at any time.");

#if SEVENTEEN
            int n = 17;
#elif EIGHTTEEN
            int n = 18;
#elif NINETEEN
            int n = 19;
#elif TWENTY
            int n = 20;
#elif TWENTYONE
            int n = 20;
#elif TWENTYTWO
            int n = 20;
#else
            int n = 12;
#endif


            //if (args.Length < 1 || !int.TryParse(args[0], out n) || n < 0)
            //{
            //    Console.WriteLine("Expected argument: positive number of players.");
            //    return 1;
            //}

            int lowerBound = 1, higherBound = Int32.MaxValue;

            Console.WriteLine("Partitioning the computations...");
            var factory = new Seatings3RFactory(n);
            var seatings = factory.GetSeatingsParallel();
            var totalCount = seatings.Length;
            Console.WriteLine("Partitioned into {0} parts", totalCount);

            if (args.Length >= 1)
            {
                if (!int.TryParse(args[0], out lowerBound) || lowerBound <= 0 || lowerBound > totalCount)
                {
                    Console.WriteLine("Expected argument #1 (lower bound): must be between 1 and {0}.", totalCount);
                    return 1;
                }
            }
            if (args.Length >= 2)
            {
                if (!int.TryParse(args[1], out higherBound) || higherBound < lowerBound || higherBound > totalCount)
                {
                    Console.WriteLine("Expected argument #2 (higher bound): must be between {0} and {1}.", lowerBound, totalCount);
                    return 1;
                }
            }
            if (args.Length == 0)
            {
                string value;
                do
                {
                    Console.Write("Please enter the lower bound [1-{0}] (default: 1): ", totalCount);
                    value = Console.ReadLine();
                }
                while (!string.IsNullOrWhiteSpace(value) && (!int.TryParse(value, out lowerBound) || lowerBound <= 0 || lowerBound > totalCount));
                do
                {
                    Console.Write("Please enter the higher bound [{0}-{1}] (default: {1}): ", lowerBound, totalCount);
                    value = Console.ReadLine();
                }
                while (!string.IsNullOrWhiteSpace(value) && (!int.TryParse(value, out higherBound) || higherBound < lowerBound || higherBound > totalCount));
            }

            var statusFile = string.Format("seating3r-{0}-status.txt", n);

            if (File.Exists(statusFile))
            {
                Console.Write("Status file found. Do you want to use it to resume? [y-n] ");
                var value = Console.ReadLine();

                if (string.Compare(value, "y", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    string[] rangesAsString = File.ReadAllLines(statusFile);
                    foreach (var range in rangesAsString)
                    {
                        var split = range.Split('-');
                        var low = int.Parse(split[0]);
                        var high = split.Count() == 2 ? int.Parse(split[1]) : low;
                        _ranges.Add(new Range { Low = low, High = high });
                        _checkIfNotInRange = true;
                    }
                }
            }


            // set the priority to idle so that it consumes cpu only when other programs don't use it
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

            //var tests = new[] { 10, 14, 15, 16, 17, 18 }; // min transfers = 5
            //var tests = new[] { 12, 13 }; // min transfers = 4

            //foreach (var test in tests)
            {
                //n = test;
                //n = 19;
                var start = DateTime.Now;
                //var factory = new SeatingsFactory2(n);
                //var factory = new Seatings3RFactory(n);

                //var test = factory.GetPossibleTablesMetTwice(3);
                //var countt = test.Count();
                //Console.WriteLine("Possible table combinaisons: " + countt);

                //var testSequence = new[] { 1, 5, 6, 9 }.OrderBy(t => t).ToArray();
                //var testSequence2 = new[] { 11, 8, 10, 3 }.OrderBy(t => t).ToArray();
                //var testSequence3 = new[] { 2, 4, 12, 7 }.OrderBy(t => t).ToArray();
                //var testSequence4 = new[] { 8, 1, 9, 2 }.OrderBy(t => t).ToArray();
                //var testSequence5 = new[] { 7, 5, 4, 11 }.OrderBy(t => t).ToArray();
                //var testSequence6 = new[] { 12, 3, 6, 10 }.OrderBy(t => t).ToArray();

                //foreach (var table in test)
                //{
                //    var tables = new Dictionary<int, List<int>>();
                //    var tablesR3 = new Dictionary<int, List<int>>();
                //    for (int i = 0; i < n; ++i)
                //    {
                //        List<int> tt;
                //        var t = table[i];
                //        if (!tables.TryGetValue(t, out tt))
                //        {
                //            tables[t] = tt = new List<int>();
                //        }
                //        tt.Add(i + 1);

                //        t = table[n + i];
                //        if (!tablesR3.TryGetValue(t, out tt))
                //        {
                //            tablesR3[t] = tt = new List<int>();
                //        }
                //        tt.Add(i + 1);

                //    }

                //    if (tables[1].SequenceEqual(testSequence)
                //        && tables[2].SequenceEqual(testSequence2)
                //        && tables[3].SequenceEqual(testSequence3)
                //        && tablesR3[1].SequenceEqual(testSequence4)
                //        && tablesR3[2].SequenceEqual(testSequence5)
                //        && tablesR3[3].SequenceEqual(testSequence6))
                //    {
                //        var result = string.Join(" / ", tables.Keys.OrderBy(k => k).Select(k => string.Format("table {0}: {1}", k, string.Join(" ", tables[k]))));
                //        var result3 = string.Join(" / ", tablesR3.Keys.OrderBy(k => k).Select(k => string.Format("table {0}: {1}", k, string.Join(" ", tablesR3[k]))));
                //        Console.WriteLine(result + " " + result3);
                //    }
                //}

                //return 0;

                string file = string.Format("seating3r-{0}.txt", n);

                Process currentProcess = Process.GetCurrentProcess();

#if USE_PARALLEL
                var availableProcessors = Enumerable.Range(0, Environment.ProcessorCount).Reverse().Take(Environment.ProcessorCount).ToArray();
                var scheduler = new Scheduler(availableProcessors);

                //long affinityMask = (long)currentProcess.ProcessorAffinity;
                //affinityMask &= availableProcessors.Select(p => 2 << (p-1)).Sum(); 
                //currentProcess.ProcessorAffinity = (IntPtr)affinityMask;

                //var rangePartitioner = Partitioner.Create(0, seatings.Length);
                if (higherBound > totalCount)
                {
                    higherBound = totalCount;
                }

                var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = availableProcessors.Count()
                };

                _count = 0;
                _lastStatusDate = DateTime.Now;

                var guid = Guid.NewGuid();

                var totalPartsCount = higherBound - lowerBound + 1;

                Parallel.ForEach(seatings.Skip(lowerBound - 1).Take(totalPartsCount), options, (seatingIterator, pls, index) =>
                {
                    var id = lowerBound + index;
                    var localCount = Interlocked.Increment(ref _count);

                    if (_checkIfNotInRange && _ranges.Any(r => r.Low <= id && id <= r.High))
                    {
                        return;
                    }

                    Console.WriteLine("Part {0} Waiting to acquire a processor", id);
                    var processor = scheduler.Wait();
                    Console.WriteLine("Part {0} Processor {1} acquired", id, processor);

                    SetThreadProcessorAffinity(processor);

                    WriteStatus(processor, lowerBound, higherBound, id, start, localCount, totalPartsCount, statusFile);

                    //WriteResultsToFile(file + "-" + Guid.NewGuid(), factory.GetBestSeatings(seatingIterator.Select(factory.GetCycledSeating)), factory);
                    WriteResultsToFile(string.Format("{0}-{1}-{2:D8}", file, guid, id), factory.GetBestSeatings(seatingIterator), factory, localCount, higherBound, id);

                    lock (_lockObject)
                    {
                        _ranges.Add(new Range { Low = id, High = id });
                        CompactRanges();

                        var rangesAsString = string.Join("\n", _ranges.Select(r => r.Low + "-" + r.High));

                        using (var stream = new StreamWriter(statusFile, false))
                        {
                            stream.WriteLine(rangesAsString);
                        }
                    }

                    Console.WriteLine("Part {0} Releasing processor {1}", id, processor);
                    scheduler.Release(processor);
                    Console.WriteLine("Part {0} Processor released {1}", id, processor);
                });
#else

                SetThreadProcessorAffinity(1);
                var seatings = new List<IEnumerable<int[]>> { factory.GetSeatings() };
                long affinityMask = (long)currentProcess.ProcessorAffinity;
                affinityMask &= 0x0008; // use only the fourth processor
                currentProcess.ProcessorAffinity = (IntPtr)affinityMask;

                ProcessThread thread = currentProcess.Threads[0];
                affinityMask = 0x0008; // use only the fourth processor
                thread.ProcessorAffinity = (IntPtr)affinityMask;
                int id = 1;
                int totalCount = 1;
                seatings.ForEach(seatingIterator =>
                {
                    //WriteResultsToFile(file + "-" + Guid.NewGuid(), factory.GetBestSeatings(seatingIterator.Select(factory.GetCycledSeating)), factory);
                    WriteResultsToFile(file + "-" + Guid.NewGuid(), factory.GetBestSeatings(seatingIterator), factory, id, totalCount);
                });
                Console.WriteLine("Process time: {0} s", currentProcess.TotalProcessorTime.TotalSeconds);
#endif
#if COUNT
                Console.WriteLine(factory.Count);
#endif
                var now = DateTime.Now;
                var elapsed = now - start;

                Console.WriteLine("Computation done from {0} to {1} on {2}", lowerBound, higherBound, now);
                Console.WriteLine("Total time: {0} s", elapsed.TotalSeconds);
                Console.ReadLine();
            }
            return 0;
        }

        private class PartInfo
        {
            public long Id { get; set; }
            public DateTime StartDate { get; set; }
        }

        private class Range
        {
            public long Low { get; set; }
            public long High { get; set; }
        }

        private static void WriteStatus(int processor, int lowerBound, int higherBound, long id, DateTime start, int localCount, int totalPartsCount, string statusFile)
        {
            lock (_lockObject)
            {
                var now = DateTime.Now;

                _partByProcessor[processor] = new PartInfo { Id = id, StartDate = now };

                if (now - _lastStatusDate <= new TimeSpan(0, 0, 5))
                {
                    return;
                }

                _lastStatusDate = now;

                var local = _partByProcessor.OrderBy(kvp => kvp.Key).ToArray();
                //var doneUpTo = _ranges.Any() && _ranges[0].Low == lowerBound ? _ranges[0].High - 1 : lowerBound;

                var headerItems = local.Select(kvp => string.Format("CPU {0}", kvp.Key)).ToArray();
                var texts = local.Select(kvp => string.Format("{0} ({1})", kvp.Value.Id, FormatTimeSpan(now - kvp.Value.StartDate))).ToArray();

                var maxPadding = Math.Max(texts.Max(v => v.Length), headerItems.Max(v => v.Length));

                var currentTotalElapsed = DateTime.Now - start;
                var remaingingPartsCount = totalPartsCount - localCount;
                var averagePartTime = currentTotalElapsed.Ticks / localCount;

                var status = new[] 
                {
                    string.Empty,
                    string.Format("Parts to compute: from {0} to {1}", lowerBound, higherBound),
                    /*doneUpTo > lowerBound ? string.Format("If you ever need to resume the computation, start at {0}.", doneUpTo) : null,*/
                    "Current activity:",
                    "   " + string.Join(" | ", headerItems.Select(v => v.PadLeft(maxPadding))),
                    "   " + string.Join(" | ", texts.Select(v => v.PadLeft(maxPadding))),
                    string.Empty,
                    string.Format("{0} / {1} parts computed ({2:0.000}%)", localCount, totalPartsCount, 100m * localCount / totalPartsCount),
                    string.Format("Elapsed time: {0} ETA: {1}", currentTotalElapsed, DateTime.Now + TimeSpan.FromTicks(averagePartTime * remaingingPartsCount)),
                    string.Empty,
                };

                Console.WriteLine(string.Join(Environment.NewLine, status));
            }
        }

        private static void CompactRanges()
        {
            _ranges.Sort((range, range1) => range.Low < range1.Low ? -1 : (range.Low > range1.Low ? 1 : 0));
            for (int i = 0; i < _ranges.Count - 1; ++i)
            {
                if (_ranges[i + 1].Low == _ranges[i].High + 1)
                {
                    _ranges[i].High = _ranges[i + 1].High;
                    _ranges.RemoveAt(i + 1);
                    --i;
                }
            }
        }

        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan < TimeSpan.FromMinutes(1))
            {
                return timeSpan.Seconds + "s";
            }
            if (timeSpan < TimeSpan.FromHours(1))
            {
                return timeSpan.Minutes + "min " + timeSpan.ToString(@"ss") + "s";
            }
            if (timeSpan < TimeSpan.FromDays(1))
            {
                return timeSpan.Hours + "h " + timeSpan.ToString(@"mm") + "min " + timeSpan.ToString(@"ss") + "s";
            }
            return timeSpan.Days + "d " +  timeSpan.ToString(@"hh") + "h " + timeSpan.ToString(@"mm") + "min " + timeSpan.ToString(@"ss") + "s";
        }

        private static void WriteResultsToFile(string file, IEnumerable<int[]> seatings, ISeatingsFactory factory, int count, int totalCount, long id)
        {
            var iterator = seatings.GetEnumerator();
            var start = DateTime.Now;
            if (iterator.MoveNext())
            {
                Console.WriteLine("{0}/{1} ({2}) Writing results to: {3}", count, totalCount, id, file);

                //using (var dest = File.Open(file, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                using (var dest = File.CreateText(file))
                {
                    do
                    {
                        var seating = iterator.Current;
                        var line = factory.GetSeatingText(seating);
                        //var buffer = Encoding.Default.GetBytes(line);
                        //dest.Write(buffer, 0, buffer.Length);
                        dest.WriteLine(line);
                        dest.Flush();
                    }
                    while (iterator.MoveNext());
                }
                var elapsed = DateTime.Now - start;
                Console.WriteLine("{0}/{1} ({2})  Done (elapsed: {3} s)", count, totalCount, id, elapsed.TotalSeconds);
            }
        }

        public static void SetThreadProcessorAffinity(params int[] cpus)
        {
            if (cpus == null)
                throw new ArgumentNullException("cpus");
            if (cpus.Length == 0)
                throw new ArgumentException("You must specify at least one CPU.", "cpus");

            // Supports up to 64 processors
            long cpuMask = 0;
            foreach (int cpu in cpus)
            {
                if (cpu < 0 || cpu >= Environment.ProcessorCount)
                    throw new ArgumentException("Invalid CPU number.");

                cpuMask |= 1L << cpu;
            }

            // Ensure managed thread is linked to OS thread; does nothing on default host in current .Net versions
            Thread.BeginThreadAffinity();

#pragma warning disable 618
            // The call to BeginThreadAffinity guarantees stable results for GetCurrentThreadId,
            // so we ignore the obsolete warning
            int osThreadId = AppDomain.GetCurrentThreadId();
#pragma warning restore 618

            // Find the ProcessThread for this thread.
            ProcessThread thread = Process.GetCurrentProcess().Threads.Cast<ProcessThread>().Single(t => t.Id == osThreadId);
            // Set the thread's processor affinity
            thread.ProcessorAffinity = new IntPtr(cpuMask);
        }
    }
}
