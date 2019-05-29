﻿using fast.modeling;
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
            var dataPath = args.Length >= 1 ? args[0] : path;
            //DoDecisionTreeTimings(dataPath);
            DoFeatureReordering(dataPath);
        }

        static void DoFeatureReordering(string dataPath)
        {
            var model = GetModel(dataPath);
            int numTrees = model.Trees.Length;
            var allPaths = model.Trees.Take(numTrees).SelectMany(FeatureReorderer.GetAllPaths).ToArray();
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
            var greedyReorderMap = FeatureReorderer.Greedy(model, numTrees);
            var allFeatureIndices = new HashSet<short>(greedyReorderMap);
            File.WriteAllLines("reorder.csv", greedyReorderMap.Select(m => m.ToString()));
            for(short i = 0; i < greedyReorderMap.Length; i++)
            {
                if (!allFeatureIndices.Contains(i)) 
                {
                    Console.WriteLine("The reorder mapping is not valid: not found index " + i);
                    break;
                }
            }
            // TODO: perform the remapping
        }

        static void DoDecisionTreeTimings(string dataPath)
        {
            var model = GetModel(dataPath);

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

        private static XGBoost GetModel(string dataPath)
        {
            var filename = Path.Combine(dataPath, @"model_xbg_trees.txt");
            var treesString = File.ReadAllText(filename);
            var model = XGBoost.Create(treesString);
            return model;
        }
    }
}
