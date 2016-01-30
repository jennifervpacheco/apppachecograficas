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
using Newtonsoft.Json;
using System.IO;

namespace apppachecograficas
{
    public partial class ConfiguracionServidorBaseDatos : Form
    {
        private string archivoConfiguracion = "config/servidor.json";
        public ConfiguracionServidorBaseDatos()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Se guarda a JSON
            Dictionary<string, string> array = new Dictionary<string, string>();
            array["ip"] = textBox1.Text;
            array["puerto"] = textBox2.Text;
            array["usuario"] = textBox3.Text;
            array["clave"] = textBox4.Text;
            array["basededatos"] = textBox5.Text;
            string json = JsonConvert.SerializeObject(array, Formatting.Indented);
            StreamWriter sw = new StreamWriter(this.archivoConfiguracion);
            sw.Write(json);
            sw.Close();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ConfiguracionServidorBaseDatos_Load(object sender, EventArgs e)
        {
            //this.TopMost = true;
            //this.FormBorderStyle = FormBorderStyle.None;
            //this.WindowState = FormWindowState.Maximized;

            StreamReader r = new StreamReader(this.archivoConfiguracion, Encoding.Default, true);
            string json = r.ReadToEnd();
            r.Close();
            dynamic array = JsonConvert.DeserializeObject(json);
            textBox1.Text = array["ip"];
            textBox2.Text = array["puerto"];
            textBox3.Text = array["usuario"];
            textBox4.Text = array["clave"];
            textBox5.Text = array["basededatos"];
        }
    }
}
