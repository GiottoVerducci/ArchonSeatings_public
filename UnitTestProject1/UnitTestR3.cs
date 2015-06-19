using System.Linq;

namespace UnitTestProject1
{
    using System;
    using System.Text;

    using ConsoleApplication5;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PermutationsHelperTest
    {
        [TestMethod]
        public void GetPermutationsTest()
        {
            var permutations = PermutationsHelper.GetPermutations(new[]{1, 2, 3}).ToArray();
            Assert.AreEqual(6, permutations.Length);
            Assert.IsTrue(permutations[0].SequenceEqual(new [] { 1, 2, 3 }));
            Assert.IsTrue(permutations[1].SequenceEqual(new [] { 1, 3, 2 }));
            Assert.IsTrue(permutations[2].SequenceEqual(new [] { 2, 1, 3 }));
            Assert.IsTrue(permutations[3].SequenceEqual(new [] { 2, 3, 1 }));
            Assert.IsTrue(permutations[4].SequenceEqual(new [] { 3, 1, 2 }));
            Assert.IsTrue(permutations[5].SequenceEqual(new [] { 3, 2, 1 }));
        }
    }

    [TestClass]
    public class UnitTestR3
    {
        [TestMethod]
        public void InitializeSeatingTest()
        {
            var factory = new Seatings3RFactory(10);
            var r2Seating = new[] { 10, 1, 9, 8, 2, 5, 6, 4, 3, 7 };
            var r3Seating = new[] { 4, 7, 10, 3, 1, 9, 2, 5, 8, 6 };

            var seating = factory.GetSeating(r2Seating, r3Seating);

            AssertSequence.SequenceEqual(r2Seating, seating, 0);
            AssertSequence.SequenceEqual(r3Seating, seating, 10);

            Assert.AreEqual(14 * 2 * 3, factory.TotalTransfer3);

            var transfersForR1 = new[] { 1, 2, 3, 4, 4, 1, 2, 3, 4, 4 };
            var transfersForR2 = new[] { 2, 4, 4, 3, 1, 2, 4, 4, 3, 1 };
            var transfersForR3 = new[] { 4, 2, 4, 1, 3, 4, 2, 4, 1, 3 };

            Assert.AreEqual((decimal)14 * 2 * 3 / 10, factory.ExactTransfer);
            //decimal exactAbsoluteDeviation = 0;
            //for (int i = 0; i < 10; ++i)
            //{
            //    var transferR1 = transfersForR1[i];
            //    var transferR2 = Math.Min(4, (Array.IndexOf(r2seating, i + 1) % 5) + 1);
            //    Assert.AreEqual(transfersForR2[i], transferR2);
            //    var transferR3 = Math.Min(4, (Array.IndexOf(r3seating, i + 1) % 5) + 1);
            //    Assert.AreEqual(transfersForR3[i], transferR3);

            //    var deviation = Math.Abs(transferR1 + transferR2 + transferR3 - factory.ExactTransfer);
            //    exactAbsoluteDeviation += deviation;
            //}

            var absoluteDeviation = 0;
            for (int i = 0; i < 10; ++i)
            {
                var tranfers = transfersForR1[i] + transfersForR2[i] + transfersForR3[i];
                absoluteDeviation += Math.Abs(tranfers * 10 - factory.TotalTransfer3);
            }
            Assert.AreEqual(absoluteDeviation, seating[2 * 10]);

            // each player plays 3 times on a 5-players table, so the absolute deviation for vps must be 0
            Assert.AreEqual(0, seating[21]);
            // vp distribution
            Assert.AreEqual(0, seating[42]);
            Assert.AreEqual(0, seating[43]);
            Assert.AreEqual(0, seating[44]);
            Assert.AreEqual(10, seating[45]);

            for (int i = 0; i < 10; ++i)
            {
                var r2I = seating[2 * 10 + 2 + i];
                var r2P = seating[3 * 10 + 2 + i];
                Assert.AreEqual(i + 1, r2Seating[r2P * 5 + r2I]);
            }

            Assert.AreEqual(5, seating[46]);
            Assert.AreEqual(5, seating[47]);
            Assert.AreEqual(5, seating[48]);
            Assert.AreEqual(5, seating[49]);
        }

