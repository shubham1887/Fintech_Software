namespace YESBankUAT
{
    partial class Form1
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
            this.txtResponse = new System.Windows.Forms.TextBox();
            this.btnQRInit = new System.Windows.Forms.Button();
            this.btnFileSelect = new System.Windows.Forms.Button();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtMerchantKey = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtMID = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtAdminVPA = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.ofdFile = new System.Windows.Forms.OpenFileDialog();
            this.txtIBMClientSecret = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMCC = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtIBMClientID = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtAmount = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.picQR = new System.Windows.Forms.PictureBox();
            this.txtMerchantName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtVPA = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnVPAValidate = new System.Windows.Forms.Button();
            this.txtPrivatePassword = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtPrivateFilePath = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtAuthPassword = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtAuthUserName = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtOrderID = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.btnStatusCheck = new System.Windows.Forms.Button();
            this.txtCallback = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.btnCallbAck = new System.Windows.Forms.Button();
            this.txtMode = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picQR)).BeginInit();
            this.SuspendLayout();
            // 
            // txtResponse
            // 
            this.txtResponse.Location = new System.Drawing.Point(527, 11);
            this.txtResponse.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtResponse.Multiline = true;
            this.txtResponse.Name = "txtResponse";
            this.txtResponse.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResponse.Size = new System.Drawing.Size(537, 229);
            this.txtResponse.TabIndex = 30;
            // 
            // btnQRInit
            // 
            this.btnQRInit.Location = new System.Drawing.Point(148, 369);
            this.btnQRInit.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnQRInit.Name = "btnQRInit";
            this.btnQRInit.Size = new System.Drawing.Size(96, 41);
            this.btnQRInit.TabIndex = 29;
            this.btnQRInit.Text = "QR Init";
            this.btnQRInit.UseVisualStyleBackColor = true;
            this.btnQRInit.Click += new System.EventHandler(this.btnQRInit_Click);
            // 
            // btnFileSelect
            // 
            this.btnFileSelect.Location = new System.Drawing.Point(391, 20);
            this.btnFileSelect.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnFileSelect.Name = "btnFileSelect";
            this.btnFileSelect.Size = new System.Drawing.Size(77, 33);
            this.btnFileSelect.TabIndex = 49;
            this.btnFileSelect.Text = "File Select";
            this.btnFileSelect.UseVisualStyleBackColor = true;
            this.btnFileSelect.Click += new System.EventHandler(this.btnFileSelect_Click);
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(90, 20);
            this.txtFilePath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(285, 20);
            this.txtFilePath.TabIndex = 48;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(22, 23);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(54, 13);
            this.label13.TabIndex = 47;
            this.label13.Text = "JOSN File";
            // 
            // txtMerchantKey
            // 
            this.txtMerchantKey.Location = new System.Drawing.Point(183, 103);
            this.txtMerchantKey.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtMerchantKey.Name = "txtMerchantKey";
            this.txtMerchantKey.Size = new System.Drawing.Size(125, 20);
            this.txtMerchantKey.TabIndex = 44;
            this.txtMerchantKey.Text = "7c0675bd9e1b99c9a7776eb49ea6fe92";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(180, 84);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(70, 13);
            this.label9.TabIndex = 43;
            this.label9.Text = "MerchantKey";
            // 
            // txtMID
            // 
            this.txtMID.Location = new System.Drawing.Point(26, 103);
            this.txtMID.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtMID.Name = "txtMID";
            this.txtMID.Size = new System.Drawing.Size(120, 20);
            this.txtMID.TabIndex = 42;
            this.txtMID.Text = "YES0000000063923";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(26, 84);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(30, 13);
            this.label10.TabIndex = 41;
            this.label10.Text = "MID:";
            // 
            // txtAdminVPA
            // 
            this.txtAdminVPA.Location = new System.Drawing.Point(344, 103);
            this.txtAdminVPA.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtAdminVPA.Name = "txtAdminVPA";
            this.txtAdminVPA.Size = new System.Drawing.Size(124, 20);
            this.txtAdminVPA.TabIndex = 40;
            this.txtAdminVPA.Text = "ambika@yesb";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(338, 84);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(63, 13);
            this.label8.TabIndex = 39;
            this.label8.Text = "Admin VPA:";
            // 
            // ofdFile
            // 
            this.ofdFile.FileName = "openFileDialog1";
            this.ofdFile.Filter = "JSON file|*.json";
            // 
            // txtIBMClientSecret
            // 
            this.txtIBMClientSecret.Location = new System.Drawing.Point(344, 157);
            this.txtIBMClientSecret.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtIBMClientSecret.Name = "txtIBMClientSecret";
            this.txtIBMClientSecret.Size = new System.Drawing.Size(125, 20);
            this.txtIBMClientSecret.TabIndex = 53;
            this.txtIBMClientSecret.Text = "sI6cO2yV1eO8uY6mG4hM2pR1xR6iF2yI6pR7iU3iX0yF3lU8hJ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(341, 138);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 52;
            this.label1.Text = "IBMClientSecret";
            // 
            // txtMCC
            // 
            this.txtMCC.Location = new System.Drawing.Point(29, 157);
            this.txtMCC.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtMCC.Name = "txtMCC";
            this.txtMCC.Size = new System.Drawing.Size(120, 20);
            this.txtMCC.TabIndex = 51;
            this.txtMCC.Text = "1520";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 138);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 50;
            this.label2.Text = "MCC:";
            // 
            // txtIBMClientID
            // 
            this.txtIBMClientID.Location = new System.Drawing.Point(183, 157);
            this.txtIBMClientID.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtIBMClientID.Name = "txtIBMClientID";
            this.txtIBMClientID.Size = new System.Drawing.Size(125, 20);
            this.txtIBMClientID.TabIndex = 55;
            this.txtIBMClientID.Text = "2082624c-8f76-4714-a605-5c2da7ff63c6";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(180, 138);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 54;
            this.label3.Text = "IBMClientID";
            // 
            // txtAmount
            // 
            this.txtAmount.Location = new System.Drawing.Point(18, 389);
            this.txtAmount.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtAmount.Name = "txtAmount";
            this.txtAmount.Size = new System.Drawing.Size(126, 20);
            this.txtAmount.TabIndex = 57;
            this.txtAmount.Text = "2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 369);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 13);
            this.label4.TabIndex = 56;
            this.label4.Text = "Amount:";
            // 
            // picQR
            // 
            this.picQR.Location = new System.Drawing.Point(527, 244);
            this.picQR.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.picQR.Name = "picQR";
            this.picQR.Size = new System.Drawing.Size(537, 358);
            this.picQR.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picQR.TabIndex = 58;
            this.picQR.TabStop = false;
            // 
            // txtMerchantName
            // 
            this.txtMerchantName.Location = new System.Drawing.Point(32, 220);
            this.txtMerchantName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtMerchantName.Name = "txtMerchantName";
            this.txtMerchantName.Size = new System.Drawing.Size(120, 20);
            this.txtMerchantName.TabIndex = 60;
            this.txtMerchantName.Text = "---";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(32, 201);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 59;
            this.label5.Text = "M Name:";
            // 
            // txtVPA
            // 
            this.txtVPA.Location = new System.Drawing.Point(14, 444);
            this.txtVPA.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtVPA.Name = "txtVPA";
            this.txtVPA.Size = new System.Drawing.Size(126, 20);
            this.txtVPA.TabIndex = 63;
            this.txtVPA.Text = "9930738842@yesb";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 423);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(28, 13);
            this.label6.TabIndex = 62;
            this.label6.Text = "VPA";
            // 
            // btnVPAValidate
            // 
            this.btnVPAValidate.Location = new System.Drawing.Point(164, 423);
            this.btnVPAValidate.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnVPAValidate.Name = "btnVPAValidate";
            this.btnVPAValidate.Size = new System.Drawing.Size(96, 41);
            this.btnVPAValidate.TabIndex = 61;
            this.btnVPAValidate.Text = "VPA validate";
            this.btnVPAValidate.UseVisualStyleBackColor = true;
            this.btnVPAValidate.Click += new System.EventHandler(this.btnVPAValidate_Click);
            // 
            // txtPrivatePassword
            // 
            this.txtPrivatePassword.Location = new System.Drawing.Point(186, 222);
            this.txtPrivatePassword.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtPrivatePassword.Name = "txtPrivatePassword";
            this.txtPrivatePassword.Size = new System.Drawing.Size(134, 20);
            this.txtPrivatePassword.TabIndex = 65;
            this.txtPrivatePassword.Text = "q/6YBfdmai0zzYEmI3ybkbkxRLvQQ23WpjIFSB1mtSU=";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(184, 203);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 64;
            this.label7.Text = "Password";
            // 
            // txtPrivateFilePath
            // 
            this.txtPrivateFilePath.Location = new System.Drawing.Point(344, 220);
            this.txtPrivateFilePath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtPrivateFilePath.Name = "txtPrivateFilePath";
            this.txtPrivateFilePath.Size = new System.Drawing.Size(125, 20);
            this.txtPrivateFilePath.TabIndex = 67;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(341, 202);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(56, 13);
            this.label11.TabIndex = 66;
            this.label11.Text = "PrivateFile";
            // 
            // txtAuthPassword
            // 
            this.txtAuthPassword.Location = new System.Drawing.Point(186, 282);
            this.txtAuthPassword.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtAuthPassword.Name = "txtAuthPassword";
            this.txtAuthPassword.Size = new System.Drawing.Size(134, 20);
            this.txtAuthPassword.TabIndex = 71;
            this.txtAuthPassword.Text = "q/6YBfdmai0zzYEmI3ybkbkxRLvQQ23WpjIFSB1mtSU=";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(184, 263);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(78, 13);
            this.label12.TabIndex = 70;
            this.label12.Text = "Auth Password";
            // 
            // txtAuthUserName
            // 
            this.txtAuthUserName.Location = new System.Drawing.Point(32, 280);
            this.txtAuthUserName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtAuthUserName.Name = "txtAuthUserName";
            this.txtAuthUserName.Size = new System.Drawing.Size(120, 20);
            this.txtAuthUserName.TabIndex = 69;
            this.txtAuthUserName.Text = "---";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(32, 261);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(85, 13);
            this.label14.TabIndex = 68;
            this.label14.Text = "Auth UserName:";
            // 
            // txtOrderID
            // 
            this.txtOrderID.Location = new System.Drawing.Point(14, 506);
            this.txtOrderID.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtOrderID.Name = "txtOrderID";
            this.txtOrderID.Size = new System.Drawing.Size(126, 20);
            this.txtOrderID.TabIndex = 74;
            this.txtOrderID.Text = "9930738842@yesb";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(14, 485);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(47, 13);
            this.label15.TabIndex = 73;
            this.label15.Text = "Order ID";
            // 
            // btnStatusCheck
            // 
            this.btnStatusCheck.Location = new System.Drawing.Point(164, 485);
            this.btnStatusCheck.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnStatusCheck.Name = "btnStatusCheck";
            this.btnStatusCheck.Size = new System.Drawing.Size(96, 41);
            this.btnStatusCheck.TabIndex = 72;
            this.btnStatusCheck.Text = "Status Check";
            this.btnStatusCheck.UseVisualStyleBackColor = true;
            this.btnStatusCheck.Click += new System.EventHandler(this.btnStatusCheck_Click);
            // 
            // txtCallback
            // 
            this.txtCallback.Location = new System.Drawing.Point(268, 354);
            this.txtCallback.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtCallback.Multiline = true;
            this.txtCallback.Name = "txtCallback";
            this.txtCallback.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtCallback.Size = new System.Drawing.Size(256, 187);
            this.txtCallback.TabIndex = 77;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(268, 333);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(47, 13);
            this.label16.TabIndex = 76;
            this.label16.Text = "Order ID";
            // 
            // btnCallbAck
            // 
            this.btnCallbAck.Location = new System.Drawing.Point(344, 306);
            this.btnCallbAck.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCallbAck.Name = "btnCallbAck";
            this.btnCallbAck.Size = new System.Drawing.Size(96, 41);
            this.btnCallbAck.TabIndex = 75;
            this.btnCallbAck.Text = "Callback";
            this.btnCallbAck.UseVisualStyleBackColor = true;
            this.btnCallbAck.Click += new System.EventHandler(this.btnCallbAck_Click);
            // 
            // txtMode
            // 
            this.txtMode.Location = new System.Drawing.Point(341, 282);
            this.txtMode.Margin = new System.Windows.Forms.Padding(2);
            this.txtMode.Name = "txtMode";
            this.txtMode.Size = new System.Drawing.Size(125, 20);
            this.txtMode.TabIndex = 79;
            this.txtMode.Text = "UAT";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(338, 264);
            this.label17.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(34, 13);
            this.label17.TabIndex = 78;
            this.label17.Text = "Mode";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1129, 613);
            this.Controls.Add(this.txtMode);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.txtCallback);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.btnCallbAck);
            this.Controls.Add(this.txtOrderID);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.btnStatusCheck);
            this.Controls.Add(this.txtAuthPassword);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txtAuthUserName);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.txtPrivateFilePath);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtPrivatePassword);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtVPA);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnVPAValidate);
            this.Controls.Add(this.txtMerchantName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.picQR);
            this.Controls.Add(this.txtAmount);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtIBMClientID);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtIBMClientSecret);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtMCC);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnFileSelect);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txtMerchantKey);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtMID);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtAdminVPA);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtResponse);
            this.Controls.Add(this.btnQRInit);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "YES Bank UAT";
            ((System.ComponentModel.ISupportInitialize)(this.picQR)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtResponse;
        private System.Windows.Forms.Button btnQRInit;
        private System.Windows.Forms.Button btnFileSelect;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtMerchantKey;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtMID;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtAdminVPA;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.OpenFileDialog ofdFile;
        private System.Windows.Forms.TextBox txtIBMClientSecret;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMCC;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtIBMClientID;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtAmount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox picQR;
        private System.Windows.Forms.TextBox txtMerchantName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtVPA;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnVPAValidate;
        private System.Windows.Forms.TextBox txtPrivatePassword;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtPrivateFilePath;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtAuthPassword;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtAuthUserName;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtOrderID;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button btnStatusCheck;
        private System.Windows.Forms.TextBox txtCallback;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button btnCallbAck;
        private System.Windows.Forms.TextBox txtMode;
        private System.Windows.Forms.Label label17;
    }
}

