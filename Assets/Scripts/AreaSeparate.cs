using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CC = Coordinate.CoordinateComponent;

namespace Coordinate
{
    /// <summary>いずれかの軸に必ず平行な辺で構成された領域の頂点が持つ座標の成分のクラス</summary>
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
    [SerializeField] GameObject _prefab;
    [SerializeField, Tooltip("生成間隔")] float _drawInterval = 0.5f;
    [Header("領域のサイズ")]
    [SerializeField] float _areaSizeX = 10;
    [SerializeField] float _areaSizeY = 10;
    [SerializeField] float _areaSizeZ = 10;
    [Header("領域の描画切り替え")]
    [SerializeField] bool _isDraw = true;

    /// <summary>領域の集合</summary>
    List<CC> _coordinateList = new List<CC>();
    LabyrinthAlgorithm _algorithm;

    //分割する方向
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
        //分割数が部屋の数になるまで繰り返す
        while (_coordinateList.Count < _algorithm.RoomNum)
        {
            //分割する領域を取得
            //ccはCoordinateComponentの頭文字
            var cc = _coordinateList[0].GetCoordinateComponent();
            _coordinateList.RemoveAt(0);

            //分割しない辺についても考慮した分割点の初期設定
            float separateX1 = cc.x.minX, separateX2 = cc.x.maxX;
            float separateY1 = cc.y.minY, separateY2 = cc.y.maxY;
            float separateZ1 = cc.z.minZ, separateZ2 = cc.z.maxZ;

            //辺の長さとその辺に垂直な方向を組にしたタプルの配列
            var sides = new (float length, int axis)[] {
                (Mathf.Abs(separateX1 - separateX2), YZ)
                , (Mathf.Abs(separateY1 - separateY2), ZX)
                , (Mathf.Abs(separateZ1 - separateZ2), XY) };
            //分割する比率を決める
            float separateRate = Random.Range(0.3f, 0.7f);
            //領域において最も長い辺に垂直な方向に分割するように決める
            int separateAxis = sides.OrderByDescending(side => side.length).FirstOrDefault().axis;
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

            if (_isDraw)
            {
                //領域を描画
                foreach (var area in _coordinateList)
                {
                    var coord = area.GetCoordinateComponent();
                    DrawLine(area);
                }
            }
            yield return wait;
        }
        Debug.Log("Separate Complete");

        //リストをx→y→zの順に昇順でソートする
        _coordinateList = new List<CC>(
            _coordinateList
            .OrderBy(z => z.GetCoordinateComponent().z.minZ)
            .ThenBy(y => y.GetCoordinateComponent().y.minY)
            .ThenBy(x => x.GetCoordinateComponent().x.minX))
            .ToList();

        /*
        //オブジェクト配置
        foreach (var area in GetPosition())
        {
            Instantiate(_prefab, new Vector3(area.x, area.y, area.z), Quaternion.identity);
            yield return wait;
        }*/

        Debug.Log("Put Object Complete");
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

    /// <summary>
    /// 分割した領域の中心座標のリストを取得する関数
    /// </summary>
    /// <returns>分割した領域の中心座標のリスト</returns>
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