        [TestMethod]
        public void InitializeSeatingTest12()
        {
            var factory = new Seatings3RFactory(12);
            var r2Seating = new[] { 6, 9, 5, 3,   7, 11, 8, 1,   4, 12, 10, 2 };
            var r3Seating = new[] { 3, 7, 2, 9,   10, 8, 4, 5,   11, 1, 12, 6 };

            var seating = factory.GetSeating(r2Seating, r3Seating);

            AssertSequence.SequenceEqual(r2Seating, seating, 0);
            AssertSequence.SequenceEqual(r3Seating, seating, 12);

            Assert.AreEqual(10 * 3 * 3, factory.TotalTransfer3);

            var transfersForR1 = new[] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4};
            var transfersForR2 = new[] { 4, 4, 4, 1, 3, 1, 1, 3, 2, 3, 2, 2 };
            var transfersForR3 = new[] { 2, 3, 1, 3, 4, 4, 2, 2, 4, 1, 1, 3 };

            Assert.AreEqual((decimal)10 * 3 * 3 / 12, factory.ExactTransfer);
            //decimal exactAbsoluteDeviation = 0;
            //for (int i = 0; i < 10; ++i)
            //{
            //    var transferR1 = transfersForR1[i];
            //    var transferR2 = Math.Min(4, (Array.IndexOf(r2seating, i + 1) % 5) + 1);
            //    Assert.AreEqual(transfersForR2[i], transferR2);
            //    var transferR3 = Math.Min(4, (Array.IndexOf(r3seating, i + 1) % 5) + 1);
            //    Assert.AreEqual(transfersForR3[i], transferR3);

            //    var deviation = Math.Abs(transferR1 + transferR2 + transferR3 - factory.ExactTransfer);
            //    exactAbsoluteDeviation += deviation;
            //}

            var absoluteDeviation = 0;
            for (int i = 0; i < 12; ++i)
            {
                var tranfers = transfersForR1[i] + transfersForR2[i] + transfersForR3[i];
                absoluteDeviation += Math.Abs(tranfers * 12 - factory.TotalTransfer3);
            }
            Assert.AreEqual(absoluteDeviation, seating[2 * 12]);

            // each player plays 3 times on a 4-players table, so the absolute deviation for vps must be 0
            Assert.AreEqual(0, seating[25]);
            // vp distribution
            Assert.AreEqual(12, seating[50]);
            Assert.AreEqual(0, seating[51]);
            Assert.AreEqual(0, seating[52]);
            Assert.AreEqual(0, seating[53]);

            for (int i = 0; i < 12; ++i)
            {
                var r2I = seating[2 * 12 + 2 + i];
                var r2P = seating[3 * 12 + 2 + i];
                Assert.AreEqual(i + 1, r2Seating[r2P * 4 + r2I]);
            }

            Assert.AreEqual(4, seating[54]);
            Assert.AreEqual(4, seating[55]);
            Assert.AreEqual(4, seating[56]);
            Assert.AreEqual(4, seating[57]);
            Assert.AreEqual(4, seating[58]);
            Assert.AreEqual(4, seating[59]);
        }

        [TestMethod]
        public void IterationsFor12StateSanityCheck()
        {
            var factory = new Seatings3RFactory(12);
            int[] players;
            int i;
            var seatings = factory.GetArraySeatings(10000, out players, out i);

            foreach (var seating in seatings)
            {
                var expectedSeating = factory.GetSeating(seating.Take(12).ToArray(), seating.Skip(12).Take(12).ToArray());
                Assert.IsTrue(expectedSeating.SequenceEqual(seating));
            }
        }

