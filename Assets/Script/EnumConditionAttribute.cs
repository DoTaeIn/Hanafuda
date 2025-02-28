using UnityEngine;

public class EnumConditionAttribute : PropertyAttribute
{
    public string EnumFieldName;
    public int[] EnumValues; // Accept multiple values

    public EnumConditionAttribute(string enumFieldName, params int[] enumValues)
    {
        this.EnumFieldName = enumFieldName;
        this.EnumValues = enumValues;
    }
}