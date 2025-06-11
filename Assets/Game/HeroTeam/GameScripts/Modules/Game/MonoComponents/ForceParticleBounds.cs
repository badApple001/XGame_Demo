using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystemRenderer))]
public class ForceParticleBounds : MonoBehaviour
{
    [Header("强制包围盒大小（单位）")]
    public Vector3 bound_size = new Vector3(50f, 50f, 50f);

    private ParticleSystemRenderer ps_renderer;

    private void Reset()
    {
        bound_size = new Vector3(50f, 50f, 50f);
    }

    private void OnEnable()
    {
        ps_renderer = GetComponent<ParticleSystemRenderer>();
        ApplyBounds();
    }

#if UNITY_EDITOR
    private void Update()
    {
        // Editor下实时更新
        ApplyBounds();
    }
#endif

    private void ApplyBounds()
    {
        if (ps_renderer != null)
        {
            ps_renderer.localBounds = new Bounds(Vector3.zero, bound_size);
        }
    }
}
