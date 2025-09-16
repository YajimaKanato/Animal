using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CreateLabyrinth : MonoBehaviour
{
    [SerializeField] GameObject _prefab;
    [Header("スタートの位置")]
    [SerializeField, Tooltip("0以上迷路のサイズ未満の数字でインデックスを指定")] int _startIndexX = 0;
    [SerializeField, Tooltip("0以上迷路のサイズ未満の数字でインデックスを指定")] int _startIndexY = 0;
    [SerializeField, Tooltip("0以上迷路のサイズ未満の数字でインデックスを指定")] int _startIndexZ = 0;

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
    /// 迷路の実体を生成する関数
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
            Debug.LogWarning("スタートの座標を迷路のサイズ内に設定してください");
            return;
        }

        if (_labyrinthSizeY < 0 || _algorithm.LabyrinthSizeY <= _labyrinthSizeY)
        {
            Debug.LogWarning("スタートの座標を迷路のサイズ内に設定してください");
            return;
        }

        if (_labyrinthSizeZ < 0 || _algorithm.LabyrinthSizeZ <= _labyrinthSizeZ)
        {
            Debug.LogWarning("スタートの座標を迷路のサイズ内に設定してください");
            return;
        }

        StartCoroutine(BFSCoroutine());
    }

    /// <summary>
    /// タイミングをずらしてオブジェクトを生成する関数
    /// </summary>
    /// <returns></returns>
    IEnumerator BFSCoroutine()
    {
        //グラフの頂点（部屋と通り道）を生成
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

        //探索頂点のキュー
        Queue<(int x, int y, int z)> vertexQueue = new Queue<(int x, int y, int z)>();

        //スタートをキューに追加
        vertexQueue.Enqueue((_labyrinthSizeX, _labyrinthSizeY, _labyrinthSizeZ));
        //探索済みに更新
        vertex[(_labyrinthSizeX, _labyrinthSizeY, _labyrinthSizeZ)] = true;
        //オブジェクト生成
        Instantiate(_prefab, new Vector3(_labyrinthSizeX, _labyrinthSizeY, _labyrinthSizeZ), Quaternion.identity);

        var wait = new WaitForSeconds(CREATEINTERVAL);
        while (vertexQueue.Count != 0)
        {
            (int, int, int) searchVertex = vertexQueue.Dequeue();

            //隣接頂点を調べる
            foreach (var connect in _algorithm.ConnectDic[searchVertex])
            {
                //探索済みの頂点は飛ばす
                if (vertex[connect]) continue;

                //新たに探索した頂点をキューに追加
                vertexQueue.Enqueue(connect);
                //探索済みに更新
                vertex[connect] = true;
                //オブジェクト生成
                Instantiate(_prefab, new Vector3(connect.x, connect.y, connect.z), Quaternion.identity);
                yield return wait;
            }
        }

        Debug.Log("BFS End");
        yield break;
    }
}
