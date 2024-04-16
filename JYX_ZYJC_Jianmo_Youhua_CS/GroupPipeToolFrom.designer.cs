namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class GroupPipeToolFrom
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GroupPipeToolFrom));
            this.button1 = new System.Windows.Forms.Button();
            this.dataGridView_PipeMessage = new System.Windows.Forms.DataGridView();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pipeLine = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ClassName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OD = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.WALL_THICKNESS = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button2 = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.comboBox_elbowOrBend = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox_elbow = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.label_wanqubanjing = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_elbow_radius = new System.Windows.Forms.ComboBox();
            this.comboBox_elbow_angle = new System.Windows.Forms.ComboBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.radioCenter = new System.Windows.Forms.RadioButton();
            this.radioTop = new System.Windows.Forms.RadioButton();
            this.radioDown = new System.Windows.Forms.RadioButton();
            this.radioLeft = new System.Windows.Forms.RadioButton();
            this.radioRight = new System.Windows.Forms.RadioButton();
            this.groupBox_bend = new System.Windows.Forms.GroupBox();
            this.textBox_lengthAfterBend = new System.Windows.Forms.TextBox();
            this.textBox_lengthToBend = new System.Windows.Forms.TextBox();
            this.textBox_radius = new System.Windows.Forms.TextBox();
            this.textBox_radiusFactor = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_PipeMessage)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox_elbow.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox_bend.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.Location = new System.Drawing.Point(36, 563);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 33);
            this.button1.TabIndex = 0;
            this.button1.Text = "添加管道";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // dataGridView_PipeMessage
            // 
            this.dataGridView_PipeMessage.AllowUserToAddRows = false;
            this.dataGridView_PipeMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_PipeMessage.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView_PipeMessage.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_PipeMessage.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView_PipeMessage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_PipeMessage.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.pipeLine,
            this.ClassName,
            this.DN,
            this.OD,
            this.WALL_THICKNESS});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView_PipeMessage.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView_PipeMessage.Location = new System.Drawing.Point(8, 30);
            this.dataGridView_PipeMessage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dataGridView_PipeMessage.Name = "dataGridView_PipeMessage";
            this.dataGridView_PipeMessage.RowHeadersVisible = false;
            this.dataGridView_PipeMessage.RowTemplate.Height = 23;
            this.dataGridView_PipeMessage.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_PipeMessage.Size = new System.Drawing.Size(343, 148);
            this.dataGridView_PipeMessage.TabIndex = 0;
            this.dataGridView_PipeMessage.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_PipeMessage_CellClick);
            // 
            // id
            // 
            this.id.FillWeight = 50F;
            this.id.HeaderText = "序号";
            this.id.Name = "id";
            // 
            // pipeLine
            // 
            this.pipeLine.HeaderText = "管线编号";
            this.pipeLine.Name = "pipeLine";
            // 
            // ClassName
            // 
            this.ClassName.FillWeight = 50F;
            this.ClassName.HeaderText = "类名";
            this.ClassName.Name = "ClassName";
            this.ClassName.ReadOnly = true;
            this.ClassName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DN
            // 
            this.DN.FillWeight = 50F;
            this.DN.HeaderText = "DN";
            this.DN.Name = "DN";
            this.DN.ReadOnly = true;
            // 
            // OD
            // 
            this.OD.FillWeight = 50F;
            this.OD.HeaderText = "OD";
            this.OD.Name = "OD";
            // 
            // WALL_THICKNESS
            // 
            this.WALL_THICKNESS.FillWeight = 50F;
            this.WALL_THICKNESS.HeaderText = "壁厚";
            this.WALL_THICKNESS.Name = "WALL_THICKNESS";
            // 
            // button2
            // 
            this.button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button2.Location = new System.Drawing.Point(232, 563);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(112, 33);
            this.button2.TabIndex = 1;
            this.button2.Text = "绘制";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label13
            // 
            this.label13.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(61, 36);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(59, 14);
            this.label13.TabIndex = 0;
            this.label13.Text = "弯头类型:";
            // 
            // comboBox_elbowOrBend
            // 
            this.comboBox_elbowOrBend.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.comboBox_elbowOrBend.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_elbowOrBend.FormattingEnabled = true;
            this.comboBox_elbowOrBend.Items.AddRange(new object[] {
            "Elbow",
            "Bend"});
            this.comboBox_elbowOrBend.Location = new System.Drawing.Point(163, 33);
            this.comboBox_elbowOrBend.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_elbowOrBend.Name = "comboBox_elbowOrBend";
            this.comboBox_elbowOrBend.Size = new System.Drawing.Size(156, 22);
            this.comboBox_elbowOrBend.TabIndex = 1;
            this.comboBox_elbowOrBend.SelectedIndexChanged += new System.EventHandler(this.comboBox_elbowOrBend_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.dataGridView_PipeMessage);
            this.groupBox1.Location = new System.Drawing.Point(13, 14);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(359, 186);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "选择管道";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.groupBox_bend);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.comboBox_elbowOrBend);
            this.groupBox2.Controls.Add(this.groupBox_elbow);
            this.groupBox2.Location = new System.Drawing.Point(13, 272);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox2.Size = new System.Drawing.Size(359, 281);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "配置所选管道的弯头";
            // 
            // groupBox_elbow
            // 
            this.groupBox_elbow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_elbow.Controls.Add(this.panel2);
            this.groupBox_elbow.Controls.Add(this.panel1);
            this.groupBox_elbow.Controls.Add(this.checkBox2);
            this.groupBox_elbow.Controls.Add(this.label_wanqubanjing);
            this.groupBox_elbow.Controls.Add(this.label3);
            this.groupBox_elbow.Controls.Add(this.comboBox_elbow_radius);
            this.groupBox_elbow.Controls.Add(this.comboBox_elbow_angle);
            this.groupBox_elbow.Location = new System.Drawing.Point(8, 60);
            this.groupBox_elbow.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox_elbow.Name = "groupBox_elbow";
            this.groupBox_elbow.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox_elbow.Size = new System.Drawing.Size(343, 208);
            this.groupBox_elbow.TabIndex = 0;
            this.groupBox_elbow.TabStop = false;
            this.groupBox_elbow.Text = "Elbow";
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.panel2.Controls.Add(this.radioButton3);
            this.panel2.Controls.Add(this.radioButton4);
            this.panel2.Location = new System.Drawing.Point(171, 119);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(143, 78);
            this.panel2.TabIndex = 6;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(8, 15);
            this.radioButton3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(61, 18);
            this.radioButton3.TabIndex = 0;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "一端切";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(8, 46);
            this.radioButton4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(61, 18);
            this.radioButton4.TabIndex = 1;
            this.radioButton4.TabStop = true;
            this.radioButton4.Text = "两端切";
            this.radioButton4.UseVisualStyleBackColor = true;
            this.radioButton4.CheckedChanged += new System.EventHandler(this.radioButton4_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.panel1.Controls.Add(this.radioButton2);
            this.panel1.Controls.Add(this.radioButton1);
            this.panel1.Location = new System.Drawing.Point(29, 119);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(143, 78);
            this.panel1.TabIndex = 5;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(6, 46);
            this.radioButton2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(97, 18);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "多根弯头切割";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(6, 15);
            this.radioButton1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(97, 18);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "一根弯头切割";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(30, 96);
            this.checkBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(98, 18);
            this.checkBox2.TabIndex = 4;
            this.checkBox2.Text = "布置切割弯头";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // label_wanqubanjing
            // 
            this.label_wanqubanjing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label_wanqubanjing.AutoSize = true;
            this.label_wanqubanjing.Location = new System.Drawing.Point(26, 32);
            this.label_wanqubanjing.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_wanqubanjing.Name = "label_wanqubanjing";
            this.label_wanqubanjing.Size = new System.Drawing.Size(83, 14);
            this.label_wanqubanjing.TabIndex = 0;
            this.label_wanqubanjing.Text = "弯曲半径类型:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(55, 66);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 14);
            this.label3.TabIndex = 2;
            this.label3.Text = "弯曲角度:";
            // 
            // comboBox_elbow_radius
            // 
            this.comboBox_elbow_radius.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.comboBox_elbow_radius.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_elbow_radius.FormattingEnabled = true;
            this.comboBox_elbow_radius.Items.AddRange(new object[] {
            "普通弯头",
            "长半径弯头",
            "短半径弯头",
            "内外丝弯头",
            "1.5倍弯曲半径弯头",
            "2倍弯曲半径弯头",
            "2.5倍弯曲半径弯头",
            "3倍弯曲半径弯头",
            "4倍弯曲半径弯头",
            "虾米弯"});
            this.comboBox_elbow_radius.Location = new System.Drawing.Point(156, 28);
            this.comboBox_elbow_radius.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_elbow_radius.Name = "comboBox_elbow_radius";
            this.comboBox_elbow_radius.Size = new System.Drawing.Size(157, 22);
            this.comboBox_elbow_radius.TabIndex = 1;
            this.comboBox_elbow_radius.SelectedIndexChanged += new System.EventHandler(this.comboBox_elbow_radius_SelectedIndexChanged);
            // 
            // comboBox_elbow_angle
            // 
            this.comboBox_elbow_angle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.comboBox_elbow_angle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_elbow_angle.FormattingEnabled = true;
            this.comboBox_elbow_angle.Items.AddRange(new object[] {
            "15度弯头",
            "30度弯头",
            "45度弯头",
            "60度弯头",
            "90度弯头"});
            this.comboBox_elbow_angle.Location = new System.Drawing.Point(155, 63);
            this.comboBox_elbow_angle.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_elbow_angle.Name = "comboBox_elbow_angle";
            this.comboBox_elbow_angle.Size = new System.Drawing.Size(157, 22);
            this.comboBox_elbow_angle.TabIndex = 3;
            this.comboBox_elbow_angle.SelectedIndexChanged += new System.EventHandler(this.comboBox_elbow_angle_SelectedIndexChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(13, 244);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(134, 18);
            this.checkBox1.TabIndex = 6;
            this.checkBox1.Text = "配置所有管道的弯头";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // radioCenter
            // 
            this.radioCenter.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.radioCenter.AutoSize = true;
            this.radioCenter.Checked = true;
            this.radioCenter.Location = new System.Drawing.Point(13, 213);
            this.radioCenter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioCenter.Name = "radioCenter";
            this.radioCenter.Size = new System.Drawing.Size(49, 18);
            this.radioCenter.TabIndex = 1;
            this.radioCenter.TabStop = true;
            this.radioCenter.Text = "中心";
            this.radioCenter.UseVisualStyleBackColor = true;
            this.radioCenter.CheckedChanged += new System.EventHandler(this.radioCenter_CheckedChanged);
            // 
            // radioTop
            // 
            this.radioTop.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.radioTop.AutoSize = true;
            this.radioTop.Location = new System.Drawing.Point(103, 213);
            this.radioTop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioTop.Name = "radioTop";
            this.radioTop.Size = new System.Drawing.Size(37, 18);
            this.radioTop.TabIndex = 2;
            this.radioTop.Text = "上";
            this.radioTop.UseVisualStyleBackColor = true;
            this.radioTop.CheckedChanged += new System.EventHandler(this.radioTop_CheckedChanged);
            // 
            // radioDown
            // 
            this.radioDown.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.radioDown.AutoSize = true;
            this.radioDown.Location = new System.Drawing.Point(178, 213);
            this.radioDown.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioDown.Name = "radioDown";
            this.radioDown.Size = new System.Drawing.Size(37, 18);
            this.radioDown.TabIndex = 3;
            this.radioDown.Text = "下";
            this.radioDown.UseVisualStyleBackColor = true;
            this.radioDown.CheckedChanged += new System.EventHandler(this.radioDown_CheckedChanged);
            // 
            // radioLeft
            // 
            this.radioLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.radioLeft.AutoSize = true;
            this.radioLeft.Location = new System.Drawing.Point(253, 213);
            this.radioLeft.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioLeft.Name = "radioLeft";
            this.radioLeft.Size = new System.Drawing.Size(37, 18);
            this.radioLeft.TabIndex = 4;
            this.radioLeft.Text = "左";
            this.radioLeft.UseVisualStyleBackColor = true;
            this.radioLeft.CheckedChanged += new System.EventHandler(this.radioLeft_CheckedChanged);
            // 
            // radioRight
            // 
            this.radioRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.radioRight.AutoSize = true;
            this.radioRight.Location = new System.Drawing.Point(328, 213);
            this.radioRight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioRight.Name = "radioRight";
            this.radioRight.Size = new System.Drawing.Size(37, 18);
            this.radioRight.TabIndex = 5;
            this.radioRight.Text = "右";
            this.radioRight.UseVisualStyleBackColor = true;
            this.radioRight.CheckedChanged += new System.EventHandler(this.radioRight_CheckedChanged);
            // 
            // groupBox_bend
            // 
            this.groupBox_bend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_bend.Controls.Add(this.textBox_lengthAfterBend);
            this.groupBox_bend.Controls.Add(this.textBox_lengthToBend);
            this.groupBox_bend.Controls.Add(this.textBox_radius);
            this.groupBox_bend.Controls.Add(this.textBox_radiusFactor);
            this.groupBox_bend.Controls.Add(this.label11);
            this.groupBox_bend.Controls.Add(this.label10);
            this.groupBox_bend.Controls.Add(this.label6);
            this.groupBox_bend.Controls.Add(this.label4);
            this.groupBox_bend.Location = new System.Drawing.Point(8, 60);
            this.groupBox_bend.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox_bend.Name = "groupBox_bend";
            this.groupBox_bend.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox_bend.Size = new System.Drawing.Size(342, 208);
            this.groupBox_bend.TabIndex = 8;
            this.groupBox_bend.TabStop = false;
            this.groupBox_bend.Text = "Bend";
            // 
            // textBox_lengthAfterBend
            // 
            this.textBox_lengthAfterBend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.textBox_lengthAfterBend.Location = new System.Drawing.Point(153, 164);
            this.textBox_lengthAfterBend.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_lengthAfterBend.Name = "textBox_lengthAfterBend";
            this.textBox_lengthAfterBend.Size = new System.Drawing.Size(157, 23);
            this.textBox_lengthAfterBend.TabIndex = 0;
            this.textBox_lengthAfterBend.Text = "0";
            this.textBox_lengthAfterBend.TextChanged += new System.EventHandler(this.textBox_lengthAfterBend_TextChanged);
            // 
            // textBox_lengthToBend
            // 
            this.textBox_lengthToBend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.textBox_lengthToBend.Location = new System.Drawing.Point(153, 118);
            this.textBox_lengthToBend.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_lengthToBend.Name = "textBox_lengthToBend";
            this.textBox_lengthToBend.Size = new System.Drawing.Size(157, 23);
            this.textBox_lengthToBend.TabIndex = 6;
            this.textBox_lengthToBend.Text = "0";
            this.textBox_lengthToBend.TextChanged += new System.EventHandler(this.textBox_lengthToBend_TextChanged);
            // 
            // textBox_radius
            // 
            this.textBox_radius.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.textBox_radius.Location = new System.Drawing.Point(154, 73);
            this.textBox_radius.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_radius.Name = "textBox_radius";
            this.textBox_radius.ReadOnly = true;
            this.textBox_radius.Size = new System.Drawing.Size(157, 23);
            this.textBox_radius.TabIndex = 4;
            this.textBox_radius.Text = "150";
            // 
            // textBox_radiusFactor
            // 
            this.textBox_radiusFactor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.textBox_radiusFactor.Location = new System.Drawing.Point(154, 28);
            this.textBox_radiusFactor.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_radiusFactor.Name = "textBox_radiusFactor";
            this.textBox_radiusFactor.Size = new System.Drawing.Size(157, 23);
            this.textBox_radiusFactor.TabIndex = 2;
            this.textBox_radiusFactor.Text = "1.5";
            this.textBox_radiusFactor.TextChanged += new System.EventHandler(this.textBox_radiusFactor_TextChanged);
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(46, 166);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(71, 14);
            this.label11.TabIndex = 7;
            this.label11.Text = "弯头后段长:";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(46, 121);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 14);
            this.label10.TabIndex = 5;
            this.label10.Text = "弯头前段长:";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(61, 76);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 14);
            this.label6.TabIndex = 3;
            this.label6.Text = "弯曲半径:";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(31, 31);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 14);
            this.label4.TabIndex = 0;
            this.label4.Text = "弯曲半径倍率:";
            // 
            // GroupPipeToolFrom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(385, 603);
            this.Controls.Add(this.radioRight);
            this.Controls.Add(this.radioLeft);
            this.Controls.Add(this.radioDown);
            this.Controls.Add(this.radioTop);
            this.Controls.Add(this.radioCenter);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GroupPipeToolFrom";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "成组布管";
            this.Load += new System.EventHandler(this.GroupPipeToolFrom_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_PipeMessage)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox_elbow.ResumeLayout(false);
            this.groupBox_elbow.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox_bend.ResumeLayout(false);
            this.groupBox_bend.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dataGridView_PipeMessage;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label13;
        public System.Windows.Forms.ComboBox comboBox_elbowOrBend;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn pipeLine;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClassName;
        private System.Windows.Forms.DataGridViewTextBoxColumn DN;
        private System.Windows.Forms.DataGridViewTextBoxColumn OD;
        private System.Windows.Forms.DataGridViewTextBoxColumn WALL_THICKNESS;
        public System.Windows.Forms.RadioButton radioCenter;
        public System.Windows.Forms.RadioButton radioTop;
        public System.Windows.Forms.RadioButton radioDown;
        public System.Windows.Forms.RadioButton radioLeft;
        public System.Windows.Forms.RadioButton radioRight;
        private System.Windows.Forms.GroupBox groupBox_elbow;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Label label_wanqubanjing;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.ComboBox comboBox_elbow_radius;
        public System.Windows.Forms.ComboBox comboBox_elbow_angle;
        private System.Windows.Forms.GroupBox groupBox_bend;
        public System.Windows.Forms.TextBox textBox_lengthAfterBend;
        public System.Windows.Forms.TextBox textBox_lengthToBend;
        public System.Windows.Forms.TextBox textBox_radius;
        public System.Windows.Forms.TextBox textBox_radiusFactor;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
    }
}