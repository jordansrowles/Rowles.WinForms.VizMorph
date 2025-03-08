using Rowles.WinForms.VizMorph;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ExampleApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            SetupChart();
        }

        private void SetupChart()
        {

            // Create a button to toggle between Line and Bar chart
            
            button1.Click += ToggleChartTypeButton_Click;

            // Create a button to generate random data
            button2.Click += RandomizeDataButton_Click;

        }

        private void ToggleChartTypeButton_Click(object sender, EventArgs e)
        {
            morphingChart1.IsBarChart = !morphingChart1.IsBarChart;
        }

        private void RandomizeDataButton_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            List<float> newData = new List<float>();
            for (int i = 0; i < 30; i++)
            {
                newData.Add((float)(rnd.NextDouble() * 80 - 40)); // Random values between -40 and 40
            }
            morphingChart1.DataPoints = newData;
        }
    }
}
