# OctarineCodex — Project Guidelines

These guidelines define how we build OctarineCodex with quality-first engineering. They apply to all contributors and AI assistants (Junie).

## 1) Project Overview
- Technology: .NET 8, MonoGame.
- Core loop: Players acquire Cards (TGC style). Cards are procedurally generated and feed a world generator. An autonomous Agent explores the world, battles enemies, and enemies drop loot that converts into more cards.
- Magic system: All systemic interactions are driven by an 8-dimensional vector space inspired by a 7D model (frequency, velocity, energy, charge, temperature, luminosity, amount) extended to 8D for symmetry. Our 8 base axes are represented in code by EleAspects.Element, with dual Aspects per axis.
- Current reference: See OctarineCodex\EleAspects.cs, which defines:
  - Element: Solidum, Febris, Ordinem, Lumines, Varias, Inertiae, Subsidium, Spatium (the 8 axes)
  - Aspect: Positive/Negative aspect per axis (e.g., Ignis/Hydris for Febris, Tellus/Aeolis for Solidum, etc.)

## 2) Architecture & Principles
- SOLID adherence throughout.
- Favor composition over inheritance.
- Boundary surfaces:
  - Domain: Magic vectors, cards, world gen rules, combat resolution, loot mapping.
  - Application: Orchestration, use-cases, game state flow.
  - Infrastructure: MonoGame integration, content pipeline, persistence, adapters.
- Each boundary communicates via interfaces. Do not depend on concretes across boundaries.

## 3) Dependency Injection
- Use Microsoft.Extensions.DependencyInjection for DI.
- Register services by interface in a single Composition Root (e.g., Program.cs startup). Avoid service location patterns in domain code.
- Lifetime guidance:
  - Singleton: Pure stateless services, configuration, factories.
  - Scoped: Not typical in MonoGame; when needed (e.g., per-simulation tick) use well-named child containers or explicit scope objects.
  - Transient: Entities, calculators, strategies.
- Domain types should be POCOs without framework dependencies.

## 4) Testing Strategy (TDD-first)
- Always write a failing test before implementing or changing behavior.
- Framework: xUnit (preferred) + FluentAssertions (optional). Mocking via NSubstitute or Moq.
- Structure:
  - Tests project: OctarineCodex.Tests (net8.0). Mirror src namespaces.
  - Unit tests for: vector math, aspect-element mapping, card generation rules, combat resolution, loot conversion.
  - Integration tests for: DI composition, world generation pipelines.
- Coverage: Target ≥ 85% for domain and application layers. Infrastructure code is validated via integration tests.
- Non‑interactive only: Tests must be fully automated and deterministic — never require or wait for user input, key presses, or console prompts.
- Banned in tests: Do not use Console.ReadLine/ReadKey/Read, Console.Read, Thread.Sleep for timing‑based waits, or start interactive loops (e.g., Program.Main, Game1.Run) inside tests. Use mocks/fakes and dependency abstractions instead.
- No manual testcases: Do not write tests that "start the program and then expect user input" under any circumstance.
- Running tests:
  - CLI: dotnet test
  - Junie: use run_test fullSolution (preferred) or target specific FQNs as needed.

## 5) Build & Run
- Build: dotnet build OctarineCodex.sln -c Debug
- Run (MonoGame): dotnet run --project OctarineCodex\OctarineCodex.csproj
- Content pipeline: keep Content.mgcb under version control; generated content artifacts should be excluded if reproducible from source.

## 6) Code Style & Quality Gates
- C# 12, net8.0.
- Enable nullable reference types and implicit usings in csproj.
- Analyzers: Microsoft.CodeAnalysis.FxCopAnalyzers or built-in .NET analyzers set to warning-as-error for domain/application.
- Formatting: EditorConfig-compatible (Rider/VS default C# conventions). Prefer expression-bodied members for trivial methods; keep methods small and cohesive.
- Naming: Interfaces start with I. Async methods end with Async. No abbreviations in public APIs.
- Immutability: Prefer immutable domain models where practical; use records for value types (e.g., magic vectors).
- Error handling: No silent catch. Use Result types or exceptions with clear messages at boundaries.

## 7) Magic System Modeling
- The 8D vector should be represented by a dedicated value type (e.g., MagicVector8) with operations: addition, scaling, projection, normalization (as meaningful), and clamping.
- EleAspects.cs defines mapping utilities between Element and Aspect. Keep these functions pure and side-effect free.
- Document physical intuition per axis (e.g., Febris ~ temperature; Varias ~ time/space manifold; Inertiae ~ density).

## 8) Repository Structure
- OctarineCodex\ (game project)
- OctarineCodex.Tests\ (tests project) — to be added
- .junie\ (automation guidelines and tooling)
- CI (e.g., GitHub Actions) — add workflow to run build + tests on PRs

## 9) Workflow for Changes (for Humans and Junie)
1) Create/Update tests describing desired behavior (failing).
2) Implement minimal code to pass tests.
3) Run all tests locally.
4) Ensure solution builds without warnings in modified areas.
5) Submit PR with:
   - Summary of change
   - Linked issue
   - Notes on DI registrations
   - Test evidence (what changed, why)

Junie-specific:
- Before submit: run tests using run_test fullSolution whenever tests exist; otherwise, at least build the solution using build.
- Keep changes minimal and isolated; prefer new types over expanding responsibilities.
- Update this guidelines file if conventions evolve.

## 10) Definition of Done
- New/changed behavior has unit tests.
- Build passes; tests pass; no new analyzer warnings.
- DI composition validated (app starts and composes successfully, or composition verified by tests).
- Public API is documented (XML comments or README section where applicable).

## 11) Misc
- Versioning: Semantic Versioning for releases.
- Licensing/Assets: Ensure assets used comply with licensing; attribute where required.


## 12) Commit Messages
- Follow Conventional Commits format: `type(scope): summary`.
  - Examples:
    - `fix(parser): handle unexpected end-of-file error`
    - `docs(guidelines): clarify testing bans and DI rules`
- Allowed types:
  - feat: Introduces a new feature
  - fix: Patches a bug
  - docs: Documentation-only changes
  - style: Changes that do not affect the meaning of the code (white-space, formatting, etc.)
  - refactor: A code change that neither fixes a bug nor adds a feature
  - perf: Improves performance
  - test: Adds missing tests or corrects existing tests
  - chore: Changes to the build process or auxiliary tools and libraries such as documentation generation
- Change listing (optional but recommended in body):
  - List each significant change using three-letter abbreviations:
    - CHG — Change/Modify existing functionality
    - ADD — Add new functionality or files
    - REM — Remove functionality or files
  - Note: Any sufficiently self-evident three-letter acronym may be used beyond the specified examples when clearer.
- Objective of commit message generator: Convert git-style patches into coherent, complete, and brief commit messages.
- Template:
  - Subject: `type(scope): concise summary`
  - Body (optional):
    - Context/Why
    - Change listing:
      - `ADD:`
      - `CHG:`
      - `REM:`
  - Footer (optional): References (e.g., `Refs #123`, `BREAKING CHANGE:` details)
