<Query Kind="Program">
  <NuGetReference>Sigil</NuGetReference>
  <Namespace>Sigil</Namespace>
</Query>

void Main()
{
	//TestThing();
	
	TestCompileTree();
}

void TestCompileTree()
{
	var dt = DecisionTree.Create(exampleDecisionTree);
	var e1 = Emit<Func<float[], float>>.NewDynamicMethod();
	var stack = new Stack<(Label, DecisionTree.DecisionTreeNode)>();
	stack.Push((e1.DefineLabel("node_0"), dt.Nodes[0]));
	while(stack.Count > 0)
	{
		var (label, node) = stack.Pop();
		e1.MarkLabel(label);
		if (node.FeatureIndex == -1)
		{
			// we're in a leaf, do a return
			e1.LoadConstant(node.Value);
			e1.Return();
		}
		else
		{
			// create the the child labels
			// create the branch
			// push the children
			var leftLabel = e1.DefineLabel("node_" + node.TrueBranch);
			var rightLabel = e1.DefineLabel("node_" + node.FalseBranch);
			e1.LoadArgument(0);
			e1.LoadConstant(node.FeatureIndex);
			e1.LoadElement(typeof(float));
			e1.LoadConstant(node.Value);
			e1.BranchIfGreater(rightLabel);
			stack.Push((leftLabel, dt.Nodes[node.TrueBranch]));
			stack.Push((rightLabel, dt.Nodes[node.FalseBranch]));
		}
	}
	var del = e1.CreateDelegate();

	var random = new Random();
	var features = Enumerable.Range(0, 850).Select(m => (float)random.NextDouble()).ToArray();
	features[0] *= 2;
	
	del(features).Dump("del");
	eval(features).Dump("eval");
}

void TestThing()
{
	var random = new Random();
	var features = Enumerable.Range(0, 850).Select(m => (float)random.NextDouble()).ToArray();
	features[0] *= 2;
	eval(features).Dump();
	var dt = DecisionTree.Create(exampleDecisionTree);
	
	var timer1 = Stopwatch.StartNew();
	var results = new float[1000];
	for(int i = 0; i < 100_000_000; i++)
	{
		results[i%1000] = eval(features);
	}
	timer1.Stop();

	var timer2 = Stopwatch.StartNew();
	var results2 = new float[1000];
	for (int i = 0; i < 100_000_000; i++)
	{
		results2[i % 1000] = dt.Evaluate(features);
	}
	timer2.Stop();
	
	timer1.Elapsed.TotalMilliseconds.Dump("func");
	timer2.Elapsed.TotalMilliseconds.Dump("dt");
}

