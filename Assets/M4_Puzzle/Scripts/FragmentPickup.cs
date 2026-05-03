using UnityEngine;

public class FragmentPickup : MonoBehaviour
{
    [Header("Piece Data")]
    public string pieceID;
    public int correctSlotIndex;
    public Sprite pieceSprite;

    [Header("Materials")]
    public Material normalMaterial;
    public Material highlightMaterial;

    private MeshRenderer meshRenderer;
    private bool isHighlighted = false;
    private bool isCollected = false;
    private Vector3 startPosition;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        startPosition = transform.position;
        gameObject.tag = "PuzzlePiece";
    }

    void Update()
    {
        if (isCollected) return;

        // Floating animation
        float newY = startPosition.y + Mathf.Sin(Time.time * 1.5f) * 0.1f;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        transform.Rotate(Vector3.up, 30f * Time.deltaTime);
    }

    public void Highlight(bool active)
    {
        if (isCollected || meshRenderer == null) return;

        if (active && !isHighlighted)
        {
            meshRenderer.material = highlightMaterial;
            isHighlighted = true;
        }
        else if (!active && isHighlighted)
        {
            meshRenderer.material = normalMaterial;
            isHighlighted = false;
        }
    }

    public void Collect()
    {
        if (isCollected) return;
        isCollected = true;

        PuzzleInventory.Instance.AddPiece(this);
        Destroy(gameObject);
    }
}