using UnityEngine;

// Holds the shared audio clips and prefabs so other scripts don't need their own references.
public class GameAssets : MonoBehaviour
{
    public static GameAssets Instance { get; private set; }

    [SerializeField]
    private AudioClip _laserClip;
    [SerializeField]
    private AudioClip _explosionClip;
    [SerializeField]
    private AudioClip _powerUpClip;
    [SerializeField]
    private GameObject _explosionPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void PlayLaser()
    {
        PlaySfx(_laserClip, 0.6f);
    }

    public void PlayPowerUp()
    {
        PlaySfx(_powerUpClip, 1f);
    }

    public void SpawnExplosion(Vector3 position, float scale)
    {
        PlaySfx(_explosionClip, 1f);

        if (_explosionPrefab != null)
        {
            GameObject explosion = Instantiate(_explosionPrefab, position, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * scale;
        }
    }

    private void PlaySfx(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            return;
        }

        Vector3 listenerPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        AudioSource.PlayClipAtPoint(clip, listenerPosition, volume);
    }
}
