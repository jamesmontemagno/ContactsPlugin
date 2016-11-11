using Plugin.Contacts.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Linq.Expressions;

namespace Plugin.Contacts
{
    public sealed class AddressBook : IQueryable<Contact>
    {
        public AddressBook()
        {
            provider = new ContactQueryProvider();
        }

        public Contact Load(string id)
        {
            throw new NotSupportedException();
        }

        public Type ElementType => typeof(Contact);

        public Expression Expression => Expression.Constant(this);

        public IQueryProvider Provider => provider;

        public IEnumerator<Contact> GetEnumerator()
        {
            return provider.GetContacts().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly ContactQueryProvider provider;
    }
}
