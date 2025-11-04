

using Domain.DTOs.Filtration;
using Domain.Models;

namespace Infrastructure.MethodExtensions
{

    public static class QueryExt
    {
        public static IQueryable<Car> CarFilter(this IQueryable<Car> query, CarFiltration filter)
        {
            if (!String.IsNullOrEmpty(filter.CarQuery))
            {
                query=query.Where(car=>
                car.CarBrand.ToUpper().Contains(filter.CarQuery.ToUpper())
                ||car.CarSpz.ToUpper().Contains(filter.CarQuery.ToUpper())
                ||car.CarOwnerName.ToUpper().Contains(filter.CarQuery.ToUpper()));
            }


            return query;
        }

        public static IQueryable<Repair> RepairFilter(this IQueryable<Repair> query,RepairFiltration filter)
        {
            if (!String.IsNullOrEmpty(filter.RepairQuery))
            {
                query = query.Where(repair => repair.RepairDesc.ToUpper().Contains(filter.RepairQuery.ToUpper())
                ||repair.CarSpz.ToUpper().Contains(filter.RepairQuery.ToUpper()));
                // play with date to filter date also
            }
            if(string.IsNullOrEmpty(filter.Criteria) || string.IsNullOrEmpty(filter.Direction))
            {
                filter.Criteria = "date";
                filter.Direction = "desc";
            }

            if (!string.IsNullOrEmpty(filter.Criteria) && !string.IsNullOrEmpty(filter.Direction))
            {
                query = filter.Criteria switch
                {
                    "price" => filter.Direction == "asc"
                        ? query.OrderBy(rep => rep.RepairPrice)
                        : query.OrderByDescending(rep => rep.RepairPrice),
                    "date" => filter.Direction == "asc"
                        ? query.OrderBy(rep => rep.DateofRepair)
                        : query.OrderByDescending(rep => rep.DateofRepair),
                    _ => query
                };
            }
            
            return query;
        }
    }
}
