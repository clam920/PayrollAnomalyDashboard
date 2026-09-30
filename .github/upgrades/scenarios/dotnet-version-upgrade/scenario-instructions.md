# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: .NET 10 (`net10.0`)

## Source Control
- **Source Branch**: main
- **Working Branch**: upgrade-dotnet-10
- **Commit Strategy**: Single Commit at End
- **Branch Sync**: Auto (Merge)

## Upgrade Options

### Strategy
- Upgrade Strategy: All-at-Once

### Project Structure
- Package Management: Central Package Management

### Compatibility
- Unsupported API Handling: Fix Inline

### Reliability
- Test Coverage: Skip

## Strategy
**Selected**: All-at-Once
**Rationale**: Seven SDK-style projects all target net8.0, the dependency graph is three tiers deep, and the assessment identifies a contained net10.0 upgrade with 17+ estimated lines of code impact.

### Execution Constraints
- Upgrade all seven projects in one atomic operation; do not introduce dependency-tier phases.
- Verify the .NET 10 SDK and global.json compatibility before changing project files.
- Establish Central Package Management as part of the prerequisites, then update target frameworks and package versions together.
- Resolve simple and complex API compatibility issues inline, including the JWT, configuration binding, and HttpContent findings.
- Run the full solution build and test suite after the atomic upgrade; document deferred package recommendations such as deprecated xunit if they remain.

## Build Tool Decisions
- **All seven projects**: `dotnet build` and `dotnet test` because the solution is SDK-style, targets modern .NET only, and has no WPF, WinForms, embedded resource, COM, or legacy project requirements.
