﻿#if !NETFX_CORE
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Diagnostics;

namespace Standard_Assets.Core.BestHTTP.PlatformSupport.Collections.Specialized
{
    public delegate void NotifyCollectionChangedEventHandler(object sender, PlatformSupport.Collections.Specialized.NotifyCollectionChangedEventArgs e);

    public interface INotifyCollectionChanged
    {
        event NotifyCollectionChangedEventHandler CollectionChanged;
    }

    /// <summary>
    /// This enum describes the action that caused a CollectionChanged event.
    /// </summary>
    public enum NotifyCollectionChangedAction
    {
        /// <summary> One or more items were added to the collection. </summary>
        Add,
        /// <summary> One or more items were removed from the collection. </summary>
        Remove,
        /// <summary> One or more items were replaced in the collection. </summary>
        Replace,
        /// <summary> One or more items were moved within the collection. </summary>
        Move,
        /// <summary> The contents of the collection changed dramatically. </summary>
        Reset,
    }

    /// <summary>
    /// Arguments for the CollectionChanged event.
    /// A collection that supports INotifyCollectionChangedThis raises this event
    /// whenever an item is added or removed, or when the contents of the collection
    /// changes dramatically.
    /// </summary>
    public class NotifyCollectionChangedEventArgs : EventArgs
    {
        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a reset change.
        /// </summary>
        /// <param name="action">The action that caused the event (must be Reset).</param>
        public NotifyCollectionChangedEventArgs(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action)
        {
            if (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("action");

            InitializeAdd(action, null, -1);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event; can only be Reset, Add or Remove action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        public NotifyCollectionChangedEventArgs(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, object changedItem)
        {
            if ((action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Add) && (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    && (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Reset))
                throw new ArgumentException("action");

            if (action == PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                if (changedItem != null)
                    throw new ArgumentException("action");

                InitializeAdd(action, null, -1);
            }
            else
            {
                InitializeAddOrRemove(action, new object[] { changedItem }, -1);
            }
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The index where the change occurred.</param>
        public NotifyCollectionChangedEventArgs(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, object changedItem, int index)
        {
            if ((action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Add) && (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    && (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Reset))
                throw new ArgumentException("action");

            if (action == PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                if (changedItem != null)
                    throw new ArgumentException("action");
                if (index != -1)
                    throw new ArgumentException("action");

                InitializeAdd(action, null, -1);
            }
            else
            {
                InitializeAddOrRemove(action, new object[] { changedItem }, index);
            }
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        public NotifyCollectionChangedEventArgs(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, IList changedItems)
        {
            if ((action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Add) && (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    && (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Reset))
                throw new ArgumentException("action");

            if (action == PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                if (changedItems != null)
                    throw new ArgumentException("action");

                InitializeAdd(action, null, -1);
            }
            else
            {
                if (changedItems == null)
                    throw new ArgumentNullException("changedItems");

                InitializeAddOrRemove(action, changedItems, -1);
            }
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change (or a reset).
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="startingIndex">The index where the change occurred.</param>
        public NotifyCollectionChangedEventArgs(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
        {
            if ((action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Add) && (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    && (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Reset))
                throw new ArgumentException("action");

            if (action == PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                if (changedItems != null)
                    throw new ArgumentException("action");
                if (startingIndex != -1)
                    throw new ArgumentException("action");

                InitializeAdd(action, null, -1);
            }
            else
            {
                if (changedItems == null)
                    throw new ArgumentNullException("changedItems");
                if (startingIndex < -1)
                    throw new ArgumentException("startingIndex");

                InitializeAddOrRemove(action, changedItems, startingIndex);
            }
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        public NotifyCollectionChangedEventArgs(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, object newItem, object oldItem)
        {
            if (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                throw new ArgumentException("action");

            InitializeMoveOrReplace(action, new object[] { newItem }, new object[] { oldItem }, -1, -1);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        /// <param name="index">The index of the item being replaced.</param>
        public NotifyCollectionChangedEventArgs(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
        {
            if (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                throw new ArgumentException("action");

            InitializeMoveOrReplace(action, new object[] { newItem }, new object[] { oldItem }, index, index);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        public NotifyCollectionChangedEventArgs(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, IList newItems, IList oldItems)
        {
            if (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                throw new ArgumentException("action");
            if (newItems == null)
                throw new ArgumentNullException("newItems");
            if (oldItems == null)
                throw new ArgumentNullException("oldItems");

            InitializeMoveOrReplace(action, newItems, oldItems, -1, -1);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        /// <param name="startingIndex">The starting index of the items being replaced.</param>
        public NotifyCollectionChangedEventArgs(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex)
        {
            if (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                throw new ArgumentException("action");
            if (newItems == null)
                throw new ArgumentNullException("newItems");
            if (oldItems == null)
                throw new ArgumentNullException("oldItems");

            InitializeMoveOrReplace(action, newItems, oldItems, startingIndex, startingIndex);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Move event.
        /// </summary>
        /// <param name="action">Can only be a Move action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The new index for the changed item.</param>
        /// <param name="oldIndex">The old index for the changed item.</param>
        public NotifyCollectionChangedEventArgs(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex)
        {
            if (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Move)
                throw new ArgumentException("action");
            if (index < 0)
                throw new ArgumentException("index");

            object[] changedItems = new object[] { changedItem };
            InitializeMoveOrReplace(action, changedItems, changedItems, index, oldIndex);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Move event.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="index">The new index for the changed items.</param>
        /// <param name="oldIndex">The old index for the changed items.</param>
        public NotifyCollectionChangedEventArgs(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex)
        {
            if (action != PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Move)
                throw new ArgumentException("action");
            if (index < 0)
                throw new ArgumentException("index");

            InitializeMoveOrReplace(action, changedItems, changedItems, index, oldIndex);
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs with given fields (no validation). Used by WinRT marshaling.
        /// </summary>
        internal NotifyCollectionChangedEventArgs(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, IList newItems, IList oldItems, int newIndex, int oldIndex)
        {
            _action = action;
            _newItems = (newItems == null) ? null : new ReadOnlyList(newItems);
            _oldItems = (oldItems == null) ? null : new ReadOnlyList(oldItems);
            _newStartingIndex = newIndex;
            _oldStartingIndex = oldIndex;
        }

        private void InitializeAddOrRemove(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
        {
            if (action == PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Add)
                InitializeAdd(action, changedItems, startingIndex);
            else if (action == PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                InitializeRemove(action, changedItems, startingIndex);
            else
                Debug.Assert(false, String.Format("Unsupported action: {0}", action.ToString()));
        }

        private void InitializeAdd(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, IList newItems, int newStartingIndex)
        {
            _action = action;
            _newItems = (newItems == null) ? null : new ReadOnlyList(newItems);
            _newStartingIndex = newStartingIndex;
        }

        private void InitializeRemove(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, IList oldItems, int oldStartingIndex)
        {
            _action = action;
            _oldItems = (oldItems == null) ? null : new ReadOnlyList(oldItems);
            _oldStartingIndex = oldStartingIndex;
        }

        private void InitializeMoveOrReplace(PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex, int oldStartingIndex)
        {
            InitializeAdd(action, newItems, startingIndex);
            InitializeRemove(action, oldItems, oldStartingIndex);
        }

        //------------------------------------------------------
        //
        //  Public Properties
        //
        //------------------------------------------------------

        /// <summary>
        /// The action that caused the event.
        /// </summary>
        public PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction Action
        {
            get { return _action; }
        }

        /// <summary>
        /// The items affected by the change.
        /// </summary>
        public IList NewItems
        {
            get { return _newItems; }
        }

        /// <summary>
        /// The old items affected by the change (for Replace events).
        /// </summary>
        public IList OldItems
        {
            get { return _oldItems; }
        }

        /// <summary>
        /// The index where the change occurred.
        /// </summary>
        public int NewStartingIndex
        {
            get { return _newStartingIndex; }
        }

        /// <summary>
        /// The old index where the change occurred (for Move events).
        /// </summary>
        public int OldStartingIndex
        {
            get { return _oldStartingIndex; }
        }

        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        private PlatformSupport.Collections.Specialized.NotifyCollectionChangedAction _action;
        private IList _newItems, _oldItems;
        private int _newStartingIndex = -1;
        private int _oldStartingIndex = -1;
    }

    internal sealed class ReadOnlyList : IList
    {
        private readonly IList _list;

        internal ReadOnlyList(IList list)
        {
            Debug.Assert(list != null);

            _list = list;
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool IsFixedSize
        {
            get { return true; }
        }

        public bool IsSynchronized
        {
            get { return _list.IsSynchronized; }
        }

        public object this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public object SyncRoot
        {
            get { return _list.SyncRoot; }
        }

        public int Add(object value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(object value)
        {
            return _list.Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            _list.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(object value)
        {
            return _list.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        public void Remove(object value)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
    }
}

#endif