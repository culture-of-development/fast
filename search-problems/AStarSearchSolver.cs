using System;
using System.Collections.Generic;
using System.Linq;

namespace search_problems
{
    public class AStarSearchSolver : ISearchAlgorithm
    {
        public ulong StatesEvaluated { get; private set; }
        public int MaxDepth { get; private set; }

        public NPuzzle.Location[] Solve(NPuzzle puzzle)
        {
            StatesEvaluated = 0UL;
            MaxDepth = 0;

            IPriorityQueue<int, VisitedState> queue = new MinHeap<int, VisitedState>(10*1024*1024);
            queue.Push(-1, new VisitedState(puzzle, null, null, 0));
            while(!queue.IsEmpty)
            {
                var top = queue.Pop();
                StatesEvaluated++;

                var state = top.state;
                if (state.IsGoal())
                {
                    return RebuildSolution(top);
                }

                var moves = state.ExpandMoves();
                foreach(var move in moves)
                {
                    var successor = state.MoveCopy(move);
                    var totalCost = top.cost + NPuzzle.StepCost + successor.HammingDistance();
                    MaxDepth = Math.Max(MaxDepth, top.cost + NPuzzle.StepCost);
                    queue.Push(totalCost, new VisitedState(successor, move, top, top.cost + NPuzzle.StepCost));
                }
            }
            return null;
        }

        private NPuzzle.Location[] RebuildSolution(VisitedState goal)
        {
            var solution = new List<NPuzzle.Location>();
            while(goal.lastMove != null)
            {
                solution.Add(goal.lastMove);
                goal = goal.previous;
            }
            solution.Reverse();
            return solution.ToArray();
        }

        private class VisitedState
        {
            public NPuzzle state { get; set; }
            public NPuzzle.Location lastMove { get; set; }
            public VisitedState previous { get; set; }
            public int cost { get; set; }

            public VisitedState(NPuzzle state, NPuzzle.Location lastMove, VisitedState previous, int cost)
            {
                this.state = state;
                this.lastMove = lastMove;
                this.previous = previous;
                this.cost = cost;
            }
        }
    }
}