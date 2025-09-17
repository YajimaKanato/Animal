using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CreateLabyrinth : MonoBehaviour
{
    [SerializeField] GameObject _prefab;
    [Header("スタートの位置（すべて奇数か偶数は１つまで）")]
    [SerializeField, Tooltip("1以上{(迷路のサイズ)*2+1}未満の数字でインデックスを指定")] int _startIndexX = 0;
    [SerializeField, Tooltip("1以上{(迷路のサイズ)*2+1}未満の数字でインデックスを指定")] int _startIndexY = 0;
    [SerializeField, Tooltip("1以上{(迷路のサイズ)*2+1}未満の数字でインデックスを指定")] int _startIndexZ = 0;

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

    /// <summary>
    /// BFSを用いて迷路を生成する関数
    /// </summary>
    public void BFS()
    {
        if (_startIndexX < 1 || (_algorithm.LabyrinthSizeX - 1) - 1 < _startIndexX)
        {
            Debug.LogWarning("X:スタートの座標を迷路のサイズ内に設定してください");
            return;
        }

        if (_startIndexY < 1 || (_algorithm.LabyrinthSizeY - 1) - 1 < _startIndexY)
        {
            Debug.LogWarning("Y:スタートの座標を迷路のサイズ内に設定してください");
            return;
        }

        if (_startIndexZ < 1 || (_algorithm.LabyrinthSizeZ - 1) - 1 < _startIndexZ)
        {
            Debug.LogWarning("Z:スタートの座標を迷路のサイズ内に設定してください");
            return;
        }

        if (!_algorithm.ConnectDic.ContainsKey((_startIndexX, _startIndexY, _startIndexZ)))
        {
            Debug.LogWarning("スタート地点に通れる場所がありません");
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
        vertexQueue.Enqueue((_startIndexX, _startIndexY, _startIndexZ));
        //探索済みに更新
        vertex[(_startIndexX, _startIndexY, _startIndexZ)] = true;
        //オブジェクト生成
        Instantiate(_prefab, new Vector3(_startIndexX, _startIndexY, _startIndexZ), Quaternion.identity);

        var wait = new WaitForSeconds(CREATEINTERVAL);
        while (vertexQueue.Count != 0)
        {
            var searchVertex = vertexQueue.Dequeue();
            //探索済みに更新
            vertex[searchVertex] = true;
            //オブジェクト生成
            Instantiate(_prefab, new Vector3(searchVertex.x, searchVertex.y, searchVertex.z), Quaternion.identity);
            yield return wait;


            //隣接頂点を調べる
            foreach (var connect in _algorithm.ConnectDic[searchVertex])
            {
                //探索済みの頂点は飛ばす
                if (vertex[connect]) continue;

                //新たに探索した頂点をキューに追加
                vertexQueue.Enqueue(connect);
            }
        }

        Debug.Log("BFS End");
        yield break;
    }

    /// <summary>
    /// DFSを用いて迷路を生成する関数
    /// </summary>
    public void DFS()
    {
        if (_startIndexX < 1 || (_algorithm.LabyrinthSizeX - 1) - 1 < _startIndexX)
        {
            Debug.LogWarning("X:スタートの座標を迷路のサイズ内に設定してください");
            return;
        }

        if (_startIndexY < 1 || (_algorithm.LabyrinthSizeY - 1) - 1 < _startIndexY)
        {
            Debug.LogWarning("Y:スタートの座標を迷路のサイズ内に設定してください");
            return;
        }

        if (_startIndexZ < 1 || (_algorithm.LabyrinthSizeZ - 1) - 1 < _startIndexZ)
        {
            Debug.LogWarning("Z:スタートの座標を迷路のサイズ内に設定してください");
            return;
        }

        if (!_algorithm.ConnectDic.ContainsKey((_startIndexX, _startIndexY, _startIndexZ)))
        {
            Debug.LogWarning("スタート地点に通れる場所がありません");
            return;
        }

        StartCoroutine(DFSCoroutine());
    }

    IEnumerator DFSCoroutine()
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

        //探索頂点のスタック
        Stack<(int x, int y, int z)> vertexStack = new Stack<(int x, int y, int z)>();
        //スタートをスタックに追加
        vertexStack.Push((_startIndexX, _startIndexY, _startIndexZ));

        var wait = new WaitForSeconds(CREATEINTERVAL);
        while (vertexStack.Count != 0)
        {
            var searchVertex = vertexStack.Pop();
            //探索済みに更新
            vertex[searchVertex] = true;
            //オブジェクト生成
            Instantiate(_prefab, new Vector3(searchVertex.x, searchVertex.y, searchVertex.z), Quaternion.identity);
            yield return wait;

            //隣接頂点を調べる
            foreach (var v in _algorithm.ConnectDic[searchVertex])
            {
                //探索済みの頂点は飛ばす
                if (vertex[v]) continue;

                //隣接頂点をスタックに追加
                vertexStack.Push(v);
            }
        }

        Debug.Log("DFS End");
        yield break;
    }
}
