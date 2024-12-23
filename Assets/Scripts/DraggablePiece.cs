using System;
using UnityEngine;

public class DraggablePiece : MonoBehaviour
{
    private static readonly int IsDragging = Animator.StringToHash("IsDragging");
    [SerializeField] private Transform pieceTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask tableMask;
    [SerializeField] private GamePiece piece;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private LayerMask cellMask;
    [SerializeField] private Player player;
    private Camera _mainCamera;
    private Vector3 _offset;
    private Vector3 _targetPosition;
    private State _state;
    private Vector3 _startDragPosition;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        if (!piece.IsTranslating && _state is State.Idle && player.State is PlayerState.WaitingForMove)
        {
            StartDragging();
        }
    }

    private void StartDragging()
    {
        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tableMask))
        {
            _offset = pieceTransform.position - hit.point;
            _state = State.Dragging;
            animator.SetBool(IsDragging, true);
            _startDragPosition = pieceTransform.position;
        }
    }

    private void LateUpdate()
    {
        switch (_state)
        {
            case State.Idle:
                break;
            case State.Moving:
                MovePiece(Time.deltaTime * moveSpeed);
                break;
            case State.Dragging:
                DragPiece();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void MovePiece(float deltaTime)
    {
        pieceTransform.position = Vector3.MoveTowards(pieceTransform.position, _targetPosition, deltaTime);
        
        if (pieceTransform.position == _targetPosition)
        {
            _state = State.Idle;
        }
    }

    private void DragPiece()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tableMask))
        {
            Vector3 targetPosition = hit.point + _offset;
            targetPosition.y = pieceTransform.position.y;
            pieceTransform.position = targetPosition;
        }
    }

    void OnMouseUp()
    {
        if (_state == State.Dragging)
        {
            EndDragging();
        }
    }

    private void EndDragging()
    {
        _state = State.Moving;
        animator.SetBool(IsDragging, false);

        if (player.State is PlayerState.WaitingForMove 
            && TryFindCell(out var cell)
            && player.TryMakeMove(piece, cell))
        {
            _targetPosition = cell.MountPosition;
            return;
        }

        _targetPosition = _startDragPosition;
    }

    private bool TryFindCell(out GameCell cell)
    {
        var ray = new Ray(pieceTransform.position + Vector3.up * 2, Vector3.down);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, cellMask))
        {
            cell = hit.collider.GetComponent<GameCell>();
            return true;
        }

        cell = null;
        return false;
    }

    private enum State
    {
        Idle,
        Moving,
        Dragging
    }
}