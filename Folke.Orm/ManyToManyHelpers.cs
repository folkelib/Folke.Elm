using System;
using System.Collections.Generic;
using System.Linq;

namespace Folke.Orm
{
    public interface IManyToManyHelperConfiguration<TChild, TDto>
            where TChild : IFolkeTable
    {
        bool AreEqual(TChild child, TDto dto);
        TChild Map(TDto dto);
        IList<TChild> QueryExisting(IFolkeConnection connection, IList<TDto> dto);
        void UpdateDto(TDto dto, TChild child);
    }

    public static class ManyToManyHelpers
    {
        public static IReadOnlyList<T> UpdateManyToMany<T, TParent, TChild, TChildDto>(this IFolkeConnection connection, TParent parent, IReadOnlyList<T> currentValues, ICollection<TChildDto> newDtos, Func<TChildDto, TChild> mapper)
            where T : class, IManyToManyTable<TParent, TChild>, new()
            where TParent : IFolkeTable
            where TChild : class, IFolkeTable, new()
            where TChildDto : IFolkeTable
        {
            return connection.UpdateManyToMany(parent, currentValues, newDtos, new FolkeTableManyToManyHelper<TChild, TChildDto>(mapper));
        }

        
        public static T MustExist<T>() where T : class
        {
            throw new Exception("The item does not exist");
        }

        private class FolkeTableManyToManyHelper<TChild, TDto> : IManyToManyHelperConfiguration<TChild, TDto>
            where TChild: class, IFolkeTable, new()
            where TDto: IFolkeTable
        {
            private readonly Func<TDto, TChild> mapper;
            public FolkeTableManyToManyHelper(Func<TDto, TChild> mapper)
            {
              this.mapper = mapper;
            }

            public bool AreEqual(TChild child, TDto dto)
            {
                return child.Id == dto.Id;
            }

            public TChild Map(TDto dto)
            {
                return mapper(dto);
            }

            public IList<TChild> QueryExisting(IFolkeConnection connection, IList<TDto> dto)
            {
                var existingChildrenIds = dto.Where(c => c.Id != 0).Select(c => c.Id).ToArray();
                return existingChildrenIds.Any() ? connection.QueryOver<TChild>().Where(c => c.Id.In(existingChildrenIds)).List() : null;
            }

            public void UpdateDto(TDto dto, TChild child)
            {
                dto.Id = child.Id;
            }
        }

        public static IReadOnlyList<T> UpdateManyToMany<T, TParent, TChild>(this IFolkeConnection connection, TParent parent, IReadOnlyList<T> currentValues, ICollection<int> newValueIds)
            where T : class, IManyToManyTable<TParent, TChild>, new()
            where TParent : IFolkeTable
            where TChild : class, IFolkeTable, new()
        {
            return connection.UpdateManyToMany(parent, currentValues, newValueIds, new FolkeTableManyToManyHelperWithIds<TChild>());
        }

        private class FolkeTableManyToManyHelperWithIds<TChild> :
            IManyToManyHelperConfiguration<TChild, int> where TChild : class, IFolkeTable, new()
        {
            public bool AreEqual(TChild child, int dto)
            {
                return child.Id == dto;
            }

            public TChild Map(int dto)
            {
                throw new NotImplementedException();
            }

            public IList<TChild> QueryExisting(IFolkeConnection connection, IList<int> dto)
            {
                return dto.Any() ? connection.QueryOver<TChild>().Where(c => c.Id.In(dto)).List() : null;
            }

            public void UpdateDto(int dto, TChild child)
            {
                throw new NotImplementedException();
            }
        }

        public static IReadOnlyList<T> UpdateManyToMany<T, TParent, TChild, TChildDto>(this IFolkeConnection connection, TParent parent, IReadOnlyList<T> currentValues, ICollection<TChildDto> newDtos, IManyToManyHelperConfiguration<TChild, TChildDto> helper)
            where T : class, IManyToManyTable<TParent, TChild>, new()
            where TParent : IFolkeTable
            where TChild : class, IFolkeTable, new()
        {
            if (newDtos == null)
                newDtos = new List<TChildDto>();

            // Looking for any value in newDtos that is not in currentValues
            var valuesToAdd = newDtos.Where(v => currentValues == null || !currentValues.Any(cv => helper.AreEqual(cv.Child, v))).ToList();
            var newValues = currentValues == null ? new List<T>() : currentValues.ToList();
            if (valuesToAdd.Any())
            {
                // Query from the database the values that needs to be added to the parent
                var existingChildren = helper.QueryExisting(connection, valuesToAdd);

                foreach (var newDto in valuesToAdd)
                {
                    var child = existingChildren == null ? null : existingChildren.SingleOrDefault(c => helper.AreEqual(c, newDto));
                    if (child == null)
                    {
                        // If the element to add does not exist in the database, create it (may fail if helper.Map/helper.UpdateDto is not implemented because one does not want to allow that)
                        child = helper.Map(newDto);
                        connection.Save(child);
                        helper.UpdateDto(newDto, child);
                    }

                    // Create a new parent-child link with this value
                    var newElement = new T { Child = child, Parent = parent };
                    connection.Save(newElement);
                    newValues.Add(newElement);
                }
            }
            
            if (currentValues != null)
            {
                // Look for values to remove
                var valuesToRemove = currentValues.Where(cv => newDtos.All(nv => !helper.AreEqual(cv.Child, nv))).ToArray();
                if (valuesToRemove.Any())
                {
                    connection.Delete<T>().From().Where(c => c.Id.In(valuesToRemove.Select(s => s.Id))).Execute();
                    foreach (var value in valuesToRemove)
                        newValues.Remove(value);
                }
            }
            return newValues;
        }
    }
}
