using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float _rotSpeed = 10;

    float _rotX;
    // Update is called once per frame
    void Update()
    {
        //c•ûŒü‚Ì‰ñ“]
        _rotX = Input.GetAxis("Mouse Y") * _rotSpeed * (-1);
        transform.rotation = transform.rotation * Quaternion.Euler(_rotX, 0, 0);
    }
}
