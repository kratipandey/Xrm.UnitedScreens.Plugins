using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using Xrm.US.Helper;
using Xrm.US.Helper.XrmHelper;

namespace Xrm.US.Entities.Deprecated
{
  public enum AccountCategoryCodeEnum
  {
    
  }
  public enum CustomerTypeCodeEnum
  {
    Prospect = 1,
    Customer = 2
  }

  /// <summary>
  /// Represents a subset of Entity: Account
  /// </summary
  internal class Account : EntityHelper
  {
    #region Entity attributes
    public KeyValue<string, EntityReference> ParentAccountId = new KeyValue<string, EntityReference>("parentaccountid", null); // FK: Account

    public KeyValue<string, EntityReference> AccountManager = new KeyValue<string, EntityReference>("ownerid", null); // FK: SystemUser   
    public KeyValue<string, string> AccountNumber = new KeyValue<string, string>("accountnumber", null);

    public KeyValue<string, OptionSetValue> CustomerTypeCode = new KeyValue<string, OptionSetValue>("customertypecode", null);
    public KeyValue<string, OptionSetValue> AccountCategoryCode = new KeyValue<string, OptionSetValue>("accountcategorycode", null);
    #endregion Entity attributes

    #region Enumeration
    #endregion Enumeration

    #region Linked Entity
    public Account LinkAccount
    {
      get;
      private set;
    }
    #endregion Linked Entity

    public Account(ServiceHelper svcHelper = null)
      : base("account", "name", svcHelper)
    {
    }

    /// <summary>
    /// Initialize property value defined in this class from the instance of Entity
    /// </summary>
    /// <param name="entity">An instance of Entity</param>
    public new void GetAttributesValue(Entity entity)
    {
      base.GetAttributesValue(entity);
      GetAttributeValue<EntityReference>(ref ParentAccountId, entity);
      GetAttributeValue<EntityReference>(ref AccountManager, entity);
      GetAttributeValue<OptionSetValue>(ref AccountCategoryCode, entity);
    }

    const string AccountAlias1 = "ALIAS1";
    const string AccountAlias2 = "ALIAS2";

    public EntityReference GetRootEntityReference(Guid accountId)
    {
      Account crmAccount = new Account();
      EntityReference entityRef = new EntityReference(LogicalName, accountId);
      EntityReference rootEntityRef = new EntityReference(LogicalName, accountId);

      string accountId1 = string.Format("{0}.{1}", AccountAlias1, crmAccount.Id.Key);
      string accountName1 = string.Format("{0}.{1}", AccountAlias1, crmAccount.Name.Key);

      string accountId2 = string.Format("{0}.{1}", AccountAlias2, crmAccount.Id.Key);
      string accountName2 = string.Format("{0}.{1}", AccountAlias2, crmAccount.Name.Key);

      string parentAccountId2 = string.Format("{0}.{1}", AccountAlias2, crmAccount.ParentAccountId.Key);

      AliasedValue aliasValue = null;
      for (; entityRef != null; )
      {
        List<Entity> accountSet = SvcHelper.RetrieveMultiple(new FetchExpression(GetFetchXml(entityRef.Id)));
        entityRef = null;
        if (accountSet[0].Contains(accountId2))
        {
          if (accountSet[0].Contains(parentAccountId2))
          {
            aliasValue = accountSet[0].GetAttributeValue<AliasedValue>(parentAccountId2);
            entityRef = (EntityReference)aliasValue.Value;
          }
          else
          {
            aliasValue = accountSet[0].GetAttributeValue<AliasedValue>(accountId2);
            rootEntityRef.Id = (Guid)aliasValue.Value;
            if (accountSet[0].Contains(accountName2))
            {
              aliasValue = accountSet[0].GetAttributeValue<AliasedValue>(accountName2);
              rootEntityRef.Name = (string)aliasValue.Value;
            }
          }
        }
        else if (accountSet[0].Contains(accountId1))
        {
          aliasValue = accountSet[0].GetAttributeValue<AliasedValue>(accountId1);
          rootEntityRef.Id = (Guid)aliasValue.Value;
          if (accountSet[0].Contains(accountName1))
          {
            aliasValue = accountSet[0].GetAttributeValue<AliasedValue>(accountName1);
            rootEntityRef.Name = (string)aliasValue.Value;
          }
        }
      }
      
      return rootEntityRef;
    }

