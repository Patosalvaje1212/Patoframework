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
public class InspectorShowOrderAttribute : Attribute
{
    public int order;
    public InspectorShowOrderAttribute(int Order)
    {
        order = Order;
    }
}
