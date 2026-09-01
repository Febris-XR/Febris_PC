This is the first part of the Febris OSS release. It is safe to call this version 4 of the Febris platform. Many aspects of Version 3 had to be stripped out (The central hub, marketplace, developer system, accreditation system, micro-credentialing, CRM, LMS components that added centralized truth, and there may be a few parts that are now gone that previously existed that I cannot recall right this second) and I used Claude to create and cut that seam. If there are lingering parts, I apologize and I will fix it as soon as I can. I feel like I stretched Claude's capabilities while working on this project. AI was not used on any of the other version of Febris so some of these cuts may seem a little ragged but the entire system was built by one person so, please cut me a little slack.

Claude is far better at documenting code than I have ever been and I suspect between my naming conventions and Claude's documentation, this release will be easy to follow.

# febris-pc

**The Febris Windows desktop client suite. Five .NET 8 applications that launch training modules
on a workstation, record the session, and report xAPI statements back to a Febris node you run.**

This is the desktop half of the training experience. A learner opens the launcher, it fetches and
installs the modules the node has assigned, runs them, records the session if recording is
enabled, and ships statements back. Two of the five run as background Windows services rather than
as windowed applications.

---

## It points at YOUR node, and it refuses to guess

This matters more than anything else in this README, so it is first.

The two background services ship with `ApiUrlPath:DataApi` as an **unsubstituted placeholder**,
not as a working default. A client that has not been told which node it belongs to **fails to
start with a message naming the missing key**. It does not fall back to a Febris-operated host,
and there is no Febris host in the shipped configuration to fall back to.

Configure it the way a Windows service is normally configured, with an environment variable:

```
ApiUrlPath__DataApi=https://your-node.example.org:5102/api/
```

A trailing slash is added if you leave it off. The equivalent key in `appsettings.json` works too
if you would rather edit the file.

That same address is what the services use to decide whether there is anything to talk to. Before
each cycle of work they open a socket to the configured host and port and skip the cycle if it does
not answer. They do not ping a public address to infer it, which matters on an isolated network
where the node is reachable and the wider internet is not.

## What is in here

| Project | Kind | What it does |
|---|---|---|
| `Febris.PCLauncherV3` | WPF, `net8.0-windows` | the launcher a learner opens, and the aggregate that references the other four |
| `Febris.PCModuleManagerV3` | service, `net8.0-windows` | downloads and installs training modules from the node |
| `Febris.PCStatementManagerV3` | service, `net8.0-windows` | builds xAPI statements and ships them to the node |
| `Febris.PCScreenRecorderV3` | console, `net8.0-windows` | records the session, encoding through ffmpeg |
| `Febris.ConsoleProgressBar` | console, `net8.0` | a small progress-bar utility |

Five curated solutions ship, one per project. `Febris.PCLauncherV3.sln` is the aggregate a
stranger should open, because it already contains all five.

**This repository is Windows-only and does not pretend otherwise.** Four projects target
`net8.0-windows` and the module manager carries a COM reference to the Windows Script Host, which
it uses to create shortcuts.

## ffmpeg is invoked, not linked

The screen recorder encodes captured frames to H.264 by running `ffmpeg.exe` as a **separate child
process**. Febris does not link against ffmpeg, statically or dynamically, and shares no address
space with it. That distinction is the reason a GPL-licensed encoder can sit beside an AGPL
application without either licence reaching the other.

**The binary is not committed here.** `pc/tools/fetch-ffmpeg.ps1` is the sanctioned way to obtain
it. It writes a `PROVENANCE.txt` beside the binary recording the version, SHA-256, source URL and
the full configure string, which is the information you need to be able to produce if you
redistribute it. See [pc/THIRD-PARTY-NOTICES.md](pc/THIRD-PARTY-NOTICES.md).

## Distribution

This repository ships **source and a zip**, not an installer. The Visual Studio installer project
that used to build an MSI is deliberately not here: it is a legacy `.vdproj` that hardcoded three
absolute paths naming a specific build machine. Whether the suite eventually ships as an MSI, an
MSIX or stays a zip is an open decision.

## Limits worth knowing

Stated here rather than left to be found:

- **`Microsoft.AspNetCore.Mvc.Core 2.2.5` is referenced and is long out of support.** It is pulled
  in by the module manager and the statement manager. It needs replacing.
- **The five applications have never been run end to end as a suite.** They compile, and that is
  the evidence that exists. Treat a green build as "it is wired", not "it works".
- **The third-party notices cover ffmpeg only.** Every other redistributed assembly still needs
  accounting for before a binary is handed to anyone.
- The shared Febris libraries arrive as NuGet packages rather than as source in this repository.

## Licence

**AGPL-3.0-only.** See [LICENSE](LICENSE).

These are desktop applications rather than network services, so the network-use clause has little
practical reach. It is the platform licence and the suite carries it for consistency with the node
these applications talk to.

## Security

Report vulnerabilities privately through this repository's Security tab. See
[SECURITY.md](SECURITY.md). Please do not open a public issue for a security bug.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). The build needs Visual Studio rather than the `dotnet` CLI
alone, and that is explained there first.
