using UnityEditor;
using UnityEngine;

namespace Ruben.SOCreator.Editor
{
    [CustomPropertyDrawer(typeof(LabelToTextFieldAttribute))]
    public class LabelToTextFieldAttributeDrawer : PropertyDrawer
    {
        private bool isEditing;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String) return;
            if (isEditing)
            {
                Rect remaining = EditorGUI.PrefixLabel(position, label); 
                property.stringValue = EditorGUI.TextField(remaining, property.stringValue);
                
                //stop editing if enter is pressed or if the user clicks outside the textfield
                if ((Event.current.isKey && Event.current.keyCode == KeyCode.Return)
                    || (Event.current.isMouse && Event.current.button == 0 &&
                        !position.Contains(Event.current.mousePosition)))
                {
                    isEditing = false;
                    //this repaints the whole unity executable, it's not optimal but it's the only way to make the textfield disappear immediately
                    HandleUtility.Repaint();
                }
            }
            else
            {
                if (GUI.Button(position, "", GUIStyle.none))
                {
                    isEditing = true;
                }

                Rect remaining = EditorGUI.PrefixLabel(position, label);
                GUIStyle style = new GUIStyle(EditorStyles.miniButtonLeft);
                style.alignment = TextAnchor.LowerLeft;
                EditorGUI.LabelField(remaining, new GUIContent(property.stringValue),style);
            }
        }
    }
}