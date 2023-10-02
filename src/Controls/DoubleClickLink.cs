using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace GeneratorETWViewer.Controls
{
    internal class DoubleClickLink : Hyperlink
    {
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                if(this.Command is not null && this.Command.CanExecute(this.CommandParameter))
                {
                    this.Command.Execute(this.CommandParameter);
                }
            }
            //base.OnMouseLeftButtonDown(e);
        }
    }
}
