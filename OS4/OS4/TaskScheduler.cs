using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

//Дорожанская Софья, 9 группа, 3 курс
//Задача 8: планирование -- кратчайшая задача первая

namespace OS4
{
    class TaskScheduler
    {
        //текстбокс для печати информации о заданиях
        private static TextBox _tb;
        //очередь заданий
        private readonly SortedList _taskQueue;
        //очередь заблокированных заданий
        private readonly SortedList _blockedQueue;
        //максимальный и минимальный квант времени, случайно присваеваемый заданию
        private static int quantMax = 1500;
        private static int quantMin = 500;
        private static int quant = 700;
        //поток для исполнения планировщика
        private readonly Thread _scheduler;
        int taskNum = 4;

        //конструктор
        public TaskScheduler(TextBox tb) 
        {
            _tb = tb;
            _taskQueue = new SortedList();
            _blockedQueue = new SortedList();
            _scheduler = new Thread(Schedule);
        }

        //функция для вывода на форму текста
        public static void AppendToTextBox(string value)
        {
            if (_tb.InvokeRequired)
            {
                _tb.Invoke(new Action<string>(AppendToTextBox), value);
                return;
            }
            _tb.AppendText(value);
            Thread.Sleep(200);
        }

        //запуск планировщика
        public void Resume()
        {            
            if (_scheduler?.ThreadState == ThreadState.Unstarted)
                _scheduler.Start();
            else if (_scheduler?.ThreadState == ThreadState.Suspended)
                _scheduler.Resume();
        }

        //функция, возвращающая текущее состояние потока
        public ThreadState State()
        {
            return _scheduler.ThreadState;
        }

        //поставить поток на паузу
        public void Suspend()
        {
            _scheduler.Suspend();
        }

        //прерывание потока
        public void Abort()
        {
            while (_taskQueue.Count > 0)
            {
                _taskQueue.Remove();
            }
            while (_blockedQueue.Count > 0)
            {
                _blockedQueue.Remove();
            }
            if (_scheduler?.ThreadState == ThreadState.Suspended)
                _scheduler.Resume();
            _scheduler.Abort();
        }

        //запускаем планировщик
        private void Schedule()
        {
            //количество созданных изначально заданий
            int number = 1;
            Random r = new Random();
            int time;

            //инициализируем очередь заданным количеством заданий
            for (int i = 0; i < taskNum; i++)
            {
                time = r.Next(quantMin, quantMax);
                _taskQueue.Add(new PlannedTask(number, quant, time, this));
                AppendToTextBox("\r\n" + "Создано задание " + number + ", требуемое время " + time + "\r\n");
                number++;
            }

            //пока очереди не пусты, продолжаем работу планировщика
            while (_taskQueue.Count > 0 || _blockedQueue.Count > 0)
            {
                if (r.Next(100) < 25)
                {
                    time = r.Next(quantMin, quantMax);
                    _taskQueue.Add(new PlannedTask(number, quant, time, this));
                    AppendToTextBox("\r\n" + "Создано задание " + number + ", требуемое время " + time + "\r\n");
                    number++;
                }

                if (_taskQueue.Count == 1)
                {
                    time = r.Next(quantMin, quantMax);
                    _taskQueue.Add(new PlannedTask(number, quant, time, this));
                    AppendToTextBox("\r\n" + "Создано задание " + number + ", требуемое время " + time + "\r\n");
                    number++;
                }
                printAllTasks();

                //если в очереди заблокированных есть задания
                if (_blockedQueue.Count > 0)
                {
                    //уменьшаем значение времени ожидания для каждого
                    for (int i = 0; i < _blockedQueue.Count; ++i)
                    {
                        _blockedQueue[i].Tick();
                    }
                    int num = 0;
                    //ищем задания с истекшим временем ожидания
                    while (num < _blockedQueue.Count)
                    {
                        if (_blockedQueue[num].Timer == 0)
                        {
                            _taskQueue.Add(_blockedQueue[num]);
                            _blockedQueue.RemoveAt(num);
                            --num;
                        }
                        ++num;
                    }
                }

                //если очередь не пуста, достаем из нее новое задание
                if (_taskQueue.Count > 0)
                {
                    //достаем найденное задание из очереди
                    var task = _taskQueue.Remove();

                    AppendToTextBox("Задание " + task.Id + " выполняется. Выделенный квант времени: "
                        + task.Quantum + " Значение: "
                                    + task.CurrentCount + "\r\n");

                    //выполняем его
                    task.Resume();
                    Suspend();

                    //если задание не успело досчитать, выводим сообщение об этом
                    if (task.Timer > 0 && task.CurrentCount < task.countTo)
                    {
                        AppendToTextBox("Задание " + task.Id + " заблокировано. Досчитали до: "
                                        + task.CurrentCount + " Необходимо досчитать до " + task.countTo + "\r\n"
                                        +  "\r\n");
                        _blockedQueue.Add(task);
                        printAllTasks();
                    }
                    else if (task.CurrentCount == task.countTo)
                    {
                        AppendToTextBox("Задание " + task.Id + " окончило свою работу. Досчитали до: " + task.CurrentCount + "\r\n" + "\r\n");
                        task.Abort();
                    }
                    else
                    {
                        AppendToTextBox("Задание " + task.Id + " приостановлено. Досчитали до: "
                                        + task.CurrentCount + " Необходимо досчитать до " + task.countTo + "\r\n"
                                        + "\r\n");
                        _taskQueue.Add(task);
                        printAllTasks();
                    }
                }
                Thread.Sleep(200);
            }
            AppendToTextBox("Работа завершена!");
            Abort();
        }

        //метод будет вызван, когда задание оповещает планировщик о своем завершении или блокировке
        public static void TaskCompleteEvent(PlannedTask task, TaskScheduler scheduler, string taskStopped)
        {          
            if (taskStopped == "read/write")
            {
                AppendToTextBox("Попытка операции ввода-вывода" + "\r\n");
            }
            else if (taskStopped == "stopped")
            {
                AppendToTextBox("Задание " + task.Id + " приостановлено." + "\r\n");
            }
            else
            {
                AppendToTextBox("Задание " + task.Id + " завершено." + "\r\n");
            }
            scheduler.Resume();
            task.Suspend();
        }

        //печать текущего состояния очередей
        private void printAllTasks()
        {
            AppendToTextBox("Текущая очередь заданий: " + "\r\n");
            string s = "";
            for (int i = 0; i < _taskQueue.Count; ++i)
            {
                s += _taskQueue[i].Id + " (ост. время работы: " + _taskQueue[i].timeAmount + "); " +  " ";
            }
            if (s == "")
                AppendToTextBox("пусто" + "\r\n" + "\r\n");
            else
                AppendToTextBox(s + "\r\n" + "\r\n");

            AppendToTextBox("Очередь заблокированных заданий: " + "\r\n");
            s = "";
            for (int i = 0; i < _blockedQueue.Count; ++i)
            {
                s += _blockedQueue[i].Id + " (ост. время ожидания: " + _blockedQueue[i].Timer + "); ";
            }
            if (s == "")
                AppendToTextBox("пусто" + "\r\n" + "\r\n");
            else
                AppendToTextBox(s + "\r\n" + "\r\n");
        }
    }
}
