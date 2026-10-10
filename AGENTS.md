# Repository Guidelines

## Project Structure & Module Organization

This directory currently contains no application code, tests, assets, or build configuration. No technology stack is established; the directory name alone does not establish a .NET project.

When adding the initial implementation, group production code, tests, and static assets into clearly named directories. Document the actual layout in this guide and add setup instructions to `README.md`. Keep related modules together and avoid committing generated build output.

## Build, Test, and Development Commands

No build, test, or local development commands are currently configured. When introducing a toolchain, document the required runtime and dependency versions and provide exact commands for:

- Installing or restoring dependencies.
- Building the application.
- Running it locally.
- Executing tests and any formatting or lint checks.

Verify each command from the repository root before documenting it. Prefer repository-managed scripts or configuration over undocumented machine-specific steps.

## Coding Style & Naming Conventions

No formatter, linter, or indentation convention is established. Use the selected language’s standard conventions and configure consistent whitespace, preferably through `.editorconfig`. Choose descriptive file, type, and function names. Keep formatting changes separate from functional changes when practical.

## Testing Guidelines

No test framework or coverage threshold is configured. Introduce tests alongside executable functionality and document the chosen framework and test discovery pattern. Name tests after the behavior they verify, cover meaningful failure cases, and keep tests deterministic.

## Commit & Pull Request Guidelines

No Git repository or commit history is available here. Until a convention is established, use concise, imperative commit subjects, such as `Add application setup instructions`.

Pull requests should explain the change, include verification commands and results, and link relevant issues. Attach screenshots for visible interface changes. Explicitly identify checks that could not be run.

## Security & Configuration

Never commit credentials or local secrets. Document required environment variables using placeholder values and add appropriate ignore rules when introducing tooling.
