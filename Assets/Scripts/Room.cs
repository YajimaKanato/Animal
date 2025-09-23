using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField, Tooltip("�A���|�C���g�̍��W")] Transform _right, _left, _up, _down, _forward, _back;

    /// <summary>
    /// ���̕�����ID���󂯎��A���|�C���g�̍��W��m�点��֐�
    /// </summary>
    /// <param name="pos">ID</param>
    /// <param name="algorithm">�N���X</param>
    /// <param name="labyrinth">�N���X</param>
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
