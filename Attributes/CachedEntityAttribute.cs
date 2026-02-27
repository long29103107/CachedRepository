namespace CachedRepository.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class CachedEntityAttribute(int durationMinutes = 5) : Attribute
{
    public int DurationMinutes { get; } = durationMinutes;
}
