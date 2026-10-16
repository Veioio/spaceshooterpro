using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private UIManager _uiManager;

    private int _score;
    private bool _isGameOver;

    public bool IsGameOver
    {
        get { return _isGameOver; }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _uiManager.UpdateScore(_score);
    }

    void Update()
    {
        if (_isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void AddScore(int points)
    {
        if (_isGameOver)
        {
            return;
        }

        _score += points;
        _uiManager.UpdateScore(_score);
    }

    public void GameOver()
    {
        _isGameOver = true;
        _uiManager.ShowGameOver();
    }
}
