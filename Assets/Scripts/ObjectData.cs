using UnityEditor;
using UnityEngine;

public class ObjectData : MonoBehaviour
{
    [SerializeField] bool _useRight, _useLeft, _useUp, _useDown, _useForward, _useBack;
    [SerializeField] NextObjectData _right, _left, _up, _down, _forward, _back;
    [SerializeField] NextObjectData _wallR, _wallL, _wallU, _wallD, _wallF, _wallB;

    private void Start()
    {

    }

    void RightSet()
    {

    }

    void LeftSet()
    {

    }

    void UpSet()
    {

    }

    void DownSet()
    {

    }

    void ForwardSet()
    {

    }

    void BackSet()
    {

    }

    /// <summary>
    /// インスペクターをカスタムするためのクラス
    /// </summary>
    [CustomEditor(typeof(ObjectData))]
    class InspectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var _data = (ObjectData)target;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("道がつながっているか");
            //右方向に関する設定を表示
            _data._useRight = EditorGUILayout.ToggleLeft("Right", _data._useRight);
            if (_data._useRight)
            {
                var _rightProp = serializedObject.FindProperty(nameof(_data._right));
                EditorGUILayout.ObjectField(_rightProp);
                _rightProp = serializedObject.FindProperty(nameof(_data._wallR));
                EditorGUILayout.ObjectField(_rightProp);
            }

            //左方向に関する設定を表示
            _data._useLeft = EditorGUILayout.ToggleLeft("Left", _data._useLeft);
            if (_data._useLeft)
            {
                var _leftProp = serializedObject.FindProperty(nameof(_data._left));
                EditorGUILayout.ObjectField(_leftProp);
                _leftProp = serializedObject.FindProperty(nameof(_data._wallL));
                EditorGUILayout.ObjectField(_leftProp);
            }

            //上方向に関する設定を表示
            _data._useUp = EditorGUILayout.ToggleLeft("Up", _data._useUp);
            if (_data._useUp)
            {
                var _upProp = serializedObject.FindProperty(nameof(_data._up));
                EditorGUILayout.ObjectField(_upProp); 
                _upProp = serializedObject.FindProperty(nameof(_data._wallU));
                EditorGUILayout.ObjectField(_upProp);
            }

            //下方向に関する設定を表示
            _data._useDown = EditorGUILayout.ToggleLeft("Down", _data._useDown);
            if (_data._useDown)
            {
                var _downProp = serializedObject.FindProperty(nameof(_data._down));
                EditorGUILayout.ObjectField(_downProp);
                _downProp = serializedObject.FindProperty(nameof(_data._wallD));
                EditorGUILayout.ObjectField(_downProp);
            }

            //前方向に関する設定を表示
            _data._useForward = EditorGUILayout.ToggleLeft("Forward", _data._useForward);
            if (_data._useForward)
            {
                var _forwardProp = serializedObject.FindProperty(nameof(_data._forward));
                EditorGUILayout.ObjectField(_forwardProp);
                _forwardProp = serializedObject.FindProperty(nameof(_data._wallF));
                EditorGUILayout.ObjectField(_forwardProp);
            }

            //後ろ方向に関する設定を表示
            _data._useBack = EditorGUILayout.ToggleLeft("Back", _data._useBack);
            if (_data._useBack)
            {
                var _backProp = serializedObject.FindProperty(nameof(_data._back));
                EditorGUILayout.ObjectField(_backProp);
                _backProp = serializedObject.FindProperty(nameof(_data._wallB));
                EditorGUILayout.ObjectField(_backProp);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_data);
            }
        }
    }
}
