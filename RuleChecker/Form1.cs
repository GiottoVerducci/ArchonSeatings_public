namespace RuleChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using ConsoleApplication5;

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            rtbDetails.Clear();
            rtbOutput.Clear();
            rtbOutput.Visible = true;
            dataGridView1.Visible = false;

            var split = tbR2.Text.Split('/');
            if (split.Count() == 2)
            {
                tbR2.Text = split[0];
                tbR3.Text = split[1];
            }

            int[] r2Seatings, r3Seatings;
            try
            {
                r2Seatings = SeatingsFactoryBase.GetSeatingFromTextNoCheck(tbR2.Text);
            }
            catch (Exception e1)
            {
                rtbOutput.AppendText("Invalid R2 seating: cannot parse.\n" + e1);
                return;
            }

            if (r2Seatings.Length == 0)
            {
                rtbOutput.AppendText("Invalid R2 seating: empty.\n");
                return;
            }

            try
            {
                r3Seatings = SeatingsFactoryBase.GetSeatingFromTextNoCheck(tbR3.Text);
            }
            catch (Exception e2)
            {
                rtbOutput.AppendText("Invalid R3 seating: cannot parse.\n" + e2);
                return;
            }

            if (!SeatingContainsAllPlayers(r2Seatings))
            {
                rtbOutput.AppendText(string.Format("Invalid R2 seating: not all values between 1 and {0} are present.\n", r2Seatings.Length));
                return;
            }

            if (r3Seatings.Length > 0 && !SeatingContainsAllPlayers(r2Seatings))
            {
                rtbOutput.AppendText(string.Format("Invalid R3 seating: not all values between 1 and {0} are present.\n", r3Seatings.Length));
                return;
            }

            if (r3Seatings.Length > 0 && r2Seatings.Length != r3Seatings.Length)
            {
                rtbOutput.AppendText(string.Format("Invalid seatings: R2 has {0} players, and R3 has {1} players.\n", r2Seatings.Length, r3Seatings.Length));
                return;
            }

            rtbOutput.Visible = false;
            dataGridView1.Visible = true;


            if (r3Seatings.Length == 0)
            {
                CheckRules2R(r2Seatings);
            }
            else
            {
                CheckRules3R(r2Seatings, r3Seatings);
            }
        }

        private void CheckRules3R(int[] r2Seatings, int[] r3Seatings)
        {
            var bestVps = new Dictionary<int, string>
            {
                { 8, "OK" },
                { 9, "0,22" },
                { 10, "OK" },
                { 12, "OK" },
                { 13, "0,071" },
                { 14, "0,061" },
                { 15, "OK" },
                { 16, "OK" },
                { 17, "0,14" },
                { 18, "0,22" },
                { 19, "0,18" },
                { 20, "OK" },
                { 21, "0,21" },
                { 22, "*4,29" },
                { 23, "*4,49" },
                { 24, "*4,73" },
                { 25, "OK" },
            };

            var bestTransfers = new Dictionary<int, string> 
            { 
                { 8, "1" },
                { 9, "0,66" },
                { 10, "0,72" },
                { 12, "1" },
                { 13, "0,804" },
                { 14, "0,734" },
                { 15, "0,72" },
                { 16, "1" },
                { 17, "0,86" },
                { 18, "0,66" },
                { 19, "0,74" },
                { 20, "0,72" },
                { 21, "0,89" },
                { 22, "*2,63" },
                { 23, "*2,69" },
                { 24, "*2,75" },
                { 25, "*2,8" },
            };
         
            int N = r2Seatings.Length;
            var factory = new Seatings3RFactory(N);

            var seatings = factory.GetSeating(r2Seatings, r3Seatings);
            var rule1 = factory.CheckRule1(r2Seatings, r3Seatings);
            var rule2 = factory.CheckRule2(r2Seatings, r3Seatings);
            var rule3 = factory.CheckRule3(r2Seatings, r3Seatings);
            var rule5 = factory.CheckRule5(r2Seatings, r3Seatings);
            var rule6 = factory.CheckRule6(r2Seatings, r3Seatings);
            var rule7 = factory.CheckRule7(r2Seatings, r3Seatings);
            var rule8 = factory.CheckRule8(r2Seatings, r3Seatings);

            foreach (var keyValuePair in rule2.Relations)
            {
                rtbDetails.AppendText(string.Format("{0} occurence(s) of players meeting each other {1} times: {2}\r\n", keyValuePair.Value.Count, keyValuePair.Key, string.Join(", ", keyValuePair.Value.Select(pair => string.Format("{0} and {1}", pair.Item1, pair.Item2)))));
            }

            var meetCount = string.Join(", ", rule2.Relations.OrderBy(kvp => kvp.Key).Select(keyValuePair => string.Format("{0} ({1})", keyValuePair.Value.Count, keyValuePair.Key == 1 ? "once" : (keyValuePair.Key == 2 ? "twice" : "thrice"))));

            string bestTransfer, bestVp;
            var result = new[]
            {
                new Tuple<string, string, string>(
                    "N",
                    null,
                    N.ToString()),
                new Tuple<string, string,  string>(
                    "1. No pair of players repeat their predator-prey relationship. This is mandatory, by the VEKN rules.",
                    null,
                    rule1.Violated ? string.Format("KO (player {0}, table {1}, position {2}, round {3})", rule1.Player, rule1.Table, rule1.Position, rule1.Round) : "OK"),
                new Tuple<string, string,  string>(
                    "2. No pair of players share a table through all two rounds, when possible.  (N/A in some 2R event.)",
                    null,
                    (rule2.Violated ? string.Format("KO (player {0} shares a table with player {1} on round {2})", rule2.Player, rule2.OtherPlayer, rule2.Round) : "OK") + ": " + meetCount),
                new Tuple<string, string,  string>(
                    "3. Available VPs are equitably distributed.",
                    bestVps.TryGetValue(N, out bestVp) ? bestVp : "?",
                    rule3.Violated ? "KO. Absolute deviation is: " + factory.GetVpAbsoluteDeviation(seatings) + " => " + string.Join(" | ", rule3.Vps.Select((vp, i) => new { Player = i + 1, Vp = vp }).GroupBy(p => p.Vp).Select(g => string.Format("{1} have {0} VP", g.Key, string.Join(", ", g.Select(p => p.Player))))) : "OK"),
                new Tuple<string, string,  string>(
                    "5. A player doesn't sit in the fifth seat more than once.",
                    null,
                    rule5.Violated ? string.Format("KO (player {0}, table {1}, round {2})", rule5.Player, rule5.Table, rule5.Round) : "OK"),
                new Tuple<string, string,  string>(
                    "6. No pair of players repeat the same relative position[*], when possible.",
                    null,
                    rule6.Violated ? string.Format("KO (player {0}, table {1}, round {2})", "?", rule6.Table, rule6.Round) : "OK"),
                new Tuple<string, string,  string>(
                    "7. A player doesn't play in the same seat position, if possible.",
                    null,
                    rule7.Violated ? string.Format("KO (player {0}, table {1}, position {2})", rule7.Player, rule7.Table, rule7.Position) : "OK"),
                new Tuple<string, string,  string>(
                    "8. Starting transfers are equitably distributed. [NOAL]",
                    bestTransfers.TryGetValue(N, out bestTransfer) ? bestTransfer : "?",
                    rule8.Violated ? "KO. Absolute deviation is: " + factory.GetTransferAbsoluteDeviation(seatings) + " => " + string.Join(" | ", rule8.Transfers.Select((transfers, i) => new { Player = i + 1, Transfers = transfers }).GroupBy(p => p.Transfers).Select(g => string.Format("{1} have {0} transfers", g.Key, string.Join(", ", g.Select(p => p.Player))))) : "OK"),
            };
            dataGridView1.DataSource = result;
        }

        private void CheckRules2R(int[] r2Seatings)
        {
            var bestVps = new Dictionary<int, string> 
            {
                { 10, "0" },
                { 12, "0" },
                { 13, "0.5128" },
                { 14, "0.4761" },
                { 15, "0" },
                { 16, "0" },
                { 17, "0.5" },
                { 18, "0.1111" },
                { 19, "0.5" },
                { 20, "0" },
                { 21, "0.4952" },
                { 22, "0.2545" },
                { 23, "0.3826" },
                { 24, "0.4666" },
                { 26, "0.4102" },
                { 27, "0.1111" },
                { 29, "0.4252" },
                { 30, "0" },
                { 31, "0.1658" }
            };

            var bestTransfers = new Dictionary<int, string> 
            { 
                { 10, "0.72" }, 
                { 12, "0.3333" },
                { 13, "0.3905" },
                { 14, "0.6122" },
                { 15, "0.72" },
                { 16, "0" },
                { 17, "0.3114" },
                { 18, "0.5185" },
                { 19, "0.6481" },
                { 20, "0.76" },
                { 21, "0.2585" },
                { 22, "0.4462" },
                { 23, "0.5784" },
                { 24, "0.6666" },
                { 26, "0.3905" },
                { 27, "0.5185" },
                { 29, "0.6777" },
                { 30, "0.72" },
                { 31, "0.4682" } 
            };

            int N = r2Seatings.Length;
            var factory = new SeatingsFactory2(N);

            var rule1 = factory.CheckRule1(r2Seatings);
            var rule2 = factory.CheckRule2(r2Seatings);
            var rule3 = factory.CheckRule3(r2Seatings);
            var rule5 = factory.CheckRule5(r2Seatings);
            var rule6 = factory.CheckRule6(r2Seatings);
            var rule7 = factory.CheckRule7(r2Seatings);
            var rule8 = factory.CheckRule8(r2Seatings);

            string bestTransfer, bestVp;
            var result = new[]
            {
                new Tuple<string, string, string>(
                    "N",
                    null,
                    N.ToString()),
                new Tuple<string, string,  string>(
                    "1. No pair of players repeat their predator-prey relationship. This is mandatory, by the VEKN rules.",
                    null,
                    rule1.Violated ? string.Format("KO (player {0}, table {1}, position {2})", rule1.Player, rule1.Table, rule1.Position) : "OK"),

                new Tuple<string, string,  string>(
                    "2. No pair of players share a table through all two rounds, when possible.  (N/A in some 2R event.)",
                    null,
                    rule2.Violated ? string.Format("KO (player {0} shares a table with player {1})", rule2.Player, rule2.OtherPlayer) : "OK"),

                new Tuple<string, string,  string>(
                    "3. Available VPs are equitably distributed.",
                    bestVps.TryGetValue(N, out bestVp) ? bestVp : "?",
                    rule3.Violated ? "KO. Absolute deviation is: " + factory.GetVpAbsoluteDeviation(r2Seatings) + " => " + string.Join(" | ", rule3.Vps.Select((vp, i) => new { Player = i + 1, Vp = vp }).GroupBy(p => p.Vp).Select(g => string.Format("{1} have {0} VP", g.Key, string.Join(", ", g.Select(p => p.Player))))) : "OK"),

                new Tuple<string, string,  string>(
                    "4. No pair of players share a table more often than necessary.",
                    null,
                    "N/A"),

                new Tuple<string, string,  string>(
                    "5. A player doesn't sit in the fifth seat more than once.",
                    null,
                    rule5.Violated ? string.Format("KO (player {0}, table {1})", rule5.Player, rule5.Table) : "OK"),

                new Tuple<string, string,  string>(
                    "6. No pair of players repeat the same relative position[*], when possible.",
                    null,
                    rule6.Violated ? string.Format("KO (player {0}, table {1})", "X", rule6.Table) : "OK"),

                new Tuple<string, string,  string>(
                    "7. A player doesn't play in the same seat position, if possible.",
                    null,
                    rule7.Violated ? string.Format("KO (player {0}, table {1}, position {2})", rule7.Player, rule7.Table, rule7.Position) : "OK"),

                new Tuple<string, string,  string>(
                    "8. Starting transfers are equitably distributed. [NOAL]",
                    bestTransfers.TryGetValue(N, out bestTransfer) ? bestTransfer : "?",
                    rule8.Violated ? "KO. Absolute deviation is: " + factory.GetTransferAbsoluteDeviation(r2Seatings) + " => " + string.Join(" | ", rule8.Transfers.Select((transfers, i) => new { Player = i + 1, Transfers = transfers }).GroupBy(p => p.Transfers).Select(g => string.Format("{1} have {0} transfers", g.Key, string.Join(", ", g.Select(p => p.Player))))) : "OK"),
};
            dataGridView1.DataSource = result;

        }

        private bool SeatingContainsAllPlayers(int[] seatings)
        {
            for (int i = 1; i < seatings.Length + 1; i++)
            {
                if (!seatings.Contains(i))
                {
                    return false;
                }
            }
            return true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            tbR2.Text = "";
            tbR3.Text = "";
        }
    }
}
