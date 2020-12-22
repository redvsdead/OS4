using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace OS4
{
    //планировщик: первый - с наименьшим временем выполнения

    public partial class FormMain : Form
    {
        private static TaskScheduler _scheduler;

        public FormMain()
        {
            InitializeComponent();
            _scheduler = null;
            Application.Idle += OnIdle;
        }

        private void OnIdle(object sender, EventArgs e)
        {
            //если работа завершена, инициализируем кнопку старт
            if ((_scheduler != null) && (_scheduler.State() == ThreadState.Stopped))
            {
                btnPause.Invoke(new Action(() => btnPause.Enabled = false));
                btnPause.Invoke(new Action(() => btnStart.Enabled = true));
                _scheduler = null;
            }             
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //если планировщик не запущен, создаем новый
            if (_scheduler == null)
            {
                _scheduler = new TaskScheduler(tbInfo);
                tbInfo.Text = "";
            }
            _scheduler.Resume();
            btnPause.Invoke(new Action(() => btnPause.Enabled = true));
            btnPause.Invoke(new Action(() => btnStart.Enabled = false));

        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            _scheduler?.Suspend();
            btnPause.Invoke(new Action(() => btnPause.Enabled = false));
            btnPause.Invoke(new Action(() => btnStart.Enabled = true));
        }

        
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _scheduler?.Abort();
        }

        private void tbInfo_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