        [TestMethod]
        public void Rule1Test()
        {
            var factory = new Seatings3RFactory(10);
            var r2Seating = new[] { 10, 1, 9, 8, 2, 5, 6, 4, 3, 7 };
            var r3Seating = new[] { 4, 7, 10, 3, 1, 9, 2, 5, 8, 6 };

            var rule1Result = factory.CheckRule1(r2Seating, r3Seating);
            Assert.IsFalse(rule1Result.Violated);

            r3Seating = new[] { 4, 7, 10, 3, 1, 2, 9, 5, 8, 6 };
            rule1Result = factory.CheckRule1(r2Seating, r3Seating);
            Assert.IsFalse(rule1Result.Violated);

            r3Seating = new[] { 2, 7, 10, 3, 1, 4, 9, 5, 8, 6 };
            rule1Result = factory.CheckRule1(r2Seating, r3Seating);
            Assert.IsTrue(rule1Result.Violated);

            r3Seating = new[] { 4, 7, 10, 3, 1, 9, 2, 5, 8, 6 };
            r2Seating = new[] { 10, 1, 9, 8, 2, 4, 5, 6, 3, 7 };
            rule1Result = factory.CheckRule1(r2Seating, r3Seating);
            Assert.IsTrue(rule1Result.Violated);

            // 10 and 8 have the same relationship in r2 - r3
            r2Seating = new[] { 2, 5, 7, 1, 6, 9, 4, 10, 8, 3 };
            r3Seating = new[] { 10, 8, 4, 7, 2, 5, 3, 1, 6, 9 };
            rule1Result = factory.CheckRule1(r2Seating, r3Seating);
            Assert.IsTrue(rule1Result.Violated);
        }

        [TestMethod]
        public void Rule2Test()
        {
            var factory = new Seatings3RFactory(25);
            var r2Seating = new[] 
            { 
                14, 10, 1, 22, 18, 
                19, 15, 6, 2, 23, 
                24, 20, 11, 7, 3, 
                4, 25, 16, 12, 8, 
                9, 5, 21, 17, 13 
            };
            var r3Seating = new[] 
            { 
                12, 23, 9, 20, 1, 
                13, 24, 10, 16, 2, 
                17, 3, 14, 25, 6,
                7, 18, 4, 15, 21,
                22, 8, 19, 5, 11 
            };

            var rule2Result = factory.CheckRule2(r2Seating, r3Seating);
            Assert.IsFalse(rule2Result.Violated);

            // swap player 14 and 24
            r3Seating = new[] 
            { 
                12, 23, 9, 20, 1, 
                13, 14, 10, 16, 2,
                17, 3, 24, 25, 6,
                7, 18, 4, 15, 21, 
                22, 8, 19, 5, 11 
            };
            rule2Result = factory.CheckRule2(r2Seating, r3Seating);
            Assert.IsTrue(rule2Result.Violated);
            Assert.AreEqual(3, rule2Result.Player);
            Assert.AreEqual(24, rule2Result.OtherPlayer);
            Assert.AreEqual(3, rule2Result.Round);
        }

        [TestMethod]
        public void Rule5Test()
        {
            var factory = new Seatings3RFactory(25);
            var r2Seating = new[] 
            { 
                14, 10, 1, 22, 18, 
                19, 15, 6, 2, 23, 
                24, 20, 11, 7, 3, 
                4, 25, 16, 12, 8, 
                9, 5, 21, 17, 13 
            };
            var r3Seating = new[] 
            { 
                12, 23, 9, 20, 1, 
                13, 24, 10, 16, 2, 
                17, 3, 14, 25, 6,
                7, 18, 4, 15, 21,
                22, 8, 19, 5, 11 
            };

            var rule5Result = factory.CheckRule5(r2Seating, r3Seating);
            Assert.IsFalse(rule5Result.Violated);

            // make player 18 sit once more in 5th position
            r3Seating = new[] 
            { 
                12, 23, 9, 20, 1, 
                13, 24, 10, 16, 2, 
                17, 3, 14, 25, 6,
                4, 15, 21, 7, 18, 
                22, 8, 19, 5, 11 
            };
            rule5Result = factory.CheckRule5(r2Seating, r3Seating);
            Assert.IsTrue(rule5Result.Violated);
            Assert.AreEqual(18, rule5Result.Player);
            Assert.AreEqual(3, rule5Result.Round);
            Assert.AreEqual(4, rule5Result.Table);
        }

