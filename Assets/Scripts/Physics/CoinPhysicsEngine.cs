using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Advanced coin physics engine with custom simulation
/// Handles realistic coin behavior: stacking, sliding, bouncing, falling
/// Optimized for mobile with object pooling
/// </summary>
public class CoinPhysicsEngine : MonoBehaviour
{
    [System.Serializable]
    public class PhysicsSettings
    {
        [Header("Gravity")]
        public float gravityScale = 1f;
        public Vector3 gravityDirection = Vector3.down;

        [Header("Coin Properties")]
        public float coinMass = 0.1f;
        public float coinRadius = 0.15f;
        public float coinThickness = 0.03f;

        [Header("Physics Parameters")]
        public float bounceDamping = 0.6f;  // Energy loss on bounce (0-1)
        public float friction = 0.3f;        // Friction coefficient
        public float airResistance = 0.01f;  // Air drag
        public float stoppingVelocity = 0.05f; // Below this, coin stops

        [Header("Collision")]
        public float coinCoinCollisionResponse = 0.8f;
        public float coinPusherCollisionResponse = 0.9f;
        public int maxContactsPerFrame = 8;
    }

    [SerializeField] private PhysicsSettings physicsSettings = new PhysicsSettings();
    [SerializeField] private int maxPoolSize = 100;
    [SerializeField] private float simulationDeltaTime = 0.016f; // 60 FPS

    // Coin pool for object reuse
    private Queue<CoinInstance> coinPool = new Queue<CoinInstance>();
    private List<CoinInstance> activeCoinList = new List<CoinInstance>();

    // Physics optimization
    private const int SPATIAL_GRID_SIZE = 20;
    private Dictionary<Vector3Int, List<CoinInstance>> spatialGrid = new Dictionary<Vector3Int, List<CoinInstance>>();

