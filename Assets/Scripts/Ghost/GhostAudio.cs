using UnityEngine;

public class GhostAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource idleSource;
    public AudioSource walkSource;
    public AudioSource growlSource;

    [Header("Footstep Settings")]
    public float walkPitch = 1f;
    public float runPitch = 1.6f;

    [Header("Fade Settings")]
    public float fadeSpeed = 3f;

    private GhostAI ghostAI;
    private UnityEngine.AI.NavMeshAgent agent;
    private GhostAI.GhostState previousState;

    // Tracks if we already growled for this chase/lose event
    private bool growledOnChase = false;
    private bool growledOnLose = false;

    void Start()
    {
        ghostAI = GetComponent<GhostAI>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (ghostAI == null)
            Debug.LogError("GhostAudio: No GhostAI found on " + gameObject.name);
        if (agent == null)
            Debug.LogError("GhostAudio: No NavMeshAgent found on " + gameObject.name);

        PlayOnly(idleSource);
    }

    void Update()
    {
        if (ghostAI == null || agent == null) return;
        HandleStateChange();
        HandleVolumeFade();
    }

    void HandleStateChange()
    {
        GhostAI.GhostState currentState = ghostAI.currentState;

        if (currentState != previousState)
        {
            // Entered Chase → growl ONCE
            if (currentState == GhostAI.GhostState.Chase
                && !growledOnChase)
            {
                PlayGrowl();
                growledOnChase = true;
                growledOnLose = false;
            }

            // Entered Search (lost player) → growl ONCE
            if (currentState == GhostAI.GhostState.Search
                && previousState == GhostAI.GhostState.Chase
                && !growledOnLose)
            {
                PlayGrowl();
                growledOnLose = true;
                growledOnChase = false;
            }

            // Back to Patrol → reset everything
            if (currentState == GhostAI.GhostState.Patrol)
            {
                growledOnChase = false;
                growledOnLose = false;
            }

            previousState = currentState;
        }

        float speed = agent.velocity.magnitude;

        switch (currentState)
        {
            case GhostAI.GhostState.Patrol:
            case GhostAI.GhostState.Search:
                if (speed > 0.1f)
                {
                    walkSource.pitch = walkPitch;
                    SetLoopingSound(walkSource);
                }
                else
                    SetLoopingSound(idleSource);
                break;

            case GhostAI.GhostState.Chase:
                walkSource.pitch = runPitch;
                SetLoopingSound(walkSource);
                break;

            case GhostAI.GhostState.Disappear:
                StopAllLooping();
                break;
        }
    }

    void HandleVolumeFade()
    {
        idleSource.volume = Mathf.MoveTowards(
            idleSource.volume,
            idleSource.isPlaying ? 0.5f : 0f,
            fadeSpeed * Time.deltaTime);

        walkSource.volume = Mathf.MoveTowards(
            walkSource.volume,
            walkSource.isPlaying ? 0.6f : 0f,
            fadeSpeed * Time.deltaTime);
    }

    void SetLoopingSound(AudioSource source)
    {
        if (!source.isPlaying)
            source.Play();

        if (source != idleSource && idleSource.isPlaying)
            idleSource.Stop();
        if (source != walkSource && walkSource.isPlaying)
            walkSource.Stop();
    }

    void PlayOnly(AudioSource source)
    {
        StopAllLooping();
        source.Play();
    }

    void StopAllLooping()
    {
        idleSource.Stop();
        walkSource.Stop();
    }

    void PlayGrowl()
    {
        if (growlSource == null) return;
        growlSource.Play();
        Debug.Log("Ghost growl!");
    }
}