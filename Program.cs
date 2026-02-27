using CachedRepository;
using CachedRepository.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices();

var app = builder.Build();
await app.ConfigureApplicationAsync();
app.MapProductEndpoints();
app.Run();