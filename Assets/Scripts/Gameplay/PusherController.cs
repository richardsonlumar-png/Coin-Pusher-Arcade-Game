using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controls the pusher platform movement and coin dropping mechanics
/// Handles touch input and manages the push animation
/// </summary>
public class PusherController : MonoBehaviour
{
    [System.Serializable]
    public class PusherSettings
    {
        [Header("Movement")]
        public float movementSpeed = 5f;
        public float movementRange = 2f;  // How far pusher moves left/right
        public float movementAmplitude = 1f;
        public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Push Mechanics")]
        public float pushForce = 10f;
        public float pushDuration = 0.2f;
        public float pushCooldown = 0.5f;

        [Header("Coin Drop")]
        public float dropHeight = 3f;
        public float dropForce = 5f;
        public int maxCoinsPerDrop = 5;
        public float timeBetweenCoinDrops = 0.05f;
    }

    [SerializeField] private PusherSettings pusherSettings = new PusherSettings();
    [SerializeField] private Transform pusherTransform;
    [SerializeField] private Transform coinSpawnPoint;
    [SerializeField] private ParticleSystem pushParticleEffect;

    private float currentMovementTime = 0f;
    private float lastPushTime = -1f;
    private float lastCoinDropTime = -1f;
    private int currentCoinsInQueue = 0;
    private bool isPushing = false;
    private Vector3 pusherStartPosition;

    // Touch input tracking
    private bool isTouching = false;
    private Vector2 touchStartPosition;
    private float touchHoldDuration = 0f;

    private void Start()
    {
        if (pusherTransform == null)
            pusherTransform = transform;
        if (coinSpawnPoint == null)
            coinSpawnPoint = transform.Find("SpawnPoint");

        pusherStartPosition = pusherTransform.position;
    }

    private void Update()
    {
        if (GameManager.Instance?.GetGameState() != GameManager.GameState.InGame)
            return;

        // Update pusher movement
        UpdatePusherMovement();

        // Handle touch input
        HandleTouchInput();

        // Update push cooldown
        if (Time.time - lastPushTime > pusherSettings.pushCooldown)
        {
            lastPushTime = -1f;
        }
    }

    /// <summary>
    /// Update pusher platform movement (continuous back and forth)
    /// </summary>
    private void UpdatePusherMovement()
    {
        currentMovementTime += Time.deltaTime;
        float t = Mathf.PingPong(currentMovementTime * pusherSettings.movementSpeed, 1f);
        float curve = pusherSettings.movementCurve.Evaluate(t);
        float offset = Mathf.Lerp(-pusherSettings.movementRange, pusherSettings.movementRange, curve);

        Vector3 newPosition = pusherStartPosition;
        newPosition.x += offset;
        pusherTransform.position = newPosition;
    }

    /// <summary>
    /// Handle touch input for coin dropping
    /// </summary>
    private void HandleTouchInput()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isTouching = true;
                    touchStartPosition = touch.position;
                    touchHoldDuration = 0f;
                    break;

                case TouchPhase.Moved:
                    if (isTouching)
                    {
                        touchHoldDuration += Time.deltaTime;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isTouching && currentCoinsInQueue == 0)
                    {
                        int coinsToSpawn = Mathf.Min(pusherSettings.maxCoinsPerDrop, Mathf.Max(1, Mathf.FloorToInt(touchHoldDuration * 10f)));
                        SpawnCoins(coinsToSpawn);
                    }
                    isTouching = false;
                    touchHoldDuration = 0f;
                    break;
            }
        }
#else
        // Mouse input for editor testing
        if (Input.GetMouseButton(0))
        {
            if (!isTouching)
            {
                isTouching = true;
                touchHoldDuration = 0f;
            }
            touchHoldDuration += Time.deltaTime;
        }
        else if (isTouching)
        {
            if (currentCoinsInQueue == 0)
            {
                int coinsToSpawn = Mathf.Min(pusherSettings.maxCoinsPerDrop, Mathf.Max(1, Mathf.FloorToInt(touchHoldDuration * 10f)));
                SpawnCoins(coinsToSpawn);
            }
            isTouching = false;
            touchHoldDuration = 0f;
        }
#endif
    }

    /// <summary>
    /// Spawn coins and schedule their drops
    /// </summary>
    private void SpawnCoins(int count)
    {
        currentCoinsInQueue = count;
        lastCoinDropTime = Time.time;
    }

    /// <summary>
    /// Perform the push action and apply force to coins
    /// </summary>
    private void PerformPush()
    {
        if (Time.time - lastPushTime < pusherSettings.pushCooldown)
            return;

        lastPushTime = Time.time;
        isPushing = true;
        StartCoroutine(PushCoroutine());

        // Apply force to nearby coins
        ApplyPusherForce();

        // Particle effect
        if (pushParticleEffect != null)
        {
            pushParticleEffect.Emit(10);
        }

        // Play sound
        AudioManager.Instance?.PlaySFX("pusher_push");
    }

    /// <summary>
    /// Apply pushing force to coins near the pusher
    /// </summary>
    private void ApplyPusherForce()
    {
        CoinPhysicsEngine coinPhysics = CoinPhysicsEngine.Instance;
        if (coinPhysics == null) return;

        List<CoinInstance> coins = coinPhysics.GetActiveCoinList();
        float pushDistance = pusherSettings.movementRange + 1f;

        foreach (CoinInstance coin in coins)
        {
            float distanceY = coin.position.y - pusherTransform.position.y;
            if (distanceY > -0.5f && distanceY < 0.5f)
            {
                float distance = Vector3.Distance(
                    new Vector3(coin.position.x, 0, coin.position.z),
                    new Vector3(pusherTransform.position.x, 0, pusherTransform.position.z)
                );

                if (distance < pushDistance)
                {
                    Vector3 pushDirection = (coin.position - pusherTransform.position).normalized;
                    coin.velocity += pushDirection * pusherSettings.pushForce;
                }
            }
        }
    }

    /// <summary>
    /// Coroutine for push animation
    /// </summary>
    private System.Collections.IEnumerator PushCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 originalPosition = pusherTransform.position;
        Vector3 pushedPosition = originalPosition + Vector3.forward * 0.3f;

        while (elapsedTime < pusherSettings.pushDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / pusherSettings.pushDuration;
            pusherTransform.position = Vector3.Lerp(originalPosition, pushedPosition, t);
            yield return null;
        }

        pusherTransform.position = originalPosition;
        isPushing = false;
    }

    /// <summary>
    /// Get current touch hold duration
    /// </summary>
    public float GetTouchHoldDuration() => isTouching ? touchHoldDuration : 0f;

    /// <summary>
    /// Get estimated coin count based on touch duration
    /// </summary>
    public int GetEstimatedCoinCount() => Mathf.Min(pusherSettings.maxCoinsPerDrop, Mathf.Max(1, Mathf.FloorToInt(touchHoldDuration * 10f)));
}
