using System;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    [Range(0, 6)]
    public int size;
    public Team team;
    
    private Vector3 _initialPosition;

    private void Awake()
    {
        _initialPosition = transform.position;
    }
}