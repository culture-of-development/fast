using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sigil;

namespace fast.modeling
{
    public sealed class DecisionTree
    {
        public struct DecisionTreeNode
        {
            public float Value;
            public short FeatureIndex;
            public byte TrueBranch;
            public byte FalseBranch;

            //public float Cover;
        }

        private DecisionTreeNode[] nodes;
        public DecisionTreeNode[] Nodes => nodes;
        private DecisionTreeNode first;

        public float[] Values;
        public short[] FeatureIndexes;

        public DecisionTree(DecisionTreeNode[] tree)
        {
            nodes = tree;
            first = tree[0];
        }

        public void PrepareFlat(int flatSize)
        {
            Values = new float[flatSize];
            FeatureIndexes = new short[flatSize];
            Expand(first, 0);
        }
        private void Expand(DecisionTreeNode node, int index)
        {
            Values[index] = node.Value;
            FeatureIndexes[index] = node.FeatureIndex;
            if (node.FeatureIndex == LeafIndex) return;
            Expand(nodes[node.TrueBranch], index * 2 + 1);
            Expand(nodes[node.FalseBranch], index * 2 + 2);
        }

        public override string ToString()
        {
            var lines = nodes.Select((m, i) => {
                if (m.FeatureIndex == LeafIndex) return $"{i}:leaf=" + m.Value;
                else return $"{i}:[f{m.FeatureIndex}<{m.Value}] yes={m.TrueBranch},no={m.FalseBranch}";
            });
            return string.Join("\n", lines);
        }

        public float Evaluate(float[] features)
        {
            var node = this.first;
            while(node.FeatureIndex != LeafIndex)
            {
                var f = features[node.FeatureIndex];
                int nodeIndex = f < node.Value ? node.TrueBranch : node.FalseBranch;
                node = nodes[nodeIndex]; // TODO: try removing array and using object graph
            }
            return node.Value;
        }

        public float EvaluateFlat(float[] features)
        {
            int index = 0;
            int featIndex = FeatureIndexes[index];
            while (featIndex != LeafIndex)
            {
                var f = features[featIndex];
                if (features[featIndex] < Values[index])
                {
                    index = index * 2 + 1;
                }
                else
                {
                    index = index * 2 + 2;
                }
                featIndex = FeatureIndexes[index];
            }
            return Values[index];
        }

        public Func<float[], float> Compile()
        {
            var e1 = Emit<Func<float[], float>>.NewDynamicMethod();
            var stack = new Stack<(Label, DecisionTree.DecisionTreeNode)>();
            stack.Push((e1.DefineLabel("node_0"), nodes[0]));
            while(stack.Count > 0)
            {
                var (label, node) = stack.Pop();
                e1.MarkLabel(label);
                if (node.FeatureIndex == -1)
                {
                    // we're in a leaf, do a return
                    e1.LoadConstant(node.Value);
                    e1.Return();
                }
                else
                {
                    // create the the child labels
                    // create the branch
                    // push the children
                    var leftLabel = e1.DefineLabel("node_" + node.TrueBranch);
                    var rightLabel = e1.DefineLabel("node_" + node.FalseBranch);
                    e1.LoadArgument(0);
                    e1.LoadConstant(node.FeatureIndex);
                    e1.LoadElement(typeof(float));
                    e1.LoadConstant(node.Value);
                    e1.BranchIfGreater(rightLabel);
                    stack.Push((rightLabel, nodes[node.FalseBranch]));
                    stack.Push((leftLabel, nodes[node.TrueBranch]));
                }
            }
            var del = e1.CreateDelegate();
            return del;
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
        private static readonly Regex leafParser = new Regex(@"^leaf=([^,]+),cover=(.*)$", RegexOptions.Compiled);
        public const int LeafIndex = -1;
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
}
