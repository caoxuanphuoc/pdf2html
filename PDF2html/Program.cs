using PDF2html.Services;
using PDF2html.Stores;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPdfExtractor, PdfExtractionService>();
builder.Services.AddSingleton<IHtmlRenderer, HtmlRenderService>();
builder.Services.AddSingleton<LayoutNormalizationService>();
builder.Services.AddSingleton<IDocumentStore, FileDocumentStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
