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
        IsWaitingAction = true;
        RequestDecision();
        RequestAction();
        Academy.Instance.EnvironmentStep();
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
        var pieceSize = actions.DiscreteActions[0];
        var cellIndex = actions.DiscreteActions[1];
        player.MakeMove(cellIndex, pieceSize);
        IsWaitingAction = false;
    }

    public void Reset()
    {
        player.Reset();
    }
}