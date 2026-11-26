
using System.Collections;
using UnityEngine;

// The Level 3 security guard. He inherits from the EnemyController super class (Lab 6).
// He patrols between two points on the upper floor, investigates now and then,
// chases Alex when he is spotted, searches where Alex was last seen, and catches him if he gets close.
public class SecurityGuardPatrol : EnemyController
{
    public float chaseSpeed = 3f;

    public Transform pointA;
    public Transform pointB;

    public float investigateInterval = 8f;
    public float investigateDuration = 3f;
    public float searchDuration = 2.5f;
    public float catchDistance = 0.55f;

    // the detection area and the flashlight are children of this object,
    // and they turn around with the guard
    public Transform detectionArea;
    public SpriteRenderer flashlight;
    public Color calmLight = new Color(1f, 0.88f, 0.58f, 0.3f);
    public Color alertLight = new Color(1f, 0.45f, 0.25f, 0.35f);

    // sounds (Lab 7): footsteps play on the guard's own AudioSource
    public AudioSource footstepSource;
    public AudioClip[] footsteps;
    public float stepTime = 0.3f;      // time between steps when he walks
    public float runStepTime = 0.22f;  // time between steps when he runs
    public float hearingDistance = 7f; // Alex can't hear his steps further away than this
    public AudioClip alertSound;
    public AudioClip caughtSound;

    private Transform target;
    private Transform player;
    private Transform alex; // used to make his footsteps quieter the further away Alex is

    private Animator animator;

    private bool alerted = false;     // he has seen Alex
    private bool chasing = false;     // he is running after Alex
    private bool investigating = false;
    private bool searching = false;
    private bool catching = false;

    private float lastSeenX;
    private float stepTimer = 0f;
    private Vector3 startPosition;
    private Coroutine searchRoutine;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        target = pointB;
        startPosition = transform.position;
        alex = FindObjectOfType<PlayerController>().transform;

        SetAnimation(true, false, false);
        StartCoroutine(InvestigateRoutine());
    }

    void Update()
    {
        if (catching)
            return;

        // CHASE: run towards Alex (like the FollowerEnemy in Lab 6)
        if (chasing && player != null)
        {
            lastSeenX = player.position.x;
            MoveTowards(player.position.x, chaseSpeed);
            SetAnimation(false, true, false);
            PlayFootsteps(runStepTime);

            // close enough: Alex is caught
            if (Vector2.Distance(transform.position, player.position) <= catchDistance)
            {
                StartCoroutine(CatchPlayer());
            }
            return;
        }

        // ALERT: he stands still for a moment before running (the Alert animation plays)
        if (alerted)
            return;

        // SEARCH and INVESTIGATE are handled by their coroutines
        if (searching || investigating)
            return;

        // PATROL between point A and point B
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

    // Moves the guard left or right only, and never past his patrol points,
    // so he can't follow Alex down the stairs
    void MoveTowards(float x, float speed)
    {
        float minX = Mathf.Min(pointA.position.x, pointB.position.x);
        float maxX = Mathf.Max(pointA.position.x, pointB.position.x);
        x = Mathf.Clamp(x, minX, maxX);

        if (x < transform.position.x - 0.05f)
            FaceLeft(true);
        else if (x > transform.position.x + 0.05f)
            FaceLeft(false);

        Vector2 destination = new Vector2(x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);
    }

    // turns the sprite with Flip() from the super class, and turns the detection area and flashlight with it
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
    }

    void PlayFootsteps(float time)
    {
        stepTimer = stepTimer - Time.deltaTime;
        if (stepTimer <= 0)
        {
            stepTimer = time;

            // the closer Alex is, the louder the steps (silent beyond the hearing distance)
            float distance = Vector2.Distance(transform.position, alex.position);
            float volume = 1f - distance / hearingDistance;

            if (volume > 0 && footstepSource != null && footsteps.Length > 0)
            {
                int index = Random.Range(0, footsteps.Length);
                footstepSource.PlayOneShot(footsteps[index], volume);
            }
        }
    }

    // Called by the detection area while Alex is inside the flashlight
    public void StartChasing(Transform targetPlayer)
    {
        player = targetPlayer;
        lastSeenX = player.position.x;

        // already alerted (or busy catching): don't start the alert again
        if (alerted || chasing || catching)
            return;

        alerted = true;
        investigating = false;

        if (searching)
        {
            StopCoroutine(searchRoutine);
            searching = false;
        }

        SetAnimation(false, false, false);
        animator.SetTrigger("Alert");
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

    // Called by the detection area when Alex gets out of the flashlight
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

            searchRoutine = StartCoroutine(SearchRoutine());
        }
    }

    // walk to where Alex was last seen and look around with the flashlight
    IEnumerator SearchRoutine()
    {
        searching = true;

        // he can't go past his patrol points, so search at the closest point he can reach
        float searchX = Mathf.Clamp(lastSeenX, Mathf.Min(pointA.position.x, pointB.position.x), Mathf.Max(pointA.position.x, pointB.position.x));

        while (Mathf.Abs(transform.position.x - searchX) > 0.1f)
        {
            MoveTowards(lastSeenX, maxSpeed);
            SetAnimation(true, false, false);
            PlayFootsteps(stepTime);
            yield return null;
        }

        SetAnimation(false, false, true);
        yield return new WaitForSeconds(searchDuration / 2);
        FaceLeft(!sr.flipX); //look the other way
        yield return new WaitForSeconds(searchDuration / 2);

        searching = false; //back to patrol
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
                FaceLeft(!sr.flipX); //look behind him

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

        // freeze Alex in place, like during dialogue in Lab 9
        Rigidbody2D playerRB = player.GetComponent<Rigidbody2D>();
        playerRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(caughtSound);

        yield return new WaitForSeconds(1f);

        // back to the checkpoint, and every collected cipher page is lost
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        levelManager.RespawnPlayer();
        levelManager.ResetCipherPages();

        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;

        // the guard goes back to the start of his patrol
        transform.position = startPosition;
        target = pointB;
        player = null;
        flashlight.color = calmLight;
        FaceLeft(false);

        catching = false;
    }

    // The guard doesn't hurt Alex by touching him like the other enemies do:
    // he catches him instead, so the super class's trigger function is overridden (like GhostEnemy in Lab 6)
    void OnTriggerEnter2D(Collider2D other)
    {
    }
}
