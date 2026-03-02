# V Rising Modding Framework Documentation Plan

## 1. Introduction and Objectives

### 1.1 Purpose of This Documentation Plan

This plan establishes a comprehensive documentation framework for the V Rising Modding Framework—a multi-plugin ecosystem consisting of VAutomationCore, Bluelock, CycleBorn, Chat, and various extension modules. The documentation objectives are designed to support three distinct user communities: server administrators deploying these mods, mod developers building upon the framework, and contributors extending the core functionality.

The framework currently comprises approximately 200+ source files spanning core services, command systems, ECS components, zone management, and lifecycle handling. This scale necessitates a structured, tiered documentation approach that addresses both conceptual understanding and practical implementation details. The plan prioritizes clarity for intermediate technical users who possess familiarity with game server administration or C# development but may lack deep expertise in Unity's ECS architecture or V Rising's specific modding constraints.

### 1.2 Documentation Goals

The primary goals driving this documentation initiative include reducing onboarding time for new users and developers, establishing consistent terminology across all documentation surfaces, enabling self-service troubleshooting through comprehensive reference materials, and creating a scalable foundation for future plugin additions. Each goal directly supports the project's accessibility objectives while maintaining technical rigor appropriate for a production-grade game modification framework.

### 1.3 Scope Overview

This plan covers documentation for all currently deployed plugins and their dependencies, with explicit attention to version compatibility between framework components. The scope excludes detailed game mechanic explanations unrelated to framework usage and assumes readers possess baseline knowledge of V Rising's modding ecosystem.

---

## 2. Target Audience Analysis

### 2.1 Primary Audience: Server Administrators

Server administrators constitute the largest user base and typically possess moderate technical competence. They require practical guidance on plugin installation, configuration file management, command syntax, and basic troubleshooting. Their documentation needs center on operational tasks rather than development activities, emphasizing clear step-by-step procedures and configuration examples they can adapt for their specific server environments.

These users typically access documentation infrequently, preferring to locate specific answers quickly rather than reading extensive conceptual material. They benefit from well-organized reference sections, searchable content, and concrete examples demonstrating common deployment scenarios. Documentation targeting this audience should minimize jargon and provide context for technical decisions when unavoidable.

### 2.2 Secondary Audience: Mod Developers

Mod developers extend or integrate with the framework, requiring deeper technical understanding of APIs, service contracts, and extension points. This audience possesses strong C# programming skills and familiarity with HarmonyLib patching patterns, though they may need guidance on V Rising-specific conventions and constraints.

Developer-focused documentation must provide comprehensive API references with usage examples, architectural overviews explaining how components interact, migration guides for version updates, and troubleshooting guidance for integration issues. Code examples should demonstrate realistic usage patterns rather than simplified abstractions, reflecting the complexity developers encounter when building production-quality mods.

### 2.3 Tertiary Audience: Framework Contributors

Contributors maintaining and extending the core framework require documentation covering internal architecture, coding standards, testing procedures, and release processes. This audience represents the smallest but most technically sophisticated group, requiring detailed technical specifications and design rationale documentation that explains why particular implementation choices were made.

### 2.4 Audience Summary Table

| Audience | Technical Level | Primary Needs | Documentation Priority |
|----------|-----------------|---------------|------------------------|
| Server Admins | Moderate | Installation, configuration, commands | Highest |
| Mod Developers | Advanced | APIs, extension points, integration | High |
| Contributors | Expert | Architecture, standards, processes | Medium |

---

## 3. Documentation Types Required

### 3.1 User Guides

User guides serve as the primary onboarding resource for server administrators. These documents provide comprehensive coverage of plugin installation procedures, initial configuration workflows, and operational best practices. Each plugin within the framework requires a dedicated user guide addressing its specific features and configuration options.

The VAutomationCore user guide covers framework installation, dependency management, and core service initialization. Bluelock documentation addresses zone creation, arena configuration, kit systems, and border customization. CycleBorn documentation handles lifecycle policies, PvP configurations, and announcement management. Additional guides address the Chat plugin and extension modules.

### 3.2 Command Reference

Command references document every chat and console command available to users, including syntax, parameters, permissions, and examples. This documentation must remain synchronized with command implementations, requiring automated generation or strict update protocols to prevent drift between code and documentation.

Each command entry should include the command name, short description, full parameter list with types and default values, required permission level, usage examples ranging from basic to advanced, and related commands for discoverability. Cross-referencing between related commands improves navigation efficiency.

