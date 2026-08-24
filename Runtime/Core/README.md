# Generated directory — do not edit

Everything in `unity-package/Runtime/Core/` is a **generated distribution copy**. The source
of truth lives in the monorepo at `wrappers/unity/Core/`.

```
wrappers/unity/Core/   ──────────────►   unity-package/Runtime/Core/
   (source of truth)      one-way            (generated copy)
                          sync
```

Regenerate with:

```bash
scripts/sync-unity-package-bindings.sh
```

To change the binding, edit `wrappers/unity/Core/` and re-run that script. Edits made
directly here will be reverted by the next sync and rejected by CI before then.

## Why this is enforced

These were once two independently maintained copies of the same P/Invoke binding, and they
silently diverged for an entire release. The shipped copy ended up naming 14 native symbols
that the C API had renamed, and was missing the `X`/`Y`/`Z` methods that the package's own
`QuantumProperty.cs` called. The result was a published package that neither compiled nor
ran, and nothing in CI could see it — because nothing related the two files.

Three checks now make that failure mode structural rather than a matter of remembering:

| Check | Fails when |
|---|---|
| `scripts/check-unity-package-synced.sh` | this directory differs from `wrappers/unity/Core/` by a single byte |
| `scripts/check-unity-dllimport-symbols.sh` | any `DllImport` entry point does not exist in the C API (`--committed-plugins` also checks the shipped binaries) |
| `scripts/check-unity-package-compiles.sh` | `unity-package/Runtime/` does not compile |

All three run in `.github/workflows/unity-package-ci.yml` on push and pull request.
