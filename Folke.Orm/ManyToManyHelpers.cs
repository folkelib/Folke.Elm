using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    public static class ManyToManyHelpers
    {
        public static IReadOnlyList<T> UpdateManyToMany<T, TParent, TChild, TChildDto>(this IFolkeConnection connection, TParent parent, IReadOnlyList<T> currentValues, ICollection<TChildDto> newDtos, Func<TChildDto, TChild> mapper)
            where T : class, IManyToManyTable<TParent, TChild>, new()
            where TParent : IFolkeTable
            where TChild : class, IFolkeTable, new()
            where TChildDto : IFolkeTable
        {
            if (newDtos == null)
                newDtos = new List<TChildDto>();

            var valuesToAdd = newDtos.Where(v => v.Id == 0 || currentValues == null || !currentValues.Any(cv => cv.Child.Id == v.Id)).ToList();
            var newValues = currentValues == null ? new List<T>() : currentValues.ToList();
            if (valuesToAdd.Any())
            {
                var existingChildrenIds = valuesToAdd.Where(c => c.Id != 0).Select(c => c.Id);
                var existingChildren = existingChildrenIds.Any() ? connection.QueryOver<TChild>().WhereIn(c => c.Id, existingChildrenIds).List()  : (List<TChild>)null;

                foreach (var newDto in valuesToAdd)
                {
                    var child = existingChildren == null ? null : existingChildren.SingleOrDefault(c => c.Id == newDto.Id);
                    if (child == null)
                    {
                        child = mapper(newDto);
                        connection.Save(child);
                        newDto.Id = child.Id;
                    }

                    var newElement = new T { Child = child, Parent = parent };
                    connection.Save(newElement);
                    newValues.Add(newElement);
                }
            }
            
            if (currentValues != null)
            {
                var valuesToRemove = currentValues.Where(cv => newDtos.All(nv => nv.Id != cv.Child.Id));
                if (valuesToRemove.Any())
                {
                    connection.Query<T>().Delete().From().WhereIn(c => c.Id, valuesToRemove.Select(s => s.Id)).Execute();
                    foreach (var value in valuesToRemove)
                        newValues.Remove(value);
                }
            }
            return newValues;
        }
    }
}
