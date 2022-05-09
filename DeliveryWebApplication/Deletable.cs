using Microsoft.EntityFrameworkCore;

namespace DeliveryWebApplication
{
    public class Deletable
    {
        public bool Deleted { get; set; } = false;
    }

    public static class DeletableExtensions
    {
        public static IQueryable<T> Alive<T>(this IQueryable<T> seq) where T : Deletable => seq.Where(x => !x.Deleted);
        public static IEnumerable<T> Alive<T>(this IEnumerable<T> seq) where T : Deletable => seq.Where(x => !x.Deleted);

        public static async ValueTask<T?> AliveFindAsync<T>(this DbSet<T> seq, int? id) where T : Deletable
        {
            var res = await seq.FindAsync(id);
            if (res is null || res.Deleted)
                return null;
            else
                return res;
        }

    }
}
