namespace Template.WebApp.Host.Application;

public static class Messages
{
    // Validation

    public const string Required = "入力してください";

    public const string MaxLength = "{1}文字以内で入力してください";

    public const string Range = "{1}~{2}の範囲で入力してください";

    public const string Empty = "入力しないでください";

    public const string EmailAddress = "メールアドレスの形式で入力してください";

    public const string AnyRequired = "いずれかを入力してください";

    public const string ByteMaxLength = "{1}バイト以内で入力してください";

    public const string ByteLength = "{1}~{2}バイトで入力してください";

    public const string ByteFixedLength = "{1}バイトで入力してください";

    public const string Ms932Encodable = "使用できない文字が含まれています";

    public const string NoHalfWidthKana = "半角カナ以外で入力してください";

    public const string FullWidth = "全角で入力してください";

    public const string UpperAlphanumeric = "半角英大文字と数字で入力してください";

    public const string UpperAlphanumericHyphen = "半角英大文字と数字と-で入力してください";

    public const string Alphanumeric = "半角英数字で入力してください";

    public const string AlphanumericHyphen = "半角英数字と-で入力してください";

    public const string Numeric = "半角数字で入力してください";

    public const string NumericHyphen = "半角数字と-で入力してください";

    public const string Katakana = "全角カタカナで入力してください";

    public const string Hiragana = "ひらがなで入力してください";

    public const string Compare = "大小関係が正しくありません";

    public const string EqualTo = "一致しません";

    public const string ElementRequired = "要素を入力してください";

    public const string MaxCount = "{1}件以内で入力してください";

    public const string DuplicateKey = "重複しています";

    public const string InvalidFormat = "形式が不正です";

    public const string Number = "数値で入力してください";

    public const string RequestBodyRequired = "本文を入力してください";

    public const string AttemptedValueIsInvalid = "{1}の形式が不正です";

    public const string UnknownValueIsInvalid = "{0}の形式が不正です";

    // Account

    public const string LoginFailed = "ログインに失敗しました";

    public const string PasswordMismatch = "パスワードが一致しません";

    public const string PasskeyFailed = "パスキー操作に失敗しました";

    public const string LockedOut = "ログインに連続して失敗したため、しばらく待ってから再試行してください";

    // Data

    public const string DuplicateName = "同じ名前のデータが存在します";
}
