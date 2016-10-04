using System;
using System.Collections.Generic;

namespace BookAStar
{
    public class ObservableString : IObservable<string>, IDisposable
    {
        public ObservableString(string data)
        {
            this.data = data;
        }
        private string data = null;
        private List<IObserver<string>> observers = new List<IObserver<string>>();

        public string Data
        {
            get
            {
                return data;
            }

            set
            {
                if (data != value)
                {
                    data = value;
                    foreach (var obs in observers)
                        obs.OnCompleted();
                }
            }
        }

        public override string ToString()
        {
            return data;
        }

        public static explicit operator ObservableString  (string data)
        {
            return new ObservableString(data);
        }

        public static explicit operator string (ObservableString observable)
        {
            return observable.ToString();
        }

        public void Dispose()
        {
            observers = null;
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            observers.Add(observer);
            return this;
        }
    }
}