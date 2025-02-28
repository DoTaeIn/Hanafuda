using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(EnumConditionAttribute))]
public class EnumConditionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnumConditionAttribute enumCondition = (EnumConditionAttribute)attribute;
        SerializedProperty enumField = property.serializedObject.FindProperty(enumCondition.EnumFieldName);

        if (enumField != null && enumCondition.EnumValues.Contains(enumField.enumValueIndex))
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        EnumConditionAttribute enumCondition = (EnumConditionAttribute)attribute;
        SerializedProperty enumField = property.serializedObject.FindProperty(enumCondition.EnumFieldName);

        return (enumField != null && enumCondition.EnumValues.Contains(enumField.enumValueIndex)) 
            ? EditorGUI.GetPropertyHeight(property, label) 
            : 0f;
    }
}