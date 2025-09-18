using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CC = Coordinate.CoordinateComponent;

namespace Coordinate
{
    /// <summary>�����ꂩ�̎��ɕK�����s�ȕӂō\�����ꂽ�̈�̒��_�������W�̐����̃N���X</summary>
    class CoordinateComponent
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
    [SerializeField] int _separateValue = 5;
    [SerializeField] float _areaSizeX = 10;
    [SerializeField] float _areaSizeY = 10;
    [SerializeField] float _areaSizeZ = 10;
    [SerializeField] float _drawInterval = 0.5f;

    /// <summary>�̈�̏W��</summary>
    List<CC> _coordinateList = new List<CC>();

    const int XY = 0;
    const int YZ = 1;
    const int ZX = 2;

    private void Update()
    {
        if (_coordinateList.Count > 0)
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
        //���������C�ӂ̐��ɂȂ�܂ŌJ��Ԃ�
        while (_coordinateList.Count < _separateValue)
        {
            //��������̈���擾
            //cc��CoordinateComponent�̓�����
            var cc = _coordinateList[0].GetCoordinateComponent();
            _coordinateList.RemoveAt(0);

            //�������鎲�����߂�
            int separateAxis = Random.Range(0, 3);
            //��������䗦�����߂�
            float separateRate = Random.Range(0.3f, 0.7f);
            //�������Ȃ��ꍇ���l�����������_�̏����ݒ�
            float separateX1 = cc.x.minX, separateX2 = cc.x.maxX;
            float separateY1 = cc.y.minY, separateY2 = cc.y.maxY;
            float separateZ1 = cc.z.minZ, separateZ2 = cc.z.maxZ;
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

            //�̈��`��
            foreach (var area in _coordinateList)
            {
                var coord = area.GetCoordinateComponent();
                DrawLine(area);
            }
            yield return wait;
        }

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
}
