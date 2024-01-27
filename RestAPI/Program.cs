using InfluxDB.Client;

var influxdbAddress = "http://localhost:8086";
var influxdbToken = "OWS1uvwaCSEmmBRMcbKsFOqVIQ_f81kNOQw9264sfYDmpvzfPzTPMwXK2pA3GVmsa_Ycm2tt9sZsjWj13nMzNQ==".ToCharArray();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var influxDbClient = InfluxDBClientFactory.Create(influxdbAddress, influxdbToken);
builder.Services.AddSingleton(x => influxDbClient);


builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
{
    builder.AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials();
}));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("CorsPolicy");

app.MapControllers();

app.Run();
