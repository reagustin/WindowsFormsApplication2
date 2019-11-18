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
        enum tipodelista
        {
            listapedidos = 1, listadevoluciones = 2

        }
        string rutaarchivo = @"C:\TP\stock.txt";
        string rutapedidohistorico = @"C:\TP\pedidoscodigohistorico.txt";
        string rutapedidodiario = @"C:\TP\pedidos.txt";
        string enviologistica = @"C:\TP\";
        string codigodecomercio = "M023";
        string rutadevolucionesprocesadas = @"C:\TP\DevolucionesProcesadas.txt";
        Empresa EmpresaInstanciada;

        List<string> ListaDevolucionesHistorica = new List<string>();
        List<string> ListaPedidosTemporal = new List<string>();

        List<Devolucion> ListaDevoluciones = new List<Devolucion>(); //antes estaba en el metodo ProcesaDevoluciones
        Devolucion Devoluc; //antes estaba en el metodo ProcesaDevoluciones

        public FrmTP1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            txtRuta.Text = openFileDialog1.FileName;


        }



        private void btnProcesar_Click(object sender, EventArgs e)
        {
            if (txtRuta.Text != string.Empty)
            {
                if (RdnRecepcionPedido.Checked)
                {
                    ProcesaPedido(txtRuta.Text);
                }
                else if (rdnRecepcionStock.Checked)
                {
                    ProcesaStock(txtRuta.Text);
                }
                else if (rdnRecepcionLogistica.Checked)
                {
                    ProcesaDevoluciones(txtRuta.Text); //PROCESO EL ENVIO A LOGISTICA -- //DEVUELVO AL STOCK
                    RegenerarArchivoDePedidos(); // DEVUELVO AL STOCK
                }
                else if (RdnEnvioLogistica.Checked)
                {
                    ProcesaEnvioLogistica(); // PROCESO EL ENVIO A LOGISTICA

                }
            }
        }



        private void RegenerarArchivoDePedidos() // ya tengo la devolucion de pedidos cargada en memoria
        {
            foreach (Devolucion devolucion in ListaDevoluciones) // EMPIEZO A RECORRER UNO POR UNO LAS DEVOLUCIONES
            {
                Pedido DevolucionPedido = (Pedido)EmpresaInstanciada.pedidos.Find(x => x.codigo == (devolucion.CodigoReferencia)); // BUSCO PARA X DEVOLUCION EL PEDIDO EQUIVALENTE EN MEMORIA

                if (DevolucionPedido != null) // SI NO ES NULO, O SEA SI ENCONTRO UN PEDIDO DENTRO DE LA LISTA DE PEDIDOS DE MEMORIA, HACE LO SIGUIENTE
                {
                    if (devolucion.Entregado == false) // SI EL CAMPO QUE VENIA DEL PRIMER PEDIDO MATCHEADO DE LA DEVOLUCION ESTABA EN FALSE
                    {
                        DevolucionPedido.Entregado = false; // ENTONCES LO CAMBIA A TRUE Y ADEMAS LO TIENE QUE DEVOLVER AL STOCK


                        foreach (Elemento ElementoPedido in DevolucionPedido.listaproducto) // PARA CADA ELEMENTO DE LA LISTA DE ELEMENTOS DEL PEDIDO QUE MATCHEO EN EL PUNTO ANTERIOR                 
                        {

                            Elemento ElementoEncontrado = (Elemento)EmpresaInstanciada.stock.Find(x => x.prod.idprod == (ElementoPedido.prod.idprod)); // BUSCO EL ELEMENTO EN EL STOCK
                            ElementoEncontrado.cantidad = ElementoEncontrado.cantidad + ElementoPedido.cantidad; // LE ADICIONO AL STOCK LO QUE DEVUELVO DE ESTE ITEM

                        }
                    }
                    else // SI EL CAMPO QUE VENIA DEL PRIMER PEDIDO MATCHEADO DE LA DEVOLUCION NO ESTABA EN FALSE, O SEA EN TRUE
                    {
                        DevolucionPedido.Entregado = true; // LO MARCO COMO EN TRUE Y YA, NO DEVUELVO NADA, SE FINALIZO EL PROCESO
                    }

                }
            }
            // YA SALI DEL PRIMER FOREACH, AHORA QUIERO QUE TODO LO QUE ESTA EN MEMORIA A CONTINUACION SE VUELVA A IMPRIMIR         
            GrabarPedidosTxt(); // LLAMO DE NUEVO A ESTE METODO QUE GRABA PEDIDOS, O SEA TOMO LO QUE ESTA EN MEMORIA Y LO IMPRIME, YO MODIFIQUE EN MEMORIA!
            GrabarStockTxt();
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se grabo el archivo pedidos");
        }

        private void ProcesaDevoluciones(string Ruta)
        {

            string[] lineas = null;


            if (File.Exists(Ruta))
            {
                lineas = File.ReadAllLines(Ruta);
                string[] lineasplit = null;
                foreach (string linea in lineas)
                {
                    if (linea != string.Empty)
                    {


                        //aca empiezo a guardar cada contenido del array en el list de elementos acorde a mi stock (llamo a los objeto elemento, les pongo lo que corresponde
                        // y luego es lo guardo en un metodo en la clase Empresa dentro de la lista Stock! voila
                        Devoluc = new Devolucion();
                        lineasplit = linea.Split(';');
                        Devoluc.CodigoReferencia = lineasplit[0];
                        Devoluc.Entregado = Convert.ToBoolean(lineasplit[1]);


                        if (!ListaDevolucionesHistorica.Contains(Devoluc.CodigoReferencia))
                        {
                            ListaDevoluciones.Add(Devoluc);
                            ListaDevolucionesHistorica.Add(Devoluc.CodigoReferencia);
                            using (StreamWriter sw = File.AppendText(rutadevolucionesprocesadas))
                            {
                                sw.Write(Devoluc.CodigoReferencia + "\r\n");
                            }

                        }
                    }

                }
            }
        }

        private bool VerificaStockDePedido(List<Elemento> lista)
        {
            foreach (Elemento elem in lista)
            {
                Elemento ElementoEncontrado = (Elemento)EmpresaInstanciada.stock.Find(x => x.prod.idprod == (elem.prod.idprod));
                if (ElementoEncontrado != null)
                {
                    if (ElementoEncontrado.cantidad >= elem.cantidad)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        private void ProcesaEnvioLogistica()
        {
            string nombreArchivoLogistica = enviologistica + "Lote_" + codigodecomercio + "_L" + new Random().Next(1, 999).ToString() + ".txt";

            using (StreamWriter sw = File.CreateText(nombreArchivoLogistica))
            {
                sw.WriteLine(EmpresaInstanciada.razonsocial + ";" + EmpresaInstanciada.cuit + ";" + EmpresaInstanciada.domicilio);

                foreach (Pedido PedidoActualComercio in EmpresaInstanciada.pedidos)
                {
                    if (PedidoActualComercio.EnviadoLogistica == false)//Esto lo habiamos visto pero no se por que no lo modificamos, unicamente procesa los enviados a logistica false.. ya que los true ya los envio
                    {
                        if (VerificaStockDePedido(PedidoActualComercio.listaproducto))//Esto es lo ultimo que hicimos no lo modifique anda
                        {
                            sw.WriteLine("---");
                            sw.WriteLine(PedidoActualComercio.codigo + ";" + PedidoActualComercio.comercio.domicilio);


                            foreach (Elemento elementopedido in PedidoActualComercio.listaproducto)
                            {

                                Elemento ElementoEncontrado = (Elemento)EmpresaInstanciada.stock.Find(x => x.prod.idprod == (elementopedido.prod.idprod));
                                if (ElementoEncontrado != null)
                                {
                                    if (ElementoEncontrado.cantidad >= elementopedido.cantidad)
                                    {
                                        ElementoEncontrado.cantidad = ElementoEncontrado.cantidad - elementopedido.cantidad;
                                        sw.WriteLine(elementopedido.prod.idprod + ";" + elementopedido.cantidad);
                                    }

                                }
                            }
                            PedidoActualComercio.EnviadoLogistica = true;
                        }
                        else
                        {
                            //A mi me parece que si el pedido que proceso ningun articulo tiene stock que es el caso que llega hasta aca
                            //ademas de no mandarlo a logistica como lo hace ahora.. deberiaponerlo en true... ya que si no lo pone en true la siguiente ves que proceses lo intentara mandar
                            //, no esta mal tampoco esto ultimo pero deberias aclararlo... digamos no lo proceso por que no tengo stock pero mañana quizas si entonces lo dejo en false para la siguiente corrida te parece?
                            //yo lo dejo aca comentado
                            //Otra opcion es tener 3 estados.. no enviado.. enviado.. rechazado... este ulimo caso en el que no se puede mandar seria rechazado .. se deberian modificar las validaciones

                            PedidoActualComercio.EnviadoLogistica = true;

                        }
                    }
                }
            }

            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se genero el archivo de lote para logistica");

            GrabarStockTxt();
            GrabarPedidosTxt();
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se actualizo stock");

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ruta"></param>
        private void ProcesaPedido(string Ruta)
        {

            int nrolinea = 0;
            string[] lineas = null;
            string[] nombrepedidoruta = null;
            string[] remueveExtension = null;
            Elemento Element;
            Pedido pedidoComercio = new Pedido();
            if (File.Exists(Ruta))
            {
                nombrepedidoruta = Ruta.Split('_');
                remueveExtension = nombrepedidoruta[1].Split('.');
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
                        if (nrolinea == 0)
                        {
                            lineasplit = linea.Split(';');
                            pedidoComercio.codigo = remueveExtension[0];
                            pedidoComercio.EnviadoLogistica = false;
                            pedidoComercio.comercio.codigo = lineasplit[0];
                            pedidoComercio.comercio.razonsocial = lineasplit[1];
                            pedidoComercio.comercio.cuit = lineasplit[2];
                            pedidoComercio.comercio.domicilio = lineasplit[3];
                            nrolinea = nrolinea + 1;

                        }
                        else
                        {
                            lineasplit = linea.Split(';');
                            Elemento ElementoEncontrado = (Elemento)EmpresaInstanciada.stock.Find(x => x.prod.idprod == lineasplit[0]);
                            if (ElementoEncontrado != null)
                            {
                                Element = new Elemento();

                                Element.prod.idprod = lineasplit[0];
                                Element.cantidad = Convert.ToInt32(lineasplit[1]);
                                pedidoComercio.GuardarPedido(Element);
                            }
                        }

                    }
                    if (pedidoComercio.listaproducto.Count > 0)
                    {
                        EmpresaInstanciada.pedidos.Add(pedidoComercio);
                    }
                    ListaPedidosTemporal.Add(remueveExtension[0]);
                    using (StreamWriter sw = File.AppendText(rutapedidohistorico))
                    {
                        sw.Write(remueveExtension[0] + "\r\n");
                    }
                }
                GrabarPedidosTxt();

            }
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Pedido Procesado");

        }


        private void GrabarPedidosTxt()
        {
            if (File.Exists(rutapedidodiario))
            {
                File.Delete(rutapedidodiario);

                using (StreamWriter sw = File.CreateText(rutapedidodiario))
                {

                    foreach (Pedido PedidoActualComercio in EmpresaInstanciada.pedidos)
                    {
                        sw.WriteLine(PedidoActualComercio.codigo + ";" + PedidoActualComercio.comercio.codigo + ";" + PedidoActualComercio.comercio.razonsocial + ";" + PedidoActualComercio.comercio.cuit + ";" + PedidoActualComercio.comercio.domicilio + ";" + PedidoActualComercio.EnviadoLogistica);


                        foreach (Elemento elementopedido in PedidoActualComercio.listaproducto)
                        {
                            sw.WriteLine(elementopedido.prod.idprod + ";" + elementopedido.cantidad);
                        }
                    }

                }

            }
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se grabo el archivo pedidos");

        }




        private void ProcesaStock(string Ruta) //////////// STOCK SE GRABA EN MEMORIA //////////////
        {


            List<Elemento> listaproduccion = new List<Elemento>();
            string[] lineas = null;
            Elemento Element;
            if (File.Exists(Ruta))
            {

                lineas = File.ReadAllLines(Ruta);
                string[] lineasplit = null;

                foreach (string linea in lineas)
                {
                    if (linea != string.Empty)
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

            }
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se lee archivo de produccion");

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
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se actualizo stock");


        }

        private void GrabarStockTxt() /////////////////////////////// STOCK PASA A TXT /////////////////////////////////////
        {
            if (File.Exists(rutaarchivo))
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
                    MessageBox.Show("Error", "No tiene informacion de stock");
                    ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Error procesando archivo");
                }
            }
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se grabo el archivo stock");
        }
        /// <summary>
        ///
        /// </summary>
        private void CargarStockInicial()
        {
            try
            {
                string[] lineas = null;
                Elemento Element;
                if (File.Exists(rutaarchivo))
                {
                    // instancie la clase elemento (utilice la metodologia del ejericio 53)
                    lineas = File.ReadAllLines(rutaarchivo);
                    string[] lineasplit = null;
                    foreach (string linea in lineas)
                    {
                        if (linea != string.Empty)
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

                }
                else
                {
                    MessageBox.Show("El Archivo stock no se encuentra en la ruta correspondiente.\n" + rutaarchivo, "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrio un error al cargar los archivos necesarios el funcionamiento del programa.\n" + ex.Message.ToString(), "Error");
            }

        }
        private void CargaDeListasHistoricas(tipodelista tipo)
        {
            try
            {
                string[] pedhistlineas = null;
                string[] lineadevolucion = null;
                if (tipo == tipodelista.listadevoluciones)
                {
                    if (File.Exists(rutadevolucionesprocesadas))
                    {
                        lineadevolucion = File.ReadAllLines(rutadevolucionesprocesadas);
                        foreach (string linea in lineadevolucion)
                        {
                            ListaDevolucionesHistorica.Add(linea);
                        }
                    }
                    else
                    {
                        MessageBox.Show("El Archivo de devoluciones no se encuentra en la ruta correspondiente.\n" + rutadevolucionesprocesadas, "Error");
                    }

                }
                else
                {

                    if (File.Exists(rutapedidohistorico))
                    {
                        pedhistlineas = File.ReadAllLines(rutapedidohistorico);
                        foreach (string linea in pedhistlineas)
                        {
                            ListaPedidosTemporal.Add(linea);
                        }
                    }
                    else
                    {
                        MessageBox.Show("El Archivo de pedidos  no se encuentra en la ruta correspondiente.\n" + rutapedidodiario, "Error");
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Ocurrio un error al cargar los archivos necesarios el funcionamiento del programa.\n" + ex.Message.ToString(), "Error");

                throw;
            }

        }




        public void CargarPedidosActualesPendientes() //aca se guarda en memo toda la lista pedidos
        {
            string[] pedidoslineas = null;
            Elemento Element;
            Pedido PedidoActual = new Pedido();
            if (File.Exists(rutapedidodiario)) // si se crashea el programa trata de levantar la lista pedido actual
            {
                string[] lineasplit = null;
                int lineacabecera = 1;
                pedidoslineas = File.ReadAllLines(rutapedidodiario);
                foreach (string linea in pedidoslineas)
                {
                    if (linea != string.Empty)
                    {
                        lineasplit = linea.Split(';');

                        if (lineasplit.Count() == 6)
                        {
                            if (lineacabecera != 1)
                            {
                                EmpresaInstanciada.GuardarPedido(PedidoActual);
                                //Cuando termino de cargar el pedido a la lista lo destruyo
                                PedidoActual = null;

                            }
                            else
                            {
                                lineacabecera = lineacabecera + 1;
                            }
                            //y Aca lo creo de vuelta cuando empiezo uno nuevo
                            PedidoActual = new Pedido();
                            PedidoActual.codigo = lineasplit[0];
                            PedidoActual.comercio.codigo = lineasplit[1];
                            PedidoActual.comercio.razonsocial = lineasplit[2];
                            PedidoActual.comercio.cuit = lineasplit[3];
                            PedidoActual.comercio.domicilio = lineasplit[4];
                            PedidoActual.EnviadoLogistica = Convert.ToBoolean(lineasplit[5]);

                        }
                        else
                        {
                            Element = new Elemento();
                            Element.prod.idprod = lineasplit[0];
                            Element.cantidad = Convert.ToInt32(lineasplit[1]);
                            PedidoActual.GuardarPedido(Element);
                        }
                    }
                }
                //Luego de que termino el ultimo detalle no agregaba el pedido a la empresa instanciada entonces lo hago fuera
                //Agus esta linea del  la cree por que si el archivo estaba vacio agregaba una linea sin nada separada por ;
                //Entonces solo controlo que agrego un pedido a la lista si tiene algo
                if (PedidoActual.codigo != null)
                {
                    EmpresaInstanciada.GuardarPedido(PedidoActual);
                }
            }
        }
        private void FrmTP1_Load(object sender, EventArgs e)
        {
            CargarDatosEmpresa();
            CargarStockInicial();
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se cargo el stock");
            CargaDeListasHistoricas(tipodelista.listapedidos);
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se cargo la tabla de codigos de pedidos diario");
            CargarPedidosActualesPendientes();
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se cargo la tabla pedidos en memoria");
            CargaDeListasHistoricas(tipodelista.listadevoluciones);
            ConfirmacionProcesado.Items.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - Se cargo la tabla de codigos de devoluciones historica");

        }

        private void CargarDatosEmpresa()
        {
            EmpresaInstanciada = new Empresa();
            EmpresaInstanciada.razonsocial = Program.RazonSocial;
            EmpresaInstanciada.cuit = Program.CUIT;
            EmpresaInstanciada.domicilio = Program.Domicilio;
        }
        private void btnPedidosTot_Click(object sender, EventArgs e)
        {
            string[] pedidoslineas = null;
            pedidoslineas = File.ReadAllLines(rutapedidodiario);
            ConfirmacionProcesado.Items.Add("---------------------------");
            foreach (string linea in pedidoslineas)
            {
                ConfirmacionProcesado.Items.Add(linea);
            }
            ConfirmacionProcesado.Items.Add("---------------------------");
        }

        private void btnStock_Click(object sender, EventArgs e)
        {
            string[] stocklineas = null;
            stocklineas = File.ReadAllLines(rutaarchivo);
            ConfirmacionProcesado.Items.Add("---------------------------");
            foreach (string linea in stocklineas)
            {

                ConfirmacionProcesado.Items.Add(linea);
            }
            ConfirmacionProcesado.Items.Add("---------------------------");
        }

        private void RdnEnvioLogistica_CheckedChanged(object sender, EventArgs e)
        {
            if (RdnEnvioLogistica.Checked)
            {
                btnExplorarArchivo.Enabled = false;

            }
            else
            {
                btnExplorarArchivo.Enabled = true;
            }
        }
    }
}
