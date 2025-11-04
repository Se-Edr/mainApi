using Domain.DTOs.PaginationResponses;


namespace Infrastructure.PaginationServices
{
    internal class PagService
    {
        public PagData<DataTypeForList> SetPaginationData<DataTypeForList>
            (ICollection<DataTypeForList> dataList, PaginationSpecs pagSpecs)

        {

            PagData<DataTypeForList> paginationData = new PagData<DataTypeForList>()
            {
                MyData = dataList,
                PagSpesc=pagSpecs
            };
            return paginationData;
        }
    }
}
