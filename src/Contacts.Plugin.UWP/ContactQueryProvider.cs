
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Contacts;

[assembly: InternalsVisibleTo("Contacts.Plugin.UWP.Tests")]

namespace Plugin.Contacts
{
	internal static class ContactEmailKindExtension
	{
		public static Abstractions.EmailType ToEmailType(this ContactEmailKind value)
		{
			switch (value)
			{
				case ContactEmailKind.Personal:
					return Abstractions.EmailType.Home;
				case ContactEmailKind.Work:
					return Abstractions.EmailType.Work;
				case ContactEmailKind.Other:
					return Abstractions.EmailType.Other;
				default:
					throw new ArgumentOutOfRangeException(nameof(value));
			}
		}
	}

	internal static class ContactPhoneKindExtension
	{
		public static Abstractions.PhoneType ToPhoneType(this ContactPhoneKind value)
		{
			switch (value)
			{
				case ContactPhoneKind.Home:
					return Abstractions.PhoneType.Home;
				case ContactPhoneKind.Mobile:
					return Abstractions.PhoneType.Mobile;
				case ContactPhoneKind.Work:
					return Abstractions.PhoneType.Work;
				case ContactPhoneKind.Other:
					return Abstractions.PhoneType.Other;
				case ContactPhoneKind.Pager:
					return Abstractions.PhoneType.Pager;
				case ContactPhoneKind.BusinessFax:
					return Abstractions.PhoneType.WorkFax;
				case ContactPhoneKind.HomeFax:
					return Abstractions.PhoneType.HomeFax;
				case ContactPhoneKind.Company:
					return Abstractions.PhoneType.Other;
				case ContactPhoneKind.Assistant:
					return Abstractions.PhoneType.Other;
				case ContactPhoneKind.Radio:
					return Abstractions.PhoneType.Other;
				default:
					throw new ArgumentOutOfRangeException(nameof(value));
			}
		}
	}

	internal class UWPContactMapToPluginContact
	{
        public IEnumerable<Abstractions.Contact> Convert(List<Contact> winContacts)
        {
            List<Abstractions.Contact> contacts = new List<Abstractions.Contact>();
            foreach (var winContact in winContacts)
            {
                contacts.Add(Convert(winContact));
            }
            return contacts.AsEnumerable();
        }

