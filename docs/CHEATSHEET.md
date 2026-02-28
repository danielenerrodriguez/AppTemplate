# Claude Code Cheat Sheet

A guide for team members who are new to coding or Claude Code.

## What is Claude Code?
Claude Code is an AI coding assistant that runs in your terminal. You talk to it in plain English, and it can read, write, and run code for you. Think of it as a very capable pair programmer.

## Getting Started
1. Open your terminal (Command Prompt, PowerShell, or Terminal)
2. Navigate to the project folder: `cd path/to/AppTemplate`
3. Type `claude` and press Enter
4. Tell Claude your name and what you're working on -- it handles the rest!

## Git is Automatic

**You don't need to know git.** Claude handles all of this for you:

- **Pulling latest code** -- happens when you start a session
- **Creating branches** -- Claude creates one for your feature
- **Committing changes** -- Claude saves your work as you go
- **Pushing to GitHub** -- your work is backed up automatically
- **Merging to main** -- when your code builds and passes tests, Claude merges it

If you're curious, here's what happens behind the scenes:
1. Claude pulls the latest code from `main` when you start
2. Creates or switches to your feature branch (e.g., `feature/user-profile`)
3. Commits your changes with descriptive messages as you work
4. When you're done, Claude pushes to GitHub and merges to `main`
5. If there's a merge conflict, Claude tells you and helps resolve it

## Common Tasks You Can Ask

### Creating New Features
- "Create a new feature called UserProfile with an API endpoint and Blazor page"
- "Add a form on the home page that collects user feedback"
- "Create a new page that displays a list of items from the API"

### Fixing Issues
- "The build is failing, can you fix it?"
- "I'm getting an error when I click the submit button" (paste the error)
- "The weather page isn't showing any data"

### Running the App
- "Build the project and tell me if there are errors"
- "Run the tests"
- "Start the API and web app"

### Understanding Code
- "Explain what the WeatherService does"
- "How does authentication work in this project?"
- "What files would I need to change to add a new API endpoint?"

### Managing Your Work
- "What have I changed so far?"
- "Save my work and sync with the team"
- "I'm done for now" (triggers auto-commit/push/merge)
- "Switch me to a new feature called dashboard"

## Useful Commands
| Command | What it does |
|---------|-------------|
| `/clear` | Start a fresh conversation |
| `Ctrl+C` | Cancel if Claude is taking too long |
| `Ctrl+O` | Toggle verbose mode (see more details) |

## Tips for Effective Prompting
1. **Be specific**: "Add a button to the Weather page that refreshes the data" is better than "add a button"
2. **Provide context**: "I'm working on the user registration feature" helps Claude focus
3. **Reference existing features**: "Look at the Weather feature and add a similar one for users"
4. **Paste errors**: If something breaks, copy the full error message and paste it
5. **Ask for explanations**: "Explain what you just did" after Claude makes changes
6. **Iterate**: "That's close but make the title bigger" -- you can refine in conversation

## What You Can Contribute (Even Without Coding)
- **UI/UX**: "Make the page look better", "Add spacing between the cards", "Change the color to blue"
- **Content**: "Update the text on the home page to say..."
- **Testing**: "Write tests for this feature"
- **Documentation**: "Add comments explaining what this code does"
- **Ideas**: Describe what you want and let Claude build it

## Troubleshooting

### Claude / Coding Issues
- **Claude seems stuck**: Press `Ctrl+C` and try rephrasing your request
- **Build errors after changes**: Say "the build is broken, can you fix it?"
- **Don't understand what happened**: Ask "explain what you just changed and why"
- **Want to undo**: Ask "undo the last changes"

### Git / Sync Issues
- **"Could not push"**: Check your internet connection, then tell Claude "try pushing again"
- **Authentication error / "not set up"**: Tell Claude "help me set up GitHub access" -- it will open a browser for you to sign in
- **Merge conflict**: Tell Claude "there's a merge conflict, help me resolve it"
- **Want to see what's changed**: Ask "show me the git status"
- **Changes not appearing for team**: Ask Claude "push my changes to GitHub"
- **Need to start over on a feature**: Ask "switch to main and create a new branch for [feature]"
- **"Need collaborator access"**: Send your GitHub username to the repo owner, accept the invite at github.com/notifications, then tell Claude "try again"
