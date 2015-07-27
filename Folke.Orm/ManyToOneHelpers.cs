using System;
using System.Collections.Generic;
using System.Linq;

namespace Folke.Orm
{
    public static class ManyToOneHelpers
    {
        public static void UpdateCollection<TChild, TChildView>(this IFolkeConnection connection, IEnumerable<TChild> currentValues, IEnumerable<TChildView> newValues,
            Func<TChildView, TChild> factory, Func<TChildView, TChild, bool> updater)
            where TChild: class, IFolkeTable, new()
            where TChildView: class, IFolkeTable, new()
        {
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
                    if (updater(newValue, currentValue))
                        connection.Update(currentValue);
                }
            }

            foreach (var childDto in newValueToAdd)
            {
                var child = factory(childDto);
                connection.Save(child);
            }
        }
    }
}
