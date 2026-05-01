using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.Core.Reflection;
using Marten.Schema;
using Marten.Schema.Identity;

namespace Aero.Marten;

/// <summary>
/// Provides an implementation of the IIdGeneration interface that generates unique numeric identifiers using the
/// Snowflake algorithm.
/// </summary>
/// <remarks>This class is typically used to assign unique, time-ordered numeric IDs to documents or entities. The
/// generated IDs are suitable for distributed systems where uniqueness and ordering are important. The implementation
/// ensures that IDs are only generated when the current value is zero, preserving existing IDs if present.</remarks>
public class SnowflakeIdGeneration : IIdGeneration
{
    public bool IsNumeric => true;
    public void GenerateCode(GeneratedMethod method, DocumentMapping mapping)
    {
        // Get the Id member (property/field)
        var idMember = mapping.IdMember;

        // This is the variable name Marten uses internally for the document
        var document = new Use(mapping.DocumentType);

        // Generate code:
        method.Frames.Code(
            $"if ({{0}}.{mapping.IdMember.Name} <= 0) _setter({{0}}, {typeof(Snowflake).FullNameInCode()}.NewId());",
            document);
        method.Frames.Code($"return {{0}}.{mapping.IdMember.Name};", document);
    }
}