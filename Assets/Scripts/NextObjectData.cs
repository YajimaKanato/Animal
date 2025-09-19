using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 任意の方向につなげるオブジェクトをまとめて持つクラス
/// </summary>
[CreateAssetMenu(fileName = "NextObjectData", menuName = "Scriptable Objects/NextObjectData")]
public class NextObjectData : ScriptableObject
{
    [SerializeField] List<GameObject> _nextObject;
    public List<GameObject> NextObject => _nextObject;
}


