//namespace ConsoleApplication5
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;

//    public class SeatingsFactory : SeatingsFactoryBase
//    {
//        public SeatingsFactory(int n, bool applyRule2, bool applyRule6 = false, bool applyRule7 = false)
//            : base(n, applyRule2, applyRule6, applyRule7)
//        {
//        }

//        protected override int GetArraySize()
//        {
//            return N + 1 + PodCount; // first extra int contains the last changed pod index, then the cycling of the tables
//        }

//        protected override void InitializeSeating(int[] seating)
//        {
//        }

//        public override IEnumerable<int[]> GetSeatings(int player, IEnumerable<int[]> seatings, int lastPlayer)
//        {
//            // TODO
//            return null;
//        }

//        public override IEnumerable<int[]> GetSeatings(int player, IEnumerable<int[]> seatings)
//        {
//            foreach (var newSeatings in seatings.Select(seating => GetSeatings(player, seating)))
//            {
//                foreach (var newSeating in newSeatings)
//                {
//                    if (!_applyRule2 || !ViolateRule2(newSeating))
//                    {
//                        if (!_applyRule6 || !ViolateRule6(newSeating))
//                        {
//                            if (!_applyRule7 || !ViolateRule78(newSeating))
//                            {
//                                //if (!ViolateRule8(newSeating))
//                                {
//                                    yield return newSeating;
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }

//        protected internal IEnumerable<int[]> GetSeatings(int player, int[] seating)
//        {
//            var podIndex = 0;

//            while (IsNonEmptyPod(seating, podIndex))
//            {
//                // we are in a non-empty pod
//                int currentPodSize = PodSizes[podIndex];
//                int currentPodIndex = PodIndices[podIndex];


//                for (var p = 1; p < currentPodSize; ++p) // non-empty pod => the first seat is already occupied
//                {
//                    var absoluteP = currentPodIndex + p;
//                    if (seating[absoluteP] == 0) // empty seat
//                    {
//                        // rule 1. No pair of players repeat their predator-prey relationship. This is mandatory, by the VEKN rules.
//                        //int predatorOnFirstRound = GetPredatorOnFirstRound(player);
//                        //var preyOnFirstRound = GetPreyOnFirstRound(player);

//                        var predator = seating[PredatorIndices[absoluteP]];
//                        int prey;

//                        //if((predator == 0 || (predator != predatorOnFirstRound && predator != preyOnFirstRound))
//                        //    && ((prey = GetPrey(p, podIndex, seating)) == 0 || (prey != predatorOnFirstRound && prey != preyOnFirstRound)))
//                        if ((predator == 0 || !IsPreyOrPredR1[player * N + predator])
//                            && ((prey = seating[PreyIndices[absoluteP]]) == 0 || !IsPreyOrPredR1[player * N + prey]))
//                        {
//                            var newSeating = new int[SeatingArraySize];
//                            Buffer.BlockCopy(seating, 0, newSeating, 0, SeatingArraySize * sizeof(int));
//                            newSeating[absoluteP] = player; // seat the player
//                            newSeating[N] = podIndex; // mark the pod as changed
//                            yield return newSeating;
//                        }
//                    }
//                }
//                podIndex++;
//                if (podIndex == PodCount)
//                {
//                    yield break;
//                }
//            }


//            // empty pods: seat the player on the first seat of the first empty pod (other non-empty pod are similar) of a given size
//            if (podIndex < PodCount)
//            {
//                var newSeating = new int[SeatingArraySize];
//                Buffer.BlockCopy(seating, 0, newSeating, 0, SeatingArraySize * sizeof(int));
//                newSeating[PodIndices[podIndex]] = player; // seat the player at the first seat
//                newSeating[N] = podIndex; // mark the pod as changed
//                yield return newSeating;
//            }

//            if (N % 5 != 0 && PodSizes[podIndex] == 5)
//            {
//                // we try to seath the player on the first seat of the first empty pod of size 4
//                while (++podIndex < PodCount && (PodSizes[podIndex] == 5 || IsNonEmptyPod(seating, podIndex))) { }

//                if (podIndex < PodCount && (PodSizes[podIndex] == 4 && !IsNonEmptyPod(seating, podIndex)))
//                {
//                    var newSeating = new int[SeatingArraySize];
//                    Buffer.BlockCopy(seating, 0, newSeating, 0, SeatingArraySize * sizeof(int));
//                    newSeating[PodIndices[podIndex]] = player; // seat the player at the first seat
//                    newSeating[N] = podIndex; // mark the pod as changed
//                    yield return newSeating;
//                }
//            }
//        }
//    }
//}
