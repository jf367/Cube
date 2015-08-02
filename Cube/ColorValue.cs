namespace Cube
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.UI;

    internal class ColorValue
    {
        public ColorValue(Color color, string name)
        {
            this.Color = color;
            this.Name = name;
        }

        public Color Color { get; private set; }
        public string Name { get; private set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
