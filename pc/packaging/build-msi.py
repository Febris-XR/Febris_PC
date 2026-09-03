#!/usr/bin/env python3
"""Build the Febris PC Suite MSI.

    python pc/packaging/build-msi.py --version 0.2.0 [--out DIR]

WHY THIS EXISTS. The v0.2.0 installer was built by hand and the recipe lived in a temporary
directory, which meant a published artifact nobody could reproduce. This is that recipe.

WHY WiX AND NOT A .vdproj. The suite used to carry a Visual Studio setup project. It was broken
as committed, because its payload SourcePaths pointed into obj\\Release\\netcoreapp3.1\\ from
before the move to net8.0-windows and three icon entries hardcoded one developer's user profile.
Visual Studio 2022 also cannot build that project type without an extension. It is not part of
this repository. WiX is a dotnet tool, so a build box needs nothing beyond the SDK.

RUNS LOCALLY AND IN CI. .github/workflows/build-msi.yml runs exactly this script on
windows-latest. Nothing here is runner-specific, and MSBuild is located rather than hardcoded,
so the two paths cannot drift apart.

USE WiX 5, NOT 7. WiX 7 refuses to build until you accept the Open Source Maintenance Fee
EULA, which is a licensing commitment rather than a build step.

    dotnet tool install --global wix --version 5.0.2

NOT CODE SIGNED, and the applications are framework-dependent, so a target needs the .NET 8
Desktop Runtime. Both are deliberate, recorded states rather than oversights.
"""

import argparse
import hashlib
import os
import shutil
import subprocess
import sys
import uuid

HERE = os.path.dirname(os.path.abspath(__file__))
PC = os.path.dirname(HERE)
ROOT = os.path.dirname(PC)

# MSBuild is LOCATED, not hardcoded. Febris.PCModuleManagerV3 carries a COMReference and
# `dotnet build` dies MSB4803 on it, so this has to be the Visual Studio MSBuild rather than the
# one shipped with the .NET SDK. Where that lives is not fixed: a GitHub-hosted windows runner
# ships Enterprise, a developer box is usually Community, and a build server often carries only
# BuildTools. Hardcoding one edition is what kept this script local-only.
#
# Order is deliberate. MSBUILD_PATH is the explicit override and always wins. vswhere is how
# Visual Studio itself answers this question, so it is correct across editions and side-by-side
# installs. PATH covers the setup-msbuild action. The edition sweep is a last resort.
def find_msbuild():
    override = os.environ.get("MSBUILD_PATH")
    if override:
        if not os.path.exists(override):
            sys.exit("MSBUILD_PATH is set to %s but nothing is there." % override)
        return override

    vswhere = os.path.join(
        os.environ.get("ProgramFiles(x86)", r"C:\Program Files (x86)"),
        "Microsoft Visual Studio", "Installer", "vswhere.exe")
    if os.path.exists(vswhere):
        try:
            found = subprocess.check_output(
                [vswhere, "-latest", "-products", "*",
                 "-requires", "Microsoft.Component.MSBuild",
                 "-find", r"MSBuild\**\Bin\MSBuild.exe"],
                universal_newlines=True, stderr=subprocess.PIPE).strip().splitlines()
            for line in found:
                if line.strip() and os.path.exists(line.strip()):
                    return line.strip()
        except Exception:
            pass

    on_path = shutil.which("msbuild")
    if on_path:
        return on_path

    for edition in ("Enterprise", "Professional", "Community", "BuildTools"):
        guess = os.path.join(
            r"C:\Program Files\Microsoft Visual Studio\2022", edition,
            "MSBuild", "Current", "Bin", "MSBuild.exe")
        if os.path.exists(guess):
            return guess
    return None

