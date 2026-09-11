# template-web-mvc

ASP.NET Core MVC のテンプレート。データアクセスは Smart.Data.Accessor(SQLite)、認証は ASP.NET Core Identity Core を **EF Core なし**で組み込んでいる。

## 認証

- 既定(`Auth:Enabled=false`)では認証を要求しない。`[Authorize]` と `Administrator` ポリシーはすべて素通しになり、ログインなしで全機能を使える
- `Auth:Enabled=true` にすると Cookie 認証が有効になる。ログイン(パスワード / パスキー)・新規登録・パスキー管理・ロックアウトが動く
- 認証の流れ(Cookie / パスキー / ロックアウト)は Identity の `SignInManager` / `UserManager` に任せ、利用者の保存だけを `AccountStore`(Smart.Data.Accessor)で行う。EF Core は使わない

| 画面 | パス | 備考 |
|---|---|---|
| ログイン | `/account/login` | パスワード、または「パスキーでログイン」(条件付き UI の自動フィルにも対応) |
| 新規登録 | `/account/register` | 作成されたアカウントのロールは `User` |
| パスキー管理 | `/account/passkeys` | 登録(上限 5 件)・名前変更・削除。ログイン後のメニューから開く |

### 構成

| 要素 | 場所 | 役割 |
|---|---|---|
| `AccountEntity` / `AccountPasskeyEntity` | `Template.WebApp.Core/Models/Entity` | 利用者とパスキー。`Database.sql` のテーブルに対応 |
| `AccountAccessor` / `AccountPasskeyAccessor` | `Template.WebApp.Core/Accessors` | Smart.Data.Accessor による永続化 |
| `AccountStore` | `Template.WebApp.Host/Infrastructure/Identity` | `IUserStore` / `IUserPasswordStore` / `IUserSecurityStampStore` / `IUserLockoutStore` / `IUserPasskeyStore` の実装 |
| `AccountPasswordHasher` | 同上 | Identity のハッシュ処理を既存の `IPasswordProvider`(PBKDF2)へ委譲 |
| `AccountClaimsPrincipalFactory` | 同上 | Cookie に載せるクレームの組み立て。ロールや業務クレームをカスタムする入口 |
| `PasskeyEndpoints` | `Template.WebApp.Host/Application` | WebAuthn のオプション生成 API(`/account/passkey/creation-options`、`/account/passkey/request-options`) |
| `AccountController` | `Areas/Default/Controllers` | ログイン・登録・パスキー管理の画面 |

### ロールとクレイム

- ロールは `Account.Role` 列を正とする(Identity の `RoleManager` は使わない)。`Roles` / `Policies` に定数があり、`Administrator` ポリシーは削除操作に使っている
- 業務固有のクレーム(部署や権限フラグなど)は `AccountClaimsPrincipalFactory.CreateAsync` で `Account` の列や別テーブルから読んで足す
- 初期ユーザーは `Auth:InitialId` / `Auth:InitialPassword`(admin / admin、`Administrator`)。起動時に `AccountService.InitializeAsync` が作成する

## メモ

- パスワードポリシーは開発用初期パスワード(admin)を許容する緩和設定になっている。運用時は `ConfigureAuthentication` の `options.Password` を強化すること
- ロックアウトの回数と期間は `IdentityOptions.Lockout`(既定 5 回 / 5 分)で調整する
- パスキーは localhost 以外では HTTPS が必要(WebAuthn の RP ID に IP アドレスは使用不可)
- テストは認証オン(`TestApplicationFactory` / `E2EApplicationFactory`)を前提にし、`AuthDisabledTests` で認証オフの状態を別途検証している。E2E の `PasskeyTests` は Playwright の CDP WebAuthn 仮想認証器でパスキーの登録〜ログインを検証する
- MFA(TOTP + リカバリーコード)は未実装
