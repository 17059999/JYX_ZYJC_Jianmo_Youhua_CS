﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bentley.MstnPlatformNET.WinForms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    /// <summary>
    /// 夹套管工具窗口
    /// </summary>
    public partial class JacketedPipeToolForm :
#if DEBUG
        Form
#else  
        Adapter
#endif 
    {
        /// <summary>
        /// 
        /// </summary>
        public JacketedPipeToolForm()
        {
            InitializeComponent();
            init();
        }
        private List<string> innerAllSpecs = new List<string>();
        private List<string> outerAllSpecs = new List<string>();
        private List<double> innerAllDn = new List<double>();
        private List<double> outerAllDn = new List<double>();
        private List<string> innerPipeline = new List<string>();
        private List<string> outerPipeline = new List<string>();
        private List<string> innerInsulationMaterial = new List<string>();
        private List<string> outerInsulationMaterial = new List<string>();

        private string currentInnerPipeline = "";
        private string currentOuterPipeline = "";
        private BindingSource bsInner = new BindingSource();
        private BindingSource bsOuter = new BindingSource();

        public List<string> allPipeLine = new List<string>();

        private void init() {
            allPipeLine = JacketedPipeTool.GetPipeLine();
            if (allPipeLine.Count < 2) return;

            currentInnerPipeline = allPipeLine[0];
            currentOuterPipeline = allPipeLine[1];

            foreach (var item in allPipeLine)
            {
                innerPipeline.Add(item);
            }
            //innerPipeline.Remove(currentInnerPipeline);
            foreach (var item in allPipeLine)
            {
                outerPipeline.Add(item);
            }
            outerPipeline.Remove(currentOuterPipeline);

            bsInner.DataSource = innerPipeline;
            this.comboBox_inner_pipeline.DataSource = bsInner;
            bsOuter.DataSource = outerPipeline;
            this.comboBox_outer_pipeline.DataSource = bsOuter;

            this.comboBox_inner_pipeline.Text = currentInnerPipeline;
            this.comboBox_outer_pipeline.Text = currentOuterPipeline;
            //innerPipeline = JacketedPipeTool.GetPipeLine();
            ////innerPipeline = allPipeLine;
            ////this.comboBox_inner_pipeline.DataSource = innerPipeline;
            //outerPipeline = JacketedPipeTool.GetPipeLine();
            ////outerPipeline = allPipeLine;
            ////this.comboBox_outer_pipeline.DataSource = outerPipeline;

            innerAllSpecs = Bentley.OpenPlantModeler.SDK.Utilities.DatabaseUtilities.GetAllSpecifications();
            outerAllSpecs = Bentley.OpenPlantModeler.SDK.Utilities.DatabaseUtilities.GetAllSpecifications();
            this.comboBox_inner_spec.DataSource = innerAllSpecs;
            this.comboBox_outer_spec.DataSource = outerAllSpecs;
            if (innerAllSpecs.Contains("mA1-OPM") && outerAllSpecs.Contains("mA1-OPM"))
            {
                this.comboBox_inner_spec.Text = "mA1-OPM";
                this.comboBox_outer_spec.Text = "mA1-OPM";
            }
            else
            {
                this.comboBox_inner_spec.SelectedIndex = 0;
                this.comboBox_outer_spec.SelectedIndex = 0;
            }

            this.textBox_inner_insulation_thickness.Text = "0";
            this.textBox_outer_insulation_thickness.Text = "0";

            innerInsulationMaterial = JacketedPipeTool.getInsulationMaterial();
            innerInsulationMaterial.Insert(0, "");
            outerInsulationMaterial = JacketedPipeTool.getInsulationMaterial();
            outerInsulationMaterial.Insert(0, "");
            this.comboBox_inner_insulation_material.DataSource = innerInsulationMaterial;
            this.comboBox_outer_insulation_material.DataSource = outerInsulationMaterial;
        }

        private void comboBox_inner_spec_SelectedIndexChanged(object sender, EventArgs e)
        {
            string name = ((ComboBox)sender).Name;
            if (name.Contains("inner_spec"))
            {
                innerAllDn = Bentley.OpenPlantModeler.SDK.Utilities.DatabaseUtilities.GetAllSizes(((ComboBox)sender).Text);
                this.comboBox_inner_dn.DataSource = innerAllDn;
                if (innerAllDn.Contains(100))
                {
                    this.comboBox_inner_dn.Text = "100";
                }
                else
                {
                    this.comboBox_inner_dn.SelectedIndex = 0;
                }
            }
            else if (name.Contains("outer_spec"))
            {
                outerAllDn = Bentley.OpenPlantModeler.SDK.Utilities.DatabaseUtilities.GetAllSizes(((ComboBox)sender).Text);
                this.comboBox_outer_dn.DataSource = outerAllDn;
                if (outerAllDn.Contains(150))
                {
                    this.comboBox_outer_dn.Text = "150";
                }
                else
                {
                    this.comboBox_outer_dn.SelectedIndex = 0;
                }
            }
        }

        private void textBox_inner_insulation_thickness_TextChanged(object sender, EventArgs e)
        {
            //TextBox temptexbox = (TextBox)sender;
            //string str = temptexbox.Text;
            //try
            //{
            //    double thickness = Convert.ToDouble(str);
            //    if (thickness < 0)
            //    {
            //        MessageBox.Show("请输入大于零的数");
            //        temptexbox.Text = "0";
            //    }
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show("请输入正确的值");
            //    temptexbox.Text = "0";
            //}
        }

        private void textBox_outer_insulation_thickness_TextChanged(object sender, EventArgs e)
        {
            //TextBox temptexbox = (TextBox)sender;
            //string str = temptexbox.Text;
            //try
            //{
            //    double thickness = Convert.ToDouble(str);
            //    if (thickness < 0)
            //    {
            //        MessageBox.Show("请输入大于零的数");
            //        temptexbox.Text = "0";
            //    }
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show("请输入正确的值");
            //    temptexbox.Text = "0";
            //}
        }

        private void JacketedPipeToolForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //JacketedPipeTool.isCloseForm = true;
            base.OnClosed(e);
            JacketedPipeTool.m_jacketedPipeToolForm = null;
            if (JacketedPipeTool.isClosedFromCode == JacketedPipeTool.StatusCloseFormEvent.DEFAULT) JacketedPipeTool.isClosedFromCode = JacketedPipeTool.StatusCloseFormEvent.FORM;
            JacketedPipeTool.MyCleanUp();
        }

        private void JacketedPipeToolForm_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void SetInnerPipeline() {

        }

        private void SetOuterPipeline() {

        }

        private void comboBox_inner_pipeline_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = comboBox_inner_pipeline.Text;
            outerPipeline.Clear();
            foreach (var item in allPipeLine)
            {
                outerPipeline.Add(item);
            }
            outerPipeline.Remove(text);
            bsOuter.ResetBindings(false);
            //if (outerPipeline.Contains(text))
            //{
            //    outerPipeline.Remove(text);
            //    bsOuter.ResetBindings(false);
            //}

        }

        private void comboBox_outer_pipeline_SelectedIndexChanged(object sender, EventArgs e)
        {
            //string text = comboBox_outer_pipeline.Text;
            //innerPipeline.Clear();
            //foreach (var item in allPipeLine)
            //{
            //    innerPipeline.Add(item);
            //}
            //innerPipeline.Remove(text);
            //bsInner.ResetBindings(false);
            //if (innerPipeline.Contains(text))
            //{
            //    innerPipeline.Remove(text);
            //    bsInner.ResetBindings(false);
            //}
        }
    }
}
