using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CC = Coordinate.CoordinateComponent;

namespace Coordinate
{
    /// <summary>いずれかの軸に必ず平行な辺で構成された領域の頂点が持つ座標の成分のクラス</summary>
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
        /// このクラスが持つ頂点の座標の成分を取得する関数
        /// </summary>
        /// <returns>このクラスが持つ頂点の座標の成分</returns>
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

    /// <summary>領域の集合</summary>
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
    /// 領域分割アルゴリズムを走らせる関数
    /// </summary>
    public void Separate()
    {
        StartCoroutine(SeparateCoroutine());
    }

    IEnumerator SeparateCoroutine()
    {
        //分割する領域の頂点の座標の成分をリストに追加
        _coordinateList.Add(new CC(0, _areaSizeX, 0, _areaSizeY, 0, _areaSizeZ));

        var wait = new WaitForSeconds(_drawInterval);
        //分割数が任意の数になるまで繰り返す
        while (_coordinateList.Count < _separateValue)
        {
            //分割する領域を取得
            //ccはCoordinateComponentの頭文字
            var cc = _coordinateList[0].GetCoordinateComponent();
            _coordinateList.RemoveAt(0);

            //分割する軸を決める
            int separateAxis = Random.Range(0, 3);
            //分割する比率を決める
            float separateRate = Random.Range(0.3f, 0.7f);
            //分割しない場合も考慮した分割点の初期設定
            float separateX1 = cc.x.minX, separateX2 = cc.x.maxX;
            float separateY1 = cc.y.minY, separateY2 = cc.y.maxY;
            float separateZ1 = cc.z.minZ, separateZ2 = cc.z.maxZ;
            //軸と比率に合わせて分割点を決定
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

            //分割後の領域を追加
            _coordinateList.Add(new CC(cc.x.minX, separateX2, cc.y.minY, separateY2, cc.z.minZ, separateZ2));
            _coordinateList.Add(new CC(separateX1, cc.x.maxX, separateY1, cc.y.maxY, separateZ1, cc.z.maxZ));
            //リストを体積の降順にソート
            _coordinateList = new List<CC>(
                _coordinateList
                .OrderByDescending(cc =>
                {
                    var coord = cc.GetCoordinateComponent();
                    return Mathf.Abs(coord.x.maxX - coord.x.minX) * Mathf.Abs(coord.y.maxY - coord.y.minY) * Mathf.Abs(coord.z.maxZ - coord.z.minZ);
                })
                .ToList());

            //領域を描画
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
    /// 与えられた頂点の座標の成分から領域を描画する関数
    /// </summary>
    /// <param name="cc">描画したい領域の頂点の座標の成分を持つクラス</param>
    void DrawLine(CC cc)
    {
        //頂点成分を取得
        var ccGet = cc.GetCoordinateComponent();
        //頂点集合
        Vector3[] coordinate = {
            new Vector3(ccGet.x.minX, ccGet.y.minY, ccGet.z.minZ)
            ,new Vector3(ccGet.x.maxX, ccGet.y.minY, ccGet.z.minZ)
            ,new Vector3(ccGet.x.minX, ccGet.y.maxY, ccGet.z.minZ)
            ,new Vector3(ccGet.x.maxX, ccGet.y.maxY, ccGet.z.minZ)
            ,new Vector3(ccGet.x.minX, ccGet.y.minY, ccGet.z.maxZ)
            ,new Vector3(ccGet.x.maxX, ccGet.y.minY, ccGet.z.maxZ)
            ,new Vector3(ccGet.x.minX, ccGet.y.maxY, ccGet.z.maxZ)
            ,new Vector3(ccGet.x.maxX, ccGet.y.maxY, ccGet.z.maxZ)};

        //2種類の頂点について調べる
        for (int i = 0; i < coordinate.Length - 1; i++)
        {
            for (int j = i; j < coordinate.Length; j++)
            {
                var coordinateDiff = coordinate[i] - coordinate[j];
                //すべての頂点の差が0でない可能性を排除
                if (coordinateDiff.x * coordinateDiff.y * coordinateDiff.z == 0)
                {
                    //各座標に0でない数字を2つ以上持っている可能性を排除
                    if (coordinateDiff.x * coordinateDiff.y + coordinateDiff.y * coordinateDiff.z + coordinateDiff.z * coordinateDiff.x == 0)
                    {
                        Debug.DrawLine(coordinate[i], coordinate[j], Color.red);
                    }
                }
            }
        }
    }
}
