using System.Windows.Forms;

namespace BankSwitcher
{
    public partial class LoadingForm : Form
    {
        public LoadingForm()
        {
            InitializeComponent();
        }

        public string labelText
        {
            get
            {
                return this.labelLoadingText.Text;
            }

            set
            {
                try
                {
                    this.labelLoadingText.Invoke((MethodInvoker)(() => labelLoadingText.Text = value));
                }
                catch
                {

                }
                
            }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
        }
    }
}
