using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Playground : MonoBehaviour
{
    [SerializeField] private GameCell[] cells;

    public PlaygroundState State { get; private set; }
    
    public int GetCellCount() => cells.Length;
    
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
        for (var i = 0; i < 9; i++)
        {
            if (IsWinningMove(new Move(minPiece, i), playgroundMask))
            {
                return true;
            }
        }

        return true;
    }

    public bool CanPreventLoss(Move move, Team team)
    {
        return false;
    }
    
    public int GetMask(Team team)
    {
        var mask = 0;
        for (var i = 0; i < 9; i++)
        {
            if (cells[i].IsTeam(team))
            {
                mask |= 1 << i;
            }
        }

        return mask;
    }

    private void GotoState(PlaygroundState state)
    {
        State = state;
    }

    private void VerifyPlayground()
    {
        Assert.IsTrue(cells.Length == 9, $"The {nameof(cells)} must contain exactly 9 cells.");
        
        for (var i = 0; i < cells.Length; i++)
        {
            Assert.IsTrue(cells[i].Index == i, $"Invalid cell size: {cells[i].Index}. Playground: {gameObject.name}.");
        }
    }

    private bool CanWinAtCell(int cell, int playgroundMask)
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