using System;

namespace TicTacToeGame
{
    public class InputValidation
    {
        public static bool ValidateInput(int[] input, int size)
        {
            if (input.Length != 2)
            {
                return false;
            }

            int row = input[0];
            int col = input[1];

            return (row >= 0 && row < size && col >= 0 && col < size);
        }
    }
}
