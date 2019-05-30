using fast.modeling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace fast.consoleapp
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: take the datasets files as params - junkfoodisbad
            var path = @"I:/culture-of-development/fast/datasets/xgboost";
            var dataPath = args.Length >= 1 ? args[0] : path;
            DoDecisionTreeTimings(dataPath, null);
            //DoFeatureReordering(dataPath);
            //CheckFeatureReordering(dataPath);
            // var reorderMapping = GetReorderMapping(path);
            // DoDecisionTreeTimings(dataPath, reorderMapping);
        }

        static void CheckFeatureReordering(string dataPath)
        {
            var model = GetModel(dataPath, null);
            ModelSummary(model);

            Console.WriteLine("\nReordered");
            var reorderMapping = GetReorderMapping("reorder.csv");
            var reorderedModel = FeatureReorderer.ReorderXGBoost(model, reorderMapping);
            ModelSummary(reorderedModel);
        }

        private static void ModelSummary(XGBoost model)
        {
            var allPaths = model.Trees.SelectMany(FeatureReorderer.GetAllPaths).ToArray();
            Console.WriteLine($"total paths: {allPaths.Length}");
            Console.WriteLine($"average path length: {allPaths.Average(m => m.Length)}");
            var pageCounts = allPaths.Select(FeatureReorderer.NumMemoryPages).ToArray();
            var pages = pageCounts.GroupBy(m => m)
                .Select(m => new { m.Key, Count = m.Count() })
                .OrderBy(m => m.Key)
                .ToArray();
            foreach(var pageCount in pages)
            {
                Console.WriteLine($"  {pageCount.Key}: {pageCount.Count}");
            }
            Console.WriteLine($"average page count: {pageCounts.Average(m => m)}");
        }

        static void DoFeatureReordering(string dataPath)
        {
            var model = GetModel(dataPath, null);
            int numTrees = model.Trees.Length;
            var timer = new Timer(SaveBestFeatures, dataPath, 60_000, 60_000);
            var greedyReorderMap = FeatureReorderer.Greedy(model, numTrees);
            timer.Dispose();
            SaveBestFeatures(null);
            var allFeatureIndices = new HashSet<short>(greedyReorderMap);
            for(short i = 0; i < greedyReorderMap.Length; i++)
            {
                if (!allFeatureIndices.Contains(i)) 
                {
                    Console.WriteLine("The reorder mapping is not valid: not found index " + i);
                    break;
                }
            }
        }

        static void SaveBestFeatures(Object stateInfo)
        {
            string dataPath = (string)stateInfo;
            var filename = Path.Combine(dataPath, @"reorder.csv");
            File.WriteAllLines(filename, FeatureReorderer.bestMap.Select(m => m.ToString()));
        }

        static void DoDecisionTreeTimings(string dataPath, short[] reorderMapping)
        {
            var model = GetModel(dataPath, reorderMapping);
            var samples = GetSamples(dataPath, reorderMapping);

            Random r = new Random(20190524);
            var toRun = samples.Select(m => m.Value)
                .Concat(samples.Select(m => m.Value))
                .OrderBy(m => r.Next())
                .ToArray();

            var timer = Stopwatch.StartNew();
            //var results = new double[toRun.Length];
            double[] results;
            for (int _ = 0; _ < 20; _++)
            {
                Console.WriteLine(_);
                // for (int i = 0; i < toRun.Length; i++)
                // {
                //     results[i] = model.EvaluateProbability(toRun[i]);
                // }
                results = model.EvaluateProbability(toRun);
            }
            timer.Stop();
            Console.WriteLine($"Time taken for {toRun.Length} evaluations: {timer.Elapsed.TotalMilliseconds} ms");
        }

        private static XGBoost GetModel(string dataPath, short[] reorderMapping)
        {
            var filename = Path.Combine(dataPath, @"model_xbg_trees.txt");
            var treesString = File.ReadAllText(filename);
            var model = XGBoost.Create(treesString);
            if (reorderMapping != null)
            {
                model = FeatureReorderer.ReorderXGBoost(model, reorderMapping);
            }
            return model;
        }

        private static Dictionary<string, float[]> GetSamples(string dataPath, short[] reorderMapping)
        {
            var filename = Path.Combine(dataPath, @"xgboost_test_cases_no_feature_names.txt");
            var samplesString = File.ReadLines(filename);
            var samples = new Dictionary<string, float[]>();
            foreach (var line in samplesString.Skip(1))
            {
                var parts = line.Split(',');
                var sample = parts[0];
                var featureIndex = int.Parse(parts[1]);
                if (reorderMapping != null && featureIndex >= 0 && featureIndex < reorderMapping.Length)
                {
                    featureIndex = reorderMapping[featureIndex];
                }
                var value = float.Parse(parts[2]);
                if (!samples.ContainsKey(sample))
                {
                    samples.Add(sample, new float[1000]);
                }
                samples[sample][featureIndex] = value;
            }
            return samples;
        }

        private static short[] GetReorderMapping(string dataPath)
        {
            var filename = Path.Combine(dataPath, @"reorder.csv");
            var reorderMapping = File.ReadAllLines(filename)
                .Select(m => short.Parse(m))
                .ToArray();
            return reorderMapping;
        }
    }
}
