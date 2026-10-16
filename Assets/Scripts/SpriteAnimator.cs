using UnityEngine;

// Plays a sequence of sprites on the SpriteRenderer of the same object.
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    [SerializeField]
    private Sprite[] _frames;
    [SerializeField]
    private float _framesPerSecond = 24f;
    [SerializeField]
    private bool _loop = true;
    [SerializeField]
    private bool _destroyOnEnd = false;

    private SpriteRenderer _renderer;
    private float _elapsed;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (_frames == null || _frames.Length == 0)
        {
            return;
        }

        _elapsed += Time.deltaTime;
        int index = (int)(_elapsed * _framesPerSecond);

        if (index >= _frames.Length)
        {
            if (_loop)
            {
                index = index % _frames.Length;
            }
            else
            {
                if (_destroyOnEnd)
                {
                    Destroy(gameObject);
                }
                return;
            }
        }

        _renderer.sprite = _frames[index];
    }
}
