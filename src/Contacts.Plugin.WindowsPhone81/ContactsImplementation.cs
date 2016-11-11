using Plugin.Contacts.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Contacts
{
    public class ContactsImplementation : IContacts
    {
        public bool AggregateContactsSupported
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IQueryable<Contact> Contacts
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool LoadSupported
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool PreferContactAggregation
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool SingleContactsSupported
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Contact LoadContact(string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RequestPermission()
        {
            throw new NotImplementedException();
        }
    }
}
