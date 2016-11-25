using AutoMapper;
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
		public static IMapper Mapper
			=> current?.mapper ?? (current = new UWPContactMapToPluginContact()).mapper;

		private sealed class DefaultMappingProfile : Profile
		{
			public DefaultMappingProfile()
			{
				CreateMap<ContactAddress, Abstractions.Address>()
					 .ForMember(x => x.Type, opt => opt.MapFrom(x => (Abstractions.AddressType)
						 Enum.Parse(typeof(Abstractions.AddressType), x.Kind.ToString(), true)))
					 .ForMember(x => x.Label, opt => opt.MapFrom(x => x.Kind.ToString()))
					 .ForMember(x => x.City, opt => opt.MapFrom(x => x.Locality));

				// TODO: Maybe better map with ContactRelationship
				CreateMap<ContactSignificantOther, Abstractions.Relationship>()
					.ForMember(x => x.Type, opt => opt.MapFrom(x => Abstractions.RelationshipType.SignificantOther));

				// Website
				CreateMap<ContactWebsite, Abstractions.Website>()
					.ForMember(x => x.Address, opt => opt.MapFrom(x => x.Uri.OriginalString));

				// Origanization
				CreateMap<ContactJobInfo, Abstractions.Organization>()
					.ForMember(x => x.Name, opt => opt.MapFrom(x => x.CompanyName))
					.ForMember(x => x.ContactTitle, opt => opt.MapFrom(x => x.Title))
					.ForMember(x => x.Type, opt => opt.MapFrom(x => Abstractions.OrganizationType.Work))
					.ForMember(x => x.Label, opt => opt.MapFrom(x => "Work"));

				// Email
				CreateMap<ContactEmail, Abstractions.Email>()
					.ForMember(x => x.Type, opt => opt.MapFrom(x => x.Kind.ToEmailType()))
					// TODO: Is mandatory same value in Type and Label?
					.ForMember(x => x.Label, opt => opt.MapFrom(x => x.Kind.ToString()))
					.ForMember(x => x.Address, opt => opt.MapFrom(x => x.Address));

				// Phone
				CreateMap<ContactPhone, Abstractions.Phone>()
					.ForMember(x => x.Type, opt => opt.MapFrom(x => x.Kind.ToPhoneType()))
					// TODO: Is mandatory same value in Type and Label?
					.ForMember(x => x.Label, opt => opt.MapFrom(x => x.Kind.ToString()))
					.ForMember(x => x.Number, opt => opt.MapFrom(x => x.Number));

				CreateMap<Contact, Abstractions.Contact>()
					.ForMember(x => x.Notes, opt => opt.MapFrom(
						x => new List<Abstractions.Note> { new Abstractions.Note() { Contents = x.Notes } }));

			}
		}

		private UWPContactMapToPluginContact()
		{
			configuration = new MapperConfiguration(cfg => cfg.AddProfile<DefaultMappingProfile>());

			mapper = configuration.CreateMapper();
		}

		private static UWPContactMapToPluginContact current;
		private MapperConfiguration configuration;
		private IMapper mapper;
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

			return UWPContactMapToPluginContact.Mapper.Map<List<Contact>, IEnumerable<Abstractions.Contact>>(mutableContacts);
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
