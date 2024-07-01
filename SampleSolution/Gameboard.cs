namespace TicTacToeGame
{
    public class Gameboard : IGameBoard
    {
        private char[,] board;
        private int size;

        public Gameboard(int size)
        {
            this.size = size;
            board = new char[size, size];
            InitializeBoard();
        }

        public void InitializeBoard()
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    board[i, j] = ' '; // Empty space
                }
            }
        }

        public void DisplayBoard()
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Console.Write(board[i, j]);
                    if (j < size - 1)
                    {
                        Console.Write("|");
                    }
                }
                Console.WriteLine();
                if (i < size - 1)
                {
                    for (int k = 0; k < size * 2 - 1; k++)
                    {
                        Console.Write("-");
                    }
                    Console.WriteLine();
                }
            }
        }

        // Implementation of the missing method 
        public int GetBoardSize()
        {
            return size;
        }
    }
}
