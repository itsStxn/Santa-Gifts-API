namespace Santa_Gifts_API.DTOs;

public class Recommendations {
	private readonly List<Dictionary<string, string>> _items = [];
	public List<Dictionary<string, string>> Items => _items;
}
