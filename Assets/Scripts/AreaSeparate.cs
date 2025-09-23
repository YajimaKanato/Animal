using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CC = Coordinate.CoordinateComponent;
using Unity.VisualScripting;

namespace Coordinate
{
    /// <summary>�����ꂩ�̎��ɕK�����s�ȕӂō\�����ꂽ�̈�̒��_�������W�̐����̃N���X</summary>
    public class CoordinateComponent
    {
        float _minX, _maxX;
        float _minY, _maxY;
        float _minZ, _maxZ;

        public CoordinateComponent(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            _minX = minX;
            _maxX = maxX;
            _minY = minY;
            _maxY = maxY;
            _minZ = minZ;
            _maxZ = maxZ;
        }

        /// <summary>
        /// ���̃N���X�������_�̍��W�̐������擾����֐�
        /// </summary>
        /// <returns>���̃N���X�������_�̍��W�̐���</returns>
        public ((float minX, float maxX) x, (float minY, float maxY) y, (float minZ, float maxZ) z) GetCoordinateComponent()
        {
            return ((_minX, _maxX), (_minY, _maxY), (_minZ, _maxZ));
        }
    }
}

public class AreaSeparate : MonoBehaviour
{
    [SerializeField] GameObject _prefab;
    [SerializeField, Tooltip("�����Ԋu")] float _drawInterval = 0.5f;
    [Header("�̈�̃T�C�Y")]
    [SerializeField] float _areaSizeX = 10;
    [SerializeField] float _areaSizeY = 10;
    [SerializeField] float _areaSizeZ = 10;
    [Header("�̈�̕`��؂�ւ�")]
    [SerializeField] bool _isDraw = true;

    /// <summary>�̈�̏W��</summary>
    List<CC> _ccList = new List<CC>();
    Dictionary<(int x, int y, int z), (int x, int y, int z)> _positionDic = new Dictionary<(int x, int y, int z), (int x, int y, int z)>();
    public Dictionary<(int x, int y, int z), (int x, int y, int z)> PositionDic { get { return _positionDic; } }
    LabyrinthAlgorithm _algorithm;

    //�����������
    const int XY = 0;
    const int YZ = 1;
    const int ZX = 2;

    private void Start()
    {
        _algorithm = GetComponent<LabyrinthAlgorithm>();
    }

    private void Update()
    {
        if (_ccList.Count > 0 && _isDraw)
        {
            foreach (var coord in _ccList)
            {
                DrawLine(coord);
            }
        }
    }

    /// <summary>
    /// �̈敪���A���S���Y���𑖂点��֐�
    /// </summary>
    public void Separate()
    {
        StartCoroutine(SeparateCoroutine());
    }

    IEnumerator SeparateCoroutine()
    {
        //��������̈�̒��_�̍��W�̐��������X�g�ɒǉ�
        _ccList.Add(new CC(0, _areaSizeX, 0, _areaSizeY, 0, _areaSizeZ));

        var wait = new WaitForSeconds(_drawInterval);
        //�������������̐��ɂȂ�܂ŌJ��Ԃ�
        while (_ccList.Count < _algorithm.RoomNum)
        {
            //��������̈���擾
            //cc��CoordinateComponent�̓�����
            var cc = _ccList[0].GetCoordinateComponent();
            _ccList.RemoveAt(0);

            //�������Ȃ��ӂɂ��Ă��l�����������_�̏����ݒ�
            float separateX1 = cc.x.minX, separateX2 = cc.x.maxX;
            float separateY1 = cc.y.minY, separateY2 = cc.y.maxY;
            float separateZ1 = cc.z.minZ, separateZ2 = cc.z.maxZ;

            //�ӂ̒����Ƃ��̕ӂɐ����ȕ�����g�ɂ����^�v���̔z��
            var sides = new (float length, int axis)[] {
                (Mathf.Abs(separateX1 - separateX2), YZ)
                , (Mathf.Abs(separateY1 - separateY2), ZX)
                , (Mathf.Abs(separateZ1 - separateZ2), XY) };
            //��������䗦�����߂�
            float separateRate = Random.Range(0.3f, 0.7f);
            //�̈�ɂ����čł������ӂɐ����ȕ����ɕ�������悤�Ɍ��߂�
            int separateAxis = sides.OrderByDescending(side => side.length).FirstOrDefault().axis;
            //���Ɣ䗦�ɍ��킹�ĕ����_������
            switch (separateAxis)
            {
                case XY:
                    separateZ1 = separateZ2 = cc.z.maxZ * (1 - separateRate) + cc.z.minZ * separateRate;
                    break;
                case YZ:
                    separateX1 = separateX2 = cc.x.maxX * (1 - separateRate) + cc.x.minX * separateRate;
                    break;
                case ZX:
                    separateY1 = separateY2 = cc.y.maxY * (1 - separateRate) + cc.y.minY * separateRate;
                    break;
            }

            //������̗̈��ǉ�
            _ccList.Add(new CC(cc.x.minX, separateX2, cc.y.minY, separateY2, cc.z.minZ, separateZ2));
            _ccList.Add(new CC(separateX1, cc.x.maxX, separateY1, cc.y.maxY, separateZ1, cc.z.maxZ));
            //���X�g��̐ς̍~���Ƀ\�[�g
            _ccList = new List<CC>(
                _ccList
                .OrderByDescending(cc =>
                {
                    var coord = cc.GetCoordinateComponent();
                    return Mathf.Abs(coord.x.maxX - coord.x.minX) * Mathf.Abs(coord.y.maxY - coord.y.minY) * Mathf.Abs(coord.z.maxZ - coord.z.minZ);
                })
                .ToList());

            if (_isDraw)
            {
                //�̈��`��
                foreach (var area in _ccList)
                {
                    var coord = area.GetCoordinateComponent();
                    DrawLine(area);
                }
            }
            yield return wait;
        }

        //���X�g��x��y��z�̏��ɏ����Ń\�[�g����
        _ccList = new List<CC>(
            _ccList
            .OrderBy(z => z.GetCoordinateComponent().z.minZ)
            .ThenBy(y => y.GetCoordinateComponent().y.minY)
            .ThenBy(x => x.GetCoordinateComponent().x.minX))
            .ToList();
        MakePositionDictionary();

        Debug.Log("Separate Complete");

        yield break;
    }

