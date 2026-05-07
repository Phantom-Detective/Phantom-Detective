using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource walkSource;      // walking/running footsteps
    public AudioSource breatheSource;   // breathing
    public AudioSource torchSource;     // torch click sound

    [Header("Footstep Settings")]
    public float walkPitch = 1f;        // normal walk
    public float runPitch = 1.6f;       // faster when running

    [Header("Breathing Settings")]
    public float idleBreathVolume = 0.3f;
    public float runBreathVolume = 0.7f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Always breathing
        if (breatheSource != null)
            breatheSource.Play();
    }

    void Update()
    {
        if (controller == null) return;

        float speed = controller.velocity.magnitude;
        var keyboard = Keyboard.current;
        bool isRunning = keyboard != null &&
                         keyboard.leftShiftKey.isPressed;

        // Footsteps
        if (speed > 0.1f)
        {
            walkSource.pitch = isRunning ? runPitch : walkPitch;
            if (!walkSource.isPlaying)
                walkSource.Play();
        }
        else
        {
            if (walkSource.isPlaying)
                walkSource.Stop();
        }

        // Breathing louder when running
        if (breatheSource != null)
        {
            float targetVolume = isRunning && speed > 0.1f
                ? runBreathVolume
                : idleBreathVolume;

            breatheSource.volume = Mathf.MoveTowards(
                breatheSource.volume, targetVolume,
                Time.deltaTime * 2f);
        }
    }

    // Called from PlayerMovement when torch toggled
    public void PlayTorchSound()
    {
        if (torchSource != null)
            torchSource.PlayOneShot(torchSource.clip);
    }
}