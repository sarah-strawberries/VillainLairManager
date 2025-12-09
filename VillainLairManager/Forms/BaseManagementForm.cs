using System;
using System.Windows.Forms;
using VillainLairManager.Services;

namespace VillainLairManager.Forms
{
    public partial class BaseManagementForm : Form
    {
        private readonly IRepository _repository;
        private readonly ISecretBaseService _secretBaseService;

        public BaseManagementForm(IRepository repository, ISecretBaseService secretBaseService)
        {
            _repository = repository;
            _secretBaseService = secretBaseService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Secret Base Management";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblStub = new Label
            {
                Text = "TODO: Implement Base Management Form",
                Location = new System.Drawing.Point(50, 50),
                Size = new System.Drawing.Size(800, 400),
                Font = new System.Drawing.Font("Arial", 10)
            };
            this.Controls.Add(lblStub);
        }
    }
}
