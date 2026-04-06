# Decision: AutoMapper licensing / migration

## Summary
AutoMapper (NuGet package) is now distributed under LuckyPenny's license requiring a paid license for production. This file captures the inventory, options, pros/cons, and recommended actions so the team can decide.

## Inventory (where AutoMapper is referenced)
- VoiceAdmin: `VoiceAdmin/Program.cs` (AddAutoMapper registration)
- VoiceAdmin project file: `VoiceAdmin/VoiceAdmin.csproj` (PackageReference AutoMapper 16.1.1)
- RazorClassLibrary: PackageReference AutoMapper 16.1.1
- WinFormsApp: PackageReference AutoMapper 16.1.1
- PlaywrightTests: PackageReference AutoMapper 16.1.1
- DataAccessLibrary: contains `Profiles/AutoMapperProfile.cs` and mapping profiles

(See repo grep for full hits)

## Options with pros / cons

1) Purchase LuckyPenny (AutoMapper) license
- Pros:
  - Minimal code changes; existing `IMapper` profiles and DI remain intact.
  - Fastest route to restore production safety.
- Cons:
  - Ongoing cost and vendor lock-in.
  - Need to manage license keys in CI/CD and environments.

2) Pin / vendor the last open-source AutoMapper release
- Pros:
  - No immediate refactor required.
  - Quick short-term fix.
- Cons:
  - You're responsible for security fixes and maintenance.
  - License and legal review required to ensure permitted use.
  - May degrade over time vs. upstream improvements.

3) Migrate to an open-source alternative (recommended long-term)
- Candidates: Mapster (recommended), TinyMapper, manual mapping with source-generation.
- Pros:
  - Avoids licensing costs and vendor lock-in.
  - Mapster supports source-generation and high performance; mapping code is explicit and easy to audit.
- Cons:
  - Migration effort required: convert profiles, update usages, run tests.
  - Possible small runtime differences; need verification.

4) Replace AutoMapper with hand-written or generated mapping code
- Pros:
  - Maximum control and performance; easiest to audit.
  - No external dependency risk.
- Cons:
  - More manual work to implement and maintain mappings.

## Recommendation (short-term & long-term)
- Short-term (fast): If you need production quickly and budget allows, purchase the LuckyPenny license and add the key to CI/CD. This buys time to do a migration properly.
- Medium-term (preferred): Plan and execute a migration to `Mapster` in the `DataAccessLibrary` first (smallest surface), validate tests, then roll out to other projects.
- Long-term: Remove AutoMapper entirely and prefer explicit mapping or Mapster source-generated mappers for clarity and performance.

## Suggested next steps / checklist
- [ ] Assign owner to this decision (suggest: @Mark-Phillipson)
- [ ] If purchasing: research LuckyPenny pricing and add license key to `dotnet user-secrets`/CI secrets
- [ ] If migrating: create a small spike branch converting `DataAccessLibrary` mappings to Mapster and run unit tests
- [ ] If pinning OSS: identify last OSS version, document license terms, and pin versions in `*.csproj`

## Links / references
- Code locations (search hits): `VoiceAdmin/Program.cs`, `DataAccessLibrary/Profiles/AutoMapperProfile.cs`, `VoiceAdmin/VoiceAdmin.csproj`, `RazorClassLibrary/*.csproj`, `WinFormsApp/*.csproj`

---

Please pick one of the short-term actions (purchase, pin OSS, migrate spike) and I will implement the chosen path (create PR, or add pinned package updates, or scaffold a Mapster migration).
