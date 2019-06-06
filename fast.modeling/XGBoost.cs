using System;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private int FlatSize;

        public XGBoost(DecisionTree[] trees)
        {
            this.trees = trees;
        }

        private void PrepareCompiled()
        {
            this.compiledTrees = trees.Select(m => m.Compile()).ToArray();
        }

        private void PrepareFlat(int? flatSize = null)
        {
            if (flatSize.HasValue)
            {
                FlatSize = flatSize.Value;
            }
            else
            {
                var maxDepth = trees
                    .SelectMany(FeatureReorderer.GetAllPaths)
                    .Max(m => m.Length);
                FlatSize = (int)Math.Pow(2, maxDepth);
            }
            foreach(var tree in trees) tree.PrepareFlat(FlatSize);
            this.FlatValues = trees.SelectMany(m => m.Values).ToArray();
            this.FlatFeatureIndexes = trees.SelectMany(m => m.FeatureIndexes).ToArray();
        }

        private int shrinkTreeSize;
        private float[] FlatValuesShrunk;
        private short[] FlatFeatureIndexesShrunk;
        private void PrepareShrink(short jobFeaturesStart)
        {
            PrepareFlat();
            int maxJobOnlyPathLength = trees
                .SelectMany(FeatureReorderer.GetAllPaths)
                .Max(m => m.Count(j => j.FeatureIndex >= jobFeaturesStart));
            shrinkTreeSize = (int)Math.Pow(2, maxJobOnlyPathLength + 1); // the +1 is for the leaves
            this.FlatValuesShrunk = new float[shrinkTreeSize * trees.Length];
            this.FlatFeatureIndexesShrunk = new short[shrinkTreeSize * trees.Length];
        }
        private (float, int) ShrinkTrees(float[] valuesBuffer, short[] featureIndexesBuffer, float[] userFeatures)
        {
            float baseline = 0f;
            int shrinkOffset = 0;
            for(int flatOffset = 0; flatOffset < FlatFeatureIndexes.Length; flatOffset += FlatSize)
            {
                (bool hasNodes, float terminatedValue) = 
                    ShrinkTree(valuesBuffer, featureIndexesBuffer, userFeatures, flatOffset, shrinkOffset, 0, 0);

                if (hasNodes) shrinkOffset += shrinkTreeSize;
                else baseline += terminatedValue;
            }
            return (baseline, shrinkOffset);
        }
        private (bool hasNodes, float baseline) ShrinkTree(
            float[] valuesBuffer, 
            short[] featureIndexesBuffer, 
            float[] userFeatures,
            int flatOffset,
            int shrinkOffset,
            int flatIndex,
            int shrinkIndex
        )
        {
            short featIndex = FlatFeatureIndexes[flatOffset + flatIndex];
            float value = FlatValues[flatOffset + flatIndex];
            // it's a leaf
            if (featIndex == -1) 
            {
                // set the value
                // if it's a necessary leaf it's in the right place
                // if it's not it will get overwritten
                FlatFeatureIndexesShrunk[shrinkOffset+shrinkIndex] = featIndex;
                FlatValuesShrunk[shrinkOffset+shrinkIndex] = value;
                return (false, value);
            }
            // it's a user feature we can eliminate
            if (featIndex < userFeatures.Length)
            {
                // we're replacing so we don't increment the shrink index
                if (userFeatures[featIndex] < value)
                    flatIndex = flatIndex * 2 + 1;
                else
                    flatIndex = flatIndex * 2 + 2;
                return ShrinkTree(valuesBuffer, featureIndexesBuffer, userFeatures, flatOffset, shrinkOffset, flatIndex, shrinkIndex);
            }
            // it's a job feature we need to build out each side
            FlatFeatureIndexesShrunk[shrinkOffset+shrinkIndex] = (short)(featIndex - userFeatures.Length);
            FlatValuesShrunk[shrinkOffset+shrinkIndex] = value;
            flatIndex = flatIndex * 2 + 1;
            shrinkIndex = shrinkIndex * 2 + 1;
            ShrinkTree(valuesBuffer, featureIndexesBuffer, userFeatures, flatOffset, shrinkOffset, flatIndex, shrinkIndex);
            flatIndex++;
            shrinkIndex++;
            ShrinkTree(valuesBuffer, featureIndexesBuffer, userFeatures, flatOffset, shrinkOffset, flatIndex, shrinkIndex);
            return (true, 0f);
        }
        public float EvaluateProbabilityFlatShrink(float[] userFeatures, float[] jobFeatures)
        {
            (float baseline, int maxTreeIndex) = ShrinkTrees(FlatValuesShrunk, FlatFeatureIndexesShrunk, userFeatures);
            var sum = baseline;
            for (int offset = 0; offset < maxTreeIndex; offset += shrinkTreeSize)
            {
                sum += EvaluateFlatShrink(jobFeatures, offset);
            }
            var result = Logit(sum);
            return result;
        }
        public void EvaluateProbabilityFlatShrink(float[] userFeatures, float[][] jobFeatures, float[] results)
        {
            (float baseline, int maxTreeIndex) = ShrinkTrees(FlatValuesShrunk, FlatFeatureIndexesShrunk, userFeatures);
            for(int j = 0; j < jobFeatures.Length; j++)
            {
                float[] jobFeats = jobFeatures[j];
                var sum = baseline;
                for (int offset = 0; offset < maxTreeIndex; offset += shrinkTreeSize)
                {
                    sum += EvaluateFlatShrink(jobFeats, offset);
                }
                results[j] = sum;
            }
            Logit(results);
        }
        private float EvaluateFlatShrink(float[] features, int offset)
        {
            int index = 0;
            int featIndex = FlatFeatureIndexesShrunk[offset + index];
            while (featIndex != -1)
            {
                if (features[featIndex] < FlatValuesShrunk[offset + index])
                {
                    index = index * 2 + 1;
                }
                else
                {
                    index = index * 2 + 2;
                }
                featIndex = FlatFeatureIndexesShrunk[offset + index];
            }
            return FlatValuesShrunk[offset + index];
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
            Logit(sums);
            return sums;
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
            Logit(sums);
            return sums;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float Logit(float value)
        {
            return 1f / (1f + (float)Math.Exp(-value));
        }

        private void Logit(float[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Logit((float)values[i]);
            }
        }


        private static readonly Regex treeSplit = new Regex(@"^booster\[\d+\]\r?\n", RegexOptions.Compiled | RegexOptions.Multiline);
        public static XGBoost Create(string allTrees, bool prepareCompiled = false, bool prepareFlat = false, bool prepareShrink = false)
        {
            var treeStrings = treeSplit.Split(allTrees);
            var trees = treeStrings
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(DecisionTree.Create)
                .ToArray();
            var model = new XGBoost(trees);
            if (prepareCompiled) model.PrepareCompiled();
            if (prepareFlat) model.PrepareFlat();
            if (prepareShrink) model.PrepareShrink(634); // HACK remove this constant
            return model;
        }
    }
}