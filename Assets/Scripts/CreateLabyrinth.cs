using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CreateLabyrinth : MonoBehaviour
{
    [SerializeField] GameObject _room, _load;
    [SerializeField, Tooltip("�����Ԋu")] float _createInterval = 0.05f;
    [Header("�X�^�[�g�̈ʒu�i���ׂĊ�������͂P�܂Łj")]
    [SerializeField, Tooltip("1�ȏ�{(���H�̃T�C�Y)*2+1}�����̐����ŃC���f�b�N�X���w��")] int _startIndexX = 0;
    [SerializeField, Tooltip("1�ȏ�{(���H�̃T�C�Y)*2+1}�����̐����ŃC���f�b�N�X���w��")] int _startIndexY = 0;
    [SerializeField, Tooltip("1�ȏ�{(���H�̃T�C�Y)*2+1}�����̐����ŃC���f�b�N�X���w��")] int _startIndexZ = 0;

    LabyrinthAlgorithm _algorithm;
    AreaSeparate _separate;
    /// <summary>�A���|�C���g�̃y�A��������</summary>
    Dictionary<(int x, int y, int z), Queue<Vector3>> _connectPointPair = new Dictionary<(int x, int y, int z), Queue<Vector3>>();
    /// <summary>�T���A���S���Y���Œ��ׂ����_�i���j�̏��Ԃ�ێ�����L���[</summary>
    Queue<(int x, int y, int z)> _searchLoadOrder = new Queue<(int x, int y, int z)>();

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
        _separate = GetComponent<AreaSeparate>();
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
                        Instantiate(_room, new Vector3(l, m, n), Quaternion.identity);
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

        var wait = new WaitForSeconds(_createInterval);
        while (vertexQueue.Count != 0)
        {
            var searchVertex = vertexQueue.Dequeue();
            //�T���ς݂ɍX�V
            vertex[searchVertex] = true;
            if (searchVertex.x * searchVertex.y * searchVertex.z % 2 != 0)
            {
                //�����̎��ɃI�u�W�F�N�g����
                var pos = _separate.PositionDic[searchVertex];
                var go = Instantiate(_room, new Vector3(pos.x, pos.y, pos.z), Quaternion.identity);
                //��������������ID��`����
                go.GetComponent<Room>().SetID(searchVertex, _algorithm, this);
            }
            else
            {
                //���ׂ������L���[�ɒǉ�
                _searchLoadOrder.Enqueue(searchVertex);
            }
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

        Debug.Log("BFS Complete");

        yield return LoadSetCoroutine();

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

        var wait = new WaitForSeconds(_createInterval);
        while (vertexStack.Count != 0)
        {
            var searchVertex = vertexStack.Pop();
            //�T���ς݂ɍX�V
            vertex[searchVertex] = true; if (searchVertex.x * searchVertex.y * searchVertex.z % 2 != 0)
            {
                //�����̎��ɃI�u�W�F�N�g����
                var pos = _separate.PositionDic[searchVertex];
                var go = Instantiate(_room, new Vector3(pos.x, pos.y, pos.z), Quaternion.identity);
                //��������������ID��`����
                go.GetComponent<Room>().SetID(searchVertex, _algorithm, this);
            }
            else
            {
                //���ׂ������L���[�ɒǉ�
                _searchLoadOrder.Enqueue(searchVertex);
            }
            yield return wait;

            //�אڒ��_�𒲂ׂ�
            foreach (var connect in _algorithm.ConnectDic[searchVertex])
            {
                //�T���ς݂̒��_�͔�΂�
                if (vertex[connect]) continue;

                //�אڒ��_���X�^�b�N�ɒǉ�
                vertexStack.Push(connect);
            }
        }

        Debug.Log("DFS Complete");

        yield return LoadSetCoroutine();

        yield break;
    }

    /// <summary>
    /// ID�ƘA���|�C���g�̃y�A�̎����ɓo�^����֐�
    /// </summary>
    /// <param name="vertex">�A���|�C���g</param>
    /// <param name="pos">�A���|�C���g�̍��W</param>
    public void RegisterDic((int x, int y, int z) vertex, Vector3 pos)
    {
        if (!_connectPointPair.ContainsKey(vertex))
        {
            _connectPointPair[vertex] = new Queue<Vector3>();
        }
        _connectPointPair[vertex].Enqueue(pos);
    }

    IEnumerator LoadSetCoroutine()
    {
        foreach (var load in _searchLoadOrder)
        {
            var connect = _connectPointPair[load];//�A���|�C���g�̃y�A�擾
            var start = connect.Dequeue();//�A���J�n�_
            var end = connect.Dequeue();//�A���I���_
            var mid = (start + end) / 2;//�A���|�C���g�̒��_
            //���ꂼ��̘A���|�C���g�����[���h���W�̐����ǂ���ɂ��邩
            var dirx = end.x - start.x > 0 ? 1 : -1;
            var diry = end.y - start.y > 0 ? 1 : -1;
            var dirz = end.z - start.z > 0 ? 1 : -1;
            var wait = new WaitForSeconds(_createInterval);

            if (load.x % 2 == 0)
            {
                //���_�܂ŐL�΂�
                int x = 0, y = 0, z = 0;
                for (int i = 0; i < (int)Mathf.Abs(mid.x - start.x); i++)
                {
                    x = i * dirx;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                for (int i = 0; i < (int)Mathf.Abs(mid.y - start.y); i++)
                {
                    y = i * diry;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                for (int i = 0; i < (int)Mathf.Abs(mid.z - start.z); i++)
                {
                    z = i * dirz;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                //���_����L�΂�
                for (int i = (int)Mathf.Abs(mid.z - start.z); i <= Mathf.Abs(end.z - start.z); i++)
                {
                    z = i * dirz;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                for (int i = (int)Mathf.Abs(mid.y - start.y); i <= Mathf.Abs(end.y - start.y); i++)
                {
                    y = i * diry;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                for (int i = (int)Mathf.Abs(mid.x - start.x); i <= Mathf.Abs(end.x - start.x); i++)
                {
                    x = i * dirx;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
            }
            else if (load.y % 2 == 0)
            {
                //���_�܂ŐL�΂�
                int x = 0, y = 0, z = 0;
                for (int i = 0; i < (int)Mathf.Abs(mid.y - start.y); i++)
                {
                    y = i * diry;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                for (int i = 0; i < (int)Mathf.Abs(mid.z - start.z); i++)
                {
                    z = i * dirz;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                for (int i = 0; i < (int)Mathf.Abs(mid.x - start.x); i++)
                {
                    x = i * dirx;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                //���_����L�΂�
                for (int i = (int)Mathf.Abs(mid.x - start.x); i <= Mathf.Abs(end.x - start.x); i++)
                {
                    x = i * dirx;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                for (int i = (int)Mathf.Abs(mid.z - start.z); i <= Mathf.Abs(end.z - start.z); i++)
                {
                    z = i * dirz;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                for (int i = (int)Mathf.Abs(mid.y - start.y); i <= Mathf.Abs(end.y - start.y); i++)
                {
                    y = i * diry;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
            }
            else
            {
                //���_�܂ŐL�΂�
                int x = 0, y = 0, z = 0;
                for (int i = 0; i < (int)Mathf.Abs(mid.z - start.z); i++)
                {
                    z = i * dirz;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                for (int i = 0; i < (int)Mathf.Abs(mid.x - start.x); i++)
                {
                    x = i * dirx;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                for (int i = 0; i < (int)Mathf.Abs(mid.y - start.y); i++)
                {
                    y = i * diry;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                //���_����L�΂�
                for (int i = (int)Mathf.Abs(mid.y - start.y); i <= Mathf.Abs(end.y - start.y); i++)
                {
                    y = i * diry;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                for (int i = (int)Mathf.Abs(mid.x - start.x); i <= Mathf.Abs(end.x - start.x); i++)
                {
                    x = i * dirx;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
                for (int i = (int)Mathf.Abs(mid.z - start.z); i <= Mathf.Abs(end.z - start.z); i++)
                {
                    z = i * dirz;
                    Instantiate(_load, start + new Vector3(x, y, z), Quaternion.identity);
                    yield return wait;
                }
            }
        }
        Debug.Log("LoadSetting Complete");
        yield break;
    }
}
