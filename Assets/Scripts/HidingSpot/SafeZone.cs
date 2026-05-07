using UnityEngine;
using TMPro;
using System.Collections;

public class SafeZone : MonoBehaviour
{
    public GameObject safeZonePanel;    // assign the panel
    public TextMeshProUGUI safeZoneText;
    public int flashCount = 3;
    public float flashSpeed = 0.2f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            StartCoroutine(FlashText());
    }

    IEnumerator FlashText()
    {
        for (int i = 0; i < flashCount; i++)
        {
            safeZonePanel.SetActive(true);
            yield return new WaitForSeconds(flashSpeed);
            safeZonePanel.SetActive(false);
            yield return new WaitForSeconds(flashSpeed);
        }
    }
}