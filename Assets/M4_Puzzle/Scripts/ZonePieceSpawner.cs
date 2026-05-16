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
    private List<GameObject> spawnedWorldPieces = new List<GameObject>();

    void Start()
    {
        Debug.Log("ZonePieceSpawner Start() called. Pieces count: " + pieces.Count);
        SpawnAllPieces();
    }

    void SpawnAllPieces()
    {
        Debug.Log("SpawnAllPieces() starting...");

        // Create/find PuzzlePieces parent
        GameObject parentObj = GameObject.Find("PuzzlePieces");
        if (parentObj == null)
        {
            parentObj = new GameObject("PuzzlePieces");
            Debug.Log("Created PuzzlePieces parent");
        }

        foreach (var piece in pieces)
        {
            if (piece.piecePrefab == null)
            {
                Debug.LogWarning("Piece prefab is null! Skipping.");
                continue;
            }
            if (piece.possibleZones == null || piece.possibleZones.Count == 0)
            {
                Debug.LogWarning("Possible zones empty! Skipping " + piece.piecePrefab.name);
                continue;
            }

            int randomIndex = Random.Range(0, piece.possibleZones.Count);
            Transform chosenZone = piece.possibleZones[randomIndex];

            if (chosenZone == null)
            {
                Debug.LogError("Chosen zone is NULL!");
                continue;
            }

            Vector2 randomCircle = Random.insideUnitCircle * piece.zoneRadius;
            Vector3 spawnPos = chosenZone.position + new Vector3(randomCircle.x, 0.5f, randomCircle.y);

            if (Physics.Raycast(spawnPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
            {
                spawnPos.y = hit.point.y + 0.1f;
            }
            else
            {
                Debug.LogWarning("Raycast missed floor at " + spawnPos);
            }

            Quaternion randomRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            GameObject spawned = Instantiate(piece.piecePrefab, spawnPos, randomRot);
            spawnedWorldPieces.Add(spawned);
            spawned.transform.SetParent(parentObj.transform);

            Debug.Log("Spawned: " + spawned.name + " at " + spawnPos);
        }

        Debug.Log("SpawnAllPieces() finished. Total spawned: " + spawnedWorldPieces.Count);
    }

    public void RespawnPieces()
    {
        Debug.Log("RespawnPieces() called. Old count: " + spawnedWorldPieces.Count);

        // Destroy old pieces
        foreach (var piece in spawnedWorldPieces)
        {
            if (piece != null)
            {
                Debug.Log("Destroying old piece: " + piece.name);
                Destroy(piece);
            }
        }
        spawnedWorldPieces.Clear();

        Debug.Log("Old pieces destroyed. Spawning new ones...");
        SpawnAllPieces();
        Debug.Log("RespawnPieces() complete!");
    }
}