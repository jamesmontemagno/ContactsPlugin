using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Plugin.Contacts.UWP.Tests
{
	[TestClass]
	public class UnitTest1
	{
		[ClassInitialize]
		public static void SetupTests(TestContext testContext)
		{
			contacts = new List<Contact>();
			contacts.Add(AddContact("sala", "sali", "0009292992").Result);
			contacts.Add(AddContact("asala", "tsali", "1009292992").Result);
			contacts.Add(AddContact("bsala", "ysali", "2009292992").Result);
		}

		[ClassCleanup]
		public static void CleanUpTests()
		{
			if (contacts != null)
				foreach (Contact contact in contacts)
					DeleteContact(contact).Wait();
		}

		[TestMethod]
		public void GeneralTest()
		{
			GeneralTest(new ContactsImplementation());
		}

		[TestMethod]
		public void GeneralCrossPlatformTest()
		{
			GeneralTest(CrossContacts.Current);
		}

		[TestMethod]
		public async Task GeneralAsyncTest()
		{
			await GeneralAsyncTest(new ContactsImplementation());
		}

		[TestMethod]
		public async Task GeneralAsyncCrossPlatformTest()
		{
			await GeneralAsyncTest(CrossContacts.Current);
		}

		[TestMethod]
		public void ContactAddressMapToPluginAddressShouldBeFillAllFields()
		{
			var a = new ContactAddress()
			{
				Country = "C",
				Description = "D",
				Kind = ContactAddressKind.Home,
				Locality = "L",
				PostalCode = "P",
				Region = "R",
				StreetAddress = "S"
			};

            Abstractions.Address pluginAddress = new Abstractions.Address
            {
                City = a.Locality,
                Label = a.Kind.ToString(),
                Country = a.Country,
                PostalCode = a.PostalCode,
                Region = a.Region,
                StreetAddress = a.StreetAddress,
                Type = a.Kind.ToAddressType(),
            };


            Assert.IsNotNull(pluginAddress);
			Assert.AreEqual(a.Country, pluginAddress.Country);
			Assert.AreEqual(a.Locality, pluginAddress.City);
			Assert.AreEqual(a.PostalCode, pluginAddress.PostalCode);
			Assert.AreEqual(a.Region, pluginAddress.Region);
			Assert.AreEqual(a.StreetAddress, pluginAddress.StreetAddress);
			Assert.AreEqual((Abstractions.AddressType)
						 Enum.Parse(typeof(Abstractions.AddressType), a.Kind.ToString(), true),
						 pluginAddress.Type);
		}

		[TestMethod]
		public void ContactMapToPluginContactShouldBeWork()
		{
			var contact = new Contact()
			{
				FirstName = "lalal",
				LastName = "hghghg"
			};


            var pluginContact = ContactQueryProvider.ConvertToContact(contact);

            Assert.IsNotNull(pluginContact);
			Assert.AreEqual(contact.FirstName, pluginContact.FirstName);
			Assert.AreEqual(contact.LastName, pluginContact.LastName);
		}

		[TestMethod]
		public void PhoneContactMapToPluginContactShouldBeWork()
		{
			var contact = new Contact()
			{
				FirstName = "lalal",
				LastName = "hghghg"
			};

			contact.Phones.Add(new ContactPhone()
			{
				Description = "Test number",
				Kind = ContactPhoneKind.Mobile,
				Number = "1234567890"
			});

            var pluginContact = ContactQueryProvider.ConvertToContact(contact);


            Assert.IsNotNull(pluginContact);
			Assert.AreEqual(contact.FirstName, pluginContact.FirstName);
			Assert.AreEqual(contact.LastName, pluginContact.LastName);
			Assert.AreEqual(contact.Phones.First().Number, pluginContact.Phones.First().Number);
		}

		[TestMethod]
		public void CollectionContactsMapToPluginContactsShouldBeWoek()
		{
			IReadOnlyList<Contact> contacts = new List<Contact>()
			{
				CreateContact("ttt", "qqqq"),
				CreateContact("ggg", "aaa"),
				CreateContact("bbb", "zzz"),
			};

            var pluginContacts =
                ContactQueryProvider.ConvertToContacts(contacts);

        }

		private void GeneralTest(Abstractions.IContacts contactsInput)
		{
			if (contactsInput.RequestPermission().Result)
			{
				List<Plugin.Contacts.Abstractions.Contact> contacts = null;
				contactsInput.PreferContactAggregation = false;

				Task.Run(() =>
				{
					Assert.IsNotNull(contactsInput.Contacts);
					contacts = contactsInput.Contacts
						.Where(c => c.Phones.Count > 0 && c.FirstName.Length >= 5)
						.ToList();

					Assert.IsNotNull(contacts);
					Assert.AreEqual(2, contacts.Count);
				}).Wait();
			}
			else
				Assert.Fail();
		}

		private async Task GeneralAsyncTest(Abstractions.IContacts contactsInput)
		{
			if (await contactsInput.RequestPermission())
			{
				List<Plugin.Contacts.Abstractions.Contact> contacts = null;
				contactsInput.PreferContactAggregation = false;

				await Task.Run(() =>
				{
					Assert.IsNotNull(contactsInput.Contacts);
					contacts = contactsInput.Contacts
						.Where(c => c.Phones.Count > 0 && c.FirstName.Length >= 5)
						.ToList();

					Assert.IsNotNull(contacts);
					Assert.AreEqual(2, contacts.Count);
				});
			}
			else
				Assert.Fail();
		}

		private Contact CreateContact(string firstName, string lastName)
		{
			var contact = new Contact()
			{
				FirstName = firstName,
				LastName = lastName
			};

			var random = new Random();
			contact.Phones.Add(new ContactPhone()
			{
				Description = firstName + " " + lastName + " phone",
				Kind = ContactPhoneKind.Mobile,
				Number = random.Next((int)Math.Pow(10, 5), (int)Math.Pow(10, 6) - 1).ToString()
			});

			return contact;
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
