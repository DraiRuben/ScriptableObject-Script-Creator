using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ruben.SOCreator.Editor
{
    [CustomPropertyDrawer(typeof(TypeDropdownAttribute))]
    public class TypeDropdownDrawer : PropertyDrawer
    {
        //I hate the fact that I need to do that to be able to differentiate each property's instance
        //why are drawers used as if they were static classes? it's so dumb :[
        private Dictionary<string, TypeDropdownData> data = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String) return;
            Rect remainingPopup = EditorGUI.PrefixLabel(position, label);
            //identify field by property path, this also means that if the property is in a collection, reordering the collection will mean switching each field's dropdown data
            if(!data.ContainsKey(property.propertyPath)) data[property.propertyPath] = new TypeDropdownData();

            TypeDropdownData dropdownData = data[property.propertyPath];
            bool wasEditing = dropdownData.rawEdit;
            Color displayColor = dropdownData.rawEdit ? Color.blue : Color.white;
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = displayColor;
            if (GUI.Button(remainingPopup.SliceW(20, 0), "Raw Edit"))
            {
                dropdownData.rawEdit = !dropdownData.rawEdit;
            }

            GUI.backgroundColor = oldColor;
            if (dropdownData.rawEdit)
            {
                EditorGUI.PropertyField(remainingPopup.RemainderW(20), property, GUIContent.none);
            }
            else
            {
                if (wasEditing)
                {
                    Type parentType = ReflectionUtility.GetClassWithName(property.stringValue);
                    dropdownData.selected = 0;
                    if (parentType == null)
                    {
                        bool isKeyword = ReflectionUtility.IsVariableKeyword(property.stringValue);
                        dropdownData.typeNames = isKeyword ? new[] { property.stringValue } : new[] { "No type found" };
                        dropdownData.canSet = isKeyword;
                    }
                    else
                    {
                        dropdownData.typeNames = parentType.GetChildClasses().Select(x => x.Name).Prepend(property.stringValue).ToArray();
                        dropdownData.canSet = true;
                    }
                }
                if(!dropdownData.canSet) GUI.color = Color.red;
                dropdownData.selected = EditorGUI.Popup(remainingPopup.RemainderW(20), dropdownData.selected, dropdownData.typeNames.ToArray());
                if(dropdownData.canSet) property.stringValue = dropdownData.typeNames[dropdownData.selected];
                GUI.color = oldColor;
            }
        }
    }
    public class TypeDropdownData
    {
        public bool rawEdit = true;
        public string[] typeNames;
        public int selected;
        public bool canSet;
    }
}