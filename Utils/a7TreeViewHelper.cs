using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Threading;
using System.Windows.Threading;

namespace a7DocumentDbStudio.Utils
{
    public static class a7TreeViewHelper
    {
        //
        // The TreeViewItem that the mouse is currently directly over (or null).
        //
        private static TreeViewItem _currentItem = null;

        //
        // IsMouseDirectlyOverItem:  A DependencyProperty that will be true only on the
        // TreeViewItem that the mouse is directly over.  I.e., this won't be set on that
        // parent item.
        //
        // This is the only public member, and is read-only.
        //

        // The property key (since this is a read-only DP)
        private static readonly DependencyPropertyKey IsMouseDirectlyOverItemKey =
            DependencyProperty.RegisterAttachedReadOnly(
                      "IsMouseDirectlyOverItem",
                      typeof(bool),
                      typeof(a7TreeViewHelper),
                      new FrameworkPropertyMetadata(null,
                            new CoerceValueCallback(CalculateIsMouseDirectlyOverItem)));

        // The DP itself
        public static readonly DependencyProperty IsMouseDirectlyOverItemProperty =
            IsMouseDirectlyOverItemKey.DependencyProperty;

        // A strongly-typed getter for the property.
        public static bool GetIsMouseDirectlyOverItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMouseDirectlyOverItemProperty);
        }

        // A coercion method for the property
        private static object CalculateIsMouseDirectlyOverItem(DependencyObject item, object value)
        {
            // This method is called when the IsMouseDirectlyOver property is being calculated
            // for a TreeViewItem. 

            if (item == _currentItem)
                return true;
            else
                return false;
        }

        //
        // UpdateOverItem:  A private RoutedEvent used to find the nearest encapsulating
        // TreeViewItem to the mouse's current position.
        //

        private static readonly RoutedEvent UpdateOverItemEvent = EventManager.RegisterRoutedEvent(
            "UpdateOverItem", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(a7TreeViewHelper));

        //
        // Class constructor
        //

        static a7TreeViewHelper()
        {
            // Get all Mouse enter/leave events for TreeViewItem.
            EventManager.RegisterClassHandler(typeof(TreeViewItem),
                                      TreeViewItem.MouseEnterEvent,
                                      new MouseEventHandler(OnMouseTransition), true);
            EventManager.RegisterClassHandler(typeof(TreeViewItem),
                                      TreeViewItem.MouseLeaveEvent,
                                      new MouseEventHandler(OnMouseTransition), true);

            // Listen for the UpdateOverItemEvent on all TreeViewItem's.
            EventManager.RegisterClassHandler(typeof(TreeViewItem),
                                      UpdateOverItemEvent,
                                      new RoutedEventHandler(OnUpdateOverItem));
        }


        //
        // OnUpdateOverItem:  This method is a listener for the UpdateOverItemEvent.  When it is received,
        // it means that the sender is the closest TreeViewItem to the mouse (closest in the sense of the
        // tree, not geographically).

        static void OnUpdateOverItem(object sender, RoutedEventArgs args)
        {
            // Mark this object as the tree view item over which the mouse
            // is currently positioned.
            _currentItem = sender as TreeViewItem;

            // Tell that item to re-calculate the IsMouseDirectlyOverItem property
            _currentItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);

            // Prevent this event from notifying other tree view items higher in the tree.
            args.Handled = true;
        }

        //
        // OnMouseTransition:  This method is a listener for both the MouseEnter event and
        // the MouseLeave event on TreeViewItems.  It updates the _currentItem, and updates
        // the IsMouseDirectlyOverItem property on the previous TreeViewItem and the new
        // TreeViewItem.

        static void OnMouseTransition(object sender, MouseEventArgs args)
        {
            lock (IsMouseDirectlyOverItemProperty)
            {
                if (_currentItem != null)
                {
                    // Tell the item that previously had the mouse that it no longer does.
                    DependencyObject oldItem = _currentItem;
                    _currentItem = null;
                    oldItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);
                }

                // Get the element that is currently under the mouse.
                IInputElement currentPosition = Mouse.DirectlyOver;

                // See if the mouse is still over something (any element, not just a tree view item).
                if (currentPosition != null)
                {
                    // Yes, the mouse is over something.
                    // Raise an event from that point.  If a TreeViewItem is anywhere above this point
                    // in the tree, it will receive this event and update _currentItem.

                    RoutedEventArgs newItemArgs = new RoutedEventArgs(UpdateOverItemEvent);
                    currentPosition.RaiseEvent(newItemArgs);

                }
            }
        }



        #region extension methods
    
            public static TreeViewItem ContainerFromItem(this TreeView treeView, object item)
            {
                //todo:needed?
                //if (treeView.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.NotStarted)
                //{
                //    IItemContainerGenerator generator = treeView.ItemContainerGenerator;
                //    GeneratorPosition position = generator.GeneratorPositionFromIndex(0);
                //    using (generator.StartAt(position, GeneratorDirection.Forward, true))
                //    {
                //        foreach (object o in treeView.Items)
                //        {
                //            DependencyObject dp = generator.GenerateNext();
                //            generator.PrepareItemContainer(dp);
                //        }
                //    }
                //}
                TreeViewItem containerThatMightContainItem = (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromItem(item);
                if (containerThatMightContainItem != null)
                    return containerThatMightContainItem;
                else
                    return ContainerFromItem(treeView.ItemContainerGenerator, treeView.Items, item);
            }

            private static TreeViewItem ContainerFromItem(ItemContainerGenerator parentItemContainerGenerator, ItemCollection itemCollection, object item)
            {
                //todo:needed?
                //if (parentItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.NotStarted)
                //{
                //    IItemContainerGenerator generator = parentItemContainerGenerator;
                //    GeneratorPosition position = generator.GeneratorPositionFromIndex(0);
                //    using (generator.StartAt(position, GeneratorDirection.Forward, true))
                //    {
                //        foreach (object o in itemCollection)
                //        {
                //            DependencyObject dp = generator.GenerateNext();
                //            generator.PrepareItemContainer(dp);
                //        }
                //    }
                //}
                foreach (object curChildItem in itemCollection)
                {
                    TreeViewItem parentContainer = (TreeViewItem)parentItemContainerGenerator.ContainerFromItem(curChildItem);
                    if (parentContainer != null)
                    {
                        TreeViewItem containerThatMightContainItem = (TreeViewItem)parentContainer.ItemContainerGenerator.ContainerFromItem(item);

                        if (containerThatMightContainItem != null)
                            return containerThatMightContainItem;
                        TreeViewItem recursionResult = ContainerFromItem(parentContainer.ItemContainerGenerator, parentContainer.Items, item);
                        if (recursionResult != null)
                            return recursionResult;
                    }
                }
                return null;
            }

            public static object ItemFromContainer(this TreeView treeView, TreeViewItem container)
            {
                TreeViewItem itemThatMightBelongToContainer = (TreeViewItem)treeView.ItemContainerGenerator.ItemFromContainer(container);
                if (itemThatMightBelongToContainer != null)
                    return itemThatMightBelongToContainer;
                else
                    return ItemFromContainer(treeView.ItemContainerGenerator, treeView.Items, container);
            }

            private static object ItemFromContainer(ItemContainerGenerator parentItemContainerGenerator, ItemCollection itemCollection, TreeViewItem container)
            {
                foreach (object curChildItem in itemCollection)
                {
                    TreeViewItem parentContainer = (TreeViewItem)parentItemContainerGenerator.ContainerFromItem(curChildItem);
                    TreeViewItem itemThatMightBelongToContainer = (TreeViewItem)parentContainer.ItemContainerGenerator.ItemFromContainer(container);
                    if (itemThatMightBelongToContainer != null)
                        return itemThatMightBelongToContainer;
                    TreeViewItem recursionResult = ItemFromContainer(parentContainer.ItemContainerGenerator, parentContainer.Items, container) as TreeViewItem;
                    if (recursionResult != null)
                        return recursionResult;
                }
                return null;
            }
    
        #endregion



            /// <summary>
            /// Expands all children of a TreeView
            /// </summary>
            /// <param name="treeView">The TreeView whose children will be expanded</param>
            public static void ExpandAll(this TreeView treeView)
            {
                ExpandSubContainers(treeView, false);
            }

            public static void ExpandAll2(this TreeView treeView)
            {
                ApplyActionToAllTreeViewItems(treeView, itemsControl =>
                {
                    itemsControl.IsExpanded = true;
                    a7UIHelper.WaitForPriority(DispatcherPriority.ContextIdle);
                });
            }

            /// <summary>
            /// Expands all children of a TreeView or TreeViewItem
            /// </summary>
            /// <param name="parentContainer">The TreeView or TreeViewItem containing the children to expand</param>
            internal static void ExpandSubContainers(this ItemsControl parentContainer, bool preserveState)
            {
                foreach (object item in parentContainer.Items)
                {
                    TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                    if (currentContainer != null && currentContainer.Items.Count > 0)
                    {
                        //expand the item
                        if (preserveState)
                        {
                            currentContainer.Tag = currentContainer.IsExpanded;
                        }
                        currentContainer.IsExpanded = true;

                        //if the item's children are not generated, they must be expanded
                        if (currentContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                        {
                            //store the event handler in a variable so we can remove it (in the handler itself)
                            EventHandler eh = null;
                            eh = new EventHandler(delegate
                            {
                                //once the children have been generated, expand those children's children then remove the event handler
                                if (currentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                                {
                                    ExpandSubContainers(currentContainer, preserveState);
                                    currentContainer.ItemContainerGenerator.StatusChanged -= eh;
                                    if (preserveState)
                                    {
                                        currentContainer.IsExpanded = (bool)currentContainer.Tag;
                                    }
                                }
                            });

                            currentContainer.ItemContainerGenerator.StatusChanged += eh;
                        }
                        else //otherwise the children have already been generated, so we can now expand those children
                        {
                            ExpandSubContainers(currentContainer, preserveState);
                        }
                    }
                }
            }

            /// <summary>
            /// Searches a TreeView for the provided object and selects it if found
            /// </summary>
            /// <param name="treeView">The TreeView containing the item</param>
            /// <param name="item">The item to search and select</param>
            public static void SelectItem(this TreeView treeView, object item)
            {
                ExpandAndSelectItem(treeView, item, false);
            }

            /// <summary>
            /// Searches a TreeView for the provided object and selects it if found
            /// </summary>
            /// <param name="treeView">The TreeView containing the item</param>
            /// <param name="item">The item to search and select</param>
            public static void SelectValue(this TreeView treeView, object value)
            {
                ExpandAndSelectItem(treeView, value, true);
            }

            static void ExpandAndSelectItem(TreeView treeView, object obj, bool isValue)
            {
                ExpandAndSelectItem(treeView, obj, isValue, null, null);
            }

            internal static void ExpandAndSelectItem(TreeView treeView, object obj, bool isValue, Action<TreeViewItem> beforeSelection, Action<TreeViewItem> afterSelection)
            {
                if (treeView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                {
                    EventHandler eh = null;
                    eh = new EventHandler(delegate
                    {
                        if (treeView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                        {
                            ExpandAndSelectItem(treeView, obj, isValue, treeView.SelectedValuePath, beforeSelection, afterSelection);

                            //remove the StatusChanged event handler since we just handled it (we only needed it once)
                            treeView.ItemContainerGenerator.StatusChanged -= eh;
                        }
                    });

                    treeView.ItemContainerGenerator.StatusChanged += eh;
                }
                else
                {
                    ExpandAndSelectItem(treeView, obj, isValue, treeView.SelectedValuePath, beforeSelection, afterSelection);
                }
            }

            /// <summary>
            /// Finds the provided object in an ItemsControl's children and selects it
            /// </summary>
            /// <param name="parentContainer">The parent container whose children will be searched for the selected item</param>
            /// <param name="itemToSelect">The item to select</param>
            /// <returns>True if the item is found and selected, false otherwise</returns>
            private static bool ExpandAndSelectItem(ItemsControl parentContainer, object itemToSelect, bool isValue, string valuePath, Action<TreeViewItem> beforeSelection, Action<TreeViewItem> afterSelection)
            {
                //check all items at the current level
                foreach (object item in parentContainer.Items)
                {
                    TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                    object equalItem = item;
                    if (isValue)
                    {
                        equalItem = BindingHelper.Eval<object>(item, valuePath);
                    }

                    //if the data item matches the item we want to select, set the corresponding
                    //TreeViewItem IsSelected to true
                    if (currentContainer != null)
                    {
                        if (equalItem.Equals(itemToSelect))
                        {
                            if (beforeSelection != null)
                            {
                                beforeSelection(currentContainer);
                            }

                            currentContainer.IsSelected = true;

                            currentContainer.BringIntoView();
                            currentContainer.Focus();
                            if (afterSelection != null)
                            {
                                afterSelection(currentContainer);
                            }

                            //the item was found
                            return true;
                        }
                        else if (currentContainer.IsSelected)
                        {
                            currentContainer.IsSelected = false;
                        }
                    }
                }

                //if we get to this point, the selected item was not found at the current level, so we must check the children
                foreach (object item in parentContainer.Items)
                {
                    TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                    //if children exist
                    if (currentContainer != null && currentContainer.Items.Count > 0)
                    {
                        //keep track of if the TreeViewItem was expanded or not
                        bool wasExpanded = currentContainer.IsExpanded;

                        //expand the current TreeViewItem so we can check its child TreeViewItems
                        currentContainer.IsExpanded = true;

                        //if the TreeViewItem child containers have not been generated, we must listen to
                        //the StatusChanged event until they are
                        if (currentContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                        {
                            //store the event handler in a variable so we can remove it (in the handler itself)
                            EventHandler eh = null;
                            eh = new EventHandler(delegate
                            {
                                if (currentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                                {
                                    if (ExpandAndSelectItem(currentContainer, itemToSelect, isValue, valuePath, beforeSelection, afterSelection) == false)
                                    {
                                        //The assumption is that code executing in this EventHandler is the result of the parent not
                                        //being expanded since the containers were not generated.
                                        //since the itemToSelect was not found in the children, collapse the parent since it was previously collapsed
                                        currentContainer.IsExpanded = false;
                                    }

                                    //remove the StatusChanged event handler since we just handled it (we only needed it once)
                                    currentContainer.ItemContainerGenerator.StatusChanged -= eh;
                                }
                            });
                            currentContainer.ItemContainerGenerator.StatusChanged += eh;
                        }
                        else //otherwise the containers have been generated, so look for item to select in the children
                        {
                            if (ExpandAndSelectItem(currentContainer, itemToSelect, isValue, valuePath, beforeSelection, afterSelection) == false)
                            {
                                //restore the current TreeViewItem's expanded state
                                currentContainer.IsExpanded = wasExpanded;
                            }
                            else //otherwise the node was found and selected, so return true
                            {
                                return true;
                            }
                        }
                    }
                }

                //no item was found
                return false;
            }

            public static void ApplyActionToAllTreeViewItems(this ItemsControl itemsControl, Action<TreeViewItem> itemAction)
            {
                Stack<ItemsControl> itemsControlStack = new Stack<ItemsControl>();
                itemsControlStack.Push(itemsControl);

                while (itemsControlStack.Count != 0)
                {
                    ItemsControl currentItem = itemsControlStack.Pop() as ItemsControl;
                    TreeViewItem currentTreeViewItem = currentItem as TreeViewItem;
                    if (currentTreeViewItem != null)
                    {
                        itemAction(currentTreeViewItem);
                    }
                    if (currentItem != null) // this handles the scenario where some TreeViewItems are already collapsed
                    {
                        foreach (object dataItem in currentItem.Items)
                        {
                            ItemsControl childElement = (ItemsControl)currentItem.ItemContainerGenerator.ContainerFromItem(dataItem);

                            itemsControlStack.Push(childElement);
                        }
                    }
                }
            }

            public static void SelectOne(this TreeView treeView, object item, bool isValue)
            {
                ApplyActionToAllTreeViewItems(treeView, itemsControl =>
                {
                    object equalItem = itemsControl.Header;
                    if (isValue)
                    {
                        equalItem = BindingHelper.Eval<object>(equalItem, treeView.SelectedValuePath);
                    }

                    if (equalItem.Equals(item))
                    {
                        itemsControl.IsSelected = true;
                        itemsControl.BringIntoView();
                        itemsControl.Focus();

                        return;
                    }
                    else
                    {
                        itemsControl.IsSelected = false;
                    }

                    bool wasExpanded = itemsControl.IsExpanded;
                    itemsControl.IsExpanded = true;

                    a7UIHelper.WaitForPriority(DispatcherPriority.ContextIdle);

                    itemsControl.IsExpanded = wasExpanded;

                });
            }

    }
}
