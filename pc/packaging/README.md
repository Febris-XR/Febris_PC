# PC suite packaging

`build-msi.py` is the recipe for `FebrisPCSuite-0.2.0-win-x64.msi`. The v0.2.0 installer was
built by hand with the steps living in a temporary directory, which meant a published artifact
nobody could reproduce.

```
dotnet tool install --global wix --version 5.0.2
python pc/packaging/build-msi.py
```

## Use WiX 5, not WiX 7

WiX 7 refuses to build until you accept the Open Source Maintenance Fee EULA, which is a
licensing commitment rather than a build step. WiX 5.0.2 carries no such requirement. Pin the
version when installing the tool.

## Why WiX and not a setup project

The suite used to carry a Visual Studio setup project. It was broken as committed, because its
payload SourcePaths pointed into `obj\Release\netcoreapp3.1\` from before the move to
net8.0-windows and three icon entries hardcoded one developer's user profile. Visual Studio 2022
also cannot build that project type without an extension. It is not part of this repository, and
WiX supersedes it rather than merely deferring it. WiX is a dotnet tool, so a build box needs
nothing beyond the SDK.

## Why VS MSBuild and not dotnet build

`Febris.PCModuleManagerV3` carries a COMReference and `dotnet build` dies MSB4803 on it, so the
script drives the five solutions through the Visual Studio MSBuild instead.

You do not need to edit anything to point it at your install. `find_msbuild()` checks
`MSBUILD_PATH` first, then asks vswhere, then looks on PATH, then sweeps the 2022 editions. That
ordering is what lets one script serve a Community developer box and a CI runner carrying
Enterprise. Set `MSBUILD_PATH` if you have a side-by-side install and want a specific one.

## Building it in CI

`.github/workflows/build-msi.yml` runs this same script on `windows-latest`, so there is no
second recipe to keep in step. It is `workflow_dispatch` only, because compiling five solutions
is the expensive job in this repository and the artifact is only wanted at release. Pull requests
that touch this directory get a syntax check instead.

The run prints the MSI filename, size and sha256 to the job summary already shaped as the
`contains[]` entry the distribution feed records, so publishing a release is a copy rather than a
re-derivation.

Supply `ffmpeg_url`, `ffmpeg_sha256` and `ffmpeg_source_url` for a release build. Leaving them
empty still produces a valid installer, but its screen recorder cannot encode, and the run says
so in the summary rather than letting it pass silently.

## The ffmpeg double-copy trap

The first MSI came out at 74 MB instead of 40 MB because ffmpeg was staged twice at 102 MB each.
The ScreenRecorder csproj already copies `pc/tools/ffmpeg` beside its executable, which is where
`ResolveFfmpegPath` looks first, so a second copy under `tools/ffmpeg/` is pure duplication.
Stage from the build output only, which is what `stage()` does. The script warns if no
`ffmpeg.exe` ends up beside the executables.

## The UpgradeCode

`UPGRADE_CODE` identifies the PRODUCT across every version it will ever have. Changing it stops
Windows treating a new MSI as an upgrade of the old one, which leaves two installs side by side.
It is derived with `uuid5` from a fixed name so it stays reproducible instead of being a value
someone has to remember to copy forward.

## Two recorded limits

The MSI is not code signed, so SmartScreen warns on first run. The applications are
framework-dependent, so a target machine needs the .NET 8 Desktop Runtime. Bundling the runtime
would need a Burn bundle and roughly triples the download. Both states are deliberate rather
than oversights.

## Verifying without installing

```
msiexec /a "pc\packaging\out\FebrisPCSuite-0.2.0-win-x64.msi" /qn TARGETDIR=C:\temp\msicheck
```

An administrative install unpacks the payload without touching the system. v0.2.0 extracts 107
files with all five executables and ffmpeg present.

## The full release pipeline

This file covers building the artifact. Publishing it is a separate step in a separate
repository.

The MSI is attached to a GitHub Release here, and the row that describes it lives in the
distribution feed at [Febris-XR/Febris_ClientDist](https://github.com/Febris-XR/Febris_ClientDist).
That feed is an index rather than a store, so its row points back at the Release asset and records
the filename, size and sha256 of the installer itself. Those are the three values the MSI job
prints to its summary.

The website reads that feed and regenerates itself, so no download link, version or checksum is
maintained by hand anywhere.
