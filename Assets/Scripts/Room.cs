using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField, Tooltip("連結ポイントの座標")] Transform _right, _left, _up, _down, _forward, _back;

    /// <summary>
    /// この部屋のIDを受け取り連結ポイントの座標を知らせる関数
    /// </summary>
    /// <param name="pos">ID</param>
    /// <param name="algorithm">クラス</param>
    /// <param name="labyrinth">クラス</param>
    public void SetID((int x, int y, int z) pos, LabyrinthAlgorithm algorithm, CreateLabyrinth labyrinth)
    {
        foreach (var connect in algorithm.ConnectDic[pos])
        {
            if (connect.x - pos.x == 1)
            {
                labyrinth.RegisterDic(connect, _right.position);
                continue;
            }

            if (connect.x - pos.x == -1)
            {
                labyrinth.RegisterDic(connect, _left.position);
                continue;
            }

            if (connect.y - pos.y == 1)
            {
                labyrinth.RegisterDic(connect, _up.position);
                continue;
            }

            if (connect.y - pos.y == -1)
            {
                labyrinth.RegisterDic(connect, _down.position);
                continue;
            }

            if (connect.z - pos.z == 1)
            {
                labyrinth.RegisterDic(connect, _forward.position);
                continue;
            }

            if (connect.z - pos.z == -1)
            {
                labyrinth.RegisterDic(connect, _back.position);
                continue;
            }
        }
    }

}
