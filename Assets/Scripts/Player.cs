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

    private bool CanMakeAnyMove()
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

    public bool CanMove(int piece, int cell)
    {
        if (piece < 0 || piece >= pieces.Length)
        {
            Debug.LogError($"Piece index is out of range. Value: {piece}.");
            return false;
        }

        return _hasPiece[piece] && playground.CanMove(cell, piece);
    }

    public bool TryMakeMove(GamePiece piece, GameCell cell)
    {
        if (TryMovePieceToCell(piece, cell))
        {
            State = PlayerState.Idle;
            return true;
        }
        
        return false;
    }

    private bool TryMovePieceToCell(GamePiece piece, GameCell cell)
    {
        if (State is PlayerState.WaitingForMove && _hasPiece[piece.Number] && playground.TryMakeMove(piece, cell))
        {
            _hasPiece[piece.Number] = false;
            return true;
        }

        return false;
    }

    public async Awaitable<bool> TryMakeMoveWithTranslation(int piece, int cell)
    {
        if (piece < 0 || piece > pieces.Length || cell < 0 || cell > playground.GetCellCount())
        {
            return false;
        }

        var gamePiece = pieces[piece];
        var gameCell = playground.GetCell(cell);
        
        if (TryMovePieceToCell(gamePiece, gameCell))
        {
            await gamePiece.TranslateToCell(gameCell);
            
            if (destroyCancellationToken.IsCancellationRequested)
            {
                return false;
            }
            
            State = PlayerState.Idle;
            return true;
        }

        return false;
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

    public bool TryStartTurn()
    {
        if (CanMakeAnyMove())
        {
            State = PlayerState.WaitingForMove;
            return true;
        }
        
        return false;
    }
}