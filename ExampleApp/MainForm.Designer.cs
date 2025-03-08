namespace ExampleApp
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            button1 = new Button();
            button2 = new Button();
            propertyGrid1 = new PropertyGrid();
            morphingChart1 = new Rowles.WinForms.VizMorph.MorphingChart();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(299, 194);
            button1.Name = "button1";
            button1.Size = new Size(112, 34);
            button1.TabIndex = 1;
            button1.Text = "Morph";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(302, 234);
            button2.Name = "button2";
            button2.Size = new Size(112, 34);
            button2.TabIndex = 2;
            button2.Text = "Data";
            button2.UseVisualStyleBackColor = true;
            // 
            // propertyGrid1
            // 
            propertyGrid1.Location = new Point(1194, 25);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.SelectedObject = morphingChart1;
            propertyGrid1.Size = new Size(587, 1235);
            propertyGrid1.TabIndex = 3;
            // 
            // morphingChart1
            // 
            morphingChart1.AnimationInterval = 16;
            morphingChart1.AnimationStep = 0.05F;
            morphingChart1.AxisColor = Color.Black;
            morphingChart1.AxisFont = new Font("Arial", 8F);
            morphingChart1.BarColor = Color.SteelBlue;
            morphingChart1.ChartTitle = "";
            morphingChart1.DataPoints = (List<float>)resources.GetObject("morphingChart1.DataPoints");
            morphingChart1.IsBarChart = false;
            morphingChart1.LineColor = Color.OrangeRed;
            morphingChart1.Location = new Point(128, 327);
            morphingChart1.MarkerColor = Color.OrangeRed;
            morphingChart1.MarkerSize = 6F;
            morphingChart1.Name = "morphingChart1";
            morphingChart1.Padding = new Padding(40, 20, 20, 40);
            morphingChart1.ShowValues = false;
            morphingChart1.ShowXAxisLabels = true;
            morphingChart1.ShowYAxisLabels = true;
            morphingChart1.Size = new Size(983, 613);
            morphingChart1.TabIndex = 4;
            morphingChart1.TitleMargin = 20;
            morphingChart1.TooltipThreshold = 6;
            morphingChart1.TooltipTimeout = 1000;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1793, 1298);
            Controls.Add(morphingChart1);
            Controls.Add(propertyGrid1);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "MainForm";
            Text = "MainForm";
            ResumeLayout(false);
        }

        #endregion
        private Button button1;
        private Button button2;
        private PropertyGrid propertyGrid1;
        private Rowles.WinForms.VizMorph.MorphingChart morphingChart1;
    }
}