        [TestMethod]
        public void Rule6Test5Players()
        {
            var factory = new Seatings3RFactory(25);
            var r2Seating = new[] 
            { 
                14, 10, 1, 22, 18, 
                19, 15, 6, 2, 23, 
                24, 20, 11, 7, 3, 
                4, 25, 16, 12, 8, 
                9, 5, 21, 17, 13 
            };
            var r3Seating = new[] 
            { 
                12, 23, 9, 20, 1, 
                13, 24, 10, 16, 2, 
                17, 3, 14, 25, 6,
                7, 18, 4, 15, 21,
                22, 8, 19, 5, 11 
            };

            var rule6Result = factory.CheckRule6(r2Seating, r3Seating);
            Assert.IsFalse(rule6Result.Violated);

            // swap 21 and 1 so that 4 has 1 as grandprey once again (as on table 1)
            r3Seating = new[] 
            { 
                12, 23, 9, 20, 21, 
                13, 24, 10, 16, 2, 
                17, 3, 14, 25, 6,
                7, 18, 4, 15, 1,
                22, 8, 19, 5, 11 
            };
            rule6Result = factory.CheckRule6(r2Seating, r3Seating);
            Assert.IsTrue(rule6Result.Violated);
            Assert.AreEqual(3, rule6Result.Round);

            // swap 23 and 18 so 1 has 18 as grandprey once again (as on table 2)
            r3Seating = new[] 
            { 
                12, 18, 9, 20, 1, 
                13, 24, 10, 16, 2, 
                17, 3, 14, 25, 6,
                7, 23, 4, 15, 21,
                22, 8, 19, 5, 11 
            };
            rule6Result = factory.CheckRule6(r2Seating, r3Seating);
            Assert.IsTrue(rule6Result.Violated);
            Assert.AreEqual(3, rule6Result.Round);

            // swap 7 and 2 so that 4 has 1 as grandpredator once again (as on table 1)
            r3Seating = new[] 
            { 
                12, 23, 9, 20, 1, 
                13, 24, 10, 16, 7, 
                17, 3, 14, 25, 6,
                2, 18, 4, 15, 21,
                22, 8, 19, 5, 11 
            };
            rule6Result = factory.CheckRule6(r2Seating, r3Seating);
            Assert.IsTrue(rule6Result.Violated);
            Assert.AreEqual(3, rule6Result.Round);

            // swap 9 and 14 so 1 has 14 as grandpredator once again (as on table 2)
            r3Seating = new[] 
            { 
                12, 23, 14, 20, 1, 
                13, 24, 10, 16, 2, 
                17, 3, 9, 25, 6,
                7, 18, 4, 15, 21,
                22, 8, 19, 5, 11 
            };
            rule6Result = factory.CheckRule6(r2Seating, r3Seating);
            Assert.IsTrue(rule6Result.Violated);
            Assert.AreEqual(3, rule6Result.Round);

            // check round 2: swap 18 and 3
            r2Seating = new[] 
            { 
                14, 10, 1, 22, 3, 
                19, 15, 6, 2, 23, 
                24, 20, 11, 7, 18, 
                4, 25, 16, 12, 8, 
                9, 5, 21, 17, 13 
            };
            rule6Result = factory.CheckRule6(r2Seating, r3Seating);
            Assert.IsTrue(rule6Result.Violated);
            Assert.AreEqual(2, rule6Result.Round);
        }

        [TestMethod]
        public void Rule6Test4And5Players()
        {
            var factory = new Seatings3RFactory(19);

            var r2Seating = new[] 
            { 
                14, 01, 03, 10, 17,
                19, 15, 02, 08, 06,
                09, 05, 12, 18, 16,
                04, 11, 13, 07
            };
            var r3Seating = new[] 
            { 
                15, 18, 07, 01, 04, 
                05, 08, 17, 11, 14, 
                10, 13, 16, 02, 19, 
                03, 09, 06, 12
            };

            var rule6Result = factory.CheckRule6(r2Seating, r3Seating);
            Assert.IsFalse(rule6Result.Violated);

            // 3 and 17 are grandpredator on R2: we swap 6 and 17 on r3 so that they are "crosstable" which is different
            r3Seating = new[] 
            { 
                15, 18, 07, 01, 04, 
                05, 08, 06, 11, 14, 
                10, 13, 16, 02, 19, 
                03, 09, 17, 12
            };

            rule6Result = factory.CheckRule6(r2Seating, r3Seating);
            Assert.IsFalse(rule6Result.Violated);

            // 9 and 18 are grandprey on R2: we swap 12 and 18 on r3 so that they are "crosstable" which is different
            r3Seating = new[] 
            { 
                15, 12, 07, 01, 04, 
                05, 08, 17, 11, 14, 
                10, 13, 16, 02, 19, 
                03, 09, 06, 18
            };

            rule6Result = factory.CheckRule6(r2Seating, r3Seating);
            Assert.IsFalse(rule6Result.Violated);

            // 4 and 13 are crosstable on R2, we put them in R3 four player table again with the same position
            r3Seating = new[] 
            { 
                15, 18, 07, 01, 12, 
                05, 08, 17, 11, 14, 
                10, 09, 16, 02, 19, 
                03, 04, 06, 13
            };

            rule6Result = factory.CheckRule6(r2Seating, r3Seating);
            Assert.IsTrue(rule6Result.Violated);
            Assert.AreEqual(3, rule6Result.Round);
        }

