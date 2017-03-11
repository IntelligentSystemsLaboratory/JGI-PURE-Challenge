using System;
using System.IO;
using System.Linq;
using MathNet.Numerics.Data.Matlab;
using MicrosoftResearch.Infer.Distributions;

namespace AVPTM
{
    internal class Program
    {
        public static string DataPath = "../../../data";

        public static void Main(string[] args)
        {
            const string dataset = "nips12raw_str602";
            string filename = Path.Combine(DataPath, dataset + ".mat");

            var dataList = MatlabReader.List(filename);

            var counts = MatlabReader.Read<double>(filename, "counts");
            var wc = MatlabReader.Read<double>(filename, "wc");

            // var lda = new LDA(wc.RowCount, 20);
            const int batches = 100;
            var lda = new LDAShared(batches, wc.RowCount, 20);

            var wordsInDoc = counts.EnumerateColumns()
                .Select(row => row.EnumerateIndexed().ToDictionary(ia => ia.Item1, ia => (int) ia.Item2))
                .ToArray();

            Dirichlet[] postTheta;
            Dirichlet[] postPhi;
            lda.Infer(wordsInDoc, 1.0, 1.0, out postTheta, out postPhi);

            var ldaPredict = new LDAPredictionModel(wc.RowCount, 20);
            var predictions = ldaPredict.Predict(postTheta, postPhi);

            foreach (var prediction in predictions)
            {
                Console.WriteLine(prediction);
            }
        }
    }
}