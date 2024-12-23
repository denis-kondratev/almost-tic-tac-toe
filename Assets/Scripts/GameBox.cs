using System;
using UnityEngine;

public class GameBox : MonoBehaviour
{    
    [SerializeField] private Player bluePlayer;
    [SerializeField] private Player redPlayer;
    [SerializeField] private Playground playground;
    [SerializeField] private float nextGameDelay = 3f;
    
    private State _state;
    
    private void Start()
    {
        StartGame();
    }
    
    private void Update()
    {
        switch (_state)
        {
            case State.None:
                StartNextGame();
                break;
            case State.WaitForBlueMove when bluePlayer.State is not PlayerState.WaitingForMove:
                OnPlayerMoved(bluePlayer, redPlayer);
                break;
            case State.WaitForRedMove when redPlayer.State is not PlayerState.WaitingForMove:
                OnPlayerMoved(redPlayer, bluePlayer);
                break;
        }
    }

    private void StartNextGame()
    {
        _state = State.GameStarting;
        Invoke(nameof(StartGame), nextGameDelay);
    }

    private void OnInvalidMove(Player player)
    {
        player.OnInvalidMove();
        _state = State.None;
    }

    private void SetupDraw()
    {
        bluePlayer.OnDraw();
        redPlayer.OnDraw();
        _state = State.None;
    }

    private void SetupWin(Player winner, Player loser)
    {
        winner.OnWin();
        loser.OnLose();
        _state = State.None;
    }

    private void OnPlayerMoved(Player currentPlayer, Player otherPlayer)
    {
        switch (playground.State)
        {
            case PlaygroundState.Playing:
                NextTurn(otherPlayer);
                break;
            case PlaygroundState.HasWin:
                SetupWin(currentPlayer, otherPlayer);
                break;
            case PlaygroundState.InvalidMove:
                OnInvalidMove(currentPlayer);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void NextTurn(Player player)
    {
        if (player.CanMakeAnyMove())
        {
            player.StartTurn();
            _state = player.Team switch
            {
                Team.Blue => State.WaitForBlueMove,
                Team.Red => State.WaitForRedMove,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else
        {
            SetupDraw();
        }
    }

    private void StartGame()
    {
        bluePlayer.Reset();
        redPlayer.Reset();
        playground.Reset();
        NextTurn(bluePlayer);
    }

    private enum State 
    {
        None,
        GameStarting,
        WaitForBlueMove,
        WaitForRedMove
    }
}