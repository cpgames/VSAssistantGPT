using System;

namespace TicTacToeGame
{
    public class CheckWinner
    {
        public static bool IsWinner(char[,] board, char symbol, int size)
        {
            return CheckRows(board, symbol, size) || CheckColumns(board, symbol, size) || CheckDiagonals(board, symbol, size);
        }

        private static bool CheckRows(char[,] board, char symbol, int size)
        {
            for (int i = 0; i < size; i++)
            {
                bool isWinner = true;
                for (int j = 0; j < size; j++)
                {
                    if (board[i, j] != symbol)
                    {
                        isWinner = false;
                        break;
                    }
                }
                if (isWinner)
                    return true;
            }
            return false;
        }

        private static bool CheckColumns(char[,] board, char symbol, int size)
        {
            for (int j = 0; j < size; j++)
            {
                bool isWinner = true;
                for (int i = 0; i < size; i++)
                {
                    if (board[i, j] != symbol)
                    {
                        isWinner = false;
                        break;
                    }
                }
                if (isWinner)
                    return true;
            }
            return false;
        }

        private static bool CheckDiagonals(char[,] board, char symbol, int size)
        {
            bool isWinner = true;
            for (int i = 0; i < size; i++)
            {
                if (board[i, i] != symbol)
                {
                    isWinner = false;
                    break;
                }
            }
            if (isWinner)
                return true;

            isWinner = true;
            for (int i = 0; i < size; i++)
            {
                if (board[i, size - 1 - i] != symbol)
                {
                    isWinner = false;
                    break;
                }
            }
            return isWinner;
        }
    }
}
