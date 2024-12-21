using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PlayerAgent : Agent
{
    [SerializeField] private Playground playground;
    [SerializeField] private Player player;
    [SerializeField] private Player rivalPlayer;
    
    public bool IsWaitingAction { get; private set; }
    
    public void MakeMove()
    {
        if (!player.CanMakeAnyMove())
        {
            playground.SetDraw();
            return;
        }
        
        IsWaitingAction = true;
        RequestDecision();
        //Academy.Instance.EnvironmentStep();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        var playgroundSize = playground.GetCellCount();
        for (var i = 0; i < playgroundSize; i++)
        {
            var cell = playground.GetCellPiece(i);
            sensor.AddOneHotObservation(cell ? cell.Number : -1, 7);
            sensor.AddObservation(cell && cell.Team == Team.Red);
        }

        var playerPieceCount = player.GetPieceCount();
        for (var i = 0; i < playerPieceCount; i++)
        {
            sensor.AddObservation(player.HasPiece(i));
        }

        var rivalPieceCount = rivalPlayer.GetPieceCount();
        for (var i = 0; i < rivalPieceCount; i++)
        {
            sensor.AddObservation(rivalPlayer.HasPiece(i));
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var pieceNumber = actions.DiscreteActions[0];
        var cellIndex = actions.DiscreteActions[1];
        
        if (!player.CanMove(cellIndex, pieceNumber))
        {
            pieceNumber = player.GetMinPiece();
        }

        player.MakeMove(cellIndex, pieceNumber);
        IsWaitingAction = false;
    }

    public void Reset()
    {
        player.Reset();
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        var hasAction = false;
        
        for (var piece = 0; piece < 7; piece++)
        {
            var hasPiece = player.HasPiece(piece);
            hasAction = hasAction || hasPiece;
            actionMask.SetActionEnabled(0, piece, hasPiece);
        }
        
        if (!hasAction)
        {
            Debug.LogError("No move!");
        }

        hasAction = false;
        var minPiece = player.GetMinPiece();
        
        for (var cell = 0; cell < 9; cell++)
        {
            var canMove = playground.CanMove(cell, minPiece);
            hasAction = hasAction || canMove;
            actionMask.SetActionEnabled(1, cell, canMove);
        }
        
        if (!hasAction)
        {
            Debug.LogError("No move!");
        }
    }
}