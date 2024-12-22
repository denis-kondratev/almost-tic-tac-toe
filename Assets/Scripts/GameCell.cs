using UnityEngine;

public class GameCell : MonoBehaviour
{
    [SerializeField] private Transform pieceMount;
    
    public Vector3 MountPosition => pieceMount.position;
    private GamePiece _currentPiece;

    public GamePiece GetCurrentPiece()
    {
        return _currentPiece;
    }

    public bool CanMove(GamePiece piece)
    {
        return !_currentPiece || _currentPiece.Number > piece.Number;
    }

    public bool TryMovePiece(GamePiece piece)
    {
        if (CanMove(piece))
        {
            _currentPiece = piece;
            return true;
        }

        return false;
    }

    public bool IsTeam(Team team)
    {
        return _currentPiece && _currentPiece.Team == team;
    }

    public void Reset()
    {
        _currentPiece = null;
    }
}
