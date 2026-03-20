---
description: Generate a commit message from staged changes (ask before committing)
allowed-tools: Bash(git status:*),Bash(git diff:*),Bash(git log:*),Bash(git commit:*)
---
## Context
- git status:
!`git status`
- staged diff:
!`git diff --cached`
- recent commits:
!`git log --oneline -10`
## Task
1) Propose ONE commit message (prefer Conventional Commits: feat/fix/refactor/chore/docs/test/style).
2) Keep the subject line <= 72 chars, imperative mood.
3) Do NOT add any AI attribution (e.g. "Generated with …").
4) Directly provide the git commands (add + commit) without asking for confirmation.
5) IMPORTANT: This is a Windows PowerShell environment. The commit message MUST use double quotes only — do NOT use heredoc (<<'EOF') syntax. Multi-line messages should be written with actual line breaks inside double quotes:
   git commit -m "subject line

   - bullet 1
   - bullet 2"
## Example
feat(cms): add page management system with tree structure
