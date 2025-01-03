using UnityEngine;

public class AiRandomPlayer : AiPlayer
{
    [SerializeField] protected Playground playground;

    private AnalogMoveUtils _moveUtils;

    private void Awake()
    {
        _moveUtils = new AnalogMoveUtils(player, playground);
    }

    protected override Move GetMove()
    {
        return _moveUtils.GetMove(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
    }
}