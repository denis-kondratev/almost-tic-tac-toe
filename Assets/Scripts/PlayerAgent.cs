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
            sensor.AddObservation(cell ? (int)cell.Team + 1 : 0);
            sensor.AddObservation(cell ? cell.Number + 1 : 0);
        }

        var playerPieceCount = player.GetPieceCount();
        for (var i = 0; i < playerPieceCount; i++)
        {
            sensor.AddObservation(player.HasPiece(i) ? 1 : 0);
        }

        var rivalPieceCount = rivalPlayer.GetPieceCount();
        for (var i = 0; i < rivalPieceCount; i++)
        {
            sensor.AddObservation(rivalPlayer.HasPiece(i) ? 1 : 0);
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