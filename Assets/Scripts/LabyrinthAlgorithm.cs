using System;
using System.Collections.Generic;
using UnityEngine;

public class LabyrinthAlgorithm : MonoBehaviour
{
    [Header("LabyrinthSize")]
    [SerializeField, Tooltip("��")] int _labyrinthSizeX = 5;
    [SerializeField, Tooltip("�K�w��")] int _labyrinthSizeY = 5;
    [SerializeField, Tooltip("���s")] int _labyrinthSizeZ = 5;
    [Header("�X�^�[�g�̈ʒu")]
    [SerializeField] int _startIndexX = 0;
    [SerializeField] int _startIndexY = 0;
    [SerializeField] int _startIndexZ = 0;

    /// <summary>�A���S���Y�����I�������ɍs���֐������f���Q�[�g</summary>
    public static Action CreateLabyrinth;


    /// <summary>�����A���A�ǂ��ׂĂ�ID</summary>
    int[,,] _roomID;
    /// <summary>�����A���A�ǂ��ׂĂ�ID���擾����v���p�e�B</summary>
    public int[,,] RoomID { get { return _roomID; } }
    /// <summary>�A���S���Y���𑖂点��Ƃ��̃T�C�Y</summary>
    int _xlen, _ylen, _zlen;
    /// <summary>���H�̕����擾����v���p�e�B</summary>
    public int LabyrinthSizeX { get { return _xlen; } }
    /// <summary>���H�̊K�w�����擾����v���p�e�B</summary>
    public int LabyrinthSizeY { get { return _ylen; } }
    /// <summary>���H�̉��s���擾����v���p�e�B</summary>
    public int LabyrinthSizeZ { get { return _zlen; } }

    //�A���S���Y���Ɏg�p����萔�l
    const int X = 0;
    const int Y = 1;
    const int Z = 2;
    const int PLUS = 1;
    const int MINUS = -1;
    const int PASS = 1;
    const int WALL = 0;
    //�v���C���[���A�N�V�������s���Ƃ�����u�����v�Ƃ���

    private void Awake()
    {
        LabyrinthCreate();
    }

    /// <summary>
    /// ���H�𐶐�����A���S���Y���𑖂点��֐�
    /// </summary>
    void LabyrinthCreate()
    {
        LabyrinthCreateSetUp();

        for (int i = 0; i < (_xlen / 2) * (_ylen / 2) * (_zlen / 2) - 1; i++)
        {
            bool connect = false;//�A���������ǂ���
            do
            {
                int randx, randy, randz;//�����_���ɑI�΂ꂽ���W

                //�����_���ɑI�񂾏ꏊ�������ɂȂ�܂ŌJ��Ԃ�
                do
                {
                    randx = UnityEngine.Random.Range(1, _xlen - 1);
                    randy = UnityEngine.Random.Range(1, _ylen - 1);
                    randz = UnityEngine.Random.Range(1, _zlen - 1);
                } while (randx * randy * randz % 2 != 1);

                //6�����ɑ΂��ăA�v���[�`
                List<(int axis, int sign)> axisSignPair = new List<(int, int)>()
                { (X, PLUS), (X, MINUS), (Y, PLUS), (Y, MINUS), (Z, PLUS), (Z, MINUS) };

                for (int j = 0; j < 6; j++)
                {
                    //�U�����̂ǂ�����I�Ԃ��A���łɑI�񂾂Ƃ���͑I�΂Ȃ�
                    int axisSignRand = UnityEngine.Random.Range(0, axisSignPair.Count);
                    connect = RoomConnectCheck(axisSignPair[axisSignRand].axis, axisSignPair[axisSignRand].sign, randx, randy, randz);
                    if (connect)
                    {
                        //�A�������烋�[�v�𔲂���
                        RoomConnect(axisSignPair[axisSignRand].axis, axisSignPair[axisSignRand].sign, randx, randy, randz);
                        break;
                    }
                    else
                    {
                        //�A�v���[�`�̌�₩��r��
                        axisSignPair.RemoveAt(axisSignRand);
                    }
                }
            } while (!connect);
        }

        Debug.Log("CreateLabyrinth");
        CreateLabyrinth();
    }

