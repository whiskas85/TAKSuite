using BlazorReflection.Data;

namespace BlazorReflection.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ShowAttribute : Attribute
    {
        public ShowAttribute(bool isShown)
        {
            Show = isShown;
        }

        public bool Show { get; private set; } = true;
    }
}
