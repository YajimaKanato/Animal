using System;
using System.Collections.Generic;
using UnityEngine;

public class LabyrinthAlgorithm : MonoBehaviour
{
    [Header("LabyrinthSize")]
    [SerializeField, Tooltip("幅")] int _labyrinthSizeX = 5;
    [SerializeField, Tooltip("階層数")] int _labyrinthSizeY = 5;
    [SerializeField, Tooltip("奥行")] int _labyrinthSizeZ = 5;
    [Header("スタートの位置")]
    [SerializeField] int _startIndexX = 0;
    [SerializeField] int _startIndexY = 0;
    [SerializeField] int _startIndexZ = 0;

    /// <summary>アルゴリズムが終わった後に行う関数を持つデリゲート</summary>
    public static Action CreateLabyrinth;


    /// <summary>部屋、柱、壁すべてのID</summary>
    int[,,] _roomID;
    /// <summary>部屋、柱、壁すべてのIDを取得するプロパティ</summary>
    public int[,,] RoomID { get { return _roomID; } }
    /// <summary>アルゴリズムを走らせるときのサイズ</summary>
    int _xlen, _ylen, _zlen;
    /// <summary>迷路の幅を取得するプロパティ</summary>
    public int LabyrinthSizeX { get { return _xlen; } }
    /// <summary>迷路の階層数を取得するプロパティ</summary>
    public int LabyrinthSizeY { get { return _ylen; } }
    /// <summary>迷路の奥行を取得するプロパティ</summary>
    public int LabyrinthSizeZ { get { return _zlen; } }

    //アルゴリズムに使用する定数値
    const int X = 0;
    const int Y = 1;
    const int Z = 2;
    const int PLUS = 1;
    const int MINUS = -1;
    const int PASS = 1;
    const int WALL = 0;
    //プレイヤーがアクションを行うところを「部屋」とする

    private void Awake()
    {
        LabyrinthCreate();
    }

    /// <summary>
    /// 迷路を生成するアルゴリズムを走らせる関数
    /// </summary>
    void LabyrinthCreate()
    {
        LabyrinthCreateSetUp();

        for (int i = 0; i < (_xlen / 2) * (_ylen / 2) * (_zlen / 2) - 1; i++)
        {
            bool connect = false;//連結したかどうか
            do
            {
                int randx, randy, randz;//ランダムに選ばれた座標

                //ランダムに選んだ場所が部屋になるまで繰り返す
                do
                {
                    randx = UnityEngine.Random.Range(1, _xlen - 1);
                    randy = UnityEngine.Random.Range(1, _ylen - 1);
                    randz = UnityEngine.Random.Range(1, _zlen - 1);
                } while (randx * randy * randz % 2 != 1);

                //6方向に対してアプローチ
                List<(int axis, int sign)> axisSignPair = new List<(int, int)>()
                { (X, PLUS), (X, MINUS), (Y, PLUS), (Y, MINUS), (Z, PLUS), (Z, MINUS) };

                for (int j = 0; j < 6; j++)
                {
                    //６方向のどこかを選ぶが、すでに選んだところは選ばない
                    int axisSignRand = UnityEngine.Random.Range(0, axisSignPair.Count);
                    connect = RoomConnectCheck(axisSignPair[axisSignRand].axis, axisSignPair[axisSignRand].sign, randx, randy, randz);
                    if (connect)
                    {
                        //連結したらループを抜ける
                        RoomConnect(axisSignPair[axisSignRand].axis, axisSignPair[axisSignRand].sign, randx, randy, randz);
                        break;
                    }
                    else
                    {
                        //アプローチの候補から排除
                        axisSignPair.RemoveAt(axisSignRand);
                    }
                }
            } while (!connect);
        }

        Debug.Log("CreateLabyrinth");
        CreateLabyrinth();
    }

