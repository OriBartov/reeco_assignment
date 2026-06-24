---
name: create-pr-commits
description: >-
  Creates a pull request targeting the development branch with one or more
  logically separated commits and descriptive commit messages. Use when the user
  asks to create or open a PR, submit work to development, or commit changes
  with logical separation before opening a PR.
---

# Create PR with Logical Commits

Open one PR into `development` with commits split by concern. Each commit gets its own descriptive message.

## Hard rules

- Do not commit, push, or open a PR until the user approves the commit plan.
- Base branch is `development`. If it does not exist locally or on the remote, stop and ask which branch to target.
- Stage only the files or hunks named in the approved plan. No `git add .` / `git add -A`.
- Never update git config. Never run destructive git commands (`reset --hard`, `clean -fdx`, force-push, history rewrite) without explicit approval.
- Do not commit secrets (`.env`, credentials, keys). Warn if the user asks to include them.
- Do not use the TodoWrite or Task tools while executing this workflow.

## 1. Inspect the work

Run in parallel:

```bash
git status
git diff
git diff --staged
git log --oneline -10
git branch -vv
git fetch origin development 2>/dev/null || true
git log origin/development..HEAD --oneline 2>/dev/null || git log development..HEAD --oneline 2>/dev/null || true
```

Use chat history to recover intent. Compare all changes (committed and uncommitted) against `development`.

## 2. Propose the commit plan

Split work into commits where each commit is one logical unit: one feature, one fix, one refactor, one test layer, or one docs/config change. Keep tightly coupled files together; separate independent concerns.

Present the plan before doing anything destructive:

```markdown
## PR plan

**Branch:** `feature/...` (new or existing)
**Base:** `development`
**PR title:** ...

### Commits (in order)

1. **`<commit subject>`**
   - Files: `path/a`, `path/b`
   - Why: one-line rationale

2. **`<commit subject>`**
   - Files: `path/c`
   - Why: one-line rationale
```

Use a Mermaid diagram when commit dependencies are non-obvious. Ask for approval. Adjust the plan if the user requests changes.

## 3. Prepare the branch

After approval:

1. Create or checkout the feature branch from `development` if needed:

   ```bash
   git fetch origin development
   git checkout development
   git pull origin development
   git checkout -b <branch-name>
   ```

2. If work already lives on another branch, rebase or cherry-pick only when necessary and safe — prefer committing uncommitted slices on a clean branch off `development`.

## 4. Create commits (one slice at a time)

For each approved commit, in order:

1. Stage only the listed paths (or explicit hunks).
2. Verify the stage: `git diff --staged --stat`
3. Commit with a descriptive message focused on **why**, not just what changed.

Use a HEREDOC for the message:

```bash
git commit -m "$(cat <<'EOF'
Short imperative subject (≤72 chars)

Optional body: context, motivation, or tradeoff in 1-3 sentences.
EOF
)"
```

Commit message rules:

- Subject: imperative mood (`add`, `fix`, `refactor`, not `added` / `fixes`)
- One concern per commit; the subject should stand alone in `git log --oneline`
- Match the repo's existing style when `git log` shows a convention (e.g. Conventional Commits)
- If a pre-commit hook modifies files, fix and create a **new** commit — do not amend unless all amend safety conditions are met

After each commit, confirm with `git log --oneline -3`.

## 5. Push and open the PR

```bash
git push -u origin HEAD
```

Create the PR with `gh`, base `development`:

```bash
gh pr create --base development --title "<title>" --body "$(cat <<'EOF'
## Summary
- ...

## Commits
- `<hash>` — subject
- `<hash>` — subject

## Test plan
- [ ] ...
EOF
)"
```

Return the PR URL.

## 6. Report back

Keep it short: branch name, commit list (subject only), PR URL, and anything left unstaged or uncommitted.

## Additional resources

- Commit split examples: [examples.md](examples.md)
