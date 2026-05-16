using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to a UI GameObject that holds the heart/life icons.
/// 
/// Unity Setup:
/// 1. Create a Canvas → Panel (optional) → Empty GameObject named "HealthBarUI"
/// 2. Add this script to "HealthBarUI"
/// 3. Inside it, create 3 Image children named "Heart1", "Heart2", "Heart3"
///    - Assign a filled heart sprite to each Image
/// 4. Assign the 3 Image components to the heartIcons array in the Inspector
/// 5. Assign an empty heart sprite to emptyHeartSprite (optional, for a greyed-out look)
/// 6. Drag the HealthBarUI GameObject into PlayerHealth → healthBarUI field
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("Heart Icons")]
    public Image[] heartIcons;          // Assign 3 heart Image UI elements

    [Header("Sprites")]
    public Sprite fullHeartSprite;      // Red / filled heart
    public Sprite emptyHeartSprite;     // Grey / empty heart (optional)

    /// <summary>
    /// Call this whenever lives change. Pass current and max lives.
    /// </summary>
    public void SetLives(int current, int max)
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] == null) continue;

            if (i < max)
            {
                // This slot is within the max — show it
                heartIcons[i].gameObject.SetActive(true);

                if (i < current)
                {
                    // Still alive: full heart
                    heartIcons[i].sprite = fullHeartSprite;
                    heartIcons[i].color = Color.white;
                }
                else
                {
                    // Lost this life
                    if (emptyHeartSprite != null)
                    {
                        heartIcons[i].sprite = emptyHeartSprite;
                        heartIcons[i].color = Color.white;
                    }
                    else
                    {
                        // No empty sprite? Just grey out the full heart
                        heartIcons[i].sprite = fullHeartSprite;
                        heartIcons[i].color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                    }
                }
            }
            else
            {
                // Slots beyond maxLives are hidden
                heartIcons[i].gameObject.SetActive(false);
            }
        }
    }
}
