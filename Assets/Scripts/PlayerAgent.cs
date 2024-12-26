using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerAgent : Agent
{
    [SerializeField] private Playground playground;
    [SerializeField] private Player player;
    [SerializeField] private Player rival;
    [SerializeField] private float defaultMoveReward = 0;
    [SerializeField] private float winReward = 1f;
    [SerializeField] private float loseReward = -0.1f;
    [SerializeField] private float drawReward = 0.5f;
    [SerializeField] private float missedWinReward = -0.1f; 
    [SerializeField] private float failedPreventLossReward = -0.1f; 

    private int _pieceCount;
    private int _cellCount;
    private Move _lastHeuristicMove;
    private BehaviorParameters _parameters;
    
    private bool IsHeuristic => _parameters.BehaviorType == BehaviorType.HeuristicOnly;
    
    protected override void Awake()
    {
        base.Awake();
        _pieceCount = player.GetPieceCount();
        _cellCount = playground.GetCellCount();
        
        Assert.AreEqual(rival.GetPieceCount(), _pieceCount,
            "Player and rival have different piece counts. " +
            $"Player: {_pieceCount}, Rival: {rival.GetPieceCount()}.");
        
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
    
    private Move DiscreteActionToMove(int action)
    {
        if (action < 0 || action >= _cellCount * _pieceCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(action),
                $"Action value {action} is out of range. Valid range: 0 to {_cellCount * _pieceCount - 1}."
            );
        }
        
        return new Move(action / _cellCount, action % _cellCount);
    }

    private int MoveToDiscreteAction(Move move)
    {
        return move.Piece * _cellCount + move.Cell;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        for (var i = 0; i < _cellCount; i++)
        {
            var piece = playground.GetCellPiece(i);
            sensor.AddOneHotObservation(piece ? piece.Number : -1, _pieceCount);
            sensor.AddObservation(piece && piece.Team == Team.Red);
        }
        
        for (var i = 0; i < _pieceCount; i++)
        {
            sensor.AddObservation(player.HasPiece(i));
        }
        
        for (var i = 0; i < _pieceCount; i++)
        {
            sensor.AddObservation(rival.HasPiece(i));
        }
    }

    public override async void OnActionReceived(ActionBuffers actions)
    {
        try
        {
            var action = actions.DiscreteActions[0];
            var move = DiscreteActionToMove(action);
            
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
        if (player.HasMissedWin(move, minPiece))
        {
            return missedWinReward;
        }

        if (player.HasFailedPreventLoss(move))
        {
            return failedPreventLossReward;
        }

        return defaultMoveReward;
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        var hasAction = false;
        var actionCount = _pieceCount * _cellCount;

        for (var action = 0; action < actionCount; action++)
        {
            var move = DiscreteActionToMove(action);
            var canMove = player.CanMove(move);
            hasAction = hasAction || canMove;
            actionMask.SetActionEnabled(0, action, canMove);
        }
        
        if (!hasAction)
        {
            Debug.LogError("No action! Cannot make any move.");
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;
        actions[0] = MoveToDiscreteAction(_lastHeuristicMove);
    }
}