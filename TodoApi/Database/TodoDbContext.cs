using Microsoft.EntityFrameworkCore;
using TodoApi.Entities;

namespace TodoApi.Database;

public class TodoDbContext : DbContext
{
	public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
	{
	}

	public DbSet<Todo> Todos { get; set; } = null!;
}