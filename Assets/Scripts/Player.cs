using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{ 
    [field: SerializeField] public Team Team { get; private set; }
    [SerializeField] private GamePiece[] pieces;
    [SerializeField] private Playground playground;

    public event Action InvalidMove;
    public event Action Win;
    public event Action Draw;
    public event Action Lose;
    public event Action Turn;
    public event Action Reset;
    public event Action Moved;
    
    private bool[] _hasPiece;
    
    public bool IsWaitingForMove {get; private set;}
    
    public void ResetPlayer()
    {
        CheckPlayer();
        Array.ForEach(pieces, x => x.Reset());
        _hasPiece ??= new bool[pieces.Length];
        Array.Fill(_hasPiece, true);
        Reset?.Invoke();
    }
    
    public int GetPieceCount()
    {
        return pieces.Length;
    }
    
    public bool HasPiece(int pieceSize)
    {
        if (pieceSize < 0 || pieceSize > pieces.Length)
        {
            Assert.IsTrue(false, $"Invalid piece size. Value: {pieceSize}.");
            return false;
        }
        
        return _hasPiece[pieceSize];
    }

    public bool MakeMove(int cellIndex, int pieceNumber)
    {
        IsWaitingForMove = false;
        
        if (pieceNumber < 0 || pieceNumber > pieces.Length)
        {
            playground.MakeInvalidMove();
            return false;
        }

        if (!_hasPiece[pieceNumber])
        {
            playground.MakeInvalidMove();
            return false;
        }
        
        _hasPiece[pieceNumber] = false;
        return playground.MakeMove(cellIndex, pieces[pieceNumber]);
    }

    public bool CanMakeAnyMove()
    {
        for (var i = 0; i < pieces.Length; i++)
        {
            if (_hasPiece[i] && playground.CanMakeAnyMove(pieces[i]))
            {
                return true;
            }
        }

        return false;
    }

    private void CheckPlayer()
    {
        for (var i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].Number != i)
            {
                throw new Exception($"Invalid piece size: {pieces[i].Number}. Player: {gameObject.name}.");
            }
        }
    }

    public int GetAvailablePieceCount()
    {
        return _hasPiece.Count(x => x);
    }

    public bool CanMove(int cellIndex, int pieceNumber)
    {
        if (pieceNumber < 0 || pieceNumber > pieces.Length || !_hasPiece[pieceNumber])
        {
            return false;
        }
        
        return playground.CanMove(cellIndex, pieceNumber);
    }

    public int GetMinPiece()
    {   
        for (var i = 0; i < _hasPiece.Length; i++)
        {
            if (_hasPiece[i])
            {
                return i;
            }
        }

        return -1;
    }

    public void OnInvalidMove()
    {
        InvalidMove?.Invoke();
    }

    public void OnDraw()
    {
        Draw?.Invoke();
    }

    public void OnWin()
    {
        Win?.Invoke();
    }

    public void OnLose()
    {
        Lose?.Invoke();
    }

    public void NextTurn()
    {
        IsWaitingForMove = true;
        Turn?.Invoke();
    }

    public bool TryMakeMove(GameCell cell, GamePiece piece)
    {
        if (_hasPiece[piece.Number] && playground.TryMakeMove(cell, piece))
        {
            _hasPiece[piece.Number] = false;
            IsWaitingForMove = false;
            EndTurn();
            return true;
        }
        
        return false;
    }
    
    private void EndTurn()
    {
        IsWaitingForMove = false;
        Moved?.Invoke();
    }
}