float eval(float[] features)
{
	//0:[f0<0.99992311] yes=1,no=2,missing=1,gain=97812.25,cover=218986
	if (features[0] < 0.99992311f)
	{
		// 1:leaf=-0.199992761,cover=27584.75
		return -0.199992761f;
	}
	else
	{
		//2:[f17<0.000367681001] yes= 3,no = 4,missing = 3,gain = 10373.0732,cover = 191401.25
		if (features[17] < 0.000367681001f)
		{
			//3:[f2<0.5] yes=5,no=6,missing=5,gain=4121.85938,cover=103511.5
			if (features[2] < 0.5f)
			{
				//5:[f6<0.00216802233] yes=9,no=10,missing=9,gain=873.340759,cover=50533.25
				if (features[6] < 0.00216802233f)
				{
					//9:[f732<0.5] yes=17,no=18,missing=17,gain=515.368896,cover=33356.75
					if (features[6] < 0.00216802233f)
					{
						//17:leaf=-0.00213295687,cover=25503.5
						return -0.00213295687f;
					}
					else
					{
						//18:leaf=-0.0314288437,cover=7853.25
						return -0.0314288437f;
					}
				}
				else
				{
					//10:[f732<0.5] yes=19,no=20,missing=19,gain=276.522034,cover=17176.5
					if (features[732] < 0.5f)
					{
						//19:leaf=0.0253729131,cover=13474
						return 0.0253729131f;
					}
					else
					{
						//20:leaf=-0.00548130181,cover=3702.5
						return -0.00548130181f;
					}
				}
			}
			else
			{
				//6:[f734<0.237819463] yes=11,no=12,missing=11,gain=2141.23145,cover=52978.25
				if (features[6] < 0.237819463f)
				{
					//11:[f8<0.00104575348] yes=21,no=22,missing=21,gain=566.334961,cover=35689
					if (features[8] < 00104575348f)
					{
						//21:leaf=-0.0620479584,cover=24457.5
						return -0.0620479584f;
					}
					else
					{
						//22:leaf=-0.0349165387,cover=11231.5
						return -0.0349165387f;
					}
				}
				else
				{
					//12:[f762<0.308019817] yes=23,no=24,missing=23,gain=483.886719,cover=17289.25
					if (features[762] < 0.308019817f)
					{
						//23:leaf=-0.0144120604,cover=16450.5
						return -0.0144120604f;
					}
					else
					{
						//24:leaf=0.063411735,cover=838.75
						return 0.063411735f;
					}
				}
			}
		}
		else 
		{
			//4:[f2<0.5] yes=7,no=8,missing=7,gain=2694.23291,cover=87889.75
			if (features[2] < 0.5f)
			{
				//7:[f27<0.000739371637] yes=13,no=14,missing=13,gain=928.447266,cover=44100.5
				if (features[27] < 0.000739371637f)
				{
					//13:[f732<0.5] yes=25,no=26,missing=25,gain=285.069702,cover=17082.25
					if (features[732] < 0.5f)
					{
						//25:leaf=0.032621529,cover=13427.25
						return 0.032621529f;
					}
					else
					{
						//26:leaf = 0.00112144416,cover = 3655
						return 0.00112144416f;
					}
				}
				else
				{
					//14:[f285<0.000919258455] yes=27,no=28,missing=27,gain=421.745117,cover=27018.25
					if (features[285] < 0.000919258455f)
					{
						//27:leaf=0.0483669229,cover=20145
						return 0.0483669229f;
					}
					else
					{
						//28:leaf = 0.077062957,cover = 6873.25
						return 0.077062957f;
					}
				}
			}
			else
			{
				//8:[f734<0.103942066] yes=15,no=16,missing=15,gain=1591.2124,cover=43789.25
				if (features[734] < 0.103942066f)
				{
					//15:[f101<0.000240761583] yes=29,no=30,missing=29,gain=608.92157,cover=24192.75
					if (features[101] < 0.000240761583f)
					{
						//29:leaf=-0.0209285971,cover=14574.75
						return -0.0209285971f;
					}
					else
					{
						//30:leaf = 0.0114876805,cover = 9618
						return 0.0114876805f;
					}
				}
				else
				{
					//16:[f722<0.5] yes=31,no=32,missing=31,gain=601.422363,cover=19596.5
					if (features[722] < 0.5f)
					{
						//31:leaf=0.0258833747,cover=18429.75
						return 0.0258833747f;
					}
					else
					{
						//32:leaf = 0.099892959,cover = 1166.75
						return 0.099892959f;
					}
				}
			}
		}
	}
}

// Define other methods and classes here
const string exampleDecisionTree =
@"0:[f0<0.99992311] yes=1,no=2,missing=1,gain=97812.25,cover=218986
1:leaf=-0.199992761,cover=27584.75
2:[f17<0.000367681001] yes=3,no=4,missing=3,gain=10373.0732,cover=191401.25
3:[f2<0.5] yes=5,no=6,missing=5,gain=4121.85938,cover=103511.5
5:[f6<0.00216802233] yes=9,no=10,missing=9,gain=873.340759,cover=50533.25
9:[f732<0.5] yes=17,no=18,missing=17,gain=515.368896,cover=33356.75
17:leaf=-0.00213295687,cover=25503.5
18:leaf=-0.0314288437,cover=7853.25
10:[f732<0.5] yes=19,no=20,missing=19,gain=276.522034,cover=17176.5
19:leaf=0.0253729131,cover=13474
20:leaf=-0.00548130181,cover=3702.5
6:[f734<0.237819463] yes=11,no=12,missing=11,gain=2141.23145,cover=52978.25
11:[f8<0.00104575348] yes=21,no=22,missing=21,gain=566.334961,cover=35689
21:leaf=-0.0620479584,cover=24457.5
22:leaf=-0.0349165387,cover=11231.5
12:[f762<0.308019817] yes=23,no=24,missing=23,gain=483.886719,cover=17289.25
23:leaf=-0.0144120604,cover=16450.5
24:leaf=0.063411735,cover=838.75
4:[f2<0.5] yes=7,no=8,missing=7,gain=2694.23291,cover=87889.75
7:[f27<0.000739371637] yes=13,no=14,missing=13,gain=928.447266,cover=44100.5
13:[f732<0.5] yes=25,no=26,missing=25,gain=285.069702,cover=17082.25
25:leaf=0.032621529,cover=13427.25
26:leaf=0.00112144416,cover=3655
14:[f285<0.000919258455] yes=27,no=28,missing=27,gain=421.745117,cover=27018.25
27:leaf=0.0483669229,cover=20145
28:leaf=0.077062957,cover=6873.25
8:[f734<0.103942066] yes=15,no=16,missing=15,gain=1591.2124,cover=43789.25
15:[f101<0.000240761583] yes=29,no=30,missing=29,gain=608.92157,cover=24192.75
29:leaf=-0.0209285971,cover=14574.75
30:leaf=0.0114876805,cover=9618
16:[f722<0.5] yes=31,no=32,missing=31,gain=601.422363,cover=19596.5
31:leaf=0.0258833747,cover=18429.75
32:leaf=0.099892959,cover=1166.75
";


