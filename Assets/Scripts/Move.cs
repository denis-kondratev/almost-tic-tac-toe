public readonly struct Move
{
    public readonly int Piece;
    public readonly int Cell;

    public Move(int piece, int cell)
    {
        Piece = piece;
        Cell = cell;
    }
    
    public void Deconstruct(out int piece, out int cell)
    {
        piece = Piece;
        cell = Cell;
    }
}