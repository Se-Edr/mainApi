using Domain.Models;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;


namespace Infrastructure.MethodExtensions
{
    public static class FilteringByDelegate
    {
        //public delegate IOrderedQueryable<Repair> OrderByTypeDelegate(IQueryable<Repair> repairList, Expression<Func<Repair, object>> sortField);

        ////criteria-  price or date
        //// direction - asc or desc  

        //public IOrderedQueryable<Repair> OrderByHelperMethod
        //    (IQueryable<Repair> repairList, OrderByTypeDelegate filtration, Expression<Func<Repair, object>> sortField) =>
        //     filtration(repairList, sortField);


        //public static IQueryable<Repair> FilterWithDelegate(this IQueryable<Repair>)
        //{
        //    return null;
        //}

    }
}
