using System;
using System.Collections.Generic;
using System.Linq;

using DecisionTreeNode = fast.modeling.DecisionTree.DecisionTreeNode;

namespace fast.modeling
{
    public static class FeatureReorderer
    {
        // attempt to reorder features to improve cache locality
        public static int[] CreateFeatureReorderingMap(XGBoost model)
        {
            var allPaths = model.Trees.SelectMany(GetAllPaths).ToArray();
            throw new NotImplementedException();
        }

        public static short[] Greedy(XGBoost model)
        {
            Console.WriteLine("doing greedy search");
            var allPaths = model.Trees.SelectMany(GetAllPaths).ToArray();
            var nodeLookups = allPaths
                .SelectMany(m => m)
                .Distinct()
                .GroupBy(m => m.FeatureIndex)
                .ToDictionary(m => m.Key, m => m.ToArray());
            var features = allPaths.SelectMany(m => m.Select(j => j.FeatureIndex)).Distinct().ToArray();
            for(short i = 0; (int)i < features.Max(); i++)
            {
                if (!nodeLookups.ContainsKey(i))
                {
                    nodeLookups[i] = new[] { new DecisionTreeNode2 { FeatureIndex = i, OriginalIndex = i } };
                }
            }
            double best = allPaths.Average(NumMemoryPages);
            int sinceImprovement = 0;
            const int maxImprovementDelay = 1_000;
            while (true)
            {
                Console.WriteLine(" loop");
                bool improved = false;
                for(int i = 0; i < features.Length; i++)
                {
                    short ii = (short)i;
                    if (!nodeLookups.ContainsKey(ii)) continue;
                    for(int j = i + 1; j < features.Length; j++)
                    {
                        short jj = (short)j;
                        if (!nodeLookups.ContainsKey(jj)) continue;
                        var nodesI = nodeLookups[ii];
                        var nodesJ = nodeLookups[jj];
                        // do swap
                        for(int x = 0; x < nodesI.Length; x++) nodesI[x].FeatureIndex = jj;
                        for(int x = 0; x < nodesJ.Length; x++) nodesJ[x].FeatureIndex = ii;
                        // eval score
                        double possible = allPaths.Average(NumMemoryPages);
                        //Console.WriteLine($"  {i}, {j}, {best}, {possible}");
                        // set best if necessary
                        if (possible < best) 
                        {
                            best = possible;
                            nodeLookups[ii] = nodesJ;
                            nodeLookups[jj] = nodesI;
                            improved = true;
                            sinceImprovement = 0;
                            Console.WriteLine($" new best: {best}");
                        }
                        else 
                        {
                            sinceImprovement++;
                            // else undo that shit
                            for(int x = 0; x < nodesI.Length; x++) nodesI[x].FeatureIndex = jj;
                            for(int x = 0; x < nodesJ.Length; x++) nodesJ[x].FeatureIndex = ii;
                        }
                        if (sinceImprovement > maxImprovementDelay) break;
                    }
                    if (sinceImprovement > maxImprovementDelay) break;
                }
                if (!improved || sinceImprovement > maxImprovementDelay) break;
            }
            var map = nodeLookups
                .SelectMany(m => m.Value)
                .GroupBy(m => m.OriginalIndex)
                .ToDictionary(m => m.Key, m => new HashSet<short>(m.Select(j => j.FeatureIndex)));
            if (map.Any(j => j.Value.Count > 1))
            {
                throw new Exception("bad map");
            }
            var result = map.OrderBy(m => m.Key).Select(m => m.Value.First()).ToArray();
            return result;
        }

        public static IEnumerable<DecisionTreeNode2[]> GetAllPaths(DecisionTree tree)
        {
            var results = new List<DecisionTreeNode2[]>();
            var currentPath = new List<DecisionTreeNode2>();
            ExpandPaths(results, currentPath, tree.Nodes, tree.Nodes[0]);
            return results;
        }

        public class DecisionTreeNode2
        {
            public short FeatureIndex;
            public short OriginalIndex;
        }
        public static DecisionTreeNode2 MapNode(DecisionTreeNode node)
        {
            return new DecisionTreeNode2
            {
                FeatureIndex = node.FeatureIndex,
                OriginalIndex = node.FeatureIndex,
            };
        }

        public static void ExpandPaths(List<DecisionTreeNode2[]> results, List<DecisionTreeNode2> currentPath, DecisionTreeNode[] nodes, DecisionTreeNode currentNode)
        {
            var n = MapNode(currentNode);
            currentPath.Add(n);
            if (currentNode.FeatureIndex == DecisionTree.LeafIndex)
            {
                results.Add(currentPath.ToArray());
            }
            else
            {
                ExpandPaths(results, currentPath, nodes, nodes[currentNode.TrueBranch]);
                ExpandPaths(results, currentPath, nodes, nodes[currentNode.FalseBranch]);
            }
            currentPath.Remove(n);
        }

        public static int NumMemoryPages(DecisionTreeNode2[] path)
        {
            // this was determined using unsafe sizeof
            // this approach is very optimistic making assumptions of
            // aligned memory and all that, might want to force that later
            const int DecisionTreeNodeMemorySize = 8;
            // the goal here is to be able to estimate the number of pages
            // swapped out during a typical pass through all trees
            // this isn't quite there, instead opting to just check number
            // of pages for each tree individually which is a bit of a greedy approach
            return path
                .Where(m => m.FeatureIndex >= 0)
                .Select(m => m.FeatureIndex - (m.FeatureIndex % DecisionTreeNodeMemorySize))
                .Distinct()
                .Count();
        }
    }
}