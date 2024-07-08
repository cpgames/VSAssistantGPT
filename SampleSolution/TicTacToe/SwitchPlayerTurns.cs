using System;

namespace TicTacToeGame
{
    public class SwitchPlayerTurns
    {
        public static char SwitchPlayer(char currentPlayer)
        {
            return (currentPlayer == 'X') ? 'O' : 'X';
        }
    }
}