public sealed class DecisionTree
{
	public struct DecisionTreeNode
	{
		public float Value;
		public short FeatureIndex;
		public byte TrueBranch;
		public byte FalseBranch;

		//public float Cover;
	}

	private DecisionTreeNode[] nodes;
	public DecisionTreeNode[] Nodes => nodes;
	private DecisionTreeNode first;

	public DecisionTree(DecisionTreeNode[] tree)
	{
		nodes = tree;
		first = tree[0];
	}

	public override string ToString()
	{
		var lines = nodes.Select((m, i) =>
		{
			if (m.FeatureIndex == LeafIndex) return $"{i}:leaf=" + m.Value;
			else return $"{i}:[f{m.FeatureIndex}<{m.Value}] yes={m.TrueBranch},no={m.FalseBranch}";
		});
		return string.Join("\n", lines);
	}

	public float Evaluate(float[] features)
	{
		var node = this.first;
		while (node.FeatureIndex != LeafIndex)
		{
			var f = features[node.FeatureIndex];
			int nodeIndex = f < node.Value ? node.TrueBranch : node.FalseBranch;
			node = nodes[nodeIndex]; // TODO: try removing array and using object graph
		}
		return node.Value;
	}

	public static DecisionTree Create(string definition)
	{
		var lines = definition.Split('\n');
		var nodes = lines
			.Where(m => !string.IsNullOrWhiteSpace(m))
			.Select(ParseLine)
			.OrderBy(m => m.index)
			.Select(m => m.node)
			.ToArray();
		var dt = new DecisionTree(nodes);
		return dt;
	}
	private static (int index, DecisionTreeNode node) ParseLine(string line)
	{
		var parts = line.Split(':');
		var index = int.Parse(parts[0]);
		var nodeInfo = parts[1];
		var node = nodeInfo.StartsWith("leaf=") ? ParseLeaf(nodeInfo) : ParseDecision(nodeInfo);
		return (index, node);
	}
	// leaf example: "leaf=-0.199992761,cover=27584.75"
	private static readonly Regex leafParser = new Regex(@"^leaf=([^,]+),cover=(.*)$", RegexOptions.Compiled);
	public const int LeafIndex = -1;
	private static DecisionTreeNode ParseLeaf(string nodeInfo)
	{
		var parts = leafParser.Match(nodeInfo);
		return new DecisionTreeNode
		{
			FeatureIndex = LeafIndex,
			Value = float.Parse(parts.Groups[1].Value),
			//Cover = float.Parse(parts.Groups[2].Value),
		};
	}
	// decision example: "[f0<0.99992311] yes=1,no=2,missing=1,gain=97812.25,cover=218986"
	private static readonly Regex decisionParser =
		new Regex(@"^\[f(\d+)\<([^\]]+)\] yes=(\d+),no=(\d+),missing=\d+,gain=[^,]+,cover=(.*)$", RegexOptions.Compiled);
	private static DecisionTreeNode ParseDecision(string nodeInfo)
	{
		var parts = decisionParser.Match(nodeInfo);
		return new DecisionTreeNode
		{
			FeatureIndex = short.Parse(parts.Groups[1].Value),
			Value = float.Parse(parts.Groups[2].Value),
			TrueBranch = byte.Parse(parts.Groups[3].Value),
			FalseBranch = byte.Parse(parts.Groups[4].Value),
			//Cover = float.Parse(parts.Groups[5].Value),
		};
	}
}