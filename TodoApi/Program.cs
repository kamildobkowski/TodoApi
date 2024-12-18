using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Database;
using TodoApi.Entities;
using TodoApi.Todos.AddTodo;
using TodoApi.Todos.GetTodo;
using TodoApi.Todos.GetTodoList;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDbContext>(options => options.UseNpgsql("Server=localhost;Database=todo;Username=dev;Password=DevPassword123"));
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
	options.AddPolicy("Frontend", b =>
	{
		b.WithOrigins("http://localhost:5019")
			.AllowAnyHeader()
			.AllowAnyMethod()
			.WithExposedHeaders("Location");
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/todo", async (AddTodoRequest request, TodoDbContext dbContext) =>
	{
		var todo = new Todo
		{
			Title = request.Title,
			Description = request.Description
		};
		dbContext.Add(todo);
		await dbContext.SaveChangesAsync();
		return Results.Created($"todo/{todo.Id}", new AddTodoResponse { Id = todo.Id });
	})
	.WithOpenApi();

app.MapGet("/todo/{id:int}", async(int id, TodoDbContext dbContext) =>
	{
		var todo = await dbContext.Todos.FindAsync(id);
		if (todo is null)
			return Results.NotFound();
		var todoRes = new GetTodoResponse
		{
			Title = todo.Title,
			Id = todo.Id,
			Description = todo.Description,
			Created = todo.Created
		};
		return Results.Ok(todoRes);
	})
	.WithOpenApi();

app.MapGet("/todo", async ([FromQuery] int page, [FromQuery] string? filter, [FromQuery] bool sortAsc, [FromQuery] string? sortBy, TodoDbContext dbContext) =>
{
	var pageSize = 10;
	var todosCount = await dbContext.Todos.Where(x => filter == null || x.Title.Contains(filter)).CountAsync();
	var todosQuery
		= dbContext
			.Todos
			.Where(x => filter == null || x.Title.Contains(filter));
	todosQuery = sortBy?.ToLower() switch
	{
		"description" => sortAsc
			? todosQuery.OrderBy(x => x.Description)
			: todosQuery.OrderByDescending(x => x.Description),
		"created" => sortAsc 
			? todosQuery.OrderBy(x => x.Created) 
			: todosQuery.OrderByDescending(x => x.Created),
		_ => sortAsc 
			? todosQuery.OrderBy(x => x.Title) 
			: todosQuery.OrderByDescending(x => x.Title)
	};

	todosQuery = 
			todosQuery.Skip((page - 1) * pageSize)
			.Take(pageSize);
		var todos 
			= await todosQuery
			.Select(x => new GetTodoResponse{Title = x.Title, Id = x.Id, Description = x.Description, Created = x.Created})
			.ToListAsync();
		if (todos.Count == 0)
			return Results.NotFound();
		var dto = new GetTodoListResponse
		{
			PageSize = pageSize,
			Page = page,
			HasNext = todosCount > page * pageSize,
			HasPrevious = page > 1,
			TodoTitles = todos
		};
		return Results.Ok(dto);
})
.WithOpenApi();

app.MapDelete("/todo", async ([FromQuery] int id, TodoDbContext dbContext) =>
{
	var todo = await dbContext.Todos.FindAsync(id);
	if (todo is null)
		return Results.NotFound();
	dbContext.Remove(todo);
	await dbContext.SaveChangesAsync();
	return Results.NoContent();
});

app.MapPut("/todo/{id:int}", async (AddTodoRequest request, [FromRoute] int id, TodoDbContext dbContext) =>
{
	var todo = await dbContext.Todos.FindAsync(id);
	if (todo is null)
		return Results.NotFound();
	todo.Title = request.Title;
	todo.Description = request.Description;
	await dbContext.SaveChangesAsync();
	return Results.NoContent();
});
app.UseCors("Frontend");
app.Run();
