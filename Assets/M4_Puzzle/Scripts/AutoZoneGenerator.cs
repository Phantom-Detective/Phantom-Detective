using UnityEngine;
using System.Collections.Generic;

public class AutoZoneGenerator : MonoBehaviour
{
    [Header("Rooms to Scan (Drag room floor/wall objects here)")]
    public List<GameObject> roomObjects = new List<GameObject>();

    [Header("Zone Settings")]
    public float heightAboveFloor = 0.5f;
    public float zoneRadius = 3f;

    [ContextMenu("Generate Zones")]
    public void Generate()
    {
        // Clear existing auto-generated zones
        Transform existingParent = transform.Find("AutoZones");
        if (existingParent != null)
            DestroyImmediate(existingParent.gameObject);

        // Create parent container
        GameObject parent = new GameObject("AutoZones");
        parent.transform.SetParent(transform);
        parent.transform.localPosition = Vector3.zero;

        for (int i = 0; i < roomObjects.Count; i++)
        {
            GameObject room = roomObjects[i];
            if (room == null) continue;

            // Get the center of the room using renderer bounds
            Renderer rend = room.GetComponent<Renderer>();
            Vector3 center;

            if (rend != null)
            {
                center = rend.bounds.center;
            }
            else
            {
                center = room.transform.position;
            }

            // Raycast down to find actual floor height
            Vector3 rayStart = new Vector3(center.x, center.y + 50f, center.z);
            float floorY = center.y;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 100f))
            {
                floorY = hit.point.y;
            }

            // Create the zone marker
            GameObject zone = new GameObject("Zone_" + CleanName(room.name));
            zone.transform.position = new Vector3(center.x, floorY + heightAboveFloor, center.z);
            zone.transform.SetParent(parent.transform);

            // Add a visual gizmo so you can see it in editor
            ZoneVisualizer visualizer = zone.AddComponent<ZoneVisualizer>();
            visualizer.radius = zoneRadius;

            Debug.Log("Created zone for " + room.name + " at " + zone.transform.position);
        }

        Debug.Log("Generated " + roomObjects.Count + " zones successfully!");
    }

    string CleanName(string name)
    {
        // Remove common suffixes/prefixes for cleaner names
        return name.Replace("_Floor", "")
                   .Replace("_Room", "")
                   .Replace("Room_", "")
                   .Replace("Floor_", "");
    }
}