### 3.3 API Reference

API reference documentation targets developers integrating framework services into custom plugins. This includes public service interfaces, event systems, configuration abstractions, and extension points. Documentation must cover method signatures, parameter semantics, return value expectations, exception conditions, and thread safety considerations.

The core API surfaces include the ServiceRegistry for dependency resolution, FlowService for lifecycle orchestration, EntityAliasMapper for entity management, and various configuration providers. Each public type requires comprehensive XML documentation comments that can be processed into reference formats.

### 3.4 Technical Specifications

Technical specifications document architectural decisions, design patterns, and implementation details relevant to contributors and advanced developers. These documents explain the reasoning behind significant technical choices, enabling future maintainers to understand the context of current implementations.

Key specification topics include ECS integration patterns, lifecycle management architecture, configuration versioning strategies, and sandbox progression systems. Specifications should include rationale sections explaining trade-offs considered during design.

### 3.5 Configuration Guides

Configuration guides provide detailed reference material for all configuration files and settings. Unlike user guides focused on workflows, configuration guides document each setting's purpose, acceptable values, default behavior, and interaction effects with other settings.

Configuration documentation must cover VAutomationCore settings, Bluelock zone configurations, CycleBorn lifecycle policies, and JSON schema definitions. Version-specific migration notes explain how configurations change between releases.

### 3.6 Troubleshooting Guide

Troubleshooting guides consolidate common issues and their resolutions, organized by symptom category. This documentation supports both self-service problem resolution and triage procedures for more complex issues requiring developer attention.

---

## 4. Detailed Content Outlines

### 4.1 VAutomationCore User Guide

The VAutomationCore user guide begins with an introduction covering framework purpose and positioning within the V Rising modding ecosystem. Installation instructions detail BepInEx setup, plugin deployment, and dependency verification procedures. The configuration overview section explains the unified configuration system and migration handling.

Core concepts chapters introduce service initialization, command registration patterns, and ECS integration basics. The operational procedures section covers server startup verification, log interpretation, and performance monitoring. Troubleshooting appendices address common installation failures, configuration errors, and runtime exceptions.

### 4.2 Bluelock User Guide

Bluelock documentation centers on zone management functionality. The getting started section explains zone concept fundamentals and initial setup procedures. Zone configuration chapters detail zone definition files, ability assignments, border configurations, and lifecycle behaviors.

Arena system documentation covers match management, respawn rules, scoring systems, and PvP configurations. The kit system section explains inventory presets, item distributions, and kit application rules. Template system documentation addresses snapshot creation, spawning procedures, and validation workflows.

### 4.3 CycleBorn User Guide

CycleBorn documentation focuses on lifecycle event handling and PvP mechanics. The lifecycle overview explains event types, handler registration, and execution order. PvP configuration covers combat rules, death handling, respawn prevention, and score tracking.

Announcement system documentation addresses message templates, trigger conditions, and delivery methods. Flow registry documentation explains how lifecycle policies connect to zone transitions and state changes.

### 4.4 Command Reference Structure

Command reference entries follow a consistent template. The header includes command name, aliases, and permission level. The syntax section provides formal parameter specification with optional and required elements clearly distinguished. The description section explains command purpose and behavior in detail.

Examples section provides three usage tiers: basic invocation demonstrating minimal parameters, intermediate usage showing common option combinations, and advanced scenarios illustrating complex configurations. The output section describes expected response formats. Related commands section provides navigation links to functionally related commands.

### 4.5 API Reference Structure

API reference entries document public types organized by namespace. Each type entry includes summary description, namespace declaration, type classification (class, interface, struct), and inheritance hierarchy. Members are documented with parameters, return types, and semantic descriptions.

Service interfaces document contract requirements, implementation expectations, and usage patterns. Extension methods include example invocations demonstrating fluent usage. Event types document payload structures and dispatch conditions.

### 4.6 Technical Specification Structure

Technical specifications follow ADR-inspired formatting with context, decision, and consequences sections. Architecture documents include component diagrams illustrating relationships between major system elements. Design rationale explains constraints, trade-offs, and alternative approaches considered.

---

## 5. Recommended Tools and Platforms

### 5.1 Documentation Authoring Tools

