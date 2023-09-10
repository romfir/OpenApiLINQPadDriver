namespace OpenApiLINQPadDriver;
internal sealed class TypeDescriptor
{
    public string Name { get; }
    public string NameSpace { get; }

    public TypeDescriptor(string name, string nameSpace)
    {
        Name = name;
        NameSpace = nameSpace;
    }

    public override string ToString() => NameSpace + "." + Name;
}
