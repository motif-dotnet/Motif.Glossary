namespace Motif.Glossary.Adapters;

/// <summary>
/// A drop of files onto the page, numbered from 1 in the order they came.
/// </summary>
public sealed record Drop(int Number, IReadOnlyList<LocalFile> Files);
