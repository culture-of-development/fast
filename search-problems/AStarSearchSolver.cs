using System;
using System.Collections.Generic;

namespace search_problems
{
    public class AStarSearchSolver
    {
        public NPuzzle.Location[] Solve(NPuzzle puzzle)
        {
            if (puzzle.IsGoal()) return new NPuzzle.Location[0];

            var queue = new SortedList<int, VisitedState>();
            queue.Add(-1, new VisitedState(puzzle, null, null, 0));
            while(queue.Count > 0)
            {
                var top = queue[0];
                queue.RemoveAt(0);

                var state = top.state;
                if (state.IsGoal())
                {
                    return RebuildSolution(top);
                }

                var moves = state.ExpandMoves();
                foreach(var move in moves)
                {
                    var successor = state.MoveCopy(move);
                    var totalCost = top.cost + NPuzzle.StepCost + 0;// successor.HammingDistance();
                    queue.Add(totalCost, new VisitedState(successor, move, top, top.cost + NPuzzle.StepCost));
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
                this.cost = 0;
            }
        }
    }
}