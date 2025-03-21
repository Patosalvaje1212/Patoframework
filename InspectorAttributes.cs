
namespace PatoframeWork;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class InspectorHideAttribute : Attribute
{}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorNonEditableAttribute : Attribute
{}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorHideNullAttribute : Attribute
{}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorShowOrderAttribute(int Order) : Attribute
{
    public int order = Order;
}
