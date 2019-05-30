using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace fast.modeling
{
    public sealed class DecisionTreeFloat
    {
        const int featureIndex = 0;
        const int value = 1;
        const int trueBranch = 2;
        const int falseBranch = 3;
        private float[] nodes;
        
        public override string ToString()
        {
            var lines = new List<string>();
            for(int i = 0; i < nodes.GetLength(0); i++)
            {
                int t = i*nodes.GetLength(1);
                if (nodes[t+0] == LeafIndex) lines.Add($"{i}:leaf=" + nodes[t+value]);
                else lines.Add($"{i}:[f{(int)nodes[t+featureIndex]}<{nodes[t+value]}] yes={(int)nodes[t+trueBranch]},no={(int)nodes[t+falseBranch]}");
            }
            return string.Join("\n", lines);
        }

        public float Evaluate(float[] features)
        {
            int node = 0;
            while(nodes[node+featureIndex] != LeafIndex)
            {
                if (features[(int)nodes[node+featureIndex]] < nodes[node+value])
                {
                    node = (int)nodes[node+trueBranch];
                } 
                else
                {
                    node = (int)nodes[node+falseBranch];
                }
            }
            return nodes[node+value];
        }

        private DecisionTreeFloat(float[] nodes)
        {
            this.nodes = nodes;
        }

        private static readonly Regex leafParser = new Regex(@"^leaf=([^,]+),cover=(.*)$", RegexOptions.Compiled);
        private static readonly Regex decisionParser = new Regex(@"^\[f(\d+)\<([^\]]+)\] yes=(\d+),no=(\d+),missing=\d+,gain=[^,]+,cover=(.*)$", RegexOptions.Compiled);
        public const float LeafIndex = -1f;
        public static DecisionTreeFloat Create(string definition)
        {
            var lines = definition.Split('\n');
            var nodes = new float[lines.Length*4];
            foreach(var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var ni_parts = line.Split(':');
                var index = int.Parse(ni_parts[0])*4;
                var nodeInfo = ni_parts[1];
                if (nodeInfo.StartsWith("leaf="))
                {
                    var parts = leafParser.Match(nodeInfo);
                    nodes[index+featureIndex] = LeafIndex;
                    nodes[index+value] = float.Parse(parts.Groups[1].Value);
                }
                else
                {
                    var parts = decisionParser.Match(nodeInfo);
                    nodes[index+featureIndex] = (float)int.Parse(parts.Groups[1].Value);
                    nodes[index+value] = float.Parse(parts.Groups[2].Value);
                    nodes[index+trueBranch] = (float)int.Parse(parts.Groups[3].Value)*4;
                    nodes[index+falseBranch] = (float)int.Parse(parts.Groups[4].Value)*4;
                }
            }
            var dt = new DecisionTreeFloat(nodes);
            return dt;
        }
    }
}
