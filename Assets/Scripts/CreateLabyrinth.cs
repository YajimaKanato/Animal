using UnityEngine;

public class CreateLabyrinth : MonoBehaviour
{
    [SerializeField] GameObject _prefab;

    LabyrinthAlgorithm _algorithm;

    const int PASS = 1;
    const int WALL = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SetUp();
    }

    private void OnEnable()
    {
        LabyrinthAlgorithm.CreateLabyrinth += LabyrinthCreate;
    }

    private void OnDisable()
    {
        LabyrinthAlgorithm.CreateLabyrinth -= LabyrinthCreate;
    }

    void SetUp()
    {
        _algorithm = GetComponent<LabyrinthAlgorithm>();
    }

    /// <summary>
    /// ñ¿òHÇÃé¿ëÃÇê∂ê¨Ç∑ÇÈä÷êî
    /// </summary>
    void LabyrinthCreate()
    {
        for (int n = 0; n < _algorithm.LabyrinthSizeZ; n++)
        {
            for (int m = 0; m < _algorithm.LabyrinthSizeY; m++)
            {
                for (int l = 0; l < _algorithm.LabyrinthSizeX; l++)
                {
                    if (_algorithm.RoomID[l, m, n] != WALL)
                    {
                        Instantiate(_prefab, new Vector3(l, m, n), Quaternion.identity);
                    }
                }
            }
        }
    }
}
