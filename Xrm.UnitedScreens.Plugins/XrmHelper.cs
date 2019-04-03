// <copyright file="XrmHelper.cs" company="Acando AB">
// Copyright (c) 2016 All Rights Reserved.
// </copyright>
// <author>Acando AB</author>
// <date>1/16/2016</date>
// <summary>Implements helpers</summary>
using System;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Generic;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Crm.Sdk.Messages;
using Xrm.US.Helper.XrmHelper;

namespace Xrm.US.Helper.XrmHelper
{
  /// <summary>
  /// Mutable KeyValue pair
  /// </summary>
  /// <typeparam name="TKey"></typeparam>
  /// <typeparam name="TValue"></typeparam>
  [SerializableAttribute]
  public struct KeyValue<TKey, TValue>
  {
    public TKey Key
    {
      get;
      private set;
    }

    public TValue Value
    {
      get;
      set;
    }

    public KeyValue(TKey key, TValue value)
      : this()
    {
      this.Key = key;
      this.Value = value;
    }
  }

  /// <summary>
  /// Helper class for Entity
  /// </summary>
  public class EntityHelper
  {
    public const string Hyphen = "-";
    public const string Fullstop = ".";
    public const string Underscore = "_";

    public const string PatternId = "id";
    public const string PatternName = "name";

    public string LogicalName
    {
      get;
      private set;
    }

    public string PrimaryFieldName
    {
      get;
      private set;
    }

    public string EntityAlias
    {
      get;
      set;
    }

    public ServiceHelper SvcHelper
    {
      get;
      private set;
    }

    #region Entity attributes
    public KeyValue<string, Guid> Id = new KeyValue<string, Guid>(PatternId, Guid.Empty);
    public KeyValue<string, string> Name = new KeyValue<string, string>(PatternName, null);

    public KeyValue<string, EntityReference> CreatedBy = new KeyValue<string, EntityReference>("createdby", null);
    public KeyValue<string, DateTime?> CreatedOn = new KeyValue<string, DateTime?>("createdon", null);
    public KeyValue<string, EntityReference> ModifiedBy = new KeyValue<string, EntityReference>("modifiedby", null);
    public KeyValue<string, DateTime?> ModifiedOn = new KeyValue<string, DateTime?>("modifiedon", null);

    public KeyValue<string, string> Description = new KeyValue<string, string>("description", null);

    public KeyValue<string, OptionSetValue> StateCode = new KeyValue<string, OptionSetValue>("statecode", null);
    public KeyValue<string, OptionSetValue> StatusCode = new KeyValue<string, OptionSetValue>("statuscode", null);

