using BookAStar.Model.Auth.Account;
using System;

namespace BookAStar
{
    public class IdentificationChangedEventArgs : EventArgs
    {
        public User NewIdentification { get; private set; }
        public IdentificationChangedEventArgs(User newId)
        {
            NewIdentification = newId;
        }
    }
}