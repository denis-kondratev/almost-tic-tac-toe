using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AnalogPlayerAgent : PlayerAgent
{
    private readonly int[] _buffer = new int[Playground.CellCount];
    
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
            var value = sign * (piece ? (piece.Number + 1f) / Player.PieceCount : 0);
            sensor.AddObservation(value);
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
        var cell = ActionToCell(actions.ContinuousActions[0]);
        var piece = ActionToPiece(actions.ContinuousActions[1], cell);
        return new Move(piece, cell);
    }

    private int ActionToCell(float normalAction)
    {
        var minPiece = player.GetMinPiece();

        if (minPiece < 0)
        {
            throw new System.InvalidOperationException("Player has no pieces.");
        }
        
        var cellCount = playground.GetAvailableCells(minPiece, _buffer);
        
        if (cellCount == 0)
        {
            throw new System.InvalidOperationException("Player has no available moves.");
        }
        
        var cellIndex = NormalToIndex(normalAction, cellCount);
        return _buffer[cellIndex];
    }
    
    private int ActionToPiece(float normalAction, int cell)
    {
        var pieceCount = player.GetAvailablePieces(cell, _buffer);
        
        if (pieceCount == 0)
        {
            throw new System.InvalidOperationException("Player has no available pieces.");
        }
        
        var pieceIndex = NormalToIndex(normalAction, pieceCount);
        return _buffer[pieceIndex];
    }

    protected override void SetupBrainParameters(BrainParameters parameters)
    {
        parameters.VectorObservationSize = Player.PieceCount * 2 + Playground.CellCount;
        parameters.ActionSpec = ActionSpec.MakeContinuous(2);
    }
    
    private static int NormalToIndex(float normal, int count)
    {
        if (normal is < -1 or > 1)
        {
            throw new System.ArgumentOutOfRangeException(nameof(normal), normal, $"Value must be in [0, 1]. Actual value is {normal}.");
        }

        if (count <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(count), count, "Value must be positive.");
        }

        normal = (normal + 1f) / 2f;
        return normal is 1 ? count - 1 : Mathf.FloorToInt(normal * count);
    }
}