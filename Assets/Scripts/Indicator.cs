using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    [SerializeField] private List<KeyedMaterial> materials;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Player player;

    private void OnEnable()
    {
        OnPlayerStateChanged(player.State);
        player.StateChanged += OnPlayerStateChanged;
    }
    
    private void OnDisable()
    {
        player.StateChanged -= OnPlayerStateChanged;
    }

    private void OnPlayerStateChanged(PlayerState state)
    {
        if (TryGetMaterial(state, out var material))
        {
            meshRenderer.material = material;
        }
    }

    private bool TryGetMaterial(PlayerState state, out Material material)
    {
        foreach (var keyedMaterial in materials)
        {
            if (keyedMaterial.state == state)
            {
                material = keyedMaterial.material;
                return true;
            }
        }

        material = null;
        return false;
    }
}