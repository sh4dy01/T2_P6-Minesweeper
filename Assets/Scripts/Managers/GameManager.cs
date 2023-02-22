using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameGrid _gameGrid;
    private UIManager _uiManager;
    private int _maxBombCounter;

    public int BombCounter { get; private set; }

    public GameGrid GameGrid { get => _gameGrid; }

    #region Singleton

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;

			Instance._gameGrid = FindObjectOfType<GameGrid>();
			Instance._uiManager = FindObjectOfType<UIManager>();

            DontDestroyOnLoad(Instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion

    public void InitBombCounter(int bombs)
    {
        _maxBombCounter = bombs;
        BombCounter = bombs;
        
        _uiManager = FindObjectOfType<UIManager>();
        _uiManager.UpdateBombText(BombCounter);
    }

    public void DecreaseBombCounter()
    {
        if (BombCounter <= 0) return;
        BombCounter--;
        _uiManager.UpdateBombText(BombCounter);
    }

    public void IncreaseBombCounter()
    {
        if (BombCounter >= _maxBombCounter) return;
        BombCounter++;
        _uiManager.UpdateBombText(BombCounter);
    }
}