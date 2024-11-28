namespace TodoApi.Todos.AddTodo;

public class AddTodoRequest
{
	public string Title { get; set; } = default!;
	public string? Description { get; set; }
}