namespace DataIntegrationDemo
{
    partial class CategoriesAddApiForm
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
            btnAddCategory = new Button();
            SuspendLayout();
            // 
            // btnAddCategory
            // 
            btnAddCategory.Location = new Point(12, 25);
            btnAddCategory.Name = "btnAddCategory";
            btnAddCategory.Size = new Size(117, 63);
            btnAddCategory.TabIndex = 0;
            btnAddCategory.Text = "Kategorileri Apiye Ekle";
            btnAddCategory.UseVisualStyleBackColor = true;
            btnAddCategory.Click += btnAddCategory_Click;
            // 
            // CategoriesAddApiForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnAddCategory);
            Name = "CategoriesAddApiForm";
            Text = "CategoriesAddApiForm";
            ResumeLayout(false);
        }

        #endregion

        private Button btnAddCategory;
    }
}