    private string GetFetchXml(Guid accountId)
    {
      Account crmAccount = new Account();
      StringBuilder fetchXml = new StringBuilder();
      fetchXml.Append("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
      fetchXml.Append("<entity name='" + crmAccount.LogicalName + "'>");
      fetchXml.Append("<attribute name='" + crmAccount.Id.Key + "' />");
      fetchXml.Append("<attribute name='" + crmAccount.Name.Key + "' />");
      fetchXml.Append("<link-entity name='" + crmAccount.LogicalName + "' from='" + crmAccount.Id.Key + "' to='" + crmAccount.ParentAccountId.Key + "' alias='" + AccountAlias1 + "' link-type='outer'>");
      fetchXml.Append("<attribute name='" + crmAccount.Id.Key + "' />");
      fetchXml.Append("<attribute name='" + crmAccount.Name.Key + "' />");
      fetchXml.Append("<link-entity name='" + crmAccount.LogicalName + "' from='" + crmAccount.Id.Key + "' to='" + crmAccount.ParentAccountId.Key + "' alias='" + AccountAlias2 + "' link-type='outer'>");
      fetchXml.Append("<attribute name='" + crmAccount.Id.Key + "' />");
      fetchXml.Append("<attribute name='" + crmAccount.Name.Key + "' />");
      fetchXml.Append("<attribute name='" + crmAccount.ParentAccountId.Key + "' />");
      fetchXml.Append("</link-entity>");
      fetchXml.Append("</link-entity>");
      fetchXml.Append("<filter type='and'>");
      fetchXml.AppendFormat("<condition attribute='" + crmAccount.Id.Key + "' operator='eq' value='{0}' />", accountId);
      fetchXml.Append("</filter>");
      fetchXml.Append("<order attribute='" + crmAccount.Name.Key + "' descending='false' />");
      fetchXml.Append("</entity>");
      fetchXml.Append("</fetch>");
      return fetchXml.ToString();
    }
  }

  /// <summary>
  /// Represents a subset of Entity: Contact
  /// </summary
  internal class Contact : EntityHelper
  {
    #region Entity attributes
    public KeyValue<string, EntityReference> ParentCustomerId = new KeyValue<string, EntityReference>("parentcustomerid", null); // FK: Account/Contact
    public KeyValue<string, EntityReference> OwnerId = new KeyValue<string, EntityReference>("ownerid", null); // FK: SystemUser
    public KeyValue<string, EntityReference> AccountId = new KeyValue<string, EntityReference>("aca_accountid", null); // FK: Account
    public KeyValue<string, EntityReference> HierarchyId = new KeyValue<string, EntityReference>("aca_hierarchyid", null); // FK: Account

    public KeyValue<string, string> EmailAddress1 = new KeyValue<string, string>("emailaddress1", null);
    #endregion Entity attributes

    #region Enumeration
    #endregion Enumeration

    public Contact(ServiceHelper svcHelper = null)
      : base("contact", "fullname", svcHelper)
    {
    }

    /// <summary>
    /// Initialize property value defined in this class from the instance of Entity
    /// </summary>
    /// <param name="entity">An instance of Entity</param>
    public new void GetAttributesValue(Entity entity)
    {
      base.GetAttributesValue(entity);
      GetAttributeValue<EntityReference>(ref ParentCustomerId, entity);
      GetAttributeValue<EntityReference>(ref HierarchyId, entity);
    }
  }

  internal class Opportunity : EntityHelper
    {
        #region Entity attributes        
        public KeyValue<string, OptionSetValue> Opportunitytype = new KeyValue<string, OptionSetValue>("aca_opportunitytype", null);
        public KeyValue<string, DateTime?> GAdate = new KeyValue<string, DateTime?>("aca_gameavailabilitydate", null);
        public KeyValue<string, OptionSetValue> Producttype = new KeyValue<string, OptionSetValue>("aca_producttype", null);
        public KeyValue<string, string> Topic = new KeyValue<string, string>("name", null);
        public KeyValue<string, OptionSetValue> Opportunitystatus = new KeyValue<string, OptionSetValue>("aca_opportunitystatusproductexpansion", null);
        public KeyValue<string, EntityReference> BrandedGame = new KeyValue<string, EntityReference>("aca_brandedgameid", null);      
        public KeyValue<string, EntityReference> Account = new KeyValue<string, EntityReference>("parentaccountid", null);
        #endregion Entity attributes

        #region Enumeration
        #endregion Enumeration

        public Opportunity(ServiceHelper svcHelper = null)
          : base("opportunity", "name", svcHelper)
        {
        }

