<Query Kind="Program">
  <Reference Relative="bin\Release\netstandard2.0\fast.modeling.dll">I:\culture-of-development\fast\fast.modeling\bin\Release\netstandard2.0\fast.modeling.dll</Reference>
  <Namespace>fast.modeling</Namespace>
</Query>

void Main()
{
	var filename = @"I:\culture-of-development\fast\datasets\xgboost\model_xbg_trees.txt";
	var treesString = File.ReadAllText(filename);
	var model = XGBoost.Create(treesString);
	
	var treeFeatures = model.Trees
		.Select(m => m.Nodes
			.Select(j => j.FeatureIndex)
			.Where(j => j != -1)
			.Distinct()
			.ToArray()
		)
		.ToArray();
		
	treeFeatures.SelectMany(m => m).Distinct().Count().Dump("features used in the model");
	
	var featureTrees = treeFeatures
		.SelectMany((m, ti) => m.Select(fi => (fi, ti)))
		.GroupBy(m => m.fi)
		.ToDictionary(m => m.Key, m => m.ToArray());
		
	PerfMetric(treeFeatures).Dump();
//	featureTrees
//		.Select(kvp => new { 
//			featureIndex = kvp.Key, 
//			treeCount = kvp.Value.Length,
//		})
//		.Dump();

	treeFeatures.Dump();
}

// Define other methods and classes here
double PerfMetric(XGBoost model)
{
	return model.Trees.SelectMany(ExpandAllPaths).Average();
}
List<double> ExpandAllPaths(DecisionTree tree)
{
	var pathPageWeights = new List<double>();
	// TODO: expand the paths, collect the page weights
	return pathPageWeights;
}
