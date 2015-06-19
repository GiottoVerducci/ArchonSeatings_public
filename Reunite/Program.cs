using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reunite
{
    using System.IO;

    using ConsoleApplication5;

    class Program
    {
        static int Main(string[] args)
        {
            int n;
            if (args.Length < 1 || !int.TryParse(args[0], out n) || n < 0)
            {
                Console.WriteLine("Expected argument: positive number of players.");
                return 1;
            }

            n = 18;
            //var factory = new SeatingsFactory2(n);
            var factory = new Seatings3RFactory(n);
            var filePrefix = "seating3r-";

            var path = @"d:\temp\csv\ArchonSeatings\Results\18\";
            //var path = @"d:\temp\csv\ArchonSeatings\Results\3f+f-19-rule2\new\temp\";
            //var path = @"d:\temp\csv\ArchonSeatings\SeatingsGenerator\bin\ReleaseTest3rf\";
            //var path = @"c:\Share\MPAPI\VR\";
            //var path = @"d:\temp\csv\ArchonSeatings\SeatingsGenerator\bin\Debug\";
            var files = Directory.GetFiles(path, string.Format("{0}{1}.txt-*", filePrefix, n));
            var result = new List<int[]>();


            //int bestTransferAbsoluteDeviation = Int32.MaxValue;
            //int bestVpAbsoluteDeviation = Int32.MaxValue;

            if (files.Length == 0)
            {
                Console.WriteLine("No files found for " + n);
                return 1;
            }

            int i = 0;
            var seatingLength = factory.GetSeatingTextLength();
            foreach (var file in files)
            {
                try
                {
                    Console.WriteLine("{0}/{1}", ++i, files.Count());
                    //var lastLine = File.ReadLines(file).Last();

                    byte[] bytes = new byte[seatingLength];
                    using (var reader = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        reader.BaseStream.Seek(-seatingLength - 2, SeekOrigin.End);
                        reader.Read(bytes, 0, seatingLength);

                        var index = Array.IndexOf(bytes, (byte)0x0A);
                        if (index != -1)
                        {
                            var newEndIndex = seatingLength + 1 - index;
                            reader.BaseStream.Seek(-seatingLength - 2 - newEndIndex, SeekOrigin.End);
                            reader.Read(bytes, 0, seatingLength);
                        }
                    }

                    var lastLine = Encoding.Default.GetString(bytes);
                    var seating = factory.GetSeatingFromText(lastLine);
                    result.Add(seating);
                }
                catch (Exception)
                {
                }

                //var transferDeviation = factory.GetTransferAbsoluteDeviationFast(seating);
                //if (transferDeviation <= bestTransferAbsoluteDeviation)
                //{
                //    var vpDeviation = factory.GetVpAbsoluteDeviationFast(seating);
                //    if (vpDeviation <= bestVpAbsoluteDeviation)
                //    {
                //        if(transferDeviation < bestTransferAbsoluteDeviation && vpDeviation < bestVpAbsoluteDeviation)
                //        {
                //            result.Clear();
                //        }
                //        bestTransferAbsoluteDeviation = transferDeviation;
                //        bestVpAbsoluteDeviation = vpDeviation;
                //        result.Add(seating);
                //    }

            }

            var resultFile = Path.Combine(path, string.Format("{0}{1}.txt", filePrefix, n));
            var bestSeatings = factory.GetBestSeatingsAbsolute(result);
            using (var dest = File.CreateText(resultFile))
            {
                foreach (var seating in bestSeatings)
                {
                    dest.WriteLine(factory.GetSeatingText(seating));
                }
            }
            return 0;
        }
    }
}
