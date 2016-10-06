namespace DroneFlightTool {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
      this.l_status_ = new System.Windows.Forms.Label();
      this.status_ = new System.Windows.Forms.Label();
      this.connect_btn_ = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.control_panel_ = new System.Windows.Forms.FlowLayoutPanel();
      this.battery_ = new System.Windows.Forms.Label();
      this.altitude_ = new System.Windows.Forms.Label();
      this.speed_ = new System.Windows.Forms.Label();
      this.wifi_ = new System.Windows.Forms.Label();
      this.panel2.SuspendLayout();
      this.control_panel_.SuspendLayout();
      this.SuspendLayout();
      // 
      // l_status_
      // 
      this.l_status_.AutoSize = true;
      this.l_status_.Location = new System.Drawing.Point(3, 5);
      this.l_status_.Name = "l_status_";
      this.l_status_.Size = new System.Drawing.Size(40, 13);
      this.l_status_.TabIndex = 0;
      this.l_status_.Text = "Status:";
      // 
      // status_
      // 
      this.status_.AutoSize = true;
      this.status_.Location = new System.Drawing.Point(49, 5);
      this.status_.Name = "status_";
      this.status_.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.status_.Size = new System.Drawing.Size(73, 13);
      this.status_.TabIndex = 1;
      this.status_.Text = "Disconnected";
      // 
      // connect_btn_
      // 
      this.connect_btn_.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.connect_btn_.Location = new System.Drawing.Point(260, 0);
      this.connect_btn_.Name = "connect_btn_";
      this.connect_btn_.Size = new System.Drawing.Size(75, 23);
      this.connect_btn_.TabIndex = 2;
      this.connect_btn_.Text = "Connect";
      this.connect_btn_.UseVisualStyleBackColor = true;
      this.connect_btn_.Click += new System.EventHandler(this.connect_btn__Click);
      // 
      // panel2
      // 
      this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.panel2.Controls.Add(this.l_status_);
      this.panel2.Controls.Add(this.status_);
      this.panel2.Controls.Add(this.connect_btn_);
      this.panel2.Location = new System.Drawing.Point(16, 12);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(335, 24);
      this.panel2.TabIndex = 4;
      // 
      // control_panel_
      // 
      this.control_panel_.Controls.Add(this.battery_);
      this.control_panel_.Controls.Add(this.wifi_);
      this.control_panel_.Controls.Add(this.speed_);
      this.control_panel_.Controls.Add(this.altitude_);
      this.control_panel_.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
      this.control_panel_.Location = new System.Drawing.Point(16, 42);
      this.control_panel_.Name = "control_panel_";
      this.control_panel_.Size = new System.Drawing.Size(335, 65);
      this.control_panel_.TabIndex = 2;
      this.control_panel_.Visible = false;
      // 
      // battery_
      // 
      this.battery_.AutoSize = true;
      this.battery_.Location = new System.Drawing.Point(3, 0);
      this.battery_.Name = "battery_";
      this.battery_.Size = new System.Drawing.Size(72, 13);
      this.battery_.TabIndex = 0;
      this.battery_.Text = "Battery: 100%";
      // 
      // altitude_
      // 
      this.altitude_.AutoSize = true;
      this.altitude_.Location = new System.Drawing.Point(3, 39);
      this.altitude_.Name = "altitude_";
      this.altitude_.Size = new System.Drawing.Size(60, 13);
      this.altitude_.TabIndex = 1;
      this.altitude_.Text = "Altitude: 0ft";
      // 
      // speed_
      // 
      this.speed_.AutoSize = true;
      this.speed_.Location = new System.Drawing.Point(3, 26);
      this.speed_.Name = "speed_";
      this.speed_.Size = new System.Drawing.Size(68, 13);
      this.speed_.TabIndex = 2;
      this.speed_.Text = "Speed: 0 0 0";
      // 
      // wifi_
      // 
      this.wifi_.AutoSize = true;
      this.wifi_.Location = new System.Drawing.Point(3, 13);
      this.wifi_.Name = "wifi_";
      this.wifi_.Size = new System.Drawing.Size(67, 13);
      this.wifi_.TabIndex = 3;
      this.wifi_.Text = "Wi-Fi: 0 dBm";
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(363, 119);
      this.Controls.Add(this.control_panel_);
      this.Controls.Add(this.panel2);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "MainForm";
      this.Text = "Drone Control";
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.control_panel_.ResumeLayout(false);
      this.control_panel_.PerformLayout();
      this.ResumeLayout(false);

        }

    #endregion

    private System.Windows.Forms.Label l_status_;
    private System.Windows.Forms.Label status_;
    private System.Windows.Forms.Button connect_btn_;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.FlowLayoutPanel control_panel_;
    private System.Windows.Forms.Label battery_;
    private System.Windows.Forms.Label altitude_;
    private System.Windows.Forms.Label speed_;
    private System.Windows.Forms.Label wifi_;
  }
}

