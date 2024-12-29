using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class PlayerAgent : Agent
{
    [SerializeField] protected Playground playground;
    [SerializeField] protected Player player;
    [SerializeField] protected Player rival;
    
    protected Move _lastHeuristicMove;
    private BehaviorParameters _parameters;
    private int _moveCount;
    private const int MinMoveToWin = 3;
    private const int MinMoveToLost = 2;
    
    private bool IsHeuristic => _parameters.BehaviorType == BehaviorType.HeuristicOnly;
    
    protected override void Awake()
    {
        base.Awake();
        _parameters = GetComponent<BehaviorParameters>();
        
        Assert.IsNotNull(_parameters, $"Cannot find {nameof(BehaviorParameters)} on game object {name}.");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        player.StateChanged += OnPlayerStateChanged;

        if (IsHeuristic)
        {
            player.MadeMove += OnPlayerMadeMove;
        }
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        player.StateChanged -= OnPlayerStateChanged;
        player.MadeMove -= OnPlayerMadeMove;
    }

    public override void OnEpisodeBegin()
    {
        _moveCount = 0;
    }

    private void OnPlayerMadeMove(Move move)
    {
        _lastHeuristicMove = move;
        RequestDecision();
    }

    private void OnPlayerStateChanged(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                break;
            case PlayerState.WaitingForMove when !IsHeuristic:
                RequestDecision();
                break;
            case PlayerState.Win:
                EndEpisode(GetWinReward());
                break;
            case PlayerState.Lose:
                EndEpisode(GetLossReward());
                break;
            case PlayerState.Draw:
                EndEpisode(GetDrawReward());
                break;
        }
    }

    private float GetDrawReward()
    {
        return 0;
    }

    private float GetLossReward()
    {
        return GetEpisodeDuration(true) / 2 - 1;
    }

    private float GetWinReward()
    {
        return 1 - GetEpisodeDuration(false) / 2;
    }
    
    private float GetEpisodeDuration(bool isLost)
    {
        Assert.IsTrue(_moveCount <= Player.PieceCount, $"Move count {_moveCount} is greater than piece count {Player.PieceCount}.");
        var monMove = isLost ? MinMoveToLost : MinMoveToWin;
        return (float)(_moveCount - monMove) / (Player.PieceCount - monMove);
    }

    private void EndEpisode(float reward)
    {
        AddReward(reward);
        EndEpisode();
    }

    protected abstract Move ActionToMove(ActionBuffers actions);

    public override async void OnActionReceived(ActionBuffers actions)
    {
        try
        {
            _moveCount++;
            var move = ActionToMove(actions);
            var hasMoved = IsHeuristic || await player.TryMakeMoveWithTranslation(move);

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

    protected abstract void SetupBrainParameters (BrainParameters parameters);
    
#if UNITY_EDITOR
    [ContextMenu("Setup Parameters")]
    private void SetupParameters()
    {
        var parameters = GetComponent<BehaviorParameters>();
        if (parameters == null)
        {
            Debug.LogError($"Cannot find {nameof(BehaviorParameters)} on game object {name}.");
            return;
        }
        
        SetupBrainParameters(parameters.BrainParameters);
        UnityEditor.EditorUtility.SetDirty(parameters);
    }
#endif
}