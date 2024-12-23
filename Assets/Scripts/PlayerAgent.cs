using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PlayerAgent : Agent
{
    [SerializeField] private Playground playground;
    [SerializeField] private Player player;
    [SerializeField] private Player rival;
    [SerializeField] private float moveReward = -0.02f;
    [SerializeField] private float winReward = 1f;
    [SerializeField] private float loseReward = -1f;
    [SerializeField] private float drawReward = 0.5f;

    private int _pieceCount;
    private int _cellCount;
    
    protected override void Awake()
    {
        base.Awake();
        _pieceCount = player.GetPieceCount();
        _cellCount = playground.GetCellCount();
        
        if (rival.GetPieceCount() != _pieceCount)
        {
            Debug.LogError("Player and rival have different piece counts. " +
                           $"Player: {_pieceCount}, Rival: {rival.GetPieceCount()}.");
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        player.StateChanged += OnPlayerStateChanged;
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        player.StateChanged -= OnPlayerStateChanged;
    }

    private void OnPlayerStateChanged(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                break;
            case PlayerState.WaitingForMove:
                OnWaitingForMove();
                break;
            case PlayerState.Win:
                OnWin();
                break;
            case PlayerState.Lose:
                OnLose();
                break;
            case PlayerState.Draw:
                OnDraw();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void OnWaitingForMove()
    {
        RequestDecision();
    }

    private void OnWin()
    {
        AddReward(winReward);
        EndEpisode();
    }

    private void OnLose()
    {
        AddReward(loseReward);
        EndEpisode();
    }

    private void OnDraw()
    {
        AddReward(drawReward);
        EndEpisode();
    }
    
    private (int piece, int cell) DiscreteActionToMove(int action)
    {
        return (action / _cellCount, action % _cellCount);
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

    public override void OnActionReceived(ActionBuffers actions)
    {
        var action = actions.DiscreteActions[0];
        var (piece, cell) = DiscreteActionToMove(action);
        
        if (!player.TryMakeMove(piece, cell))
        {
            Debug.LogError($"Invalid move. Piece: {piece}, Cell: {cell}.");
        }

        AddReward(moveReward);
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        var hasAction = false;
        var actionCount = _pieceCount * _cellCount;

        for (var action = 0; action < actionCount; action++)
        {
            var (piece, cell) = DiscreteActionToMove(action);
            var canMove = player.CanMove(piece, cell);
            hasAction = hasAction || canMove;
            actionMask.SetActionEnabled(0, action, canMove);
        }
        
        if (!hasAction)
        {
            Debug.LogError("No action! Cannot make any move.");
        }
    }
}