using System;

namespace TicTacToeGame
{
    public class UpdateGameboard
    {
        public static void UpdateBoard(char[,] board, int[] input, char symbol)
        {
            int row = input[0];
            int col = input[1];

            board[row, col] = symbol;
        }
    }
}
