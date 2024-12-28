using System;

public readonly struct Move
{
    public readonly int Piece;
    public readonly int Cell;

    public Move(int piece, int cell)
    {
        if (piece is < 0 or >= Player.PieceCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(piece),
                $"Piece value {piece} is out of range. Valid range: 0 to {Player.PieceCount - 1}."
            );
        }
        
        if (cell is < 0 or >= Playground.CellCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cell),
                $"Cell value {cell} is out of range. Valid range: 0 to {Playground.CellCount - 1}."
            );
        }
        
        Piece = piece;
        Cell = cell;
    }
    
    public void Deconstruct(out int piece, out int cell)
    {
        piece = Piece;
        cell = Cell;
    }
}