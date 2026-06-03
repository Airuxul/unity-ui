# Documentation Governance

**Last Updated:** 2026-06-02 · **Scope:** this package repository

## Layout

| Track | Paths |
|-------|--------|
| User | `README.md` (English), `README.zh-CN.md` (Chinese) |
| Agent | `docs/*.md` (English) |

## Update workflow

1. In the **meta repository** ([AirUnityPackage](https://github.com/Airuxul/AirUnityPackage)), run skill `doc-read-index` (read-only inventory).
2. Apply changes with meta skill `doc-generate-update` (skills live only at meta repo `.cursor/skills/`, not in this package).
3. Keep `README.md` and `README.zh-CN.md` in sync for user-visible changes.
4. Append non-trivial agent edits to `docs/CHANGELOG_AGENT.md`.

## Cross-repo rules

Layering, install index, and C# layout are defined in the meta repo:

- [DOC_GOVERNANCE](https://github.com/Airuxul/AirUnityPackage/blob/main/docs/DOC_GOVERNANCE.md)
- [AGENTS](https://github.com/Airuxul/AirUnityPackage/blob/main/docs/AGENTS.md)
- [ARCHITECTURE](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/PACKAGE_ARCHITECTURE.md)

Validation hooks (`tools/validate-docs.ps1`) run on the meta repo clone, not inside this submodule.
