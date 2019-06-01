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

        private float[] FlatValues;
        private short[] FlatFeatureIndexes;
        private const int FlatSize = DecisionTree.FlatSize;

        public XGBoost(DecisionTree[] trees)
        {
            this.trees = trees;
            this.compiledTrees = trees.Select(m => m.Compile()).ToArray();

            this.FlatValues = trees.SelectMany(m => m.Values).ToArray();
            this.FlatFeatureIndexes = trees.SelectMany(m => m.FeatureIndexes).ToArray();
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

        public float EvaluateProbabilityFlat(float[] instance)
        {
            var sum = 0f;
            for (int offset = 0; offset < FlatFeatureIndexes.Length; offset += FlatSize)
            {
                sum += EvaluateFlat(instance, offset);
            }
            var result = Logit(sum);
            return result;
        }
        private float EvaluateFlat(float[] features, int offset)
        {
            int index = 0;
            int featIndex = FlatFeatureIndexes[offset + index];
            while (featIndex != -1)
            {
                if (features[featIndex] < FlatValues[offset + index])
                {
                    index = index * 2 + 1;
                }
                else
                {
                    index = index * 2 + 2;
                }
                featIndex = FlatFeatureIndexes[offset + index];
            }
            return FlatValues[offset + index];
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

        public float[] EvaluateProbability(float[][] instances)
        {
            var sums = new float[instances.Length];
            for (int i = 0; i < trees.Length; i++)
            {
                var eval = trees[i];
                for(int j = 0; j < instances.Length; j++)
                {
                    sums[j] += eval.Evaluate(instances[j]);
                }
            }
            var result = Logit(sums);
            return result;
        }

        public float[] EvaluateProbabilityCompiled(float[][] instances)
        {
            var sums = new float[instances.Length];
            for (int i = 0; i < trees.Length; i++)
            {
                var eval = compiledTrees[i];
                for(int j = 0; j < instances.Length; j++)
                {
                    sums[j] += eval(instances[j]);
                }
            }
            var result = Logit(sums);
            return result;
        }

        private float Logit(float value)
        {
            return 1f / (1f + (float)Math.Exp(-value));
        }

        private float[] Logit(float[] values)
        {
            // can probably vectorize this
            var result = new float[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = Logit((float)values[i]);
            }
            return result;
        }


        private static readonly Regex treeSplit = new Regex(@"^booster\[\d+\]\r?\n", RegexOptions.Compiled | RegexOptions.Multiline);
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