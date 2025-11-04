namespace Domain.DTOs.PaginationResponses
{
    public class PaginationSpecs
    {
        public int page { get; set; }
        public int totalPages { get; set; }
        public int pageSize { get; set; }
        public bool hasnextPage => page < totalPages;
        public bool hasPrevPage => page > 1;
    }
}