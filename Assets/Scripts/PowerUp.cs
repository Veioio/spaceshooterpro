using UnityEngine;

public enum PowerUpType
{
    TripleShot,
    Speed,
    Shield
}

public class PowerUp : MonoBehaviour
{
    [SerializeField]
    private float _speed = 3f;
    [SerializeField]
    private PowerUpType _type;

    void Update()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y < -6f)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.transform.GetComponent<Player>();

        if (player == null)
        {
            return;
        }

        switch (_type)
        {
            case PowerUpType.TripleShot:
                player.ActivateTripleShot();
                break;
            case PowerUpType.Speed:
                player.ActivateSpeedBoost();
                break;
            case PowerUpType.Shield:
                player.ActivateShield();
                break;
        }

        if (GameAssets.Instance != null)
        {
            GameAssets.Instance.PlayPowerUp();
        }

        Destroy(this.gameObject);
    }
}
