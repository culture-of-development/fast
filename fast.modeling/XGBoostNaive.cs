using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace fast.modeling
{
    public sealed class DecisionTreeNaive
    {
        public struct DecisionTreeNode
        {
            public int FeatureIndex { get; set; }
            public double Value { get; set; }
            public int TrueBranch { get; set; }
            public int FalseBranch { get; set; }
        }

        const int LeafIndex = -1;

        private readonly DecisionTreeNode[] nodes;

        public DecisionTreeNaive(DecisionTreeNode[] tree)
        {
            nodes = tree;
        }

        public double Evaluate(double[] features)
        {
            var node = nodes[0];
            while(node.FeatureIndex != LeafIndex)
            {
                int nodeIndex = features[node.FeatureIndex] < node.Value ? node.TrueBranch : node.FalseBranch;
                node = nodes[nodeIndex];
            }
            return node.Value;
        }

        public static DecisionTreeNaive Create(string definition)
        {
            var lines = definition.Split('\n');
            var nodes = lines
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(ParseLine)
                .OrderBy(m => m.index)
                .Select(m => m.node)
                .ToArray();
            var dt = new DecisionTreeNaive(nodes);
            return dt;
        }
        private static (int index, DecisionTreeNode node) ParseLine(string line)
        {
            var parts = line.Split(':');
            var index = int.Parse(parts[0]);
            var nodeInfo = parts[1];
            var node = nodeInfo.StartsWith("leaf=") ? ParseLeaf(nodeInfo) : ParseDecision(nodeInfo);
            return (index, node);
        }
        // leaf example: "leaf=-0.199992761,cover=27584.75"
        private static readonly Regex leafParser = new Regex(@"^leaf=([^,]+),cover=(.*)$", RegexOptions.Compiled);
        private static DecisionTreeNode ParseLeaf(string nodeInfo)
        {
            var parts = leafParser.Match(nodeInfo);
            return new DecisionTreeNode
            { 
                FeatureIndex = LeafIndex, 
                Value = float.Parse(parts.Groups[1].Value),
                //Cover = float.Parse(parts.Groups[2].Value),
            };
        }
        // decision example: "[f0<0.99992311] yes=1,no=2,missing=1,gain=97812.25,cover=218986"
        private static readonly Regex decisionParser = 
            new Regex(@"^\[f(\d+)\<([^\]]+)\] yes=(\d+),no=(\d+),missing=\d+,gain=[^,]+,cover=(.*)$", RegexOptions.Compiled);
        private static DecisionTreeNode ParseDecision(string nodeInfo)
        {
            var parts = decisionParser.Match(nodeInfo);
            return new DecisionTreeNode
            {
                FeatureIndex = short.Parse(parts.Groups[1].Value),
                Value = float.Parse(parts.Groups[2].Value),
                TrueBranch = byte.Parse(parts.Groups[3].Value),
                FalseBranch = byte.Parse(parts.Groups[4].Value),
                //Cover = float.Parse(parts.Groups[5].Value),
            };
        }
    }

    public sealed class XGBoostNaive
    {
        private readonly DecisionTreeNaive[] trees;

        public XGBoostNaive(DecisionTreeNaive[] trees)
        {
            this.trees = trees;
        }

        public double EvaluateProbability(double[] instance)
        {
            var sum = trees.Sum(t => t.Evaluate(instance));
            var result = Logit(sum);
            return result;
        }

        // this converts the output to a probability
        private double Logit(double value)
        {
            return 1f / (1f + Math.Exp(-value));
        }

        private static readonly Regex treeSplit = new Regex(@"^booster\[\d+\]\r?\n", RegexOptions.Compiled | RegexOptions.Multiline);
        public static XGBoostNaive Create(string allTrees)
        {
            var treeStrings = treeSplit.Split(allTrees);
            var trees = treeStrings
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(DecisionTreeNaive.Create)
                .ToArray();
            var model = new XGBoostNaive(trees);
            return model;
        }
    }
}