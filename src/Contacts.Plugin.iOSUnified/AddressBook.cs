//
//  Copyright 2011-2013, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//

using System;
using System.Linq;
using System.Linq.Expressions;
using AddressBook;
using UIKit;
using Foundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Contacts.Abstractions;

namespace Plugin.Contacts
{
    [Preserve(AllMembers = true)]
    public class AddressBook
      : IQueryable<Contact> //IQueryable<Contact>
    {
        public AddressBook()
        {
            this.provider = new ContactQueryProvider(this.addressBook);
        }

        public Task<bool> RequestPermission()
        {


            var info = NSBundle.MainBundle.InfoDictionary;

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                if (!info.ContainsKey(new NSString("NSContactsUsageDescription")))
                    throw new UnauthorizedAccessException("On iOS 10 and higher you must set NSContactsUsageDescription in your Info.plist file to enable Authorization Requests for Photo Library access!");
            }


            var tcs = new TaskCompletionSource<bool>();
            if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
            {
                var status = ABAddressBook.GetAuthorizationStatus();
                if (status == ABAuthorizationStatus.Denied || status == ABAuthorizationStatus.Restricted)
                    tcs.SetResult(false);
                else
                {
                    if (this.addressBook == null)
                    {
                        this.addressBook = new ABAddressBook();
                        this.provider = new ContactQueryProvider(this.addressBook);
                    }

                    if (status == ABAuthorizationStatus.NotDetermined)
                    {
                        this.addressBook.RequestAccess((s, e) =>
                        {
                            tcs.SetResult(s);
                            if (!s)
                            {
                                this.addressBook.Dispose();
                                this.addressBook = null;
                                this.provider = null;
                            }
                        });
                    }
                    else
                        tcs.SetResult(true);
                }
            }
            else
                tcs.SetResult(true);

            return tcs.Task;
        }

        public IEnumerator<Contact> GetEnumerator()
        {
            CheckStatus();

            return this.addressBook.GetPeople().Select(ContactHelper.GetContact).GetEnumerator();
        }

        public Contact Load(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("id");

            CheckStatus();

            int rowId;
            if (!Int32.TryParse(id, out rowId))
                throw new ArgumentException("Not a valid contact ID", "id");

            ABPerson person = this.addressBook.GetPerson(rowId);
            if (person == null)
                return null;

            return ContactHelper.GetContact(person);
        }

        private ABAddressBook addressBook;
        private IQueryProvider provider;

        private void CheckStatus()
        {

            var info = NSBundle.MainBundle.InfoDictionary;

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                if (!info.ContainsKey(new NSString("NSContactsUsageDescription")))
                    throw new UnauthorizedAccessException("On iOS 10 and higher you must set NSContactsUsageDescription in your Info.plist file to enable Authorization Requests for Photo Library access!");
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
            {
                var status = ABAddressBook.GetAuthorizationStatus();
                if (status != ABAuthorizationStatus.Authorized)
                    throw new System.Security.SecurityException("AddressBook has not been granted permission");
            }

            if (this.addressBook == null)
            {
                this.addressBook = new ABAddressBook();
                this.provider = new ContactQueryProvider(this.addressBook);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        //		Type IQueryable.ElementType
        //		{
        //			get { return typeof(Contact); }
        //		}
        //		
        //		Expression IQueryable.Expression
        //		{
        //			get { return Expression.Constant (this); }
        //		}
        //		
        //		IQueryProvider IQueryable.Provider
        //		{
        //			get { return this.provider; }
        //		}

        Type IQueryable.ElementType
        {
            get { return typeof(Contact); }
        }

        Expression IQueryable.Expression
        {
            get { return Expression.Constant(this); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return this.provider; }
        }

    }
}