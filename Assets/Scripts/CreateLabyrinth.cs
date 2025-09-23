using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CreateLabyrinth : MonoBehaviour
{
    [SerializeField] GameObject _room, _load;
    [SerializeField, Tooltip("生成間隔")] float _createInterval = 0.05f;
    [Header("スタートの位置（すべて奇数か偶数は１つまで）")]
    [SerializeField, Tooltip("1以上{(迷路のサイズ)*2+1}未満の数字でインデックスを指定")] int _startIndexX = 0;
    [SerializeField, Tooltip("1以上{(迷路のサイズ)*2+1}未満の数字でインデックスを指定")] int _startIndexY = 0;
    [SerializeField, Tooltip("1以上{(迷路のサイズ)*2+1}未満の数字でインデックスを指定")] int _startIndexZ = 0;

    LabyrinthAlgorithm _algorithm;
    AreaSeparate _separate;
    /// <summary>連結ポイントのペアを持つ辞書</summary>
    Dictionary<(int x, int y, int z), Queue<Vector3>> _connectPointPair = new Dictionary<(int x, int y, int z), Queue<Vector3>>();
    /// <summary>探索アルゴリズムで調べた頂点（道）の順番を保持するキュー</summary>
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
                        Instantiate(_room, new Vector3(l, m, n), Quaternion.identity);
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

        var wait = new WaitForSeconds(_createInterval);
        while (vertexQueue.Count != 0)
        {
            var searchVertex = vertexQueue.Dequeue();
            //探索済みに更新
            vertex[searchVertex] = true;
            if (searchVertex.x * searchVertex.y * searchVertex.z % 2 != 0)
            {
                //部屋の時にオブジェクト生成
                var pos = _separate.PositionDic[searchVertex];
                var go = Instantiate(_room, new Vector3(pos.x, pos.y, pos.z), Quaternion.identity);
                //生成した部屋にIDを伝える
                go.GetComponent<Room>().SetID(searchVertex, _algorithm, this);
            }
            else
            {
                //調べた道をキューに追加
                _searchLoadOrder.Enqueue(searchVertex);
            }
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

        Debug.Log("BFS Complete");

        yield return LoadSetCoroutine();

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

        var wait = new WaitForSeconds(_createInterval);
        while (vertexStack.Count != 0)
        {
            var searchVertex = vertexStack.Pop();
            //探索済みに更新
            vertex[searchVertex] = true; if (searchVertex.x * searchVertex.y * searchVertex.z % 2 != 0)
            {
                //部屋の時にオブジェクト生成
                var pos = _separate.PositionDic[searchVertex];
                var go = Instantiate(_room, new Vector3(pos.x, pos.y, pos.z), Quaternion.identity);
                //生成した部屋にIDを伝える
                go.GetComponent<Room>().SetID(searchVertex, _algorithm, this);
            }
            else
            {
                //調べた道をキューに追加
                _searchLoadOrder.Enqueue(searchVertex);
            }
            yield return wait;

            //隣接頂点を調べる
            foreach (var connect in _algorithm.ConnectDic[searchVertex])
            {
                //探索済みの頂点は飛ばす
                if (vertex[connect]) continue;

                //隣接頂点をスタックに追加
                vertexStack.Push(connect);
            }
        }

        Debug.Log("DFS Complete");

        yield return LoadSetCoroutine();

        yield break;
    }

    /// <summary>
    /// IDと連結ポイントのペアの辞書に登録する関数
    /// </summary>
    /// <param name="vertex">連結ポイント</param>
    /// <param name="pos">連結ポイントの座標</param>
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
            var connect = _connectPointPair[load];//連結ポイントのペア取得
            var start = connect.Dequeue();//連結開始点
            var end = connect.Dequeue();//連結終了点
            var mid = (start + end) / 2;//連結ポイントの中点
            //それぞれの連結ポイントがワールド座標の正負どちらにあるか
            var dirx = end.x - start.x > 0 ? 1 : -1;
            var diry = end.y - start.y > 0 ? 1 : -1;
            var dirz = end.z - start.z > 0 ? 1 : -1;
            var wait = new WaitForSeconds(_createInterval);

            if (load.x % 2 == 0)
            {
                //中点まで伸ばす
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
                //中点から伸ばす
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
                //中点まで伸ばす
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
                //中点から伸ばす
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
                //中点まで伸ばす
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
                //中点から伸ばす
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
