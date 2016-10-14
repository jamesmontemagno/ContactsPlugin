using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using System.Collections.Generic;
using System.Linq;
using Nito.AsyncEx;
using System.Diagnostics;

namespace Contacts.Plugin.UWP.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [ClassInitialize]
        public static void SetupTests(TestContext testContext)
        {
            contacts = new List<Contact>();
            contacts.Add(AsyncContext.Run(async () => await AddContact("sala", "sali", "0009292992")));
            contacts.Add(AsyncContext.Run(async () => await AddContact("asala", "tsali", "1009292992")));
            contacts.Add(AsyncContext.Run(async () => await AddContact("bsala", "ysali", "2009292992")));
        }

        [ClassCleanup]
        public static void CleanUpTests()
        {
            if (contacts != null)
                foreach (Contact contact in contacts)
                    AsyncContext.Run(async () => await DeleteContact(contact));
        }

        [TestMethod]
        public void TestMethod1()
        {
        }

        private static async Task<Contact> AddContact(string firstName, string lastName, string phoneNumber)
        {
            // Thanks to http://stackoverflow.com/a/34652963/1539100

            var contact = new Contact()
            {
                FirstName = firstName,
                LastName = lastName
            };

            contact.Phones.Add(new ContactPhone()
            {
                Number = phoneNumber,
                Kind = ContactPhoneKind.Mobile
            });

            /*
             * Get he contact store for the app (so no lists from outlook
             * and other stuff will be in the returned lists..)
             */
            var contactStore = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);

            IReadOnlyList<ContactList> contactLists = await contactStore.FindContactListsAsync();

            ContactList contactList = contactLists.Where(x => x.DisplayName == "TestList").FirstOrDefault()
                ?? await contactStore.CreateContactListAsync("TestList");

            await contactList.SaveContactAsync(contact);

            return contact;
        }

        //you can obviusly couple the changes better then this... this is just to show the basics 
        private static async Task DeleteContact(Contact contactToRemove)
        {
            var contactStore = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);

            ContactList contactList = await contactStore.GetContactListAsync(contactToRemove.ContactListId);

            Contact contact = await contactList.GetContactAsync(contactToRemove.Id);

            await contactList.DeleteContactAsync(contact);
        }

        private static IList<Contact> contacts;
    }
}
