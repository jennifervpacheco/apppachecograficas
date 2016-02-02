using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//
//using MekkoGraphics.Web.ChartControls;
using System.Windows.Forms.DataVisualization.Charting;
//using DevExpress.XtraCharts;

namespace apppachecograficas
{
    public partial class Form1 : Form
    {
        string nit;
        string fecha;
        public Form1()
        {
            InitializeComponent();
            this.fecha = fecha;
            this.nit = nit;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            comboBox3.SelectedIndex = 0;
            this.cargarPropiedadHorizontal();
        }

        private void cargarPropiedadHorizontal()
        {
            ConexionPostgres conn = new ConexionPostgres();
            string cadenaSql = "SELECT * FROM modelo.propiedad_horizontal;";
            var resultado = conn.consultar(cadenaSql);

            List<select> sl = new List<select>();
            foreach (Dictionary<string, string> fila in resultado)
            {
                sl.Add(new select() { Text = fila["nit"] + " - " + fila["nombre"], Value = fila["nit"] });
            }
            comboBox1.DataSource = sl;
            comboBox1.DisplayMember = "Text";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //MessageBox.Show("Se agrega");
            this.cargarDatos();
        }

        private void cargarDatos()
        {
            this.chart1.Series["votos"].Points.Clear();

            select sl1 = comboBox1.SelectedItem as select;
            this.nit = Convert.ToString(sl1.Value);

            select sl2 = comboBox2.SelectedItem as select;
            this.fecha = Convert.ToString(sl2.Value);

            ConexionPostgres conn = new ConexionPostgres();
            string cadenaSql = "SELECT id_pregunta FROM modelo.pregunta_actual WHERE nit = '"+nit+"' AND fecha = '"+fecha+"';";
            var resultado = conn.consultar(cadenaSql);

            //Si no hay una pregunta actual y no retorna resultados (longitud igual a 0)
            if(resultado.ToArray().Length == 0)
            {
                MessageBoxEx.Show("Por favor ingrese una pregunta actual.",2000);
                return;
            }

            string pregunta_actual = resultado[0]["id_pregunta"];

            //Se pone la pregunta en la ventana
            cadenaSql = "SELECT pregunta FROM modelo.pregunta WHERE id_pregunta='" + pregunta_actual + "';";
            resultado = conn.consultar(cadenaSql);
            label8.Text = resultado[0]["pregunta"];

            //Se buscan las opciones de la pregunta actual
            cadenaSql = "SELECT id_opcion, opcion FROM modelo.opcion_pregunta WHERE id_pregunta = '" + pregunta_actual + "'";
            resultado = conn.consultar(cadenaSql);
            if (resultado.ToArray().Length == 0)
            {
                MessageBoxEx.Show("No hay opciones registradas para la pregunta actual.", 2000);
                return;
            }
            Dictionary<int, string> indiceOpciones = new Dictionary<int, string>();
            //Se inicializa la votación con ceros
            Dictionary<int, double> resultadoVotacion = new Dictionary<int, double>();
            foreach (Dictionary<string, string> fila in resultado)
            {
                int id_opcion = Int32.Parse(fila["id_opcion"]);
                indiceOpciones[id_opcion] = fila["opcion"];
                resultadoVotacion[id_opcion] = 0;
            }

            //Se consultan los votos
            if (comboBox3.SelectedIndex == 0)//Unidades residenciales
            {
                cadenaSql = "SELECT * FROM modelo.voto WHERE id_pregunta='" + pregunta_actual + "';";
            }
            else//Coeficientes
            {
                cadenaSql = @"SELECT
                a.id_pregunta AS id_pregunta,
                a.numero_unidad AS numero_unidad,
                a.id_opcion AS id_opcion,
                a.nit AS nit,
                a.fecha AS fecha,
                b.coeficiente AS coeficiente
                FROM modelo.voto AS a
                LEFT JOIN modelo.unidad_residencial AS b
                ON b.numero_unidad = a.numero_unidad
                AND b.nit = a.nit
                WHERE a.id_pregunta = '" + pregunta_actual + "';";
            }
            resultado = conn.consultar(cadenaSql);
            if (resultado.ToArray().Length == 0)
            {
                MessageBoxEx.Show("No hay votos registrados para la pregunta actual.", 2000);
                return;
            }

            //Resultado de la Votación
            string nombreVoto = "";
            double totalVotos = 0;
            if (comboBox3.SelectedIndex == 0)//Unidades residenciales
            {
                //El resultado votacion es para Unidad
                nombreVoto = "Votos";
                foreach (Dictionary<string, string> fila in resultado)
                {
                    int id_opcion = Int32.Parse(fila["id_opcion"]);
                    resultadoVotacion[id_opcion] += 1;
                    totalVotos += 1;
                }
            }
            else//Coeficientes
            {
                //El resultado votacion es para coeficiente
                nombreVoto = "Coef.";
                foreach (Dictionary<string, string> fila in resultado)
                {
                    int id_opcion = Int32.Parse(fila["id_opcion"]);
                    double coeficiente = Double.Parse(fila["coeficiente"]);
                    resultadoVotacion[id_opcion] += coeficiente;
                    totalVotos += coeficiente;
                }
            }

            foreach (KeyValuePair<int, double> resultadoOpcion in resultadoVotacion)
            {
                this.chart1.Series["votos"].Points.AddXY( "Op. " + resultadoOpcion.Key + " - " + indiceOpciones[resultadoOpcion.Key] + ": " + resultadoOpcion.Value + " " + nombreVoto, resultadoOpcion.Value);
            }
            
            //Se carga el quorum asistencia total caso unidades residenciales (iguales)
            string quorum = "";
            if (comboBox3.SelectedIndex == 0)//Unidades residenciales
            {
                cadenaSql = "SELECT count(*) FROM modelo.asamblea_unidad_residencial WHERE nit='" + nit + "' AND fecha='" + fecha + "';";
                double asistenciaCasoUnidades = Double.Parse(conn.consultar(cadenaSql)[0]["count"]);

                cadenaSql = "SELECT count(*) FROM modelo.unidad_residencial WHERE nit='" + nit + "';";
                double registradosCasoUnidades = Double.Parse(conn.consultar(cadenaSql)[0]["count"]);
                double porcentaje = (100 * asistenciaCasoUnidades / registradosCasoUnidades);
                porcentaje = Math.Round(porcentaje, 2);
                quorum = (porcentaje).ToString();
            }
            else//Coeficientes
            {
                cadenaSql = "SELECT sum(b.coeficiente) FROM modelo.asamblea_unidad_residencial AS a LEFT JOIN modelo.unidad_residencial AS b ON (a.numero_unidad = b.numero_unidad AND a.nit = b.nit) WHERE a.nit='" + nit + "' AND a.fecha='" + fecha + "';";
                double asistenciaCasoCoeficientes = Double.Parse(conn.consultar(cadenaSql)[0]["sum"]);

                cadenaSql = "SELECT sum(coeficiente) FROM modelo.unidad_residencial WHERE nit='" + nit + "';";
                double registradosCasoCoeficientes = Double.Parse(conn.consultar(cadenaSql)[0]["sum"]);

                double porcentaje = (100 * asistenciaCasoCoeficientes / registradosCasoCoeficientes);
                porcentaje = Math.Round(porcentaje, 2);
                quorum = (porcentaje).ToString();
            }
            
            label3.Text = quorum + "%";
            label4.Text = (totalVotos*0.51).ToString() + " " + nombreVoto;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConfiguracionServidorBaseDatos form = new ConfiguracionServidorBaseDatos();
            form.Show();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //comboBox2.SelectedText = "";
            comboBox2.SelectedIndex = -1;
            //comboBox2.Focus();
            //comboBox2.SelectionLength = 0;
            List<select> empty = new List<select>();
            comboBox2.DataSource = empty;
            
            select sl1 = comboBox1.SelectedItem as select;
            string nit = Convert.ToString(sl1.Value);

            ConexionPostgres conn = new ConexionPostgres();
            string cadenaSql = "SELECT * FROM modelo.asamblea WHERE nit='"+ nit + "';";
            var resultado = conn.consultar(cadenaSql);

            List<select> sl = new List<select>();
            foreach (Dictionary<string, string> fila in resultado)
            {
                string fecha = fila["fecha"];
                DateTime dt = Convert.ToDateTime(fecha);
                fecha = dt.Year + "-" + dt.Month + "-" + dt.Day;
                sl.Add(new select() { Text = "Fecha: " + fecha + ", Nombre: " + fila["nombre"], Value = fecha });
            }
            comboBox2.DataSource = sl;
            comboBox2.DisplayMember = "Text";
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.cargarPropiedadHorizontal();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
            if (timer1.Enabled)//Activo el Timer
            {
                MessageBoxEx.Show("Se activa el seguimiento de la asamblea.", 1000);
                this.cargarDatos();
            }
            else//Inactivo el Timer
            {
                MessageBoxEx.Show("Se desactiva el seguimiento de la asamblea.", 1000);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Quorum qu = new Quorum( this.nit, this.fecha);
            qu.Show();
        }
    }
}