        /// <summary>
        /// Initialize property value defined in this class from the instance of Entity
        /// </summary>
        /// <param name="entity">An instance of Entity</param>
        public new void GetAttributesValue(Entity entity)
        {
            base.GetAttributesValue(entity);
        }
    }


  /// <summary>
  /// Represents a subset of Entity: SavedQuery
  /// </summary
  internal class SavedQuery : EntityHelper
  {
    #region Entity attributes
    public KeyValue<string, bool> IsPrivate = new KeyValue<string, bool>("isprivate", false);
    public KeyValue<string, bool> IsDefault = new KeyValue<string, bool>("isdefault", false);
    public KeyValue<string, bool> IsCustom = new KeyValue<string, bool>("iscustom", false);
    public KeyValue<string, bool> IsManaged = new KeyValue<string, bool>("ismanaged", false);
    public KeyValue<string, bool> CanBeDeleted = new KeyValue<string, bool>("canbedeleted", false);
    public KeyValue<string, bool> IsUserDefined = new KeyValue<string, bool>("isuserdefined", false);
    public KeyValue<string, bool> IsCustomizable = new KeyValue<string, bool>("iscustomizable", false);
    public KeyValue<string, bool> IsQuickFindQuery = new KeyValue<string, bool>("isquickfindquery", false);
    
    public KeyValue<string, string> LayoutXml = new KeyValue<string, string>("layoutxml", null);
    public KeyValue<string, string> FetchXml = new KeyValue<string, string>("fetchxml", null);

    //public KeyValue<string, int> QueryType = new KeyValue<string, int>("querytype", 0);
    //public KeyValue<string, int> ReturnTypeCode = new KeyValue<string, int>("returntypecode", 0);

    #endregion Entity attributes

    #region Enumeration
    #endregion Enumeration

    public SavedQuery(ServiceHelper svcHelper = null)
      : base("savedquery", "name", svcHelper)
    {
    }

    /// <summary>
    /// Initialize property value defined in this class from the instance of Entity
    /// </summary>
    /// <param name="entity">An instance of Entity</param>
    public new void GetAttributesValue(Entity entity)
    {
      base.GetAttributesValue(entity);
    }
  }

  /// <summary>
  /// Represents a subset of Entity: EmailActivity
  /// </summary
  internal class Email : EntityHelper
  {
    #region Entity attributes
    public KeyValue<string, Guid> ActivityId = new KeyValue<string, Guid>("activityid", Guid.Empty);
    
    public KeyValue<string, EntityCollection> From = new KeyValue<string, EntityCollection>("from", null); // FK: Activity Party
    public KeyValue<string, EntityCollection> To = new KeyValue<string, EntityCollection>("to", null); // FK: Activity Party
    public KeyValue<string, EntityCollection> Cc = new KeyValue<string, EntityCollection>("cc", null); // FK: Activity Party
    public KeyValue<string, EntityCollection> Bcc = new KeyValue<string, EntityCollection>("bcc", null); // FK: Activity Party
    public KeyValue<string, EntityReference> RegardingObjectId = new KeyValue<string, EntityReference>("regardingobjectid", null); // FK: Activity Party

    public KeyValue<string, EntityReference> ParentActivityId = new KeyValue<string, EntityReference>("parentactivityid", null); // FK: Activity

    public KeyValue<string, string> Sender = new KeyValue<string, string>("sender", null);
    public KeyValue<string, EntityReference> EmailSender = new KeyValue<string, EntityReference>("emailsender", null); // FK: Activity Party

    public KeyValue<string, bool> DirectionCode = new KeyValue<string, bool>("directioncode", false);
    public KeyValue<string, int> AttachmentCount = new KeyValue<string, int>("attachmentcount", 0);
    public KeyValue<string, string> Category = new KeyValue<string, string>("category", null);
    public KeyValue<string, string> SubCategory = new KeyValue<string, string>("subcategory", null);
    #endregion Entity attributes

    #region Enumeration
    public static bool Incoming = false;
    public static bool Outgoing = true;

    public new enum StateCodeEnum
    {
      Open = 0,
      Completed = 1,
      Canceled = 2
    }

    public new enum StatusCodeEnum
    {
      Draft = 1,
      Completed = 2,
      Sent = 3,
      Received = 4,
      Canceled = 5,
      PendingSend = 6,
      Sending = 7,
      Failed = 8
    }
    #endregion Enumeration

    public Email(ServiceHelper svcHelper = null)
      : base("email", "subject", svcHelper)
    {
      this.Id = this.ActivityId;
    }

