// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AngelListOrganizationVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the AngelListOrganizationVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.AngelList.Vocabularies
{
    /// <summary>The angel list organization vocabulary.</summary>
    /// <seealso cref="CluedIn.Core.Data.Vocabularies.SimpleVocabulary" />
    public class AngelListOrganizationVocabulary : SimpleVocabulary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AngelListOrganizationVocabulary"/> class.
        /// </summary>
        public AngelListOrganizationVocabulary()
        {
            this.VocabularyName = "AngelList Organization";
            this.KeyPrefix      = "angelList.organization";
            this.KeySeparator   = ".";
            this.Grouping       = EntityType.Organization;

            this.Id               = this.Add(new VocabularyKey("id",                    VocabularyKeyDataType.Number,               VocabularyKeyVisibility.Hidden));
            this.Name             = this.Add(new VocabularyKey("name"));
            this.CompanySize      = this.Add(new VocabularyKey("companySize"));

            this.LaunchDate       = this.Add(new VocabularyKey("launchDate",            VocabularyKeyDataType.DateTime));
            this.HighConcept      = this.Add(new VocabularyKey("highConcept"));
            this.ProductDesc      = this.Add(new VocabularyKey("productDesc"));

            this.FollowerCount    = this.Add(new VocabularyKey("followerCount",         VocabularyKeyDataType.Integer));
            this.CommunityProfile = this.Add(new VocabularyKey("communityProfile",      VocabularyKeyDataType.Boolean));
            this.Quality          = this.Add(new VocabularyKey("quality",               VocabularyKeyDataType.Integer,              VocabularyKeyVisibility.Hidden));
            this.Hidden           = this.Add(new VocabularyKey("hidden", VocabularyKeyVisibility.Hidden));

            this.LogoUrl          = this.Add(new VocabularyKey("logoUrl",               VocabularyKeyDataType.Uri,                  VocabularyKeyVisibility.Hidden));
            this.ThumbUrl         = this.Add(new VocabularyKey("thumbUrl",              VocabularyKeyDataType.Uri,                  VocabularyKeyVisibility.Hidden));

            this.StatusCreatedAt  = this.Add(new VocabularyKey("statusCreatedAt"));
            this.StatusMessage    = this.Add(new VocabularyKey("statusMessage"));

            this.CompanyUrl       = this.Add(new VocabularyKey("companyUrl",            VocabularyKeyDataType.Uri));
            this.BlogUrl          = this.Add(new VocabularyKey("blogUrl",               VocabularyKeyDataType.Uri));
            this.VideoUrl         = this.Add(new VocabularyKey("videoUrl",              VocabularyKeyDataType.Uri));

            this.AngelListUrl     = this.Add(new VocabularyKey("angelListUrl",          VocabularyKeyDataType.Uri));
            this.FacebookUrl      = this.Add(new VocabularyKey("facebookUrl",           VocabularyKeyDataType.Uri));
            this.LinkedInUrl      = this.Add(new VocabularyKey("linkedInUrl",           VocabularyKeyDataType.Uri));
            this.TwitterUrl       = this.Add(new VocabularyKey("twitterUrl",            VocabularyKeyDataType.Uri));
            this.CrunchbaseUrl    = this.Add(new VocabularyKey("crunchbaseUrl",         VocabularyKeyDataType.Uri));

            this.CreatedAt        = this.Add(new VocabularyKey("createdAt"));
            this.UpdatedAt        = this.Add(new VocabularyKey("updatedAt"));
            

            this.AddMapping(this.Name,          CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.OrganizationName);

            this.AddMapping(this.AngelListUrl,  CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.WebProfileAngelList);
            this.AddMapping(this.CrunchbaseUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.WebProfileCrunchBase);
            this.AddMapping(this.TwitterUrl,    CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Social.Twitter);
            this.AddMapping(this.FacebookUrl,   CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Social.Facebook);
            this.AddMapping(this.LinkedInUrl,   CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Social.LinkedIn);
            this.AddMapping(this.CompanyUrl,    CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website);
            this.AddMapping(this.CompanySize,   CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.EmployeeCount);

            this.AddMapping(this.CreatedAt,     CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInDates.CreatedDate);
            this.AddMapping(this.UpdatedAt,     CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInDates.ModifiedDate);
        }

        public VocabularyKey Id { get; set; }

        public VocabularyKey AngelListUrl { get; set; }

        public VocabularyKey CommunityProfile { get; set; }

        public VocabularyKey CompanyUrl { get; set; }

        public VocabularyKey FollowerCount { get; set; }

        public VocabularyKey Hidden { get; set; }

        public VocabularyKey HighConcept { get; set; }

        public VocabularyKey LogoUrl { get; set; }

        public VocabularyKey Quality { get; set; }

        public VocabularyKey StatusCreatedAt { get; set; }

        public VocabularyKey StatusMessage { get; set; }

        public VocabularyKey ThumbUrl { get; set; }

        public VocabularyKey TwitterUrl { get; set; }

        public VocabularyKey UpdatedAt { get; set; }

        public VocabularyKey VideoUrl { get; set; }

        public VocabularyKey CreatedAt { get; set; }

        public VocabularyKey BlogUrl { get; set; }

        public VocabularyKey LaunchDate { get; set; }
        public VocabularyKey ProductDesc { get; set; }
        public VocabularyKey CrunchbaseUrl { get; set; }
        public VocabularyKey FacebookUrl { get; set; }
        public VocabularyKey LinkedInUrl { get; set; }
        public VocabularyKey CompanySize { get; set; }
        public VocabularyKey Name { get; set; }
    }
}
