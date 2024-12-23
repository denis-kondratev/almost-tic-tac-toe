using System;
using UnityEngine;

public class GameBox : MonoBehaviour
{    
    [SerializeField] private Player bluePlayer;
    [SerializeField] private Player redPlayer;
    [SerializeField] private Playground playground;
    [SerializeField] private float gameOverDelay = 3f;
    
    private float _nextStepTime;
    private State _currentState;
    
    private void Start()
    {
        _nextStepTime = Time.time;
    }
    
    private void Update()
    {
        if (_nextStepTime < Time.time)
        {
            _nextStepTime = Time.time + NextStep();
        }
    }

    private float NextStep()
    {
        return _currentState switch
        {
            State.None => StartGame(),
            State.BlueTurn => NextTurn(bluePlayer),
            State.WaitForBlueMove => WaitForMove(bluePlayer),
            State.RedTurn => NextTurn(redPlayer),
            State.WaitForRedMove => WaitForMove(redPlayer),
            State.BlueWin => SetupWin(bluePlayer, redPlayer),
            State.RedWin => SetupWin(redPlayer, bluePlayer),
            State.Draw => SetupDraw(),
            State.BlueMadeInvalidMove => OnInvalidMove(bluePlayer),
            State.RedMadeInvalidMove => OnInvalidMove(redPlayer),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float OnInvalidMove(Player player)
    {
        player.OnInvalidMove();
        GotoState(State.None);
        return gameOverDelay;
    }

    private float SetupDraw()
    {
        bluePlayer.OnDraw();
        redPlayer.OnDraw();
        GotoState(State.None);
        return gameOverDelay;
    }

    private float SetupWin(Player winner, Player loser)
    {
        winner.OnWin();
        loser.OnLose();
        GotoState(State.None);
        return gameOverDelay;
    }

    private float WaitForMove(Player player)
    {
        if (player.State is not PlayerState.WaitingForMove)
        {
            OnPlayerMoved(player.Team);
        }

        return 0;
    }

    private void OnPlayerMoved(Team team)
    {
        var nextState = GetNextState(playground.State, team);
        GotoState(nextState);
    }

    private State GetNextState(PlaygroundState playgroundState, Team team)
    {
        return playgroundState switch
        {
            PlaygroundState.Playing => team switch
            {
                Team.Blue => State.RedTurn,
                Team.Red => State.BlueTurn,
                _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
            },
            PlaygroundState.HasWin => team switch
            {
                Team.Blue => State.BlueWin,
                Team.Red => State.RedWin,
                _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
            },
            PlaygroundState.InvalidMove => team switch
            {
                Team.Blue => State.BlueMadeInvalidMove,
                Team.Red => State.RedMadeInvalidMove,
                _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float NextTurn(Player player)
    {
        if (player.CanMakeAnyMove())
        {
            player.StartTurn();
            var nextState = player.Team == Team.Blue ? State.WaitForBlueMove : State.WaitForRedMove;
            GotoState(nextState);
        }
        else
        {
            GotoState(State.Draw);
        }

        return 0;
    }

    private float StartGame()
    {
        bluePlayer.ResetPlayer();
        redPlayer.ResetPlayer();
        playground.StartGame();
        GotoState(State.BlueTurn);
        return 0;
    }
    
    private void GotoState(State state)
    {
        _currentState = state;
    }

    private enum State 
    {
        None,
        BlueTurn,
        WaitForBlueMove,
        RedTurn,
        WaitForRedMove,
        RedMadeInvalidMove,
        BlueMadeInvalidMove,
        BlueWin,
        RedWin,
        Draw
    }
}