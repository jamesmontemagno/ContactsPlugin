using Plugin.Contacts.Abstractions;
using System.Threading.Tasks;
using System.Linq;
using Foundation;

namespace Plugin.Contacts
{
    /// <summary>
    /// Implementation for Contacts
    /// </summary>
    [Preserve(AllMembers = true)]
    public class ContactsImplementation : IContacts
    {
        private AddressBook addressBook;
        private AddressBook AddressBook
        {
            get { return addressBook ?? (addressBook = new AddressBook()); }
        }

        public Task<bool> RequestPermission()
        {
            return AddressBook.RequestPermission();
        }

        public IQueryable<Contact> Contacts
        {
            get { return (IQueryable<Contact>)AddressBook; }
        }

        public Contact LoadContact(string id)
        {
            return AddressBook.Load(id);
        }

        public bool LoadSupported
        {
            get { return true; }
        }

        public bool PreferContactAggregation
        {
            get;
            set;
        }

        public bool AggregateContactsSupported
        {
            get { return true; }
        }

        public bool SingleContactsSupported
        {
            get { return true; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

    }
}