    public enum StateCodeEnum
    {
      Active = 0,
      Inactive = 1
    };
    public enum StatusCodeEnum
    {
      Active = 1,
      Inactive = 2
    };
    #endregion Entity attributes

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logicalName">Logical name of Entity</param>
    /// <param name="primaryFieldName">Primary field name of Entity.</param>
    public EntityHelper(string logicalName, string primaryFieldName)
    {
      this.LogicalName = logicalName;
      this.PrimaryFieldName = primaryFieldName;
      Initialize();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logicalName">Logical name of Entity</param>
    /// <param name="primaryFieldName">Primary field name of Entity.</param>
    /// <param name="svcHelper">Instance of custom ServiceHelper class</param>
    public EntityHelper(string logicalName, string primaryFieldName, ServiceHelper svcHelper)
    {
      this.LogicalName = logicalName;
      this.PrimaryFieldName = primaryFieldName;
      this.SvcHelper = svcHelper;
      Initialize();
    }

    #region Private/Protected
    private void Initialize()
    {
      Id = new KeyValue<string, Guid>(this.LogicalName + "id", Guid.Empty);
      Name = new KeyValue<string, string>(this.PrimaryFieldName, null);
      EntityAlias = string.Format("ALIAS_{0}_{1}",
        this.LogicalName.ToUpper(), Guid.NewGuid().ToString().Replace(Hyphen, string.Empty).ToUpperInvariant());
    }

    /// <summary>
    /// Gets the value of the attribute.
    /// </summary>
    /// <returns>
    /// The value of the attribute.
    /// </returns>
    /// <param name="attributeLogicalName">The logical name of the attribute.</param>
    protected virtual T GetAttributeValue<T>(Object objectValue)
    {
      return ((objectValue == null))
        ? default(T) : (typeof(AliasedValue).Equals(objectValue.GetType()) ? (T)(((AliasedValue)objectValue).Value) : (T)objectValue);
    }

    protected virtual T GetAttributeValue<T>(string attributeName, Entity entity)
    {
      return (entity == null)
        ? default(T) : entity.Attributes.Contains(attributeName) ? entity.GetAttributeValue<T>(attributeName) : default(T);
    }

    protected virtual void GetAttributeValue<T>(ref KeyValue<string, T> keyValue, Entity entity)
    {
      keyValue.Value = (entity == null)
        ? default(T) : entity.Attributes.Contains(keyValue.Key) ? entity.GetAttributeValue<T>(keyValue.Key) : default(T);
    }
    #endregion Private/Protected

    /// <summary>
    /// Get defined attribute values from the instane of Entity
    /// </summary>
    /// <param name="entity">Instance of Entity</param>
    public virtual void GetAttributesValue(Entity entity)
    {
      GetAttributeValue<Guid>(ref Id, entity);
      GetAttributeValue<string>(ref Name, entity);
      GetAttributeValue<EntityReference>(ref CreatedBy, entity);
      GetAttributeValue<DateTime?>(ref CreatedOn, entity);
      GetAttributeValue<EntityReference>(ref ModifiedBy, entity);
      GetAttributeValue<DateTime?>(ref ModifiedOn, entity);
      GetAttributeValue<OptionSetValue>(ref StateCode, entity);
      GetAttributeValue<OptionSetValue>(ref StatusCode, entity);
    }

    public virtual string GetEntityAlias(string attributeLogicalName = null)
    {
      return string.Format("{0}{1}{2}", EntityAlias,
        (string.IsNullOrWhiteSpace(attributeLogicalName) ? string.Empty : Fullstop), attributeLogicalName);
    }

    public virtual List<Entity> RetrieveEntitySet(QueryExpression queryExpr, params FilterExpression[] filters)
    {
      if (!queryExpr.ColumnSet.Columns.Contains(Id.Key)) queryExpr.ColumnSet.AddColumns(Id.Key);

      if (filters != null) queryExpr.Criteria.Filters.AddRange(filters);

      return SvcHelper.RetrieveMultiple(queryExpr);
    }

    public virtual List<Entity> RetrieveEntitySet(ColumnSet columnSet = null, params FilterExpression[] filters)
    {
      QueryExpression queryExpr = new QueryExpression(LogicalName);
      if (columnSet != null) queryExpr.ColumnSet = columnSet;

      if (!queryExpr.ColumnSet.Columns.Contains(Id.Key)) queryExpr.ColumnSet.AddColumns(Id.Key);
      if (filters != null) queryExpr.Criteria.Filters.AddRange(filters);

      return SvcHelper.RetrieveMultiple(queryExpr);
    }

    public virtual List<Entity> RetrieveEntitySet(QueryExpression queryExpr, params ConditionExpression[] conditions)
    {
      if (!queryExpr.ColumnSet.Columns.Contains(Id.Key)) queryExpr.ColumnSet.AddColumns(Id.Key);

      if (conditions != null) queryExpr.Criteria.Conditions.AddRange(conditions);

      return SvcHelper.RetrieveMultiple(queryExpr);
    }

    public virtual List<Entity> RetrieveEntitySet(ColumnSet columnSet = null, params ConditionExpression[] conditions)
    {
      QueryExpression queryExpr = new QueryExpression(LogicalName);
      if (columnSet != null) queryExpr.ColumnSet = columnSet;

      if (!queryExpr.ColumnSet.Columns.Contains(Id.Key)) queryExpr.ColumnSet.AddColumns(Id.Key);
      if (conditions != null) queryExpr.Criteria.Conditions.AddRange(conditions);

      return SvcHelper.RetrieveMultiple(queryExpr);
    }
  }
}

namespace Xrm.US.Helper.XrmHelper
{
  public sealed class SettingContext
  {
    const string ID = "id";
    const string TARGET = "Target";
    const int DEFAULT_LANGUAGE_CODE = 1033;
    const int DEFAULT_RECORD_PER_PAGE = 1000;
    const int DEFAULT_REQUEST_PER_BATCH = 20;

