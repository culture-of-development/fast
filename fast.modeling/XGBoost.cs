using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Numerics;

namespace fast.modeling
{
    public sealed class XGBoost
    {
        private DecisionTree[] trees;
        public DecisionTree[] Trees => trees;

        private Func<float[], float[], float>[] compiledTrees;

        public XGBoost(DecisionTree[] trees, int userFeaturesCount)
        {
            this.trees = trees;
            this.compiledTrees = trees.Select(m => m.Compile(userFeaturesCount)).ToArray();
        }

        public double EvaluateProbability(float[] userFeatures, float[] instance)
        {
            var sum = 0f;
            for (int i = 0; i < trees.Length; i++)
            {
                sum += trees[i].Evaluate(userFeatures, instance);
            }
            var result = Logit(sum);
            return result;
        }

        public double EvaluateProbabilityCompiled(float[] userFeatures, float[] instance)
        {
            var sum = 0f;
            for (int i = 0; i < compiledTrees.Length; i++)
            {
                sum += compiledTrees[i](userFeatures, instance);
            }
            var result = Logit(sum);
            return result;
        }

        public void EvaluateProbability(float[] userFeatures, float[][] instances, double[] resultBuffer)
        {
            for (int i = 0; i < trees.Length; i++)
            {
                var eval = trees[i];
                for(int j = 0; j < instances.Length; j++)
                {
                    resultBuffer[j] += eval.Evaluate(userFeatures, instances[i]);
                }
            }
            Logit(resultBuffer);
        }

        public void EvaluateProbabilityCompiled(float[] userFeatures, float[][] jobFeatures, double[] resultBuffer)
        {
            for (int i = 0; i < trees.Length; i++)
            {
                var eval = compiledTrees[i];
                for(int j = 0; j < jobFeatures.Length; j++)
                {
                    resultBuffer[j] += eval(userFeatures, jobFeatures[i]);
                }
            }
            Logit(resultBuffer);
        }

        private double Logit(double value)
        {
            return 1d / (1d+Math.Exp(-value));
        }

        private void Logit(double[] values)
        {
            for(int i = 0; i < values.Length; i++)
            {
                values[i] = Logit(values[i]);
            }
        }


        private static readonly Regex treeSplit = new Regex(@"^booster\[\d+\]\r?\n", RegexOptions.Compiled | RegexOptions.Multiline);
        public static XGBoost Create(string allTrees, int userFeaturesCount)
        {
            var treeStrings = treeSplit.Split(allTrees);
            var trees = treeStrings
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(DecisionTree.Create)
                .ToArray();
            var model = new XGBoost(trees, userFeaturesCount);
            return model;
        }
    }
}