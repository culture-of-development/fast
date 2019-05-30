using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace fast.modeling
{
    public sealed class XGBoost
    {
        private DecisionTree[] trees;
        public DecisionTree[] Trees => trees;

        private Func<float[], float>[] compiledTrees;

        public XGBoost(DecisionTree[] trees)
        {
            this.trees = trees;
            this.compiledTrees = trees.Select(m => m.Compile()).ToArray();
        }

        public double EvaluateProbability(float[] instance)
        {
            var sum = 0f;
            for (int i = 0; i < trees.Length; i++)
            {
                sum += trees[i].Evaluate(instance);
            }
            var result = Logit(sum);
            return result;
        }

        public double EvaluateProbabilityCompiled(float[] instance)
        {
            var sum = 0f;
            for (int i = 0; i < compiledTrees.Length; i++)
            {
                sum += compiledTrees[i](instance);
            }
            var result = Logit(sum);
            return result;
        }

        public double[] EvaluateProbability(float[][] instances)
        {
            var sums = new float[instances.Length];
            for (int i = 0; i < trees.Length; i++)
            {
                var eval = trees[i];
                for(int j = 0; j < instances.Length; j++)
                {
                    sums[j] += eval.Evaluate(instances[i]);
                }
            }
            var result = Logit(sums);
            return result;
        }

        public double[] EvaluateProbabilityCompiled(float[][] instances)
        {
            var sums = new float[instances.Length];
            for (int i = 0; i < trees.Length; i++)
            {
                var eval = compiledTrees[i];
                for(int j = 0; j < instances.Length; j++)
                {
                    sums[j] += eval(instances[i]);
                }
            }
            var result = Logit(sums);
            return result;
        }

        private double Logit(double value)
        {
            return 1d / (1d+Math.Exp(-value));
        }

        private double[] Logit(float[] values)
        {
            // can probably vectorize this
            double[] result = new double[values.Length];
            for(int i = 0; i < values.Length; i++)
            {
                result[i] = Logit((double)values[i]);
            }
            return result;
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