using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace fast.modeling
{
    public class DecisionTree
    {
        private struct DecisionTreeNode
        {
            public int FeatureIndex { get; set; }
            public double Value { get; set; }
            public int TrueBranch { get; set; }
            public int FalseBranch { get; set; }
        }

        private DecisionTreeNode[] nodes;

        private DecisionTree(DecisionTreeNode[] tree)
        {
            nodes = tree;
        }

        public override string ToString()
        {
            var lines = nodes.Select((m, i) => {
                if (m.FeatureIndex == LeafIndex) return $"{i}:leaf=" + m.Value;
                else return $"{i}:[f{m.FeatureIndex}<{m.Value}] yes={m.TrueBranch},no={m.FalseBranch}";
            });
            return string.Join("\n", lines);
        }

        public double Evaluate(double[] features)
        {
            var node = this.nodes[0];
            while(node.FeatureIndex != LeafIndex)
            {
                int nodeIndex = features[node.FeatureIndex] < node.Value ? node.TrueBranch : node.FalseBranch;
                node = nodes[nodeIndex];
            }
            return node.Value;
        }

        public static DecisionTree Create(string definition)
        {
            var lines = definition.Split('\n');
            var nodes = lines
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(ParseLine)
                .OrderBy(m => m.index)
                .Select(m => m.node)
                .ToArray();
            var dt = new DecisionTree(nodes);
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
        private static readonly Regex leafParser = new Regex(@"^leaf=([^,]+)", RegexOptions.Compiled);
        private const int LeafIndex = -1;
        private static DecisionTreeNode ParseLeaf(string nodeInfo)
        {
            var parts = leafParser.Match(nodeInfo);
            return new DecisionTreeNode
            { 
                FeatureIndex = LeafIndex, 
                Value = double.Parse(parts.Groups[1].Value)  
            };
        }
        // decision example: "[f0<0.99992311] yes=1,no=2,missing=1,gain=97812.25,cover=218986"
        private static readonly Regex decisionParser = new Regex(@"^\[f(\d+)\<([^\]]+)\] yes=(\d+),no=(\d+)", RegexOptions.Compiled);
        private static DecisionTreeNode ParseDecision(string nodeInfo)
        {
            var parts = decisionParser.Match(nodeInfo);
            return new DecisionTreeNode
            {
                FeatureIndex = int.Parse(parts.Groups[1].Value),
                Value = double.Parse(parts.Groups[2].Value),
                TrueBranch = int.Parse(parts.Groups[3].Value),
                FalseBranch = int.Parse(parts.Groups[4].Value),
            };
        }
    }
}
