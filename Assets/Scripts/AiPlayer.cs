using System;
using UnityEngine;

public abstract class AiPlayer : MonoBehaviour
{
    [SerializeField] protected Player player;
    
    protected void OnEnable()
    {
        player.StateChanged += OnPlayerStateChanged;
    }

    protected void OnDisable()
    {
        player.StateChanged -= OnPlayerStateChanged;
    }
    
    private void OnPlayerStateChanged(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                break;
            case PlayerState.WaitingForMove:
                MakeMove();
                break;
        }
    }

    protected abstract Move GetMove();

    private async void MakeMove()
    {
        try
        {
            var move = GetMove();
            var hasMoved = await player.TryMakeMoveWithTranslation(move);

            if (destroyCancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (!hasMoved)
            {
                Debug.LogError($"Invalid move. Piece: {move.Piece}, Cell: {move.Cell}. Player: {player.name}.");
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}