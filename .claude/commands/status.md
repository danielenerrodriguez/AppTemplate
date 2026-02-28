Show a comprehensive project status report. Run all of the following and present a clear summary:

1. **Project Plan**: Read `PLAN.md` and summarize: goal, tasks done/in-progress/remaining, who's working on what
2. **Git Status**: Run `git status` and `git branch -a` to show current branch, pending changes, and all branches
3. **Build Status**: Run `dotnet build --nologo --verbosity quiet` and report pass/fail with any errors
4. **Test Status**: Run `dotnet test --nologo --verbosity quiet` and report pass/fail with counts
5. **Recent Activity**: Run `git log --oneline -10 --all` to show recent commits across all branches
6. **Feature Progress**: List all feature folders in `src/AppTemplate.Api/Features/` and `src/AppTemplate.Web/Components/Features/` to show what features exist

Present the results in a clean summary table format.
