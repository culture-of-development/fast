<Query Kind="Program">
  <Reference Relative="bin\Release\netstandard2.0\fast.modeling.dll">I:\culture-of-development\fast\fast.modeling\bin\Release\netstandard2.0\fast.modeling.dll</Reference>
  <Namespace>fast.modeling</Namespace>
</Query>

void Main()
{
	var filename = @"I:\culture-of-development\fast\datasets\xgboost\model_xbg_trees.txt";
	var treesString = File.ReadAllText(filename);
	var model = XGBoost.Create(treesString, 640);
	
	var allPaths = model.Trees.SelectMany(GetAllPaths).ToArray();
	
	allPaths.GroupBy(m => m.Length).ToDictionary(m => m.Key, m => m.Count()).OrderBy(m => m.Key).Dump();
	
	model.Trees
		.GroupBy(m => m.Nodes.Count(j => j.FeatureIndex != -1))
		.ToDictionary(m => m.Key, m => m.Count())
		.OrderBy(m => m.Key)
		.Dump();
	
//	var treeFeatures = model.Trees
//		.Select(m => m.Nodes
//			.Select(j => j.FeatureIndex)
//			.Where(j => j != -1)
//			.Distinct()
//			.ToArray()
//		)
//		.ToArray();
		
	//treeFeatures.SelectMany(m => m).Distinct().Count().Dump("features used in the model");
	
//	var featureTrees = treeFeatures
//		.SelectMany((m, ti) => m.Select(fi => (fi, ti)))
//		.GroupBy(m => m.fi)
//		.ToDictionary(m => m.Key, m => m.ToArray());
		
	//PerfMetric(treeFeatures).Dump();
//	featureTrees
//		.Select(kvp => new { 
//			featureIndex = kvp.Key, 
//			treeCount = kvp.Value.Length,
//		})
//		.Dump();

	//treeFeatures.Dump();
}

// Define other methods and classes here
public static IEnumerable<DecisionTree.DecisionTreeNode[]> GetAllPaths(DecisionTree tree)
{
	var results = new List<DecisionTree.DecisionTreeNode[]>();
	var currentPath = new List<DecisionTree.DecisionTreeNode>();
	ExpandPaths(results, currentPath, tree.Nodes, tree.Nodes[0]);
	return results;
}
public static void ExpandPaths(
	List<DecisionTree.DecisionTreeNode[]> results, 
	List<DecisionTree.DecisionTreeNode> currentPath, 
	DecisionTree.DecisionTreeNode[] nodes, 
	DecisionTree.DecisionTreeNode currentNode
)
{
	var n = currentNode;
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