    /// <summary>
    /// Language code. Default is 1033.
    /// </summary>
    public int LanguageCode
    {
      get;
      set;
    }

    /// <summary>
    /// Number of record(s) per page. Default is 1000.
    /// </summary>
    public int RecordPerPage
    {
      get;
      set;
    }

    /// <summary>
    /// Number of request(s) per batch. Default is 20. Larger value may result in timeout.
    /// </summary>
    public int RequestPerBatch
    {
      get;
      set;
    }

    /// <summary>
    /// How to execute batch, asynchronous or synchronous. For larger batch asynchronous result in performance.
    /// </summary>
    public bool IsSynchronous
    {
      get;
      set;
    }

    public string Target
    {
      get
      {
        return TARGET;
      }
    }

    public string Id
    {
      get
      {
        return ID;
      }
    }

    public SettingContext()
    {
      this.LanguageCode = DEFAULT_LANGUAGE_CODE;
      this.RecordPerPage = DEFAULT_RECORD_PER_PAGE;
      this.RequestPerBatch = DEFAULT_REQUEST_PER_BATCH;
      this.IsSynchronous = false;
    }
  }

  public class ServiceHelperBase : IDisposable
  {
    private Guid _callerId;

    public SettingContext SettingCtxt
    {
      get;
      set;
    }

    internal CrmConnection CrmConnection
    {
      get;
      set;
    }

    public OrganizationServiceProxy OrgSvcProxy
    {
      get;
      set;
    }

    public OrganizationServiceContext OrgSvcCtxt
    {
      get;
      set;
    }

    private IOrganizationService _orgSvc = null;
    public IOrganizationService OrgSvc
    {
      get
      {
        return (OrgSvcProxy != null) ? (IOrganizationService)this.OrgSvcProxy : _orgSvc;
      }
      set
      {
        _orgSvc = value;
      }
    }

    internal ServiceHelperBase(IOrganizationService orgSvc)
    {
      OrgSvc = orgSvc;
      this.SettingCtxt = new SettingContext();
    }

    public ServiceHelperBase(string connectionStringName)
    {
      CrmConnection = new CrmConnection(connectionStringName);

      this.SettingCtxt = new SettingContext();
      
      this.OrgSvcProxy = new OrganizationServiceProxy(CrmConnection.ServiceUri, CrmConnection.HomeRealmUri, CrmConnection.ClientCredentials, CrmConnection.DeviceCredentials);
      this.OrgSvcCtxt = new OrganizationServiceContext(this.OrgSvcProxy);
      this.OrgSvcCtxt.MergeOption = MergeOption.NoTracking;
      this._callerId = OrgSvcProxy.CallerId;
    }

    public void ResetCallerId(Guid callerId)
    {
      if (OrgSvcProxy != null)
        this.OrgSvcProxy.CallerId = (!Guid.Empty.Equals(callerId)) ? callerId : _callerId;
    }

    public void ResetCallerId(Guid? callerId)
    {
      if (OrgSvcProxy != null)
        this.OrgSvcProxy.CallerId = (callerId != null) ? callerId.Value : _callerId;
    }

    public virtual T Execute<T>(OrganizationRequest orgReq)
    {
      return (T)Convert.ChangeType(OrgSvc.Execute(orgReq), typeof(T));
    }

