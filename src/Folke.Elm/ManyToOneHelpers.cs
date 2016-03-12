using System;
using System.Collections.Generic;
using System.Linq;

namespace Folke.Elm
{
    public static class ManyToOneHelpers
    {
        /// <summary>
        /// Updates a collection of items from a collection of item views. Existing items are updated or deleted. New items are added.
        /// </summary>
        /// <typeparam name="TChild">The database item</typeparam>
        /// <typeparam name="TChildView">A view of this database item. Matching items have same Id</typeparam>
        /// <param name="connection">The database connection</param>
        /// <param name="currentValues">The current value from the database</param>
        /// <param name="newValues">The new values</param>
        /// <param name="factory">A factory that create a new item</param>
        /// <param name="updater">A delegate that updates an existing item</param>
        public static List<TChild> UpdateCollectionFromViews<TChild, TChildView>(this IFolkeConnection connection, IReadOnlyCollection<TChild> currentValues, IReadOnlyCollection<TChildView> newValues,
            Func<TChildView, TChild> factory, Func<TChildView, TChild, bool> updater)
            where TChild: class, IFolkeTable, new()
            where TChildView: class, IFolkeTable, new()
        {
            var ret = new List<TChild>();
            if (currentValues == null || !currentValues.Any())
            {
                foreach (var childValue in newValues)
                {
                    var child = factory(childValue);
                    ret.Add(child);
                    connection.Save(child);
                }
                return ret;
            }

            var newValueToAdd = newValues.Where(x => currentValues.All(y => y.Id != x.Id));
            foreach (var currentValue in currentValues)
            {
                var newValue = newValues.FirstOrDefault(x => x.Id == currentValue.Id);
                if (newValue == null)
                {
                    connection.Delete(currentValue);
                }
                else
                {
                    if (updater != null && updater(newValue, currentValue))
                        connection.Update(currentValue);
                    ret.Add(currentValue);
                }
            }

            foreach (var childDto in newValueToAdd)
            {
                var child = factory(childDto);
                ret.Add(child);
                connection.Save(child);
            }
            return ret;
        }
        /// <summary>
        /// Update a collection of items using a collection of values. These values don't have an id to identity them, so a areEqual delegate
        /// must be given as a parameter. Existing items that is not in newValues are deleted. 
        /// </summary>
        /// <typeparam name="TChild">The database item type</typeparam>
        /// <typeparam name="TChildView">The view type</typeparam>
        /// <param name="connection">The database connection</param>
        /// <param name="currentValues">The current value from the database</param>
        /// <param name="newValues">The new values</param>
        /// <param name="areEqual">Must return true if the two values are equal</param>
        /// <param name="factory">Create a new item</param>
        public static List<TChild> UpdateCollectionFromValues<TChild, TChildView>(this IFolkeConnection connection, IReadOnlyCollection<TChild> currentValues, IReadOnlyCollection<TChildView> newValues,
            Func<TChildView, TChild> factory, Func<TChildView, TChild, bool> areEqual)
            where TChild : class, IFolkeTable, new()
        {
            var ret = new List<TChild>();
            if (currentValues == null || !currentValues.Any())
            {
                foreach (var childValue in newValues)
                {
                    var child = factory(childValue);
                    connection.Save(child);
                    ret.Add(child);
                }
                return ret;    
            }

            var newValueToAdd = newValues.Where(x => currentValues.All(y => !areEqual(x, y)));
            foreach (var currentValue in currentValues)
            {
                if (!newValues.Any(x => areEqual(x, currentValue)))
                {
                    connection.Delete(currentValue);
                }
                else
                {
                    ret.Add(currentValue);
                }
            }

            foreach (var childDto in newValueToAdd)
            {
                var child = factory(childDto);
                connection.Save(child);
                ret.Add(child);
            }
            return ret;
        }
    }
}
