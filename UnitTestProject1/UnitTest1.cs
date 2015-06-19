
namespace UnitTestProject1
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using System.Collections.Generic;
    using System.Linq;

    using ConsoleApplication5;

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Rule2Test()
        {
            var factory = new SeatingsFactory2(25);

            var seating = new int[] { 1, 0, 0, 0, 0, 2, 0, 3, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* last table modified */ 1 };
            Assert.IsTrue(factory.ViolateRule2(seating));

            factory = new SeatingsFactory2(16);
            seating = new int[] { 1, 10, 7, 14, 2, 15, 12, 8, 3, 11, 5, 13, 4, 9, 6, 16, /* last table modified */ 0 };
            Assert.IsFalse(factory.ViolateRule2(seating));
            seating = new int[] { 1, 10, 7, 14, 2, 15, 12, 8, 3, 11, 5, 13, 4, 9, 6, 16, /* last table modified */ 1 };
            Assert.IsFalse(factory.ViolateRule2(seating));
            seating = new int[] { 1, 10, 7, 14, 2, 15, 12, 8, 3, 11, 5, 13, 4, 9, 6, 16, /* last table modified */ 2 };
            Assert.IsFalse(factory.ViolateRule2(seating));
            seating = new int[] { 1, 10, 7, 14, 2, 15, 12, 8, 3, 11, 5, 13, 4, 9, 6, 16, /* last table modified */ 3 };
            Assert.IsFalse(factory.ViolateRule2(seating));
        }

        [TestMethod]
        public void Rule7Test()
        {
            var factory = new SeatingsFactory2(5);

            var seating = new int[] { 1, 2, 3, 4, 5 };
            int[] newSeating;
            Assert.IsTrue(factory.ViolateRule7(seating, true, out newSeating));
            Assert.IsNotNull(newSeating);

            Assert.IsFalse(factory.ViolateRule7(seating, false, out newSeating));

            var newSeatings = factory.ApplyRule7(new[] { seating }).ToArray();
            Assert.AreEqual(1, newSeatings.Count());
            AssertSequence.SequenceEqual(new int[] { 2, 3, 4, 5, 1 }, newSeatings[0].ToArray());

            seating = newSeatings[0];
            Assert.IsFalse(factory.ViolateRule7(seating, true, out newSeating));
            Assert.IsFalse(factory.ViolateRule7(seating, false, out newSeating));

            factory = new SeatingsFactory2(16);
            seating = new int[] { 1, 12, 14, 5, 2, 13, 7, 11, 3, 15, 10, 6, 4, 8, 16, 9 };

            Assert.IsTrue(factory.ViolateRule7(seating, true, out newSeating));
            Assert.IsNull(newSeating);
            Assert.IsTrue(factory.ViolateRule7(seating, false, out newSeating));

            newSeatings = factory.ApplyRule7(new[] { seating }).ToArray();
            Assert.AreEqual(0, newSeatings.Count());
        }

        [TestMethod]
        public void GetInitialRelationShipTest()
        {
            var factory = new SeatingsFactory2(18);

            for (int i = 0; i < 10; i += 5)
            {
                Assert.AreEqual(RelationShip.Predator, factory.GetInitialRelationShip(i + 1, i + 2));
                Assert.AreEqual(RelationShip.GrandPredator, factory.GetInitialRelationShip(i + 1, i + 3));
                Assert.AreEqual(RelationShip.LittlePrey, factory.GetInitialRelationShip(i + 1, i + 4));
                Assert.AreEqual(RelationShip.Prey, factory.GetInitialRelationShip(i + 1, i + 5));

                Assert.AreEqual(RelationShip.Predator, factory.GetInitialRelationShip(i + 2, i + 3));
                Assert.AreEqual(RelationShip.GrandPredator, factory.GetInitialRelationShip(i + 2, i + 4));
                Assert.AreEqual(RelationShip.LittlePrey, factory.GetInitialRelationShip(i + 2, i + 5));
                Assert.AreEqual(RelationShip.Prey, factory.GetInitialRelationShip(i + 2, i + 1));

                Assert.AreEqual(RelationShip.Predator, factory.GetInitialRelationShip(i + 3, i + 4));
                Assert.AreEqual(RelationShip.GrandPredator, factory.GetInitialRelationShip(i + 3, i + 5));
                Assert.AreEqual(RelationShip.LittlePrey, factory.GetInitialRelationShip(i + 3, i + 1));
                Assert.AreEqual(RelationShip.Prey, factory.GetInitialRelationShip(i + 3, i + 2));

                Assert.AreEqual(RelationShip.Predator, factory.GetInitialRelationShip(i + 4, i + 5));
                Assert.AreEqual(RelationShip.GrandPredator, factory.GetInitialRelationShip(i + 4, i + 1));
                Assert.AreEqual(RelationShip.LittlePrey, factory.GetInitialRelationShip(i + 4, i + 2));
                Assert.AreEqual(RelationShip.Prey, factory.GetInitialRelationShip(i + 4, i + 3));

                Assert.AreEqual(RelationShip.Predator, factory.GetInitialRelationShip(i + 5, i + 1));
                Assert.AreEqual(RelationShip.GrandPredator, factory.GetInitialRelationShip(i + 5, i + 2));
                Assert.AreEqual(RelationShip.LittlePrey, factory.GetInitialRelationShip(i + 5, i + 3));
                Assert.AreEqual(RelationShip.Prey, factory.GetInitialRelationShip(i + 5, i + 4));
            }

            for (int i = 10; i < 18; i += 4)
            {
                Assert.AreEqual(RelationShip.Predator, factory.GetInitialRelationShip(i + 1, i + 2));
                Assert.AreEqual(RelationShip.CrossTable, factory.GetInitialRelationShip(i + 1, i + 3));
                Assert.AreEqual(RelationShip.Prey, factory.GetInitialRelationShip(i + 1, i + 4));

                Assert.AreEqual(RelationShip.Predator, factory.GetInitialRelationShip(i + 2, i + 3));
                Assert.AreEqual(RelationShip.CrossTable, factory.GetInitialRelationShip(i + 2, i + 4));
                Assert.AreEqual(RelationShip.Prey, factory.GetInitialRelationShip(i + 2, i + 1));

                Assert.AreEqual(RelationShip.Predator, factory.GetInitialRelationShip(i + 3, i + 4));
                Assert.AreEqual(RelationShip.CrossTable, factory.GetInitialRelationShip(i + 3, i + 1));
                Assert.AreEqual(RelationShip.Prey, factory.GetInitialRelationShip(i + 3, i + 2));

                Assert.AreEqual(RelationShip.Predator, factory.GetInitialRelationShip(i + 4, i + 1));
                Assert.AreEqual(RelationShip.CrossTable, factory.GetInitialRelationShip(i + 4, i + 2));
                Assert.AreEqual(RelationShip.Prey, factory.GetInitialRelationShip(i + 4, i + 3));
            }

            Assert.AreEqual(RelationShip.None, factory.GetInitialRelationShip(1, 6));
            Assert.AreEqual(RelationShip.None, factory.GetInitialRelationShip(1, 7));
            Assert.AreEqual(RelationShip.None, factory.GetInitialRelationShip(1, 8));
            Assert.AreEqual(RelationShip.None, factory.GetInitialRelationShip(1, 9));
            Assert.AreEqual(RelationShip.None, factory.GetInitialRelationShip(1, 10));
            Assert.AreEqual(RelationShip.None, factory.GetInitialRelationShip(5, 6));
            Assert.AreEqual(RelationShip.None, factory.GetInitialRelationShip(10, 11));
            Assert.AreEqual(RelationShip.None, factory.GetInitialRelationShip(14, 15));

            factory = new SeatingsFactory2(15);
            Assert.AreEqual(RelationShip.None, factory.GetInitialRelationShip(14, 10));
        }

        [TestMethod]
        public void GetPredatorOnFirstRoundTest()
        {
            var factory = new SeatingsFactory2(14);
            Assert.AreEqual(5, factory.GetPredatorOnFirstRound(1));
            Assert.AreEqual(1, factory.GetPredatorOnFirstRound(2));
            Assert.AreEqual(2, factory.GetPredatorOnFirstRound(3));
            Assert.AreEqual(3, factory.GetPredatorOnFirstRound(4));
            Assert.AreEqual(4, factory.GetPredatorOnFirstRound(5));

            Assert.AreEqual(10, factory.GetPredatorOnFirstRound(6));
            Assert.AreEqual(6, factory.GetPredatorOnFirstRound(7));
            Assert.AreEqual(7, factory.GetPredatorOnFirstRound(8));
            Assert.AreEqual(8, factory.GetPredatorOnFirstRound(9));
            Assert.AreEqual(9, factory.GetPredatorOnFirstRound(10));

            Assert.AreEqual(14, factory.GetPredatorOnFirstRound(11));
            Assert.AreEqual(11, factory.GetPredatorOnFirstRound(12));
            Assert.AreEqual(12, factory.GetPredatorOnFirstRound(13));
            Assert.AreEqual(13, factory.GetPredatorOnFirstRound(14));
        }

        [TestMethod]
        public void GetPreyOnFirstRoundTest()
        {
            var factory = new SeatingsFactory2(14);
            Assert.AreEqual(2, factory.GetPreyOnFirstRound(1));
            Assert.AreEqual(3, factory.GetPreyOnFirstRound(2));
            Assert.AreEqual(4, factory.GetPreyOnFirstRound(3));
            Assert.AreEqual(5, factory.GetPreyOnFirstRound(4));
            Assert.AreEqual(1, factory.GetPreyOnFirstRound(5));

            Assert.AreEqual(7, factory.GetPreyOnFirstRound(6));
            Assert.AreEqual(8, factory.GetPreyOnFirstRound(7));
            Assert.AreEqual(9, factory.GetPreyOnFirstRound(8));
            Assert.AreEqual(10, factory.GetPreyOnFirstRound(9));
            Assert.AreEqual(6, factory.GetPreyOnFirstRound(10));

            Assert.AreEqual(12, factory.GetPreyOnFirstRound(11));
            Assert.AreEqual(13, factory.GetPreyOnFirstRound(12));
            Assert.AreEqual(14, factory.GetPreyOnFirstRound(13));
            Assert.AreEqual(11, factory.GetPreyOnFirstRound(14));
        }

        [TestMethod]
        public void GetPredatorTest()
        {
            var factory = new SeatingsFactory2(14);
            var seating = new int[] { 5, 4, 3, 2, 1, 10, 9, 8, 7, 6, 14, 13, 12, 11 };

            Assert.AreEqual(1, seating[factory.PredatorIndices[0]]);
            Assert.AreEqual(5, seating[factory.PredatorIndices[1]]);
            Assert.AreEqual(4, seating[factory.PredatorIndices[2]]);
            Assert.AreEqual(3, seating[factory.PredatorIndices[3]]);
            Assert.AreEqual(2, seating[factory.PredatorIndices[4]]);

            Assert.AreEqual(6, seating[factory.PredatorIndices[5]]);
            Assert.AreEqual(10, seating[factory.PredatorIndices[6]]);
            Assert.AreEqual(9, seating[factory.PredatorIndices[7]]);
            Assert.AreEqual(8, seating[factory.PredatorIndices[8]]);
            Assert.AreEqual(7, seating[factory.PredatorIndices[9]]);

            Assert.AreEqual(11, seating[factory.PredatorIndices[10]]);
            Assert.AreEqual(14, seating[factory.PredatorIndices[11]]);
            Assert.AreEqual(13, seating[factory.PredatorIndices[12]]);
            Assert.AreEqual(12, seating[factory.PredatorIndices[13]]);
        }

        [TestMethod]
        public void GetPreyTest()
        {
            var factory = new SeatingsFactory2(14);
            var seating = new int[] { 5, 4, 3, 2, 1, 10, 9, 8, 7, 6, 14, 13, 12, 11 };

            Assert.AreEqual(4, seating[factory.PreyIndices[0]]);
            Assert.AreEqual(3, seating[factory.PreyIndices[1]]);
            Assert.AreEqual(2, seating[factory.PreyIndices[2]]);
            Assert.AreEqual(1, seating[factory.PreyIndices[3]]);
            Assert.AreEqual(5, seating[factory.PreyIndices[4]]);

            Assert.AreEqual(9, seating[factory.PreyIndices[5]]);
            Assert.AreEqual(8, seating[factory.PreyIndices[6]]);
            Assert.AreEqual(7, seating[factory.PreyIndices[7]]);
            Assert.AreEqual(6, seating[factory.PreyIndices[8]]);
            Assert.AreEqual(10, seating[factory.PreyIndices[9]]);

            Assert.AreEqual(13, seating[factory.PreyIndices[10]]);
            Assert.AreEqual(12, seating[factory.PreyIndices[11]]);
            Assert.AreEqual(11, seating[factory.PreyIndices[12]]);
            Assert.AreEqual(14, seating[factory.PreyIndices[13]]);
        }

        //[TestMethod]
        //public void Rule5Test()
        //{
        //    var factory = new SeatingsFactory2(10);

        //    var seating = new int[] { 2, 3, 4, 5, 1, 7, 8, 9, 10, 6 };
        //    Assert.IsFalse(factory.ViolateRule5(seating));

        //    seating = new int[] { 2, 3, 4, 5, 10, 7, 8, 9, 1, 6 };
        //    Assert.IsTrue(factory.ViolateRule5(seating));

        //    seating = new int[] { 2, 3, 4, 10, 1, 7, 8, 9, 6, 5 };
        //    Assert.IsTrue(factory.ViolateRule5(seating));

        //    seating = new int[] { 2, 3, 4, 1, 10, 7, 8, 9, 6, 5 };
        //    Assert.IsTrue(factory.ViolateRule5(seating));
        //}

        //[TestMethod]
        //public void Rule7Test()
        //{
        //    var factory = new SeatingsFactory2(9);

        //    var seating = new int[] { 2, 3, 4, 5, 1, 7, 8, 9, 6 };
        //    Assert.IsFalse(factory.ViolateRule7(seating));

        //    seating = new int[] { 2, 3, 4, 1, 5, 7, 8, 9, 6 };
        //    Assert.IsTrue(factory.ViolateRule7(seating));

        //    seating = new int[] { 2, 3, 4, 5, 1, 7, 8, 6, 9 };
        //    Assert.IsTrue(factory.ViolateRule7(seating));

        //    seating = new int[] { 8, 9, 1, 2, 3, 6, 5, 4, 7 };
        //    Assert.IsTrue(factory.ViolateRule7(seating));
        //}

        [TestMethod]
        public void GetTransfersTest()
        {
            var seatings = new int[] { 3, 7, 5, 1, 4, 8, 6, 2, 11, 15, 13, 9, 12, 16, 14, 10 };
            var factory = new SeatingsFactory2(16);

            var expectedTransfers = new[] { 5, 6, 4, 5, 4, 5, 5, 6, 5, 6, 4, 5, 4, 5, 5, 6 };

            var tranfers = factory.GetTransfers(seatings);
            AssertSequence.SequenceEqual(expectedTransfers, tranfers);
        }

        [TestMethod]
        public void GetVpsTest()
        {
            var seatings = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            var factory = new SeatingsFactory2(9);

            var expectedVps = new[] { 9, 9, 9, 9, 10, 9, 9, 9, 9 };

            var vps = factory.GetVps(seatings);
            AssertSequence.SequenceEqual(expectedVps, vps);

            seatings = new int[] { 2, 8, 7, 6, 5, 4, 3, 9, 1 };
            expectedVps = new[] { 9, 10, 9, 9, 10, 9, 9, 9, 8 };

            vps = factory.GetVps(seatings);
            AssertSequence.SequenceEqual(expectedVps, vps);
        }

        [TestMethod]
        public void ExpectedNumberOfPlayerWithVpsText()
        {
            var factory = new SeatingsFactory2(10);
            Assert.AreEqual(0, factory.ExpectedNumberOfPlayerWithVps[8]);
            Assert.AreEqual(0, factory.ExpectedNumberOfPlayerWithVps[9]);
            Assert.AreEqual(10, factory.ExpectedNumberOfPlayerWithVps[10]);

            factory = new SeatingsFactory2(8);
            Assert.AreEqual(8, factory.ExpectedNumberOfPlayerWithVps[8]);
            Assert.AreEqual(0, factory.ExpectedNumberOfPlayerWithVps[9]);
            Assert.AreEqual(0, factory.ExpectedNumberOfPlayerWithVps[10]);

            factory = new SeatingsFactory2(9);
            Assert.AreEqual(0, factory.ExpectedNumberOfPlayerWithVps[8]);
            Assert.AreEqual(8, factory.ExpectedNumberOfPlayerWithVps[9]);
            Assert.AreEqual(1, factory.ExpectedNumberOfPlayerWithVps[10]);

            factory = new SeatingsFactory2(12);
            Assert.AreEqual(12, factory.ExpectedNumberOfPlayerWithVps[8]);
            Assert.AreEqual(0, factory.ExpectedNumberOfPlayerWithVps[9]);
            Assert.AreEqual(0, factory.ExpectedNumberOfPlayerWithVps[10]);

            factory = new SeatingsFactory2(13);
            Assert.AreEqual(3, factory.ExpectedNumberOfPlayerWithVps[8]);
            Assert.AreEqual(10, factory.ExpectedNumberOfPlayerWithVps[9]);
            Assert.AreEqual(0, factory.ExpectedNumberOfPlayerWithVps[10]);

            factory = new SeatingsFactory2(14);
            Assert.AreEqual(0, factory.ExpectedNumberOfPlayerWithVps[8]);
            Assert.AreEqual(8, factory.ExpectedNumberOfPlayerWithVps[9]);
            Assert.AreEqual(6, factory.ExpectedNumberOfPlayerWithVps[10]);
        }

        [TestMethod]
        public void ViolateRule6For5PlayersTableTest()
        {
            var factory = new SeatingsFactory2(15);
            var seatings = new[] { 14, 0, 0, 0, 0 };

            for (int i = 0; i <= seatings.Length; ++i)
            {
                Assert.IsFalse(factory.ViolateRule6ForTable(seatings, 0));
                seatings = ArrayHelper.ShiftRight(seatings, 1);
            }

            seatings = new[] { 1, 0, 3, 0, 0 };

            for (int i = 0; i <= seatings.Length; ++i)
            {
                Assert.IsTrue(factory.ViolateRule6ForTable(seatings, 0));
                seatings = ArrayHelper.ShiftRight(seatings, 1);
            }

            seatings = new[] { 1, 0, 0, 3, 0 };

            for (int i = 0; i <= seatings.Length; ++i)
            {
                Assert.IsFalse(factory.ViolateRule6ForTable(seatings, 0));
                seatings = ArrayHelper.ShiftRight(seatings, 1);
            }

            seatings = new[] { 1, 0, 4, 0, 0 };

            for (int i = 0; i <= seatings.Length; ++i)
            {
                Assert.IsFalse(factory.ViolateRule6ForTable(seatings, 0));
                seatings = ArrayHelper.ShiftRight(seatings, 1);
            }

            seatings = new[] { 1, 0, 0, 4, 0 };

            for (int i = 0; i <= seatings.Length; ++i)
            {
                Assert.IsTrue(factory.ViolateRule6ForTable(seatings, 0));
                seatings = ArrayHelper.ShiftRight(seatings, 1);
            }
        }

        [TestMethod]
        public void ViolateRule6For4PlayersTableTest()
        {
            var factory = new SeatingsFactory2(16);
            var seatings = new[] { 14, 0, 0, 0 };

            for (int i = 0; i <= seatings.Length; ++i)
            {
                Assert.IsFalse(factory.ViolateRule6ForTable(seatings, 0));
                seatings = ArrayHelper.ShiftRight(seatings, 1);
            }

            seatings = new[] { 1, 0, 3, 0 };

            for (int i = 0; i <= seatings.Length; ++i)
            {
                Assert.IsTrue(factory.ViolateRule6ForTable(seatings, 0));
                seatings = ArrayHelper.ShiftRight(seatings, 1);
            }

            seatings = new[] { 1, 0, 0, 3, };

            for (int i = 0; i <= seatings.Length; ++i)
            {
                Assert.IsFalse(factory.ViolateRule6ForTable(seatings, 0));
                seatings = ArrayHelper.ShiftRight(seatings, 1);
            }

            seatings = new[] { 1, 0, 4, 0 };

            for (int i = 0; i <= seatings.Length; ++i)
            {
                Assert.IsFalse(factory.ViolateRule6ForTable(seatings, 0));
                seatings = ArrayHelper.ShiftRight(seatings, 1);
            }

            // our rule 6 doesn't check prey-predator relationship (it's rule 2)
            //seatings = new[] { 1, 0, 0, 4 };

            //for (int i = 0; i <= seatings.Length; ++i)
            //{
            //    Assert.IsTrue(factory.ViolateRule6ForTable(seatings, 0));
            //    seatings = ArrayHelper.ShiftRight(seatings, 1);
            //}
        }
    }

    public static class ArrayHelper
    {
        public static int MathMod(int a, int b)
        {
            int c = ((a % b) + b) % b;
            return c;
        }

        public static T[] ShiftRight<T>(IList<T> values, int shift)
        {
            return values.Select((t, index) => values[MathMod(index - shift, values.Count)]).ToArray();
        }
    }

    public static class AssertSequence
    {
        public static void SequenceEqual<T>(IList<T> expected, IList<T> result)
        {
            Assert.AreEqual(expected.Count(), result.Count());
            for (int i = 0; i < expected.Count(); ++i)
            {
                Assert.AreEqual(expected[i], result[i], "At index " + i);
            }
        }

        public static void SequenceEqual<T>(IList<T> expected, IList<T> result, int index)
        {
            Assert.IsTrue(result.Count() - index >= expected.Count());
            for (int i = 0; i < expected.Count(); ++i)
            {
                Assert.AreEqual(expected[i], result[i + index], "At index " + i);
            }
        }
    }
}
