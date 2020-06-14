To get breakpoints to hit:
- Install the C# extension version 1.22.0
- Install JS debugger (nightly) version 2020.6.617
- `dotnet watch build`
- `dotnet run` (in separate terminal)
- Trigger launch configuration 'Debug'

Tests
- Tests can be run by running `dotnet test Tests/BlaTeX.Tests.csproj` or pressing F7
- Tests can be debuged by selecting them with a regex 
  - in launch.json -> Debug Test -> args for selection tests, or `*` for all
