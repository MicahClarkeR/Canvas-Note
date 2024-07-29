using Canvas_Note_Desktop.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Canvas_Note_Desktop.Builders
{
    internal class ContextMenuBuilder
    {
        private ContextMenu _menu = new ContextMenu();

        public ContextMenuBuilder()
        {

        }

        public ContextMenuBuilder Add(string header, Action<object, RoutedEventArgs> callback)
        {
            MenuItem item = CreateMenuItem(header, callback);
            _menu.Items.Add(item);

            return this;
        }

        public ContextMenuBuilder Add(ContextMenu menu)
        {
            _menu.Items.Add(menu);
            return this;
        }

        public ContextMenuBuilder Add(MenuItem item)
        {
            _menu.Items.Add(item);
            return this;
        }

        public ContextMenuBuilder Add(SubMenuBuilder builder)
        {
            builder.BuildAndReturn(this);
            return this;
        }

        public ContextMenu Build()
        {
            return _menu;
        }

        public SubMenuBuilder CreateSubMenu(string header)
        {
            MenuItem item = new MenuItem()
            {
                Header = header
            };

            return new SubMenuBuilder(this, item);
        }

        private static MenuItem CreateMenuItem(string header, Action<object, RoutedEventArgs> callback)
        {
            MenuItem item = new MenuItem()
            {
                Header = header
            };

            item.Click += callback.Invoke;

            return item;
        }

        internal class SubMenuBuilder
        {
            private readonly ContextMenuBuilder _parent;
            private readonly MenuItem _item;

            public SubMenuBuilder(ContextMenuBuilder parent, MenuItem item)
            {
                _parent = parent;
                _item = item;
            }

            public SubMenuBuilder Add(string header, Action<object, RoutedEventArgs> callback)
            {
                MenuItem item = CreateMenuItem(header, callback);
                return Add(item);
            }

            public SubMenuBuilder Add(MenuItem item)
            {
                _item.Items.Add(item);

                return this;
            }

            public ContextMenuBuilder BuildAndReturn()
            {
                _parent.Add(_item);
                return _parent;
            }

            public ContextMenuBuilder BuildAndReturn(ContextMenuBuilder parent)
            {
                parent.Add(_item);
                return _parent;
            }
        }

        internal class SubMenuBuilderParentless
        {
            private readonly MenuItem _item;

            public SubMenuBuilderParentless(MenuItem item)
            {
                _item = item;
            }

            public SubMenuBuilderParentless Add(string header, Action<object, RoutedEventArgs> callback)
            {
                MenuItem item = CreateMenuItem(header, callback);
                return Add(item);
            }

            public SubMenuBuilderParentless Add(MenuItem item)
            {
                _item.Items.Add(item);
                return this;
            }

            public MenuItem Build() => _item;

            public SubMenuBuilderParentless CreateSubMenu(string header) => ContextMenuFactory.CreateMenuItemSubMenu(header);
        }
    }
}
