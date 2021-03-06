using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private float _speed = 10.0f;

    // Update is called once per frame
    void Update()
    {
        CalculateMovement();

    }

    void CalculateMovement()
    {
        Vector3 direction = new Vector3(0, 1, 0);
        transform.Translate(direction * _speed * Time.deltaTime);

        if (transform.position.y > 8f)
        {
            Destroy(gameObject);
        }
    }
}
