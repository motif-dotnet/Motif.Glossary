namespace Motif.Glossary;

/// <summary>Where a term sits: one of the seven kinds. The review groups terms by kind, in this order.</summary>
public enum Kind
{
    System,
    Actor,
    ExternalSystem,
    Storage,
    Port,
    Adapter,
    DomainConcept,
}

public static class Kinds
{
    /// <summary>The seven kinds, in the order the review shows them.</summary>
    public static readonly IReadOnlyList<Kind> All = Enum.GetValues<Kind>();

    /// <summary>A kind as the glossary writes it, such as "external system".</summary>
    public static string Name(this Kind kind) => kind switch
    {
        Kind.System => "system",
        Kind.Actor => "actor",
        Kind.ExternalSystem => "external system",
        Kind.Storage => "storage",
        Kind.Port => "port",
        Kind.Adapter => "adapter",
        Kind.DomainConcept => "domain concept",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    /// <summary>The plural heading the review uses for a group of terms of this kind.</summary>
    public static string GroupName(this Kind kind) => kind switch
    {
        Kind.System => "System",
        Kind.Actor => "Actors",
        Kind.ExternalSystem => "External systems",
        Kind.Storage => "Storage",
        Kind.Port => "Ports",
        Kind.Adapter => "Adapters",
        Kind.DomainConcept => "Domain concepts",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    public static bool TryRead(string text, out Kind kind)
    {
        foreach (var k in All)
        {
            if (string.Equals(k.Name(), text, StringComparison.OrdinalIgnoreCase))
            {
                kind = k;
                return true;
            }
        }
        kind = default;
        return false;
    }
}