    public Tuple<List<Entity>, List<Tuple<Entity, string>>> ExecuteMultiple<T>(List<Entity> entitySet)
    {
      // Prevents AgrumentNullException.
      Tuple<List<Entity>, List<Tuple<Entity, string>>> entitySetOkFail =
        new Tuple<List<Entity>, List<Tuple<Entity, string>>>(new List<Entity>(), new List<Tuple<Entity, string>>());

      // Create instance of ExecuteMultipleRequest message for bulk operation.
      ExecuteMultipleRequest exeMultiReq = new ExecuteMultipleRequest()
      {
        //Assign settings that define execution behavior: ContinueOnError, ReturnResponses. 
        Settings = new ExecuteMultipleSettings
        {
          ContinueOnError = true,
          ReturnResponses = SettingCtxt.IsSynchronous
        },
        Requests = new OrganizationRequestCollection()
      };

      object instOfOrgReq = null;

      for (int iReqNum = 0, iBatchNum = 0; iReqNum < entitySet.Count; iReqNum++)
      {
        // OBS!!! Request Message must be a CreateRequest.
        instOfOrgReq = Activator.CreateInstance(typeof(T));

        if ((typeof(EntityReference).Equals(instOfOrgReq.GetType().GetProperty(SettingCtxt.Target).PropertyType)))
          instOfOrgReq.GetType().GetProperty(SettingCtxt.Target).SetValue(instOfOrgReq, entitySet[iReqNum].ToEntityReference());
        else
          instOfOrgReq.GetType().GetProperty(SettingCtxt.Target).SetValue(instOfOrgReq, entitySet[iReqNum]);

        exeMultiReq.Requests.Add((OrganizationRequest)instOfOrgReq);

        if (((iReqNum + 1) % SettingCtxt.RequestPerBatch != 0) && (iReqNum + 1) < entitySet.Count) continue;

        iBatchNum += 1;

        // Execute all Request in the ExecuteMultipleRequest collection using a single web method. Possibly result in timeout Exception.
        ExecuteMultipleResponse exeMultiResp = Execute<ExecuteMultipleResponse>(exeMultiReq);
        ParseExecuteMultipleResponse(exeMultiReq, exeMultiResp, entitySetOkFail);
        exeMultiReq.Requests.Clear(); // Reset Requests instance for next Batch.
      }

      return entitySetOkFail;
    }

    public virtual List<Entity> RetrieveMultiple(QueryBase queryBase)
    {
      return OrgSvc.RetrieveMultiple(queryBase).Entities.ToList();
    }

    public void Dispose()
    {
      if (OrgSvc != null) OrgSvc = null;
      if (OrgSvcCtxt != null) OrgSvcCtxt = null;
      if (OrgSvcProxy != null) OrgSvcProxy = null;
    }

    protected void ParseExecuteMultipleResponse(ExecuteMultipleRequest exeMultiReq, ExecuteMultipleResponse exeMultiResp,
      Tuple<List<Entity>, List<Tuple<Entity, string>>> entitySetOkFail)
    {
      for (int iRespNum = 0, iReqNum = 0; iRespNum < exeMultiResp.Responses.Count; iRespNum++)
      {
        iReqNum = exeMultiResp.Responses[iRespNum].RequestIndex;

        object target = null;
        exeMultiReq.Requests[iReqNum].Parameters.TryGetValue(SettingCtxt.Target, out target);

        Entity entity = (target != null) ? (typeof(EntityReference).Equals(target.GetType()) ? new Entity() : (Entity)target) : null;

        if (exeMultiResp.Responses[iRespNum].Response != null)
        {
          if (entity != null && exeMultiResp.Responses[iRespNum].Response.Results.ContainsKey(SettingCtxt.Id))
            entity.Id = (Guid)exeMultiResp.Responses[iRespNum].Response.Results[SettingCtxt.Id];
          entitySetOkFail.Item1.Add(entity);
        }
        else if (exeMultiResp.Responses[iRespNum].Fault != null)
        {
          OrganizationServiceFault orgSvcFault = exeMultiResp.Responses[iRespNum].Fault;
          entitySetOkFail.Item2.Add(new Tuple<Entity, string>(entity, string.Format("MAIN: {0}{1}\n{2}", orgSvcFault.Message,
            ((orgSvcFault.InnerFault != null) ? string.Format("INNER: {0}", orgSvcFault.InnerFault.Message) : string.Empty), orgSvcFault.TraceText)));
        }
      }
    }
  }

