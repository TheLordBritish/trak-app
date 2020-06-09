using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Sparky.TrakApp.Common
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        public ObservableRangeCollection()
        {
        }
        
        public ObservableRangeCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public void AddRange(IEnumerable<T> collection,
            NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
        {
            if (notificationMode != NotifyCollectionChangedAction.Add &&
                notificationMode != NotifyCollectionChangedAction.Reset)
            {
                throw new ArgumentException("Mode must be either Add or Reset for AddRange.", nameof(notificationMode));
            }

            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            CheckReentrancy();

            var startIndex = Count;

            var enumerable = collection as T[] ?? collection.ToArray();
            var itemsAdded = AddArrangeCore(enumerable);

            if (!itemsAdded)
                return;

            if (notificationMode == NotifyCollectionChangedAction.Reset)
            {
                RaiseChangeNotificationEvents(NotifyCollectionChangedAction.Reset);
                return;
            }

            var changedItems = collection is List<T> list ? list : new List<T>(enumerable);

            RaiseChangeNotificationEvents(
                NotifyCollectionChangedAction.Add,
                changedItems,
                startIndex);
        }

        public void RemoveRange(IEnumerable<T> collection,
            NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Reset)
        {
            if (notificationMode != NotifyCollectionChangedAction.Remove &&
                notificationMode != NotifyCollectionChangedAction.Reset)
            {
                throw new ArgumentException("Mode must be either Remove or Reset for RemoveRange.",
                    nameof(notificationMode));
            }
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            CheckReentrancy();

            if (notificationMode == NotifyCollectionChangedAction.Reset)
            {
                var raiseEvents = false;
                foreach (var item in collection)
                {
                    Items.Remove(item);
                    raiseEvents = true;
                }

                if (raiseEvents)
                {
                    RaiseChangeNotificationEvents(NotifyCollectionChangedAction.Reset);
                }

                return;
            }

            var changedItems = new List<T>(collection);
            for (var i = 0; i < changedItems.Count; i++)
            {
                if (Items.Remove(changedItems[i]))
                {
                    continue;
                }

                changedItems.RemoveAt(i);
                i--;
            }

            if (changedItems.Count == 0)
            {
                return;
            }

            RaiseChangeNotificationEvents(
                NotifyCollectionChangedAction.Remove,
                changedItems);
        }

        public void Replace(T item) => ReplaceRange(new[] {item});

        public void ReplaceRange(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            CheckReentrancy();

            var previouslyEmpty = Items.Count == 0;

            Items.Clear();

            AddArrangeCore(collection);

            var currentlyEmpty = Items.Count == 0;

            if (previouslyEmpty && currentlyEmpty)
            {
                return;
            }

            RaiseChangeNotificationEvents(NotifyCollectionChangedAction.Reset);
        }

        private bool AddArrangeCore(IEnumerable<T> collection)
        {
            var itemAdded = false;
            foreach (var item in collection)
            {
                Items.Add(item);
                itemAdded = true;
            }

            return itemAdded;
        }

        private void RaiseChangeNotificationEvents(NotifyCollectionChangedAction action, IList changedItems = null,
            int startingIndex = -1)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

            OnCollectionChanged(changedItems is null
                ? new NotifyCollectionChangedEventArgs(action)
                : new NotifyCollectionChangedEventArgs(action, changedItems, startingIndex));
        }
    }
}