# Febris.PCModuleManagerV3 carries a COMReference, and `dotnet build` dies MSB4803 on it, so
# VS MSBuild is the only route for the suite. Solutions rather than projects, because each
# carries its own.
SOLUTIONS = [
    "FebrisPCLauncher/Febris.PCLauncherV3.sln",
    "FebrisPCModuleManager/Febris.PCModuleManagerV3.sln",
    "FebrisPCScreenRecorder/Febris.PCScreenRecorderV3.sln",
    "FebrisPCStatementManager/Febris.PCStatementManagerV3.sln",
    "FebrisPCProgressBar/Febris.ConsoleProgressBar.sln",
]

EXES = [
    "FebrisPCLauncher/Febris.PCLauncherV3",
    "FebrisPCModuleManager/Febris.PCModuleManagerV3",
    "FebrisPCScreenRecorder/Febris.PCScreenRecorderV3",
    "FebrisPCStatementManager/Febris.PCStatementManagerV3",
    "FebrisPCProgressBar/Febris.ConsoleProgressBar",
]

# Identifies the PRODUCT across every version it will ever have. Changing it stops Windows
# treating a new MSI as an upgrade of the old one, leaving two installs side by side. Derived
# from a fixed name so it is reproducible instead of a value someone must remember to copy.
UPGRADE_CODE = str(uuid.uuid5(uuid.NAMESPACE_DNS, "febris-pc-suite.upgradecode.febr.is")).upper()


def run(cmd, **kw):
    print("  $ %s" % (cmd if isinstance(cmd, str) else " ".join(cmd)))
    r = subprocess.run(cmd, **kw)
    if r.returncode != 0:
        sys.exit("FAILED: exit %d" % r.returncode)


def ident(s):
    return ("f_" + "".join(c if (c.isalnum() or c == "_") else "_" for c in s))[:70]


def build(config):
    msbuild = find_msbuild()
    if not msbuild:
        sys.exit("MSBuild not found. Install VS2022 with the Microsoft.Component.MSBuild "
                 "component, or set MSBUILD_PATH to MSBuild.exe.")
    print("   using %s" % msbuild)
    for sln in SOLUTIONS:
        run([msbuild, os.path.join(PC, sln.replace("/", os.sep)),
             "-v:q", "-nologo", "-p:Configuration=" + config, "-restore"])


def stage(config, dest):
    """Copy each project's build output into one flat payload.

    ffmpeg is NOT copied separately. The ScreenRecorder csproj already copies pc/tools/ffmpeg
    beside its executable, which is where ResolveFfmpegPath looks first. Adding a second copy
    under tools/ffmpeg produced a 74 MB MSI instead of 40 MB, because the 102 MB binary went in
    twice.
    """
    if os.path.isdir(dest):
        shutil.rmtree(dest)
    os.makedirs(dest)
    for rel in EXES:
        d, n = rel.split("/")
        base = os.path.join(PC, d, "bin", config)
        found = None
        for root, _, files in os.walk(base):
            if n + ".exe" in files:
                found = root
                break
        if not found:
            sys.exit("no %s.exe under %s -- did the build succeed?" % (n, base))
        for item in os.listdir(found):
            src, dst = os.path.join(found, item), os.path.join(dest, item)
            if os.path.isdir(src):
                if not os.path.exists(dst):
                    shutil.copytree(src, dst)
            elif not os.path.exists(dst):
                shutil.copy2(src, dst)
    n = sum(len(f) for _, _, f in os.walk(dest))
    print("  staged %d files" % n)
    if not os.path.exists(os.path.join(dest, "ffmpeg.exe")):
        print("  WARNING: no ffmpeg.exe beside the executables. Run pc/tools/fetch-ffmpeg.ps1")
    return n


