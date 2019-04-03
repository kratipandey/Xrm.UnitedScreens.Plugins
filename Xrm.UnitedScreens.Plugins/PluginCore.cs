// <copyright file="PluginCore.cs" company="Acando AB">
// Copyright (c) 2016 All Rights Reserved.
// </copyright>
// <author>Acando AB</author>
// <date>1/16/2016</date>
// <summary>Implements the custom Plugins.</summary>
using System;
using System.Text;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace Xrm.US.Helper.XrmHelper
{
  #region Plugin Internal
  internal class Literal
  {
    public const string LeftBrace = "{";
    public const string RightBrace = "}";
    public const string LeftSquareBrace = "[";
    public const string RightSquareBrace = "]";
    public const string LeftParentheses = "(";
    public const string RightParentheses = ")";
    public const string Period = ".";
    public const string Comma = ",";
    public const string Colon = ":";
    public const string SemiColon = ";";
    public const string Pipe = "|";
    public const string Id = "id";
    public const string Backslash = "\\";
    public const string Frontslash = "/";
  }

  internal class PluginMessage
  {
    public const string Assign = "Assign";
    public const string Associate = "Associate";
    public const string Create = "Create";
    public const string Delete = "Delete";
    public const string Disassociate = "Disassociate";
    public const string Retrieve = "Retrieve";
    public const string RetrieveMultiple = "RetrieveMultiple";
    public const string Send = "Send";
    public const string SetState = "SetState";
    public const string SetStateDynamicEntity = "SetStateDynamicEntity";
    public const string Update = "Update";
  }

  internal class PluginParameter
  {
    public const string Query = "Query";
    public const string Target = "Target";
    public const string State = "State";
    public const string Status = "Status";
    public const string Relationship = "Relationship";
    public const string EntityMoniker = "EntityMoniker";
    public const string RelatedEntities = "RelatedEntities";
    public const string BusinessEntity = "BusinessEntity";
    public const string BusinessEntityCollection = "BusinessEntityCollection";
  }

  /// <summary>
  /// Alias of the image registered for the snapshot of the primary entity's attributes before the core platform operation executes.
  /// Alias of the image registered for the snapshot of the primary entity's attributes after the core platform operation executes.
  /// Note: Only synchronous post-event and asynchronous registered plug-ins have PostEntityImages populated.
  /// </summary>
  internal class PluginImage
  {
    public const string PreImage = "PreImage";
    public const string PostImage = "PostImage";
  }

  public enum PluginStage
  {
    PreValidation = 10,
    PreOperation = 20,
    PostOperation = 40
  };

  public enum PluginMode
  {
    Synchronous = 0,
    Asynchronous = 1
  }
  #endregion Plugin Internal

  public abstract class PluginCore
  {
    protected string LogicalName = string.Empty;

    #region Plugin Properties
    public IServiceProvider SvcProvider
    {
      get;
      private set;
    }
    public IPluginExecutionContext PluginCtx
    {
      get;
      private set;
    }
    public ParameterCollection InputParameters
    {
      get;
      private set;
    }
    public ParameterCollection OutputParameters
    {
      get;
      private set;
    }
    public ITracingService TraceSvc
    {
      get;
      private set;
    }
    public IOrganizationServiceFactory OrgSvcFactory
    {
      get;
      private set;
    }
    public IOrganizationService OrgSvc
    {
      get;
      private set;
    }

    public Entity ParentTarget
    {
      get;
      private set;
    }
    public IPluginExecutionContext ParentCtx
    {
      get;
      private set;
    }
    public ParameterCollection ParentInputParameters
    {
      get;
      private set;
    }

    public Entity Target
    {
      get;
      private set;
    }
    public Entity TargetDynamic
    {
      get;
      private set;
    }
    public EntityReference TargetMoniker
    {
      get;
      private set;
    }
    public EntityReference TargetReference
    {
      get;
      private set;
    }
    public EntityReferenceCollection RelatedEntities
    {
      get;
      private set;
    }
    public Relationship TargetRelationship
    {
      get;
      private set;
    }
    public QueryExpression TargetQueryExpression
    {
      get;
      protected set;
    }

    public Entity BusinessEntity
    {
      get;
      private set;
    }
    public EntityCollection BusinessEntityCollection
    {
      get;
      private set;
    }

    public Entity PreImage
    {
      get;
      private set;
    }
    public Entity PostImage
    {
      get;
      private set;
    }
    public string UnsecureConfig
    {
      get;
      private set;
    }
    public string SecureConfig
    {
      get;
      private set;
    }
    public int Stage
    {
      get;
      private set;
    }
    #endregion Plugin Properties

    /// <summary>
    /// Gets the List of events that the plug-in should fire for.
    /// Each List item is a Tuple containing the Message, Pipeline Stage, and the delegate to invoke on a matching registration.
    /// </summary>
    private Collection<Tuple<string, PluginStage, Action>> registeredEvents;
    public Collection<Tuple<string, PluginStage, Action>> RegisteredEvents
    {
      get
      {
        if (this.registeredEvents == null) this.registeredEvents = new Collection<Tuple<string, PluginStage, Action>>();
        return this.registeredEvents;
      }
    }

    public PluginCore(IServiceProvider svcProvider, string logicalName, PluginStage pluginStage, string unsecureConfig = null, string secureConfig = null)
    {
      LogicalName = logicalName;
      SecureConfig = secureConfig;
      UnsecureConfig = unsecureConfig;
      Stage = (int)pluginStage;

      SvcProvider = svcProvider;
      TraceSvc = (ITracingService)SvcProvider.GetService(typeof(ITracingService)); // Extract the tracing service for use in debugging sandboxed plug-ins.
      PluginCtx = (IPluginExecutionContext)SvcProvider.GetService(typeof(IPluginExecutionContext)); // Obtain the execution context from the service provider.
      OrgSvcFactory = (IOrganizationServiceFactory)SvcProvider.GetService(typeof(IOrganizationServiceFactory));
      OrgSvc = OrgSvcFactory.CreateOrganizationService(PluginCtx.UserId);

      Initialize();
      ParentInitialize();

      Trace("{0} is firing for Entity {1}/{2} on Stage: {3} for Message {4} with Depth {5} in Mode {6}", GetType().Name,
        PluginCtx.PrimaryEntityName, PluginCtx.PrimaryEntityId, PluginCtx.Stage, PluginCtx.MessageName, PluginCtx.Depth, PluginCtx.Mode);
    }

    private void Initialize() 
    {
      InputParameters = PluginCtx.InputParameters;
      OutputParameters = PluginCtx.OutputParameters;

      Target = null;
      if (InputParameters.Contains(PluginParameter.Target) && InputParameters[PluginParameter.Target] is Entity)
      {
        this.Target = (Entity)InputParameters[PluginParameter.Target];
        Trace("Target(Entity): {0}; Id: {1}", Target.LogicalName, Target.Id);
        if(PluginMessage.Update == PluginCtx.MessageName)
          Trace("Target has {0} attributes.{1}{2}", Target.Attributes.Count, Environment.NewLine, string.Join(Literal.SemiColon, Target.Attributes.Select(kV => kV.Key)));
      }

      TargetReference = null;
      if (InputParameters.Contains(PluginParameter.Target) && InputParameters[PluginParameter.Target] is EntityReference)
      {
        this.TargetReference = (EntityReference)InputParameters[PluginParameter.Target];
        Trace("Target(EntityReference): {0}; Id: {1}", TargetReference.LogicalName, TargetReference.Id);
      }

      TargetMoniker = null;
      if (InputParameters.Contains(PluginParameter.EntityMoniker) && InputParameters[PluginParameter.EntityMoniker] is EntityReference)
      {
        this.TargetMoniker = (EntityReference)InputParameters[PluginParameter.EntityMoniker];
        Trace("Target(EntityMoniker): {0}; Id: {1}", TargetMoniker.LogicalName, TargetMoniker.Id);
      }

      TargetDynamic = null;
      List<KeyValuePair<string, object>> listOfKeyValue = InputParameters.Where(kV => (kV.Value != null && kV.Value.GetType() == typeof(Entity))).ToList();
      if (listOfKeyValue.Count > 0)
      {
        TargetDynamic = (Entity)listOfKeyValue.First().Value;
        Trace("Target(Dymanic): {0}; Id: {1}", TargetDynamic.LogicalName, TargetDynamic.Id);
      }

      TargetRelationship = null;
      if (InputParameters.Contains(PluginParameter.Relationship) && InputParameters[PluginParameter.Relationship] is Relationship)
      {
        TargetRelationship = (Relationship)InputParameters[PluginParameter.Relationship];
        Trace("Target(Relationship): {0}; PrimaryEntityRole: {1}", TargetRelationship.SchemaName, (TargetRelationship.PrimaryEntityRole != null) ? TargetRelationship.PrimaryEntityRole.Value.ToString() : string.Empty);
      }

      RelatedEntities = new EntityReferenceCollection();
      if (InputParameters.Contains(PluginParameter.RelatedEntities) && InputParameters[PluginParameter.RelatedEntities] is EntityReferenceCollection)
      {
        RelatedEntities = (EntityReferenceCollection)InputParameters[PluginParameter.RelatedEntities];
        Trace("RelatedEntities: {0}", RelatedEntities.Count);
      }

      TargetQueryExpression = null;
      if (InputParameters.Contains(PluginParameter.Query) && InputParameters[PluginParameter.Query] is QueryExpression)
      {
        TargetQueryExpression = (QueryExpression)InputParameters[PluginParameter.Query];
        Trace("Target(QueryExpression): {0}", TargetQueryExpression.EntityName);
      }

      #region Pre/Post images
      PreImage = new Entity(Target != null ? Target.LogicalName : string.Empty);
      Trace("PreImage(s): {0}", PluginCtx.PreEntityImages.Count);
      if (PluginCtx.PreEntityImages.ContainsKey(PluginImage.PreImage))
      {
        PreImage = PluginCtx.PreEntityImages[PluginImage.PreImage];
        Trace("PreImage has {0} attributes.{1}{2}", PreImage.Attributes.Count, Environment.NewLine, string.Join(Literal.SemiColon, PreImage.Attributes.Select(kV => kV.Key)));
      }

      PostImage = new Entity(Target != null ? Target.LogicalName : string.Empty);
      Trace("PostImage(s): {0}", PluginCtx.PostEntityImages.Count);
      if (PluginCtx.PostEntityImages.ContainsKey(PluginImage.PostImage))
      {
        PostImage = PluginCtx.PostEntityImages[PluginImage.PostImage];
        Trace("PostImage has {0} attributes.{1}{2}", PostImage.Attributes.Count, Environment.NewLine, string.Join(Literal.SemiColon, PostImage.Attributes.Select(kV => kV.Key)));
      }
      #endregion Pre/Post images

      #region BusinessEntity
      BusinessEntity = null;
      if (OutputParameters.Contains(PluginParameter.BusinessEntity) && OutputParameters[PluginParameter.BusinessEntity] is Entity)
      {
        BusinessEntity = (Entity)OutputParameters[PluginParameter.BusinessEntity];
        Trace("BusinessEntity: {0}; Id: {1}", BusinessEntity.LogicalName, BusinessEntity.Id);
        Trace("BusinessEntity has {0} attributes.{1}{2}", BusinessEntity.Attributes.Count, Environment.NewLine, string.Join(Literal.SemiColon, BusinessEntity.Attributes.Select(kV => kV.Key)));
      }
      BusinessEntityCollection = null;
      if (OutputParameters.Contains(PluginParameter.BusinessEntityCollection) && OutputParameters[PluginParameter.BusinessEntityCollection] is EntityCollection)
      {
        BusinessEntityCollection = (EntityCollection)OutputParameters[PluginParameter.BusinessEntityCollection];
        Trace("BusinessEntityCollection: {0}; Total/Count Record(s) : {1}/{2}", BusinessEntityCollection.EntityName, BusinessEntityCollection.TotalRecordCount, BusinessEntityCollection.Entities.Count);
        if (BusinessEntityCollection.Entities.Count > 0)
        {
          Entity businessEntity = BusinessEntityCollection.Entities[0];
          //Trace("BusinessEntity has {0} attributes.{1}{2}", businessEntity.Attributes.Count, Environment.NewLine, string.Join(Literal.SemiColon, businessEntity.Attributes.Select(kV => string.Format("{0}|{1}", kV.Key, (kV.Value != null? kV.Value.GetType() : null)))));
        }
      }
      #endregion BusinessEntity
    }

    private void ParentInitialize()
    {
      if (PluginCtx.ParentContext == null) return;
      if (PluginCtx.ParentContext.ParentContext == null) return;
      ParentCtx = this.PluginCtx.ParentContext.ParentContext;
      
      if (ParentCtx.InputParameters == null) return;
      ParentInputParameters = ParentCtx.InputParameters;

      if (ParentInputParameters.Contains(PluginParameter.Target) && ParentInputParameters[PluginParameter.Target] is Entity)
      {
        ParentTarget = (Entity)ParentInputParameters[PluginParameter.Target];
        Trace("Parent Target: Entity: {0}; Id: {1}", ParentTarget.LogicalName, ParentTarget.Id);
      }
    }

    public bool IsValidContext()
    {
      bool isValidCtx = false;
      if (PluginCtx.Stage != Stage) return isValidCtx;

    if (string.IsNullOrEmpty(LogicalName))
        isValidCtx = true;
    else
    {
        if (Target != null && Target.LogicalName.Equals(LogicalName))
            isValidCtx = true;
        else if (TargetReference != null && TargetReference.LogicalName.Equals(LogicalName))
            isValidCtx = true;
        else if (TargetMoniker != null && TargetMoniker.LogicalName.Equals(LogicalName))
            isValidCtx = true;
        else if (PluginCtx.PrimaryEntityName == LogicalName)
            isValidCtx = true;
    }

      Trace("IsValidContext? {0}", isValidCtx);
      return isValidCtx;
    }

    public void Trace(string format, params object[] args)
    {
      if (TraceSvc == null) return;
      TraceSvc.Trace(format, args);
    }

    public void TraceException(Exception ex)
    {
      Trace(ex.Message);
      if(ex.InnerException != null) Trace(ex.InnerException.Message);
      Trace(ex.StackTrace);
    }

    public void Execute()
    {
      // Iterate over all of the expected registered events to ensure that the plugin has been invoked by an expected event
      // For any given plug-in event at an instance in time, we would expect at most 1 result to match.
      Action actionSet =
          (from eP in this.RegisteredEvents
           where (eP.Item1 == PluginCtx.MessageName &&
           (int)eP.Item2 == PluginCtx.Stage &&
           LogicalName == PluginCtx.PrimaryEntityName
           ) select eP.Item3).FirstOrDefault();

      if (actionSet != null)
      {
        Trace("Firing: {0}");
        actionSet.Invoke();
        // Exit - if the derived plug-in has incorrectly registered overlapping event registrations, guard against multiple executions.
        return;
      }
    }
  }

  #region PluginSettings
  internal sealed class PluginSetting
  {
    #region Literals
    public const string LeftBrace = "{";
    public const string RightBrace = "}";
    public const string LeftParentheses = "(";
    public const string RightParentheses = ")";
    #endregion Literals

    public XmlDocument PluginSettingXml
    {
      get;
      private set;
    }

    public PluginSetting(string pluginSettingXml)
    {
      PluginSettingXml = new XmlDocument();
      try
      {
        PluginSettingXml.LoadXml(pluginSettingXml);
      }
      catch
      {
      }
    }

    public KeyValuePair<string, string> GetSetting(string settingKey)
    {
      XmlNode xmlNode = null;
      try
      {
        xmlNode = PluginSettingXml.SelectSingleNode(String.Format("pluginSettings/setting[@key='{0}']", settingKey));
        if (xmlNode != null)
        {
          return new KeyValuePair<string, string>(xmlNode.Attributes["name"].Value, xmlNode.Attributes["value"].Value);
        }
      }
      catch
      {
      }

      return new KeyValuePair<string, string>(string.Empty, string.Empty);
    }

    public string GetCDataNodeValue(string nodeName)
    {
      XmlNode xmlNode = null;
      try
      {
        if (!string.IsNullOrWhiteSpace(nodeName))
        {
          xmlNode = PluginSettingXml.SelectSingleNode(string.Format("pluginSettings/{0}", nodeName));
        }
      }
      catch
      {
      }
      return (xmlNode != null && xmlNode.ChildNodes.Count == 3) ? xmlNode.ChildNodes[1].OuterXml : string.Empty;
    }

    public List<Tuple<string, string, string>> GetAllSettings(string settingKey = null)
    {
      List<Tuple<string, string, string>> settingTupleSet = new List<Tuple<string, string, string>>();
      XmlNodeList xmlNodeList = null;
      try
      {
        if (!string.IsNullOrWhiteSpace(settingKey))
        {
          xmlNodeList = PluginSettingXml.SelectNodes(String.Format("pluginSettings/setting[@key='{0}']", settingKey));
        }
        else
        {
          xmlNodeList = PluginSettingXml.SelectNodes(String.Format("pluginSettings/setting"));
        }

        if (xmlNodeList != null)
        {
          for (int iIndex = 0; iIndex < xmlNodeList.Count; iIndex++)
          {
            settingTupleSet.Add(new Tuple<string, string, string>(
              xmlNodeList[iIndex].Attributes["key"].Value,
              xmlNodeList[iIndex].Attributes["name"].Value,
              xmlNodeList[iIndex].Attributes["value"].Value));
          }
        }
      }
      catch
      {
      }

      return settingTupleSet;
    }

    public List<string> GetParseResult(string leftDelimiter, string rightDelimiter, string stringToParse)
    {
      List<string> parseResult = new List<string>();
      if(string.IsNullOrWhiteSpace(stringToParse)) return parseResult;

      List<Tuple<int, int>> delimiterIndexSet = new List<Tuple<int, int>>();
      
      int offsetLeftDelimiter = -1;
      int offsetRightDelimiter = -1;

      offsetLeftDelimiter = stringToParse.IndexOf(leftDelimiter, (offsetLeftDelimiter += 1));
      offsetRightDelimiter = stringToParse.IndexOf(rightDelimiter, (offsetRightDelimiter += 1));
      
      while (offsetLeftDelimiter > -1 && offsetRightDelimiter > -1)
      {
        delimiterIndexSet.Add(new Tuple<int, int>(offsetLeftDelimiter, offsetRightDelimiter));
        
        offsetLeftDelimiter = stringToParse.IndexOf(leftDelimiter, (offsetLeftDelimiter += 1));
        offsetRightDelimiter = stringToParse.IndexOf(rightDelimiter, (offsetRightDelimiter += 1));
      }

      for (int iIndex = 0, startIndex = 0, length = 0; iIndex < delimiterIndexSet.Count; iIndex++)
      {
        if ((delimiterIndexSet[iIndex].Item1 != -1 && delimiterIndexSet[iIndex].Item2 != -1)
          && (delimiterIndexSet[iIndex].Item1 < delimiterIndexSet[iIndex].Item2))
        {
          startIndex = delimiterIndexSet[iIndex].Item1 + 1;
          length = delimiterIndexSet[iIndex].Item2 - startIndex;
          parseResult.Add(stringToParse.Substring(startIndex, length).Trim());
        }
      }

      return parseResult;
    }

    public Tuple<string, string, string> GetParseResultTuple(string leftDelimiter, string rightDelimiter, string stringToParse)
    {
      Tuple<string, string, string> parseResult = new Tuple<string, string, string>(string.Empty, string.Empty, string.Empty);
      if (string.IsNullOrWhiteSpace(stringToParse)) return parseResult;

      List<string> splitSet = stringToParse.Replace(
        rightDelimiter, string.Empty).Split(new string[] { leftDelimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();

      parseResult = new Tuple<string, string, string>(splitSet[0].Trim(),
        splitSet.Count > 1 ? splitSet[1].Trim() : string.Empty, splitSet.Count > 2 ? splitSet[2].Trim() : string.Empty);

      return parseResult;
    }
  }
  #endregion PluginSettings

  #region AutoNumber Service
  public class AutoNumber
  {
    public static readonly string KeyAutoNumber = "AutoNumber";

    public static long GetNext(IOrganizationService orgSvc)
    {
      long nextAutoNumber = 0;

      Entity campaign = new Entity("campaign");
      campaign["name"] = campaign.LogicalName;
      campaign.Id = orgSvc.Create(campaign);

      campaign = orgSvc.Retrieve(campaign.LogicalName, campaign.Id, new ColumnSet("campaignid", "codename"));
      orgSvc.Delete(campaign.LogicalName, campaign.Id);

      string codeName = campaign.GetAttributeValue<string>("codename");

      try
      {
        codeName = codeName.Split('-')[1];
        long.TryParse(codeName, out nextAutoNumber);
      }
      catch
      {
        nextAutoNumber = DateTime.Now.Ticks; // Try should not fail and if in case hasfailed, use alternative.
      }

      return (nextAutoNumber);
    }
  }
  #endregion AutoNumber Service

  #region QueryExtension
  internal sealed class QueryRevisor
  {
    public IOrganizationService OrgSvc
    {
      get;
      private set;
    }

    public enum ConfigCommand
    {
      Unknown = -1,
      Append,
      Adjust,
      Replace,
      Interchange,
      Populate,
      Maneuver
    }

    public enum ConfigAction
    {
      Unknown = -1,
      Condition,
      Criteria,
      Filter,
      LinkEntity,
      ConditionValue,
      ConditionAttributeValue,
      LinkEntityColumn,
      ConditionLinkEntity,
      ConditionLinkEntityNotExists
    }

    public enum ConfigValue
    {
      Unknown = -1,
      FetchXml = 1
    }

    public FetchExpression FetchExpr
    {
      get;
      private set;
    }

    public QueryExpression QueryExpr
    {
      get;
      private set;
    }

    StringBuilder _tractText = new StringBuilder();
    public string TraceText
    {
      get
      {
        return _tractText.ToString();
      }
    }

    ConfigAction _configAction = ConfigAction.Unknown;
    ConfigCommand _configCommand = ConfigCommand.Unknown;

    string[] NameSet
    {
      get;
      set;
    }
    string[] ValueSet
    {
      get;
      set;
    }

    public QueryRevisor(IOrganizationService orgSvc, string name, string value, string fetchXml)
    {
      if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(fetchXml)) return;

      NameSet = name.Split(new string[] { Literal.LeftBrace, Literal.RightBrace }, StringSplitOptions.RemoveEmptyEntries);
      ValueSet = value.Split(new string[] { Literal.LeftBrace, Literal.RightBrace }, StringSplitOptions.RemoveEmptyEntries);

      if (!(NameSet.Length >= 2 && ValueSet.Length > 0)) return;
      if (!ConfigValue.FetchXml.ToString().Equals(ValueSet[0])) return;

      OrgSvc = orgSvc;
      _tractText.AppendLine(string.Format("Config Name/Value: {0}/{1}", name, value));
      #region QueryExpression
      try
      {
        FetchExpr = new FetchExpression(fetchXml);
        FetchXmlToQueryExpressionRequest orgReq = new FetchXmlToQueryExpressionRequest
        {
          FetchXml = FetchExpr.Query
        };
        QueryExpr = ((FetchXmlToQueryExpressionResponse)OrgSvc.Execute(orgReq)).Query;

        Enum.TryParse(NameSet[0], out _configCommand);
        Enum.TryParse(NameSet[1], out _configAction);
      }
      catch
      {
      }
      #endregion QueryExpression
      _tractText.AppendLine(string.Format("Config Command/Action: {0}/{1}", _configCommand.ToString(), _configAction.ToString()));
    }

    public void ReviseQuery(QueryExpression queryExpr)
    {
      switch(_configCommand)
      {
        case ConfigCommand.Adjust:
          switch(_configAction)
          {
            case ConfigAction.ConditionLinkEntityNotExists:
              AdjustConditionLinkEntityNotExists(queryExpr);
              break;
          };
          break;

        case ConfigCommand.Append:
          switch (_configAction)
          {
            case ConfigAction.Criteria:
              AppendCondition(queryExpr);
              break;

            case ConfigAction.LinkEntity:
              AppendLinkEntity(queryExpr);
              break;
          };
          break;

        case ConfigCommand.Replace:
          switch (_configAction)
          {
            case ConfigAction.Condition:
              ReplaceCondition(queryExpr);
              break;

            case ConfigAction.ConditionValue:
              ReplaceConditionValue(queryExpr);
              break;

            case ConfigAction.ConditionAttributeValue:
              ReplaceConditionAttributeValue(queryExpr);
              break;
          };
          break;

        case ConfigCommand.Maneuver:
          switch(_configAction)
          {
            case ConfigAction.ConditionLinkEntity:
              ManeuverConditionLinkEntity(queryExpr);
              break;
          };
          break;
      };
    }

    public void ReviseCollection(QueryExpression queryExpr, EntityCollection entitySet)
    {
      switch (_configCommand)
      {
        case ConfigCommand.Populate:
          switch (_configAction)
          {
            case ConfigAction.LinkEntityColumn:
              PopulateLinkEntityColumn(queryExpr, entitySet);
              break;
          };
          break;
      };
    }

    #region Command Action
    FilterExpression ExpectedFilter(string attributeName, DataCollection<FilterExpression> filterSet)
    {
      if (filterSet == null) return null;

      FilterExpression filterExpr = null;
      bool iNullCond = false, iNotNullCond = false;
      for (int iIndex = 0; iIndex < filterSet.Count; iIndex++)
      {
        filterExpr = filterSet[iIndex];
        for (int iCondition = 0; iCondition < filterExpr.Conditions.Count; iCondition++)
        {
          if (filterExpr.Conditions[iCondition].AttributeName != attributeName) continue;
          switch (filterExpr.Conditions[iCondition].Operator)
          {
            case ConditionOperator.Null:
              iNullCond = true;
              break;
            case ConditionOperator.NotNull:
              iNotNullCond = true;
              break;
          };
          if (iNullCond && iNotNullCond) return filterExpr;
        }

        #region Nested filters
        for (int iFilter = 0; iFilter < filterSet[iIndex].Filters.Count; iFilter++)
        {
          filterExpr = filterSet[iIndex].Filters[iFilter];
          for (int iCondition = 0; iCondition < filterExpr.Conditions.Count; iCondition++)
          {
            if (filterExpr.Conditions[iCondition].AttributeName != attributeName) continue;
            switch (filterExpr.Conditions[iCondition].Operator)
            {
              case ConditionOperator.Null:
                iNullCond = true;
                break;
              case ConditionOperator.NotNull:
                iNotNullCond = true;
                break;
            };
            if (iNullCond && iNotNullCond) return filterExpr;
          }
          if (iNullCond && iNotNullCond) return filterExpr;
        }
        #endregion Nested filters

        if (iNullCond && iNotNullCond) return filterExpr;
      }
      _tractText.AppendLine(string.Format("ExpectedFilter? Null/NotNull: {0}/{1}", iNullCond, iNotNullCond));
      return null;
    }

    void ReplaceCondition(QueryExpression queryExpr)
    {
    }

    void ReplaceConditionValue(QueryExpression queryExpr)
    {
      if (ValueSet.Length < 2) return;
      string[] valueSet = ValueSet[1].Split(new string[] { Literal.LeftParentheses, Literal.RightParentheses }, StringSplitOptions.RemoveEmptyEntries);
      if (valueSet.Length < 1) return;

      _tractText.AppendLine("Expected Filter...");
      if (ExpectedFilter(valueSet[0], queryExpr.Criteria.Filters) == null) return;

      int iFormCond = -1;
      ConditionExpression condExpr = null, condExprForm = null;

      #region FormFilterCondition
      for (int iIndex = 0; iIndex < queryExpr.Criteria.Conditions.Count; iIndex++)
      {
        condExpr = queryExpr.Criteria.Conditions[iIndex];
        if(condExpr.AttributeName == valueSet[0] && condExpr.Operator == ConditionOperator.Equal
          && condExpr.Values.Count == 1 && condExpr.Values[0] is Guid)
        {
          iFormCond = iIndex;
          condExprForm = new ConditionExpression(condExpr.AttributeName, condExpr.Operator, condExpr.Values);
        }
      }
      #endregion FormFilterCondition

      _tractText.AppendLine(string.Format("Form: {0}", iFormCond));
      Guid entityId = Guid.Empty; // What if when missing condition!

      #region Mutate EntityId
      if (condExprForm != null && valueSet.Length >= 3)
      {
        Entity entity = OrgSvc.Retrieve(valueSet[1], (Guid)condExprForm.Values[0], new ColumnSet(valueSet[1] + Literal.Id, valueSet[2]));
        if(entity.Contains(valueSet[2]) && entity[valueSet[2]] is EntityReference)
        {
          entityId = entity.GetAttributeValue<EntityReference>(valueSet[2]).Id;
          _tractText.AppendFormat("Id From/To: {0}/{1}", entity.Id, entityId);
        }
      }
      else
      {
        condExprForm = new ConditionExpression(valueSet[0], ConditionOperator.Equal, entityId);
      }
      #endregion Mutate EntityId

      #region Mutate at once!
      _tractText.AppendLine("Mutating...");
      if(iFormCond > -1)
      {
        queryExpr.Criteria.Conditions[iFormCond].Values[0] = entityId;
      }
      else
      {
        queryExpr.Criteria.Conditions.Add(condExprForm);
      }
      #endregion Mutate at once!
    }

    void ReplaceConditionAttributeValue(QueryExpression queryExpr)
    {
      if (ValueSet.Length < 2) return;
      string[] valueSet = ValueSet[1].Split(new string[] { Literal.LeftParentheses, Literal.RightParentheses }, StringSplitOptions.RemoveEmptyEntries);
      if (valueSet.Length < 4) return;

      _tractText.AppendLine("Expected Filter...");
      if (ExpectedFilter(valueSet[0], queryExpr.Criteria.Filters) == null) return;

      int iFormCond = -1;
      ConditionExpression condExpr = null, condExprForm = null;

      #region FormFilterCondition
      for (int iIndex = 0; iIndex < queryExpr.Criteria.Conditions.Count; iIndex++)
      {
        condExpr = queryExpr.Criteria.Conditions[iIndex];
        if (condExpr.AttributeName == valueSet[0] && condExpr.Operator == ConditionOperator.Equal
          && condExpr.Values.Count == 1 && condExpr.Values[0] is Guid)
        {
          iFormCond = iIndex;
          condExprForm = new ConditionExpression(condExpr.AttributeName, condExpr.Operator, condExpr.Values);
        }
      }
      #endregion FormFilterCondition

      _tractText.AppendLine(string.Format("Form: {0}", iFormCond));
      Guid entityId = Guid.Empty; // What if when missing condition!

      #region Mutate EntityId
      if (condExprForm != null && valueSet.Length >= 3)
      {
        Entity entity = OrgSvc.Retrieve(valueSet[1], (Guid)condExprForm.Values[0], new ColumnSet(valueSet[1] + Literal.Id, valueSet[2]));
        if (entity.Contains(valueSet[2]) && entity[valueSet[2]] is EntityReference)
        {
          entityId = entity.GetAttributeValue<EntityReference>(valueSet[2]).Id;
          _tractText.AppendFormat("Id From/To: {0}/{1}", entity.Id, entityId);
        }
      }
      else
      {
        condExprForm = new ConditionExpression(valueSet[2], ConditionOperator.Equal, entityId);
      }
      #endregion Mutate EntityId

      #region Mutate at once!
      _tractText.AppendLine("Mutating...");
      if (iFormCond > -1)
      {
        queryExpr.Criteria.Conditions[iFormCond].AttributeName = valueSet[3];
        queryExpr.Criteria.Conditions[iFormCond].Values[0] = entityId;
      }
      else
      {
        queryExpr.Criteria.Conditions.Add(condExprForm);
      }
      #endregion Mutate at once!
    }

    void AdjustConditionLinkEntityNotExists(QueryExpression queryExpr)
    {
      if (ValueSet.Length != 2) return;
      string[] valueSet = ValueSet[1].Split(new string[] { Literal.LeftParentheses, Literal.RightParentheses }, StringSplitOptions.RemoveEmptyEntries);
      if (valueSet.Length != 2) return;

      _tractText.AppendLine("Expected Filter...");
      FilterExpression filterExpr = ExpectedFilter(valueSet[1], queryExpr.Criteria.Filters);
      if (filterExpr == null) return;

      #region LinkEntity System
      int iLinkEntitySys = -1;
      LinkEntity linkEntitySys = null;
      for (int iIndex = 0; iIndex < queryExpr.LinkEntities.Count; iIndex++)
      {
        linkEntitySys = queryExpr.LinkEntities[iIndex];
        // OBS! Standard CRM 1 : N relationship.
        if (linkEntitySys.LinkFromEntityName == queryExpr.EntityName && linkEntitySys.LinkFromAttributeName == queryExpr.EntityName + Literal.Id
          && linkEntitySys.LinkToEntityName == valueSet[0] && linkEntitySys.LinkToAttributeName == valueSet[1])
        {
          iLinkEntitySys = iIndex;
          break;
        }
      }
      _tractText.AppendLine(string.Format("System Link Entity: {0}", iLinkEntitySys));
      #endregion LinkEntity System
      if (iLinkEntitySys <= -1) return;

      filterExpr = ExpectedFilter(valueSet[1], queryExpr.LinkEntities[iLinkEntitySys].LinkCriteria.Filters);
      _tractText.AppendLine(string.Format("Entity/LinkEntity Filter: {0}/{1}", true, (filterExpr != null)));
      if (filterExpr == null) return;

      #region Mutate at once!
      _tractText.AppendLine("Mutating...");
      queryExpr.LinkEntities[iLinkEntitySys].JoinOperator = JoinOperator.LeftOuter;
      queryExpr.Criteria.AddCondition(valueSet[0], valueSet[1], ConditionOperator.Null);
      filterExpr.Conditions.Clear();
      #endregion Mutate at once!
    }

    void AppendCondition(QueryExpression queryExpr)
    {
      queryExpr.Criteria.Conditions.AddRange(QueryExpr.Criteria.Conditions);
      queryExpr.Criteria.Filters.AddRange(QueryExpr.Criteria.Filters);
    }

    void AppendLinkEntity(QueryExpression queryExpr)
    {
      if (ValueSet.Length != 3) return;
      string[] valueSet = ValueSet[2].Split(new string[] { Literal.LeftParentheses, Literal.RightParentheses }, StringSplitOptions.RemoveEmptyEntries);
      if (valueSet.Length != 2) return;

      _tractText.AppendLine("Expected Column...");
      int iLinkColumn = queryExpr.ColumnSet.Columns.IndexOf(valueSet[1]);
      if (iLinkColumn < 0) return; // Expected!

      _tractText.AppendLine("Expected Filter...");
      if (ExpectedFilter(valueSet[1], queryExpr.Criteria.Filters) == null) return;

      #region LinkEntity System
      int iLinkEntitySys = -1;
      LinkEntity linkEntitySys = null;
      for (int iIndex = 0; iIndex < queryExpr.LinkEntities.Count; iIndex++)
      {
        linkEntitySys = queryExpr.LinkEntities[iIndex];
        // OBS! Standard CRM N : 1 relationship. Possibility of standard view.
        if (linkEntitySys.LinkFromEntityName == queryExpr.EntityName && linkEntitySys.LinkFromAttributeName == valueSet[1]
          && linkEntitySys.LinkToEntityName == valueSet[0] && linkEntitySys.LinkToAttributeName == linkEntitySys.LinkToEntityName + Literal.Id)
        {
          iLinkEntitySys = iIndex;
          break;
        }
      }
      _tractText.AppendLine(string.Format("System Link Entity: {0}", iLinkEntitySys));
      #endregion LinkEntity System

      #region LinkEntity Custom
      int iLinkEntityCust = -1;
      LinkEntity linkEntityCust = null;
      for (int iIndex = 0; iIndex < QueryExpr.LinkEntities.Count; iIndex++)
      {
        linkEntityCust = QueryExpr.LinkEntities[iIndex];
        if (linkEntityCust.EntityAlias == ValueSet[1])
        {
          iLinkEntityCust = iIndex;
          break;
        }
      }
      _tractText.AppendLine(string.Format("Custom Link Entity: {0}", iLinkEntityCust));
      #endregion LinkEntity Custom

      if (iLinkEntityCust <= -1) return; // Custom

      #region Mutate at once!
      _tractText.AppendLine("Mutating...");
      if (iLinkEntitySys > -1)
      {
        foreach(string column in queryExpr.LinkEntities[iLinkEntitySys].Columns.Columns)
        {
          if (linkEntityCust.Columns.Columns.IndexOf(column) < 0) linkEntityCust.Columns.AddColumn(column);
        }
      }
      queryExpr.LinkEntities.Add(linkEntityCust);
      #endregion Mutate at once!
    }

    void PopulateLinkEntityColumn(QueryExpression queryExpr, EntityCollection entitySet)
    {
      if (ValueSet.Length != 3) return;
      string[] valueSetCust = ValueSet[1].Split(new string[] { Literal.LeftParentheses, Literal.RightParentheses }, StringSplitOptions.RemoveEmptyEntries);
      if (valueSetCust.Length != 3) return;
      string[] valueSetSys = ValueSet[2].Split(new string[] { Literal.LeftParentheses, Literal.RightParentheses }, StringSplitOptions.RemoveEmptyEntries);
      if (valueSetSys.Length != 2) return;

      _tractText.AppendLine("Expected Column...");
      int iLinkColumn = queryExpr.ColumnSet.Columns.IndexOf(valueSetSys[1]);
      if (iLinkColumn < 0) return; // Expected!

      _tractText.AppendLine("Expected Filter...");
      if (ExpectedFilter(valueSetSys[1], queryExpr.Criteria.Filters) == null) return;

      #region LinkEntity System
      int iLinkEntitySys = -1;
      LinkEntity linkEntitySys = null;
      string entityAlias = string.Empty;
      ColumnSet columnSet = new ColumnSet();
      for (int iIndex = 0; iIndex < queryExpr.LinkEntities.Count; iIndex++)
      {
        linkEntitySys = queryExpr.LinkEntities[iIndex];
        // OBS! Standard CRM N : 1 relationship. Possibility of standard view.
        if (linkEntitySys.LinkFromEntityName == queryExpr.EntityName && linkEntitySys.LinkFromAttributeName == valueSetSys[1]
          && linkEntitySys.LinkToEntityName == valueSetSys[0] && linkEntitySys.LinkToAttributeName == linkEntitySys.LinkToEntityName + Literal.Id)
        {
          iLinkEntitySys = iIndex;
          columnSet = linkEntitySys.Columns;
          entityAlias = linkEntitySys.EntityAlias;
          break;
        }
      }
      _tractText.AppendLine(string.Format("System Link Entity: {0}", iLinkEntitySys));
      #endregion LinkEntity System

      string keyId = valueSetCust[0] + Literal.Period + valueSetCust[1];
      string nameKey = valueSetCust[0] + Literal.Period + valueSetCust[2];
      _tractText.AppendLine(string.Format("Key Id/Name: {0}/{1}", keyId, nameKey));

      #region Mutate at once!
      _tractText.AppendLine("Mutating...");
      string colAliasCust = string.Empty;
      AliasedValue aliasValue = null;
      EntityReference entityIdRef = null;
      foreach (Entity entity in entitySet.Entities)
      {
        entityIdRef = new EntityReference();
        if (entity.Contains(nameKey))
        {
          aliasValue = entity.GetAttributeValue<AliasedValue>(nameKey);
          entityIdRef.Name = (string)aliasValue.Value;
        }

        if (entity.Contains(keyId))
        {
          aliasValue = entity.GetAttributeValue<AliasedValue>(keyId);
          entityIdRef.Id = (Guid)aliasValue.Value;
          entityIdRef.LogicalName = aliasValue.EntityLogicalName;
          entity[valueSetSys[1]] = entityIdRef;
        }

        foreach(string attributeName in columnSet.Columns)
        {
          colAliasCust = valueSetCust[0] + Literal.Period + attributeName;
          if(entity.Contains(colAliasCust))
            entity[entityAlias + Literal.Period + attributeName] = entity[colAliasCust];
        }
      }
      #endregion Mutate at once!
    }

    void ManeuverConditionLinkEntity(QueryExpression queryExpr)
    {
      if (ValueSet.Length < 3) return;
      string[] valueSet = ValueSet[2].Split(new string[] { Literal.LeftParentheses, Literal.RightParentheses }, StringSplitOptions.RemoveEmptyEntries);
      if (valueSet.Length < 2 || valueSet.Length % 2 != 0) return;

      int iFormCond = -1;
      ConditionExpression condExpr = null, condExprForm = null;

      #region FormFilterCondition
      for (int iIndex = 0; iIndex < queryExpr.Criteria.Conditions.Count; iIndex++)
      {
        condExpr = queryExpr.Criteria.Conditions[iIndex];
        if (condExpr.AttributeName == ValueSet[1] && condExpr.Operator == ConditionOperator.Equal
          && condExpr.Values.Count == 1 && condExpr.Values[0] is Guid)
        {
          iFormCond = iIndex;
          condExprForm = new ConditionExpression(condExpr.AttributeName, condExpr.Operator, condExpr.Values);
        }
      }
      #endregion FormFilterCondition

      _tractText.AppendLine(string.Format("Form: {0}", iFormCond));
      if (iFormCond <= -1) return; // Expected!

      #region LinkEntityFilterCondition
      int iIndexLinkEntity = -1;
      string linkFrom = queryExpr.EntityName;
      string linkToAttribute = valueSet[0] + Literal.Id;

      // Recursive linked entity
      LinkEntity linkEntity = null;
      DataCollection<LinkEntity> linkEntitySet = queryExpr.LinkEntities;
      for(int iLinkValue = 0; iLinkValue <= (valueSet.Length / 2); iLinkValue += 2)
      {
        for (int iIndex = 0; iIndex < linkEntitySet.Count; iIndex++)
        {
          linkEntity = linkEntitySet[iIndex];
          if (linkEntity.LinkFromEntityName == linkFrom && linkEntity.LinkFromAttributeName == valueSet[iLinkValue + 1]
            && linkEntity.LinkToEntityName == valueSet[iLinkValue] && linkEntity.LinkToAttributeName == linkToAttribute)
          {
            iIndexLinkEntity = iIndex;
            linkFrom = linkEntity.LinkToEntityName;
            linkToAttribute = linkEntity.LinkToEntityName + Literal.Id;
            linkEntitySet = linkEntitySet[iIndex].LinkEntities;
            break;
          }
        }
      }
      #endregion LinkEntityFilterCondition

      _tractText.AppendLine(string.Format("LinkEntity: {0}", iIndexLinkEntity));
      if (iIndexLinkEntity <= -1) return;
      FilterExpression filterExpr = ExpectedFilter(ValueSet[1], linkEntity.LinkCriteria.Filters);
      if (filterExpr == null) return;

      #region Mutate at once!
      _tractText.AppendLine("Mutating...");
      queryExpr.Criteria.AddFilter(filterExpr);
      linkEntity.LinkCriteria.Conditions.Add(condExprForm);

      queryExpr.Criteria.Conditions.RemoveAt(iFormCond);
      linkEntity.LinkCriteria.Filters.Remove(filterExpr);
      #endregion Mutate at once!
    }
    #endregion Command Action
  }
  #endregion QueryExtension
}
