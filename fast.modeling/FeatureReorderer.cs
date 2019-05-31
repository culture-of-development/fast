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

        public static XGBoost ReorderXGBoost(XGBoost model, short[] reorderMapping, int userFeaturesCount)
        {
            var reorderedTrees = model.Trees
                .Select(m => ReorderTree(m, reorderMapping))
                .ToArray();
            var results = new XGBoost(reorderedTrees, userFeaturesCount);
            return results;
        }

        private static DecisionTree ReorderTree(DecisionTree model, short[] reorderMapping)
        {
            var reorderedNodes = model.Nodes
                .Select(m => new DecisionTreeNode
                {
                    FeatureIndex = m.FeatureIndex < 0 ? m.FeatureIndex : reorderMapping[m.FeatureIndex],
                    Value = m.Value,
                    TrueBranch = m.TrueBranch,
                    FalseBranch = m.FalseBranch,
                })
                .ToArray();
            var result = new DecisionTree(reorderedNodes);
            return result;
        }

        public static short[] Greedy(XGBoost model, int treeCount)
        {
            Console.WriteLine("doing greedy search");
            var allPaths = model.Trees.Take(treeCount).SelectMany(GetAllPaths).ToArray();
            var nodeLookups = allPaths
                .SelectMany(m => m)
                .Distinct()
                .Where(m => m.FeatureIndex >= 0)
                .GroupBy(m => m.FeatureIndex)
                .ToDictionary(m => m.Key, m => m.ToArray());
            var featuresMax = allPaths.SelectMany(m => m.Select(j => j.FeatureIndex)).Max();
            for(short i = 0; (int)i <= featuresMax; i++)
            {
                if (!nodeLookups.ContainsKey(i))
                {
                    nodeLookups[i] = new[] { new DecisionTreeNode2 { FeatureIndex = i, OriginalIndex = i } };
                }
            }
            double best = EvalAveragePages(allPaths);
            int sinceImprovement = 0;
            const int maxImprovementDelay = 25_000;
            while (true)
            {
                Console.WriteLine(" loop");
                bool improved = false;
                for(int i = 0; i < featuresMax; i++)
                {
                    short ii = (short)i;
                    for(int j = 0; j < featuresMax; j++)
                    {
                        if (i==j) continue;
                        short jj = (short)j;
                        var nodesI = nodeLookups[ii];
                        var nodesJ = nodeLookups[jj];
                        // do swap
                        for(int x = 0; x < nodesI.Length; x++) nodesI[x].FeatureIndex = jj;
                        for(int x = 0; x < nodesJ.Length; x++) nodesJ[x].FeatureIndex = ii;
                        // eval score
                        double possible = EvalAveragePages(allPaths);
                        //Console.WriteLine($"  {i}, {j}, {best}, {possible}");
                        // set best if necessary
                        if (possible < best) 
                        {
                            best = possible;
                            nodeLookups[ii] = nodesJ;
                            nodeLookups[jj] = nodesI;
                            improved = true;
                            sinceImprovement = 0;
                            UpdateBestMap(nodeLookups);
                            Console.WriteLine($" new best: {best}");
                        }
                        else 
                        {
                            sinceImprovement++;
                            // else undo that shit
                            for(int x = 0; x < nodesI.Length; x++) nodesI[x].FeatureIndex = ii;
                            for(int x = 0; x < nodesJ.Length; x++) nodesJ[x].FeatureIndex = jj;
                        }
                        if (sinceImprovement > maxImprovementDelay) break;
                    }
                    if (sinceImprovement > maxImprovementDelay) break;
                }
                if (!improved || sinceImprovement > maxImprovementDelay) break;
            }
            UpdateBestMap(nodeLookups);
            return bestMap;
        }

        public static short[] bestMap = null;
        private static void UpdateBestMap(Dictionary<short, DecisionTreeNode2[]> nodeLookups)
        {
            var map = nodeLookups
                .SelectMany(m => m.Value)
                .GroupBy(m => m.OriginalIndex)
                .ToDictionary(m => m.Key, m => new HashSet<short>(m.Select(j => j.FeatureIndex)));
            if (map.Any(j => j.Value.Count > 1))
            {
                throw new Exception("bad map");
            }
            bestMap = map.OrderBy(m => m.Key).Select(m => m.Value.First()).ToArray();
        }

        public static float EvalAveragePages(DecisionTreeNode2[][] allPaths)
        {
            return allPaths
                .GroupBy(m => m[0])
                .ToDictionary(m => m.Key, m => m.ToArray())
                .Select(m => m.Value
                    .Sum(path => {
                        var pageCount = NumMemoryPages(path);
                        var weight = 1f; //path[path.Length-1].Cover / path[0].Cover;
                        var weightedPageCount = pageCount * weight;
                        return weightedPageCount;
                    })
                )
                .Average();
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
            public float Cover;
        }
        public static DecisionTreeNode2 MapNode(DecisionTreeNode node)
        {
            return new DecisionTreeNode2
            {
                FeatureIndex = node.FeatureIndex,
                OriginalIndex = node.FeatureIndex,
                //Cover = node.Cover,
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
            const int DecisionTreeNodeMemorySize = 16;
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