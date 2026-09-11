namespace Template.WebApp.Host.Application;

public static class Messages
{
    // Validation

    public const string Required = "入力してください";

    public const string MaxLength = "{1}文字以内で入力してください";

    public const string Range = "{1}~{2}の範囲で入力してください";

    public static string MakeInvalid(string item) => $"{item}の形式が不正です";

    // Account

    public const string LoginFailed = "ログインに失敗しました";

    public const string PasswordMismatch = "パスワードが一致しません";

    public const string PasskeyFailed = "パスキー操作に失敗しました";

    public const string LockedOut = "ログインに連続して失敗したため、しばらく待ってから再試行してください";

    // Data

    public const string DuplicateName = "同じ名前のデータが存在します";
}