  public sealed class ServiceHelper : ServiceHelperBase
  {
    public ServiceHelper(IOrganizationService orgSvc)
      : base (orgSvc)
    {
      OrgSvc = orgSvc;
    }

    public ServiceHelper(string connectionStringName)
      : base(connectionStringName)
    {
    }

    /// <summary>
    /// Convert FetchXml to QueryExpression
    /// </summary>
    /// <param name="fetchExpr">An instance of FetchExpression to be converted to QueryExpression</param>
    public QueryExpression FetchXmlToQueryExpression(FetchExpression fetchExpr)
    {
      FetchXmlToQueryExpressionRequest orgReq = new FetchXmlToQueryExpressionRequest
      {
        FetchXml = fetchExpr.Query
      };

      return Execute<FetchXmlToQueryExpressionResponse>(orgReq).Query;
    }

    /// <summary>
    /// Convert QueryExpression to FetchXml
    /// </summary>
    /// <param name="queryExpr">An instance of QueryExpression to be converted to FetchExpression</param>
    public FetchExpression QueryExpressionToFetchXml(QueryExpression queryExpr)
    {
      QueryExpressionToFetchXmlRequest orgReq = new QueryExpressionToFetchXmlRequest
      {
        Query = queryExpr
      };

      return new FetchExpression(Execute<QueryExpressionToFetchXmlResponse>(orgReq).FetchXml);
    }

    /// <summary>
    /// Convert Local datetime from UTC datetime
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <param name="timeZoneCode"></param>
    public DateTime LocalFromUtcDateTime(DateTime utcDateTime, int? timeZoneCode)
    {
      if (!timeZoneCode.HasValue) return utcDateTime;

      LocalTimeFromUtcTimeRequest orgReq = new LocalTimeFromUtcTimeRequest
      {
        TimeZoneCode = timeZoneCode.Value,
        UtcTime = utcDateTime
      };

      return Execute<LocalTimeFromUtcTimeResponse>(orgReq).LocalTime;
    }

    /// <summary>
    /// Convert UTC datetime from UTC datetime
    /// </summary>
    /// <param name="localDateTime"></param>
    /// <param name="timeZoneCode"></param>
    public DateTime UtcFromLocalDateTime(DateTime localDateTime, int? timeZoneCode)
    {
      if (!timeZoneCode.HasValue) return localDateTime;

      UtcTimeFromLocalTimeRequest orgReq = new UtcTimeFromLocalTimeRequest
      {
        TimeZoneCode = timeZoneCode.Value,
        LocalTime = localDateTime
      };

      return Execute<UtcTimeFromLocalTimeResponse>(orgReq).UtcTime;
    }

