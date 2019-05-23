using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace fast.modeling
{
    public class XGBoost
    {
        private DecisionTree[] trees;
        public DecisionTree[] Trees => trees;

        private XGBoost(DecisionTree[] trees)
        {
            this.trees = trees;
        }

        public double EvaluateProbability(double[] instance)
        {
            var sum = trees.Sum(m => m.Evaluate(instance));
            var result = Logit(sum);
            return result;
        }

        private double Logit(double value)
        {
            return 1d / (1d+Math.Exp(-value));
        }


        private static readonly Regex treeSplit = new Regex(@"^booster\[\d+\]\n", RegexOptions.Compiled | RegexOptions.Multiline);
        public static XGBoost Create(string allTrees)
        {
            var treeStrings = treeSplit.Split(allTrees);
            var trees = treeStrings
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(DecisionTree.Create)
                .ToArray();
            var model = new XGBoost(trees);
            return model;
        }
    }
}