    /// <summary>
    /// Initialize property value defined in this class from the instance of Entity
    /// </summary>
    /// <param name="entity">An instance of Entity</param>
    public new void GetAttributesValue(Entity entity)
    {
      base.GetAttributesValue(entity);
      GetAttributeValue<Guid>(ref ActivityId, entity);
      GetAttributeValue<EntityCollection>(ref From, entity);
      GetAttributeValue<EntityCollection>(ref To, entity);
      GetAttributeValue<EntityCollection>(ref Cc, entity);
      GetAttributeValue<EntityCollection>(ref Bcc, entity);
      GetAttributeValue<bool>(ref DirectionCode, entity);
      GetAttributeValue<string>(ref Description, entity);
      GetAttributeValue<EntityReference>(ref RegardingObjectId, entity);
      GetAttributeValue<int>(ref AttachmentCount, entity);
      GetAttributeValue<string>(ref Category, entity);
      GetAttributeValue<string>(ref SubCategory, entity);
      GetAttributeValue<string>(ref Sender, entity);
      GetAttributeValue<EntityReference>(ref EmailSender, entity);
    }

    public Entity Retrieve(Guid emailId)
    {
      ColumnSet columnSet = new ColumnSet(Id.Key, From.Key, To.Key, Cc.Key, Bcc.Key, Name.Key, Description.Key, AttachmentCount.Key,
        Sender.Key, EmailSender.Key);
      return SvcHelper.OrgSvc.Retrieve(LogicalName, emailId, columnSet);
    }
  }

  /// <summary>
  /// Represents a subset of Entity: EmailActivity
  /// </summary
  internal class Project : EntityHelper
  {
    #region Entity attributes
    public KeyValue<string, EntityReference> OpportunityId = new KeyValue<string, EntityReference>("us_opportunutyid", null); // FK: Activity
    

    public KeyValue<string, string> ProjectNr = new KeyValue<string, string>("us_projectnumber", null);
    public KeyValue<string, string> ProjectName = new KeyValue<string, string>("us_name", null);
    
    #endregion Entity attributes

    #region Enumeration
    
    #endregion Enumeration

    public Project(ServiceHelper svcHelper = null)
      : base("us_project", "us_anme", svcHelper)
    {
    }

    /// <summary>
    /// Initialize property value defined in this class from the instance of Entity
    /// </summary>
    /// <param name="entity">An instance of Entity</param>
    public new void GetAttributesValue(Entity entity)
    {
      base.GetAttributesValue(entity);
      //GetAttributeValue<EntityReference>(ref ActivityId, entity);
      //GetAttributeValue<EntityReference>(ref AttachmentId, entity);
      //GetAttributeValue<string>(ref AttachmentContentId, entity);
      //GetAttributeValue<Guid>(ref ActivityMimeAttachmentUnique, entity);
      //GetAttributeValue<EntityReference>(ref ObjectId, entity);
      //GetAttributeValue<string>(ref Subject, entity);
      //GetAttributeValue<string>(ref Body, entity);
      //GetAttributeValue<string>(ref MimeType, entity);
      //GetAttributeValue<int>(ref FileSize, entity);
      //GetAttributeValue<int>(ref AttachmentNumber, entity);
      //GetAttributeValue<OptionSetValue>(ref ObjectTypeCode, entity);
    }

    //public List<Entity> RetrieveEntitySet(Guid objectId)
    //{
    //  ColumnSet columnSet = new ColumnSet(Name.Key, ActivityId.Key, AttachmentId.Key, ObjectId.Key, AttachmentContentId.Key, 
    //    Subject.Key, Body.Key, MimeType.Key, FileSize.Key, AttachmentNumber.Key, ObjectTypeCode.Key);
    //  return RetrieveEntitySet(columnSet, new ConditionExpression(ObjectId.Key, ConditionOperator.Equal, objectId));
    //}
  }

  /// <summary>
  /// Represents a subset of Entity: ActivityParty
  /// </summary
  internal class aca_autonumbertest : EntityHelper
  {
    #region Entity attributes
        public KeyValue<string, string> entityname = new KeyValue<string, string>("us_entity_name", null);
        public KeyValue<string, Guid> autonumberId = new KeyValue<string, Guid>("us_autonumberId", Guid.Empty);
        public KeyValue<string, string> fieldname = new KeyValue<string, string>("us_field_name", null);
        public KeyValue<string, string> name = new KeyValue<string, string>("us_name", null);
        public KeyValue<string, string> postfix = new KeyValue<string, string>("us_postfix", null);
        public KeyValue<string, string> prefix = new KeyValue<string, string>("us_prefix", null);
        public KeyValue<string, string> preview = new KeyValue<string, string>("us_preview", null);
        public KeyValue<string, int> nextnumber = new KeyValue<string, int>("us_next_number", 0);
        public KeyValue<string, int> digits = new KeyValue<string, int>("us_digits", 0);