    /// <summary>
    /// Helper method to retrieve Entity instances.
    /// </summary>
    /// <param name="queryExpr">An instance of QueryExpression</param>
    /// <returns>An instance of EntityCollection if callback not provided.</returns>
    public List<Entity> RetrieveMultiple(QueryExpression queryExpr)
    {
      // Define the paging attributes. Initialize the page number. Assign the PageInfo properties to the QueryExpression.
      queryExpr.PageInfo = new PagingInfo();
      queryExpr.PageInfo.Count = SettingCtxt.RecordPerPage;
      queryExpr.PageInfo.PageNumber = 1;
      queryExpr.PageInfo.PagingCookie = null; // The current paging cookie. PagingCookie should be null for first page.

      // Create instance of RetrieveMultipleRequest message.
      RetrieveMultipleRequest retMultiReq = new RetrieveMultipleRequest();

      // Set Query Criteria for the retrieval in RetrieveMultipleRequest message.
      retMultiReq.Query = queryExpr;

      List<Entity> entitySet = new List<Entity>(); // Prevents AgrumentNullException.

      RetrieveMultipleResponse retMultiResp = null;
      do
      {
        // Request OrganizationService to Execute the RetrieveMultipleRequest message.
        retMultiResp = Execute<RetrieveMultipleResponse>(retMultiReq);

        if (retMultiResp.EntityCollection.Entities != null)
        {
          entitySet.AddRange(retMultiResp.EntityCollection.Entities);
        }

        // Check for more records, if it returns true.
        if (retMultiResp.EntityCollection.MoreRecords)
        {
          queryExpr.PageInfo.PageNumber += 1; // Increment the page number to retrieve the next page.
          queryExpr.PageInfo.PagingCookie = retMultiResp.EntityCollection.PagingCookie; // Set the paging cookie to the paging cookie returned from current results.
        }
      } while (retMultiResp.EntityCollection.MoreRecords); // Exit the loop, if no more record(s) are available in result nodes.

      return entitySet;
    }

    public delegate void RetrieveMultipleResultPage(List<Entity> entitySet);

    /// <summary>
    /// Helper method to retrieve Entity instances.
    /// </summary>
    /// <param name="queryExpr">An instance of QueryExpression.</param>
    /// <param name="callbackRetrieveMultipleResultPage">Callback function to handle retrieved Entity instances for each page.</param>
    /// <returns>An instance of EntityCollection if callback not provided.</returns>
    public void RetrieveMultiple(QueryExpression queryExpr, RetrieveMultipleResultPage callbackRetrieveMultipleResultPage)
    {
      // Define the paging attributes. Initialize the page number. Assign the PageInfo properties to the QueryExpression.
      queryExpr.PageInfo = new PagingInfo();
      queryExpr.PageInfo.Count = SettingCtxt.RecordPerPage;
      queryExpr.PageInfo.PageNumber = 1;
      queryExpr.PageInfo.PagingCookie = null; // The current paging cookie. PagingCookie should be null for first page.

      // Create instance of RetrieveMultipleRequest message.
      RetrieveMultipleRequest retMultiReq = new RetrieveMultipleRequest();

      // Set Query Criteria for the retrieval in RetrieveMultipleRequest message.
      retMultiReq.Query = queryExpr;

      RetrieveMultipleResponse retMultiResp = null;
      do
      {
        // Request OrganizationService to Execute the RetrieveMultipleRequest message.
        retMultiResp = Execute<RetrieveMultipleResponse>(retMultiReq);

        if (retMultiResp.EntityCollection.Entities != null)
          callbackRetrieveMultipleResultPage(retMultiResp.EntityCollection.Entities.ToList());

        // Check for more records, if it returns true.
        if (retMultiResp.EntityCollection.MoreRecords)
        {
          queryExpr.PageInfo.PageNumber += 1; // Increment the page number to retrieve the next page.
          queryExpr.PageInfo.PagingCookie = retMultiResp.EntityCollection.PagingCookie; // Set the paging cookie to the paging cookie returned from current results.
        }
      } while (retMultiResp.EntityCollection.MoreRecords); // Exit the loop, if no more record(s) are available in result nodes.
    }

    /// <summary>
    /// Helper to set State and Status of Entity instance.
    /// </summary>
    /// <param name="entity">An instance of Entity</param>
    /// <param name="stateCodeType">A valid state code.</param>
    /// <param name="statusCodeType">A valid status reason code</param>
    public SetStateResponse SetState(Entity entity, Enum stateCodeEnum, Enum statusCodeEnum)
    {
      return SetState(entity, Convert.ToInt32(stateCodeEnum), Convert.ToInt32(statusCodeEnum));
    }

