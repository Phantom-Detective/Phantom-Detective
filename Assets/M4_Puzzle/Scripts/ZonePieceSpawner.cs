using UnityEngine;
using System.Collections.Generic;

public class ZonePieceSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PieceZone
    {
        public GameObject piecePrefab;
        public List<Transform> possibleZones;
        public float zoneRadius = 3f;
    }

    public List<PieceZone> pieces = new List<PieceZone>();

    void Start()
    {
        SpawnAllPieces();
    }

    void SpawnAllPieces()
    {
        foreach (var piece in pieces)
        {
            if (piece.piecePrefab == null) continue;
            if (piece.possibleZones == null || piece.possibleZones.Count == 0) continue;

            int randomIndex = Random.Range(0, piece.possibleZones.Count);
            Transform chosenZone = piece.possibleZones[randomIndex];

            Vector2 randomCircle = Random.insideUnitCircle * piece.zoneRadius;
            Vector3 spawnPos = chosenZone.position + new Vector3(randomCircle.x, 0.5f, randomCircle.y);

            if (Physics.Raycast(spawnPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
            {
                spawnPos.y = hit.point.y + 0.1f;
            }

            Quaternion randomRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            GameObject spawned = Instantiate(piece.piecePrefab, spawnPos, randomRot);

            Transform parent = GameObject.Find("PuzzlePieces")?.transform;
            if (parent != null)
                spawned.transform.SetParent(parent);
            else
                spawned.transform.SetParent(this.transform);
        }

        Debug.Log("All puzzle pieces spawned!");
    }
}