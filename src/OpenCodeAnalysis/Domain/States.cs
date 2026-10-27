namespace OpenCodeAnalysis;

/// State shared by pull requests and issues.
public static class ItemState
{
    public const string Open = "open";
    public const string Closed = "closed";
}

public static class ReviewState
{
    public const string Approved = "approved";
    public const string Commented = "commented";
    public const string ChangesRequested = "changes_requested";

    public static bool IsApproved(string? state) =>
        string.Equals(state, Approved, StringComparison.OrdinalIgnoreCase);
}
