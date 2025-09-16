using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CreateLabyrinth : MonoBehaviour
{
    [SerializeField] GameObject _prefab;
    [Header("�X�^�[�g�̈ʒu")]
    [SerializeField, Tooltip("0�ȏ���H�̃T�C�Y�����̐����ŃC���f�b�N�X���w��")] int _startIndexX = 0;
    [SerializeField, Tooltip("0�ȏ���H�̃T�C�Y�����̐����ŃC���f�b�N�X���w��")] int _startIndexY = 0;
    [SerializeField, Tooltip("0�ȏ���H�̃T�C�Y�����̐����ŃC���f�b�N�X���w��")] int _startIndexZ = 0;

    LabyrinthAlgorithm _algorithm;

    int _labyrinthSizeX;
    int _labyrinthSizeY;
    int _labyrinthSizeZ;

    const float CREATEINTERVAL = 0.1f;
    const int PASS = 1;
    const int WALL = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SetUp();
    }

    private void OnEnable()
    {
        LabyrinthAlgorithm.CreateLabyrinth += BFS;
    }

    private void OnDisable()
    {
        LabyrinthAlgorithm.CreateLabyrinth -= BFS;
    }

    void SetUp()
    {
        _algorithm = GetComponent<LabyrinthAlgorithm>();
        _labyrinthSizeX = _startIndexX * 2 + 1;
        _labyrinthSizeY = _startIndexY * 2 + 1;
        _labyrinthSizeZ = _startIndexZ * 2 + 1;
    }

    /// <summary>
    /// ���H�̎��̂𐶐�����֐�
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

    void BFS()
    {
        if (_labyrinthSizeX < 0 || _algorithm.LabyrinthSizeX <= _labyrinthSizeX)
        {
            Debug.LogWarning("�X�^�[�g�̍��W����H�̃T�C�Y���ɐݒ肵�Ă�������");
            return;
        }

        if (_labyrinthSizeY < 0 || _algorithm.LabyrinthSizeY <= _labyrinthSizeY)
        {
            Debug.LogWarning("�X�^�[�g�̍��W����H�̃T�C�Y���ɐݒ肵�Ă�������");
            return;
        }

        if (_labyrinthSizeZ < 0 || _algorithm.LabyrinthSizeZ <= _labyrinthSizeZ)
        {
            Debug.LogWarning("�X�^�[�g�̍��W����H�̃T�C�Y���ɐݒ肵�Ă�������");
            return;
        }

        StartCoroutine(BFSCoroutine());
    }

    /// <summary>
    /// �^�C�~���O�����炵�ăI�u�W�F�N�g�𐶐�����֐�
    /// </summary>
    /// <returns></returns>
    IEnumerator BFSCoroutine()
    {
        //�O���t�̒��_�i�����ƒʂ蓹�j�𐶐�
        Dictionary<(int x, int y, int z), bool> vertex = new Dictionary<(int x, int y, int z), bool>();
        for (int n = 0; n < _algorithm.LabyrinthSizeZ; n++)
        {
            for (int m = 0; m < _algorithm.LabyrinthSizeY; m++)
            {
                for (int l = 0; l < _algorithm.LabyrinthSizeX; l++)
                {
                    if (_algorithm.RoomID[l, m, n] != WALL)
                    {
                        vertex.Add((l, m, n), false);
                    }
                }
            }
        }

        //�T�����_�̃L���[
        Queue<(int x, int y, int z)> vertexQueue = new Queue<(int x, int y, int z)>();

        //�X�^�[�g���L���[�ɒǉ�
        vertexQueue.Enqueue((_labyrinthSizeX, _labyrinthSizeY, _labyrinthSizeZ));
        //�T���ς݂ɍX�V
        vertex[(_labyrinthSizeX, _labyrinthSizeY, _labyrinthSizeZ)] = true;
        //�I�u�W�F�N�g����
        Instantiate(_prefab, new Vector3(_labyrinthSizeX, _labyrinthSizeY, _labyrinthSizeZ), Quaternion.identity);

        var wait = new WaitForSeconds(CREATEINTERVAL);
        while (vertexQueue.Count != 0)
        {
            (int, int, int) searchVertex = vertexQueue.Dequeue();

            //�אڒ��_�𒲂ׂ�
            foreach (var connect in _algorithm.ConnectDic[searchVertex])
            {
                //�T���ς݂̒��_�͔�΂�
                if (vertex[connect]) continue;

                //�V���ɒT���������_���L���[�ɒǉ�
                vertexQueue.Enqueue(connect);
                //�T���ς݂ɍX�V
                vertex[connect] = true;
                //�I�u�W�F�N�g����
                Instantiate(_prefab, new Vector3(connect.x, connect.y, connect.z), Quaternion.identity);
                yield return wait;
            }
        }

        Debug.Log("BFS End");
        yield break;
    }
}
