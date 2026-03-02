using CachedRepository;
using CachedRepository.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

await app.ConfigureApplicationAsync();

app.MapProductEndpoints();
app.MapCategoryEndpoints();
app.MapTransactionSampleEndpoints();

app.Run();