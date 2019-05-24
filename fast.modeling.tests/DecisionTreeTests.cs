using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using fast.modeling;

namespace fast.modeling.tests
{
    public class DecisionTreeTests : TestsBase
    {
        public DecisionTreeTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void TestDecisionTreeCreate()
        {
            var dt = DecisionTree.Create(exampleDecisionTree);
            var nodeStrings = dt.ToString().Split('\n');
            var exampleStrings = exampleDecisionTree.Split('\n');
            foreach(var n in nodeStrings)
            {
                bool isCool = false;
                foreach(var e in exampleStrings)
                {
                    isCool = isCool || e.StartsWith(n);
                }
                if (!isCool) Assert.True(isCool, "node failed to be found: '" + n + "'");
            }
        }

        [Fact]
        public void TestDecisionTreeEvaluate()
        {
            var dt = DecisionTree.Create(exampleDecisionTree);
            var actual = dt.Evaluate(testValues);
            var expected = 0.077062957;
            Assert.InRange(actual, expected - 1e-06, expected + 1e06);
        }


        [Fact]
        public void TestXGBoostCreate()
        {
            var filename = @"../../../../datasets/xgboost/model_xbg_trees.txt";
            var treesString = File.ReadAllText(filename);
            var model = XGBoost.Create(treesString);
            // TODO: ensure correctness of each tree
        }

        [Fact]
        public void TestXGBoostEvaluate()
        {
            var filename = @"../../../../datasets/xgboost/model_xbg_trees.txt";
            var treesString = File.ReadAllText(filename);
            var model = XGBoost.Create(treesString);

            var filename2 = @"../../../../datasets/xgboost/xgboost_test_cases_no_feature_names.txt";
            var samplesString = File.ReadLines(filename2);
            var samples = new Dictionary<string, float[]>();
            foreach(var line in samplesString.Skip(1))
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

            var timer = Stopwatch.StartNew();
            const int probablity_feature_index = 840;
            int i = 0;
            for(; i < 2; i++)
            foreach(var sample in samples)
            {
                var actual = model.EvaluateProbability(sample.Value);
                var expected = sample.Value[probablity_feature_index];
                Assert.InRange(actual, expected - 1e-06, expected + 1e06);
            }
            timer.Stop();
            output.WriteLine($"Time taken for {i*samples.Count} evaluations: {timer.Elapsed.TotalMilliseconds} ms");
        }

        [Fact]
        public void TestXGBoostEvaluateTiming()
        {
            var filename = @"../../../../datasets/xgboost/model_xbg_trees.txt";
            var treesString = File.ReadAllText(filename);
            var model = XGBoost.Create(treesString);

            var filename2 = @"../../../../datasets/xgboost/xgboost_test_cases_no_feature_names.txt";
            var samplesString = File.ReadLines(filename2);
            var samples = new Dictionary<string, float[]>();
            foreach(var line in samplesString.Skip(1))
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
            for(int i = 0; i < toRun.Length; i++)
            {
                results[i] = model.EvaluateProbability(toRun[i]);
            }
            timer.Stop();
            output.WriteLine($"Time taken for {toRun.Length} evaluations: {timer.Elapsed.TotalMilliseconds} ms");
        }


        static DecisionTreeTests()
        {
            foreach(var i in new[] { 17, 2, 6, 732, 734, 8, 762, 2, 27, 285 })
            { 
                testValues[i] = 0.1f;
            }
            testValues[0] = 1.0f;
        }
        static float[] testValues = new float[850];
        const string exampleDecisionTree = 
@"0:[f0<0.99992311] yes=1,no=2,missing=1,gain=97812.25,cover=218986
1:leaf=-0.199992761,cover=27584.75
2:[f17<0.000367681001] yes=3,no=4,missing=3,gain=10373.0732,cover=191401.25
3:[f2<0.5] yes=5,no=6,missing=5,gain=4121.85938,cover=103511.5
5:[f6<0.00216802233] yes=9,no=10,missing=9,gain=873.340759,cover=50533.25
9:[f732<0.5] yes=17,no=18,missing=17,gain=515.368896,cover=33356.75
17:leaf=-0.00213295687,cover=25503.5
18:leaf=-0.0314288437,cover=7853.25
10:[f732<0.5] yes=19,no=20,missing=19,gain=276.522034,cover=17176.5
19:leaf=0.0253729131,cover=13474
20:leaf=-0.00548130181,cover=3702.5
6:[f734<0.237819463] yes=11,no=12,missing=11,gain=2141.23145,cover=52978.25
11:[f8<0.00104575348] yes=21,no=22,missing=21,gain=566.334961,cover=35689
21:leaf=-0.0620479584,cover=24457.5
22:leaf=-0.0349165387,cover=11231.5
12:[f762<0.308019817] yes=23,no=24,missing=23,gain=483.886719,cover=17289.25
23:leaf=-0.0144120604,cover=16450.5
24:leaf=0.063411735,cover=838.75
4:[f2<0.5] yes=7,no=8,missing=7,gain=2694.23291,cover=87889.75
7:[f27<0.000739371637] yes=13,no=14,missing=13,gain=928.447266,cover=44100.5
13:[f732<0.5] yes=25,no=26,missing=25,gain=285.069702,cover=17082.25
25:leaf=0.032621529,cover=13427.25
26:leaf=0.00112144416,cover=3655
14:[f285<0.000919258455] yes=27,no=28,missing=27,gain=421.745117,cover=27018.25
27:leaf=0.0483669229,cover=20145
28:leaf=0.077062957,cover=6873.25
8:[f734<0.103942066] yes=15,no=16,missing=15,gain=1591.2124,cover=43789.25
15:[f101<0.000240761583] yes=29,no=30,missing=29,gain=608.92157,cover=24192.75
29:leaf=-0.0209285971,cover=14574.75
30:leaf=0.0114876805,cover=9618
16:[f722<0.5] yes=31,no=32,missing=31,gain=601.422363,cover=19596.5
31:leaf=0.0258833747,cover=18429.75
32:leaf=0.099892959,cover=1166.75
";
    }
}
