using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication5
{
    public sealed class Seatings3RFactory : SeatingsFactoryBase
    {
        protected internal int[] ExpectedNumberOfPlayerWithVps;
        internal int TotalTransfer3;
        internal int TotalVp3;

        public Seatings3RFactory(int n) :
            base(n)
        {
            TotalTransfer3 = TotalTransfer * 3;
            TotalVp3 = TotalVp * 3;
            ExactTransfer = (decimal)TotalTransfer / N * 3;
            ExactVp = (decimal)TotalVp / PodCount * 3;

            ABSOLUTE_DEVIATION_TRANSFERS = 2 * N;
            ABSOLUTE_DEVIATION_VP = 2 * N + 1;
            R2_POSITION = 2 * N + 2;
            R2_POD = 3 * N + 2;
            R2_PLAYERS_SEATED_IN_POD = 4 * N + 6;
            R3_PLAYERS_SEATED_IN_POD = 4 * N + 6 + PodCount;
            PLAYERS_WITH_6_TRANSFERS = 4 * N + 6 + 2 * PodCount;
            FIVE_PLAYERS_OCCURENCE = 4 * N + 7 + 2 * PodCount;
            PLAYERS_WITH_0_VP = 4 * N + 2 - 12;

#if EIGHTTEEN
            ExpectedNumberOfPlayerWithVps = new int[16];
            ExpectedNumberOfPlayerWithVps[13] = 6;
            ExpectedNumberOfPlayerWithVps[14] = 12;
#endif
#if NINETEEN
            ExpectedNumberOfPlayerWithVps = new int[16];
            ExpectedNumberOfPlayerWithVps[14] = 12;
            ExpectedNumberOfPlayerWithVps[15] = 7;
#endif
#if TWENTYTWO
            ExpectedNumberOfPlayerWithVps = new int[16];
            ExpectedNumberOfPlayerWithVps[13] = 12;
            ExpectedNumberOfPlayerWithVps[14] = 10;
#endif
        }

        private readonly int ABSOLUTE_DEVIATION_TRANSFERS;
        private readonly int ABSOLUTE_DEVIATION_VP;
        private readonly int R2_POSITION;
        private readonly int R2_POD;
        private readonly int R2_PLAYERS_SEATED_IN_POD;
        private readonly int R3_PLAYERS_SEATED_IN_POD;
        private readonly int PLAYERS_WITH_6_TRANSFERS;
        private readonly int FIVE_PLAYERS_OCCURENCE;
        private readonly int PLAYERS_WITH_0_VP;

#if THIRTEEN
        private const int ABSOLUTE_DEVIATION_TRANSFERS = 26;
        private const int ABSOLUTE_DEVIATION_VP = 27;
        private const int R2_POSITION = 28;
        private const int R2_POD = 41;
        private const int R2_PLAYERS_SEATED_IN_POD = 58;
        private const int R3_PLAYERS_SEATED_IN_POD = 61;
        private const int PLAYERS_WITH_6_TRANSFERS = 64;
        private const int FIVE_PLAYERS_OCCURENCE = 65;
        private const int PLAYERS_WITH_0_VP = 42;
#endif
#if EIGHTTEEN
        private const int ABSOLUTE_DEVIATION_TRANSFERS = 36;
        private const int ABSOLUTE_DEVIATION_VP = 37;
        private const int R2_POSITION = 38;
        private const int R2_POD = 56;
        private const int R2_PLAYERS_SEATED_IN_POD = 78;
        private const int R3_PLAYERS_SEATED_IN_POD = 82;
        private const int PLAYERS_WITH_6_TRANSFERS = 86;
        private const int FIVE_PLAYERS_OCCURENCE = 87;
        private const int PLAYERS_WITH_0_VP = 62;
#endif
#if TWENTYONE
        private const int ABSOLUTE_DEVIATION_TRANSFERS = 42;
        private const int ABSOLUTE_DEVIATION_VP = 43;
        private const int R2_POSITION = 44;
        private const int R2_POD = 65;
        private const int R2_PLAYERS_SEATED_IN_POD = 90;
        private const int R3_PLAYERS_SEATED_IN_POD = 95;
        private const int PLAYERS_WITH_6_TRANSFERS = 100;
        private const int FIVE_PLAYERS_OCCURENCE = 101;
        private const int PLAYERS_WITH_0_VP = 74;
#endif
#if TWENTYTWO
        private const int ABSOLUTE_DEVIATION_TRANSFERS = 44;
        private const int ABSOLUTE_DEVIATION_VP = 45;
        private const int R2_POSITION = 46;
        private const int R2_POD = 68;
        private const int R2_PLAYERS_SEATED_IN_POD = 94;
        private const int R3_PLAYERS_SEATED_IN_POD = 99;
        private const int PLAYERS_WITH_6_TRANSFERS = 104;
        private const int FIVE_PLAYERS_OCCURENCE = 105;
        private const int PLAYERS_WITH_0_VP = 78;
#endif
        public IEnumerable<int[]> GetPossibleTablesMetOnlyOnce()
        {
            IEnumerable<int[]> tables = new[] { new int[N * 2 + PodCount * 2] };

            for (int i = 1; i < N + 1; ++i)
            {
                tables = GetPossibleTablesMetOnlyOnce(i, tables)
                .Select(table =>
                {
                    // we clone the seating because it's only valid until we iterate to the next item
                    var newTable = new int[N * 2 + PodCount * 2];
                    Buffer.BlockCopy(table, 0, newTable, 0, (N * 2 + PodCount * 2) * sizeof(int));
                    return newTable;
                })
                .ToArray();
            }

            return tables;
        }

        public IEnumerable<int[]> GetPossibleTablesMetTwice(int limit)
        {
            IEnumerable<int[]> tables = new[] { new int[N * 2 + PodCount * 2 + 1] };

            for (int i = 1; i < N + 1; ++i)
            {
                tables = GetPossibleTablesMetTwice(i, tables, limit);
                //.Select(table =>
                //{
                //    // we clone the seating because it's only valid until we iterate to the next item
                //    var newTable = new int[N * 2 + PodCount * 2];
                //    Buffer.BlockCopy(table, 0, newTable, 0, (N * 2 + PodCount * 2 + 1) * sizeof(int));
                //    return newTable;
                //})
                //.ToArray();
            }

            return tables;
        }

        private IEnumerable<int[]> GetPossibleTablesMetOnlyOnce(int player, IEnumerable<int[]> tables)
        {
            var sharedPlayers = new byte[N];
            var sharedPlayers3 = new byte[N];
            foreach (var table in tables)
            {
                for (int p = 0; p < PodCount; ++p)
                {
                    var playersSeatedOnTableCount = table[2 * N + p];
                    var podSize = PodSizes[p];
                    if (playersSeatedOnTableCount == podSize) // table is full
                    {
                        continue;
                    }

                    // precomputation for rule 2
                    var metSomeoneTwice = false;
                    Buffer.BlockCopy(PlayersMet, (player - 1) * N, sharedPlayers, 0, N); // sizeof(byte) is 1
                    var count = 0;
                    int scan = 0;
                    while (scan < N && count < playersSeatedOnTableCount)
                    {
                        if (table[scan] == p + 1)
                        {
                            if (++sharedPlayers[scan] > 1)
                            {
                                metSomeoneTwice = true;
                                break;
                            }
                            ++count;
                        }
                        ++scan;
                    }

                    if (metSomeoneTwice)
                    {
                        continue;
                    }

                    table[2 * N + p]++; // seat the player at this table
                    table[player - 1] = p + 1;

                    for (int p3 = 0; p3 < PodCount; ++p3)
                    {
                        var playersSeatedOnTableCount3 = table[2 * N + PodCount + p3];
                        var podSize3 = PodSizes[p3];
                        if (playersSeatedOnTableCount3 == podSize3) // table is full
                        {
                            continue;
                        }

                        Buffer.BlockCopy(sharedPlayers, 0, sharedPlayers3, 0, N); // sizeof(byte) is 1
                        count = 0;
                        scan = 0;
                        var failure = false;
                        while (scan < N && count < playersSeatedOnTableCount3)
                        {
                            if (table[N + scan] == p3 + 1)
                            {
                                if (++sharedPlayers3[scan] > 1)
                                {
                                    failure = true;
                                    break;
                                }
                                ++count;
                            }
                            ++scan;
                        }

                        if (failure)
                        {
                            continue;
                        }

                        table[2 * N + PodCount + p3]++; // seat the player at this table
                        table[N + player - 1] = p3 + 1;

                        yield return table;

                        // restore state
                        table[2 * N + PodCount + p3]--;
                        table[N + player - 1] = 0;
                    }

                    // restore state
                    table[2 * N + p]--; // seat the player at this table
                    table[player - 1] = 0;
                }
            }
        }

        private IEnumerable<int[]> GetPossibleTablesMetTwice(int player, IEnumerable<int[]> tables, int limit)
        {
            var sharedPlayers = new byte[N];
            var sharedPlayers3 = new byte[N];
            foreach (var table in tables)
            {
                var backupCount = table[N * 2 + PodCount * 2];
                for (int p = 0; p < PodCount; ++p)
                {
                    table[N * 2 + PodCount * 2] = backupCount;
                    var playersSeatedOnTableCount = table[2 * N + p];
                    var podSize = PodSizes[p];
                    if (playersSeatedOnTableCount == podSize) // table is full
                    {
                        continue;
                    }

                    // precomputation for rule 2
                    var metSomeoneTwice = false;
                    Buffer.BlockCopy(PlayersMet, (player - 1) * N, sharedPlayers, 0, N); // sizeof(byte) is 1
                    var count = 0;
                    int scan = 0;
                    var failure = false;
                    while (scan < N && count < playersSeatedOnTableCount)
                    {
                        if (table[scan] == p + 1)
                        {
                            if (++sharedPlayers[scan] > 1)
                            {
                                if (++table[N * 2 + PodCount * 2] > limit)
                                {
                                    failure = true;
                                    break;
                                }
                                metSomeoneTwice = true;
                            }
                            ++count;
                        }
                        ++scan;
                    }

                    if (failure)
                    {
                        // restore state
                        table[N * 2 + PodCount * 2] = backupCount;
                        continue;
                    }

                    table[2 * N + p]++; // seat the player at this table
                    table[player - 1] = p + 1;
                    var backupCountR3 = table[N * 2 + PodCount * 2];

                    for (int p3 = 0; p3 < PodCount; ++p3)
                    {
                        table[N * 2 + PodCount * 2] = backupCountR3;
                        var playersSeatedOnTableCount3 = table[2 * N + PodCount + p3];
                        var podSize3 = PodSizes[p3];
                        if (playersSeatedOnTableCount3 == podSize3) // table is full
                        {
                            continue;
                        }

                        failure = false;
                        if (metSomeoneTwice)
                        {
                            Buffer.BlockCopy(sharedPlayers, 0, sharedPlayers3, 0, N); // sizeof(byte) is 1
                            count = 0;
                            scan = 0;
                            while (scan < N && count < playersSeatedOnTableCount3)
                            {
                                if (table[N + scan] == p3 + 1)
                                {
                                    if (++sharedPlayers3[scan] > 2 || (sharedPlayers3[scan] == 2 && ++table[N * 2 + PodCount * 2] > limit))
                                    {
                                        failure = true;
                                        break;
                                    }
                                    ++count;
                                }
                                ++scan;
                            }
                        }

                        if (failure)
                        {
                            // restore state
                            table[N * 2 + PodCount * 2] = backupCountR3;
                            continue;
                        }

                        table[2 * N + PodCount + p3]++; // seat the player at this table
                        table[N + player - 1] = p3 + 1;

                        yield return table;

                        // restore state
                        table[2 * N + PodCount + p3]--;
                        table[N + player - 1] = 0;
                        table[N * 2 + PodCount * 2] = backupCountR3;
                    }

                    // restore state
                    table[2 * N + p]--; // seat the player at this table
                    table[player - 1] = 0;
                    table[N * 2 + PodCount * 2] = backupCount;
                }
            }
        }

        protected override int GetArraySize()
        {
            return 5 * N + 7 + 2 * PodCount;
            // 2N  : the current absolute deviation for transfers
            // 2N+1: the current absolute deviation for vps
            // 2N+2+i: the position (0-based index) of i (0-based index) in the R2 pod 
            // 3N+2+i: the pod (0-based index) containing i (0-based index) in the R2 
            // 4N+2: number of players with 12 vp
            // 4N+3: number of players with 13 vp
            // 4N+4: number of players with 14 vp
            // 4N+5: number of players with 15 vp
            // 4N+6+i: number of players seated in pod i in R2 (0-based index)
            // 4N+6+PodCount+i: number of players seated in pod i in R3 (0-based index)
            // 4N+6+2*PodCount: number of players with 6 transfers
            // 4N+7+2*PodCount+i: number of times the player has been seated in a 5-players table
        }

        public int[] GetSeating(int[] r2Seating, int[] r3Seating)
        {
            var result = new int[GetArraySize()];
            Array.Copy(r2Seating, 0, result, 0, N);
            Array.Copy(r3Seating, 0, result, N, N);
            InitializeSeating(result);
            return result;
        }

        protected override void InitializeSeating(int[] seating)
        {
            var currentIndex = 0;
            for (var p = 0; p < PodCount; ++p)
            {
                var podSize = PodSizes[p];

                for (int i = 0; i < podSize; ++i)
                {
                    //if (podSize == 5)
                    //{
                    //    seating[FIVE_PLAYERS_OCCURENCE + PodIndices[p] + i]++; // number of times player has been seated in a 5-players pod
                    //}

                    var player = seating[currentIndex++];
                    if (player == 0)
                    {
                        continue;
                    }
                    --player;
                    seating[R2_POSITION + player] = i; // the position of i in the R2 pod (0-based)
                    seating[R2_POD + player] = p; // the pod containing i in the R2 (0-based)
                    seating[R2_PLAYERS_SEATED_IN_POD + p]++; // number of players seated in pod i in R2
                    //if (podSize == 5)
                    //{
                    //    seating[FIVE_PLAYERS_OCCURENCE + player]++; // number of times player has been seated in a 5-players pod
                    //}
                }
            }
            for (var p = 0; p < PodCount; ++p)
            {
                var podSize = PodSizes[p];
                for (int i = 0; i < podSize; ++i)
                {
                    var player = seating[currentIndex++];
                    if (player == 0)
                    {
                        continue;
                    }
                    --player;
                    var initialPosition = InitialPositions[player];
                    var r2Position = seating[R2_POSITION + player] + 1; // 1-based index
                    var transfers = Math.Min(4, initialPosition + 1) + Math.Min(4, i + 1) + Math.Min(4, r2Position);

                    var initialPodSize = PodSizes[InitialPod[player]];
                    var r2PodSize = PodSizes[seating[R2_POD + player]];
                    var vps = initialPodSize + podSize + r2PodSize;

                    seating[4 * N + vps - 10]++;

                    var transfersDeviation = Math.Abs(transfers * N - TotalTransfer3);
                    seating[ABSOLUTE_DEVIATION_TRANSFERS] += transfersDeviation;

                    var vpsDeviation = Math.Abs(vps * PodCount - TotalVp3);
                    seating[ABSOLUTE_DEVIATION_VP] += vpsDeviation;

                    seating[R3_PLAYERS_SEATED_IN_POD + p]++; // number of seated players in pod p in R3

#if !TWENTYTWO
                    if (transfers == 6)
                    {
                        seating[PLAYERS_WITH_6_TRANSFERS]++;
                    }
#endif
                    //if (podSize == 5)
                    //{
                    //    seating[FIVE_PLAYERS_OCCURENCE + player]++; // number of times player has been seated in a 5-players pod
                    //}
                }
            }
        }

        public override IEnumerable<int[]> GetSeatings(int player, IEnumerable<int[]> seatings)
        {
            return seatings
                .SelectMany(seating => GetSeatings(player, seating, 0, 0));
        }

        public override int[][] GetSeatings(int player, IEnumerable<int[]> seatings, int lastPlayer)
        {
            var result = seatings
                .SelectMany(seating => GetSeatings(player, seating, lastPlayer))
                .Select(seating =>
                {
                    // we clone the seating because it's only valid until we iterate to the next item
                    var newSeating = new int[SeatingArraySize];
                    Buffer.BlockCopy(seating, 0, newSeating, 0, SeatingArraySize * sizeof(int));
                    return newSeating;
                })
                .ToArray();

            var comparer = new R3SeatingsComparer(N);

            result = result.Distinct(comparer).ToArray();

            return result;
            // TODO: eliminate duplicates
        }

        private IEnumerable<int[]> GetSeatings(int player, int[] seating, int lastPlayer)
        {
            // set the starting pod (p) and the starting position in the pod (startingPosition)
            for (int p = 0; p < PodCount; ++p)
            {
                var podSize = PodSizes[p];
                for (var startingPosition = 0; startingPosition < podSize; ++startingPosition)
                {
                    int index = PodIndices[p] + startingPosition;
                    if (seating[index] == lastPlayer)
                    {
                        //++startingPosition;
                        return GetSeatings(player, seating, p, startingPosition);
                    }
                }
            }
            return GetSeatings(player, seating, 0, 0);
        }

        public override bool MoveNextSeating(State state)
        {
            var 

        private IEnumerable<int[]> GetSeatings(int player, int[] seating, int p, int startingPosition)
        {
            bool placedInA5PlayerEmptyPod = false;
            bool placedInA4PlayerEmptyPod = false;
            var initialPosition = InitialPositions[player - 1];

#if !SKIP_RULE_2
            var sharedPlayers = new byte[N];
#endif
            for (; p < PodCount; ++p)
            {
                var podSize = PodSizes[p];

#if !SKIP_RULE_2
                // precomputation for rule 2
                var metSomeoneTwice = false;
                Buffer.BlockCopy(PlayersMet, (player - 1) * N, sharedPlayers, 0, N); // sizeof(byte) is 1
                var scanIndex = PodIndices[p];
                for (int scan = 0; scan < podSize; ++scan)
                {
                    var sp = seating[scanIndex++];
                    if (sp != 0 && sp != player)
                    {
                        if (++sharedPlayers[sp - 1] == 2)
                        {
                            metSomeoneTwice = true;
                        }
                    }
                }
#endif

#if SEVENTEEN || TWENTYONE
                // custom case for 17 and 21 players: can't seat twice in a 5-players table 
                if (podSize == 5 && seating[FIVE_PLAYERS_OCCURENCE + player - 1] > 0)
                {
                    p = First4TableIndex - 1;
                    continue;
                }
                // end of custom case for 17 and 21 players
#endif
#if TWENTYONE
                // if a player is seated a second time on a 4-players table and the only 5-players table on R3 is full, then break
                if (podSize == 4 && player > 5 && seating[R3_PLAYERS_SEATED_IN_POD] == 5)
                {
                    break;
                }
#endif

                var vps = PodSizes[InitialPod[player - 1]] + podSize;
#if EIGHTTEEN || TWENTYTWO
                const int specialVp = 8; // if the players has 8 vps on R2, he'll have 13 vps on R3
#endif
#if NINETEEN
                const int specialVp = 9; // if the players has 9 vps on R2, he'll have 14 vps on R3
#endif

#if EIGHTTEEN || NINETEEN || TWENTYTWO
                if (vps != specialVp || seating[PLAYERS_WITH_0_VP + specialVp + 5] < ExpectedNumberOfPlayerWithVps[specialVp + 5])
#endif
                {
#if EIGHTTEEN || NINETEEN || TWENTYTWO
                    if (vps == specialVp)
                    {
                        seating[PLAYERS_WITH_0_VP + specialVp + 5]++;
                    }
#endif
                    var playersInPod2 = seating[R2_PLAYERS_SEATED_IN_POD + p];
                    if (playersInPod2 == podSize) // pod is full
                    {
                        continue;
                    }

                    for (int i = startingPosition; i < podSize; ++i)
                    {
#if COUNT
                        // uncomment to get also the count of failed attempts
                        //lock (this)
                        //{
                        //    Count++;
                        //}
#endif
                        var index = PodIndices[p] + i;
                        if (seating[index] == 0) // empty seat
                        {
                            if (i == startingPosition && playersInPod2 == 0) // look if the pod is empty
                            {
                                if (podSize == 5)
                                {
                                    if (!placedInA5PlayerEmptyPod)
                                    {
                                        placedInA5PlayerEmptyPod = true;
                                    }
                                    else
                                    {
                                        if (First4TableIndex == -1)
                                        {
                                            // all the following tables have the same size
                                            // restore whatever we have changed
#if EIGHTTEEN || NINETEEN || TWENTYTWO
                                            if (vps == specialVp)
                                            {
                                                seating[PLAYERS_WITH_0_VP + specialVp + 5]--;
                                            }
#endif
                                            yield break;
                                        }
                                        p = First4TableIndex;
                                        // index = PodIndices[p];
                                        --p;
                                        break; // break for (int i = 0; i < podSize; ++i) 
                                    }
                                }
                                else
                                {
                                    if (!placedInA4PlayerEmptyPod)
                                    {
                                        placedInA4PlayerEmptyPod = true;
                                    }
                                    else
                                    {
                                        // all the following tables have the same size
                                        // restore whatever we have changed
#if EIGHTTEEN || NINETEEN || TWENTYTWO
                                        if (vps == specialVp)
                                        {
                                            seating[PLAYERS_WITH_0_VP + specialVp + 5]--;
                                        }
#endif
                                        yield break;
                                    }
                                }
                            }

                            if (initialPosition != i) // rule 7: same starting position
                            {
                                //if (initialPosition + i >= 2) // to have at least 8 transfers, a player must have at least 4 transfers in r2 (note that the real test is initialPosition + 1 + player + 1 >= 4
                                {
                                    var transfers = Math.Min(4, initialPosition + 1) + Math.Min(4, i + 1);
                                    //if (transfers <= 7) // we limit to 7 transfers
                                    {
                                        // rule 1. No pair of players repeat their predator-prey relationship. This is mandatory, by the VEKN rules.
                                        var predator = seating[PredatorIndices[index]];
                                        int prey;

                                        if ((predator == 0 || !IsPreyOrPredR1[player * N + predator])
                                            && ((prey = seating[PreyIndices[index]]) == 0 || !IsPreyOrPredR1[player * N + prey]))
                                        {
                                            seating[index] = player; // seat the player
                                            if (!ViolateRule6ForTable(seating, p))
                                            {
                                                {
                                                    seating[R2_PLAYERS_SEATED_IN_POD + p]++; // number of players seated in pod p in R2
                                                    // 2N+2+i: the position (0-based index) of i (0-based index) in the R2 pod 
                                                    // 3N+2+i: the pod (0-based index) containing i (0-based index) in the R2 
                                                    seating[R2_POSITION + player - 1] = i; // used to know the positions of other players (otherwise we can still use i for the current player)
                                                    seating[R2_POD + player - 1] = p; // used to know the positions of other players (otherwise we can still use P for the current player)
#if SEVENTEEN || TWENTYONE
                                                    if (podSize == 5)
                                                    {
                                                        seating[FIVE_PLAYERS_OCCURENCE + player - 1]++;
                                                    }
#endif
                                                    /////////////////////////////////////
                                                    //        now we iterate on R3     //
                                                    /////////////////////////////////////

                                                    bool placedInA5PlayerEmptyPodR3 = false;
                                                    bool placedInA4PlayerEmptyPodR3 = false;

                                                    for (var p3 = 0; p3 < PodCount; ++p3)
                                                    {
                                                        var podSize3 = PodSizes[p3];

#if SEVENTEEN || TWENTYONE
                                                        // custom case for 17 and 21 players: can't seat twice in a 5-players table 
                                                        if (podSize3 == 5 && seating[FIVE_PLAYERS_OCCURENCE + player - 1] > 0)
                                                        {
                                                            p3 = First4TableIndex - 1;
                                                            continue;
                                                        }
                                                        // end of custom case for 17 and 21 players
#endif
#if EIGHTTEEN || NINETEEN || TWENTYTWO
                                                        if (vps == specialVp && podSize3 < 5) // there are no other 5 players table, and he must be sit on a 5-players table, so we break
                                                        {
                                                            break;
                                                        }
#endif

                                                        var vps3 = vps + podSize3;
#if EIGHTTEEN || NINETEEN || TWENTYTWO
                                                        if (vps == specialVp || seating[PLAYERS_WITH_0_VP + vps3] < ExpectedNumberOfPlayerWithVps[vps3]) // we expect no more than x players to have that amount of vps
#endif
                                                        {
                                                            var playersInPod3 = seating[R3_PLAYERS_SEATED_IN_POD + p3];
                                                            if (playersInPod3 == podSize3) // pod is full
                                                            {
                                                                continue;
                                                            }

#if !SKIP_RULE_2
                                                            // rule 2. No pair of players share a table through all three rounds, when possible.
                                                            var rule2Violated = false;
                                                            if (metSomeoneTwice && playersInPod3 > 0) // otherwise it's impossible to meet a third time
                                                            {
                                                                scanIndex = N + PodIndices[p3];
                                                                int sp;
                                                                rule2Violated = ((sp = seating[scanIndex]) > 0 && sp != player && sharedPlayers[sp - 1] == 2)
                                                                    || ((sp = seating[scanIndex + 1]) > 0 && sp != player && sharedPlayers[sp - 1] == 2)
                                                                    || ((sp = seating[scanIndex + 2]) > 0 && sp != player && sharedPlayers[sp - 1] == 2)
                                                                    || ((sp = seating[scanIndex + 3]) > 0 && sp != player && sharedPlayers[sp - 1] == 2);

                                                                if (!rule2Violated && podSize3 == 5)
                                                                {
                                                                    rule2Violated = ((sp = seating[scanIndex + 4]) > 0 && sp != player && sharedPlayers[sp - 1] == 2);
                                                                }
                                                            }

                                                            if (!rule2Violated)
#endif
                                                            {
                                                                for (int i3 = 0; i3 < podSize3; ++i3)
                                                                {
#if COUNT
                                                                    // uncomment to get also the count of failed attempts
                                                                    //lock (this)
                                                                    //{
                                                                    //    Count++;
                                                                    //}
#endif
                                                                    var index3 = PodIndices[p3] + i3;
                                                                    var absoluteIndex3 = N + index3;
                                                                    if (seating[absoluteIndex3] == 0) // empty seat
                                                                    {
                                                                        if (i3 == 0 && playersInPod3 == 0) // look if the pod is empty
                                                                        {
                                                                            if (podSize3 == 5)
                                                                            {
                                                                                if (!placedInA5PlayerEmptyPodR3)
                                                                                {
                                                                                    placedInA5PlayerEmptyPodR3 = true;
                                                                                }
                                                                                else
                                                                                {
                                                                                    if (First4TableIndex == -1)
                                                                                    {
                                                                                        // all the following tables have the same size
                                                                                        // restore the previous state
                                                                                        seating[R2_PLAYERS_SEATED_IN_POD + p]--; // number of players seated in pod p in R2
                                                                                        seating[R2_POSITION + player - 1] = 0; // used to know the positions of other players (otherwise we can still use i for the current player)
                                                                                        seating[R2_POD + player - 1] = 0; // used to know the positions of other players (otherwise we can still use P for the current player)
                                                                                        seating[index] = 0; // remove the seated player now we've returned the seating (or not)

#if SEVENTEEN || TWENTYONE
                                                                                        if (podSize == 5)
                                                                                        {
                                                                                            seating[FIVE_PLAYERS_OCCURENCE + player - 1]--;
                                                                                        }
#endif
#if EIGHTTEEN || NINETEEN || TWENTYTWO
                                                                                    if (vps == specialVp)
                                                                                    {
                                                                                        seating[PLAYERS_WITH_0_VP + specialVp + 5]--;
                                                                                    }
#endif

                                                                                        yield break;
                                                                                    }
                                                                                    p3 = First4TableIndex;
                                                                                    // index3 = PodIndices[p3];
                                                                                    --p3;
                                                                                    break; // break for (int i3 = 0; i3 < podSize3; ++i3) 
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                if (!placedInA4PlayerEmptyPodR3)
                                                                                {
                                                                                    placedInA4PlayerEmptyPodR3 = true;
                                                                                }
                                                                                else
                                                                                {
                                                                                    // all the following tables have the same size
                                                                                    // restore the previous state
                                                                                    seating[R2_PLAYERS_SEATED_IN_POD + p]--; // number of players seated in pod p in R2
                                                                                    seating[R2_POSITION + player - 1] = 0; // used to know the positions of other players (otherwise we can still use i for the current player)
                                                                                    seating[R2_POD + player - 1] = 0; // used to know the positions of other players (otherwise we can still use P for the current player)
                                                                                    seating[index] = 0; // remove the seated player now we've returned the seating (or not)
#if SEVENTEEN || TWENTYONE
                                                                                    if (podSize == 5)
                                                                                    {
                                                                                        seating[FIVE_PLAYERS_OCCURENCE + player - 1]--;
                                                                                    }
#endif
#if EIGHTTEEN || NINETEEN || TWENTYTWO
                                                                                if (vps == specialVp)
                                                                                {
                                                                                    seating[PLAYERS_WITH_0_VP + specialVp + 5]--;
                                                                                }
#endif

                                                                                    yield break;
                                                                                }
                                                                            }
                                                                        }

                                                                        if (initialPosition != i3 && i != i3) // rule 7: same starting position
                                                                        {

#if THIRTEEN
                                                                            const int EXPECTED_PLAYERS_WITH_6_TRANSFERS = 1;
#elif SEVENTEEN
                                                                            const int EXPECTED_PLAYERS_WITH_6_TRANSFERS = 2;
#elif TWELVE
                                                                            const int EXPECTED_PLAYERS_WITH_6_TRANSFERS = 3;
#endif
#if TWELVE || THIRTEEN ||SEVENTEEN
                                                                            if (transfers + i3 >= 6 || (transfers + i3 == 5 && seating[PLAYERS_WITH_6_TRANSFERS] < EXPECTED_PLAYERS_WITH_6_TRANSFERS)) // a player can't have X or less transfers, it's lame! (note that the real test is transfers + i3 + 1 >= 7
#else
                                                                            if (transfers + i3 >= 6) // a player can't have X or less transfers, it's lame! (note that the real test is transfers + i3 + 1 >= 7
#endif
                                                                            {
                                                                                var transfersR3 = transfers + Math.Min(4, i3 + 1);
                                                                                if (transfersR3 <= 9) // we limit to 9 transfers
                                                                                {
                                                                                    // rule 1. No pair of players repeat their predator-prey relationship. This is mandatory, by the VEKN rules.
                                                                                    var predator3 = seating[N + PredatorIndices[index3]];
                                                                                    int prey3;

                                                                                    // check prey-predator relationship between r1 and r3, and also r2 and r3
                                                                                    if ((predator3 == 0 // nothing to check
                                                                                        || (!IsPreyOrPredR1[player * N + predator3] // check R1-R3
                                                                                            && predator3 != predator && predator3 != prey)) // check R2-R3

                                                                                        && ((prey3 = seating[N + PreyIndices[index3]]) == 0 // nothing to check
                                                                                        || (!IsPreyOrPredR1[player * N + prey3] // check R1-R3
                                                                                            && prey3 != predator && prey3 != prey)) // check R2-R3
                                                                                        )
                                                                                    {
                                                                                        seating[absoluteIndex3] = player; // seat the player
                                                                                        if (!ViolateRule6ForTable(seating, p3, N) && !ViolateRule6ForTableR2R3(seating, p3))
                                                                                        {
                                                                                            {
#if EIGHTTEEN || NINETEEN || TWENTYTWO
                                                                                            if (vps != specialVp)
                                                                                            {
                                                                                                seating[PLAYERS_WITH_0_VP + vps3]++;
                                                                                            }
#endif

                                                                                                seating[R3_PLAYERS_SEATED_IN_POD + p3]++; // number of players seated in pod p3 in R3

                                                                                                var transfersDeviation = Math.Abs(transfersR3 * N - TotalTransfer3);
                                                                                                seating[ABSOLUTE_DEVIATION_TRANSFERS] += transfersDeviation;

                                                                                                var vpsDeviation = Math.Abs(vps3 * PodCount - TotalVp3);
                                                                                                seating[ABSOLUTE_DEVIATION_VP] += vpsDeviation;
#if TWELVE || THIRTEEN ||SEVENTEEN
                                                                                                if (transfersR3 == 6)
                                                                                                {
                                                                                                    seating[PLAYERS_WITH_6_TRANSFERS]++;
                                                                                                }
#endif
#if SEVENTEEN || TWENTYONE
                                                                                                if (podSize3 == 5)
                                                                                                {
                                                                                                    seating[FIVE_PLAYERS_OCCURENCE + player - 1]++;
                                                                                                }
#endif

#if COUNT
                                                                                                lock (this)
                                                                                                {
                                                                                                    Count++;
                                                                                                    // sanity check:
                                                                                                    //var s = new int[GetArraySize()];
                                                                                                    //Array.Copy(seating, 0, s, 0, N * 2);
                                                                                                    //InitializeSeating(s);
                                                                                                    //if (!s.SequenceEqual(seating))
                                                                                                    //{
                                                                                                    //    throw new Exception();
                                                                                                    //}
                                                                                                    //var log = string.Format("player:{0} st.Pos.:{1} p:{2} i:{3} p3:{4} i3:{5} pl.5Pl.Pod:{6} pl.4Pl.Pod:{7} init.Pos.:{8} p.Siz:{9} index:{10} tfers:{11} pred:{12} prey:{13} vps:{14} pl.5Pl.PodR3:{15} pl.4Pl.PodR3:{16} p.Siz3:{17} index3:{18} absIndex3:{19} tfersR3:{20} pred3:{21} prey3:{22} vps3:{23} tfersDev.:{24} vpsDev.:{25} [{26}]",
                                                                                                    //    player, startingPosition, p, i, p3, i3, placedInA5PlayerEmptyPod, placedInA4PlayerEmptyPod, initialPosition, podSize, index, transfers, predator, prey, vps, placedInA5PlayerEmptyPodR3, placedInA4PlayerEmptyPodR3, podSize3, index3, absoluteIndex3, transfersR3, predator3, prey3, vps3, transfersDeviation, vpsDeviation, string.Concat(seating));
                                                                                                    //_sw.WriteLine(log);
                                                                                                    //if (Count % 1000 == 0)
                                                                                                    //{
                                                                                                    //    _sw.Flush();
                                                                                                    //}
                                                                                                }

#endif
                                                                                                yield return seating; // since we reuse the same memory range, the seating is only valid NOW. If it's not used now, it must be cloned in the caller

                                                                                                // restore the previous R3 state
#if SEVENTEEN || TWENTYONE
                                                                                                if (podSize3 == 5)
                                                                                                {
                                                                                                    seating[FIVE_PLAYERS_OCCURENCE + player - 1]--;
                                                                                                }
#endif
#if TWELVE || THIRTEEN ||SEVENTEEN
                                                                                                if (transfersR3 == 6)
                                                                                                {
                                                                                                    seating[PLAYERS_WITH_6_TRANSFERS]--;
                                                                                                }
#endif
                                                                                                seating[ABSOLUTE_DEVIATION_VP] -= vpsDeviation;
                                                                                                seating[ABSOLUTE_DEVIATION_TRANSFERS] -= transfersDeviation;
                                                                                                seating[R3_PLAYERS_SEATED_IN_POD + p3]--; // number of players seated in pod p3 in R3
#if EIGHTTEEN || NINETEEN || TWENTYTWO
                                                                                            if (vps != specialVp)
                                                                                            {
                                                                                                seating[PLAYERS_WITH_0_VP + vps3]--;
                                                                                            }
#endif
                                                                                            }
                                                                                            //else // we can skip all the tables of the same size
                                                                                            //{
                                                                                            //    seating[absoluteIndex3] = 0; // remove the seated player
                                                                                            //    if (podSize3 == 4 || First4TableIndex3 == -1)
                                                                                            //    {
                                                                                            //        // all the following tables have the same size
                                                                                            //        yield break;
                                                                                            //    }
                                                                                            //    p3 = First4TableIndex3;
                                                                                            //    //index = PodIndices[p3];
                                                                                            //    --p3;
                                                                                            //    break; // break for (int i3 = 0; i3 < podSize3; ++i3) 
                                                                                            //}
                                                                                        }
                                                                                        seating[absoluteIndex3] = 0; // remove the seated player in R3 now we've returned the seating (or not)
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }

                                                    // restore the previous state
#if SEVENTEEN || TWENTYONE
                                                    if (podSize == 5)
                                                    {
                                                        seating[FIVE_PLAYERS_OCCURENCE + player - 1]--;
                                                    }
#endif
                                                    seating[R2_PLAYERS_SEATED_IN_POD + p]--; // number of players seated in pod p in R2
                                                    seating[R2_POSITION + player - 1] = 0; // used to know the positions of other players (otherwise we can still use i for the current player)
                                                    seating[R2_POD + player - 1] = 0; // used to know the positions of other players (otherwise we can still use P for the current player)
                                                }
                                                //else // we can skip all the tables of the same size
                                                //{
                                                //    seating[index] = 0; // remove the seated player
                                                //    if (podSize == 4 || First4TableIndex == -1)
                                                //    {
                                                //        // all the following tables have the same size
                                                //        yield break;
                                                //    }
                                                //    p = First4TableIndex;
                                                //    //index = PodIndices[p];
                                                //    --p;
                                                //    break; // break for (int i = 0; i < podSize; ++i) 
                                                //}
                                            }
                                            seating[index] = 0; // remove the seated player now we've returned the seating (or not)
                                        }
                                    }
                                }
                            }
                        }
                        ++index;
                    }
#if EIGHTTEEN || NINETEEN || TWENTYTWO
                    if (vps == specialVp)
                    {
                        seating[PLAYERS_WITH_0_VP + specialVp + 5]--;
                    }
#endif
                }
            }
        }

        /// <summary>
        /// 1. No pair of players repeat their predator-prey relationship. This is mandatory, by the VEKN rules.
        /// </summary>
        public Rule1Result CheckRule1(int[] r2Seating, int[] r3Seating)
        {
            var seating = GetSeating(r2Seating, r3Seating);
            for (int p = 0; p < PodCount; ++p)
            {
                var podIndex = PodIndices[p];
                for (int i = 0; i < PodSizes[p]; ++i)
                {
                    var playerIndex = podIndex + i;
                    var preyIndex = podIndex + ((i + 1) % PodSizes[p]);
                    var player = seating[N + playerIndex];
                    // check R3
                    var preyOnR3 = seating[N + preyIndex];
                    if (GetInitialRelationShip(player, preyOnR3) == RelationShip.Predator)
                    {
                        return new Rule1Result { Violated = true, Player = player, Table = p + 1, Position = i + 1, Round = 3 };
                    }
                    // check R2
                    var playerPositionOnR2 = seating[R2_POSITION + player - 1]; // 0-based index
                    var playerTableOnR2 = seating[R2_POD + player - 1]; // 0-based index
                    preyIndex = PodIndices[playerTableOnR2] + ((playerPositionOnR2 + 1) % PodSizes[playerTableOnR2]);
                    var preyOnR2 = seating[preyIndex];
                    if (GetInitialRelationShip(player, preyOnR2) == RelationShip.Predator)
                    {
                        return new Rule1Result { Violated = true, Player = player, Table = p + 1, Position = i + 1, Round = 2 };
                    }
                    // check that the prey on R2 wasn't the same as on R3
                    if (preyOnR2 == preyOnR3)
                    {
                        return new Rule1Result { Violated = true, Player = player, Table = p + 1, Position = i + 1, Round = 3 };
                    }
                }
            }
            return new Rule1Result { Violated = false };
        }

        /// <summary>
        /// 2. No pair of players share a table through all two rounds, when possible.  (N/A in some 2R event.)
        /// </summary>
        public Rule2Result CheckRule2(int[] r2Seating, int[] r3Seating)
        {
            KeyValuePair<int, List<Tuple<int, int>>>[] relations = WhoMeetsWho(r2Seating, r3Seating).OrderByDescending(kvp => kvp.Key).ToArray();

            var r2Check = base.CheckRule2(r2Seating);
            if (r2Check.Violated)
            {
                return new Rule2Result { Violated = true, Player = r2Check.Player, OtherPlayer = r2Check.OtherPlayer, Round = 2, Relations = relations };
            }

            for (int p = 0; p < PodCount; ++p)
            {
                // get the players on R3
                var tablePlayers = r3Seating.Skip(PodIndices[p]).Take(PodSizes[p]).ToArray();
                AddPlayersOnTheSameTableToThePlayersMet(tablePlayers, r2Check.PlayersMet);
            }

            for (int i = 0; i < N; ++i)
            {
                r2Check.PlayersMet[i].RemoveAll(v => v == i + 1); // remove the player from his tables
                if (r2Check.PlayersMet[i].Count != r2Check.PlayersMet[i].Distinct().Count())
                {
                    IEnumerable<int> duplicates = r2Check.PlayersMet[i].GroupBy(s => s).SelectMany(grp => grp.Skip(1)).ToArray();
                    return new Rule2Result { Violated = true, Player = i + 1, OtherPlayer = duplicates.First(), Round = 3, Relations = relations };
                }
            }
            return new Rule2Result { Violated = false, Relations = relations };
        }

        /// <summary>
        /// 3. Available VPs are equitably distributed.
        /// </summary>
        /// <param name="seating"></param>
        /// <returns></returns>
        public Rule3Result CheckRule3(int[] r2Seating, int[] r3Seating)
        {
            var vps = new int[N];
            for (int p = 0; p < PodCount; ++p)
            {
                var podIndex = PodIndices[p];
                for (var i = 0; i < PodSizes[p]; ++i)
                {
                    vps[podIndex + i] += PodSizes[p];
                    vps[r2Seating[podIndex + i] - 1] += PodSizes[p];
                    vps[r3Seating[podIndex + i] - 1] += PodSizes[p];
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
        public Rule5Result CheckRule5(int[] r2Seating, int[] r3Seating)
        {
            var fifthPlayers = new List<int>();
            for (int p = 0; p < PodCount; ++p)
            {
                if (PodSizes[p] != 5)
                {
                    continue;
                }

                var podIndex = PodIndices[p];
                var fifthPlayer = r2Seating[podIndex + 4];
                if (InitialPositions[fifthPlayer - 1] + 1 == 5 || fifthPlayers.Contains(fifthPlayer))
                {
                    return new Rule5Result { Violated = true, Player = fifthPlayer, Table = p + 1, Round = 2 };
                }
                fifthPlayers.Add(fifthPlayer);

                fifthPlayer = r3Seating[podIndex + 4];
                if (InitialPositions[fifthPlayer - 1] + 1 == 5 || fifthPlayers.Contains(fifthPlayer))
                {
                    return new Rule5Result { Violated = true, Player = fifthPlayer, Table = p + 1, Round = 3 };
                }
                fifthPlayers.Add(fifthPlayer);
            }

            // check any duplicate
            return new Rule5Result { Violated = false };
        }

        /// <summary>
        /// 6. No pair of players repeat the same relative position[*], when possible.
        /// </summary>
        /// <param name="seating"></param>
        /// <returns></returns>
        /// <remarks>Prey/predator relationship are not checked.</remarks>
        public Rule6Result CheckRule6(int[] r2Seating, int[] r3Seating)
        {
            var seating = GetSeating(r2Seating, r3Seating);
            for (int p = 0; p < PodCount; ++p)
            {
                if (ViolateRule6ForTable(r2Seating, p))
                {
                    return new Rule6Result { Violated = true, Table = p + 1, Round = 2 };
                }
                if (ViolateRule6ForTable(r3Seating, p))
                {
                    return new Rule6Result { Violated = true, Table = p + 1, Round = 3 };
                }
                if (ViolateRule6ForTableR2R3(seating, p))
                {
                    return new Rule6Result { Violated = true, Table = p + 1, Round = 3 };
                }
            }

            return new Rule6Result { Violated = false };
        }

        /// <summary>
        /// Returns a value indicating that the relationship is the same as the initial relationship for any player of a given pod in R3. Prey/predator relationship are not checked.
        /// </summary>
        /// <param name="seating">The seating.</param>
        /// <param name="p">The pod to check.</param>
        /// <returns>True if the rule is violated, false otherwise.</returns>
        internal bool ViolateRule6ForTableR2R3(int[] seating, int p)
        {
            var podIndex = PodIndices[p];

            var first = seating[N + podIndex] - 1;
            var second = seating[N + podIndex + 1] - 1;
            var third = seating[N + podIndex + 2] - 1;
            var fourth = seating[N + podIndex + 3] - 1;

            if (PodSizes[p] == 5)
            {
                if (first >= 0)
                {
                    var r2TableForFirstPlayer = seating[R2_POD + first];
                    if (PodSizes[r2TableForFirstPlayer] == 5) // if the R2 table was of a different size, the relationship couldn't be repeated (grandprey != crosstable at 4 players)
                    {
                        if (third >= 0 && seating[R2_POD + third] == r2TableForFirstPlayer) // first and third were seated at the same table in r2
                        {
                            var r2PositionForFirstPlayer = seating[R2_POSITION + first];
                            var r2PositionForThirdPlayer = seating[R2_POSITION + third];
                            if ((r2PositionForFirstPlayer + 2 % 5) == r2PositionForThirdPlayer)
                            {
                                return true;
                            }
                        }
                        if (fourth >= 0 && seating[R2_POD + fourth] == r2TableForFirstPlayer) // first and fourth were seated at the same table in r2
                        {
                            var r2PositionForFirstPlayer = seating[R2_POSITION + first];
                            var r2PositionForFourthPlayer = seating[R2_POSITION + fourth];
                            if ((r2PositionForFirstPlayer + 3 % 5) == r2PositionForFourthPlayer)
                            {
                                return true;
                            }
                        }
                    }
                }
                if (second >= 0)
                {
                    var r2TableForSecondPlayer = seating[R2_POD + second];
                    if (PodSizes[r2TableForSecondPlayer] == 5) // if the R2 table was of a different size, the relationship couldn't be repeated (grandprey != crosstable at 4 players)
                    {
                        if (fourth >= 0 && seating[R2_POD + fourth] == r2TableForSecondPlayer) // second and fourth were seated at the same table in r2
                        {
                            var r2PositionForSecondPlayer = seating[R2_POSITION + second];
                            var r2PositionForFourthPlayer = seating[R2_POSITION + fourth];
                            if ((r2PositionForSecondPlayer + 2 % 5) == r2PositionForFourthPlayer)
                            {
                                return true;
                            }
                        }
                        var fifth = seating[N + podIndex + 3] - 1;
                        if (fifth >= 0 && seating[R2_POD + fifth] == r2TableForSecondPlayer) // second and fifth were seated at the same table in r2
                        {
                            var r2PositionForSecondPlayer = seating[R2_POSITION + second];
                            var r2PositionForfifthPlayer = seating[R2_POSITION + fifth];
                            if ((r2PositionForSecondPlayer + 3 % 5) == r2PositionForfifthPlayer)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                if (first >= 0)
                {
                    var r2TableForFirstPlayer = seating[R2_POD + first];
                    if (PodSizes[r2TableForFirstPlayer] == 4) // if the R2 table was of a different size, the relationship couldn't be repeated (grandprey != crosstable at 4 players)
                    {
                        if (third >= 0 && seating[R2_POD + third] == r2TableForFirstPlayer) // first and third were seated at the same table in r2
                        {
                            var r2PositionForFirstPlayer = seating[R2_POSITION + first];
                            var r2PositionForThirdPlayer = seating[R2_POSITION + third];
                            if ((r2PositionForFirstPlayer + 2 % 5) == r2PositionForThirdPlayer)
                            {
                                return true;
                            }
                        }
                    }
                }
                if (second >= 0)
                {
                    var r2TableForSecondPlayer = seating[R2_POD + second];
                    if (PodSizes[r2TableForSecondPlayer] == 4) // if the R2 table was of a different size, the relationship couldn't be repeated (grandprey != crosstable at 4 players)
                    {
                        if (fourth >= 0 && seating[R2_POD + fourth] == r2TableForSecondPlayer) // second and fourth were seated at the same table in r2
                        {
                            var r2PositionForSecondPlayer = seating[R2_POSITION + second];
                            var r2PositionForfourthPlayer = seating[R2_POSITION + fourth];
                            if ((r2PositionForSecondPlayer + 2 % 5) == r2PositionForfourthPlayer)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 7. A player doesn't play in the same seat position, if possible.
        /// </summary>
        /// <param name="seating"></param>
        /// <returns></returns>
        public Rule7Result CheckRule7(int[] r2Seating, int[] r3Seating)
        {
            var seating = GetSeating(r2Seating, r3Seating);
            for (int p = 0; p < PodCount; ++p)
            {
                var podIndex = PodIndices[p];
                for (var i = 0; i < PodSizes[p]; ++i)
                {
                    var player = seating[podIndex + i];
                    var initialPosition = InitialPositions[player - 1];

                    if (initialPosition == i)
                    {
                        return new Rule7Result { Violated = true, Table = p + 1, Player = player, Position = i + 1, Round = 2 };
                    }

                    // r3
                    player = seating[N + podIndex + i];
                    initialPosition = InitialPositions[player - 1];
                    var r2Position = seating[R2_POSITION + player - 1];
                    if (initialPosition == i || r2Position == i)
                    {
                        return new Rule7Result { Violated = true, Table = p + 1, Player = player, Position = i + 1, Round = 3 };
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
        public Rule8Result CheckRule8(int[] r2Seating, int[] r3Seating)
        {
            var transfers = new int[N];
            for (int p = 0; p < PodCount; ++p)
            {
                var podIndex = PodIndices[p];
                for (var i = 0; i < PodSizes[p]; ++i)
                {
                    var transfer = Math.Min(4, i + 1);
                    transfers[podIndex + i] += transfer;
                    transfers[r2Seating[podIndex + i] - 1] += transfer;
                    transfers[r3Seating[podIndex + i] - 1] += transfer;
                }
            }
            var referenceTransfer = transfers[0];
            if (transfers.All(vp => vp == referenceTransfer))
            {
                return new Rule8Result { Violated = false };
            }

            return new Rule8Result { Violated = true, Transfers = transfers };
        }

        public override int GetTransferAbsoluteDeviationFast(int[] seating)
        {
            return seating[ABSOLUTE_DEVIATION_TRANSFERS];
        }

        public override int GetVpAbsoluteDeviationFast(int[] seating)
        {
            return seating[ABSOLUTE_DEVIATION_VP];
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
            public int Round;
        }

        public class Rule2Result : RuleResult
        {
            public int OtherPlayer;
            public int Round;
            public KeyValuePair<int, List<Tuple<int, int>>>[] Relations;
        }

        public class Rule3Result : RuleResult
        {
            public int[] Vps;
        }

        public class Rule5Result : RuleResult
        {
            public int Table;
            public int Round;
        }

        public class Rule6Result : RuleResult
        {
            public int Table;
            public int Round;
        }

        public class Rule7Result : RuleResult
        {
            public int Table;
            public int Position;
            public int Round;
        }

        public class Rule8Result : RuleResult
        {
            public int[] Transfers;
        }

        public override string GetSeatingText(int[] seating)
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
            stringBuilder.Append("/ ");
            for (int p = 0; p < PodCount; ++p)
            {
                var podIndex = N + PodIndices[p];
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

        public override int GetSeatingTextLength()
        {
            int[] seating = Enumerable.Range(1, N).Concat(Enumerable.Range(1, N)).ToArray();
            return GetSeatingText(seating).Length;
        }

        public override int[] GetSeatingFromText(string text)
        {
            var seatings = text.Split('/');
            if (seatings.Length != 2)
            {
                throw new Exception("Invalid input, expected two rounds separated by a '/': " + text);
            }

            var r2Seating = GetSeatingFromTextNoCheck(seatings[0]);
            if (r2Seating.Length != N)
            {
                throw new Exception("Invalid size for R2 in input: " + text);
            }
            var r3Seating = GetSeatingFromTextNoCheck(seatings[1]);
            if (r3Seating.Length != N)
            {
                throw new Exception("Invalid size for R3 in input: " + text);
            }

            var result = new int[2 * N];
            Array.Copy(r2Seating, 0, result, 0, N);
            Array.Copy(r3Seating, 0, result, N, N);
            return result;
        }

        public override double GetTransferAbsoluteDeviation(int[] seating)
        {
            var result = new int[GetArraySize()];
            Array.Copy(seating, 0, result, 0, 2 * N);
            InitializeSeating(result);
            return (double)GetTransferAbsoluteDeviationFast(result) / (N * N);
        }

        public override double GetVpAbsoluteDeviation(int[] seating)
        {
            var result = new int[GetArraySize()];
            Array.Copy(seating, 0, result, 0, seating.Length);
            InitializeSeating(result);
            return (double)GetVpAbsoluteDeviationFast(result) / (N * N);
        }

        public override int GetMeetOnceCount(int[] seatings)
        {
            var r2Seating = new int[N];
            var r3Seating = new int[N];

            Buffer.BlockCopy(seatings, 0, r2Seating, 0, N * sizeof(int));
            Buffer.BlockCopy(seatings, N * sizeof(int), r3Seating, 0, N * sizeof(int));

            var matrix = GetEncounterMatrix(r2Seating, r3Seating);
            var result = 0;
            foreach (var i in matrix)
            {
                if (i == 1)
                {
                    ++result;
                }
            }
            return result;
        }

        public Dictionary<int, List<Tuple<int, int>>> WhoMeetsWho(int[] r2Seating, int[] r3Seating)
        {
            var matrix = GetEncounterMatrix(r2Seating, r3Seating);

            var result = new Dictionary<int, List<Tuple<int, int>>>();

            for (int i = 0; i < N; ++i)
            {
                for (int j = 0; j < N; ++j)
                {
                    var count = matrix[i, j];
                    if (count == 0)
                    {
                        continue;
                    }

                    List<Tuple<int, int>> list;
                    if (!result.TryGetValue(count, out list))
                    {
                        list = new List<Tuple<int, int>>();
                        result[count] = list;
                    }

                    list.Add(new Tuple<int, int>(i + 1, j + 1));
                }
            }


            return result;
        }

        private int[,] GetEncounterMatrix(int[] r2Seating, int[] r3Seating)
        {
            var matrix = new int[N, N];

            var seating = GetSeating(r2Seating, r3Seating);
            for (int p = 0; p < PodCount; ++p)
            {
                var podIndex = PodIndices[p];
                var podIndex3 = N + podIndex;

                for (int i = 0; i < PodSizes[p] - 1; ++i)
                {
                    for (int j = i + 1; j < PodSizes[p]; ++j)
                    {
                        AddToMatrix(matrix, podIndex + i, podIndex + j);
                        AddToMatrix(matrix, seating[podIndex + i] - 1, seating[podIndex + j] - 1);
                        AddToMatrix(matrix, seating[podIndex3 + i] - 1, seating[podIndex3 + j] - 1);
                    }
                }
            }
            return matrix;
        }

        private void AddToMatrix(int[,] matrix, int v1, int v2)
        {
            if (v1 < v2)
            {
                matrix[v1, v2]++;
            }
            else
            {
                matrix[v2, v1]++;
            }
        }
    }

    public class R3SeatingsComparer : IEqualityComparer<int[]>
    {
        public int N { get; set; }

        public R3SeatingsComparer(int n)
        {
            this.N = n;
        }

        public bool Equals(int[] x, int[] y)
        {
            var permutations = GetPermutations(x.Take(N).Where(v => v > 0).ToArray()).ToArray();

            foreach (var p2 in permutations)
            {
                foreach (var p3 in permutations)
                {
                    // this comparer works only if the seatings contains only brothers
                    var firstR2 = x.Take(N).Select(v => v == 0 ? 0 : Array.IndexOf(p2, v) + 1).ToArray();
                    var firstR3 = x.Skip(N).Take(N).Select(v => v == 0 ? 0 : Array.IndexOf(p2, v) + 1).ToArray();

                    var secondR2 = y.Take(N).Select(v => v == 0 ? 0 : Array.IndexOf(p3, v) + 1).ToArray();
                    var secondR3 = y.Skip(N).Take(N).Select(v => v == 0 ? 0 : Array.IndexOf(p3, v) + 1).ToArray();

                    var result = (firstR2.SequenceEqual(secondR2) && firstR3.SequenceEqual(secondR3))
                        || (firstR2.SequenceEqual(secondR3) && firstR3.SequenceEqual(secondR2));
                    if (result)
                    {
                        return true;
                    }
                }
            }
            return false;

            //// this comparer works only if the seatings contains only brothers
            //var firstR2 = x.Take(N).Select(v => v > 0 ? 1 : 0).ToArray();
            //var firstR3 = x.Skip(N).Take(N).Select(v => v > 0 ? 1 : 0).ToArray();

            //var secondR2 = y.Take(N).Select(v => v > 0 ? 1 : 0).ToArray();
            //var secondR3 = y.Skip(N).Take(N).Select(v => v > 0 ? 1 : 0).ToArray();

            //var result = (firstR2.SequenceEqual(secondR2) && firstR3.SequenceEqual(secondR3))
            //    || (firstR2.SequenceEqual(secondR3) && firstR3.SequenceEqual(secondR2));
            //return result;

        }

        private IEnumerable<int[]> GetPermutations(int[] values)
        {
            if (values.Length == 1)
            {
                yield return values;
            }
            else
            {
                foreach (var value in values)
                {
                    foreach (var permutations in GetPermutations(values.Where(v => v != value).ToArray()))
                    {
                        yield return new[] { value }.Concat(permutations).ToArray();
                    }
                }
            }
        }

        public int GetHashCode(int[] seating)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                for (int i = 0; i < N; ++i)
                {
                    hash = hash * 23 + ((seating[i] + seating[i + N]) != 0 ? 1 : 0);
                }
                return hash;
            }
        }
    }
}
