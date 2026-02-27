namespace CachedRepository.Configuration;

public class CachedEntitySettings
{
    public const string SectionName = "CachedEntity";

    public int? DefaultDuration { get; set; }

    public List<EntityOverride>? Override { get; set; }
}

public class EntityOverride
{
    public string EntityName { get; set; } = string.Empty;

    public int? Duration { get; set; }
}
