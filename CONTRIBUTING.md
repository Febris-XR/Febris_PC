# Contributing

Thanks for looking. Two things about this repository are unusual enough to state before anything
else: the build needs Visual Studio rather than the `dotnet` CLI alone, and there are no tests
here at all. Both are explained below rather than left for you to discover.

## What you need

**Windows, and Visual Studio 2022.** Four of the five projects target `net8.0-windows`, and
`Febris.PCModuleManagerV3` carries a COM reference to the Windows Script Host, which it uses to
create shortcuts. The .NET Core version of MSBuild cannot resolve a COM reference, so `dotnet
build` on that project fails with:

```
error MSB4803: The task "ResolveComReference" is not supported on the .NET Core version of MSBuild.
```

That is expected, not a broken checkout. Use the Visual Studio MSBuild, or open
`Febris.PCLauncherV3.sln` and build from the IDE.

**Three of the five projects do build with the `dotnet` CLI**, which is enough for most changes:

```bash
dotnet build pc/FebrisPCProgressBar/Febris.ConsoleProgressBar.csproj      -c Release
dotnet build pc/FebrisPCScreenRecorder/Febris.PCScreenRecorderV3.csproj   -c Release
dotnet build pc/FebrisPCStatementManager/Febris.PCStatementManagerV3.csproj -c Release
```

The launcher is not one of them, and the reason is worth knowing so you do not go looking for a
second COM reference: the launcher has none. It fails because it references the module manager,
and the failure is reported against the module manager's project file.

## Before running the screen recorder

Fetch ffmpeg first. The recorder shells out to it and will not find it otherwise:

```powershell
pc/tools/fetch-ffmpeg.ps1
```

The binary is deliberately not committed. The script records provenance beside it, and that record
is what makes redistribution defensible.

## There are no tests in this repository, and that is a gap rather than a decision

The xAPI and shared-library test suites live with the code they test, which is not here. Nothing
in this repository is covered by an automated test, and the five applications have never been run
end to end as a suite. Compilation is the only evidence that currently exists.

So please be specific about verification. "It builds" is a real statement and a small one. If you
ran the launcher against a node, say which node and what you did. A pull request that adds a test
project is welcome and would be the single most valuable contribution here.

## Never add a fallback host

The background services ship with `ApiUrlPath:DataApi` as an unsubstituted placeholder and refuse
to start without it. That is deliberate and it is not a rough edge to smooth over.

A client that has not been told which node it belongs to must fail loudly rather than guess, and
it must never fall back to a host the project maintainer operates. An installed client silently
talking to somebody else's server is a much worse outcome than a service that will not start. If a
default would be convenient for your workflow, set the environment variable.

## Style

- One logical change per pull request.
- Match the surrounding file. The codebase is not uniform and there is no formatter gate.
- New first-party source files carry the two-line SPDX header the existing files carry.
- Keep configuration in configuration. Hardcoded hosts and paths are how this suite acquired the
  problems listed at the end of the [README](README.md), and two of them are still there.

## Reporting a security issue

Do not open a public issue. See [SECURITY.md](SECURITY.md) for the private reporting channel.

## Licence

By contributing you agree that your contributions are licensed under AGPL-3.0-only, the same
licence as the project. See [LICENSE](LICENSE).
