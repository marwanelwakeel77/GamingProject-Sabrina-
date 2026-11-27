using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SecurityGuardPatrol : EnemyController
{
    public float chaseSpeed = 3f;

    public Transform pointA;
    public Transform pointB;

    public float investigateInterval = 8f;
    public float investigateDuration = 3f;
    public float searchDuration = 2.5f;
    public float catchDistance = 0.55f;

    public Transform detectionArea;
    public SpriteRenderer flashlight;
    public Color calmLight = new Color(1f, 0.88f, 0.58f, 0.3f);
    public Color alertLight = new Color(1f, 0.45f, 0.25f, 0.35f);

    public AudioSource footstepSource;
    public AudioClip[] footsteps;
    public float stepTime = 0.3f;
    public float runStepTime = 0.22f;
    public float hearingDistance = 7f;
    public AudioClip alertSound;
    public AudioClip caughtSound;

    public Image blackScreen;
    public float fadeTime = 0.5f;

    private Transform target;
    private Transform player;
    private Transform alex;

    private Animator animator;
    private CameraFollow cameraFollow;

    private bool alerted = false;
    private bool chasing = false;
    private bool investigating = false;
    private bool searching = false;
    private bool walkingToSearchSpot = false;
    private bool catching = false;

    private float lastSeenX;
    private float searchX;
    private float stepTimer = 0f;
    private float fadeTarget = 0f;
    private Vector3 startPosition;
    private Coroutine searchRoutine;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        cameraFollow = FindObjectOfType<CameraFollow>();

        target = pointB;
        startPosition = transform.position;
        alex = FindObjectOfType<PlayerController>().transform;

        SetAnimation(true, false, false);
        StartCoroutine(InvestigateRoutine());
    }

    void Update()
    {
        if (blackScreen != null)
        {
            Color c = blackScreen.color;
            c.a = Mathf.MoveTowards(c.a, fadeTarget, Time.deltaTime / fadeTime);
            blackScreen.color = c;
        }

        if (catching)
            return;

        if (chasing && player != null)
        {
            lastSeenX = player.position.x;
            MoveTowards(player.position.x, chaseSpeed);
            SetAnimation(false, true, false);
            PlayFootsteps(runStepTime);

            if (Vector2.Distance(transform.position, player.position) <= catchDistance)
            {
                StartCoroutine(CatchPlayer());
            }
            return;
        }

        if (alerted)
            return;

        if (searching)
        {
            if (walkingToSearchSpot)
                WalkToSearchSpot();
            return;
        }

        if (investigating)
            return;

        MoveTowards(target.position.x, maxSpeed);
        SetAnimation(true, false, false);
        PlayFootsteps(stepTime);

        if (Mathf.Abs(transform.position.x - target.position.x) < 0.1f)
        {
            if (target == pointA)
                target = pointB;
            else
                target = pointA;
        }
    }

    float KeepInsidePatrol(float x)
    {
        if (pointA.position.x < pointB.position.x)
            return Mathf.Clamp(x, pointA.position.x, pointB.position.x);
        else
            return Mathf.Clamp(x, pointB.position.x, pointA.position.x);
    }

    void MoveTowards(float x, float speed)
    {
        x = KeepInsidePatrol(x);

        if (x < transform.position.x - 0.05f)
            FaceLeft(true);
        else if (x > transform.position.x + 0.05f)
            FaceLeft(false);

        Vector2 destination = new Vector2(x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);
    }

    void FaceLeft(bool left)
    {
        if (sr.flipX != left)
        {
            Flip();
        }

        if (sr.flipX == true)
            detectionArea.localScale = new Vector3(-1, 1, 1);
        else
            detectionArea.localScale = new Vector3(1, 1, 1);
    }

    void SetAnimation(bool walking, bool running, bool investigatingAnim)
    {
        animator.SetBool("IsWalking", walking);
        animator.SetBool("IsRunning", running);
        animator.SetBool("IsInvestigating", investigatingAnim);
        animator.SetBool("IsAlert", false);
    }

    void PlayFootsteps(float time)
    {
        stepTimer = stepTimer - Time.deltaTime;
        if (stepTimer <= 0)
        {
            stepTimer = time;

            float distance = Vector2.Distance(transform.position, alex.position);
            float volume = 1f - distance / hearingDistance;

            if (volume > 0 && footstepSource != null && footsteps.Length > 0)
            {
                int index = Random.Range(0, footsteps.Length);
                footstepSource.PlayOneShot(footsteps[index], volume);
            }
        }
    }

    public void StartChasing(Transform targetPlayer)
    {
        player = targetPlayer;
        lastSeenX = player.position.x;

        if (alerted || chasing || catching)
            return;

        alerted = true;
        investigating = false;

        if (searching)
        {
            if (searchRoutine != null)
                StopCoroutine(searchRoutine);
            searching = false;
            walkingToSearchSpot = false;
        }

        SetAnimation(false, false, false);
        animator.SetBool("IsAlert", true);
        flashlight.color = alertLight;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(alertSound);

        StartCoroutine(BeginChasingAfterAlert());
    }

    IEnumerator BeginChasingAfterAlert()
    {
        yield return new WaitForSeconds(0.75f);

        if (alerted)
        {
            alerted = false;
            chasing = true;
        }
    }

    public void StopChasing()
    {
        if (catching)
            return;

        if (chasing || alerted)
        {
            chasing = false;
            alerted = false;
            player = null;
            flashlight.color = calmLight;

            searchX = KeepInsidePatrol(lastSeenX);
            searching = true;
            walkingToSearchSpot = true;
        }
    }

    void WalkToSearchSpot()
    {
        if (Mathf.Abs(transform.position.x - searchX) > 0.1f)
        {
            MoveTowards(lastSeenX, maxSpeed);
            SetAnimation(true, false, false);
            PlayFootsteps(stepTime);
        }
        else
        {
            walkingToSearchSpot = false;
            searchRoutine = StartCoroutine(LookAround());
        }
    }

    IEnumerator LookAround()
    {
        SetAnimation(false, false, true);
        yield return new WaitForSeconds(searchDuration / 2);
        FaceLeft(!sr.flipX);
        yield return new WaitForSeconds(searchDuration / 2);

        searching = false;
    }

    IEnumerator InvestigateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(investigateInterval);

            if (chasing || alerted || searching || catching)
                continue;

            investigating = true;
            SetAnimation(false, false, true);

            yield return new WaitForSeconds(investigateDuration / 2);
            if (investigating)
                FaceLeft(!sr.flipX);

            yield return new WaitForSeconds(investigateDuration / 2);
            if (investigating)
                FaceLeft(!sr.flipX);

            investigating = false;
        }
    }

    IEnumerator CatchPlayer()
    {
        catching = true;
        chasing = false;
        SetAnimation(false, false, false);

        Rigidbody2D playerRB = player.GetComponent<Rigidbody2D>();
        playerRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(caughtSound);

        yield return new WaitForSeconds(0.6f);

        fadeTarget = 1f;
        yield return new WaitForSeconds(fadeTime);

        LevelManager levelManager = FindObjectOfType<LevelManager>();
        levelManager.RespawnPlayer();
        levelManager.ResetCipherPages();

        BookMechanism mechanism = FindObjectOfType<BookMechanism>();
        if (mechanism != null)
            mechanism.ResetMechanism();

        playerRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;

        transform.position = startPosition;
        target = pointB;
        player = null;
        flashlight.color = calmLight;
        FaceLeft(false);

        Vector3 alexPosition = playerRB.transform.position;
        cameraFollow.transform.position = new Vector3(alexPosition.x, alexPosition.y, -10f);
        yield return new WaitForSeconds(0.4f);

        fadeTarget = 0f;
        yield return new WaitForSeconds(fadeTime);
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;

        catching = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
    }
}
