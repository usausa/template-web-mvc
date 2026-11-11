# E2E テスト

E2E(`tests/Template.WebApp.E2ETests`)は Playwright で動かす。日常の実行には含めず、必要なときとリリース前に流す。コマンドはリポジトリ直下の PowerShell で実行する。

```powershell
# ブラウザの入手(初回。ビルドで出力されるスクリプトを使う)
dotnet build tests/Template.WebApp.E2ETests
pwsh tests/Template.WebApp.E2ETests/bin/Debug/net10.0/playwright.ps1 install

# 実行(既定は Chromium・画面なし)
dotnet run --project tests/Template.WebApp.E2ETests

# 画面を出して実行
$env:HEADED = "1"; dotnet run --project tests/Template.WebApp.E2ETests; Remove-Item Env:HEADED

# ブラウザを切り替える(chromium / firefox / webkit)
$env:BROWSER = "firefox"; dotnet run --project tests/Template.WebApp.E2ETests; Remove-Item Env:BROWSER

# Playwright Inspector で 1 手ずつ実行
$env:PWDEBUG = "1"; dotnet run --project tests/Template.WebApp.E2ETests; Remove-Item Env:PWDEBUG
```

- 失敗したテストだけ、操作ごとの画面・DOM・通信・コンソールを記録したトレースを `tests/Template.WebApp.E2ETests/bin/Debug/net10.0/playwright-traces/<テストの表示名>.zip` に残す。`pwsh tests/Template.WebApp.E2ETests/bin/Debug/net10.0/playwright.ps1 show-trace <zip>` か https://trace.playwright.dev で開く
- パスキーのテスト(`PasskeyTests`)は CDP の仮想認証器を使うので、Chromium でしか通らない(firefox / webkit では失敗する)
