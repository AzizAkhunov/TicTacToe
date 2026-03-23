namespace TicTacToe.Helpers
{
    public static class GameLogicHelper
    {
        private static readonly int[][] WinningCombinations =
    {
        new[] { 0, 1, 2 },
        new[] { 3, 4, 5 },
        new[] { 6, 7, 8 },
        new[] { 0, 3, 6 },
        new[] { 1, 4, 7 },
        new[] { 2, 5, 8 },
        new[] { 0, 4, 8 },
        new[] { 2, 4, 6 }
    };

        public static string? CheckWinner(string board)
        {
            foreach (var combo in WinningCombinations)
            {
                var a = board[combo[0]];
                var b = board[combo[1]];
                var c = board[combo[2]];

                if (a != '-' && a == b && b == c)
                    return a.ToString();
            }

            if (!board.Contains('-'))
                return "Draw";

            return null;
        }
    }
}
