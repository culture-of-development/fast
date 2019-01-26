using System;
using System.Collections.Generic;

namespace search_problems
{
    public class BreadthFirstSearchSolver : ISearchAlgorithm
    {
        public ulong StatesEvaluated { get; private set; }

        public NPuzzle.Location[] Solve(NPuzzle puzzle)
        {
            StatesEvaluated = 0UL;
            List<(NPuzzle state, NPuzzle.Location move, int cameFrom)> queue = new List<(NPuzzle, NPuzzle.Location, int)>();
            queue.Add((puzzle, null, -1));
            for(int i = 0; i < queue.Count; i++)
            {
                var (state, lastMove, cameFrom) = queue[i];
                this.StatesEvaluated++;
                var moves = state.ExpandMoves();
                foreach(var move in moves)
                {
                    var successor = state.MoveCopy(move);
                    if (successor.IsGoal())
                    {
                        return RebuildSolution(queue, move, i);
                    }
                    queue.Add((successor, move, i));
                }
            }
            return null;
        }

        private NPuzzle.Location[] RebuildSolution(
            List<(NPuzzle state, NPuzzle.Location move, int cameFrom)> visited,
            NPuzzle.Location lastMove,
            int lastStateIndex
        )
        {
            var solution = new List<NPuzzle.Location>();
            solution.Add(lastMove);
            NPuzzle state;
            NPuzzle.Location move;
            while(lastStateIndex > 0)
            {
                (state, move, lastStateIndex) = visited[lastStateIndex];
                solution.Add(move);
            }
            solution.Reverse();
            return solution.ToArray();
        }
    }
}