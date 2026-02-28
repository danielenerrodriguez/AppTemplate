The hackathon topic has been announced: "$ARGUMENTS"

Execute the following kickoff sequence immediately:

1. **Update CLAUDE.md** -- fill in the placeholders:
   - Replace `<!-- FILL IN: Describe the project topic and goals here when decided -->` with a clear description based on the topic
   - Replace `<!-- FILL IN: Team name -->` with the team name if provided, or ask for it
   - Replace `<!-- FILL IN: Project goal -->` with a concrete goal statement

2. **Propose a feature breakdown** -- Based on the topic, propose 3-5 vertical slice features that would make a compelling hackathon project. For each feature, describe:
   - Feature name (short, for the folder name)
   - What it does (1-2 sentences)
   - Key endpoints / pages needed
   - Priority (must-have vs nice-to-have)

   Remind the team about existing capabilities:
   - Built-in AI chat bubble (global, on every page -- no separate chat page needed)
   - `AppDbContext` with SQLite for quick persistence
   - `ApiResponse<T>` and `PagedResult<T>` common models
   - MudBlazor 9.0 component library for UI
   - `IAIService` with per-user API key support

3. **Present the plan** to the user and ask for feedback/adjustments. Suggest which features to assign to which team members if the team size is known.

4. **After user confirms**, write the task list into `PLAN.md`:
   - Fill in the `## Goal` section with the project description
   - Add each confirmed feature as a task under `## Tasks` with assignment and branch name:
     ```
     - [ ] Feature name (@person, feature/branch-name) - brief description
     ```
   - Mark priority (must-have tasks first, nice-to-have after a separator)
   - Update the "Last updated" line

5. **Scaffold all the features**:
   - Create the folder structure for each feature in both Api and Web projects
   - Add entity classes to `Shared/Data/` if features need persistence
   - Add DbSets and configurations to `AppDbContext.cs`
   - Create stub interfaces and DTOs for each feature based on the proposed design
   - Register all services in ServiceCollectionExtensions.cs
   - Map all endpoints in Program.cs
   - Add all pages to NavMenu.razor
   - Update _Imports.razor

6. **Run the build** to verify everything compiles: `dotnet build`

7. **Update README.md** with the project description and feature list

8. **Update CLAUDE.md** with any new architecture notes

9. **Commit** with message `feat: scaffold project features for {topic}` (include PLAN.md in this commit)

10. **Tell the team** what branches to create:
   - List each feature with a suggested branch name: `feature/{feature-name}`
   - Explain which person/pair should work on each branch

The goal is to go from "topic announced" to "everyone has a branch and is building" in under 5 minutes.
