using UnityEngine;

public class ZoneVisualizer : MonoBehaviour
{
    public float radius = 3f;
    public Color gizmoColor = new Color(1f, 0.8f, 0f, 0.3f); // Gold transparent

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.1f);
        Gizmos.DrawSphere(transform.position, radius);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}