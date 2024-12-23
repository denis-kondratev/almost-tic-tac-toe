using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Playground : MonoBehaviour
{
    [SerializeField] private GameCell[] cells;

    public PlaygroundState State { get; private set; }
    
    public int GetCellCount() => cells.Length;

    private void Awake()
    {
        Assert.IsTrue(cells.Length == 9, $"The {nameof(cells)} must contain exactly 9 cells.");
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

    public bool CanMove(int cellIndex, int pieceNumber)
    {
        if (cellIndex < 0 || cellIndex >= cells.Length)
        {
            Assert.IsTrue(false, $"Cell index is out of range. Value: {cellIndex}");
            return false;
        }
        
        var currentPiece = cells[cellIndex].GetCurrentPiece();
        return currentPiece == null || currentPiece.Number > pieceNumber;
    }

    public void MakeInvalidMove()
    {
        GotoState(PlaygroundState.InvalidMove);
    }

    public bool MakeMove(int cellIndex, GamePiece piece)
    {
        if (cellIndex < 0 || cellIndex >= cells.Length)
        {
            MakeInvalidMove();
            return false;
        }

        if (!cells[cellIndex].TryMovePiece(piece))
        {
            MakeInvalidMove();
            return false;
        }

        if (CheckWin(cellIndex, piece.Team))
        {
            GotoState(PlaygroundState.HasWin);
        }
        
        return true;
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

    private bool CheckWin(int cellIndex, Team team)
    {
        var rowStart = cellIndex / 3 * 3;
        if (cells[rowStart].IsTeam(team) && cells[rowStart + 1].IsTeam(team) && cells[rowStart + 2].IsTeam(team))
        {
            return true;
        }

        var colStart = cellIndex % 3;
        if (cells[colStart].IsTeam(team) && cells[colStart + 3].IsTeam(team) && cells[colStart + 6].IsTeam(team))
        {
            return true;
        }

        if (cellIndex is 0 or 4 or 8)
        {
            if (cells[0].IsTeam(team) && cells[4].IsTeam(team) && cells[8].IsTeam(team))
            {
                return true;
            }
        }

        if (cellIndex is 2 or 4 or 6)
        {
            if (cells[2].IsTeam(team) && cells[4].IsTeam(team) && cells[6].IsTeam(team))
            {
                return true;
            }
        }

        return false;
    }
    
    private void GotoState(PlaygroundState state)
    {
        State = state;
    }

    public bool TryMakeMove(GameCell cell, GamePiece piece)
    {
        if (State is not PlaygroundState.Playing || !cell.TryMovePiece(piece))
        {
            return false;
        }
        
        var cellIndex = Array.IndexOf(cells, cell);
            
        if (CheckWin(cellIndex, piece.Team))
        {
            GotoState(PlaygroundState.HasWin);
        }

        return true;
    }
}