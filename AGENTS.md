# Repository skills

Project skills live in [.agents/skills](.agents/skills). For .NET 11 work, check the `*-net11/SKILL.md` descriptions and read the skills relevant to the task before making changes. Preserve each project's target frameworks, language version, and dependency compatibility unless the task calls for changing them.

# Code style

Put each top-level type (class, record, struct, interface, enum) in its own file named after the type, in the folder of its namespace, except interfaces with a matching class: keep those interfaces in the class's file (for example, `INadeoAPI` belongs in `NadeoAPI.cs` alongside `NadeoAPI`). Interfaces without a matching class stay in their own files. Nested types stay with their containing type. Keep only the `using` directives each file needs.
