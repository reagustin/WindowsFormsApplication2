using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication2
{
    public class Elemento
    {
        public Producto prod { get; set; }
        public int cantidad { get; set; }

        public Elemento()
        {
            prod = new Producto();
        }
    }
}
