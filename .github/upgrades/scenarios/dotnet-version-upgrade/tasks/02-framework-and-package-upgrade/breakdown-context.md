# Breakdown Context

- Execution guidance loaded: `skills/scenarios/dotnet-version-upgrade/execution.md`.
- Breakdown hints loaded: `breakdown-hints/common.md` and `breakdown-hints/test.md`.
- Scope: seven SDK-style projects, six project references across a three-tier dependency chain, and inline package references with no existing CPM file.
- Matching hints: common multi-project dependency ordering; test project lifecycle; test framework upgrade is not triggered because the assessed xUnit/test SDK versions remain compatible.
- Non-matching hints: no SDK-style conversion, stub resolution, or unknown package replacement. The large package replacement batch does not apply because all six selected package changes have assessed target versions.
- Verdict: atomic execution is retained because scenario instructions require the complete solution upgrade in one all-at-once operation. Validation must build and test the full solution after the coordinated project edits.