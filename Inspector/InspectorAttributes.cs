
namespace PatoFramework.Inspector;

/// <summary>
/// Prevents the Inspector from exposing this variable
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class InspectorHideAttribute : Attribute
{}


/// <summary>
/// Prevents this variable from being edited in the inspector ( readonly )
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorNonEditableAttribute : Attribute
{}

/// <summary>
/// Hides this property if its value is null
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorHideNullAttribute : Attribute
{}

/// <summary>
/// Forces the Inspector to draw this property in an specific order
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorShowOrderAttribute(int Order) : Attribute
{
    public int order = Order;
}


/// <summary>
/// Makes this property be able to receive Data of type DataReceiver by dropping it in the Inspector
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorReceiveDropAttribute(string DataReceiver) : Attribute
{
    public string receiveData = DataReceiver;
}