

using System.Collections;

namespace Domain.DTOs.PaginationResponses
{
    public class PagData<DataType>
    {
        public ICollection<DataType> MyData { get; set; }

        public PaginationSpecs PagSpesc { get; set; }
    }
}
