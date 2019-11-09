using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApplication2
{
    public partial class FrmTP1 : Form
    {
        string rutaarchivo = @"C:\TP\stock.txt";
        string rutapedidohistorico = @"C:\TP\pedidoscodigohistorico.txt";
        string rutapedidodiario = @"C:\TP\pedidos.txt";
        Empresa EmpresaInstanciada = new Empresa();
        Pedido PedidoActual = new Pedido();
        List<string> ListaPedidosTemporal = new List<string>();
        Pedido PedidoLoad = new Pedido(); // lo necesito para poder levantar los pedidos acumulados del txt que actua como base de datos
        public FrmTP1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBox1.Text = openFileDialog1.FileName;
            
            
        }
        
    

        private void btnProcesar_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty)
            {
                if (RdnRecepcionPedido.Checked)
                {
                    ProcesaPedido(textBox1.Text);
                }
                else if (rdnRecepcionStock.Checked)
                {
                    ProcesaStock(textBox1.Text);
                }
                else if (rdnRecepcionLogistica.Checked)
                { }
                else if (RdnEnvioLogistica.Checked)
                { }                
            }
        }

        private void ProcesaPedido(string Ruta) //recordar tomar el codigo de la ruta ALE
        {
            
            int nrolinea = 0;            
            string[] lineas = null;
            string[] nombrepedidoruta = null;
            string[] remueveExtension = null;
            Elemento Element;
            if (File.Exists(Ruta))
            {
                nombrepedidoruta = Ruta.Split('_');
                remueveExtension = nombrepedidoruta[1].Split('.'); /// VER CON ALE ACA PORQUE NO REMUEVE EL .TXT
                if (ListaPedidosTemporal.Contains(remueveExtension[0]))
                {
                    MessageBox.Show("Ya esta cargado este pedido", "Atencion");
                }
                else
                {                 
                lineas = File.ReadAllLines(Ruta);
                string[] lineasplit = null;
                    foreach (string linea in lineas)
                    {
                        if (nrolinea == 0) // donde reconoce que es la linea 0 ?????? // para que cargo todo el pedido en un list si despues lo voy a poner en un txt??
                        {
                            lineasplit = linea.Split(';');
                            PedidoActual.codigo = remueveExtension[0];
                            PedidoActual.comercio.codigo = lineasplit[0];
                            PedidoActual.comercio.razonsocial = lineasplit[1];
                            PedidoActual.comercio.cuit = lineasplit[2];
                            PedidoActual.comercio.domicilio = lineasplit[3];
                            nrolinea = nrolinea + 1;
                            //using (StreamWriter sw = File.AppendText(rutapedidodiario))
                            //{
                            //    sw.WriteLine(PedidoActual.comercio.codigo + ";" + PedidoActual.comercio.razonsocial + ";" + PedidoActual.comercio.cuit + ";" + PedidoActual.comercio.domicilio);
                            //}
                        }
                        else
                        {
                            Element = new Elemento();
                            lineasplit = linea.Split(';');
                            Element.prod.idprod = lineasplit[0];
                            Element.cantidad = Convert.ToInt32(lineasplit[1]);
                            PedidoActual.GuardarPedido(Element);
                            //using (StreamWriter sw = File.AppendText(rutapedidodiario))
                            //{
                            //    sw.WriteLine(Element.prod.idprod + ";" + Element.cantidad);
                            //}
                        }
                        

                    }
                    using (StreamWriter sw = File.AppendText(rutapedidohistorico))
                    {
                        sw.WriteLine("\n" + remueveExtension[0]);
                    }
                }
                GrabarPedidosTxt();

            }
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortTimeString() + " - Pedido Procesado");            

        }

        /// <summary>
        /// VER CON ALE XQ EN VEZ DE GRABAR LOS PEDIDOS CON UN METODO LO ESTOY HACIENDO DIRECTAMENTE ANTES EN EL METODO PROCESA PEDIDO
        /// 
        /// </summary>
        private void GrabarPedidosTxt()
        {
            if (File.Exists(rutapedidodiario))         
            {
                File.Delete(rutapedidodiario);

                using (StreamWriter sw = File.CreateText(rutapedidodiario))
                {

                    foreach (Pedido PedidoActual in EmpresaInstanciada.pedidos)
                    {
                        sw.WriteLine(PedidoActual.comercio.codigo + ";" + PedidoActual.comercio.razonsocial + ";" + PedidoActual.comercio.cuit + ";" + PedidoActual.comercio.domicilio);


                        foreach (Elemento elementopedido in PedidoActual.listaproducto)
                        {
                            sw.WriteLine(elementopedido.prod.idprod + ";" + elementopedido.cantidad);
                        }
                    }

                }

            }
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortTimeString() + " - Se grabo el archivo stock");

        }




        private void ProcesaStock(string Ruta)
        {
            
            // Este es uno de los archivos por fuera de los que diseño el profesor, es un archivo con 
            // la produccion de mi planta con el que debo actualizar mi archivo de stock.

            // Punto 1: realizar la carga en una lista de lo que recibo de la planta, esta lista es lo que luego va a actualizar el stock.
            // Punto 2: tengo que realizar la carga en una lista del stock actual total. (acumula lo que tengo en stock, es mi STOCK)
            // Punto 3: recorrer la lista del punto1, lo busco en la lista del punto2, y lo actualizo.
            List<Elemento> listaproduccion = new List<Elemento>();
            string[] lineas = null;
            Elemento Element;
            if (File.Exists(Ruta))
            {
                
                lineas = File.ReadAllLines(Ruta);
                string[] lineasplit = null;
                foreach (string linea in lineas)
                {
                    //aca empiezo a guardar cada contenido del array en el list de elementos acorde a mi stock (llamo a los objeto elemento, les pongo lo que corresponde
                    // y luego es lo guardo en un metodo en la clase Empresa dentro de la lista Stock! voila
                    Element = new Elemento();
                    lineasplit = linea.Split(';');
                    Element.prod.idprod = lineasplit[0];  
                    Element.cantidad = Convert.ToInt32(lineasplit[1]);

                    listaproduccion.Add(Element);
                    
                    
                }
                
            }
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortTimeString() + " - Se lee archivo de produccion");

            //en el punto anterior chupe lo que tenia en el txt que viene de planta y guarde en una lista todo en memoria
            // a continuacion voy a buscar los elementos de la lista del punto anterior a ver si existe en mi stock actual, 
            // recorda que en el form load el stock actual siempre se carga en memoria, y abajo esta lista para ser recorrido,
            // busco si los elementos del punto anterior estan presentes en mi stock, si es asi, solo acumulo la cantidad, de lo contrario agrego una linea.

            foreach (Elemento elemprod in listaproduccion)
            {
                Elemento ElementoEncontrado = (Elemento)EmpresaInstanciada.stock.Find(x => x.prod.idprod == (elemprod.prod.idprod));
                if (ElementoEncontrado != null)
                { 
                ElementoEncontrado.cantidad = ElementoEncontrado.cantidad + elemprod.cantidad; // aca adiciona al stock los items que existen
                }
                else
                {
                    EmpresaInstanciada.GuardarStock(elemprod); // aca guarda items nuevos 
                }
            }
            GrabarStockTxt(); // aca vuelca al txt lo que tiene en memoria de la lista resultante actualizada del foreach anterior.
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortTimeString() + " - Se actualizo stock");


        }

        private void GrabarStockTxt()
        {
            if (File.Exists(rutaarchivo))         //Verifica que no exista la ruta con anteriorirdad. O verifica que el archivo existe en la ruta indicada?
            {
                if (EmpresaInstanciada.stock.Count > 0)
                {
                    using (StreamWriter sw = File.CreateText(rutaarchivo)) 
                    {
                        foreach (Elemento elementoarchivo in EmpresaInstanciada.stock)
                        {
                            sw.WriteLine(elementoarchivo.prod.idprod + ";" + elementoarchivo.cantidad);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Error","No tiene informacion de stock");
                    ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortTimeString() + " - Error procesando archivo");
                }
            }
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortTimeString() + " - Se grabo el archivo stock");
        }

        private void FrmTP1_Load(object sender, EventArgs e)
        {
            
            string[] lineas = null;
            string[] pedhistlineas = null;
            string[] pedidoslineas = null;
            Elemento Element;
            if (File.Exists(rutaarchivo))
            {
                // instancie la clase elemento (utilice la metodologia del ejericio 53)
                lineas = File.ReadAllLines(rutaarchivo);
                string[] lineasplit = null;
                foreach (string linea in lineas)
                {
                    //aca empiezo a guardar cada contenido del array en el list de elementos acorde a mi stock (llamo a los objeto elemento, les pongo lo que corresponde
                    // y luego es lo guardo en un metodo en la clase Empresa dentro de la lista Stock! voila
                    Element = new Elemento();
                    lineasplit = linea.Split(';');
                    Element.prod.idprod = lineasplit[0]; 
                    Element.cantidad = Convert.ToInt32(lineasplit[1]);

                    
                    EmpresaInstanciada.GuardarStock(Element);

                }

            }
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se cargo el stock");

            
            
            if (File.Exists(rutapedidohistorico))
            {
                pedhistlineas = File.ReadAllLines(rutapedidohistorico);
                foreach (string linea in pedhistlineas)
                {
                    ListaPedidosTemporal.Add(linea);
                }
            }
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se cargo la tabla de codigos de pedidos diario");
            // aca acordate de levantar los pedidos para tenerlos en memo y hacer append.

            // cargar en memoria la lista de pedidos previo a procesado.


            
            if (File.Exists(rutapedidodiario)) // si se crashea el programa trata de levantar la lista pedido actual
            {
                string[] lineasplit = null;
                int lineacabecera = 1;
                pedidoslineas = File.ReadAllLines(rutapedidodiario);
                foreach (string linea in pedidoslineas)
                {
                    lineasplit = linea.Split(';');
                    if (lineasplit.Count() == 4)
                    {
                        if (lineacabecera != 1)
                        {
                            EmpresaInstanciada.GuardarPedido(PedidoActual);
                            lineacabecera = 1; 
                        }
                        else { lineacabecera = lineacabecera + 1; }
                        
                        PedidoActual.comercio.codigo = lineasplit[0];
                        PedidoActual.comercio.razonsocial = lineasplit[1];
                        PedidoActual.comercio.cuit = lineasplit[2];
                        PedidoActual.comercio.domicilio = lineasplit[3];
                        
                    }
                    else
                    {
                        Element = new Elemento();                        
                        Element.prod.idprod = lineasplit[0];
                        Element.cantidad = Convert.ToInt32(lineasplit[1]);
                        PedidoActual.GuardarPedido(Element);
                        
                    }

                }
                ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se cargo la tabla pedidos en memoria");
            }

        }

        private void btnPedidosTot_Click(object sender, EventArgs e)
        {
            string[] pedidoslineas = null;
            pedidoslineas = File.ReadAllLines(rutapedidodiario);
            foreach (string linea in pedidoslineas)
            {
                ConfirmacionProcesado.Items.Add(linea);
            }
        }
    }
}
