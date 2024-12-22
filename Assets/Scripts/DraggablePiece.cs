using UnityEngine;

public class DraggablePiece : MonoBehaviour
{
    private static readonly int IsDragging = Animator.StringToHash("IsDragging");
    [SerializeField] private Transform pieceTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask tableMask;
    private Camera _mainCamera;
    private Vector3 _offset;
    private bool _isDragging;

    void Start()
    {
        _mainCamera = Camera.main;
    }

    void OnMouseDown()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tableMask))
        {
            _offset = pieceTransform.position - hit.point;
            _isDragging = true;
            animator.SetBool(IsDragging, true);
        }
    }

    void LateUpdate()
    {
        if (!_isDragging) return;
        
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
        _isDragging = false;
        animator.SetBool(IsDragging, false);
    }
}