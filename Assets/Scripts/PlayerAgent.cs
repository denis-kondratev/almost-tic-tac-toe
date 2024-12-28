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
    [SerializeField] private float defaultMoveReward = -0.02f;
    [SerializeField] private float winReward = 1f;
    [SerializeField] private float loseReward = -0.2f;
    [SerializeField] private float drawReward = 0.5f;
    [SerializeField] private float missedWinReward = -0.2f;
    
    protected Move _lastHeuristicMove;
    private BehaviorParameters _parameters;
    
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
                EndEpisode(winReward);
                break;
            case PlayerState.Lose:
                EndEpisode(loseReward);
                break;
            case PlayerState.Draw:
                EndEpisode(drawReward);
                break;
        }
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
            var move = ActionToMove(actions);
            
            AddReward(GetRewardForMove(move));
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
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private float GetRewardForMove(Move move)
    {
        if (IsHeuristic)
        {
            return defaultMoveReward;
        }

        var minPiece = Math.Min(move.Piece, player.GetMinPiece());
        Assert.IsTrue(minPiece >= 0, $"Player '{player.name}' has no piece.");
        
        return HasMissedWin(move, minPiece) ? missedWinReward : defaultMoveReward;
    }
    
    private bool HasMissedWin(Move move, int minPiece)
    {
        var playgroundMask = playground.GetMask(player.Team);
        
        return !playground.IsWinningMove(move, playgroundMask) 
               && playground.HasWinningMove(playgroundMask, minPiece);
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