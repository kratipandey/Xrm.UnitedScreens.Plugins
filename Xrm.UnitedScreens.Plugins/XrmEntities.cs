//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute()]
namespace CRMRepository
{
    [System.Runtime.Serialization.DataContractAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "8.2.1.8676")]
    public enum us_autonumberState
    {

        [System.Runtime.Serialization.EnumMemberAttribute()]
        Active = 0,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        Inactive = 1,
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Runtime.Serialization.DataContractAttribute()]
    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("us_autonumber")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "8.2.1.8676")]
    public partial class us_autonumber : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
    {

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public us_autonumber() :
                base(EntityLogicalName)
        {
        }

        public const string EntityLogicalName = "us_autonumber";

        public const int EntityTypeCode = 10054;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

        private void OnPropertyChanged(string propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnPropertyChanging(string propertyName)
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Unique identifier for entity instances
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("us_autonumberid")]
        public System.Nullable<System.Guid> us_autonumberId
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>("us_autonumberid");
            }
            set
            {
                this.OnPropertyChanging("us_autonumberId");
                this.SetAttributeValue("us_autonumberid", value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = System.Guid.Empty;
                }
                this.OnPropertyChanged("us_autonumberId");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("us_autonumberid")]
        public override System.Guid Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                this.us_autonumberId = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("us_digits")]
        public System.Nullable<int> us_digits
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("us_digits");
            }
            set
            {
                this.OnPropertyChanging("us_digits");
                this.SetAttributeValue("us_digits", value);
                this.OnPropertyChanged("us_digits");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("us_entity_name")]
        public string us_entity_name
        {
            get
            {
                return this.GetAttributeValue<string>("us_entity_name");
            }
            set
            {
                this.OnPropertyChanging("us_entity_name");
                this.SetAttributeValue("us_entity_name", value);
                this.OnPropertyChanged("us_entity_name");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("us_field_name")]
        public string us_field_name
        {
            get
            {
                return this.GetAttributeValue<string>("us_field_name");
            }
            set
            {
                this.OnPropertyChanging("us_field_name");
                this.SetAttributeValue("us_field_name", value);
                this.OnPropertyChanged("us_field_name");
            }
        }

        /// <summary>
        /// The name of the custom entity.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("us_name")]
        public string us_name
        {
            get
            {
                return this.GetAttributeValue<string>("us_name");
            }
            set
            {
                this.OnPropertyChanging("us_name");
                this.SetAttributeValue("us_name", value);
                this.OnPropertyChanged("us_name");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("us_next_number")]
        public System.Nullable<int> us_next_number
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("us_next_number");
            }
            set
            {
                this.OnPropertyChanging("us_next_number");
                this.SetAttributeValue("us_next_number", value);
                this.OnPropertyChanged("us_next_number");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("us_next_reset")]
        public System.Nullable<System.DateTime> us_next_reset
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("us_next_reset");
            }
            set
            {
                this.OnPropertyChanging("us_next_reset");
                this.SetAttributeValue("us_next_reset", value);
                this.OnPropertyChanged("us_next_reset");
            }
        }

        /// <summary>
        /// Type
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("us_type")]
        public Microsoft.Xrm.Sdk.OptionSetValue us_type
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("us_type");
            }
            set
            {
                this.OnPropertyChanging("us_type");
                this.SetAttributeValue("us_type", value);
                this.OnPropertyChanged("us_type");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("us_postfix")]
        public string us_postfix
        {
            get
            {
                return this.GetAttributeValue<string>("us_postfix");
            }
            set
            {
                this.OnPropertyChanging("us_postfix");
                this.SetAttributeValue("us_postfix", value);
                this.OnPropertyChanged("us_postfix");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("us_prefix")]
        public string us_prefix
        {
            get
            {
                return this.GetAttributeValue<string>("us_prefix");
            }
            set
            {
                this.OnPropertyChanging("us_prefix");
                this.SetAttributeValue("us_prefix", value);
                this.OnPropertyChanged("us_prefix");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("us_preview")]
        public string us_preview
        {
            get
            {
                return this.GetAttributeValue<string>("us_preview");
            }
            set
            {
                this.OnPropertyChanging("us_preview");
                this.SetAttributeValue("us_preview", value);
                this.OnPropertyChanged("us_preview");
            }
        }

        /// <summary>
        /// Unique identifier of the user who created the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
        public Microsoft.Xrm.Sdk.EntityReference CreatedBy
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdby");
            }
        }

        /// <summary>
        /// Date and time when the record was created.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdon")]
        public System.Nullable<System.DateTime> CreatedOn
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("createdon");
            }
        }

        /// <summary>
        /// Unique identifier of the delegate user who created the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
        public Microsoft.Xrm.Sdk.EntityReference CreatedOnBehalfBy
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdonbehalfby");
            }
        }

        /// <summary>
        /// Sequence number of the import that created this record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("importsequencenumber")]
        public System.Nullable<int> ImportSequenceNumber
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("importsequencenumber");
            }
            set
            {
                this.OnPropertyChanging("ImportSequenceNumber");
                this.SetAttributeValue("importsequencenumber", value);
                this.OnPropertyChanged("ImportSequenceNumber");
            }
        }

        /// <summary>
        /// Unique identifier of the user who modified the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
        public Microsoft.Xrm.Sdk.EntityReference ModifiedBy
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("modifiedby");
            }
        }

        /// <summary>
        /// Date and time when the record was modified.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedon")]
        public System.Nullable<System.DateTime> ModifiedOn
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("modifiedon");
            }
        }

        /// <summary>
        /// Unique identifier of the delegate user who modified the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
        public Microsoft.Xrm.Sdk.EntityReference ModifiedOnBehalfBy
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("modifiedonbehalfby");
            }
        }

        /// <summary>
        /// Date and time that the record was migrated.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("overriddencreatedon")]
        public System.Nullable<System.DateTime> OverriddenCreatedOn
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("overriddencreatedon");
            }
            set
            {
                this.OnPropertyChanging("OverriddenCreatedOn");
                this.SetAttributeValue("overriddencreatedon", value);
                this.OnPropertyChanged("OverriddenCreatedOn");
            }
        }

        /// <summary>
        /// Owner Id
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("ownerid")]
        public Microsoft.Xrm.Sdk.EntityReference OwnerId
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("ownerid");
            }
            set
            {
                this.OnPropertyChanging("OwnerId");
                this.SetAttributeValue("ownerid", value);
                this.OnPropertyChanged("OwnerId");
            }
        }

        /// <summary>
        /// Unique identifier for the business unit that owns the record
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningbusinessunit")]
        public Microsoft.Xrm.Sdk.EntityReference OwningBusinessUnit
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("owningbusinessunit");
            }
        }

        /// <summary>
        /// Unique identifier for the team that owns the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningteam")]
        public Microsoft.Xrm.Sdk.EntityReference OwningTeam
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("owningteam");
            }
        }

        /// <summary>
        /// Unique identifier for the user that owns the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owninguser")]
        public Microsoft.Xrm.Sdk.EntityReference OwningUser
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("owninguser");
            }
        }

        /// <summary>
        /// Status of the Auto number
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statecode")]
        public System.Nullable<CRMRepository.us_autonumberState> statecode
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statecode");
                if ((optionSet != null))
                {
                    return ((CRMRepository.us_autonumberState)(System.Enum.ToObject(typeof(CRMRepository.us_autonumberState), optionSet.Value)));
                }
                else
                {
                    return null;
                }
            }
            set
            {
                this.OnPropertyChanging("statecode");
                if ((value == null))
                {
                    this.SetAttributeValue("statecode", null);
                }
                else
                {
                    this.SetAttributeValue("statecode", new Microsoft.Xrm.Sdk.OptionSetValue(((int)(value))));
                }
                this.OnPropertyChanged("statecode");
            }
        }

        /// <summary>
        /// Reason for the status of the Auto number
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statuscode")]
        public Microsoft.Xrm.Sdk.OptionSetValue statuscode
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statuscode");
            }
            set
            {
                this.OnPropertyChanging("statuscode");
                this.SetAttributeValue("statuscode", value);
                this.OnPropertyChanged("statuscode");
            }
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("timezoneruleversionnumber")]
        public System.Nullable<int> TimeZoneRuleVersionNumber
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("timezoneruleversionnumber");
            }
            set
            {
                this.OnPropertyChanging("TimeZoneRuleVersionNumber");
                this.SetAttributeValue("timezoneruleversionnumber", value);
                this.OnPropertyChanged("TimeZoneRuleVersionNumber");
            }
        }

        /// <summary>
        /// Time zone code that was in use when the record was created.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("utcconversiontimezonecode")]
        public System.Nullable<int> UTCConversionTimeZoneCode
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("utcconversiontimezonecode");
            }
            set
            {
                this.OnPropertyChanging("UTCConversionTimeZoneCode");
                this.SetAttributeValue("utcconversiontimezonecode", value);
                this.OnPropertyChanged("UTCConversionTimeZoneCode");
            }
        }

        /// <summary>
        /// Version Number
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("versionnumber")]
        public System.Nullable<long> VersionNumber
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<long>>("versionnumber");
            }
        }
    }
}