Documentation creation should leverage Markdown as the primary source format, enabling version control integration and flexible output generation. Markdown provides broad tool support, developer familiarity, and straightforward conversion to multiple output formats. VS Code with Markdown extensions provides an efficient authoring environment with live preview capabilities.

For API documentation, tools that process XML documentation comments automate reference generation from source code. DocFX or similar tools extract C# XML comments and generate searchable HTML output. This approach ensures API reference accuracy by deriving documentation directly from code.

### 5.2 Hosting Platforms

GitHub Pages provides free hosting with automatic deployment from repository content, making it the recommended platform for this open-source project. GitHub Pages integrates directly with the repository's documentation directory, enabling pull-request-based content updates with built-in review workflows.

Alternative hosting options include Read the Docs for more sophisticated documentation platforms offering version switching, search functionality, and PDF generation. However, GitHub Pages provides sufficient functionality for this project's current needs while minimizing infrastructure complexity.

### 5.3 Additional Tools

Static site generators transform Markdown content into HTML. MkDocs with Material theme provides attractive, searchable documentation with minimal configuration. Hugo offers faster build times for larger documentation sets. The Material theme's navigation features support the hierarchical structure required for multi-plugin documentation.

Diagramming tools should produce vector graphics compatible with Markdown embedding. Mermaid.js enables diagram-as-code approaches, maintaining diagrams within version-controlled source files. PlantUML provides more sophisticated UML capabilities when detailed class or sequence diagrams are required.

---

## 6. Content Review and Maintenance Processes

### 6.1 Content Review Workflow

All documentation changes require peer review through pull request processes. Review requirements vary by documentation type: user-facing guides require at least one reviewer with server administration experience, while API reference changes require review by a contributor familiar with the documented components.

Automated checks verify Markdown syntax validity, link integrity, and consistency with defined style guidelines. Broken link detection should run on every build, identifying references to missing files or invalid URLs before deployment.

### 6.2 Documentation Versioning

Documentation versions align with framework release versions. The main documentation branch tracks the current stable release, with version branches maintaining historical documentation for prior releases. Git tags mark documentation snapshots corresponding to each framework release.

When releasing new framework versions, documentation updates must accompany code changes in the same pull request. This practice ensures documentation remains synchronized with implementation and prevents knowledge debt accumulation.

### 6.3 Maintenance Responsibilities

Documentation maintenance distributes across the contributor team based on domain expertise. Each plugin's maintainer bears primary responsibility for their plugin's documentation accuracy. Core framework documentation falls under general contributor responsibility, with specific sections assigned to relevant component owners.

A quarterly documentation audit reviews content freshness, identifies outdated examples, and prioritizes improvements based on user feedback and support channel inquiries. This audit ensures documentation evolves alongside the framework rather than becoming stale.

### 6.4 Content Freshness Tracking

Documentation pages include last-updated timestamps and version compatibility indicators. Clear deprecation notices inform users when documented features change or become obsolete. Migration guides address breaking changes between versions, providing upgrade paths for affected users.

---

## 7. Timeline and Milestones

### 7.1 Phase 1: Foundation (Weeks 1-3)

The foundation phase establishes documentation infrastructure and produces initial user-facing content. Week 1 focuses on repository structure setup, including documentation directory organization, MkDocs configuration, and GitHub Pages deployment pipeline. Week 2 produces the VAutomationCore user guide covering installation, configuration, and core concepts. Week 3 completes Bluelock user documentation addressing zone management and arena systems.

Milestone 1.1: Documentation repository configured and publicly accessible with automated deployment
Milestone 1.2: VAutomationCore user guide complete and reviewed
Milestone 1.3: Bluelock user documentation complete

### 7.2 Phase 2: Expansion (Weeks 4-6)

Phase 2 expands documentation coverage to remaining plugins and reference materials. Week 4 produces CycleBorn documentation covering lifecycle events and PvP configurations. Week 5 generates command reference documentation through automated extraction or manual compilation. Week 6 develops configuration reference covering all settings across plugins.

Milestone 2.1: CycleBorn documentation complete
Milestone 2.2: Command reference published and searchable
Milestone 2.3: Configuration reference complete

### 7.3 Phase 3: Developer Content (Weeks 7-9)

Developer-focused documentation occupies phase 3. Week 7 produces API reference through DocFX generation and review. Week 8 develops technical specifications for core architecture sections. Week 9 creates developer guides covering extension patterns and integration scenarios.

