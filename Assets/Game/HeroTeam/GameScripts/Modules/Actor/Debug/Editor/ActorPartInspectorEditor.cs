using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using XGame.Entity;
using XGame.Entity.Part;

namespace GameScripts.HeroTeam
{
    [CustomEditor(typeof(ActorPartInspector))]
    public class ActorPartInspectorEditor : Editor
    {
        private ActorPartInspector m_actorPartInspector;
        private FieldInfo partsField;

        private GUIStyle m_headerStyle;
        private GUIStyle m_tagStyle;
        private GUIStyle m_outerBoxStyle;
        private bool m_showEntityInfo = false;
        private bool m_stylesInitialized = false;

        void OnEnable()
        {
            m_actorPartInspector = (ActorPartInspector)target;
            partsField = typeof(BaseEntity).GetField("m_Parts", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void OnInspectorGUI()
        {
            if (m_actorPartInspector == null || m_actorPartInspector.Entity == null) return;

            var parts = (Dictionary<Type, IEntityPart>)partsField.GetValue(m_actorPartInspector.Entity);
            if (parts == null || parts.Count == 0) return;

            InitStyles();

            GUILayout.Space(6);
            GUILayout.BeginVertical(m_outerBoxStyle); // 灰色外框
            GUILayout.Label("Parts", m_headerStyle);
            GUILayout.Space(6);

            float maxWidth = EditorGUIUtility.currentViewWidth - 40;
            float currentWidth = 0;
            EditorGUILayout.BeginHorizontal();

            foreach (var part in parts)
            {
                string shortName = part.Key.Name;
                GUIContent labelContent = new GUIContent(shortName);
                Vector2 size = m_tagStyle.CalcSize(labelContent);

                if (currentWidth + size.x > maxWidth)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    currentWidth = 0;
                }

                // 绿色背景
                GUI.backgroundColor = new Color(0, 1f, 0);

                if (GUILayout.Button(labelContent, m_tagStyle))
                {
                    MonoScript script = FindScriptFromType(part.Key);
                    if (script != null)
                        AssetDatabase.OpenAsset(script);
                }

                GUI.backgroundColor = Color.white; // 重置背景色
                currentWidth += size.x + 8;
            }

            EditorGUILayout.EndHorizontal();

            // 新增折叠Entity Info区
            GUILayout.Space(10);
            m_showEntityInfo = EditorGUILayout.Foldout(m_showEntityInfo, "Entity Info", true);
            if (m_showEntityInfo)
            {
                GUILayout.BeginVertical(m_outerBoxStyle);

                // 显示5个字段，只读
                DrawReadOnlyField("id", m_actorPartInspector.Entity.id.ToString());
                DrawReadOnlyField("configId", m_actorPartInspector.Entity.configId.ToString());
                DrawReadOnlyField("type", m_actorPartInspector.Entity.type.ToString());
                DrawReadOnlyField("name", m_actorPartInspector.Entity.name);
                DrawReadOnlyField("config", m_actorPartInspector.Entity.config);
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();
        }

        private void DrawReadOnlyField(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(80));
            EditorGUILayout.SelectableLabel(value, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawReadOnlyField(string label, object jsonOjbct)
        {
            string configJson = "null";
            if (jsonOjbct != null)
            {
                try
                {
                    configJson = JsonUtility.ToJson(jsonOjbct, true); // true表示格式化输出
                }
                catch
                {
                    configJson = jsonOjbct.ToString();
                }
            }
            int lineCount = configJson.Split('\n').Length;
            float lineHeight = 30f; // 一行文本高度，约18像素
            float height = Mathf.Clamp(lineCount * lineHeight, lineHeight * 2, lineHeight * 10); // 最小2行，高最大10行
            EditorGUILayout.LabelField(label);
            EditorGUILayout.TextArea(configJson, EditorStyles.textArea, GUILayout.Height(height));  // 高度可以自己调
            GUILayout.Space(4);
        }


        private void InitStyles()
        {
            if (m_stylesInitialized) return;

            m_headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.2f, 0.6f, 1f) }
            };

            m_tagStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,  // 字体加粗
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                hover = { textColor = Color.gray },
                margin = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(10, 10, 4, 4),
                wordWrap = false
            };

            m_outerBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            m_stylesInitialized = true;
        }

        private MonoScript FindScriptFromType(Type type)
        {
            string[] guids = AssetDatabase.FindAssets($"{type.Name} t:Script");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                    return script;
            }
            return null;
        }
    }
}
