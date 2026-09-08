# Third-Party Notices, Febris PC Client Tier

This file records third-party components **redistributed in binary form** with the Febris PC launcher.
It exists because redistributing a binary carries obligations that redistributing nothing does not.
Ship this file with the installer.

Vendored source-tree components that are not shipped as binaries (for example the `gentelella` admin
theme under the portal `wwwroot/`) are tracked separately in
the project's release manifest, which is workshop-internal and not published with this repository.

---

## ffmpeg

**What it is.** The screen recorder encodes captured frames to H.264 by invoking `ffmpeg.exe` as a
**separate child process** (`ScreenRecorder.RunFfmpeg`). Febris does not link against ffmpeg, statically
or dynamically, and shares no address space with it.

**How it reaches the build.** It is not committed to this repository. `pc/tools/fetch-ffmpeg.ps1`
populates `pc/tools/ffmpeg/`, and the csproj copies that folder beside the executable. The script writes
a `PROVENANCE.txt` next to the binary recording the exact version, SHA-256, source URL and the full
`configure` string, which is the information GPL-3.0 §6 requires you to be able to produce.

**Licence.** A stock Windows ffmpeg build is compiled `--enable-gpl` (for libx264 and libx265) and
`--enable-version3`, which makes the resulting binary **GPL-3.0**. Verify per build, do not assume, and
confirm `--enable-nonfree` is **absent**: a nonfree build cannot be redistributed at all.

### Obligations when shipping it

These attach to the **ffmpeg binary only**. They do not reach Febris code.

1. **Include the licence text.** The fetch script copies ffmpeg's `LICENSE` or `COPYING` next to the
   binary. Ship it.
2. **Make the Corresponding Source available** for the exact binary shipped, meaning the ffmpeg source
   plus the scripts controlling compilation. The configure string is recorded in `PROVENANCE.txt`.
   GPL-3.0 §6 routes that are open to a commercial distributor:
   - **§6(d), recommended.** Host the matching source tarball on the same download server as the
     installer, with clear directions to it.
   - **§6(b).** A written offer, valid for at least three years, to supply the source.
   - **§6(c) is NOT available.** Passing along the upstream builder's offer is permitted for
     **noncommercial** distribution only.
3. **Preserve copyright and licence notices.**
4. **Add no further restrictions.** The installer EULA must not restrict a recipient's GPL-3.0 rights to
   the ffmpeg binary. Include a carve-out to that effect, for example:

   > Portions of this product are third-party components licensed under their own terms, listed in
   > THIRD-PARTY-NOTICES. Your rights in those components are governed solely by those licences,
   > notwithstanding any conflicting term in this agreement.

5. **Watch GPL-3.0 §6 Installation Information** if Febris is ever shipped on locked-down hardware where
   modified binaries cannot be installed. A normal Windows installer is unaffected.

### What is NOT required

Febris source does not become GPL. Invoking a separate process is not linking and does not create a
derivative work, and shipping both in one installer is mere aggregation under GPL-3.0 §5. The node is
AGPL-3.0, which is compatible with GPL-3.0 in any case, and AGPL's network clause does not reach ffmpeg
because it runs locally.

### Patents are a separate question

GPL is a **copyright** licence. It conveys no rights under the H.264/AVC **patents**, which are a
distinct legal regime administered by the Via LA (formerly MPEG LA) AVC pool. Shipping an
x264-based encoder satisfies copyright but grants no patent licence. Most AVC essential patents have
expired or are expiring, since the standard was finalised in 2003, so the practical exposure is far
lower than it was, but this is a question for counsel and it depends on distribution volume and market.

Two alternatives change that calculus:

- **Cisco OpenH264** is the encoder whose distributor pays the AVC royalties, but only for binaries
  **obtained from Cisco**. Rebuilding it forfeits that cover, and fetching from Cisco at install time
  conflicts with the air-gapped deployments this product supports.
- **Windows Media Foundation** ships an H.264 encoder as part of Windows. Nothing third-party is
  redistributed, so there is no GPL obligation and no separate patent posture to take. It costs more
  code and is Windows-only, which this tier already is. See
  the project's release manifest, which is workshop-internal and not published with this repository.

> Not legal advice. The licence facts above are verifiable from the binary. Confirm the compliance
> mechanics with counsel before any public release.