def write_wxs(stage_dir, out, version):
    dirs, comps = {}, []
    for root, _, files in os.walk(stage_dir):
        rel = os.path.relpath(root, stage_dir).replace("\\", "/")
        dirs[rel] = "INSTALLFOLDER" if rel == "." else ident("d_" + rel)
        for f in sorted(files):
            key = (rel + "/" + f).lstrip("./")
            comps.append((ident("c_" + key), dirs[rel], os.path.join(root, f), ident("fi_" + key)))

    def tree(prefix, indent):
        out_ = []
        for c in sorted(d for d in dirs if d != "." and
                        os.path.dirname(d).replace("\\", "/") == (prefix if prefix != "." else "")):
            out_.append('%s<Directory Id="%s" Name="%s">' % (indent, dirs[c], os.path.basename(c)))
            out_.append(tree(c, indent + "  "))
            out_.append("%s</Directory>" % indent)
        return "\n".join(x for x in out_ if x)

    body = "\n".join(
        '      <Component Id="%s" Directory="%s" Guid="*">\n'
        '        <File Id="%s" Source="%s" KeyPath="yes" />\n'
        '      </Component>' % (c, d, f, p.replace("&", "&amp;"))
        for c, d, p, f in comps)

    xml = '''<?xml version="1.0" encoding="utf-8"?>
<!-- GENERATED by pc/packaging/build-msi.py. Do not hand-edit. -->
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <Package Name="Febris PC Suite" Manufacturer="Febris" Version="%s"
           UpgradeCode="%s" Scope="perMachine" Compressed="yes">
    <MajorUpgrade AllowSameVersionUpgrades="yes"
                  DowngradeErrorMessage="A newer version of the Febris PC Suite is already installed." />
    <MediaTemplate EmbedCab="yes" />
    <StandardDirectory Id="ProgramFiles64Folder">
      <Directory Id="INSTALLFOLDER" Name="Febris">
%s
      </Directory>
    </StandardDirectory>
    <Feature Id="Main" Title="Febris PC Suite" Level="1">
      <ComponentGroupRef Id="AllFiles" />
    </Feature>
    <ComponentGroup Id="AllFiles">
%s
    </ComponentGroup>
  </Package>
</Wix>
''' % (version, UPGRADE_CODE, tree(".", "        "), body)
    open(out, "w", encoding="utf-8").write(xml)
    print("  wrote %s (%d components)" % (os.path.basename(out), len(comps)))


def main():
    ap = argparse.ArgumentParser()
    # No default. A default is a version number that lives in source, which is the whole problem
    # this pipeline exists to remove. Forgetting --version used to produce an installer named
    # 0.2.0 whatever you were actually building, and nothing downstream would notice until the
    # filename disagreed with the feed row. Making it required turns that into an argument error.
    ap.add_argument("--version", required=True,
                    help="MAJOR.MINOR.PATCH stamped into the filename and the WiX package. On a "
                         "tag build the workflow passes the tag.")
    ap.add_argument("--config", default="Release")
    ap.add_argument("--out", default=os.path.join(PC, "packaging", "out"))
    ap.add_argument("--skip-build", action="store_true")
    a = ap.parse_args()

    os.makedirs(a.out, exist_ok=True)
    stage_dir = os.path.join(a.out, "stage")
    wxs = os.path.join(a.out, "FebrisPC.wxs")
    msi = os.path.join(a.out, "FebrisPCSuite-%s-win-x64.msi" % a.version)

    if not a.skip_build:
        print("== 1. build (%s)" % a.config)
        build(a.config)
    print("== 2. stage")
    stage(a.config, stage_dir)
    print("== 3. generate WiX source")
    write_wxs(stage_dir, wxs, a.version)
    print("== 4. build the MSI")
    wix = shutil.which("wix") or os.path.expanduser(r"~\.dotnet\tools\wix.exe")
    if not os.path.exists(wix) and not shutil.which("wix"):
        sys.exit("wix not found. Run: dotnet tool install --global wix --version 5.0.2")
    run([wix, "build", wxs, "-o", msi, "-arch", "x64"])

    raw = open(msi, "rb").read()
    print("\n  %s\n  %d bytes\n  sha256 %s"
          % (msi, len(raw), hashlib.sha256(raw).hexdigest()))
    print("\n  Verify without installing:  msiexec /a \"%s\" /qn TARGETDIR=<dir>" % msi)


if __name__ == "__main__":
    main()
