using System;
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

    public void MakeMove(int cellIndex, int pieceNumber)
    {
        if (pieceNumber < 0 || pieceNumber > pieces.Length)
        {
            playground.MakeInvalidMove(Team);
            return;
        }

        if (!_hasPiece[pieceNumber])
        {
            playground.MakeInvalidMove(Team);
            return;
        }
        
        _hasPiece[pieceNumber] = false;
        playground.MakeMove(cellIndex, pieces[pieceNumber]);
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
}