using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float _speed = 3.5f;
    [SerializeField]
    private float _speedBoostMultiplier = 2f;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private float _fireRate = 0.5f;
    private float _canFire = -1f;
    [SerializeField]
    private int _lives = 3;
    [SerializeField]
    private float _powerUpDuration = 5f;
    [SerializeField]
    private GameObject _shieldVisual;

    private float _tripleShotUntil = -1f;
    private float _speedBoostUntil = -1f;
    private bool _shieldActive;

    private SpawnManager _spawnManager;
    private UIManager _uiManager;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, -3.48f, 0);

        GameObject spawnManagerObject = GameObject.Find("Spawn_Manager");
        if (spawnManagerObject != null)
        {
            _spawnManager = spawnManagerObject.GetComponent<SpawnManager>();
        }

        if (_spawnManager == null)
        {
            Debug.LogError("The Spawn Manager is NULL");
        }

        _uiManager = FindObjectOfType<UIManager>();

        if (_uiManager == null)
        {
            Debug.LogError("The UI Manager is NULL");
        }
        else
        {
            _uiManager.UpdateLives(_lives);
        }

        if (_shieldVisual != null)
        {
            _shieldVisual.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CalculateMovement();
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire)
        {
            ShootLaser();
        }
    }


    void ShootLaser()
    {
        _canFire = Time.time + _fireRate;
        Vector3 offset = new Vector3(0, 1.05f, 0);

        if (Time.time < _tripleShotUntil)
        {
            Instantiate(_laserPrefab, transform.position + offset + new Vector3(-0.8f, -0.3f, 0), Quaternion.identity);
            Instantiate(_laserPrefab, transform.position + offset, Quaternion.identity);
            Instantiate(_laserPrefab, transform.position + offset + new Vector3(0.8f, -0.3f, 0), Quaternion.identity);
        }
        else
        {
            Instantiate(_laserPrefab, transform.position + offset, Quaternion.identity);
        }

        if (GameAssets.Instance != null)
        {
            GameAssets.Instance.PlayLaser();
        }
    }


    void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        float speed = Time.time < _speedBoostUntil ? _speed * _speedBoostMultiplier : _speed;

        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);
        transform.Translate(direction * speed * Time.deltaTime);

        transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, -3.8f, 0), 0);

        if (transform.position.x <= -11)
        {
            transform.position = new Vector3(11, transform.position.y, 0);
        }
        else if (transform.position.x >= 11)
        {
            transform.position = new Vector3(-11, transform.position.y, 0);
        }
    }

    public void Damage()
    {
        if (_shieldActive)
        {
            _shieldActive = false;
            if (_shieldVisual != null)
            {
                _shieldVisual.SetActive(false);
            }
            return;
        }

        _lives = _lives - 1;

        if (_uiManager != null)
        {
            _uiManager.UpdateLives(_lives);
        }

        if (_lives < 1)
        {
            if (_spawnManager != null)
            {
                _spawnManager.OnPlayerDeath();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }

            if (GameAssets.Instance != null)
            {
                GameAssets.Instance.SpawnExplosion(transform.position, 0.5f);
            }

            Destroy(this.gameObject);
        }
    }

    public void ActivateTripleShot()
    {
        _tripleShotUntil = Time.time + _powerUpDuration;
    }

    public void ActivateSpeedBoost()
    {
        _speedBoostUntil = Time.time + _powerUpDuration;
    }

    public void ActivateShield()
    {
        _shieldActive = true;
        if (_shieldVisual != null)
        {
            _shieldVisual.SetActive(true);
        }
    }
}
