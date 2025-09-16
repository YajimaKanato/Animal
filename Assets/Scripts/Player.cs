using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField] float _speed = 5;
    [SerializeField] float _jump = 10;
    [SerializeField] float _rotSpeed = 10;
    [SerializeField] float _gravityScale = 5;
    [SerializeField] LayerMask _ground;

    Rigidbody _rb;

    Vector3 _direction = Vector3.zero;
    Vector3 _directionRight = Vector3.zero;
    Vector3 _directionForward = Vector3.zero;

    float _playerRotY;
    float _moveX, _moveZ;
    float _rotY;
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

        //ˆÚ“®
        _moveX = Input.GetAxisRaw("Horizontal");
        _moveZ = Input.GetAxisRaw("Vertical");
        _playerRotY = transform.rotation.eulerAngles.y * Mathf.Deg2Rad * (-1);
        _directionRight = new Vector3(Mathf.Cos(_playerRotY), 0, Mathf.Sin(_playerRotY)) * _moveX;
        _directionForward = new Vector3(Mathf.Cos(_playerRotY + (Mathf.PI / 2)), 0, Mathf.Sin(_playerRotY + (Mathf.PI / 2))) * _moveZ;
        _direction = (_directionRight + _directionForward).normalized * _speed;
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

        //‰¡•ûŒü‚Ì‰ñ“]
        _rotY = Input.GetAxis("Mouse X") * _rotSpeed;
        transform.rotation = transform.rotation * Quaternion.Euler(0, _rotY, 0);
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _direction;
    }
}
