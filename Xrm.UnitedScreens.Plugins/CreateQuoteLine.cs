using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.ServiceModel;

namespace Xrm.UnitedScreens.Plugins
{
    public class CreateQuoteLine : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //throw new NotImplementedException();

            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    
                    // Plug-in business logic goes here.
                    if (entity.LogicalName != "quote")
                        return;

                    EntityReference opportunity = entity.GetAttributeValue<EntityReference>("opportunityid");
                    if (opportunity == null)
                        return;
                    
                    Guid quoteid = entity.Id;

                    tracingService.Trace("Give query condition");
                    QueryExpression queryLineItem = new QueryExpression
                    {
                        EntityName = "us_lineitem",
                        ColumnSet = new ColumnSet(true),
                        //ColumnSet = new ColumnSet(new string[] { "us_adformat","us_additionalcampaigninformation", "us_amountofimpressions", "us_amountofviews", "us_audibility","us_budget","us_campaigntype","us_clicks","us_cpm","us_cost","us_cpv","us_numberofads","us_lengthofads","us_ifskippablespecifylengthofads","us_totalamountofimpressionstobebooked","us_overdeliveryofimpressions","us_geographicaltargeting","us_platform" }),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                            new ConditionExpression
                            {
                            AttributeName = "us_opportunityid",
                            Operator = ConditionOperator.Equal,
                            Values = { opportunity.Id }
                            }
                            }
                        }
                    };
                    tracingService.Trace("Retrieving lineitems");
                    EntityCollection lineCol = service.RetrieveMultiple(queryLineItem);

                    tracingService.Trace("Number of lineitems "+ lineCol.Entities.Count);
                    Decimal sumbudget = 0;
                    
                    
                    foreach (Entity objItem in lineCol.Entities)
                    {
                        sumbudget = sumbudget + objItem.GetAttributeValue<Money>("us_budget").Value;
                        _createlineitem(context, service, objItem, quoteid, tracingService);

                    }

                    if (entity.GetAttributeValue<string>("us_opportunitytype") == "Branded Projects")
                        CreateInfluencers(opportunity.Id, tracingService, service, entity.Id);

                    if (entity.GetAttributeValue<string>("us_opportunitytype") == "Others")
                        CreateItems(opportunity.Id, tracingService, service,entity.Id);

                    tracingService.Trace("sum of budget "+ sumbudget);
                    entity.Attributes["us_totalrevenue"] = new Money(sumbudget);
                    entity.Attributes["totallineitemamount"] = new Money(sumbudget);
                    entity.Attributes["totalamountlessfreight"] = new Money(sumbudget);
                    entity.Attributes["totalamount"] = new Money(sumbudget);                    
                    service.Update(entity);
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in CreateQuoteLinePlugin."+  ex.Message);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("CreateQuoteLinePlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }

        private void CreateInfluencers(Guid opportunityId,ITracingService tracingService, IOrganizationService service, Guid quoteid)
        {
            QueryExpression queryInfluencer = new QueryExpression
            {
                EntityName = "us_influencer",
                ColumnSet = new ColumnSet(true),                
                Criteria = new FilterExpression
                {
                    Conditions =
                            {
                            new ConditionExpression
                                {
                                AttributeName = "us_opportunityid",
                                Operator = ConditionOperator.Equal,
                                Values = { opportunityId }
                                }
                            }
                }
            };
            tracingService.Trace("Retrieving influencers");
            EntityCollection influencerCol = service.RetrieveMultiple(queryInfluencer);

            tracingService.Trace("Number of influencers " + influencerCol.Entities.Count);

            foreach (Entity objInfluencer in influencerCol.Entities)
            {
                Entity _influencer = new Entity("us_campaignpartner");

                //_influencer["us_name"] = objInfluencer.GetAttributeValue<string>("us_name");
                _influencer["us_name"] = objInfluencer.GetAttributeValue<EntityReference>("us_contactid").Name;
                _influencer["us_quoteid"] = new EntityReference("quote", quoteid);
                tracingService.Trace("Name "+objInfluencer.GetAttributeValue<EntityReference>("us_contactid").Name);              
                _influencer["us_cost"] = objInfluencer.GetAttributeValue<Money>("us_cost");
                _influencer["us_youtube"] = objInfluencer.GetAttributeValue<Int32>("us_youtube");
                _influencer["us_instagram"] = objInfluencer.GetAttributeValue<Int32>("us_instagram");
                _influencer["us_facebook"] = objInfluencer.GetAttributeValue<Int32>("us_facebook");
                _influencer["us_snapchat"] = objInfluencer.GetAttributeValue<Int32>("us_snapchat");
                _influencer["us_podcast"] = objInfluencer.GetAttributeValue<Int32>("us_podcast");
                _influencer["us_twitch"] = objInfluencer.GetAttributeValue<Int32>("us_twitch");
                _influencer["us_blog"] = objInfluencer.GetAttributeValue<Int32>("us_blog");
               


                // Create the influencer in Microsoft Dynamics CRM.
                tracingService.Trace("CreateCampaignPartnerPlugin: Creating the influencer activity.");
                service.Create(_influencer);

            }
        }

        private void CreateItems(Guid opportunityId, ITracingService tracingService, IOrganizationService service, Guid quoteid)
        {
            QueryExpression queryInfluencer = new QueryExpression
            {
                EntityName = "us_item",
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                            {
                            new ConditionExpression
                                {
                                AttributeName = "us_opportunityid",
                                Operator = ConditionOperator.Equal,
                                Values = { opportunityId }
                                }
                            }
                }
            };
            tracingService.Trace("Retrieving items");
            EntityCollection itemsCol = service.RetrieveMultiple(queryInfluencer);

            tracingService.Trace("Number of items " + itemsCol.Entities.Count);

            foreach (Entity objItem in itemsCol.Entities)
            {
                Entity _item = new Entity("us_campaignitem");

                _item["us_name"] = objItem.GetAttributeValue<string>("us_name");               
                tracingService.Trace("Name " + objItem.GetAttributeValue<string>("us_name"));
                _item["us_cost"] = objItem.GetAttributeValue<Money>("us_cost");
                _item["us_quoteid"] = new EntityReference("quote", quoteid);

                // Create the item in Microsoft Dynamics CRM.
                tracingService.Trace("CreateCampaignPartnerPlugin: Creating the influencer activity.");
                service.Create(_item);

            }
        }

        public void _createlineitem(IPluginExecutionContext context, IOrganizationService service, Entity lineitem, Guid id, ITracingService tracingService)
        {
            // Create a task activity to follow up with the account customer in 7 days. 
            Entity quoteline = new Entity("quotedetail");

            quoteline["productdescription"] = lineitem.GetAttributeValue<string>("us_name");
            tracingService.Trace("Setting cpm");
            quoteline["us_cpm"] = lineitem.GetAttributeValue<Money>("us_cpm");
            quoteline["us_cpv"] = lineitem.GetAttributeValue<Money>("us_cpv");
            quoteline["priceperunit"] = new Money(lineitem.GetAttributeValue<Money>("us_cpm").Value/1000);
            quoteline["quantity"] = lineitem.GetAttributeValue<decimal>("us_amountofimpressions");
            quoteline["us_amountofviews"] = lineitem.GetAttributeValue<decimal>("us_amountofviews");
            quoteline["us_campaigntype"] = lineitem.GetAttributeValue<OptionSetValue>("us_campaigntype"); 
            quoteline["extendedamount"] = lineitem.GetAttributeValue<Money>("us_budget");
            quoteline["baseamount"] = lineitem.GetAttributeValue<Money>("us_budget");
            quoteline["us_budget"] = lineitem.GetAttributeValue<Money>("us_budget");
            quoteline["ispriceoverridden"] = true;
            quoteline["quoteid"] = new EntityReference("quote",id);
                       

            // Create the quoteline in Microsoft Dynamics CRM.
            tracingService.Trace("CreateQuotelinePlugin: Creating the quoteline activity.");
            service.Create(quoteline);
        }
    }
}
