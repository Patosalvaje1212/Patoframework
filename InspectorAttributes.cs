[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class InspectorHideAttribute : Attribute
{}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorNonEditableAttribute : Attribute
{}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorHideNull : Attribute
{}
