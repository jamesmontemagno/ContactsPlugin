using System;
using System.Collections.Generic;
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
                    return Abstractions.EmailType.Other;
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
                    return Abstractions.PhoneType.Other;
            }
		}
	}

    internal static class ContactAddressKindExtension
    {
        public static Abstractions.AddressType ToAddressType(this ContactAddressKind value)
        {
            switch (value)
            {
                case ContactAddressKind.Home:
                    return Abstractions.AddressType.Home;
                case ContactAddressKind.Work:
                    return Abstractions.AddressType.Work;
                default:
                    return Abstractions.AddressType.Other;
            }
        }
    }

    internal static class ContactSignificantOtherKindExtension
    {
        public static Abstractions.RelationshipType ToRelationshipType(this ContactRelationship value)
        {
            switch (value)
            {
                case ContactRelationship.Child:
                    return Abstractions.RelationshipType.Child;
                case ContactRelationship.Spouse:
                case ContactRelationship.Partner:
                    return Abstractions.RelationshipType.SignificantOther;
                default:
                    return Abstractions.RelationshipType.Other;
            }
        }
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

		public IEnumerable<Abstractions.Contact> GetContacts()
		{
			var contactStore = ContactManager.RequestStoreAsync(
				ContactStoreAccessType.AllContactsReadOnly).AsTask().Result;

			var contacts = contactStore.FindContactsAsync().AsTask().Result;

            /*
			 * TODO: Why cannot write simple:
			 * UWPContactMapToPluginContact.Mapper.Map<IReadOnlyList<Contact>, IEnumerable<Abstractions.Contact>>(mutableContacts);
			 */

            return ConvertToContacts(contacts);
        }
        public static IEnumerable<Abstractions.Contact> ConvertToContacts(IReadOnlyCollection<Contact> contacts)
        {
            return contacts.Select(c => ConvertToContact(c));
        }
        public static Abstractions.Contact ConvertToContact(Contact contact)
        {

            return  new Abstractions.Contact(contact.Id, true)
            {
                Addresses = contact.Addresses.Select(a => new Abstractions.Address
                {
                    City = a.Locality,
                    Label = a.Kind.ToString(),
                    Country = a.Country,
                    PostalCode = a.PostalCode,
                    Region = a.Region,
                    StreetAddress = a.StreetAddress,
                    Type = a.Kind.ToAddressType(),
                }).ToList(),
                DisplayName = contact.DisplayName,
                Emails = contact.Emails.Select(a => new Abstractions.Email
                {
                    Address = a.Address,
                    Type = a.Kind.ToEmailType(),
                    Label = a.Description
                }).ToList(),
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                MiddleName = contact.MiddleName,
                Nickname = contact.Nickname,
                Notes = new List<Abstractions.Note>(new[] { new Abstractions.Note { Contents = contact.Notes } }),
                Organizations = contact.JobInfo.Select(a => new Abstractions.Organization
                {
                    ContactTitle = a.Title,
                    Label = a.Description,
                    Name = a.CompanyAddress,
                    Type = Abstractions.OrganizationType.Work
                }).ToList(),
                Phones = contact.Phones.Select(a => new Abstractions.Phone
                {
                    Label = a.Description,
                    Number = a.Number,
                    Type = a.Kind.ToPhoneType()
                }).ToList(),
                Prefix = contact.HonorificNamePrefix,
                Relationships = contact.SignificantOthers.Select(a => new Abstractions.Relationship
                {
                    Name = a.Name,
                    Type = a.Relationship.ToRelationshipType()
                }).ToList(),
                Suffix = contact.HonorificNameSuffix,
                Websites = contact.Websites.Select(a => new Abstractions.Website
                {
                    Address = a.Uri?.ToString() ?? string.Empty
                }).ToList(),
            };
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