    /// <summary>
    /// �A���S���Y���𑖂点��Ƃ��̏��������s���֐�
    /// </summary>
    void LabyrinthCreateSetUp()
    {
        //�A���S���Y���𑖂点��Ƃ��̃T�C�Y��ݒ�
        _xlen = _labyrinthSizeX * 2 + 1;
        _ylen = _labyrinthSizeY * 2 + 1;
        _zlen = _labyrinthSizeZ * 2 + 1;

        //���⒌�ɓ����镔�����܂߂��T�C�Y�ŏ�����
        _roomID = new int[_xlen, _ylen, _zlen];

        //�����ɔԍ��𓖂ĂĂ���
        for (int n = 0; n < _zlen; n++)
        {
            for (int m = 0; m < _ylen; m++)
            {
                for (int l = 0; l < _xlen; l++)
                {
                    if (l * m * n % 2 == 1)
                    {
                        //x,y,z�����ׂĊ�̎��͕����ɊY�����A���ׂĂ̕����ɕʁX�̔ԍ���t�^
                        _roomID[l, m, n] = n * _ylen * _xlen + m * _xlen + l;
                    }
                    else
                    {
                        //����ȊO�͂��ׂ�0�ŏ�����
                        _roomID[l, m, n] = WALL;
                    }
                }
            }
        }
    }

    /// <summary>
    /// �I�΂ꂽ�����ƘA�����悤�Ƃ��Ă��镔�����A���\���ǂ�����Ԃ��֐�
    /// </summary>
    /// <param name="axis">�A������������w����</param>
    /// <param name="pm">�A���������</param>
    /// <param name="randx">�I�΂ꂽ������x���W</param>
    /// <param name="randy">�I�΂ꂽ������y���W</param>
    /// <param name="randz">�I�΂ꂽ������z���W</param>
    /// <returns>�A���\���ǂ���</returns>
    bool RoomConnectCheck(int axis, int pm, int randx, int randy, int randz)
    {
        bool roomConnectable = false;//�A���\���ǂ���
        int roomID = _roomID[randx, randy, randz];//�I�΂ꂽ������ID
        if (pm == PLUS)
        {
            switch (axis)
            {
                //�u�I�΂ꂽ�����̍��W����ԑ傫�����W�ł͂Ȃ��v���u�A�����悤�Ƃ��Ă��镔���ɂ܂��A�����Ă��Ȃ��v
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
                //�u�I�΂ꂽ�����̍��W����ԏ��������W�ł͂Ȃ��v���u�A�����悤�Ƃ��Ă��镔���ɂ܂��A�����Ă��Ȃ��v
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
    /// ������A������֐�
    /// </summary>
    /// <param name="axis">�A������������w����</param>
    /// <param name="pm">�A���������</param>
    /// <param name="randx">�I�΂ꂽ������x���W</param>
    /// <param name="randy">�I�΂ꂽ������y���W</param>
    /// <param name="randz">�I�΂ꂽ������z���W</param>
    void RoomConnect(int axis, int pm, int randx, int randy, int randz)
    {
        int connectedRoomID = 0;//�A�����ꂽ������ID
        switch (axis)
        {
            case X:
                connectedRoomID = _roomID[randx + 2 * pm, randy, randz];
                //�����̊Ԃ̕ǂ��폜
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

        //������ID�̍X�V
        for (int n = 0; n < _zlen; n++)
        {
            for (int m = 0; m < _ylen; m++)
            {
                for (int l = 0; l < _xlen; l++)
                {
                    if (l * m * n % 2 == 1)
                    {
                        //���ׂĂ���̎�������
                        if (_roomID[l, m, n] == connectedRoomID)
                        {
                            //�A�����ꂽ���̕�����ID�ɓ��������̂͂��ׂđI�΂ꂽ������ID�ɏ㏑��
                            _roomID[l, m, n] = _roomID[randx, randy, randz];
                        }
                    }
                }
            }
        }
    }
}