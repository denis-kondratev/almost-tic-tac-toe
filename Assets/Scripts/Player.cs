using System;
using UnityEngine;

public class Player : MonoBehaviour
{ 
    [field: SerializeField] public Team Team { get; private set; }
    [SerializeField] private GamePiece[] pieces;
    [SerializeField] private Playground playground;
    private bool[] _hasPiece;

    private PlayerState _state;

    public PlayerState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                StateChanged?.Invoke(value);
            }
        }
    }
    
    public event Action<PlayerState> StateChanged; 
    
    public void Reset()
    {
        VerifyPlayer();
        Array.ForEach(pieces, x => x.Reset());
        _hasPiece ??= new bool[pieces.Length];
        Array.Fill(_hasPiece, true);
        State = PlayerState.Idle;
    }
    
    public int GetPieceCount()
    {
        return pieces.Length;
    }
    
    public bool HasPiece(int piece)
    {
        if (piece < 0 || piece > pieces.Length)
        {
            Debug.LogError($"Invalid piece. Value: {piece}.");
            return false;
        }
        
        return _hasPiece[piece];
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

    private void VerifyPlayer()
    {
        for (var i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].Number != i)
            {
                throw new Exception($"Invalid piece size: {pieces[i].Number}. Player: {gameObject.name}.");
            }
        }
    }

    public bool CanMove(int cell, int piece)
    {
        if (piece < 0 || piece > pieces.Length || !_hasPiece[piece])
        {
            return false;
        }
        
        return playground.CanMove(cell, piece);
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

    public bool TryMakeMove(GameCell cell, GamePiece piece)
    {
        if (State is PlayerState.WaitingForMove && _hasPiece[piece.Number] && playground.TryMakeMove(cell, piece))
        {
            _hasPiece[piece.Number] = false;
            State = PlayerState.Idle;
            return true;
        }
        
        return false;
    }

    public void OnInvalidMove()
    {
        State = PlayerState.Invalid;
    }

    public void OnDraw()
    {
        State = PlayerState.Draw;
    }

    public void OnWin()
    {
        State = PlayerState.Win;
    }

    public void OnLose()
    {
        State = PlayerState.Lose;
    }

    public void StartTurn()
    {
        State = PlayerState.WaitingForMove;
    }
}