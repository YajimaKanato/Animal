using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CC = Coordinate.CoordinateComponent;

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
    List<CC> _coordinateList = new List<CC>();
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
        if (_coordinateList.Count > 0 && _isDraw)
        {
            foreach (var coord in _coordinateList)
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
        _coordinateList.Add(new CC(0, _areaSizeX, 0, _areaSizeY, 0, _areaSizeZ));

        var wait = new WaitForSeconds(_drawInterval);
        //�������������̐��ɂȂ�܂ŌJ��Ԃ�
        while (_coordinateList.Count < _algorithm.RoomNum)
        {
            //��������̈���擾
            //cc��CoordinateComponent�̓�����
            var cc = _coordinateList[0].GetCoordinateComponent();
            _coordinateList.RemoveAt(0);

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
            _coordinateList.Add(new CC(cc.x.minX, separateX2, cc.y.minY, separateY2, cc.z.minZ, separateZ2));
            _coordinateList.Add(new CC(separateX1, cc.x.maxX, separateY1, cc.y.maxY, separateZ1, cc.z.maxZ));
            //���X�g��̐ς̍~���Ƀ\�[�g
            _coordinateList = new List<CC>(
                _coordinateList
                .OrderByDescending(cc =>
                {
                    var coord = cc.GetCoordinateComponent();
                    return Mathf.Abs(coord.x.maxX - coord.x.minX) * Mathf.Abs(coord.y.maxY - coord.y.minY) * Mathf.Abs(coord.z.maxZ - coord.z.minZ);
                })
                .ToList());

            if (_isDraw)
            {
                //�̈��`��
                foreach (var area in _coordinateList)
                {
                    var coord = area.GetCoordinateComponent();
                    DrawLine(area);
                }
            }
            yield return wait;
        }
        Debug.Log("Separate Complete");

        //���X�g��x��y��z�̏��ɏ����Ń\�[�g����
        _coordinateList = new List<CC>(
            _coordinateList
            .OrderBy(z => z.GetCoordinateComponent().z.minZ)
            .ThenBy(y => y.GetCoordinateComponent().y.minY)
            .ThenBy(x => x.GetCoordinateComponent().x.minX))
            .ToList();

        /*
        //�I�u�W�F�N�g�z�u
        foreach (var area in GetPosition())
        {
            Instantiate(_prefab, new Vector3(area.x, area.y, area.z), Quaternion.identity);
            yield return wait;
        }*/

        Debug.Log("Put Object Complete");
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
                //���ׂĂ̒��_�̍���0�łȂ��\����r��
                if (coordinateDiff.x * coordinateDiff.y * coordinateDiff.z == 0)
                {
                    //�e���W��0�łȂ�������2�ȏ㎝���Ă���\����r��
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
            _coordinateList
            .Select(cc =>
            {
                var coord = cc.GetCoordinateComponent();
                return ((int)((coord.x.maxX + coord.x.minX) / 2), (int)((coord.y.maxY + coord.y.minY) / 2), (int)((coord.z.maxZ + coord.z.minZ) / 2));
            })
            .ToList());
    }
}
