
using System.Reflection.Metadata.Ecma335;

internal class FormControlDropDownTypeAttribute : Attribute
{
    public Type Type { get; set; }
    public FormControlDropDownTypeAttribute(Type type)
    {
        Type = type;
    }
}