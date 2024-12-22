using UnityEngine;

public class GamePiece : MonoBehaviour
{
    [Range(0, 6)]
    [field: SerializeField] public int Number { get; private set; }
    [field: SerializeField] public Team Team { get; private set; }

    private Vector3 _initialPosition;

    private void Awake()
    {
        _initialPosition = transform.position;
    }

    public void Reset()
    {
        transform.position = _initialPosition;
    }

    public void MoveTo(Transform pieceMount)
    {
        transform.position = pieceMount.position;
    }
}