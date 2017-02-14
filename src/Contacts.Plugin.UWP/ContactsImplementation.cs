using Plugin.Contacts.Abstractions;
using Plugin.Permissions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		/// <summary>
		/// Request permissions for Contacts
		/// </summary>
		/// <returns></returns>
		public async Task<bool> RequestPermission()
		{
			var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permissions.Abstractions.Permission.Contacts).ConfigureAwait(false);
			if (status != Permissions.Abstractions.PermissionStatus.Granted)
			{
#if DEBUG
                Debug.WriteLine("Currently does not have Contacts permissions, requesting permissions");
#endif
                var request = await CrossPermissions.Current.RequestPermissionsAsync(Permissions.Abstractions.Permission.Contacts);

				if (request[Permissions.Abstractions.Permission.Contacts] != Permissions.Abstractions.PermissionStatus.Granted)
				{
#if DEBUG
                    Debug.WriteLine("Contacts permission denied, can not get positions async.");
#endif
                    return false;
				}
			}

			return true;
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