        public Abstractions.Contact Convert(Contact winContact)
        {
            Abstractions.Contact contact = new Abstractions.Contact(winContact.Id,winContact.IsAggregate);
            try
            {
                contact.FirstName = winContact.FirstName;
                contact.MiddleName = winContact.MiddleName;
                contact.LastName = winContact.LastName;
                contact.DisplayName = winContact.DisplayName;
                contact.Nickname = winContact.Nickname;
                contact.Prefix = winContact.HonorificNamePrefix;
                contact.Suffix = winContact.HonorificNameSuffix;
                contact.Phones = new List<Abstractions.Phone>();
                foreach (var phone in winContact.Phones)
                {
                    contact.Phones.Add(Convert(phone));
                }
                contact.Addresses = new List<Abstractions.Address>();
                foreach (var contactAddress in winContact.Addresses)
                {
                    contact.Addresses.Add(Convert(contactAddress));
                }
                contact.Emails = new List<Abstractions.Email>();
                foreach (var contactEmail in winContact.Emails)
                {
                    contact.Emails.Add(Convert(contactEmail));
                }
                contact.InstantMessagingAccounts = new List<Abstractions.InstantMessagingAccount>();
                foreach (var contactIM in winContact.ConnectedServiceAccounts)
                {
                    contact.InstantMessagingAccounts.Add(Convert(contactIM));
                }
                contact.Notes = new List<Abstractions.Note>() { new Abstractions.Note { Contents = winContact.Notes } };
                contact.Organizations = new List<Abstractions.Organization>();
                foreach (var jobInfo in winContact.JobInfo)
                {
                    contact.Organizations.Add(Convert(jobInfo));
                }
                contact.Websites = new List<Abstractions.Website>();
                foreach (var contactWebsite in winContact.Websites)
                {
                    contact.Websites.Add(Convert(contactWebsite));
                }
                contact.Relationships = new List<Abstractions.Relationship>();
                foreach (var relationship in winContact.SignificantOthers)
                {
                    contact.Relationships.Add(Convert(relationship));
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
            }
            
            return contact;
        }

        #region AddressConverters

        private Abstractions.Address Convert(ContactAddress contactAddress)
        {
            Abstractions.Address address = new Abstractions.Address();
            address.City = contactAddress.Locality;
            address.Country = contactAddress.Country;
            address.Label = contactAddress.Description;
            address.PostalCode = contactAddress.PostalCode;
            address.Region = contactAddress.Region;
            address.StreetAddress = contactAddress.StreetAddress;
            address.Type = Convert(contactAddress.Kind);
            return address;
        }

        private Abstractions.AddressType Convert(ContactAddressKind kind)
        {
            if (kind == ContactAddressKind.Home)
                return Abstractions.AddressType.Home;
            else if (kind == ContactAddressKind.Work)
                return Abstractions.AddressType.Work;
            else
                return Abstractions.AddressType.Other;
        }

        #endregion

        #region EmailConverters

        private Abstractions.Email Convert(ContactEmail contactEmail)
        {
            Abstractions.Email email = new Abstractions.Email();
            email.Address = contactEmail.Address;
            email.Label = contactEmail.Description;
            email.Type = Convert(contactEmail.Kind);
            return email;
        }

        private Abstractions.EmailType Convert(ContactEmailKind kind)
        {
            if (kind == ContactEmailKind.Personal)
                return Abstractions.EmailType.Home;
            else if (kind == ContactEmailKind.Work)
                return Abstractions.EmailType.Work;
            else
                return Abstractions.EmailType.Other;
        }
        #endregion

        private Abstractions.InstantMessagingAccount Convert(ContactConnectedServiceAccount contactIM)
        {
            Abstractions.InstantMessagingAccount im = new Abstractions.InstantMessagingAccount();
            im.Account = contactIM.Id;
            im.ServiceLabel = contactIM.ServiceName;
            return im;
        }

        private Abstractions.Organization Convert(ContactJobInfo jobInfo)
        {
            Abstractions.Organization org = new Abstractions.Organization();
            org.Name = jobInfo.CompanyName;
            org.Label = jobInfo.Description;
            org.ContactTitle = jobInfo.Department;
            return org;
        }

        #region EmailConverters

        private Abstractions.Phone Convert(ContactPhone contactPhone)
        {
            Abstractions.Phone phone = new Abstractions.Phone();
            phone.Number = contactPhone.Number;
            phone.Label = contactPhone.Description;
            phone.Type = Convert(contactPhone.Kind);
            return phone;
        }

        private Abstractions.PhoneType Convert(ContactPhoneKind kind)
        {
            Abstractions.PhoneType type = Abstractions.PhoneType.Other;
            switch (kind)
            {
                case ContactPhoneKind.Home:
                    type = Abstractions.PhoneType.Home;
                    break;
                case ContactPhoneKind.Mobile:
                    type = Abstractions.PhoneType.Mobile;
                    break;
                case ContactPhoneKind.Company:
                case ContactPhoneKind.Work:
                    type = Abstractions.PhoneType.Work;
                    break;
                case ContactPhoneKind.Other:
                    break;
                case ContactPhoneKind.Pager:
                    break;
                case ContactPhoneKind.BusinessFax:
                    type = Abstractions.PhoneType.WorkFax;
                    break;
                case ContactPhoneKind.HomeFax:
                    type = Abstractions.PhoneType.HomeFax;
                    break;
            }
            return type;
        }
        #endregion

        private Abstractions.Website Convert(ContactWebsite contactWebsite)
        {
            Abstractions.Website website = new Abstractions.Website();
            website.Address = contactWebsite.RawValue; // should be replace with Uri?
            return website;
        }

        #region RelationshipConverters

        private Abstractions.Relationship Convert(ContactSignificantOther contactRelationship)
        {
            Abstractions.Relationship relationship = new Abstractions.Relationship();
            relationship.Name = contactRelationship.Name;
            relationship.Type = Convert(contactRelationship.Relationship);
            return relationship;
        }

        private Abstractions.RelationshipType Convert(ContactRelationship relationship)
        {
            Abstractions.RelationshipType type = Abstractions.RelationshipType.Other;
            switch (relationship)
            {
                case ContactRelationship.Other:
                case ContactRelationship.Partner:
                case ContactRelationship.Sibling:
                case ContactRelationship.Parent:
                    type = Abstractions.RelationshipType.Other;
                    break;
                case ContactRelationship.Spouse:
                    type = Abstractions.RelationshipType.SignificantOther;
                    break;
                case ContactRelationship.Child:
                    type = Abstractions.RelationshipType.Child;
                    break;
            }
            return type;
        }
        #endregion

        //public static IMapper Mapper
        //	=> current?.mapper ?? (current = new UWPContactMapToPluginContact()).mapper;

        //private sealed class DefaultMappingProfile : Profile
        //{
        //	public DefaultMappingProfile()
        //	{
        //		CreateMap<ContactAddress, Abstractions.Address>()
        //			 .ForMember(x => x.Type, opt => opt.MapFrom(x => (Abstractions.AddressType)
        //				 Enum.Parse(typeof(Abstractions.AddressType), x.Kind.ToString(), true)))
        //			 .ForMember(x => x.Label, opt => opt.MapFrom(x => x.Kind.ToString()))
        //			 .ForMember(x => x.City, opt => opt.MapFrom(x => x.Locality));

        //		// TODO: Maybe better map with ContactRelationship
        //		CreateMap<ContactSignificantOther, Abstractions.Relationship>()
        //			.ForMember(x => x.Type, opt => opt.MapFrom(x => Abstractions.RelationshipType.SignificantOther));

        //		// Website
        //		CreateMap<ContactWebsite, Abstractions.Website>()
        //			.ForMember(x => x.Address, opt => opt.MapFrom(x => x.Uri.OriginalString));

        //		// Origanization
        //		CreateMap<ContactJobInfo, Abstractions.Organization>()
        //			.ForMember(x => x.Name, opt => opt.MapFrom(x => x.CompanyName))
        //			.ForMember(x => x.ContactTitle, opt => opt.MapFrom(x => x.Title))
        //			.ForMember(x => x.Type, opt => opt.MapFrom(x => Abstractions.OrganizationType.Work))
        //			.ForMember(x => x.Label, opt => opt.MapFrom(x => "Work"));

        //		// Email
        //		CreateMap<ContactEmail, Abstractions.Email>()
        //			.ForMember(x => x.Type, opt => opt.MapFrom(x => x.Kind.ToEmailType()))
        //			// TODO: Is mandatory same value in Type and Label?
        //			.ForMember(x => x.Label, opt => opt.MapFrom(x => x.Kind.ToString()))
        //			.ForMember(x => x.Address, opt => opt.MapFrom(x => x.Address));

        //		// Phone
        //		CreateMap<ContactPhone, Abstractions.Phone>()
        //			.ForMember(x => x.Type, opt => opt.MapFrom(x => x.Kind.ToPhoneType()))
        //			// TODO: Is mandatory same value in Type and Label?
        //			.ForMember(x => x.Label, opt => opt.MapFrom(x => x.Kind.ToString()))
        //			.ForMember(x => x.Number, opt => opt.MapFrom(x => x.Number));

        //		CreateMap<Contact, Abstractions.Contact>()
        //			.ForMember(x => x.Notes, opt => opt.MapFrom(
        //				x => new List<Abstractions.Note> { new Abstractions.Note() { Contents = x.Notes } }));

        //	}
        //}

        //private UWPContactMapToPluginContact()
        //{
        //	configuration = new MapperConfiguration(cfg => cfg.AddProfile<DefaultMappingProfile>());

        //	mapper = configuration.CreateMapper();
        //}

        //private static UWPContactMapToPluginContact current;
        //private MapperConfiguration configuration;
        //private IMapper mapper;
    }

