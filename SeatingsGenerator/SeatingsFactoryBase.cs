namespace ConsoleApplication5
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Remoting;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class SeatingsFactoryBase : ISeatingsFactory
    {
        protected internal int N;

        protected int PodCount;
        protected int[] PodIndices;
        protected int[] PodSizes;
        protected int[] InitialPositions; // the initial position of the players (0-based index for both)
        protected int[] InitialPod; // the initial pod of the players (0-based index for both)
        protected int TotalTransfer; // for one round, for all players. Must be multiplied by the number of rounds and divided by the number of players
        protected int TotalVp;       // for one round, for all tables. Must be multiplied by the number of rounds and divided by the number of tables.
        protected int ExpectedTransfer;
        //protected bool IsExactTransfer;
        public decimal ExactTransfer;
        public decimal ExactVp;
        protected bool[] IsPreyOrPredR1;
        protected int SeatingArraySize;
        protected RelationShip[] InitialRelationShip;
        protected internal int[] PredatorIndices;
        protected internal int[] PreyIndices;
        protected internal byte[] PlayersMet;

        public long Count;
        protected int First4TableIndex = -1;

        protected SeatingsFactoryBase(int n)
        {
            N = n;
            ComputePods();
        }

        protected internal void ComputePods()
        {
            PodIndices = new int[(N / 5) + 1];
            PodSizes = new int[(N / 5) + 1];
            var n = N;
            PodCount = 0;
            var podIndex = 0;

            var initialPositions = new List<int>();
            var initialPod = new List<int>();

            while (n > 0)
            {
                int size;
                switch (n)
                {
                    case 1:
                    case 2:
                    case 3: throw new Exception("Can't compute pods");
                    case 4: size = 4; break;
                    case 5: size = 5; break;
                    case 6:
                    case 7: throw new Exception("Can't compute pods");
                    case 8: size = 4; break;
                    case 9:
                    case 10: size = 5; break;
                    case 11: throw new Exception("Can't compute pods");
                    case 12:
                    case 16: size = 4; break;
                    default: size = 5; break;
                }
                if (size == 4 && First4TableIndex == -1)
                {
                    First4TableIndex = PodCount;
                }

                PodIndices[PodCount] = podIndex;
                PodSizes[PodCount] = size;
                podIndex += size;
                n -= size;

                initialPositions.AddRange(Enumerable.Range(0, size));
                initialPod.AddRange(Enumerable.Repeat(PodCount, size));
                TotalTransfer += size == 4 ? 10 : 14;
                TotalVp += size;
                PodCount++;
            }

            //ExpectedTransfer = (int)(TotalTransfer / N * 2); // 2 rounds
            //IsExactTransfer = TotalTransfer % N == 0;
            InitialPositions = initialPositions.ToArray();
            InitialPod = initialPod.ToArray();

            SeatingArraySize = GetArraySize();
            IsPreyOrPredR1 = new bool[(N + 1) * (N + 1)];
            for (var i = 1; i <= N; ++i)
            {
                var pred = GetPredatorOnFirstRound(i);
                var prey = GetPreyOnFirstRound(i);
                IsPreyOrPredR1[i * N + pred] = true;
                IsPreyOrPredR1[pred * N + i] = true;
                IsPreyOrPredR1[i * N + prey] = true;
                IsPreyOrPredR1[prey * N + i] = true;
            }

            InitialRelationShip = new RelationShip[(N + 1) * (N + 1)];
            for (var i = 0; i <= N; ++i)
            {
                for (var j = 0; j <= N; ++j)
                {
                    InitialRelationShip[i * N + j] = GetInitialRelationShip(i, j);
                    InitialRelationShip[j * N + i] = GetInitialRelationShip(j, i);
                }
            }

            PredatorIndices = new int[N];
            PreyIndices = new int[N];
            var index = 0;
            for (var p = 0; p < PodCount; ++p)
            {
                for (var i = 0; i < PodSizes[p]; ++i)
                {
                    PredatorIndices[index] = GetPredatorIndex(i, p);
                    PreyIndices[index] = GetPreyIndex(i, p);
                    ++index;
                }
            }

            PlayersMet = new byte[N*N];
            for (int i = 0; i < N; ++i)
            {
                var p = InitialPod[i];
                for (int j = 0; j < PodSizes[p]; ++j)
                {
                    var p2 = PodIndices[p] + j;
                    if (p2 != i)
                    {
                        PlayersMet[i * N + p2]++;
                    }
                }
            }

            //var bestTransfer = int.MaxValue;
            //for (int i = 0; i <= N; ++i)
            //{
            //    for (int j = 0; j <= N - i; ++j)
            //    {
            //        //for (int k = N - j; i + j + k <= N; ++k)
            //        var k = N - i - j;
            //        if (i * 5 + j * 6 + k * 7 == TotalTransfer2)
            //        {
            //            var value = i * Math.Abs(5 * N - TotalTransfer2)
            //                + j * Math.Abs(6 * N - TotalTransfer2)
            //                + k * Math.Abs(7 * N - TotalTransfer2);
            //            if (bestTransfer > value)
            //            {
            //                bestTransfer = value;
            //            }
            //        }
            //    }
            //}
            //var deviation = (double)bestTransfer / (double)(N * N);
            //Console.WriteLine(deviation);
        }

        protected abstract int GetArraySize();
        protected abstract void InitializeSeating(int[] seating);

        public IEnumerable<int[]> GetSeatings()
        {
            var initialSeating = new int[SeatingArraySize]; // first extra int contains the last changed pod index, then the cycling of the tables

            IEnumerable<int[]> seatings = new[] { initialSeating };

            // THIS IS WORSE, DON'T USE (at least for N <= 18)
            //// constraints are stronger on players with the lowest starting position
            //// we iterate first on them to reduce the number of permutations
            //var players = Enumerable.Range(1, N)
            //    .Reverse()
            //    .OrderByDescending(v => PodSizes[InitialPod[v - 1] - 1])
            //    .ThenBy(v => InitialPositions[v - 1])
            //    .ToArray();

            var players = Enumerable.Range(1, N)
                .Reverse()
                .OrderBy(v => InitialPositions[v - 1])
                .ToArray();

            //var players = new[] { 6, 1 }.Concat(Enumerable.Range(1, N).Except(new[] { 6, 1 }));
            //var players = new[] { 1, 6, 11 }.Concat(Enumerable.Range(1, N).Except(new[] { 1, 6, 11 }));

            // old way (without brotherhood)
            foreach (var player in players)
            {
                seatings = GetSeatings(player, seatings);
            }
            return seatings;

            var brotherhood = new List<List<int>>();
            foreach (var player in players)
            {
                var list = brotherhood.FirstOrDefault(l => l.Any(p => InitialPositions[p - 1] == InitialPositions[player - 1] && PodSizes[InitialPod[p - 1]] == PodSizes[InitialPod[player - 1]]));
                if (list == null)
                {
                    brotherhood.Add(new List<int> { player });
                }
                else
                {
                    list.Add(player);
                }
            }

            var maxGroupSize = brotherhood.Max(brothers => brothers.Count);
            var mostInterestingGroupIndex = brotherhood.FindIndex(brothers => brothers.Count == maxGroupSize);

            int lastPlayer = -1;
            var firstGroupOfPlayers = brotherhood[mostInterestingGroupIndex];
            foreach (var player in firstGroupOfPlayers)
            {
                seatings = GetSeatings(player, seatings, lastPlayer); // cloned and evaluated

                lastPlayer = player;
            }

            //var toto = string.Join("\n", seatings.Select(GetSeatingText));
            //Console.WriteLine(toto);

            brotherhood.RemoveAt(mostInterestingGroupIndex);

            var otherPlayers = brotherhood.SelectMany(p => p).ToArray();
            foreach (var player in otherPlayers)
            {
                seatings = GetSeatings(player, seatings);
            }

            return seatings;
        }

        public IEnumerable<int[]>[] GetSeatingsParallel()
        {
            const int MaxParallelSize = 100;
            int[] players;
            int i;
            var arraySeatings = GetArraySeatings(MaxParallelSize, out players, out i);

            var result = new List<IEnumerable<int[]>>();

            foreach (var seating in arraySeatings)
            {
                IEnumerable<int[]> ienum = new[] { seating };
                var j = i;
                while (j < N)
                {
                    ienum = GetSeatings(players[j], ienum);
                    ++j;
                }
                result.Add(ienum);
            }

            return result.ToArray();
        }

        public int[][] GetArraySeatings(int maxParallelSize, out int[] players, out int i)
        {
            var initialSeating = new int[SeatingArraySize];

            var arraySeatings = new[] { initialSeating };

            players = Enumerable.Range(1, N)
                .Reverse()
                .OrderBy(v => InitialPositions[v - 1])
                .ToArray();

            i = 0;

            var brotherhood = GetBrotherhood(players);
            var untouchedBrotherHood = brotherhood.Select(b => b.ToArray()).ToArray();

#if SEVENTEEN
            // special case for 17 players
            // only 2 players won't be able to play at all on a 5-players table
            // we choose players 12 and 17

            initialSeating[6] = 12;
            initialSeating[11] = 17;
            initialSeating[17 + 12] = 12;
            initialSeating[17 + 14] = 17;

            InitializeSeating(initialSeating);
            players = new[] { 12, 17, 5, 4, 3, 2, 1 }.Union(players.Except(new[] { 12, 17, 5, 4, 3, 2, 1 })).ToArray();
            i = 2;
#endif
#if TWENTYONE
            // special case for 21 players
            // 6 players won't be able to play at all on a 5-players table
            // we choose players 8, 9, 12, 13, 17 and 21

            initialSeating[6] = 12;
            initialSeating[7] = 9;
            initialSeating[11] = 13;
            initialSeating[15] = 17;
            initialSeating[18] = 8;
            initialSeating[19] = 21;
            initialSeating[21 + 6] = 9;
            initialSeating[21 + 10] = 13;
            initialSeating[21 + 14] = 17;
            initialSeating[21 + 16] = 8;
            initialSeating[21 + 18] = 21;
            initialSeating[21 + 20] = 12;

            InitializeSeating(initialSeating);
            var constrainedPlayers = new[] { 8, 9, 12, 13, 17, 21, 5, 4, 3, 2, 1 };
            players = constrainedPlayers.Union(players.Except(constrainedPlayers)).ToArray();
            i = 6;
#endif
#if SEVENTEEN || TWENTYONE
            // old way (without brotherhood)
            do
            {
                arraySeatings = GetSeatings(players[i], arraySeatings)
                    .Select(seating =>
                    {
                        // we clone the seating because it's only valid until we iterate to the next item
                        var newSeating = new int[SeatingArraySize];
                        Buffer.BlockCopy(seating, 0, newSeating, 0, SeatingArraySize * sizeof(int));
                        return newSeating;
                    })
                    .ToArray();
                ++i;
            }
            while (arraySeatings.Length < maxParallelSize && i < N);
            return arraySeatings;
#endif
            //initialSeating[0] = 19;
            //initialSeating[1] = 23;
            //initialSeating[2] = 17;
            //initialSeating[3] = 21;
            //initialSeating[4] = 16;
            //initialSeating[5] = 24;
            //initialSeating[6] = 18;
            //InitializeSeating(initialSeating);
            //var seatedPlayers = new[] { 19, 23, 17, 21, 16, 24, 18, 22 };
            //players = seatedPlayers.Union(players.Except(seatedPlayers)).ToArray();
            //i = seatedPlayers.Length;

            var maxGroupSize = brotherhood.Max(brothers => brothers.Count);
            var mostInterestingGroupIndex = brotherhood.FindIndex(brothers => brothers.Count == maxGroupSize);

            int lastPlayer = -1;
            var firstGroupOfPlayers = brotherhood[mostInterestingGroupIndex];
            foreach (var player in firstGroupOfPlayers)
            {
                arraySeatings = GetSeatings(player, arraySeatings, lastPlayer); // cloned and evaluated

                lastPlayer = player;
            }

            var complexComparer = new R3SeatingsComplexComparer(untouchedBrotherHood, (Seatings3RFactory)this);

            var seatingAnalyses = arraySeatings
                .Select(s => new SeatingAnalysis() { Seating = s })
                .ToArray();

            //var t1 = DateTime.Now;
            //var duplicates = seatingAnalyses
            //    .GroupBy(x => x, complexComparer)
            //    .Where(group => group.Count() > 1)
            //    .ToArray();
            //Console.WriteLine(DateTime.Now - t1);

            //var t2 = DateTime.Now;
            //var test = seatingAnalyses.Distinct(complexComparer).ToArray();
            //Console.WriteLine(DateTime.Now - t2);

            //return new int[0][];

            //if (test.Length != arraySeatings.Length)
            //{
            //    var contents = string.Join("\r\n\r\n", duplicates.Select(g => string.Join("\r\n", g.Select(gg => GetSeatingText(gg.Seating)))));
            //    File.WriteAllText("duplicates.txt", contents);
            //}

            var distinct = seatingAnalyses.Distinct(complexComparer).ToArray();

            arraySeatings = distinct.Select(sa => sa.Seating).ToArray();

            //var toto = string.Join("\n", arraySeatings.Select(GetSeatingText));

            brotherhood.RemoveAt(mostInterestingGroupIndex);

            players = firstGroupOfPlayers.Union(brotherhood.SelectMany(p => p)).ToArray();
            i = firstGroupOfPlayers.Count;

            var t = DateTime.Now;
            // i = 0;
            do
            {
                arraySeatings = GetSeatings(players[i], arraySeatings)
                    .Select(seating =>
                        {
                            // we clone the seating because it's only valid until we iterate to the next item
                            var newSeating = new int[SeatingArraySize];
                            Buffer.BlockCopy(seating, 0, newSeating, 0, SeatingArraySize * sizeof(int));
                            return newSeating;
                        })
                    .ToArray();
                ++i;

                var seatingAnalysisItems = arraySeatings
                    .AsParallel()
                    .Select(s => new SeatingAnalysis { Seating = s, Analysis = ((Seatings3RFactory)this).GetAnalysis(s, untouchedBrotherHood) })
                    .ToArray();
                distinct = SequenceHelper.ParallelDistinct(seatingAnalysisItems, complexComparer);

                //distinct = arraySeatings
                //    .Select(s => new SeatingAnalysis() { Seating = s })
                //    .ToArray()
                //    .Distinct(complexComparer)
                //    .ToArray();
                arraySeatings = distinct.Select(sa => sa.Seating).ToArray();
            }
            while (arraySeatings.Length < maxParallelSize && i < N);

            Console.WriteLine(DateTime.Now - t);
            Console.WriteLine(arraySeatings.Length);

            return arraySeatings;
        }

        internal List<List<int>> GetBrotherhood(int[] players)
        {
            var brotherhood = new List<List<int>>();
            foreach (var player in players)
            {
                var list = brotherhood.FirstOrDefault(l => l.Any(p => InitialPositions[p - 1] == InitialPositions[player - 1] && PodSizes[InitialPod[p - 1]] == PodSizes[InitialPod[player - 1]]));
                if (list == null)
                {
                    brotherhood.Add(new List<int> { player });
                }
                else
                {
                    list.Add(player);
                }
            }
            return brotherhood;
        }

        public abstract IEnumerable<int[]> GetSeatings(int player, IEnumerable<int[]> seatings);
        public abstract int[][] GetSeatings(int player, IEnumerable<int[]> seatings, int lastPlayer);

        protected internal int GetPredatorOnFirstRound(int player)
        {
            --player;
            var initialPosition = InitialPositions[player];
            if (initialPosition > 0)
            {
                return player;
            }
            return player + PodSizes[InitialPod[player]];
        }

        protected internal int GetPreyOnFirstRound(int player)
        {
            var initialPosition = InitialPositions[player - 1];
            var initialPod = InitialPod[player - 1];
            var podSize = PodSizes[initialPod];
            if (initialPosition < podSize - 1)
            {
                return ++player;
            }
            return PodIndices[initialPod] + 1;
        }

        protected internal int GetPredatorIndex(int relativeIndex, int tableIndex)
        {
            if (relativeIndex > 0)
            {
                return PodIndices[tableIndex] + relativeIndex - 1;
            }
            return PodIndices[tableIndex] + PodSizes[tableIndex] - 1; // return the last player of the table
        }

        protected internal int GetPreyIndex(int relativeIndex, int tableIndex)
        {
            var podSize = PodSizes[tableIndex];
            if (relativeIndex < podSize - 1)
            {
                return PodIndices[tableIndex] + relativeIndex + 1;
            }
            return PodIndices[tableIndex]; // return the first player of the table
        }

        protected internal int GetLittlePreyIndex(int relativeIndex, int tableIndex, int podSize)
        {
            if (relativeIndex < podSize - 2)
            {
                return PodIndices[tableIndex] + relativeIndex + 2;
            }
            return PodIndices[tableIndex] - podSize + 2 + relativeIndex;
        }

        protected internal int GetGrandPredatorIndex(int relativeIndex, int tableIndex, int podSize)
        {
            if (relativeIndex > 1)
            {
                return PodIndices[tableIndex] + relativeIndex - 2;
            }
            return PodIndices[tableIndex] + podSize - 2 + relativeIndex;
        }

        protected internal bool IsNonEmptyPod(int[] seating, int podIndex)
        {
            var i = PodIndices[podIndex];
            return seating[i] != 0 || seating[i + 1] != 0 || seating[i + 2] != 0 || seating[i + 3] != 0 || (PodSizes[podIndex] == 5 && seating[i + 4] != 0);
        }

        /// <summary>
        /// 2. No pair of players share a table through all two rounds, when possible.  (N/A in some 2R event.)
        /// </summary>
        protected internal bool ViolateRule2(int[] seating)
        {
            int[] initialPods = null;
            //for (int p = 0; p < PodCount; ++p)
            var p = seating[N];
            {
                // the same initial pod must not appear twice
                var c = 0;
                for (int i = 0; i < PodSizes[p]; ++i)
                {
                    int podIndex = PodIndices[p];
                    var player = seating[podIndex + i];
                    if (player > 0)
                    {
                        if (initialPods == null)
                        {
                            initialPods = new int[5];
                        }
                        var initialPod = InitialPod[player - 1];
                        if (c > 0)
                        {
                            for (int j = 0; j < c; ++j)
                            {
                                if (initialPods[j] == initialPod)
                                {
                                    return true;
                                }
                            }
                        }
                        initialPods[c++] = initialPod;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 6. No pair of players repeat the same relative position[*], when possible.
        /// [*] "relative position" relationship values:
        /// A) prey
        /// B) predator
        /// C) cross-table at a 4-player
        /// D) grand-prey at a 5
        /// E) grand-predator at a 5
        /// </summary>
        /// <param name="seating"></param>
        /// <returns></returns>
        /// <remarks>Prey/predator relationship are not checked.</remarks>
        protected internal bool ViolateRule6(int[] seating)
        {
            // A and B are alredy handled by construction (rule 1)
            // for (int p = 0; p < PodCount; ++p)
            var p = seating[N];
            {
                if (ViolateRule6ForTable(seating, p))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a value indicating that the relationship is the same as the initial relationship for any player of a given pod. Prey/predator relationship are not checked.
        /// </summary>
        /// <param name="seating">The seating.</param>
        /// <param name="p">The pod to check.</param>
        /// <returns>True if the rule is violated, false otherwise.</returns>
        internal bool ViolateRule6ForTable(int[] seating, int p)
        {
            var podIndex = PodIndices[p];

            var first = seating[podIndex];
            var third = seating[podIndex + 2];
            var fourth = seating[podIndex + 3];

            if (PodSizes[p] == 5)
            {
                if (first != 0)
                {
                    var firstIndex = first * N;
                    if (third != 0 && InitialRelationShip[firstIndex + third] == RelationShip.GrandPredator
                        || fourth != 0 && InitialRelationShip[firstIndex + fourth] == RelationShip.LittlePrey)
                    {
                        return true;
                    }
                }

                var fifth = seating[podIndex + 4];
                var second = seating[podIndex + 1];
                if (second != 0)
                {
                    var secondIndex = second * N;
                    if (fourth != 0 && InitialRelationShip[secondIndex + fourth] == RelationShip.GrandPredator
                        || fifth != 0 && InitialRelationShip[secondIndex + fifth] == RelationShip.LittlePrey)
                    {
                        return true;
                    }
                }

                return third != 0 && fifth != 0 && InitialRelationShip[third * N + fifth] == RelationShip.GrandPredator;
                //if (GetInitialRelationShip(first, third) == RelationShip.GrandPredator
                //    || GetInitialRelationShip(first, fourth) == RelationShip.GrandPrey
                //    || GetInitialRelationShip(second, fourth) == RelationShip.GrandPredator
                //    || GetInitialRelationShip(second, fifth) == RelationShip.GrandPrey
                //    || GetInitialRelationShip(third, fifth) == RelationShip.GrandPredator)
                //{
                //    return true;
                //}
            }
            else
            {
                var second = seating[podIndex + 1];
                return first != 0 && third != 0 && InitialRelationShip[first * N + third] == RelationShip.CrossTable
                       || second != 0 && fourth != 0 && InitialRelationShip[second * N + fourth] == RelationShip.CrossTable;

                //if (GetInitialRelationShip(first, third) == RelationShip.CrossTable
                //    || GetInitialRelationShip(second, fourth) == RelationShip.CrossTable)
                //{
                //    return true;
                //}
            }
            return false;
        }

        internal bool ViolateRule6ForTable(int[] seating, int p, int startIndex)
        {
            var podIndex = PodIndices[p] + startIndex;

            var first = seating[podIndex];
            var third = seating[podIndex + 2];
            var fourth = seating[podIndex + 3];

            if (PodSizes[p] == 5)
            {
                if (first != 0)
                {
                    var firstIndex = first * N;
                    if (third != 0 && InitialRelationShip[firstIndex + third] == RelationShip.GrandPredator
                        || fourth != 0 && InitialRelationShip[firstIndex + fourth] == RelationShip.LittlePrey)
                    {
                        return true;
                    }
                }

                var fifth = seating[podIndex + 4];
                var second = seating[podIndex + 1];
                if (second != 0)
                {
                    var secondIndex = second * N;
                    if (fourth != 0 && InitialRelationShip[secondIndex + fourth] == RelationShip.GrandPredator
                        || fifth != 0 && InitialRelationShip[secondIndex + fifth] == RelationShip.LittlePrey)
                    {
                        return true;
                    }
                }

                return third != 0 && fifth != 0 && InitialRelationShip[third * N + fifth] == RelationShip.GrandPredator;
                //if (GetInitialRelationShip(first, third) == RelationShip.GrandPredator
                //    || GetInitialRelationShip(first, fourth) == RelationShip.GrandPrey
                //    || GetInitialRelationShip(second, fourth) == RelationShip.GrandPredator
                //    || GetInitialRelationShip(second, fifth) == RelationShip.GrandPrey
                //    || GetInitialRelationShip(third, fifth) == RelationShip.GrandPredator)
                //{
                //    return true;
                //}
            }
            else
            {
                var second = seating[podIndex + 1];
                return first != 0 && third != 0 && InitialRelationShip[first * N + third] == RelationShip.CrossTable
                       || second != 0 && fourth != 0 && InitialRelationShip[second * N + fourth] == RelationShip.CrossTable;

                //if (GetInitialRelationShip(first, third) == RelationShip.CrossTable
                //    || GetInitialRelationShip(second, fourth) == RelationShip.CrossTable)
                //{
                //    return true;
                //}
            }
            return false;
        }


        protected internal RelationShip GetInitialRelationShip(int a, int b)
        {
            if (a == 0 || b == 0 || a - b < -4 || a - b > 4)
            {
                return RelationShip.None;
            }

            var tableA = 0;
            var t = a;
            while (t > PodSizes[tableA])
            {
                t -= PodSizes[tableA];
                ++tableA;
            }
            if (b - 1 < PodIndices[tableA] || b - 1 >= PodIndices[tableA] + PodSizes[tableA])
            {
                return RelationShip.None;
            }

            var relA = InitialPositions[a - 1];
            var relB = InitialPositions[b - 1];

            if (relA == relB + 1 || relA == 0 && relB == PodSizes[tableA] - 1)
            {
                return RelationShip.Prey;
            }
            if (relA == relB - 1 || relA == PodSizes[tableA] - 1 && relB == 0)
            {
                return RelationShip.Predator;
            }

            if (PodSizes[tableA] == 4)
            {
                return RelationShip.CrossTable;
            }
            if (relA == relB + 2 || relA == 0 && relB == 3 || relA == 1 && relB == 4)
            {
                return RelationShip.LittlePrey;
            }
            return RelationShip.GrandPredator;
        }

        public IEnumerable<int[]> ApplyRule7(IEnumerable<int[]> seatings)
        {
            foreach (var seating in seatings)
            {
                int[] newSeating;
                if (!ViolateRule7(seating, true, out newSeating))
                {
                    yield return seating;
                }
                if (newSeating != null)
                {
                    yield return newSeating;
                }
            }
        }

        /// <summary>
        /// 7. A player doesn't play in the same seat position, if possible.
        /// </summary>
        protected internal bool ViolateRule7(int[] seating, bool computeNewSeating, out int[] newSeating)
        {
            newSeating = null;
            var result = false;
            // try to cycle each pod until the rule is no longer violated
            for (int p = 0; p < PodCount; ++p)
            {
                var podIndex = PodIndices[p];
                var podSize = PodSizes[p];
                var cycling = 0;

                while (cycling < podSize)
                {
                    bool violated = false;
                    for (int i = 0; i < podSize; ++i)
                    {
                        var player = seating[podIndex + (i + cycling) % podSize];
                        if (player > 0 && InitialPositions[player - 1] == i)
                        {
                            result = true;
                            violated = true;
                            break; // violated
                        }
                    }
                    if (violated)
                    {
                        ++cycling;
                    }
                    else
                    {
                        break;
                    }
                }
                if (cycling == podSize)
                {
                    newSeating = null;
                    return true; // couldn't find any way to cycle the pod to make it respect rule 7
                }
                if (computeNewSeating)
                {
                    if (newSeating == null)
                    {
                        newSeating = (int[])seating.Clone();
                    }

                    var temp = new int[podSize];
                    Array.Copy(newSeating, podIndex, temp, 0, podSize);

                    for (int i = 0; i < podSize; ++i)
                    {
                        newSeating[podIndex + i] = temp[(i + cycling) % podSize];
                    }
                }
            }
            return computeNewSeating && result;
        }

        /// <summary>
        /// 7. A player doesn't play in the same seat position, if possible.
        /// 8. Starting transfers are equitably distributed. [NOAL]
        /// </summary>
        protected internal bool ViolateRule78(int[] seating)
        {
            // try to cycle each pod until the rules are no longer violated
            //for (int p = 0; p < PodCount; ++p)
            var p = seating[N];
            {
                var podIndex = PodIndices[p];
                var podSize = PodSizes[p];
                var cycling = 0;

                while (cycling < podSize)
                {
                    bool violated = false;
                    var rel = cycling;
                    for (var i = 0; i < podSize; /*++i*/)
                    {
                        var player = seating[podIndex + rel];
                        ++i;
                        ++rel;
                        if (rel >= podSize)
                        {
                            rel = 0;
                        }
                        if (player != 0)
                        {
                            //int initialPosition = InitialPositions[player - 1];
                            //if (initialPosition == i + 1 // rule 7: same starting position
                            //    || initialPosition + i + 1 <= 3) // a player can't have 3 or less transfers, it's lame!

                            // faster writing: i is already incremented
                            var initialPosition = InitialPositions[player - 1];
                            if (initialPosition + i < 4 // a player can't have X or less transfers, it's lame!
                                || initialPosition == i) // rule 7: same starting position
                            {
                                violated = true;
                                break;
                            }
                        }
                    }
                    if (violated)
                    {
                        ++cycling;
                    }
                    else
                    {
                        seating[N + p + 1] = cycling; // we store the last good cycling
                        break;
                    }
                }
                if (cycling == podSize)
                {
                    return true; // couldn't find any way to cycle the pod to make it respect rule 7 and 8
                }
            }
            return false;
        }

        public virtual string GetSeatingText(int[] seating)
        {
            var stringBuilder = new StringBuilder();
            for (int p = 0; p < PodCount; ++p)
            {
                var podIndex = PodIndices[p];
                for (int i = 0; i < PodSizes[p]; ++i)
                {
                    var player = seating[podIndex + i];

                    stringBuilder.Append(player);
                    stringBuilder.Append(' ');
                }
                if (p < PodCount - 1)
                {
                    stringBuilder.Append("| ");
                }
            }
            return stringBuilder.ToString();
        }

        public virtual int GetSeatingTextLength()
        {
            int[] seating = Enumerable.Range(1, N).ToArray();
            return GetSeatingText(seating).Length;
        }

        public static int[] GetSeatingFromTextNoCheck(string text)
        {
            return text.Split('|', ' ', '\t').Where(s => s.Length > 0).Select(int.Parse).ToArray();
        }

        public virtual int[] GetSeatingFromText(string text)
        {
            var result = GetSeatingFromTextNoCheck(text);
            if (result.Length != N)
            {
                throw new Exception("Invalid size in input: " + text);
            }
            return result;
        }

        /// <summary>
        /// 1. No pair of players repeat their predator-prey relationship. This is mandatory, by the VEKN rules.
        /// </summary>
        /// <param name="seating"></param>
        /// <returns></returns>
        public Rule1Result CheckRule1(int[] seating)
        {
            for (int p = 0; p < PodCount; ++p)
            {
                var podIndex = PodIndices[p];
                for (int i = 0; i < PodSizes[p]; ++i)
                {
                    var first = podIndex + i;
                    var second = podIndex + ((i + 1) % PodSizes[p]);
                    if (GetInitialRelationShip(seating[first], seating[second]) == RelationShip.Predator)
                    {
                        return new Rule1Result { Violated = true, Player = seating[first], Table = p + 1, Position = i + 1 };
                    }
                }
            }
            return new Rule1Result { Violated = false };
        }

        /// <summary>
        /// 2. No pair of players share a table through all two rounds, when possible.  (N/A in some 2R event.)
        /// </summary>
        /// <param name="seating"></param>
        /// <returns></returns>
        public Rule2Result CheckRule2(int[] seating)
        {
            var playersMet = new Dictionary<int, List<int>>();
            for (int p = 0; p < PodCount; ++p)
            {
                // add the list of players met on R1
                var tablePlayers = Enumerable.Range(PodIndices[p] + 1, PodSizes[p]).ToArray();
                AddPlayersOnTheSameTableToThePlayersMet(tablePlayers, playersMet);

                // get the players on R2
                tablePlayers = seating.Skip(PodIndices[p]).Take(PodSizes[p]).ToArray();
                AddPlayersOnTheSameTableToThePlayersMet(tablePlayers, playersMet);
            }

            for (int i = 0; i < N; ++i)
            {
                playersMet[i].RemoveAll(v => v == i + 1); // remove the player from his tables
                if (playersMet[i].Count != playersMet[i].Distinct().Count())
                {
                    IEnumerable<int> duplicates = playersMet[i].GroupBy(s => s).SelectMany(grp => grp.Skip(1));
                    return new Rule2Result { Violated = true, Player = i + 1, OtherPlayer = duplicates.First() };
                }
            }
            return new Rule2Result { Violated = false, PlayersMet = playersMet };
        }

        protected static void AddPlayersOnTheSameTableToThePlayersMet(int[] tablePlayers, Dictionary<int, List<int>> playersMet)
        {
            foreach (var player in tablePlayers)
            {
                List<int> current;
                if (playersMet.TryGetValue(player - 1, out current))
                {
                    current.AddRange(tablePlayers);
                }
                else
                {
                    playersMet[player - 1] = new List<int>(tablePlayers);
                }
            }
        }

        /// <summary>
        /// 3. Available VPs are equitably distributed.
        /// </summary>
        /// <param name="seating"></param>
        /// <returns></returns>
        public Rule3Result CheckRule3(int[] seating)
        {
            var vps = new int[N];
            for (int p = 0; p < PodCount; ++p)
            {
                var podIndex = PodIndices[p];
                for (var i = 0; i < PodSizes[p]; ++i)
                {
                    vps[podIndex + i] += PodSizes[p];
                    vps[seating[podIndex + i] - 1] += PodSizes[p];
                }
            }
            var referenceVp = vps[0];
            if (vps.All(vp => vp == referenceVp))
            {
                return new Rule3Result { Violated = false };
            }

            return new Rule3Result { Violated = true, Vps = vps };
        }

        /// <summary>
        /// 5. A player doesn't sit in the fifth seat more than once.
        /// </summary>
        /// <param name="seating"></param>
        /// <returns></returns>
        public Rule5Result CheckRule5(int[] seating)
        {
            for (int p = 0; p < PodCount; ++p)
            {
                if (PodSizes[p] != 5)
                {
                    continue;
                }

                var podIndex = PodIndices[p];
                var fifthPlayer = seating[podIndex + 4];
                if (InitialPositions[fifthPlayer - 1] + 1 == 5)
                {
                    return new Rule5Result { Violated = true, Player = fifthPlayer, Table = p + 1 };
                }
            }
            return new Rule5Result { Violated = false };
        }

        /// <summary>
        /// 6. No pair of players repeat the same relative position[*], when possible.
        /// </summary>
        /// <param name="seating"></param>
        /// <returns></returns>
        public Rule6Result CheckRule6(int[] seating)
        {
            for (int p = 0; p < PodCount; ++p)
            {
                if (ViolateRule6ForTable(seating, p))
                {
                    return new Rule6Result { Violated = true, Table = p + 1 };
                }
            }
            return new Rule6Result { Violated = false };
        }

        /// <summary>
        /// 7. A player doesn't play in the same seat position, if possible.
        /// </summary>
        /// <param name="seating"></param>
        /// <returns></returns>
        public Rule7Result CheckRule7(int[] seating)
        {
            for (int p = 0; p < PodCount; ++p)
            {
                var podIndex = PodIndices[p];
                for (var i = 0; i < PodSizes[p]; ++i)
                {
                    var player = seating[podIndex + i];
                    var initialPosition = InitialPositions[player - 1];

                    if (initialPosition == i)
                    {
                        return new Rule7Result { Violated = true, Table = p + 1, Player = player, Position = i + 1 };
                    }
                }
            }
            return new Rule7Result { Violated = false };
        }

        /// <summary>
        /// 8. Starting transfers are equitably distributed. [NOAL]
        /// </summary>
        /// <param name="seating"></param>
        /// <returns></returns>
        public Rule8Result CheckRule8(int[] seating)
        {
            var transfers = GetTransfers(seating);
            var referenceTransfers = transfers[0];
            if (transfers.All(transfer => transfer == referenceTransfers))
            {
                return new Rule8Result { Violated = false };
            }

            return new Rule8Result { Violated = true, Transfers = transfers };
        }

        protected internal int[] GetTransfers(int[] seating)
        {
            var result = new int[N];
            var index = 0;
            for (var p = 0; p < PodCount; ++p)
            {
                //for (var i = 0; i < PodSizes[p]; ++i)
                //{
                //    var value = Math.Min(4, i + 1);
                //    result[index] += value;
                //    result[seating[index] - 1] += value;
                //    ++index;
                //}
                // inline:
                result[index]++;
                result[seating[index++] - 1]++;
                result[index] += 2;
                result[seating[index++] - 1] += 2;
                result[index] += 3;
                result[seating[index++] - 1] += 3;
                result[index] += 4;
                result[seating[index++] - 1] += 4;
                if (PodSizes[p] == 5)
                {
                    result[index] += 4;
                    result[seating[index++] - 1] += 4;
                }
            }
            return result;
        }

        protected internal int[] GetVps(int[] seating)
        {
            var result = new int[N];
            var podIndex = 0;
            for (var p = 0; p < PodCount; ++p)
            {
                var podSize = PodSizes[p];
                //for (int i = 0; i < podSize; ++i)
                //{
                //    result[podIndex + i] += podSize;
                //    result[seating[podIndex + i] - 1] += podSize;
                //}
                // inline:
                if (podSize == 4)
                {
                    result[podIndex] += 4;
                    result[seating[podIndex++] - 1] += 4;
                    result[podIndex] += 4;
                    result[seating[podIndex++] - 1] += 4;
                    result[podIndex] += 4;
                    result[seating[podIndex++] - 1] += 4;
                    result[podIndex] += 4;
                    result[seating[podIndex++] - 1] += 4;
                }
                else
                {
                    result[podIndex] += 5;
                    result[seating[podIndex++] - 1] += 5;
                    result[podIndex] += 5;
                    result[seating[podIndex++] - 1] += 5;
                    result[podIndex] += 5;
                    result[seating[podIndex++] - 1] += 5;
                    result[podIndex] += 5;
                    result[seating[podIndex++] - 1] += 5;
                    result[podIndex] += 5;
                    result[seating[podIndex++] - 1] += 5;
                }
            }
            return result;
        }

        public virtual int[] GetCycledSeating(int[] seating)
        {
            var result = new int[N];
            var index = 0;
            for (int p = 0; p < PodCount; ++p)
            {
                var podIndex = PodIndices[p];
                var cycling = seating[N + p + 1];
                for (int i = 0; i < PodSizes[p]; ++i)
                {
                    var rel = (i + cycling) % PodSizes[p];
                    var player = seating[podIndex + rel];

                    result[index++] = player;
                }
            }
            return result;
        }

        public abstract class RuleResult
        {
            public bool Violated;
            public int Player;
        }

        public class Rule1Result : RuleResult
        {
            public int Table;
            public int Position;
        }

        public class Rule2Result : RuleResult
        {
            public int OtherPlayer;
            public Dictionary<int, List<int>> PlayersMet;
        }

        public class Rule3Result : RuleResult
        {
            public int[] Vps;
        }

        public class Rule5Result : RuleResult
        {
            public int Table;
        }

        public class Rule6Result : RuleResult
        {
            public int Table;
        }

        public class Rule7Result : RuleResult
        {
            public int Table;
            public int Position;
        }

        public class Rule8Result : RuleResult
        {
            public int[] Transfers;
        }

        public virtual double GetTransferAbsoluteDeviation(int[] seating)
        {
            return GetTransfers(seating).Sum(v => Math.Abs((double)(v - ExactTransfer))) / N;
        }

        public virtual double GetVpAbsoluteDeviation(int[] seating)
        {
            return GetVps(seating).Sum(v => Math.Abs((double)(v - ExactVp))) / N;
        }

        public abstract int GetTransferAbsoluteDeviationFast(int[] seating);
        public abstract int GetVpAbsoluteDeviationFast(int[] seating);

        public IEnumerable<int[]> GetBestSeatings(IEnumerable<int[]> seatings)
        {
            int bestTransferAbsoluteDeviation = Int32.MaxValue;
            int bestVpAbsoluteDeviation = Int32.MaxValue;
            int bestMeetOnceCount = 0;

            foreach (var seating in seatings)
            {
                var transferDeviation = GetTransferAbsoluteDeviationFast(seating);
                if (transferDeviation < bestTransferAbsoluteDeviation)
                {
                    var vpDeviation = GetVpAbsoluteDeviationFast(seating);
                    if (vpDeviation <= bestVpAbsoluteDeviation)
                    {
                        bestTransferAbsoluteDeviation = transferDeviation;
                        bestVpAbsoluteDeviation = vpDeviation;
                        bestMeetOnceCount = GetMeetOnceCount(seating);

                        // we clone the seating because it's only valid until we iterate to the next item
                        var newSeating = new int[SeatingArraySize];
                        Buffer.BlockCopy(seating, 0, newSeating, 0, SeatingArraySize * sizeof(int));
                        yield return newSeating;
                    }
                }
                else if (transferDeviation == bestTransferAbsoluteDeviation)
                {
                    var meetOnceCount = GetMeetOnceCount(seating);
                    var vpDeviation = GetVpAbsoluteDeviationFast(seating);
                    if (vpDeviation < bestVpAbsoluteDeviation)
                    {
                        bestTransferAbsoluteDeviation = transferDeviation;
                        bestVpAbsoluteDeviation = vpDeviation;
                        bestMeetOnceCount = meetOnceCount;

                        // we clone the seating because it's only valid until we iterate to the next item
                        var newSeating = new int[SeatingArraySize];
                        Buffer.BlockCopy(seating, 0, newSeating, 0, SeatingArraySize * sizeof(int));
                        yield return newSeating;
                    }
                    else if (vpDeviation == bestVpAbsoluteDeviation && meetOnceCount > bestMeetOnceCount)
                    {
                        bestTransferAbsoluteDeviation = transferDeviation;
                        bestVpAbsoluteDeviation = vpDeviation;
                        bestMeetOnceCount = meetOnceCount;

                        // we clone the seating because it's only valid until we iterate to the next item
                        var newSeating = new int[SeatingArraySize];
                        Buffer.BlockCopy(seating, 0, newSeating, 0, SeatingArraySize * sizeof(int));
                        yield return newSeating;
                    }
                }
            }
        }

        public abstract int GetMeetOnceCount(int[] seatings);

        public IEnumerable<int[]> GetBestSeatingsAbsolute(IEnumerable<int[]> seatings)
        {
            if (!seatings.Any())
            {
                return Enumerable.Empty<int[]>();
            }

            // the best seatings are those with the smaller absolute deviation
            seatings = seatings
                .Select(s => new { Dev = GetTransferAbsoluteDeviation(s), Seat = s })
                .GroupBy(p => p.Dev)
                .OrderBy(g => g.Key)
                .First()
                .Select(p => p.Seat);

            seatings = seatings
                .Select(s => new { Dev = GetVpAbsoluteDeviation(s), Seat = s })
                .GroupBy(p => p.Dev)
                .OrderBy(g => g.Key)
                .First()
                .Select(p => p.Seat);

            // and those that maximize the number of times each player meet each other only once

            var zorg = seatings
                .Select(s => new { EncounterOnceCount = GetMeetOnceCount(s), Seat = s })
                .GroupBy(p => p.EncounterOnceCount)
                .ToArray();
            seatings = zorg
                .OrderByDescending(g => g.Key)
                .First()
                .Select(p => p.Seat);

            return seatings;
        }
    }
}