    public static CoinPhysicsEngine Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeCoinPool();
    }

    /// <summary>
    /// Initialize object pool with coins
    /// </summary>
    private void InitializeCoinPool()
    {
        for (int i = 0; i < maxPoolSize; i++)
        {
            CoinInstance coin = new CoinInstance();
            coin.Initialize(physicsSettings);
            coinPool.Enqueue(coin);
        }
    }

    /// <summary>
    /// Spawn a coin at the specified position
    /// </summary>
    public CoinInstance SpawnCoin(Vector3 position, Vector3 velocity = default, int coinValue = 1, CoinType coinType = CoinType.Normal)
    {
        CoinInstance coin;

        if (coinPool.Count > 0)
        {
            coin = coinPool.Dequeue();
            coin.Reuse();
        }
        else
        {
            coin = new CoinInstance();
            coin.Initialize(physicsSettings);
        }

        coin.position = position;
        coin.velocity = velocity;
        coin.coinValue = coinValue;
        coin.coinType = coinType;
        coin.isActive = true;

        activeCoinList.Add(coin);
        return coin;
    }

    /// <summary>
    /// Simulate physics for all active coins
    /// </summary>
    private void Update()
    {
        if (GameManager.Instance?.GetGameState() != GameManager.GameState.InGame)
            return;

        // Update spatial grid
        UpdateSpatialGrid();

        // Simulate each coin
        for (int i = activeCoinList.Count - 1; i >= 0; i--)
        {
            CoinInstance coin = activeCoinList[i];

            if (!coin.isActive)
            {
                activeCoinList.RemoveAt(i);
                coinPool.Enqueue(coin);
                continue;
            }

            // Apply physics simulation
            SimulateCoin(coin);

            // Check collisions
            CheckCollisions(coin);
        }
    }

    /// <summary>
    /// Simulate single coin physics
    /// </summary>
    private void SimulateCoin(CoinInstance coin)
    {
        // Apply gravity
        Vector3 gravityForce = physicsSettings.gravityDirection.normalized * 9.81f * physicsSettings.gravityScale;
        coin.velocity += gravityForce * simulationDeltaTime;

        // Apply air resistance
        coin.velocity *= (1f - physicsSettings.airResistance);

        // Check if coin should stop
        if (coin.velocity.magnitude < physicsSettings.stoppingVelocity && coin.position.y < 0.5f)
        {
            coin.velocity = Vector3.zero;
        }

        // Update position
        coin.position += coin.velocity * simulationDeltaTime;
    }

    /// <summary>
    /// Check and handle collisions for a coin
    /// </summary>
    private void CheckCollisions(CoinInstance coin)
    {
        Vector3Int gridCell = GetGridCell(coin.position);
        int contactCount = 0;

        // Check nearby coins
        for (int x = -1; x <= 1; x++)
        for (int y = -1; y <= 1; y++)
        for (int z = -1; z <= 1; z++)
        {
            Vector3Int neighborCell = gridCell + new Vector3Int(x, y, z);
            if (spatialGrid.TryGetValue(neighborCell, out List<CoinInstance> neighbors))
            {
                foreach (CoinInstance other in neighbors)
                {
                    if (other == coin) continue;
                    if (contactCount >= physicsSettings.maxContactsPerFrame) break;

                    if (CheckCoinCollision(coin, other))
                    {
                        ResolveCoinCollision(coin, other);
                        contactCount++;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if two coins are colliding
    /// </summary>
    private bool CheckCoinCollision(CoinInstance coin1, CoinInstance coin2)
    {
        float distance = Vector3.Distance(coin1.position, coin2.position);
        return distance < (physicsSettings.coinRadius * 2);
    }

    /// <summary>
    /// Resolve collision between two coins
    /// </summary>
    private void ResolveCoinCollision(CoinInstance coin1, CoinInstance coin2)
    {
        Vector3 collisionNormal = (coin2.position - coin1.position).normalized;
        float relativeVelocity = Vector3.Dot(coin1.velocity - coin2.velocity, collisionNormal);

        if (relativeVelocity < 0) return; // Already separating

        // Calculate impulse
        float impulse = relativeVelocity * physicsSettings.coinCoinCollisionResponse;

        // Apply impulse
        coin1.velocity -= collisionNormal * impulse;
        coin2.velocity += collisionNormal * impulse;

        // Separate coins to prevent overlap
        float overlap = (physicsSettings.coinRadius * 2) - Vector3.Distance(coin1.position, coin2.position);
        if (overlap > 0)
        {
            Vector3 separation = collisionNormal * (overlap / 2f + 0.001f);
            coin1.position -= separation;
            coin2.position += separation;
        }
    }

    /// <summary>
    /// Update spatial grid for efficient collision detection
    /// </summary>
    private void UpdateSpatialGrid()
    {
        spatialGrid.Clear();

        foreach (CoinInstance coin in activeCoinList)
        {
            Vector3Int cell = GetGridCell(coin.position);
            if (!spatialGrid.ContainsKey(cell))
                spatialGrid[cell] = new List<CoinInstance>();
            spatialGrid[cell].Add(coin);
        }
    }

    /// <summary>
    /// Get spatial grid cell for a position
    /// </summary>
    private Vector3Int GetGridCell(Vector3 position)
    {
        return new Vector3Int(
            Mathf.FloorToInt(position.x / SPATIAL_GRID_SIZE),
            Mathf.FloorToInt(position.y / SPATIAL_GRID_SIZE),
            Mathf.FloorToInt(position.z / SPATIAL_GRID_SIZE)
        );
    }

    /// <summary>
    /// Get all active coins
    /// </summary>
    public List<CoinInstance> GetActiveCoinList() => new List<CoinInstance>(activeCoinList);

    /// <summary>
    /// Get coin pool statistics for optimization
    /// </summary>
    public (int active, int pooled) GetPoolStats() => (activeCoinList.Count, coinPool.Count);
}

/// <summary>
/// Represents a single coin instance in the physics simulation
/// </summary>
public class CoinInstance
{
    public Vector3 position;
    public Vector3 velocity = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;
    public Vector3 angularVelocity = Vector3.zero;

    public int coinValue = 1;
    public CoinType coinType = CoinType.Normal;
    public bool isActive = false;
    public float lifetime = 0f;
    public const float MAX_LIFETIME = 300f; // 5 minutes max

    private CoinPhysicsEngine.PhysicsSettings physicsSettings;

    public void Initialize(CoinPhysicsEngine.PhysicsSettings settings)
    {
        physicsSettings = settings;
    }

    public void Reuse()
    {
        velocity = Vector3.zero;
        rotation = Quaternion.identity;
        angularVelocity = Vector3.zero;
        lifetime = 0f;
    }
}

public enum CoinType
{
    Normal,
    Gold,
    Gem,
    Bonus,
    Multiplier,
    Mystery
}
