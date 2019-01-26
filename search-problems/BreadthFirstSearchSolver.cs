using System;
using System.Collections.Generic;

namespace search_problems
{
    public class BreadthFirstSearchSolver : ISearchAlgorithm
    {
        public ulong StatesEvaluated { get; private set; }
        public int MaxDepth { get; private set; }

        public NPuzzle.Location[] Solve(NPuzzle puzzle)
        {
            StatesEvaluated = 0UL;
            MaxDepth = 0;
            
            List<(NPuzzle state, NPuzzle.Location move, int cameFrom, int depth)> queue;
            queue = new List<(NPuzzle, NPuzzle.Location, int, int)>();
            queue.Add((puzzle, null, -1, 0));
            for(int i = 0; i < queue.Count; i++)
            {
                var (state, lastMove, cameFrom, depth) = queue[i];
                this.StatesEvaluated++;
                var moves = state.ExpandMoves();
                foreach(var move in moves)
                {
                    var successor = state.MoveCopy(move);
                    if (successor.IsGoal())
                    {
                        return RebuildSolution(queue, move, i);
                    }
                    queue.Add((successor, move, i, depth + 1));
                }
                MaxDepth = Math.Max(MaxDepth, depth + 1);
            }
            return null;
        }

        private NPuzzle.Location[] RebuildSolution(
            List<(NPuzzle state, NPuzzle.Location move, int cameFrom, int depth)> visited,
            NPuzzle.Location lastMove,
            int lastStateIndex
        )
        {
            var solution = new List<NPuzzle.Location>();
            solution.Add(lastMove);
            NPuzzle state;
            NPuzzle.Location move;
            int depth;
            while(lastStateIndex > 0)
            {
                (state, move, lastStateIndex, depth) = visited[lastStateIndex];
                solution.Add(move);
            }
            solution.Reverse();
            return solution.ToArray();
        }
    }
}