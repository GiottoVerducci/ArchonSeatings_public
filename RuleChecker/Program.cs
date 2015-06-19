using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RuleChecker
{
    using System.IO;

    using ConsoleApplication5;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //var lines = File.ReadAllLines(@"d:\temp\csv\ArchonSeatings\SeatingsGenerator\bin\ReleaseTest3rf\seating3r-13.txt");
            //var factory = new Seatings3RFactory(13);

            //var result = new List<string>();
            //foreach (var line in lines)
            //{
            //    var items = line.Split('/');
            //    var r2 = SeatingsFactoryBase.GetSeatingFromTextNoCheck(items[0]);
            //    var r3 = SeatingsFactoryBase.GetSeatingFromTextNoCheck(items[1]);

            //    var relations = factory.WhoMeetsWho(r2, r3);

            //    var count = new int[3];

            //    foreach (var relation in relations)
            //    {
            //        count[relation.Key - 1] = relation.Value.Count;
            //    }

            //    result.Add(string.Join("\t", count));
            //}
            //File.WriteAllLines(@"d:\temp\csv\ArchonSeatings\SeatingsGenerator\bin\ReleaseTest3rf\seating3r-13.txt", result);
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
