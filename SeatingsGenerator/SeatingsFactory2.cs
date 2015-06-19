namespace ConsoleApplication5
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class SeatingsFactory2 : SeatingsFactoryBase
    {
        protected int TotalTransfer2; // for two round, for all players. Must be divided by the number of players
        protected int TotalVp2;       // for two round, for all tables. Must be divided by the number of tables.
        protected internal int[] ExpectedNumberOfPlayerWithVps;

        public SeatingsFactory2(int n)
            : base(n)
        {
            TotalTransfer2 = TotalTransfer * 2;
            TotalVp2 = TotalVp * 2;
            ExactTransfer = (decimal)TotalTransfer / N * 2;
            ExactVp = (decimal)TotalVp / PodCount * 2;

            var fivePlayersTables = Enumerable.Range(1, PodCount).Count(p => PodSizes[p - 1] == 5);
            var fourPlayersTables = Enumerable.Range(1, PodCount).Count(p => PodSizes[p - 1] == 4);
            ExpectedNumberOfPlayerWithVps = new int[11];
            if (fivePlayersTables == 0)
            {
                ExpectedNumberOfPlayerWithVps[8] = N;
            }
            else if (fourPlayersTables == 0)
            {
                ExpectedNumberOfPlayerWithVps[10] = N;
            }
            else
            {
                if (fivePlayersTables > fourPlayersTables)
                {
                    ExpectedNumberOfPlayerWithVps[9] = fourPlayersTables * 4 * 2;
                    ExpectedNumberOfPlayerWithVps[10] = N - ExpectedNumberOfPlayerWithVps[9];
                }
                else if (fivePlayersTables < fourPlayersTables)
                {
                    ExpectedNumberOfPlayerWithVps[9] = fivePlayersTables * 5 * 2;
                    ExpectedNumberOfPlayerWithVps[8] = N - ExpectedNumberOfPlayerWithVps[9];
                }
                else
                {
                    ExpectedNumberOfPlayerWithVps[9] = 4 * (fourPlayersTables + fivePlayersTables);
                    ExpectedNumberOfPlayerWithVps[10] = fivePlayersTables;

                }
            }
        }

        protected override int GetArraySize()
        {
            return N + 5 + PodCount;
            // N  : the current absolute deviation for transfers
            // N+1: the current absolute deviation for vps
            // N+2: number of players with 8 vp
            // N+3: number of players with 9 vp
            // N+4: number of players with 10 vp
            // N+5+i: number of players seated in pod i (0-based index)
        }

        protected override void InitializeSeating(int[] seating)
        {
            var currentIndex = 0;
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
                    var initialPosition = InitialPositions[player - 1];
                    var transfers = Math.Min(4, initialPosition + 1) + Math.Min(4, i + 1);
                    var vps = PodSizes[InitialPod[player - 1]] + podSize;

                    seating[N + vps - 6]++;

                    var transfersDeviation = Math.Abs(transfers * N - TotalTransfer2);
                    seating[N] += transfersDeviation;

                    var vpsDeviation = Math.Abs(vps * PodCount - TotalVp2);
                    seating[N + 1] += vpsDeviation;

                    seating[N + 5 + p]++; // number of seated players in pod p
                }
            }
        }

        public override int[][] GetSeatings(int player, IEnumerable<int[]> seatings, int lastPlayer)
        {
            return seatings
                .SelectMany(seating => GetSeatings(player, seating, lastPlayer))
                .Select(seating =>
                {
                    // we clone the seating because it's only valid until we iterate to the next item
                    var newSeating = new int[SeatingArraySize];
                    Buffer.BlockCopy(seating, 0, newSeating, 0, SeatingArraySize * sizeof(int));
                    return newSeating;
                })
                .ToArray();
        }

        public override IEnumerable<int[]> GetSeatings(int player, IEnumerable<int[]> seatings)
        {
            return seatings
                .SelectMany(seating => GetSeatings(player, seating, 0, 0));
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

        private IEnumerable<int[]> GetSeatings(int player, int[] seating, int p, int startingPosition)
        {
            bool placedInA5PlayerEmptyPod = false;
            bool placedInA4PlayerEmptyPod = false;
            var initialPosition = InitialPositions[player - 1];

            for (; p < PodCount; ++p)
            {
                var podSize = PodSizes[p];
                for (int i = startingPosition; i < podSize; ++i)
                //for (int i = podSize - 1; i < podSize; ++i)
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
                        //if (i == podSize - 1 && seating[index - 1] == 0 && seating[index - 2] == 0 && seating[index - 3] == 0 && (podSize == 4 || seating[index - 4] == 0)) // look if the pod is empty
                        if (i == startingPosition && seating[N + 5 + p] == 0) // look if the pod is empty
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
                                    yield break;
                                }
                            }
                        }

                        if (initialPosition != i) // rule 7: same starting position
                        {
                            if (initialPosition + i >= 3) // a player can't have X or less transfers, it's lame! (note that the real test is initialPosition + 1 + player + 1 >= 5
                            {
                                var transfers = Math.Min(4, initialPosition + 1) + Math.Min(4, i + 1);
                                if (transfers <= 7) // we limit to 7 transfers
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
                                            var vps = PodSizes[InitialPod[player - 1]] + podSize;
                                            if (seating[N + vps - 6] < ExpectedNumberOfPlayerWithVps[vps]) // -8+2=-6; we expect no more than x players to have that amount of vps
                                            {
                                                seating[N + vps - 6]++;
                                                seating[N + 5 + p]++; // number of players seated in pod p

                                                var transfersDeviation = Math.Abs(transfers * N - TotalTransfer2);
                                                seating[N] += transfersDeviation;

                                                var vpsDeviation = Math.Abs(vps * PodCount - TotalVp2);
                                                seating[N + 1] += vpsDeviation;

#if COUNT
                                                lock (this)
                                                {
                                                    Count++;
                                                }
#endif
                                                yield return seating; // since we reuse the same memory range, the seating is only valid NOW. If it's not used now, it must be cloned in the caller

                                                // restore the previous state
                                                seating[N + 1] -= vpsDeviation;
                                                seating[N] -= transfersDeviation;
                                                seating[N + 5 + p]--; // number of players seated in pod p
                                                seating[N + vps - 6]--;
                                            }
                                            else // we can skip all the tables of the same size
                                            {
                                                seating[index] = 0; // remove the seated player
                                                if (podSize == 4 || First4TableIndex == -1)
                                                {
                                                    // all the following tables have the same size
                                                    yield break;
                                                }
                                                p = First4TableIndex;
                                                //index = PodIndices[p];
                                                --p;
                                                break; // break for (int i = 0; i < podSize; ++i) 
                                            }
                                        }
                                        seating[index] = 0; // remove the seated player now we've returned the seating (or not)
                                    }
                                }
                            }
                        }
                    }
                    ++index;
                }
            }
        }

        //public override int GetTransferAbsoluteDeviationFast(int[] seating)
        //{
        //    int result = 0;
        //    foreach (var v in GetTransfers(seating))
        //    {
        //        result += Math.Abs(v * N - TotalTransfer2);
        //    }
        //    return result;
        //}

        //public override int GetVpAbsoluteDeviationFast(int[] seating)
        //{
        //    int result = 0;
        //    foreach (int v in GetVps(seating))
        //    {
        //        result += Math.Abs(v * PodCount - TotalVp2);
        //    }
        //    return result;
        //}

        public override int GetTransferAbsoluteDeviationFast(int[] seating)
        {
            return seating[N];
        }

        public override int GetVpAbsoluteDeviationFast(int[] seating)
        {
            return seating[N + 1];
        }

        public override int[] GetCycledSeating(int[] seating)
        {
            throw new Exception("This factory doesn't need any cycling.");
        }

        public override int GetMeetOnceCount(int[] seatings)
        {
            return 0; // todo
        }
    }
}
