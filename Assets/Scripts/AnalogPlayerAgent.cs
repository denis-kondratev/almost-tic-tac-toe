using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AnalogPlayerAgent : PlayerAgent
{
    private readonly int[] _buffer = new int[Playground.CellCount];
    private AnalogMoveUtils _moveUtils;

    protected override void Awake()
    {
        base.Awake();
        _moveUtils = new AnalogMoveUtils(player, playground);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        for (var i = 0; i < Playground.CellCount; i++)
        {
            var piece = playground.GetCellPiece(i);

            if (piece == null)
            {
                sensor.AddObservation(0f);
                continue;
            }

            var sign = piece.Team == player.Team ? 1 : -1;
            var amount = (Player.PieceCount - piece.Number) / Player.PieceCount;
            sensor.AddObservation(sign * amount);
        }

        for (var i = 0; i < Player.PieceCount; i++)
        {
            sensor.AddObservation(player.HasPiece(i));
        }

        for (var i = 0; i < Player.PieceCount; i++)
        {
            sensor.AddObservation(rival.HasPiece(i));
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        var (cellAction, pieceAction) = MoveToContinuousActions(_lastHeuristicMove);
        actions[0] = cellAction;
        actions[1] = pieceAction;
    }

    private (int cellAction, int pieceAction) MoveToContinuousActions(Move _)
    {
        throw new System.NotImplementedException();
    }

    protected override Move ActionToMove(ActionBuffers actions)
    {
        return _moveUtils.GetMove(actions.ContinuousActions[0], actions.ContinuousActions[1]);
    }

    protected override void SetupBrainParameters(BrainParameters parameters)
    {
        parameters.VectorObservationSize = Player.PieceCount * 2 + Playground.CellCount;
        parameters.ActionSpec = ActionSpec.MakeContinuous(2);
    }
}