# SFTPSync AGENTS.md

## Release process

When asked to "do a release" (or similar), follow these steps. A release is a version-bump PR merged to `master`,
followed by a version tag on the merge commit. The tag triggers the CI workflow that signs the executables and the
MSI and uploads the MSI as the `MSI` artifact.

### Rules

- The version number is required. If the user did not give one, ask for it. Versions look like `1.12` (major.minor);
  the previous release is the latest `V*` tag (`git tag --sort=-creatordate | head -3`).
- Do not push the tag without asking the user first. Pushing a tag starts a signing run that uses the Azure secrets.
- Merging to `master` does NOT build anything. CI runs on pull requests (build and package, unsigned) and on `V*`
  tags or a manual run (build, sign, upload MSI). See `.github/workflows/build.yml`.
- Do not merge a PR until its `build` check is green (`gh pr checks <n> --watch`).
- Do not commit directly to `master`. Use a branch and a PR (merge commit, like earlier PRs).

### Steps

1. Make sure `master` is current and the working tree is clean: `git checkout master && git pull`. Confirm any
   pending PRs the user wants in this release are already merged.
2. Create a branch such as `version-<version>` (for example `version-1.12`).
3. Change the old version to the new one in these 6 files (this is what the "Bump version to 1.11" commit did):
   - `SFTPSync/SFTPSync.csproj`, `SFTPSyncLib/SFTPSyncLib.csproj`, `SFTPSyncStop/SFTPSyncStop.csproj`,
     `SFTPSyncUI/SFTPSyncUI.csproj`: `<Version>`, `<AssemblyVersion>` and `<FileVersion>`.
   - `SFTPSyncSetup/Product.wxs`: the `Version="..."` attribute.
   - `SFTPSyncSetup/SFTPSyncSetup.wixproj`: `<OutputName>SFTPSync-<version></OutputName>`.

   Then search the tracked source (not `bin`, `obj` or `SSH.NET`) for the old version string to be sure nothing else
   needs changing.
4. Commit with the message `Bump version to <version>.`, push the branch, and open a PR titled
   `Bump version to <version>`.
5. Wait for the PR's `build` check to pass, then merge it (merge commit).
6. Pull `master` and confirm HEAD is the merge commit of the bump PR.
7. Ask the user to confirm, then create an annotated tag and push it. The tag name is `V` plus the version, matching
   the version in the files:

   ```
   git tag -a V<version> -m "Version <version>"
   git push origin V<version>
   ```

8. Watch the workflow run started by the tag (`gh run list --limit 3`, then `gh run watch <id>`). When it succeeds,
   tell the user the signed MSI is on that run as the `MSI` artifact, and give the run URL. If it fails, report the
   failing step; do not retag without asking.

### Notes

- Tags are annotated and capitalised (`V1.11`), and point at the merge commit of the version-bump PR.
- The workflow does not check that the tag matches the version in the files, so verify they agree.
- A signed test build can also be made without a tag by running the workflow manually
  (`gh workflow run "Build and Package SFTPSync" --ref <branch>`). Only do this when the user asks.
