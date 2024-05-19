using System;

namespace TicTacToeGame
{
    public class PlayerInput
    {
        public static int[] GetInput()
        {
            int[] player_input = new int[2];
            Console.WriteLine("Enter row and column numbers separated by space: ");
            string[] inputString = Console.ReadLine().Split(' ');
            player_input[0] = int.Parse(inputString[0]);
            player_input[1] = int.Parse(inputString[1]);
            return player_input;
        }
    }
}