Milestone 3.1: API reference published and integrated
Milestone 3.2: Technical specifications complete
Milestone 3.3: Developer guides available

### 7.4 Phase 4: Refinement (Weeks 10-12)

The final phase addresses quality improvements and process establishment. Week 10 implements automated checks for broken links and style consistency. Week 11 conducts comprehensive review addressing gaps identified during earlier phases. Week 12 establishes ongoing maintenance processes and documentation governance.

Milestone 4.1: Automated quality checks operational
Milestone 4.2: Content review complete with identified improvements addressed
Milestone 4.3: Maintenance processes documented and operational

---

## 8. Version Control and Collaboration Workflow

### 8.1 Repository Structure

Documentation resides in a dedicated docs/ directory within the main repository. This structure keeps documentation close to source code, enabling synchronized updates during development. The docs/ directory follows this organization:

```
docs/
├── index.md                    # Documentation home
├── user-guides/
│   ├── vautomation-core.md
│   ├── bluelock.md
│   └── cycleborn.md
├── command-reference/
│   ├── index.md
│   ├── admin-commands.md
│   └── player-commands.md
├── api-reference/
│   ├── index.md
│   └── generated/              # DocFX output
├── technical-specs/
│   ├── architecture.md
│   └── lifecycle-design.md
├── configuration/
│   └── reference.md
└── troubleshooting/
    └── index.md
```

### 8.2 Branching Strategy

Documentation updates follow the same branching strategy as code contributions. Feature branches document specific additions or changes, merging through pull requests after review. Hotfix branches address urgent documentation corrections, following expedited review procedures.

Documentation-only pull requests are welcome and encouraged. Contributors need not implement code changes to improve documentation—spelling corrections, clarity improvements, and example additions represent valuable contributions.

### 8.3 Commit Conventions

Commit messages follow conventional commits format with type prefixes indicating documentation changes. Types include docs for general documentation updates, doc-guide for user guide modifications, doc-api for API reference changes, and doc-fix for correction commits. This convention enables automated changelog generation and commit filtering.

### 8.4 Collaboration Guidelines

Documentation discussions occur through GitHub issues tagged with documentation category. Issue templates provide structured formats for feature requests, corrections, and questions. Pull request descriptions should explain documentation changes and provide context for reviewers.

The team maintains a documentation channel in community communication platforms for real-time coordination. Regular documentation triage sessions review incoming issues and prioritize content development.

### 8.5 Review Standards

Documentation reviews assess accuracy, clarity, completeness, and consistency with existing content. Reviewers verify technical correctness of code examples, ensuring they compile and reflect current API signatures. Style consistency checks confirm adherence to formatting guidelines and terminology standards.

---

## 9. Summary

This documentation plan provides a structured framework for creating comprehensive, maintainable documentation for the V Rising Modding Framework. The plan addresses all requested components: introduction explaining documentation objectives, target audience analysis, documentation type specifications, content outlines, tool recommendations, review processes, timeline with milestones, and collaboration workflows.

Successful implementation requires consistent attention to documentation quality during ongoing development. The processes established in this plan should become integral to the development workflow rather than treated as separate activities. Documentation coherence directly impacts user experience and community growth, justifying sustained investment in the practices outlined here.

The phased timeline distributes development across twelve weeks, producing immediate value early while establishing sustainable practices for ongoing maintenance. Regular audits and clear responsibility assignments ensure documentation quality persists beyond initial creation.

This plan may require adjustment based on team capacity, user feedback, and evolving framework scope. Annual plan reviews should assess progress, update priorities, and incorporate lessons learned from execution.

---

## Appendix A: Documentation Checklist

The following checklist summarizes required documentation artifacts:

- [ ] VAutomationCore User Guide
- [ ] Bluelock User Guide
- [ ] CycleBorn User Guide
- [ ] Command Reference (complete)
- [ ] Configuration Reference
- [ ] API Reference (auto-generated)
- [ ] Technical Specifications (architecture, lifecycle)
- [ ] Developer Guides (integration, extension)
- [ ] Troubleshooting Guide
- [ ] Glossary of Terms

## Appendix B: Style Guidelines Summary

Documentation should maintain consistent voice addressing users in second person, use present tense for describing behaviors, prefer active voice over passive constructions, employ parallel structure in lists and procedures, and provide context before technical details. Code examples should use syntax highlighting and include comments explaining non-obvious logic.
