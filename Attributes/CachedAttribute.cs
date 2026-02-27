namespace CachedRepository.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class CachedAttribute(int durationMinutes = 5) : Attribute
{
    public int DurationMinutes { get; } = durationMinutes;
}
