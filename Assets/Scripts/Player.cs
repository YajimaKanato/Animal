using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField] float _speed = 5;
    [SerializeField] float _jump = 10;
    [SerializeField] float _gravityScale = 5;
    [SerializeField] LayerMask _ground;

    Rigidbody _rb;

    Vector3 _direction = Vector3.zero;

    float _moveX, _moveZ;
    const float GRAVITY = 9.8f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        _moveX = Input.GetAxisRaw("Horizontal");
        _moveZ = Input.GetAxisRaw("Vertical");
        _direction.x = transform.right.x * _moveX;
        _direction.z = transform.forward.z * _moveZ;
        _direction = _direction.normalized * _speed;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Physics.Linecast(transform.position, transform.position + Vector3.down * transform.localScale.y * 1.1f, out var groundHit, _ground))
            {
                _rb.AddForce(Vector3.up * _jump, ForceMode.Impulse);
            }
        }
        else
        {
            _direction.y = _rb.linearVelocity.y;
            if (_direction.y != 0)
            {
                _direction.y -= GRAVITY * _gravityScale * Time.deltaTime;
            }
        }
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _direction;
    }
}
