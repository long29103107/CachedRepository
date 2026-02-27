using System.Reflection;
using CachedRepository.Attributes;
using CachedRepository.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using CachedRepository.Data;
using CachedRepository.Repositories.Base;
using CachedRepository.Repositories.Cache;
using CachedRepository.Services;

namespace CachedRepository;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
        services.AddMemoryCache();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("CachedRepositoryDb"));

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        services.AddRepositoryDI(configuration);
    }

    private static void AddRepositoryDI(this IServiceCollection services, IConfiguration configuration)
    {
        var entityRepos = GetEntityRepositoriesFromDbContext();
        var cachedEntities = GetCachedEntityTypesFromDbContext(configuration);

        foreach (var (interfaceType, concreteType) in entityRepos)
        {
            services.AddScoped(interfaceType, concreteType);
        }

        foreach (var (entityType, duration) in cachedEntities)
        {
            var interfaceType = GetRepositoryInterface(entityType);
            var decoratorType = GetCachedDecoratorType(entityType);
            if (interfaceType is null || decoratorType is null) 
                throw new Exception($"Repository or decorator not found for entity type: {entityType}");

            DecorateWithCached(services, interfaceType, decoratorType, duration);
        }
    }

    private static Dictionary<Type, Type> GetEntityRepositoriesFromDbContext()
    {
        var result = new Dictionary<Type, Type>();
        foreach (var entityType in GetDbContextEntityTypes())
        {
            var (interfaceType, concreteType) = FindRepositoryTypes(entityType);
            if (interfaceType is not null && concreteType is not null)
                result[interfaceType] = concreteType;
        }
        return result;
    }

    private static List<Type> GetDbContextEntityTypes()
    {
        return typeof(AppDbContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GenericTypeArguments[0])
            .ToList();
    }

    private static (Type? Interface, Type? Concrete) FindRepositoryTypes(Type entityType)
    {
        var baseRepo = typeof(IBaseRepository<>).MakeGenericType(entityType);
        var assembly = Assembly.GetExecutingAssembly();

        var interfaceType = assembly.GetTypes()
            .Where(t => t.IsInterface && t != baseRepo)
            .FirstOrDefault(t => baseRepo.IsAssignableFrom(t));
        if (interfaceType is null) return (null, null);

        var concreteType = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => interfaceType.IsAssignableFrom(t))
            .FirstOrDefault(t => t.BaseType?.IsGenericType != true || t.BaseType?.GetGenericTypeDefinition() != typeof(CachedBaseRepository<>));

        return (interfaceType, concreteType);
    }

    private static Type? GetRepositoryInterface(Type entityType)
    {
        var (interfaceType, _) = FindRepositoryTypes(entityType);
        return interfaceType;
    }

    private static Dictionary<Type, int> GetCachedEntityTypesFromDbContext(IConfiguration configuration)
    {
        var result = new Dictionary<Type, int>();
        var settings = configuration.GetSection(CachedEntitySettings.SectionName).Get<CachedEntitySettings>();

        foreach (var prop in typeof(AppDbContext).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            {
                var cached = prop.GetCustomAttribute<CachedEntityAttribute>();
                if (cached is not null)
                {
                    var entityType = prop.PropertyType.GenericTypeArguments[0];
                    var duration = ResolveDuration(entityType, cached.DurationMinutes, settings);
                    result[entityType] = duration;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Resolves cache duration with priority: 1) entity override > 2) DefaultDuration > 3) attribute > 4) default 5.
    /// </summary>
    private static int ResolveDuration(Type entityType, int attributeDuration, CachedEntitySettings? settings)
    {
        var entityName = entityType.Name;

        // Priority 1: Override by entityType
        if (settings?.Override is { Count: > 0 })
        {
            var match = settings.Override.FirstOrDefault(o => o.EntityName == entityName);
            if (match?.Duration is { } d)
                return d;
        }

        // Priority 2: DefaultDuration
        if (settings?.DefaultDuration is { } defaultDuration)
            return defaultDuration;

        // Priority 3: [CachedEntity(duration)] on DbSet
        // Priority 4: CachedEntityAttribute default (5) - already in attributeDuration
        return attributeDuration;
    }

    private static Type? GetCachedDecoratorType(Type entityType)
    {
        var cachedBase = typeof(CachedBaseRepository<>).MakeGenericType(entityType);
        var derived = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .FirstOrDefault(t => cachedBase.IsAssignableFrom(t) && t != cachedBase);

        return derived;
    }

    private static void DecorateWithCached(IServiceCollection services, Type interfaceType, Type decoratorType, int duration)
    {
        typeof(DependencyInjection)
            .GetMethod(nameof(DecorateWithCachedCore), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(interfaceType, decoratorType)
            .Invoke(null, [services, duration]);
    }

    private static void DecorateWithCachedCore<TInterface, TDecorator>(IServiceCollection services, int duration)
        where TDecorator : class, TInterface
    {
        services.Decorate<TInterface>((inner, sp) =>
        {
            var cache = sp.GetRequiredService<IMemoryCache>();
            var loggerType = typeof(ILogger<>).MakeGenericType(typeof(TDecorator));
            var logger = sp.GetService(loggerType);

            object? instance = logger is not null
                ? Activator.CreateInstance(typeof(TDecorator), inner, cache, logger, duration)
                : Activator.CreateInstance(typeof(TDecorator), inner, cache, duration);

            return (TDecorator)instance!;
        });
    }
}
