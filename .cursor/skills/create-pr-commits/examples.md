# Commit split examples

## Good splits

**Feature + tests**

1. `feat(orders): add order cancellation endpoint` — handler, service, DTOs
2. `test(orders): cover cancellation validation and not-found cases` — test project only

**Refactor before feature**

1. `refactor(billing): extract payment gateway interface` — no behavior change
2. `feat(billing): add Stripe payment provider` — new provider + wiring

**Config vs application code**

1. `chore(docker): add PostgreSQL service to compose` — `docker-compose.yml` only
2. `feat(api): add EF Core DbContext and product repository` — application code

## Poor splits (avoid)

- **"WIP" / "fix review" / "address comments"** mixed with unrelated feature code in one commit
- **Formatting-only** changes bundled with logic changes — split or omit drive-by formatting
- **Breaking a single cohesive change** across commits that do not build independently (e.g. interface in commit 1, every implementation missing until commit 3)

## Commit message examples

```
feat(auth): validate JWT issuer against configured authority

Reject tokens from unexpected issuers before role checks run.
```

```
fix(orders): return 404 when cancelling a non-existent order

Previously surfaced as 500 from unhandled repository null.
```

```
chore(deps): bump EF Core to 8.0.11

Aligns with ASP.NET Core 8 package set in Directory.Packages.props.
```