    /// <summary>
    /// �^����ꂽ���_�̍��W�̐�������̈��`�悷��֐�
    /// </summary>
    /// <param name="cc">�`�悵�����̈�̒��_�̍��W�̐��������N���X</param>
    void DrawLine(CC cc)
    {
        //���_�������擾
        var ccGet = cc.GetCoordinateComponent();
        //���_�W��
        Vector3[] coordinate = {
            new Vector3(ccGet.x.minX, ccGet.y.minY, ccGet.z.minZ)
            ,new Vector3(ccGet.x.maxX, ccGet.y.minY, ccGet.z.minZ)
            ,new Vector3(ccGet.x.minX, ccGet.y.maxY, ccGet.z.minZ)
            ,new Vector3(ccGet.x.maxX, ccGet.y.maxY, ccGet.z.minZ)
            ,new Vector3(ccGet.x.minX, ccGet.y.minY, ccGet.z.maxZ)
            ,new Vector3(ccGet.x.maxX, ccGet.y.minY, ccGet.z.maxZ)
            ,new Vector3(ccGet.x.minX, ccGet.y.maxY, ccGet.z.maxZ)
            ,new Vector3(ccGet.x.maxX, ccGet.y.maxY, ccGet.z.maxZ)};

        //2��ނ̒��_�ɂ��Ē��ׂ�
        for (int i = 0; i < coordinate.Length - 1; i++)
        {
            for (int j = i; j < coordinate.Length; j++)
            {
                var coordinateDiff = coordinate[i] - coordinate[j];
                //���W�̍������ׂ�0�ł͂Ȃ��\����r��
                if (coordinateDiff.x * coordinateDiff.y * coordinateDiff.z == 0)
                {
                    //����0�ł͂Ȃ����W��2�ȏ゠��\����r��
                    if (coordinateDiff.x * coordinateDiff.y + coordinateDiff.y * coordinateDiff.z + coordinateDiff.z * coordinateDiff.x == 0)
                    {
                        Debug.DrawLine(coordinate[i], coordinate[j], Color.red);
                    }
                }
            }
        }
    }

    /// <summary>
    /// ���������̈�̒��S���W�̃��X�g���擾����֐�
    /// </summary>
    /// <returns>���������̈�̒��S���W�̃��X�g</returns>
    public List<(int x, int y, int z)> GetPosition()
    {
        return new List<(int x, int y, int z)>(
            _ccList
            .Select(cc =>
            {
                var coord = cc.GetCoordinateComponent();
                return ((int)((coord.x.maxX + coord.x.minX) / 2), (int)((coord.y.maxY + coord.y.minY) / 2), (int)((coord.z.maxZ + coord.z.minZ) / 2));
            })
            .ToList());
    }

    /// <summary>
    /// �̈�̒��S���W��ID���y�A�ɂ����������쐬����֐�
    /// </summary>
    void MakePositionDictionary()
    {
        var list = new List<(int x, int y, int z)>(
            _ccList
            .Select(cc =>
            {
                var coord = cc.GetCoordinateComponent();
                return ((int)((coord.x.maxX + coord.x.minX) / 2), (int)((coord.y.maxY + coord.y.minY) / 2), (int)((coord.z.maxZ + coord.z.minZ) / 2));
            })
            .ToList());

        int forx = _algorithm.LabyrinthSizeX / 2, fory = _algorithm.LabyrinthSizeY / 2, forz = _algorithm.LabyrinthSizeZ / 2;
        for (int i = 0; i < forz; i++)
        {
            for (int j = 0; j < fory; j++)
            {
                for (int k = 0; k < forx; k++)
                {
                    _positionDic.Add((k * 2 + 1, j * 2 + 1, i * 2 + 1), list[i * fory * forx + j * forx + k]);
                }
            }
        }
    }
}
