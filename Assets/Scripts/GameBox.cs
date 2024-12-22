using System;
using UnityEngine;

public class GameBox : MonoBehaviour
{    
    [SerializeField] private Player bluePlayer;
    [SerializeField] private Player redPlayer;
    [SerializeField] private Playground playground;
    [SerializeField] private float stepDuration = 0.5f; 
    
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
            _nextStepTime += stepDuration;
            NextStep();
        }
    }

    private void NextStep()
    {
        switch (_currentState)
        {
            case State.None:
                StartGame();
                break;
            case State.BlueTurn:
                NextTurn(bluePlayer);
                break;
            case State.WaitForBlueMove:
                WaitForMove(bluePlayer);
                break;
            case State.RedTurn:
                NextTurn(redPlayer);
                break;
            case State.WaitForRedMove:
                WaitForMove(redPlayer);
                break;
            case State.BlueWin:
                SetupWin(bluePlayer, redPlayer);
                break;
            case State.RedWin:
                SetupWin(redPlayer, bluePlayer);
                break;
            case State.Draw:
                SetupDraw();
                break;
            case State.BlueMadeInvalidMove:
                OnInvalidMove(bluePlayer);
                break;
            case State.RedMadeInvalidMove:
                OnInvalidMove(redPlayer);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnInvalidMove(Player player)
    {
        player.OnInvalidMove();
        GotoState(State.None);
    }

    private void SetupDraw()
    {
        bluePlayer.OnDraw();
        redPlayer.OnDraw();
        GotoState(State.None);
    }

    private void SetupWin(Player winner, Player loser)
    {
        winner.OnWin();
        loser.OnLose();
        GotoState(State.None);
    }

    private void WaitForMove(Player player)
    {
        if (!player.IsWaitingForMove())
        {
            OnPlayerMoved(player.Team);
        }
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

    private void NextTurn(Player player)
    {
        if (player.CanMakeAnyMove())
        {
            player.NextTurn();
            var nextState = player.Team == Team.Blue ? State.WaitForBlueMove : State.WaitForRedMove;
            GotoState(nextState);
        }
        else
        {
            GotoState(State.Draw);
        }
    }

    private void StartGame()
    {
        bluePlayer.ResetPlayer();
        redPlayer.ResetPlayer();
        playground.StartGame();
        GotoState(State.BlueTurn);
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