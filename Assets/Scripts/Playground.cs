using System;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class Playground : MonoBehaviour
{
    [SerializeField] private GameCell[] cells;

    public PlaygroundState State { get; private set; }
    
    public int GetCellCount() => cells.Length;

    private void Awake()
    {
        Assert.IsTrue(cells.Length == 9, $"The {nameof(cells)} must contain exactly 9 cells.");
    }

    public void StartGame()
    {
        State = PlaygroundState.Playing;
        Array.ForEach(cells, cell => cell.Reset());
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

    public bool CanMove(int cellIndex, int pieceSize)
    {
        if (cellIndex < 0 || cellIndex >= cells.Length)
        {
            Assert.IsTrue(false, $"Cell index is out of range. Value: {cellIndex}");
            return false;
        }
        
        var currentPiece = cells[cellIndex].GetCurrentPiece();
        return currentPiece == null || currentPiece.Number < pieceSize;
    }

    public void MakeInvalidMove(Team team)
    {
        State = team switch
        {
            Team.Blue => PlaygroundState.BlueMadeInvalidMove,
            Team.Red => PlaygroundState.RedMadeInvalidMove,
            _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
        };
    }

    public void MakeMove(int cellIndex, GamePiece piece)
    {
        if (cellIndex < 0 || cellIndex >= cells.Length)
        {
            MakeInvalidMove(piece.Team);
            return;
        }

        if (!cells[cellIndex].MovePiece(piece))
        {
            MakeInvalidMove(piece.Team);
            return;
        }

        if (CheckWin(cellIndex, piece.Team))
        {
            State = piece.Team switch
            {
                Team.Blue => PlaygroundState.BlueWins,
                Team.Red => PlaygroundState.RedWins,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
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

    public void EndGame()
    {
        State = PlaygroundState.None;
    }
}