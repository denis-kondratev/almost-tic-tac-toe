using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{ 
    [field: SerializeField] public Team Team { get; private set; }
    [SerializeField] private GamePiece[] pieces;
    [SerializeField] private Playground playground;
    
    private bool[] _hasPiece;
    
    public void Reset()
    {
        CheckPlayer();
        Array.ForEach(pieces, x => x.Reset());
        _hasPiece ??= new bool[pieces.Length];
        Array.Fill(_hasPiece, true);
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
        if (pieceNumber < 0 || pieceNumber > pieces.Length)
        {
            playground.MakeInvalidMove(Team);
            return false;
        }

        if (!_hasPiece[pieceNumber])
        {
            playground.MakeInvalidMove(Team);
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

        return true;
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
}