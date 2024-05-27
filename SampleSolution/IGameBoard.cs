namespace TicTacToeGame
{
    public interface IGameBoard
    {
        void InitializeBoard();
        void DisplayBoard();
        int GetBoardSize();
    }
}