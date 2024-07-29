using Canvas_Note_Desktop.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Canvas_Note_Desktop.Builders.ContextMenuBuilder;

namespace Canvas_Note_Desktop.Factories
{
    internal class ContextMenuFactory
    {
        public static SubMenuBuilderParentless CreateMenuItemSubMenu(string header)
        {
            MenuItem item = new MenuItem()
            { Header = header };
            return new SubMenuBuilderParentless(item);
        }
    }
}
