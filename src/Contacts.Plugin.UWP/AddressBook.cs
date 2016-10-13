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
        public Type ElementType => typeof(Contact);

        public Expression Expression => Expression.Constant(this);

        public IQueryProvider Provider
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerator<Contact> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