    /// <summary>
    /// アルゴリズムを走らせるときの初期化を行う関数
    /// </summary>
    void LabyrinthCreateSetUp()
    {
        //アルゴリズムを走らせるときのサイズを設定
        _xlen = _labyrinthSizeX * 2 + 1;
        _ylen = _labyrinthSizeY * 2 + 1;
        _zlen = _labyrinthSizeZ * 2 + 1;

        //道や柱に当たる部分も含めたサイズで初期化
        _roomID = new int[_xlen, _ylen, _zlen];

        //部屋に番号を当てている
        for (int n = 0; n < _zlen; n++)
        {
            for (int m = 0; m < _ylen; m++)
            {
                for (int l = 0; l < _xlen; l++)
                {
                    if (l * m * n % 2 == 1)
                    {
                        //x,y,zがすべて奇数の時は部屋に該当し、すべての部屋に別々の番号を付与
                        _roomID[l, m, n] = n * _ylen * _xlen + m * _xlen + l;
                    }
                    else
                    {
                        //それ以外はすべて0で初期化
                        _roomID[l, m, n] = WALL;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 選ばれた部屋と連結しようとしている部屋が連結可能かどうかを返す関数
    /// </summary>
    /// <param name="axis">連結する方向を指す軸</param>
    /// <param name="pm">連結する方向</param>
    /// <param name="randx">選ばれた部屋のx座標</param>
    /// <param name="randy">選ばれた部屋のy座標</param>
    /// <param name="randz">選ばれた部屋のz座標</param>
    /// <returns>連結可能かどうか</returns>
    bool RoomConnectCheck(int axis, int pm, int randx, int randy, int randz)
    {
        bool roomConnectable = false;//連結可能かどうか
        int roomID = _roomID[randx, randy, randz];//選ばれた部屋のID
        if (pm == PLUS)
        {
            switch (axis)
            {
                //「選ばれた部屋の座標が一番大きい座標ではない」かつ「連結しようとしている部屋にまだ連結していない」
                case X:
                    roomConnectable = (randx < _xlen - 2) && (roomID != _roomID[randx + 2, randy, randz]);
                    break;
                case Y:
                    roomConnectable = (randy < _ylen - 2) && (roomID != _roomID[randx, randy + 2, randz]);
                    break;
                case Z:
                    roomConnectable = (randz < _zlen - 2) && (roomID != _roomID[randx, randy, randz + 2]);
                    break;
            }
        }
        else
        {
            switch (axis)
            {
                //「選ばれた部屋の座標が一番小さい座標ではない」かつ「連結しようとしている部屋にまだ連結していない」
                case X:
                    roomConnectable = (randx > 1) && (roomID != _roomID[randx - 2, randy, randz]);
                    break;
                case Y:
                    roomConnectable = (randy > 1) && (roomID != _roomID[randx, randy - 2, randz]);
                    break;
                case Z:
                    roomConnectable = (randz > 1) && (roomID != _roomID[randx, randy, randz - 2]);
                    break;
            }
        }

        return roomConnectable;
    }

    /// <summary>
    /// 部屋を連結する関数
    /// </summary>
    /// <param name="axis">連結する方向を指す軸</param>
    /// <param name="pm">連結する方向</param>
    /// <param name="randx">選ばれた部屋のx座標</param>
    /// <param name="randy">選ばれた部屋のy座標</param>
    /// <param name="randz">選ばれた部屋のz座標</param>
    void RoomConnect(int axis, int pm, int randx, int randy, int randz)
    {
        int connectedRoomID = 0;//連結された部屋のID
        switch (axis)
        {
            case X:
                connectedRoomID = _roomID[randx + 2 * pm, randy, randz];
                //部屋の間の壁を削除
                _roomID[randx + 1 * pm, randy, randz] = PASS;
                break;
            case Y:
                connectedRoomID = _roomID[randx, randy + 2 * pm, randz];
                _roomID[randx, randy + 1 * pm, randz] = PASS;
                break;
            case Z:
                connectedRoomID = _roomID[randx, randy, randz + 2 * pm];
                _roomID[randx, randy, randz + 1 * pm] = PASS;
                break;
        }

        //部屋のIDの更新
        for (int n = 0; n < _zlen; n++)
        {
            for (int m = 0; m < _ylen; m++)
            {
                for (int l = 0; l < _xlen; l++)
                {
                    if (l * m * n % 2 == 1)
                    {
                        //すべてが奇数の時が部屋
                        if (_roomID[l, m, n] == connectedRoomID)
                        {
                            //連結された方の部屋のIDに等しいものはすべて選ばれた部屋のIDに上書き
                            _roomID[l, m, n] = _roomID[randx, randy, randz];
                        }
                    }
                }
            }
        }
    }
}