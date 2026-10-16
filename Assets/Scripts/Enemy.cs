using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _speed = 4f;
    [SerializeField]
    private int _points = 10;

    private bool _isDestroyed;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(Random.Range(-8f, 8f), 8f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y <= -6f)
        {
            float randomX = Random.Range(-8f, 8f);
            transform.position = new Vector3(randomX, 8f, 0);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDestroyed)
        {
            return;
        }

        if (other.tag == "Laser")
        {
            Destroy(other.gameObject);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(_points);
            }

            Explode();
        }

        if (other.tag == "Player")
        {
            Player player = other.transform.GetComponent<Player>();

            if (player != null)
            {
                player.Damage();
            }

            Explode();
        }

    }

    private void Explode()
    {
        _isDestroyed = true;

        if (GameAssets.Instance != null)
        {
            GameAssets.Instance.SpawnExplosion(transform.position, 0.25f);
        }

        Destroy(this.gameObject);
    }
}
