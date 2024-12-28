using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Playground : MonoBehaviour
{
    [SerializeField] private GameCell[] cells;

    public PlaygroundState State { get; private set; }
    
    public const int CellCount = 9;
    
    private static readonly int[] WinningMasks = {
        0b111000000, 0b000111000, 0b000000111, 0b100100100,
        0b010010010, 0b001001001, 0b100010001, 0b001010100
    };

    private void Awake()
    {
        VerifyPlayground();
    }

    public void Reset()
    {
        Array.ForEach(cells, cell => cell.Reset());
        GotoState(PlaygroundState.Playing);
    }

    public GamePiece GetCellPiece(int cellIndex)
    {
        if (cellIndex < 0 || cellIndex >= cells.Length)
        {
            Assert.IsTrue(false, $"Cell index is out of range. Value: {cellIndex}");
            return null;
        }
        
        return cells[cellIndex].GetCurrentPiece();
    }

    public bool CanMove(Move move)
    {
        var (piece, cell) = move;
        if (cell < 0 || cell >= cells.Length)
        {
            Debug.LogError($"Cell index is out of range. Value: {cell}");
            return false;
        }
        
        var currentPiece = cells[cell].GetCurrentPiece();
        return currentPiece == null || currentPiece.Number > piece;
    }

    public bool CanMakeAnyMove(GamePiece piece)
    {
        foreach (var cell in cells)
        {
            if (cell.CanMove(piece))
            {
                return true;
            }
        }

        return false;
    }

    public bool TryMakeMove(GamePiece piece, GameCell cell)
    {
        if (State is not PlaygroundState.Playing || !cell.TryMovePiece(piece))
        {
            return false;
        }

        if (CanWinAtCell(cell.Index, GetMask(piece.Team)))
        {
            GotoState(PlaygroundState.HasWin);
        }

        return true;
    }

    public GameCell GetCell(int cell)
    {
        if (cell < 0 || cell >= cells.Length)
        {
            Debug.LogError($"Cell index is out of range. Value: {cell}");
            cell = 0;
        }
        
        return cells[cell];
    }
    
    public bool IsWinningMove(Move move, int playgroundMask)
    {
        return CanMove(move) && CanWinAtCell(move.Cell, playgroundMask);
    }

    public bool HasWinningMove(int playgroundMask, int minPiece)
    {
        for (var i = 0; i < CellCount; i++)
        {
            if (IsWinningMove(new Move(minPiece, i), playgroundMask))
            {
                return true;
            }
        }

        return true;
    }
    
    public int GetMask(Team team)
    {
        var mask = 0;
        for (var i = 0; i < CellCount; i++)
        {
            if (cells[i].IsTeam(team))
            {
                mask |= 1 << i;
            }
        }

        return mask;
    }
    
    public int GetAvailableCells(int piece, int[] buffer)
    {
        if (buffer.Length < CellCount)
        {
            throw new ArgumentException(
                $"The buffer must have at least {CellCount} elements. Actual buffer size is {buffer.Length}.", 
                nameof(buffer));
        }
        
        var count = 0;
        
        for (var i = 0; i < CellCount; i++)
        {
            if (CanMove(new Move(piece, i)))
            {
                buffer[count++] = i;
            }
        }

        return count;
    }

    private void GotoState(PlaygroundState state)
    {
        State = state;
    }

    private void VerifyPlayground()
    {
        Assert.AreEqual(cells.Length, CellCount, $"The {nameof(cells)} must contain exactly {CellCount} cells.");
        
        for (var i = 0; i < cells.Length; i++)
        {
            Assert.IsTrue(cells[i].Index == i, $"Invalid cell size: {cells[i].Index}. Playground: {gameObject.name}.");
        }
    }

    private static bool CanWinAtCell(int cell, int playgroundMask)
    {
        var moveMask = 1 << cell;
        
        foreach (var mask in WinningMasks)
        {
            if (((playgroundMask | moveMask) & mask) == mask)
            {
                return true;
            }
        }

        return false;
    }
}