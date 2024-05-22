using System;

namespace TicTacToeGame
{
    public class PlayerInput
    {
        public static int[] GetInput()
        {
            int[] player_input = new int[2];
            Console.WriteLine("Enter row and column numbers separated by space: ");
            string[] inputString = Console.ReadLine()?.Split(' ');
            if (inputString != null)
            {
                if (inputString.Length >= 2)
                {
                    player_input[0] = int.TryParse(inputString[0], out int row) ? row : 0;
                    player_input[1] = int.TryParse(inputString[1], out int col) ? col : 0;
                }
            }
            return player_input;
        }
    }
}