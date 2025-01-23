using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ruben.SOCreator.Editor
{
    public class ScriptableObjectCreatorWindow : EditorWindow
    {
        [LabelToTextField] public string SoName = "PlaceHolder";
        
        //I wanted to do a non reorderable list attribute to prevent type dropdown from breaking
        //but unity applies attributes to each element in an array instead of the whole array, super dumb
        //because it prevents me from drawing a list as I want, this changed in Unity 6 but I'm not using unity 6 so ¯\_(ツ)_/¯
        //I could have made a custom wrapper such as a SerializableHashSet, also solving the problem of duplicate variable names,
        //but that would have been too much work for such a small project
        public List<DeclaredField> VariablesToDeclare = new();
        public List<DeclaredMethod> MethodsToDeclare = new();
        
        [MenuItem("Window/ScriptableObject Creator")]
        public static void CreateWindow()
        {
            GetWindow<ScriptableObjectCreatorWindow>();
        }

        private void OnGUI()
        {
            SerializedObject serializedObject = new SerializedObject(this);

            //display so name, then the list of all fields to declare, then a warning about reordering the list and the save button
            Rect displayRect = Rect.zero;
            displayRect.height = EditorGUIUtility.singleLineHeight;
            displayRect.width = position.width;

            
            Rect remainingRect = EditorGUI.PrefixLabel(displayRect, new GUIContent("Name of the SO to create"));
            EditorGUI.PropertyField(remainingRect, serializedObject.FindProperty(nameof(SoName)), GUIContent.none);

            SerializedProperty variables = serializedObject.FindProperty(nameof(VariablesToDeclare));
            displayRect.y += EditorGUIUtility.singleLineHeight;
            float height = EditorGUI.GetPropertyHeight(variables);
            displayRect.height = height;
            EditorGUI.PropertyField(displayRect,variables);
            
            SerializedProperty methods = serializedObject.FindProperty(nameof(MethodsToDeclare));
            displayRect.y += displayRect.height;
            height = EditorGUI.GetPropertyHeight(methods);
            displayRect.height = height;
            EditorGUI.PropertyField(displayRect, methods);
            serializedObject.ApplyModifiedProperties();


            Rect warningRect = new Rect(0, position.height - EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.HelpBox(warningRect,"Avoid Reordering the list, it will break the script generation", MessageType.Warning);
            
            Rect saveRect = new Rect(position.width, position.height, 50, EditorGUIUtility.singleLineHeight);
            saveRect.x -= saveRect.width;
            saveRect.y -= saveRect.height;
            if (GUI.Button(saveRect, "Save"))
            {
                SaveNewScriptableObject();
            }
        }
        private void SaveNewScriptableObject()
        {
            string path = EditorUtility.SaveFilePanel("Save ScriptableObject", Application.dataPath, SoName, "cs");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            FileStream file = File.Create(path);
            StreamWriter writer = new StreamWriter(file);
            writer.Write(WriteScriptableObjectContent());
            writer.Close();
            file.Close();
            AssetDatabase.Refresh();
        }
        private string WriteScriptableObjectContent()
        {
            //it works, however it doesn't handle adding usings for classes that are in other namespaces
            //it doesn't handle properties and their accessor's visibility
            //it doesn't handle default value declaration nor function declarations or generic<T> types
            string content = "using UnityEngine;\n\n";
            content += $"public class {SoName} : ScriptableObject\n";
            content += "{\n";
            foreach (DeclaredField variable in VariablesToDeclare)
            {
                if (!IsDeclaredVariableValid(variable)) continue;
                
                // \t is a tab
                content += $"\t{GetFieldDeclaration(variable)}\n";
            }

            foreach (DeclaredMethod method in MethodsToDeclare)
            {
                if(!IsDeclaredMethodValid(method)) continue;
                content += $"\t{GetMethodDeclaration(method)}\n";
            }
            content += "}\n";
            return content;
        }
        #region FieldGeneration
        private string GetFieldDeclaration(DeclaredField field)
        {
            return $"{field.Visibility.ToString().ToLower()} {field.DeclaredTypeName} {GetVariableName(field.FieldName)};";
        }
        private static bool IsDeclaredVariableValid(DeclaredField field)
        {
            return !string.IsNullOrEmpty(field.FieldName) 
                   && (ReflectionUtility.GetClassWithName(field.DeclaredTypeName) != null || ReflectionUtility.IsVariableKeyword(field.DeclaredTypeName));
        }
        private static string GetVariableName(string name)
        {
            //remove useless spaces before and after variable name, and replace spaces with underscores since you can't have that in a variable name
            return name.Trim().Replace(" ", "_").TrimStart('0','1','2','3','4','5','6','7','8','9').Trim('&','*','+','-','/','<','>','=','\'','\\','@','`','^','!','?','.',',',';',':','|','{','}','[',']','(',')','"');
            //there is no handling for duplicate variable names, but I'm too lazy for that, the optimal solution would be to tell the user that the variable is duplicate
            //in the window with a yellow background for both elements, but this would be wayyyyy too long, I'd need a wrapper for the list, then a drawer for the wrapper
            //then draw the list manually, then check for duplicates which is hell to do with serialized properties and not values,
            //otherwise do the duplicate check in the wrapper and store the results in some collection which is then read by the drawer in a serialized property
        }
        #endregion
        
        #region MethodGeneration
        private string GetMethodDeclaration(DeclaredMethod method)
        {
            List<Parameter> orderedParams = method.Parameters.OrderBy(x => x.Keyword).ToList();
            string parameters = string.Join(", ", orderedParams.ConvertAll(GetParameterDeclaration));
            return $"{method.Visibility.ToString().ToLower()} {method.ReturnType} {method.MethodName}({parameters}){{ }}";
        }
        private string GetParameterDeclaration(Parameter parameter)
        {
            if (parameter.Keyword == EParameterKeyword.Params)
            {
                return $"{parameter.Keyword.ToString().ToLower()} {parameter.DeclaredTypeName}[] {parameter.ParameterName}";
            }
            if (parameter.Keyword == EParameterKeyword.None)
            {
                return $"{parameter.DeclaredTypeName} {parameter.ParameterName}";
            }
            return $"{parameter.Keyword.ToString().ToLower()} {parameter.DeclaredTypeName} {parameter.ParameterName}";
        }
        private static bool IsDeclaredMethodValid(DeclaredMethod method)
        {
            return !string.IsNullOrEmpty(method.MethodName) 
                   && (ReflectionUtility.GetClassWithName(method.ReturnType) != null || ReflectionUtility.IsVariableKeyword(method.ReturnType));
        }
        #endregion
    }
    [Serializable]
    public struct DeclaredField
    {
        public string FieldName;
        [TypeDropdown] public string DeclaredTypeName;
        public EVariableVisibility Visibility;
    }

    [Serializable]
    public struct DeclaredMethod
    {
        public string MethodName;
        [TypeDropdown] public string ReturnType;
        public List<Parameter> Parameters;
        public EVariableVisibility Visibility;
    }

    [Serializable]
    public struct Parameter
    {
        public string ParameterName;
        [TypeDropdown] public string DeclaredTypeName;
        public EParameterKeyword Keyword;
    }

    public enum EParameterKeyword
    {
        None,
        In,
        Out,
        Ref,
        Params
    }
    

    public enum EVariableVisibility
    {
        Public,
        Private,
        Protected
    }
}