        public KeyValue<string, OptionSetValue> type = new KeyValue<string, OptionSetValue>("us_type", null);

        public KeyValue<string, DateTime?> nextreset = new KeyValue<string, DateTime?>("us_next_reset", null);
        public KeyValue<string, DateTime?> ScheduledStart = new KeyValue<string, DateTime?>("scheduledstart", null);
    #endregion Entity attributes

    #region Enumeration
    public enum typeEnum
    {
      Sender = 1, // Sender From
      ToRecipient = 2, // Recipient To
      CCRecipient = 3, // Recipient Cc
      BccRecipient = 4, // Recipient Bcc
      RequiredAttendee = 5, // Required Attendee
      OptionalAttendee = 6, // Optional Attendee
      Organizer = 7, // Organizer
      Regarding = 8, // Regarding 
      Owner = 9, // Activity Owner
      Resource = 10, // Resource
      Customer = 11, //Customer
    }
    #endregion Enumeration

    public aca_autonumbertest(ServiceHelper svcHelper = null)
      : base("us_autonumber", "us_name", svcHelper)
    {
    }

    /// <summary>
    /// Initialize property value defined in this class from the instance of Entity
    /// </summary>
    /// <param name="entity">An instance of Entity</param>
    public new void GetAttributesValue(Entity entity)
    {
      base.GetAttributesValue(entity);           

    }

    
  }

  /// <summary>
  /// Represents a subset of Entity: SystemUser
  /// </summary
  internal class SystemUser : EntityHelper
  {
    #region Entity attributes
    public KeyValue<string, EntityReference> QueueId = new KeyValue<string, EntityReference>("queueid", null); // FK: Queue

    #endregion Entity attributes

    #region Enumeration
    #endregion Enumeration

    public SystemUser(ServiceHelper svcHelper = null)
      : base("systemuser", "fullname", svcHelper)
    {
    }

    /// <summary>
    /// Initialize property value defined in this class from the instance of Entity
    /// </summary>
    /// <param name="entity">An instance of Entity</param>
    public new void GetAttributesValue(Entity entity)
    {
      base.GetAttributesValue(entity);
      GetAttributeValue<EntityReference>(ref QueueId, entity);
    }
  }

  /// <summary>
  /// Represents a subset of Entity: SystemUser
  /// </summary
  internal class Queue : EntityHelper
  {
    #region Entity attributes
    public KeyValue<string, string> EmailAddress = new KeyValue<string, string>("emailaddress", null);
    #endregion Entity attributes

    #region Enumeration
    #endregion Enumeration

    public Queue(ServiceHelper svcHelper = null)
      : base("queue", "name", svcHelper)
    {
    }

    /// <summary>
    /// Initialize property value defined in this class from the instance of Entity
    /// </summary>
    /// <param name="entity">An instance of Entity</param>
    public new void GetAttributesValue(Entity entity)
    {
      base.GetAttributesValue(entity);
      GetAttributeValue<string>(ref EmailAddress, entity);
    }
  }

  /// <summary>
  /// Represents a subset of Entity: UserSettings.
  /// </summary
  public partial class UserSettings : EntityHelper
  {
    #region Entity attributes
    public KeyValue<string, Guid> SystemUserId = new KeyValue<string, Guid>("systemuserid", Guid.Empty);
    public KeyValue<string, int> UiLanguageId = new KeyValue<string, int>("uilanguageid", 0);
    public KeyValue<string, string> CurrencySymbol = new KeyValue<string, string>("currencysymbol", string.Empty);
    public KeyValue<string, int?> TimeZoneCode = new KeyValue<string, int?>("timezonecode", 0);

    public KeyValue<string, int> PagingLimit = new KeyValue<string, int>("paginglimit", 0);
    #endregion Entity attributes

    #region Accessors
    #endregion Accessors

    #region Enumerations
    #endregion Enumerations

    /// <summary>
    /// Default Constructor.
    /// </summary>
    public UserSettings(ServiceHelper svcHelper = null)
      : base("usersettings", "userprofile", svcHelper)
    {
      this.Id = this.SystemUserId;;
    }
  }


}
