namespace OpenCodeAnalysis;

public enum AuthorClass
{
    Bot,
    Top10,
    RepeatAuthor,
    FirstTimeAuthor,
}

public static class AuthorClasses
{
    public const string All = "ALL";

    public static string Label(this AuthorClass value) => value switch
    {
        AuthorClass.Bot => "bot",
        AuthorClass.Top10 => "top-10 activity",
        AuthorClass.RepeatAuthor => "repeat PR author",
        AuthorClass.FirstTimeAuthor => "first PR author",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    public static string Key(this AuthorClass value) => value.Label().Replace(' ', '_');

    public static string LabelOf(AuthorClass? value) => value?.Label() ?? All;

    public static string KeyOf(AuthorClass? value) => value?.Key() ?? All.ToLowerInvariant();
}
