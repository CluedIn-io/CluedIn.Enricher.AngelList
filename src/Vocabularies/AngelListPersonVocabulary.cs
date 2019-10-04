// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AngelListPersonVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the AngelListPersonVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.AngelList.Vocabularies
{
    /// <summary>The angel list person vocabulary.</summary>
    /// <seealso cref="CluedIn.Core.Data.Vocabularies.SimpleVocabulary" />
    public class AngelListPersonVocabulary : SimpleVocabulary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AngelListPersonVocabulary"/> class.
        /// </summary>
        public AngelListPersonVocabulary()
        {
            this.VocabularyName = "AngelList Person";
            this.KeyPrefix      = "angelList.person";
            this.KeySeparator   = ".";
            this.Grouping       = EntityType.Person;

            this.Id             = this.Add(new VocabularyKey("id",                      VocabularyKeyDataType.Number,           VocabularyKeyVisibility.Hidden));
            this.Name           = this.Add(new VocabularyKey("name"));
            this.Organization   = this.Add(new VocabularyKey("organization"));
            this.Bio            = this.Add(new VocabularyKey("bio"));
            this.Role           = this.Add(new VocabularyKey("role"));
            this.Investor       = this.Add(new VocabularyKey("investor",                VocabularyKeyDataType.Boolean));
            this.Location       = this.Add(new VocabularyKey("location",                VocabularyKeyDataType.GeographyLocation));

            this.Criteria       = this.Add(new VocabularyKey("criteria"));
            this.Image          = this.Add(new VocabularyKey("image", VocabularyKeyVisibility.HiddenInFrontendUI));

            this.WhatIDo        = this.Add(new VocabularyKey("whatIDo"));
            this.WhatIveBuilt   = this.Add(new VocabularyKey("whatIveBuilt"));

            this.AngellistUrl   = this.Add(new VocabularyKey("angellistUrl"));
            this.AboutmeUrl     = this.Add(new VocabularyKey("aboutmeUrl"));
            this.BehanceUrl     = this.Add(new VocabularyKey("behanceUrl"));
            this.BlogUrl        = this.Add(new VocabularyKey("blogUrl"));
            this.DribbbleUrl    = this.Add(new VocabularyKey("dribbbleUrl"));
            this.FacebookUrl    = this.Add(new VocabularyKey("facebookUrl"));
            this.FollowerCount  = this.Add(new VocabularyKey("followerCount"));
            this.GithubUrl      = this.Add(new VocabularyKey("githubUrl"));
            this.LinkedinUrl    = this.Add(new VocabularyKey("linkedinUrl"));
            this.OnlineBioUrl   = this.Add(new VocabularyKey("onlineBioUrl"));
            this.ResumeUrl      = this.Add(new VocabularyKey("resumeUrl"));
            this.TwitterUrl     = this.Add(new VocabularyKey("twitterUrl"));

            this.AddMapping(this.Name, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.FullName);

            this.AddMapping(this.AngellistUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.WebProfileAngelList);
            this.AddMapping(this.AboutmeUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.SocialAboutMe);
            this.AddMapping(this.BehanceUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.WebProfileBehance);
            //this.AddMapping(this.BlogUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.BlogUrl);
            this.AddMapping(this.DribbbleUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.WebProfileDribbble);
            this.AddMapping(this.FacebookUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.SocialFacebook);
            this.AddMapping(this.GithubUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.WebProfileGithub);
            this.AddMapping(this.LinkedinUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.SocialLinkedIn);
            this.AddMapping(this.ResumeUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.Website);
            this.AddMapping(this.OnlineBioUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.Website);
            this.AddMapping(this.TwitterUrl, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.SocialTwitter);
            this.AddMapping(this.Organization, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.Organization);
            this.AddMapping(this.Role, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.JobTitle);
        }

        public VocabularyKey AboutmeUrl     { get; set; }
        public VocabularyKey AngellistUrl   { get; set; }
        public VocabularyKey BehanceUrl     { get; set; }
        public VocabularyKey Bio            { get; set; }
        public VocabularyKey BlogUrl        { get; set; }
        public VocabularyKey Criteria       { get; set; }
        public VocabularyKey DribbbleUrl    { get; set; }
        public VocabularyKey FacebookUrl    { get; set; }
        public VocabularyKey FollowerCount  { get; set; }
        public VocabularyKey GithubUrl      { get; set; }
        public VocabularyKey Id             { get; set; }
        public VocabularyKey Image          { get; set; }
        public VocabularyKey Investor       { get; set; }
        public VocabularyKey LinkedinUrl    { get; set; }
        public VocabularyKey Location       { get; set; }
        public VocabularyKey Name           { get; set; }
        public VocabularyKey OnlineBioUrl   { get; set; }
        public VocabularyKey ResumeUrl      { get; set; }
        public VocabularyKey TwitterUrl     { get; set; }
        public VocabularyKey WhatIDo        { get; set; }
        public VocabularyKey WhatIveBuilt   { get; set; }
        public VocabularyKey Organization   { get; set; }
        public VocabularyKey Role   { get; set; }
    }
}
