using System;

class Program
{
    static void Main()
    {
        // Initialize the game board
        char[,] gameBoard = new char[3, 3] { { ' ', ' ', ' ' }, { ' ', ' ', ' ' }, { ' ', ' ', ' ' } };
        bool isGameRunning = true;
        char currentPlayer = 'X';

        while (isGameRunning)
        {
            // Display the current game board
            Console.WriteLine("Current Game Board:");
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("| " + gameBoard[i, 0] + " | " + gameBoard[i, 1] + " | " + gameBoard[i, 2] + " |");
            }

            // Player makes a move
            Console.Write($"Player {currentPlayer}, enter row (0-2): ");
            int row = Convert.ToInt32(Console.ReadLine());
            Console.Write($"Player {currentPlayer}, enter column (0-2): ");
            int column = Convert.ToInt32(Console.ReadLine());

            // Update the game board
            gameBoard[row, column] = currentPlayer;

            // Check if the current player has won
            // Add logic here to check for a winner using CheckWinner.cs

            // Switch player turns
            currentPlayer = (currentPlayer == 'X') ? 'O' : 'X';

            // Check for a tie or continue the game
            // Implement tie check logic here
        }
    }
}