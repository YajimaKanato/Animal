using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �C�ӂ̕����ɂȂ���I�u�W�F�N�g���܂Ƃ߂Ď��N���X
/// </summary>
[CreateAssetMenu(fileName = "NextObjectData", menuName = "Scriptable Objects/NextObjectData")]
public class NextObjectData : ScriptableObject
{
    [SerializeField] List<GameObject> _nextObject;
    public List<GameObject> NextObject => _nextObject;
}