        [TestMethod]
        public void Rule7Test()
        {
            var factory = new Seatings3RFactory(25);
            var r2Seating = new[] 
            { 
                14, 10, 1, 22, 18, 
                19, 15, 6, 2, 23, 
                24, 20, 11, 7, 3, 
                4, 25, 16, 12, 8, 
                9, 5, 21, 17, 13 
            };
            var r3Seating = new[] 
            { 
                12, 23, 9, 20, 1, 
                13, 24, 10, 16, 2, 
                17, 3, 14, 25, 6,
                7, 18, 4, 15, 21,
                22, 8, 19, 5, 11 
            };

            var rule7Result = factory.CheckRule7(r2Seating, r3Seating);
            Assert.IsFalse(rule7Result.Violated);

            // check r2 / r1 with player 16
            r2Seating = new[] 
            { 
                14, 10, 1, 22, 18, 
                19, 15, 6, 2, 23, 
                24, 20, 11, 7, 3, 
                16, 12, 8, 4, 25, 
                9, 5, 21, 17, 13 
            };
            rule7Result = factory.CheckRule7(r2Seating, r3Seating);
            Assert.IsTrue(rule7Result.Violated);
            Assert.AreEqual(16, rule7Result.Player);
            Assert.AreEqual(2, rule7Result.Round);
            Assert.AreEqual(4, rule7Result.Table);
            Assert.AreEqual(1, rule7Result.Position);

            // check r3 / r1 with player 6
            r2Seating = new[] 
            { 
                14, 10, 1, 22, 18, 
                19, 15, 6, 2, 23, 
                24, 20, 11, 7, 3, 
                4, 25, 16, 12, 8, 
                9, 5, 21, 17, 13 
            };
            r3Seating = new[] 
            { 
                12, 23, 9, 20, 1, 
                13, 24, 10, 16, 2, 
                6, 17, 3, 14, 25, 
                7, 18, 4, 15, 21,
                22, 8, 19, 5, 11 
            };
            rule7Result = factory.CheckRule7(r2Seating, r3Seating);
            Assert.IsTrue(rule7Result.Violated);
            Assert.AreEqual(6, rule7Result.Player);
            Assert.AreEqual(3, rule7Result.Round);
            Assert.AreEqual(3, rule7Result.Table);
            Assert.AreEqual(1, rule7Result.Position);

            // check r3 / r2 with player 4
            r3Seating = new[] 
            { 
                12, 23, 9, 20, 1, 
                13, 24, 10, 16, 2, 
                17, 3, 14, 25, 6,
                4, 15, 21, 7, 18, 
                22, 8, 19, 5, 11 
            };
            rule7Result = factory.CheckRule7(r2Seating, r3Seating);
            Assert.IsTrue(rule7Result.Violated);
            Assert.AreEqual(4, rule7Result.Player);
            Assert.AreEqual(3, rule7Result.Round);
            Assert.AreEqual(4, rule7Result.Table);
            Assert.AreEqual(1, rule7Result.Position);
        }

