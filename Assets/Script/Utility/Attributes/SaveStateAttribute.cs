using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SaveStateAttribute : Attribute
{
    // Optional unique key override if you want to decouple from field name
    public string Key { get; }

    public SaveStateAttribute(string key = null)
    {
        Key = key;
    }
}