using UnityEngine;
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

    const int XY = 0;
    const int YZ = 1;
    const int ZX = 2;
    const float DRAWINTERVAL = 0.05f;

    /// <summary>
    /// �̈敪���A���S���Y���𑖂点��֐�
    /// </summary>
    public void Separate()
    {
        StartCoroutine(SeparateCoroutine());
    }

    IEnumerator SeparateCoroutine()
    {
        //Coordinate�N���X�̃L���[���쐬���邱�Ƃŗ̈�̏W�����쐬
        Queue<CC> coordinateQueue = new Queue<CC>();
        //��������̈�̒��_�̍��W�̐������L���[�ɒǉ�
        coordinateQueue.Enqueue(new CC(0, 1, 0, 1, 0, 1));

        var wait = new WaitForSeconds(DRAWINTERVAL);
        //���������C�ӂ̐��ɂȂ�܂ŌJ��Ԃ�
        while (coordinateQueue.Count < _separateValue)
        {
            //��������̈���擾
            //cc��CoordinateComponent�̓�����
            var cc = coordinateQueue.Dequeue().GetCoordinateComponent();

            //�������鎲�����߂�
            int separateAxis = Random.Range(0, 3);
            //��������䗦�����߂�
            float separateRate = Random.Range(0.4f, 0.6f);
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

            //������̗̈���쐬
            var newArea1 = new CC(cc.x.minX, separateX2, cc.y.minY, separateY2, cc.z.minZ, separateZ2);
            var newArea2 = new CC(separateX1, cc.x.maxX, separateY1, cc.y.maxY, separateZ1, cc.z.maxZ);
            //�̈�̑傫�����傫���ق����珇�ɃL���[�ɒǉ�
            if (separateRate < 0.5f)
            {
                coordinateQueue.Enqueue(newArea2);
                coordinateQueue.Enqueue(newArea1);
            }
            else
            {
                coordinateQueue.Enqueue(newArea1);
                coordinateQueue.Enqueue(newArea2);
            }

            //�̈��`��
            foreach (var area in coordinateQueue)
            {
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
            new(ccGet.x.minX, ccGet.y.minY, ccGet.z.minZ)
            ,new(ccGet.x.maxX, ccGet.y.minY, ccGet.z.minZ)
            ,new(ccGet.x.minX, ccGet.y.maxY, ccGet.z.minZ)
            ,new(ccGet.x.maxX, ccGet.y.maxY, ccGet.z.minZ)
            ,new(ccGet.x.minX, ccGet.y.minY, ccGet.z.maxZ)
            ,new(ccGet.x.maxX, ccGet.y.minY, ccGet.z.maxZ)
            ,new(ccGet.x.minX, ccGet.y.maxY, ccGet.z.maxZ)
            ,new(ccGet.x.maxX, ccGet.y.maxY, ccGet.z.maxZ)};

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