        [TestMethod]
        public void Rule6ByGenerationTest()
        {
            var factory = new Seatings3RFactory(10);
            var r2Seating = new[] 
            { 
                0, 6, 1, 7, 2, 
                3, 9, 4, 10, 8 
            };
            var r3Seating = new[] 
            { 
                8, 4, 6, 3, 1,
                7, 0, 10, 2, 9 
            };

            var seating = factory.GetSeating(r2Seating, r3Seating);

            // 5 should be placed in the empty spaces, but doing so should violate rule 6 and therefore we should have no result
            var result = factory.GetSeatings(5, new[] { seating }).Count();
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Rule2ByGenerationTest()
        {
            var factory = new Seatings3RFactory(13);

            var r2Seating = new[] 
            { 
                9, 10, 6, 1, 11, 
                7, 4, 13, 3,
                2, 12, 0, 8
            };
            var r3Seating = new[] 
            { 
                13, 8, 10, 12, 7,
                3, 1, 4, 6,
                11, 0, 9, 2
            };

            var seating = factory.GetSeating(r2Seating, r3Seating);

            // 5 should be placed in the empty spaces, but doing so should violate rule 2 and therefore we should have no result
            var result = factory.GetSeatings(5, new[] { seating }).Count();
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetLittlePreyIndexTest()
        {
            var factory = new Seatings3RFactory(9);
            Assert.AreEqual(2, factory.GetLittlePreyIndex(0, 0, 5));
            Assert.AreEqual(3, factory.GetLittlePreyIndex(1, 0, 5));
            Assert.AreEqual(4, factory.GetLittlePreyIndex(2, 0, 5));
            Assert.AreEqual(0, factory.GetLittlePreyIndex(3, 0, 5));
            Assert.AreEqual(1, factory.GetLittlePreyIndex(4, 0, 5));

            Assert.AreEqual(7, factory.GetLittlePreyIndex(0, 1, 4));
            Assert.AreEqual(8, factory.GetLittlePreyIndex(1, 1, 4));
            Assert.AreEqual(5, factory.GetLittlePreyIndex(2, 1, 4));
            Assert.AreEqual(6, factory.GetLittlePreyIndex(3, 1, 4));
        }

        [TestMethod]
        public void GetGrandPredatorIndexTest()
        {
            var factory = new Seatings3RFactory(9);
            Assert.AreEqual(3, factory.GetGrandPredatorIndex(0, 0, 5));
            Assert.AreEqual(4, factory.GetGrandPredatorIndex(1, 0, 5));
            Assert.AreEqual(0, factory.GetGrandPredatorIndex(2, 0, 5));
            Assert.AreEqual(1, factory.GetGrandPredatorIndex(3, 0, 5));
            Assert.AreEqual(2, factory.GetGrandPredatorIndex(4, 0, 5));

            Assert.AreEqual(7, factory.GetGrandPredatorIndex(0, 1, 4));
            Assert.AreEqual(8, factory.GetGrandPredatorIndex(1, 1, 4));
            Assert.AreEqual(5, factory.GetGrandPredatorIndex(2, 1, 4));
            Assert.AreEqual(6, factory.GetGrandPredatorIndex(3, 1, 4));
        }

        [TestMethod]
        public void GetAnalysisTest()
        {
            var N = 13;
            var factory = new Seatings3RFactory(N);
            var brotherhood = factory.GetBrotherhood(Enumerable.Range(1, N).ToArray()).Select(b => b.ToArray()).ToArray();
            var seatingAnalysis = new SeatingAnalysis { Seating = factory.GetSeating(new[] { 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 10, 0 }, new[] { 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 10, 0 }) };
            var seatingAnalysis2 = new SeatingAnalysis { Seating = factory.GetSeating(new[] { 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, 6, 0 }, new[] { 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 10, 0 }) };
            var comparer = new R3SeatingsComplexComparer(brotherhood, factory);
            Assert.IsTrue(comparer.Equals(seatingAnalysis, seatingAnalysis2));
        }

        [TestMethod]
        public void GetAnalysisTestSimilarRelation()
        {
            var N = 12;
            var factory = new Seatings3RFactory(N);
            var brotherhood = factory.GetBrotherhood(Enumerable.Range(1, N).ToArray()).Select(b => b.ToArray()).ToArray();

            var seating1 = new[] { new[] { 4, 0, 0, 0 }, new[] { 0, 8, 11, 0 }, new[] { 12, 0, 0, 0 } }.SelectMany(a => a).ToArray();
            var seating2 = new[] { new[] { 8, 0, 0, 0 }, new[] { 0, 12, 3, 0 }, new[] { 4, 0, 0, 0 } }.SelectMany(a => a).ToArray();

            var seatingAnalysis = new SeatingAnalysis { Seating = factory.GetSeating(seating1, seating1) };
            var seatingAnalysis2 = new SeatingAnalysis { Seating = factory.GetSeating(seating2, seating2) };
            var comparer = new R3SeatingsComplexComparer(brotherhood, factory);
            Assert.IsTrue(comparer.Equals(seatingAnalysis, seatingAnalysis2));
        }

        [TestMethod]
        public void GetAnalysisTestDifferentRelation()
        {
            var N = 12;
            var factory = new Seatings3RFactory(N);
            var brotherhood = factory.GetBrotherhood(Enumerable.Range(1, N).ToArray()).Select(b => b.ToArray()).ToArray();

            var seating1 = new[] { new[] { 4, 0, 0, 0 }, new[] { 0, 8, 11, 0 }, new[] { 12, 0, 0, 0 } }.SelectMany(a => a).ToArray();
            var seating2 = new[] { new[] { 8, 0, 0, 0 }, new[] { 0, 12, 11, 0 }, new[] { 4, 0, 0, 0 } }.SelectMany(a => a).ToArray();

            var seatingAnalysis = new SeatingAnalysis { Seating = factory.GetSeating(seating1, seating1) };
            var seatingAnalysis2 = new SeatingAnalysis { Seating = factory.GetSeating(seating2, seating2) };
            var comparer = new R3SeatingsComplexComparer(brotherhood, factory);
            Assert.IsFalse(comparer.Equals(seatingAnalysis, seatingAnalysis2));
        }

        [TestMethod]
        public void GetAnalysisTestSameRelationButOnDifferentTableSizes()
        {
            var N = 13;
            var factory = new Seatings3RFactory(N);
            var brotherhood = factory.GetBrotherhood(Enumerable.Range(1, N).ToArray()).Select(b => b.ToArray()).ToArray();

            var seating1 = new[] { new[] { 0, 0, 0, 8, 9 }, new[] { 0, 0, 12, 13 }, new[] { 0, 0, 0, 0 } }.SelectMany(a => a).ToArray();
            var seating2 = new[] { new[] { 0, 0, 0, 12, 13 }, new[] { 0, 0, 8, 9}, new[] { 0, 0, 0, 0 } }.SelectMany(a => a).ToArray();

            var seatingAnalysis = new SeatingAnalysis { Seating = factory.GetSeating(seating1, seating1) };
            var seatingAnalysis2 = new SeatingAnalysis { Seating = factory.GetSeating(seating2, seating2) };
            var comparer = new R3SeatingsComplexComparer(brotherhood, factory);
            Assert.IsTrue(comparer.Equals(seatingAnalysis, seatingAnalysis2));
        }

        [TestMethod]
        public void SanityCheck8Players()
        {
            var N = 8;
            var factory = new Seatings3RFactory(N);

            var seating1 = new[] { new[] { 7, 1, 0, 2 }, new[] { 3, 5, 4, 6 } }.SelectMany(a => a).ToArray();
            var seating2 = new[] { new[] { 4, 7, 6, 1 }, new[] { 0, 3, 2, 5 } }.SelectMany(a => a).ToArray();

            var seating = factory.GetSeating(seating1, seating2);

            // 8 should be placed in the empty spaces, but we forbid the relation 8-5 to happen a second time, even though
            // the first time 8 was predator of 5, and on round 3 the reverse (we apply strictly 
            // "9. No pair of players repeat the same relative position group[^], when possible."
            // but it's ok since it works for higher numbers and helps reducing the number of combinations)
            var result = factory.GetSeatings(8, new[] { seating }).Count();
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void SanityCheck10Players()
        {
            var N = 10;
            var factory = new Seatings3RFactory(N);

            var seating1 = new[] { new[] { 0, 1, 9, 8, 2 }, new[] { 5, 6, 4, 3, 7 } }.SelectMany(a => a).ToArray();
            var seating2 = new[] { new[] { 4, 7, 0, 3, 1 }, new[] { 9, 2, 5, 8, 6 } }.SelectMany(a => a).ToArray();

            var seating = factory.GetSeating(seating1, seating2);

            // 10 should be placed in the empty spaces, but doing so violate rule 6
            var result = factory.GetSeatings(10, new[] { seating }).Count();
            Assert.AreEqual(0, result);
        }
    }
}