using System;

namespace TicTacToeGame
{
    public class DeclareResult
    {
        public static void DeclareWinner(char symbol)
        {
            Console.WriteLine($"Player {symbol} wins!");
        }

        public static void DeclareDraw()
        {
            Console.WriteLine("It's a draw!");
        }
    }
}
