using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xrm.US.Helper.XrmHelper;
//using Xrm.US.Entities;
using CRMRepository;


namespace Xrm.US.Plugins
{
    public class AutoNumberGenerator : IPlugin
    {
        readonly string _secureString = string.Empty;
        readonly string _unsecureString = string.Empty;

        public AutoNumberGenerator(string unsecureString, string secureString)
        {
            _secureString = secureString;
            _unsecureString = unsecureString;
        }

        public void Execute(IServiceProvider svcProvider)
        {
            var plugin = new Processor(svcProvider, string.Empty, PluginStage.PreOperation, _unsecureString, _secureString );
            if (!plugin.IsValidContext()) return;
            
            plugin.Run();
        }

        public class Processor : PluginCoreExtended<Entity>
        {
            public Processor(IServiceProvider svcProvider, string logicalName, PluginStage pluginStage, string unsecureConfig = null, string secureConfig = null)
              : base(svcProvider, logicalName, pluginStage, unsecureConfig, secureConfig)
            {

            }

            public override void ExecutePlugin()
            {
                Trace("Plugin Start");
                var triggerEvent = PluginCtx.MessageName;

                Trace("Has us_opportunitytype " + Target.Contains("us_opportunitytype").ToString());

                var opportunutytype = Target.GetAttributeValue<OptionSetValue>("us_opportunitytype");
                if (opportunutytype == null)
                    return;

                Trace("opportunutytype "+ opportunutytype.Value.ToString());
                us_autonumber autoNumber = GetTargetAutoNumberRecord(opportunutytype.Value);

                LockAutoNumberRecord(autoNumber);

                var targetAttribute = autoNumber.us_field_name;

                Trace("targetAttribute " + targetAttribute.ToString());

                if (PluginCtx.MessageName == PluginMessage.Update && !Target.Contains(targetAttribute))
                {
                    return;  // Continue, if this is an Update event and the target does not contain the trigger value
                }
                else if (Target.Contains(targetAttribute) && !string.IsNullOrWhiteSpace(Target.GetAttributeValue<string>(targetAttribute)))
                {
                    return;  // Continue so we don't overwrite a manual value
                }
                else if (triggerEvent == PluginMessage.Update && PreImage.Contains(targetAttribute) && !string.IsNullOrWhiteSpace(PreImage.GetAttributeValue<string>(targetAttribute)))
                {
                    return;  // Continue, so we don't overwrite an existing value
                }

                var numDigits = autoNumber.us_digits;
                Trace("numDigit " + numDigits);
                var prefix = string.Empty;
                var postfix = string.Empty;

                if (autoNumber.Contains(EntityMetadata<us_autonumber>.AttributeName(a => a.us_prefix)))
                    prefix = ReplacePredefined(autoNumber.us_prefix);
                if (autoNumber.Contains(EntityMetadata<us_autonumber>.AttributeName(a => a.us_postfix)))
                    postfix = ReplacePredefined(autoNumber.us_postfix);

                Trace("autoNumber.us_next_number.Value " + autoNumber.us_next_number);
                //Trace("autoNumber.us_next_number.Value " + autoNumber.us_next_number);
                var number = numDigits == 0 ? "" : autoNumber.us_next_number.Value.ToString("D" + numDigits);

                Trace("Number " + number.ToString());

                Target[targetAttribute] = $"{prefix}{number}{postfix}";

                var updatedAutoNumber = new us_autonumber()
                {
                    Id = autoNumber.Id,
                    us_next_number = autoNumber.us_next_number + 1,
                    us_preview = Target[targetAttribute].ToString()
                };

                OrgSvc.Update(updatedAutoNumber);
            }

            private string ReplacePredefined(string value)
            {
                value = value.Replace("{Year}", DateTime.Today.Year.ToString());
                Trace("value " + value);
                return value;
            }

            private void LockAutoNumberRecord(us_autonumber autoNumber)
            {
                var lockingUpdate = new us_autonumber()
                {
                    Id = autoNumber.Id,
                    us_preview = "Lock"
                };

                OrgSvc.Update(lockingUpdate);
            }

            private us_autonumber GetTargetAutoNumberRecord(Int32 type)
            {
                QueryExpression qry = new QueryExpression()
                {
                    EntityName = "us_autonumber",
                    ColumnSet = new ColumnSet(EntityMetadata<us_autonumber>.AttributeName(a => a.us_autonumberId),
                                              EntityMetadata<us_autonumber>.AttributeName(a => a.us_next_number),
                                              EntityMetadata<us_autonumber>.AttributeName(a => a.us_entity_name),
                                              EntityMetadata<us_autonumber>.AttributeName(a => a.us_field_name),
                                              EntityMetadata<us_autonumber>.AttributeName(a => a.us_digits),
                                              EntityMetadata<us_autonumber>.AttributeName(a => a.us_postfix),
                                              EntityMetadata<us_autonumber>.AttributeName(a => a.us_prefix),
                                              EntityMetadata<us_autonumber>.AttributeName(a => a.us_preview))                

                };

                qry.Criteria.AddCondition(EntityMetadata<us_autonumber>.AttributeName(a => a.us_entity_name), ConditionOperator.Equal, PluginCtx.PrimaryEntityName);
                qry.Criteria.AddCondition(EntityMetadata<us_autonumber>.AttributeName(a => a.statuscode), ConditionOperator.Equal, 1); //Active
                qry.Criteria.AddCondition(EntityMetadata<us_autonumber>.AttributeName(a => a.us_type), ConditionOperator.Equal, type); 

                qry.AddOrder(EntityMetadata<us_autonumber>.AttributeName(a => a.us_autonumberId), OrderType.Ascending);

                List<us_autonumber> autoNumberIdList = OrgSvc.RetrieveMultiple(qry).Entities.Cast<us_autonumber>().ToList();

                if (autoNumberIdList.Count() == 0)
                    throw new InvalidPluginExecutionException($"No active auto-number record found for {PluginCtx.PrimaryEntityName}");
                else if (autoNumberIdList.Count() > 1)
                    throw new InvalidPluginExecutionException($"Multiple active auto-number records found for {PluginCtx.PrimaryEntityName}");

                return autoNumberIdList.First();
            }
        }
    }
}
