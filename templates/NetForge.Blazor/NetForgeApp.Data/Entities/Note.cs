namespace NetForgeApp.Data.Entities;

/// <summary>
/// A short note owned by one user. A row owned by someone else is treated as missing.
/// </summary>
public class Note : BaseEntity<int>
{
    public required string Title { get; set; }

    public string Body { get; set; } = string.Empty;

    /// <summary>Identity user id, Keycloak <c>sub</c>, or the synthetic id in <c>None</c> mode.</summary>
    public required string OwnerUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class NoteMap : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes", Constants.Schemas.App);
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(Constants.MaxLengths.Title).IsUnicode(true);
        builder.Property(m => m.Body).IsRequired().IsUnicode(true);
        builder.Property(m => m.OwnerUserId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(false);

        builder.HasIndex(m => m.OwnerUserId);
    }
}