    internal sealed class ContactQueryProvider : IQueryProvider
	{
		public IQueryable CreateQuery(Expression expression)
		{
			throw new NotImplementedException();
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			return new Query<TElement>(this, expression);
		}

		public object Execute(Expression expression)
		{
			IQueryable<Abstractions.Contact> q = GetContacts().AsQueryable();

			expression = ReplaceQueryable(expression, q);

			if (expression.Type.GetTypeInfo().IsGenericType &&
				expression.Type.GetGenericTypeDefinition() == typeof(IQueryable<>))
				return q.Provider.CreateQuery(expression);
			else
				return q.Provider.Execute(expression);
		}

		public TResult Execute<TResult>(Expression expression)
		{
			return (TResult)((IQueryProvider)this).Execute(expression);
		}

		public IEnumerable<Plugin.Contacts.Abstractions.Contact> GetContacts()
		{
			var contactStore = ContactManager.RequestStoreAsync(
				ContactStoreAccessType.AllContactsReadOnly).AsTask().Result;

			IReadOnlyList<Contact> contacts = contactStore.FindContactsAsync().AsTask().Result;

			/*
			 * TODO: Why cannot write simple:
			 * UWPContactMapToPluginContact.Mapper.Map<IReadOnlyList<Contact>, IEnumerable<Abstractions.Contact>>(mutableContacts);
			 */

			var mutableContacts = new List<Contact>();
			foreach (Contact contact in contacts)
				mutableContacts.Add(contact);

            //return UWPContactMapToPluginContact.Mapper.Map<List<Contact>, IEnumerable<Abstractions.Contact>>(mutableContacts);
            return new UWPContactMapToPluginContact().Convert(mutableContacts);
        }

		private Expression ReplaceQueryable(Expression expression, object value)
		{
			MethodCallExpression mc = expression as MethodCallExpression;
			if (mc != null)
			{
				Expression[] args = mc.Arguments.ToArray();
				Expression narg = ReplaceQueryable(mc.Arguments[0], value);
				if (narg != args[0])
				{
					args[0] = narg;
					return Expression.Call(mc.Method, args);
				}
				else
					return mc;
			}

			ConstantExpression c = expression as ConstantExpression;
			if (c != null && c.Type.GetInterfaces().Contains(typeof(IQueryable)))
				return Expression.Constant(value);

			return expression;
		}
	}
}
