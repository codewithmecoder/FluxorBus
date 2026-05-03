namespace FluxorBus.SourceGen;

/// <summary>
/// Specifies that a class is intended to handle messages within a messaging framework.
/// </summary>
/// <remarks>Apply this attribute to a class to indicate that it should be discovered and used as a message
/// handler. This attribute is typically used by frameworks that scan for handler types at runtime. Only classes can be
/// marked with this attribute.</remarks>
[AttributeUsage(AttributeTargets.Class)]
public class MessageHandlerAttribute : Attribute { }