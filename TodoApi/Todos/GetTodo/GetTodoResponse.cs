namespace TodoApi.Todos.GetTodo;

public class GetTodoResponse
{
	public int Id { get; set; }
	public string Title { get; set; } = default!;
	public string? Description { get; set; }
	public DateTime Created { get; set; }
}