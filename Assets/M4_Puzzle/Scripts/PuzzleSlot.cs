using UnityEngine;
using UnityEngine.UI;

public class PuzzleSlot : MonoBehaviour
{
    public int slotIndex;
    private PuzzlePiece currentPiece;

    public bool IsCorrectlyFilled()
    {
        if (currentPiece == null) return false;
        return currentPiece.correctSlotIndex == slotIndex;
    }

    public void PlacePiece(PuzzlePiece piece)
    {
        currentPiece = piece;
        piece.transform.SetParent(transform);
        piece.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    public void ClearSlot()
    {
        currentPiece = null;
    }

    // Call this when a piece is dragged out
    public void RemovePiece()
    {
        currentPiece = null;
    }
}