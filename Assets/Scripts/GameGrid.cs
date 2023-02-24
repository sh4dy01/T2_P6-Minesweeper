using Managers;
using ScriptableObjects.script;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameGrid : MonoBehaviour
{
    [SerializeField] private GameObject baseBlock;
    [SerializeField] private GameObject bombContainer;
    [SerializeField] private GameObject blockContainer;
    [SerializeField] private AudioClip explodeSfx;
    [SerializeField] private AudioClip breakSfx;

    private AudioSource _blockAudioSource;
    
    private GameDifficultySo _gameMod;
    private int _flagCounter;
	private int _numBlocks;

	// Shaking.
	private Vector3 _originalPosition;
    private float _shakeIntensity;

    public class BlockInfo
    {
        public Vector3 WorldPosition { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public bool IsBomb { get; private set; }
        public int BombCounter { get; private set; }
        
        // Block position in game grid.
        public int X { get => GridPosition.x; }
		public int Y { get => GridPosition.y; }

		public void IncrementBombCounter() => BombCounter++;
        public void SetBomb() => IsBomb = true;
        
        public void Init(Vector2Int position, Vector3 worldPosition)
        {
            IsBomb = false;
            BombCounter = 0;
            GridPosition = position;
            WorldPosition = worldPosition;
        }
    }
    
    private BlockInfo[,] _grid;
    private Block[,] _blocks;
    
    private readonly Vector2Int[] _neighbourPositions = 
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.up + Vector2Int.right,
        Vector2Int.up + Vector2Int.left,
        Vector2Int.down + Vector2Int.right,
        Vector2Int.down + Vector2Int.left
    };

    private void Awake()
    {
        GameManager.Instance.InitializeGame();
        
        if (GameManager.Instance.IsSeedSet)
            Random.InitState(GameManager.Instance.Seed);

        _gameMod = GameManager.Instance.GameDifficulty;
        _grid = new BlockInfo[_gameMod.Width, _gameMod.Height];
        _blocks = new Block[_gameMod.Width, _gameMod.Height];

        _blockAudioSource = GetComponent<AudioSource>();
        _blockAudioSource.clip = breakSfx;

		_shakeIntensity = 0.0F;
		_numBlocks = _gameMod.Width * _gameMod.Height;

		if (Camera.main == null) return;
        
        var main = Camera.main;
        //main.transform.position = new Vector3(_gameMod.Width * 0.5f, _gameMod.Height * 0.5f, -10);
        //main.orthographicSize = (_gameMod.Height / 2) + 2;

        _originalPosition = transform.position;
	}

    private void Start()
    {
		CreateGrid();
        SetBomb();
        SetBlock();
    }

	private void Update()
	{
        _shakeIntensity -= Time.deltaTime;
        if (_shakeIntensity < 0.0F)
        {
            _shakeIntensity = 0.0F;
		}

        Vector3 randomOffset = new Vector3(Random.Range(-1.0F, 1.0F), Random.Range(-1.0F, 1.0F), 0.0F) * _shakeIntensity;
        transform.SetPositionAndRotation(randomOffset, Quaternion.identity);
	}

	private void CreateGrid()
    {
	    float halfWidth = _gameMod.Width * 0.5f;
	    float halfHeight = _gameMod.Height * 0.5f;

	    for (int x = 0; x < _gameMod.Width; x++)
        {
            for (int y = 0; y < _gameMod.Height; y++)
            {
                BlockInfo info = new();
                info.Init(new Vector2Int(x, y), new Vector3(x - halfWidth, y - halfHeight, 0));
                _grid[x, y] = info;
            }
        }
    }

    private void SetBomb()
    {
        int bombPlaced = 0;
        
        while (bombPlaced < _gameMod.BombQuantity)
        {
            int x = Random.Range(0, _gameMod.Width);
            int y = Random.Range(0, _gameMod.Height);
            
            if (_grid[x, y].IsBomb) continue;

            BlockInfo info = _grid[x, y];
            
            info.SetBomb();
            Vector2Int bombPos = info.GridPosition;
            foreach (var position in _neighbourPositions)
            {
                Vector2Int neighbor = bombPos + position;
                if (neighbor.x >= _gameMod.Width || neighbor.y >= _gameMod.Height || neighbor.x < 0 || neighbor.y < 0)
                    continue;
                
                    
                _grid[neighbor.x, neighbor.y].IncrementBombCounter();
            }
                
            bombPlaced++;
        }
    }

    private void SetBlock()
    {
        foreach (BlockInfo info in _grid)
        {
            Transform parent = info.IsBomb ? bombContainer.transform : blockContainer.transform;
            GameObject blockObj = Instantiate(baseBlock, info.WorldPosition, Quaternion.identity, parent);
            Block infoComponent = blockObj.GetComponent<Block>();

            _blocks[info.X, info.Y] = infoComponent;
            infoComponent.BlockInfo = info;
            blockObj.name = info.IsBomb ? "Bomb" : "Empty";
            //blockObj.GetComponent<AudioSource>().clip = info.IsBomb ? explodeSfx : breakSfx;
            infoComponent.SetBomb(info.IsBomb);
            infoComponent.SetBombAroundCounter(info.BombCounter);
        }
    }

	public void RevealBlock(BlockInfo info)
    {
        RevealBlock(info.X, info.Y);

		_blockAudioSource.Play();
	}

    private void RevealBlock(int x, int y)
    {
        Block b = _blocks[x, y];
        BlockInfo info = _grid[x, y];

        // Already revealed.
        if (b.Revealed) return;

        b.RevealThisBlock();

		_numBlocks--;
        if (_numBlocks == _gameMod.BombQuantity)
        {
			GameManager.Instance.FinishTheGame(true);
        }

		// Add to shake intensity.
		// With recursion, the effect will add up, shaking more vigorously the more tiles are revealed at one time.
		_shakeIntensity += 0.02F;
        if (_shakeIntensity > 1.5F) _shakeIntensity = 1.5F;

        if (info.IsBomb)
        {
	        GameManager.Instance.FinishTheGame(false);
            _blockAudioSource.clip = explodeSfx;
	        b.Explosion();
		}
        else
        {
            // Propagate.
            if (info.BombCounter != 0) return;

			foreach (Vector2Int position in _neighbourPositions)
			{
				Vector2Int neighbor = info.GridPosition + position;
				if (neighbor.x >= _gameMod.Width || neighbor.y >= _gameMod.Height || neighbor.x < 0 ||
				    neighbor.y < 0) return;
				int nx = info.GridPosition.x + position.x;
				int ny = info.GridPosition.y + position.y;
				if (nx >= _gameMod.Width || ny >= _gameMod.Height || nx < 0 || ny < 0)
					continue;

				RevealBlock(nx, ny);
			}
		}
    }

    // Called when the player presses a number. It will try to reveal the squares around it, if there
    // are enough flags.
    public void RevealAround(BlockInfo info)
    {
        RevealAround(info.X, info.Y, info.BombCounter);

		_blockAudioSource.Play();
    }

	private void RevealAround(int x, int y, int requiredFlags)
    {
        // Get number of surrounding flags.
        int numFlags = 0;
		foreach (Vector3Int position in _neighbourPositions)
		{
			Vector3Int neighbor = new Vector3Int(x, y) + position;
			if (neighbor.x >= _gameMod.Width || neighbor.y >= _gameMod.Height || neighbor.x < 0 || neighbor.y < 0)
				continue;

			if (_blocks[neighbor.x, neighbor.y].Flagged)
            {
                numFlags++;
            }
		}

        // Not enough.
        if (numFlags < requiredFlags) return;

		foreach (Vector3Int position in _neighbourPositions)
		{
			Vector3Int neighbor = new Vector3Int(x, y) + position;
			if (neighbor.x >= _gameMod.Width || neighbor.y >= _gameMod.Height || neighbor.x < 0 || neighbor.y < 0)
				continue;

			if (!_blocks[neighbor.x, neighbor.y].Flagged)
			{
                RevealBlock(neighbor.x, neighbor.y);
			}
		}
	}
}
