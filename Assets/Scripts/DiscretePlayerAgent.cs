using System;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class DiscretePlayerAgent : PlayerAgent
{
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        var hasAction = false;
        var actionCount = Player.PieceCount * Playground.CellCount;

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
    
    public override void CollectObservations(VectorSensor sensor)
    {
        for (var i = 0; i < Playground.CellCount; i++)
        {
            var piece = playground.GetCellPiece(i);
            sensor.AddOneHotObservation(piece ? piece.Number : -1, Player.PieceCount);
            sensor.AddObservation(piece && piece.Team == Team.Red);
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
    
    protected override Move ActionToMove(ActionBuffers actions)
    {
        return DiscreteActionToMove(actions.DiscreteActions[0]);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;
        actions[0] = MoveToDiscreteAction(_lastHeuristicMove);
    }
    
    private static Move DiscreteActionToMove(int action)
    {
        if (action is < 0 or >= Playground.CellCount * Player.PieceCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(action),
                $"Action value {action} is out of range. Valid range: 0 to {Playground.CellCount * Player.PieceCount - 1}."
            );
        }
        
        return new Move(action / Playground.CellCount, action % Playground.CellCount);
    }

    private static int MoveToDiscreteAction(Move move)
    {
        return move.Piece * Playground.CellCount + move.Cell;
    }
    
    protected override void SetupBrainParameters(BrainParameters parameters)
    {
        parameters.VectorObservationSize = 86;
        parameters.ActionSpec = ActionSpec.MakeDiscrete(63);
    }
}