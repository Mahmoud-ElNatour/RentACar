using System;
using System.Collections.Generic;
using RentACar.Core.Entities;

namespace RentACar.Web.ViewModels
{
    public class EmailConfigurationCenterVM
    {
        // Settings
        public NotificationSettings NotificationSettings { get; set; }
        public EmailProviderSettings ProviderSettings { get; set; }

        // Data Lists
        public List<SenderIdentity> SenderIdentities { get; set; }
        public List<EmailFeatureConfig> FeatureConfigs { get; set; }
        
        // Select Lists (Dropdowns)
        public List<EmailTemplate> AvailableTemplates { get; set; }
        public List<RentACar.Application.DTOs.DistributionListDto> AvailableDistributionLists { get; set; }
        
        // Diagnostics (Last Run)
        public ServiceRunRecord? LastRunRecord { get; set; }
    }
}
