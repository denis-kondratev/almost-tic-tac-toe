using UnityEngine;

public class GamePiece : MonoBehaviour
{
    private static readonly int IsDragging = Animator.StringToHash("IsDragging");

    [Range(0, 6)]
    [field: SerializeField] public int Number { get; private set; }
    [field: SerializeField] public Team Team { get; private set; }
    [SerializeField] private float translateSpeed = 0.1f;
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationCurve translateCurve;
    
    public bool IsTranslating { get; private set; }

    private Vector3 _initialPosition;

    private void Awake()
    {
        _initialPosition = transform.position;
    }

    public void Reset()
    {
        transform.position = _initialPosition;
    }

    public async Awaitable TranslateToCell(GameCell gameCell)
    {
        IsTranslating = true;
        var targetPosition = gameCell.MountPosition;
        animator.SetBool(IsDragging, true);
        var pieceTransform = transform;
        var startPosition = pieceTransform.position;
        var elapsedTime = 0f;
        var duration = Vector3.Distance(startPosition, targetPosition) / translateSpeed;

        while (elapsedTime < duration)
        {
            var time = translateCurve.Evaluate(elapsedTime / duration);    
            pieceTransform.position = Vector3.Lerp(startPosition, targetPosition, time);
            elapsedTime += Time.deltaTime;
            await Awaitable.NextFrameAsync(destroyCancellationToken);
        }

        pieceTransform.position = targetPosition;
        animator.SetBool(IsDragging, false);
        IsTranslating = false;
    }
}