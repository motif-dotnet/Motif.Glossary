namespace Motif.Glossary;

/// <summary>What the Reviewer does: the four ways in, and the "Proposed only" filter.</summary>
public interface UserTasks
{
    /// <summary>Reviews the Example: Motif Sketch's glossary with its sketch file.</summary>
    void OpenExample();

    /// <summary>Reviews pasted Markdown.</summary>
    void Paste(string markdown);

    /// <summary>Shows the file picker and reviews the glossary.md picked, with a sketch file if one was picked too.</summary>
    Task OpenFiles();

    /// <summary>Reviews the files just dropped onto the page. A sketch file dropped on its own joins the glossary under review.</summary>
    Task OpenDroppedFiles();

    /// <summary>Reviews a public repo's glossary. Takes owner/name or a github.com URL.</summary>
    Task OpenRepo(string repo);

    /// <summary>Opens a repo link's fragment (#repo=owner/name). False if the fragment isn't one.</summary>
    Task<bool> OpenLink(string? fragment);

    void ShowProposedOnly(bool proposedOnly);

    void DismissProblem();
}
