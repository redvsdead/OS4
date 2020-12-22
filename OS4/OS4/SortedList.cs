using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS4
{
    class SortedList : IEnumerable
    {
        List<PlannedTask> elements;

        public int Count { get { return elements.Count; } }

        public PlannedTask this[int key]
        {
            get
            {
                return elements[key];
            }
            set
            {
                elements[key] = value;
            }
        }

        public SortedList()
        {
            elements = new List<PlannedTask>();
        }

        // Добавляем элемент в список, и сортируем его
        public void Add(PlannedTask elem)
        {
            if (!elements.Contains(elem))
            {
                elements.Add(elem);
                elements.Sort((i1, i2) => i1.timeAmount.CompareTo(i2.timeAmount));
            }
        }

        // удаляем первый элемент из списка и возвращаем его
        public PlannedTask Remove()
        {
            PlannedTask th = elements[0];
            elements.RemoveAt(0);
            return th;
        }

        public void Remove(PlannedTask elem)
        {
            PlannedTask to_remove = elements.Find(el => el.Id == elem.Id);
            elements.Remove(to_remove);
        }

        public PlannedTask RemoveAt(int index)
        {
            PlannedTask th = elements[index];
            elements.RemoveAt(index);
            return th;
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)elements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)elements).GetEnumerator();
        }
    }
}
