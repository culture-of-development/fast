using fast.modeling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace fast.consoleapp
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: take the datasets files as params - junkfoodisbad
            var path = @"I:/culture-of-development/fast/datasets/xgboost";
            DoDecisionTreeTimings(path);
        }

        static void DoDecisionTreeTimings(string dataPath)
        {
            var filename = Path.Combine(dataPath, @"model_xbg_trees.txt");
            var treesString = File.ReadAllText(filename);
            var model = XGBoost.Create(treesString);

            var filename2 = Path.Combine(dataPath, @"xgboost_test_cases_no_feature_names.txt");
            var samplesString = File.ReadLines(filename2);
            var samples = new Dictionary<string, float[]>();
            foreach (var line in samplesString.Skip(1))
            {
                var parts = line.Split(',');
                var sample = parts[0];
                var featureIndex = int.Parse(parts[1]);
                var value = float.Parse(parts[2]);
                if (!samples.ContainsKey(sample))
                {
                    samples.Add(sample, new float[1000]);
                }
                samples[sample][featureIndex] = value;
            }

            Random r = new Random(20190524);
            var toRun = samples.Select(m => m.Value)
                .Concat(samples.Select(m => m.Value))
                .OrderBy(m => r.Next())
                .ToArray();

            var timer = Stopwatch.StartNew();
            var results = new double[toRun.Length];
            for (int _ = 0; _ < 100; _++)
            {
                Console.WriteLine(_);
                for (int i = 0; i < toRun.Length; i++)
                {
                    results[i] = model.EvaluateProbability(toRun[i]);
                }
            }
            timer.Stop();
            Console.WriteLine($"Time taken for {toRun.Length} evaluations: {timer.Elapsed.TotalMilliseconds} ms");
        }
    }
}
