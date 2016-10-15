using Nito.AsyncEx;
using Plugin.Contacts.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Plugin.Contacts
{
    /// <summary>
    /// Implementation for Contacts
    /// </summary>
    public class ContactsImplementation : IContacts
    {
        public Task<bool> RequestPermission()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                try
                {
                    // TODO: Is it better approach exists?
                    return AsyncContext.Run(async () => await
                        Windows.ApplicationModel.Contacts.ContactManager.RequestStoreAsync(
                        Windows.ApplicationModel.Contacts.ContactStoreAccessType.AllContactsReadOnly))
                        == null ? false : true;
                }
                catch (Exception)
                {
                    return false;
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        private AddressBook AddressBook
        {
            get { return addressBook ?? (addressBook = new AddressBook()); }
        }

        public IQueryable<Contact> Contacts => AddressBook;

        public Contact LoadContact(string id)
        {
            return AddressBook.Load(id);
        }

        public bool LoadSupported => false;

        public bool PreferContactAggregation
        {
            get; set;
        }

        public bool AggregateContactsSupported => true;

        public bool SingleContactsSupported => false;

        public bool IsReadOnly => true;

        private AddressBook addressBook;
    }
}