    /// <summary>
    /// Helper to set State and Status of Entity instance.
    /// </summary>
    /// <param name="entity">An instance of Entity.</param>
    /// <param name="stateCode">A valid state code.</param>
    /// <param name="statusCode">A valid status reason code</param>
    public SetStateResponse SetState(Entity entity, int stateCode, int statusCode)
    {
      SetStateRequest setStateReq = new SetStateRequest
      {
        EntityMoniker = entity.ToEntityReference(),
        State = new OptionSetValue(stateCode),
        Status = new OptionSetValue(statusCode)
      };

      return Execute<SetStateResponse>(setStateReq);
    }

    /// <summary>
    /// Bulk Helper to set State and Status of Entity instance.
    /// </summary>
    /// <returns>Entity instances for which operation succeeded and failed.</returns>
    /// <param name="entitySet">Entity instances.</param>
    /// <param name="stateCodeType">A valid state code.</param>
    /// <param name="statusCodeType">A valid status reason code</param>
    public Tuple<List<Entity>, List<Tuple<Entity, string>>> SetStateMultiple(List<Entity> entitySet, Enum stateCodeType, Enum statusCodeType)
    {
      // Prevents AgrumentNullException.
      Tuple<List<Entity>, List<Tuple<Entity, string>>> entitySetOkFail =
        new Tuple<List<Entity>, List<Tuple<Entity, string>>>(new List<Entity>(), new List<Tuple<Entity, string>>());

      // Create instance of ExecuteMultipleRequest message for bulk update.
      ExecuteMultipleRequest exeMultiReq = new ExecuteMultipleRequest()
      {
        //Assign settings that define execution behavior: ContinueOnError, ReturnResponses. 
        Settings = new ExecuteMultipleSettings
        {
          ContinueOnError = true,
          ReturnResponses = SettingCtxt.IsSynchronous
        },
        Requests = new OrganizationRequestCollection()
      };

      Entity entity = null;
      for (int iReqNum = 0, iBatchNum = 0; iReqNum < entitySet.Count; iReqNum++)
      {
        entity = entitySet[iReqNum];

        exeMultiReq.Requests.Add(new SetStateRequest
        {
          State = new OptionSetValue(Convert.ToInt32(stateCodeType)),
          Status = new OptionSetValue(Convert.ToInt32(statusCodeType)),
          EntityMoniker = entity.ToEntityReference()
        });

        if (((iReqNum + 1) % SettingCtxt.RequestPerBatch != 0) && (iReqNum + 1) < entitySet.Count) continue;

        iBatchNum += 1;

        // Execute all Request in the ExecuteMultipleRequest collection using a single web method.
        // Possibly result in timeout Exception.
        ExecuteMultipleResponse exeMultiResp = Execute<ExecuteMultipleResponse>(exeMultiReq);

        ParseExecuteMultipleResponse(exeMultiReq, exeMultiResp, entitySetOkFail);
        exeMultiReq.Requests.Clear(); // Reset Requests instance for next Batch.
      }

      return entitySetOkFail;
    }

    public List<OptionMetadata> RetrieveOptionSet(string entityLogicalName, string attributeLogicalName)
    {
      RetrieveAttributeRequest retAttributeReq = new RetrieveAttributeRequest
      {
        EntityLogicalName = entityLogicalName, // Logical name of the Entity that contains the attribute.
        LogicalName = attributeLogicalName, // Logical name of the attribute to be retrieved.
        RetrieveAsIfPublished = true // Retrieves metadata that has not been published.
      };

      RetrieveAttributeResponse retAttributeResp = Execute<RetrieveAttributeResponse>(retAttributeReq);

      List<OptionMetadata> listOfOption = new List<OptionMetadata>();
      listOfOption.AddRange(((PicklistAttributeMetadata)retAttributeResp.AttributeMetadata).OptionSet.Options.Where(oS => oS.Value != null));

      return listOfOption;
    }
  }
}
