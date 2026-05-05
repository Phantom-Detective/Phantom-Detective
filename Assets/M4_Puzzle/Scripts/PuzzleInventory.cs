using UnityEngine;
using System.Collections.Generic;

public class PuzzleInventory : MonoBehaviour
{
    public static PuzzleInventory Instance;

    public List<FragmentPickup> collectedPieces = new List<FragmentPickup>();
    public int totalPieces = 9;

    public delegate void PieceCollected(int currentCount, int total);
    public static event PieceCollected OnPieceCollected;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddPiece(FragmentPickup piece)
    {
        if (!collectedPieces.Exists(p => p.pieceID == piece.pieceID))
        {
            collectedPieces.Add(piece);
            OnPieceCollected?.Invoke(collectedPieces.Count, totalPieces);
            Debug.Log("Collected: " + collectedPieces.Count + "/" + totalPieces);
        }
    }

    public int GetCollectedCount() => collectedPieces.Count;

    public void ResetInventory()
    {
        collectedPieces.Clear();
        OnPieceCollected?.Invoke(0, totalPieces);
        Debug.Log("Puzzle inventory reset to 0/" + totalPieces);
    }
}