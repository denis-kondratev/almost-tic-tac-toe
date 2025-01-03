using UnityEngine;

public class AnalogMoveUtils
{
    private readonly Player _player;
    private readonly Playground _playground;
    private readonly int[] _buffer = new int[Playground.CellCount];

    public AnalogMoveUtils(Player player, Playground playground)
    {
        _player = player;
        _playground = playground;
    }

    public Move GetMove(float normalCell, float normalPiece)
    {
        var cell = GetCell(normalCell);
        var piece = GetPiece(normalPiece, cell);
        return new Move(piece, cell);
    }

    private int GetCell(float normalCell)
    {
        var minPiece = _player.GetMinPiece();

        if (minPiece < 0)
        {
            throw new System.InvalidOperationException("Player has no pieces.");
        }

        var cellCount = _playground.GetAvailableCells(minPiece, _buffer);

        if (cellCount == 0)
        {
            throw new System.InvalidOperationException("Player has no available moves.");
        }

        var cellIndex = SignedNormalToIndex(normalCell, cellCount);
        return _buffer[cellIndex];
    }

    private int GetPiece(float normalPiece, int cell)
    {
        var pieceCount = _player.GetAvailablePieces(cell, _buffer);

        if (pieceCount == 0)
        {
            throw new System.InvalidOperationException("Player has no available pieces.");
        }

        var pieceIndex = SignedNormalToIndex(normalPiece, pieceCount);
        return _buffer[pieceIndex];
    }

    private static int SignedNormalToIndex(float signedNorm, int count)
    {
        if (signedNorm is < -1 or > 1)
        {
            throw new System.ArgumentOutOfRangeException(nameof(signedNorm), signedNorm,
                $"Value must be in [-1, 1]. Actual value is {signedNorm}.");
        }

        if (count <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(count), count, "Value must be positive.");
        }

        var norm = (signedNorm + 1f) / 2f;
        return norm is 1 ? count - 1 : Mathf.FloorToInt(norm * count);
    }
}