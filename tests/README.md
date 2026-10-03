# Portable source regressions

## Scope
Each feed item now starts with empty content; a second same-feed completion is rejected after the await. Tests cover summary-only, content-only and absent-content items, out-of-order completions and an already loaded group.

## Run
From repository root, with .NET SDK 10.0.x:

```sh
dotnet run --project tests/Portable.Tests/Portable.Tests.csproj --configuration Release
```

No NuGet packages are restored: tests/NuGet.Config clears package sources. The SDK targets only this harness; original application project files, package references and framework targets are unchanged. C# 5 syntax is enforced. CI uses commit-pinned checkout/setup-dotnet, a read-only token, no persisted credentials and a ten-minute timeout. setup-dotnet installs only the official .NET 10 SDK for the isolated harness; it does not install legacy/native SDKs.

## What is (and is not) verified
The project links actual production source files directly; it does not copy their logic. WinRT syndication is represented by queued Task responses; HTML conversion is identity for plain-text fixtures; image/binding surfaces are minimal no-op adapters. This does not validate real feeds, HTML conversion, WinRT synchronization contexts, image decoding or maps.

The unchanged original source at `2d8597877cdb9388bfea638a40ecfc8638a31135` failed the new synthetic regression harness before the fix; the final source passes. Additional reversal tests independently fail for each corrected notification/null path and for the asynchronous duplicate guard where applicable.

## Dependency upgrade boundary
Windows Store XAML, Bing Maps/VCLibs SDKs, Coding4Fun, MVVM Light and WindowsMapsHelper require their original compatible build environment. No app package was upgraded without an original-target restore/build.

Microsoft documents [historical Windows/Phone toolchain requirements](https://learn.microsoft.com/en-us/visualstudio/releases/2022/port-migrate-and-upgrade-visual-studio-projects). [Newtonsoft.Json 13.0.4 package assets](https://www.nuget.org/packages/Newtonsoft.Json/13.0.4) must be evaluated against each original target; computed compatibility is not a native Phone assembly or an app validation. No full application build, native runtime/device test or exhaustive dependency vulnerability audit is claimed. The next decision is whether to recover the matching historical toolchain for compatible dependency work, or separately scope a supported-platform migration.
