using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CreateLabyrinth : MonoBehaviour
{
    [SerializeField] GameObject _prefab;
    [Header("�X�^�[�g�̈ʒu�i���ׂĊ�������͂P�܂Łj")]
    [SerializeField, Tooltip("1�ȏ�{(���H�̃T�C�Y)*2+1}�����̐����ŃC���f�b�N�X���w��")] int _startIndexX = 0;
    [SerializeField, Tooltip("1�ȏ�{(���H�̃T�C�Y)*2+1}�����̐����ŃC���f�b�N�X���w��")] int _startIndexY = 0;
    [SerializeField, Tooltip("1�ȏ�{(���H�̃T�C�Y)*2+1}�����̐����ŃC���f�b�N�X���w��")] int _startIndexZ = 0;

    LabyrinthAlgorithm _algorithm;

    const float CREATEINTERVAL = 0.05f;
    const int PASS = 1;
    const int WALL = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SetUp();
    }

    void SetUp()
    {
        _algorithm = GetComponent<LabyrinthAlgorithm>();
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

    /// <summary>
    /// BFS��p���Ė��H�𐶐�����֐�
    /// </summary>
    public void BFS()
    {
        if (_startIndexX < 1 || (_algorithm.LabyrinthSizeX - 1) - 1 < _startIndexX)
        {
            Debug.LogWarning("X:�X�^�[�g�̍��W����H�̃T�C�Y���ɐݒ肵�Ă�������");
            return;
        }

        if (_startIndexY < 1 || (_algorithm.LabyrinthSizeY - 1) - 1 < _startIndexY)
        {
            Debug.LogWarning("Y:�X�^�[�g�̍��W����H�̃T�C�Y���ɐݒ肵�Ă�������");
            return;
        }

        if (_startIndexZ < 1 || (_algorithm.LabyrinthSizeZ - 1) - 1 < _startIndexZ)
        {
            Debug.LogWarning("Z:�X�^�[�g�̍��W����H�̃T�C�Y���ɐݒ肵�Ă�������");
            return;
        }

        if (!_algorithm.ConnectDic.ContainsKey((_startIndexX, _startIndexY, _startIndexZ)))
        {
            Debug.LogWarning("�X�^�[�g�n�_�ɒʂ��ꏊ������܂���");
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
        vertexQueue.Enqueue((_startIndexX, _startIndexY, _startIndexZ));
        //�T���ς݂ɍX�V
        vertex[(_startIndexX, _startIndexY, _startIndexZ)] = true;
        //�I�u�W�F�N�g����
        Instantiate(_prefab, new Vector3(_startIndexX, _startIndexY, _startIndexZ), Quaternion.identity);

        var wait = new WaitForSeconds(CREATEINTERVAL);
        while (vertexQueue.Count != 0)
        {
            var searchVertex = vertexQueue.Dequeue();
            //�T���ς݂ɍX�V
            vertex[searchVertex] = true;
            //�I�u�W�F�N�g����
            Instantiate(_prefab, new Vector3(searchVertex.x, searchVertex.y, searchVertex.z), Quaternion.identity);
            yield return wait;


            //�אڒ��_�𒲂ׂ�
            foreach (var connect in _algorithm.ConnectDic[searchVertex])
            {
                //�T���ς݂̒��_�͔�΂�
                if (vertex[connect]) continue;

                //�V���ɒT���������_���L���[�ɒǉ�
                vertexQueue.Enqueue(connect);
            }
        }

        Debug.Log("BFS End");
        yield break;
    }

    /// <summary>
    /// DFS��p���Ė��H�𐶐�����֐�
    /// </summary>
    public void DFS()
    {
        if (_startIndexX < 1 || (_algorithm.LabyrinthSizeX - 1) - 1 < _startIndexX)
        {
            Debug.LogWarning("X:�X�^�[�g�̍��W����H�̃T�C�Y���ɐݒ肵�Ă�������");
            return;
        }

        if (_startIndexY < 1 || (_algorithm.LabyrinthSizeY - 1) - 1 < _startIndexY)
        {
            Debug.LogWarning("Y:�X�^�[�g�̍��W����H�̃T�C�Y���ɐݒ肵�Ă�������");
            return;
        }

        if (_startIndexZ < 1 || (_algorithm.LabyrinthSizeZ - 1) - 1 < _startIndexZ)
        {
            Debug.LogWarning("Z:�X�^�[�g�̍��W����H�̃T�C�Y���ɐݒ肵�Ă�������");
            return;
        }

        if (!_algorithm.ConnectDic.ContainsKey((_startIndexX, _startIndexY, _startIndexZ)))
        {
            Debug.LogWarning("�X�^�[�g�n�_�ɒʂ��ꏊ������܂���");
            return;
        }

        StartCoroutine(DFSCoroutine());
    }

    IEnumerator DFSCoroutine()
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

        //�T�����_�̃X�^�b�N
        Stack<(int x, int y, int z)> vertexStack = new Stack<(int x, int y, int z)>();
        //�X�^�[�g���X�^�b�N�ɒǉ�
        vertexStack.Push((_startIndexX, _startIndexY, _startIndexZ));

        var wait = new WaitForSeconds(CREATEINTERVAL);
        while (vertexStack.Count != 0)
        {
            var searchVertex = vertexStack.Pop();
            //�T���ς݂ɍX�V
            vertex[searchVertex] = true;
            //�I�u�W�F�N�g����
            Instantiate(_prefab, new Vector3(searchVertex.x, searchVertex.y, searchVertex.z), Quaternion.identity);
            yield return wait;

            //�אڒ��_�𒲂ׂ�
            foreach (var v in _algorithm.ConnectDic[searchVertex])
            {
                //�T���ς݂̒��_�͔�΂�
                if (vertex[v]) continue;

                //�אڒ��_���X�^�b�N�ɒǉ�
                vertexStack.Push(v);
            }
        }

        Debug.Log("DFS End");
        yield break;
    }
}
