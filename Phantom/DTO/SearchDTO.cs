using Backend.Interface;

namespace Backend.DTO;

public class SearchResultDTO
{
    public IEnumerable<object> Results { get; set; } = [];
    public PaginationMetaData Metadata { get; set; } = new PaginationMetaData();
}

public class PaginationMetaData
{
    public int Total { get; set; } = 0;
    public int Limit { get; set; } = 10;
    public int Offset { get; set;} = 0;
}