using System;
using System.Threading;

namespace OS4
{
    class PlannedTask
    {
        //начальное значение времени ожидания
        private const int initialTime = 2000;
        //значение, на которое будет меняться время ожидания
        private const int incrementTime = 500;
        //ID задания
        public int Id { get; }
        //значение до которого досчитали
        public long CurrentCount { get; private set; }
        //квант времени
        public int Quantum { get; private set; }
        public int timeAmount { get; private set; }
        public int countTo { get; private set; }
        //время ожидания заблокированного потока
        public int Timer { get; private set; }
        //делегат, которому передается статический метод планировщика
        //если задание было заблокировано или завершено, оно оповещает об этом планировщик
        private delegate void TaskCompleteDelegate(PlannedTask task, TaskScheduler scheduler, string taskStopped);
        private Thread thread;
        private TaskScheduler st;


        //конструктор
        public PlannedTask(int id, int q, int t, TaskScheduler _st)
        {
            Id = id;
            Quantum = q;
            timeAmount = t;
            thread = new Thread(Run);
            Timer = 0;
            st = _st;
            countTo = timeAmount;
        }

        //функция, уменьшающая время ожидания
        public void Tick()
        {
            Timer -= incrementTime;
        }

        public void Abort()
        {
            if (thread.ThreadState == ThreadState.Suspended)
            {
                thread.Resume();
            }
            thread.Abort();
        }

        public void Suspend()
        {
            try
            {
                if (thread.ThreadState == ThreadState.Running || thread.ThreadState == ThreadState.WaitSleepJoin) 
                    thread.Suspend();
            }
            catch { }
        }

        public void Resume()
        {
            if (thread.ThreadState == ThreadState.Suspended)        //если поток приостановлен
            {
                thread.Resume();
            }
            else
            {
                if (thread.ThreadState == ThreadState.Unstarted)    //если поток еще не запущен
                {
                    thread.Start();
                }
            }
        }

        //запуск задания
        public void Run() 
        {
            bool readwrite;
            while (CurrentCount < countTo) 
            {
                ++CurrentCount;
                --timeAmount;
                //с маленькой вероятностью запускаем операцию ввода-вывода
                readwrite = new Random().Next(1000) > 800;
                if (readwrite)
                {
                    //сообщаем планировщику, что задание было заблокировано операцией ввода-вывода
                    Timer = initialTime;
                    TaskCompleteDelegate eventReadWrite = TaskScheduler.TaskCompleteEvent;
                    eventReadWrite(this, st, "read/write");
                    //Suspend();
                    //return;
                }
                else if (CurrentCount % Quantum == 0)
                {
                    TaskCompleteDelegate eventReadWrite = TaskScheduler.TaskCompleteEvent;
                    eventReadWrite(this, st, "stopped");
                }
            }
            //сообщаем планировщику, что задание завершено 
            TaskCompleteDelegate eventTaskComlete = TaskScheduler.TaskCompleteEvent;
            eventTaskComlete(this, st, "finished");
            //Suspend();
        }
    }
}
