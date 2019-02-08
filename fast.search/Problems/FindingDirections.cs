using System;
using System.Collections.Generic;

namespace fast.search.problems
{
    public class FindingDirectionsProblem : IProblem<FindingDirectionsState>
    {
        // this is essentially just a graph
        // we're going to use an adjacency list to keep track of all edges
        private IWeightedGraph<FindingDirectionsState, double> map;
        private FindingDirectionsState start;
        private FindingDirectionsState end;

        public FindingDirectionsProblem(IWeightedGraph<FindingDirectionsState, double> map, FindingDirectionsState start, FindingDirectionsState end)
        {
            this.map = map;
            this.start = start;
            this.end = end;
        }

        public (FindingDirectionsState, double) ApplyAction(FindingDirectionsState state, IProblemAction action)
        {
            var nextState = (FindingDirectionsState)action;
            var distance = map.GetEdgeWeight(state, nextState);
            return (nextState, distance);
        }

        public IEnumerable<IProblemAction> Expand(FindingDirectionsState state) => map.GetNeighbors(state);
        public FindingDirectionsState GetInitialState() => start;
        public bool IsGoal(FindingDirectionsState state) => state == end;
    }

    public class FindingDirectionsState : IProblemState<FindingDirectionsState>, IProblemAction, IGraphNode
    {
        public string LocationName { get; private set; }
        public ulong NodeId { get; private set; }

        public bool Equals(FindingDirectionsState other)
        {
            return this.NodeId == other.NodeId;
        }
    }
}