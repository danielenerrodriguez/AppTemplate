View or update the project plan. Usage: `/plan [action] [args]`

Parse "$ARGUMENTS" to determine the action:

### No arguments (or "show" / "status")
Read `PLAN.md` and present a formatted summary:
1. Show the Goal
2. Count tasks by status: done [x], in progress [~], todo [ ]
3. List in-progress tasks with who's working on them
4. List remaining todo tasks
5. Show recent decisions

### `add {task description}`
Add a new task to the Tasks section in `PLAN.md`:
- Append `- [ ] {task description}` under the `## Tasks` section
- If the user mentions a person (e.g., "add Build auth @alice"), include the @mention
- Do NOT commit separately -- just edit the file. It will be committed with the next feature work.
- Confirm: "Added task: {task description}"

### `done {task keyword}`
Find the task in `PLAN.md` that best matches the keyword:
- Change `- [ ]` or `- [~]` to `- [x]`
- Append `(done)` if not already marked
- If multiple tasks match, list them and ask the user to clarify
- Do NOT commit separately.
- Confirm: "Marked done: {task}"

### `progress {task keyword}`
Find the matching task and mark it in progress:
- Change `- [ ]` to `- [~]`
- Add the current branch name and user if known (e.g., `(@bob, feature/dashboard)`)
- Do NOT commit separately.
- Confirm: "Marked in progress: {task}"

### `assign {task keyword} to {person}`
Find the matching task and add/update the @person mention:
- If the task already has an @mention, replace it
- If not, add `(@person)` after the task description
- Do NOT commit separately.
- Confirm: "Assigned: {task} to @{person}"

### `note {text}`
Add a note to the Notes or Decisions section in `PLAN.md`:
- If the text starts with "decision:" or "decided:", add to the Decisions section
- Otherwise add to the Notes section
- Prefix with `- ` and the current date
- Do NOT commit separately.
- Confirm: "Added note"

### `sync`
Pull the latest plan from main and merge into the current branch:
1. `git stash` (if there are uncommitted changes)
2. `git fetch origin main`
3. `git merge origin/main --no-edit` (to get latest PLAN.md from other teammates)
4. If there's a merge conflict in PLAN.md, resolve it by combining both sides (keep all tasks, keep all decisions, take the most recent status for each task)
5. `git stash pop` (if stashed)
6. Show the updated plan summary

### Important rules
- NEVER commit PLAN.md changes as a separate commit. Let them ride with the next feature commit.
- Update the "Last updated" line at the top of PLAN.md with the current date whenever making changes.
- When resolving PLAN.md merge conflicts, always combine both sides -- never discard tasks or decisions from either side.
