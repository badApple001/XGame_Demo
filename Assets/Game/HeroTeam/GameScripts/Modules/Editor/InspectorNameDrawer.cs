using UnityEngine;
using UnityEditor;

namespace GameScripts.HeroTeam
{
    /// <summary>
    /// 自定义字段名显示中文
    /// </summary>
    [CustomPropertyDrawer(typeof(InspectorNameAttribute))]
    public class InspectorNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var name = (InspectorNameAttribute)attribute;
            label.text = name.DisplayName;
            EditorGUI.PropertyField(position, property, label);
        }
    }

}