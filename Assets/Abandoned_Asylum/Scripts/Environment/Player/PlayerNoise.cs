using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    public float currentNoiseLevel = 0f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float speed = controller.velocity.magnitude;

        if (speed > 5f)
            currentNoiseLevel = 1.0f;    // running = loud
        else if (speed > 0.1f)
            currentNoiseLevel = 0.4f;    // walking = moderate
        else
            currentNoiseLevel = 0.0f;    // still = silent
    }
}