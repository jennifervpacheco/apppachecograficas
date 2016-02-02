using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace apppachecograficas
{
    public partial class Quorum : Form
    {
        private string fecha;
        private string nit;
        public Quorum(string nit, string fecha)
        {
            InitializeComponent();
            this.fecha = fecha;
            this.nit = nit;

        }

        private void Quorum_Load(object sender, EventArgs e)
        {
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

        }
        private void cargarQuorum()
        {
            string quorum = "";
            ConexionPostgres conn = new ConexionPostgres();

            var cadenaSql = "SELECT sum(b.coeficiente) FROM modelo.asamblea_unidad_residencial AS a LEFT JOIN modelo.unidad_residencial AS b ON (a.numero_unidad = b.numero_unidad AND a.nit = b.nit) WHERE a.nit='"+this.nit+"' AND a.fecha='"+this.fecha+"';";
            double asistenciaCasoCoeficientes = Double.Parse(conn.consultar(cadenaSql)[0]["sum"]);

            cadenaSql = "SELECT sum(coeficiente) FROM modelo.unidad_residencial WHERE nit='"+this.nit+"';";
            double registradosCasoCoeficientes = Double.Parse(conn.consultar(cadenaSql)[0]["sum"]);

            cadenaSql = "SELECT sum(b.coeficiente) FROM modelo.asamblea_unidad_residencial AS a LEFT JOIN modelo.unidad_residencial AS b ON (a.numero_unidad = b.numero_unidad AND a.nit = b.nit) WHERE a.nit='" + this.nit + "' AND a.fecha='" + this.fecha + "' and id_tipo_asistencia_final ='4';";
            double registradosCasoUnidadesdescargue = Double.Parse(conn.consultar(cadenaSql)[0]["sum"]);

            double porcentaje = (100 * (asistenciaCasoCoeficientes - registradosCasoUnidadesdescargue) / registradosCasoCoeficientes);
            porcentaje = Math.Round(porcentaje, 2);
            quorum = (porcentaje).ToString();

            label1.Text = quorum + "%";
            if (porcentaje >= 51)
            {
                label1.ForeColor = System.Drawing.Color.Green;
            }
        }
    }
}
