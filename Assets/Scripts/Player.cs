using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{ 
    [field: SerializeField] public Team Team { get; private set; }
    [SerializeField] private GamePiece[] pieces;
    [SerializeField] private Playground playground;
    private bool[] _hasPiece;
    
    public const int PieceCount = 7;

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
    public event Action<Move> MadeMove;
    
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
        var minPiece = GetMinPiece();
        return minPiece >= 0 && playground.CanMakeAnyMove(pieces[minPiece]);
    }

    private void VerifyPlayer()
    {
        Assert.AreEqual(pieces.Length, PieceCount, 
            $"Invalid piece count: {pieces.Length}. Player: {gameObject.name}. Expected: {PieceCount}.");
        
        for (var i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].Number != i)
            {
                throw new Exception($"Invalid piece size: {pieces[i].Number}. Player: {gameObject.name}.");
            }
        }
    }

    public bool CanMove(Move move)
    {
        var (piece, _) = move;
        if (piece < 0 || piece >= pieces.Length)
        {
            Debug.LogError($"Piece index is out of range. Value: {piece}.");
            return false;
        }

        return _hasPiece[piece] && playground.CanMove(move);
    }

    public bool TryMakeMove(GamePiece piece, GameCell cell)
    {
        if (TryMovePieceToCell(piece, cell))
        {
            MadeMove?.Invoke(new Move(piece.Number, cell.Index));
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

    public async Awaitable<bool> TryMakeMoveWithTranslation(Move move)
    {
        var (piece, cell) = move;
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

    public int GetAvailablePieces(int cell, int[] buffer)
    {
        if (cell is < 0 or >= Playground.CellCount)
        {
            throw new ArgumentException($"Invalid cell index. Value: {cell}.", nameof(cell));
        }
        
        if (buffer.Length < PieceCount)
        {
            throw new ArgumentException(
                $"The buffer is too small. Expected: {PieceCount}. Actual: {buffer.Length}.",
                nameof(buffer));
        }
        
        var count = 0;
        
        for (var i = 0; i < PieceCount; i++)
        {
            if (_hasPiece[i] && playground.CanMove(new Move(i, cell)))
            {
                buffer[count++] = i;
            }
        }

        return count;
    }
}