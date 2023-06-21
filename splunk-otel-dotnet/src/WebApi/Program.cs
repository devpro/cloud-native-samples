// creates the builder
var builder = WebApplication.CreateBuilder(args);
var configuration = new ApplicationConfiguration(builder.Configuration);

// add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger(configuration);
builder.Services.AddHealthChecks();
builder.Services.AddOpenTelemetry(configuration, builder.Logging);


// create the application and configures the HTTP request pipeline
var app = builder.Build();
app.UseDeveloperExceptionPage(app.Environment);
app.UseSwagger(configuration);
app.UseHttps(configuration);
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks(ApplicationConfiguration.HealthCheckEndpoint);

// runs the application
app.Run();
