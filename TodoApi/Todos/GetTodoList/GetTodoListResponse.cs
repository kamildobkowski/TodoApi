namespace TodoApi.Todos.GetTodoList;

public class GetTodoListResponse
{
	public int Page { get; set; }
	public int PageSize { get; set; }
	public bool HasNext { get; set; }
	public bool HasPrevious { get; set; }
	public List<string> TodoTitles { get; set; } = [];
}