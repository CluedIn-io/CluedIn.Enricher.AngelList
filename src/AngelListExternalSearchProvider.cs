// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AngelListExternalSearchProvider.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the AngelListExternalSearchProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using CluedIn.Core;
using CluedIn.Core.Configuration;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Data.Vocabularies.CluedIn;
using CluedIn.Core.Processing;
using CluedIn.Crawling.Helpers;
using CluedIn.ExternalSearch.Filters;
using CluedIn.ExternalSearch.Providers.AngelList.Model;
using CluedIn.ExternalSearch.Providers.AngelList.Vocabularies;

using RestSharp;

namespace CluedIn.ExternalSearch.Providers.AngelList
{
    /// <summary>The angel list external search provider.</summary>
    /// <seealso cref="CluedIn.ExternalSearch.ExternalSearchProviderBase" />
    public class AngelListExternalSearchProvider : ExternalSearchProviderBase
    {
        /**********************************************************************************************************
         * FIELDS
         **********************************************************************************************************/
        private List<string> sharedApiTokens = new List<string>()
            {
                "0ab8b29d6592cd163282cf92340192a8e27a5599e87040d7", "925adf7b4e10a560bc05cc14c105ccb366bc45b3f96940d1", "61f4fce6f719068548c6cee8f51293743c2ab300458ef641"
            };

        /// <summary>The shared API tokens index</summary>
        private int sharedApiTokensIdx = 0;

        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="AngelListExternalSearchProvider" /> class.
        /// </summary>
        public AngelListExternalSearchProvider()
            : base(ExternalSearchProviderPriority.Last, Constants.ExternalSearchProviders.AngelListId, EntityType.Organization, EntityType.Person)
        {
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        public override bool IsExecuteSearchEnabled(ExecutionContext context)
        {
            // AngelList closed their API / requires paid account.
            // Disabling executing searches against the live API, but results is still produced for existing cached queries
            return false;
        }

        /// <summary>Builds the queries.</summary>
        /// <param name="context">The context.</param>
        /// <param name="request">The request.</param>
        /// <returns>The search queries.</returns>
        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request)
        {
            if (!this.Accepts(request.EntityMetaData.EntityType))
                return new IExternalSearchQuery[0];

            if (request.EntityMetaData.EntityType.Is(EntityType.Organization))
                return this.BuildCompanyQueries(context, request);
            else if (request.EntityMetaData.EntityType.Is(EntityType.Person))
                return this.BuildUserQueries(context, request);

            return new IExternalSearchQuery[0];
        }

        private IEnumerable<IExternalSearchQuery> BuildCompanyQueries(ExecutionContext context, IExternalSearchRequest request)
        {
            if (request.EntityMetaData.Properties.GetValue(AngelListVocabulary.Organization.Id, null) != null)
                yield break;

            var existingResults = request.GetQueryResults<StartupFullObject>(this).ToList();

            Func<string, bool> profileUriFilter = value => existingResults.Any(r => string.Equals(UriUtility.GetAngelCoProfileId(r.Data.Startup.angellist_url), value, StringComparison.InvariantCultureIgnoreCase));
            Func<string, bool> nameFilter       = value => OrganizationFilters.NameFilter(context, value) || existingResults.Any(r => string.Equals(r.Data.Startup.name, value, StringComparison.InvariantCultureIgnoreCase));

            // Query Input
            var entityType          = request.EntityMetaData.EntityType;
            var angelListId         = request.QueryParameters.GetValue(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Social.AngelCo, null);
            var angelListId2        = request.QueryParameters.GetValue(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.WebProfileAngelList, null);
            var organizationName    = request.QueryParameters.GetValue(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.OrganizationName, new HashSet<string>());

            if (!string.IsNullOrEmpty(request.EntityMetaData.Name))
                organizationName.Add(request.EntityMetaData.Name);
            if (!string.IsNullOrEmpty(request.EntityMetaData.DisplayName))
                organizationName.Add(request.EntityMetaData.DisplayName);

            request.EntityMetaData.Aliases.ForEach(a => organizationName.Add(a));

            angelListId = angelListId != null
                            ? 
                                angelListId2 != null
                                ? angelListId.Concat(angelListId2).ToHashSet()
                                : angelListId
                            : angelListId2;

            if (angelListId != null)
            {
                var values = angelListId.Where(UriUtility.IsAngelCoProfileUri)
                                        .Select(UriUtility.GetAngelCoProfileId);

                foreach (var value in values.Where(v => !profileUriFilter(v)))
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Identifier, value);
            }
            else if (organizationName != null)
            {
                var values = organizationName.GetOrganizationNameVariants()
                                             .Select(NameNormalization.Normalize)
                                             .ToHashSet();

                foreach (var value in values.Where(v => !nameFilter(v)))
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Name, value);
            }
        }

        private IEnumerable<IExternalSearchQuery> BuildUserQueries(ExecutionContext context, IExternalSearchRequest request)
        {
            var existingResults = request.GetQueryResults<StartupRole>(this).ToList();

            Func<string, bool> userProfileIdFilter = value => existingResults.Any(r => string.Equals(r.Data.tagged.id.ToString(), value, StringComparison.InvariantCultureIgnoreCase));

            // Query Input
            var entityType  = request.EntityMetaData.EntityType;
            var angelListId = request.QueryParameters.GetValue(AngelListVocabulary.Person.Id, null);

            if (angelListId != null)
            {
                var values = angelListId.Where(v => v.IsNumber());

                foreach (var value in values.Where(v => !userProfileIdFilter(v)))
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Identifier, value);
            }
        }

        /// <summary>Executes the search.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <returns>The results.</returns>
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query)
        {
            if (query.EntityType.Is(EntityType.Organization))
                return this.ExecuteCompanySearch(context, query);
            else if (query.EntityType.Is(EntityType.Person))
                return this.ExecuteUserSearch(context, query);

            throw new ApplicationException("Cannot execute external search query for " + query.EntityType);
        }

        private IEnumerable<IExternalSearchQueryResult> ExecuteCompanySearch(ExecutionContext context, IExternalSearchQuery query)
        {
            var angelListProfileId  = query.QueryParameters.GetValue<string, HashSet<string>>(ExternalSearchQueryParameter.Identifier.ToString(), new HashSet<string>()).FirstOrDefault();
            var name                = query.QueryParameters.GetValue<string, HashSet<string>>(ExternalSearchQueryParameter.Name.ToString(), new HashSet<string>()).FirstOrDefault();

            var client = new RestClient("https://api.angel.co/");

            List<SearchResult> results = null;

            if (!string.IsNullOrEmpty(angelListProfileId))
                results = this.GetSearchResults(angelListProfileId, client);
            else if (!string.IsNullOrEmpty(name))
                results = this.GetSearchResultsByName(name, client);

            if (results != null)
            {
                foreach (var searchResult in results)
                {
                    var profile  = this.GetStartupProfile(searchResult.Id, client);

                    if (profile == null)
                        continue;

                    if (!string.IsNullOrEmpty(name) && profile.community_profile) // Skip community profiles if searched by name
                        continue;

                    var roles = new List<StartupRole>();
                    if (ConfigurationManager.AppSettings.GetFlag("Feature.ExternalSearch.AngelList.Roles", true))
                    {
                        var startupRoles    = this.GetStartupRoles(searchResult.Id, client).Where(r => r.tagged != null && r.tagged.type == "User");
                        roles               = startupRoles.ToList();
                    }
                    
                    yield return new ExternalSearchQueryResult<StartupFullObject>(query, new StartupFullObject { SearchResult = searchResult, Startup = profile, Roles = roles });
                }
            }
        }

        private IEnumerable<IExternalSearchQueryResult> ExecuteUserSearch(ExecutionContext context, IExternalSearchQuery query)
        {
            var angelListProfileId  = query.QueryParameters.GetValue<string, HashSet<string>>(ExternalSearchQueryParameter.Identifier.ToString(), new HashSet<string>()).FirstOrDefault();

            var client = new RestClient("https://api.angel.co/");

            if (string.IsNullOrEmpty(angelListProfileId))
                yield break;

            var role = query.CustomQueryInput as StartupRole;

            if (role == null)
                yield break;

            var userProfile = this.GetUserProfile(int.Parse(angelListProfileId), client);

            if (userProfile == null)
                yield break;

            yield return new ExternalSearchQueryResult<UserProfile>(query, userProfile);
        }

        private List<SearchResult> GetSearchResults(string angelListProfileId, RestClient client)
        {
            string sharedApiToken;

            lock (this)
            {
                sharedApiToken = this.sharedApiTokens[this.sharedApiTokensIdx++];

                if (this.sharedApiTokensIdx >= this.sharedApiTokens.Count)
                    this.sharedApiTokensIdx = 0;
            }


            var request  = new RestRequest(string.Format("/1/search?query={0}&type=Startup&access_token={1}", angelListProfileId, sharedApiToken), Method.GET);
            var response = client.ExecuteTaskAsync<List<SearchResult>>(request).Result;

            if (response.StatusCode == HttpStatusCode.OK)
                return response.Data.Where(r => r.Name == angelListProfileId || UriUtility.GetAngelCoProfileId(r.Url) == angelListProfileId).ToList();
            else if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
                return null;
            else if (response.ErrorException != null)
                throw new AggregateException(response.ErrorException.Message, response.ErrorException);
            else if (response.ErrorMessage != null)
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode + "; ErrorMessage: " + response.ErrorMessage);
            else
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode);
        }

        private List<SearchResult> GetSearchResultsByName(string name, RestClient client)
        {
            string sharedApiToken;

            lock (this)
            {
                sharedApiToken = this.sharedApiTokens[this.sharedApiTokensIdx++];

                if (this.sharedApiTokensIdx >= this.sharedApiTokens.Count)
                    this.sharedApiTokensIdx = 0;
            }

            var request  = new RestRequest("/1/search", Method.GET);
            request.AddQueryParameter("query", name);
            request.AddQueryParameter("type", "Startup");
            request.AddQueryParameter("access_token", sharedApiToken);

            var response = client.ExecuteTaskAsync<List<SearchResult>>(request).Result;

            if (response.StatusCode == HttpStatusCode.OK)
                return response.Data;
            else if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
                return null;
            else if (response.ErrorException != null)
                throw new AggregateException(response.ErrorException.Message, response.ErrorException);
            else if (response.ErrorMessage != null)
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode + "; ErrorMessage: " + response.ErrorMessage);
            else
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode);
        }

        private Startup GetStartupProfile(string profileId, RestClient client)
        {
            string sharedApiToken;

            lock (this)
            {
                sharedApiToken = this.sharedApiTokens[this.sharedApiTokensIdx++];

                if (this.sharedApiTokensIdx >= this.sharedApiTokens.Count)
                    this.sharedApiTokensIdx = 0;
            }

            var request  = new RestRequest(string.Format("/1/startups/{0}?access_token={1}", profileId, sharedApiToken), Method.GET);
            var response = client.ExecuteTaskAsync<Startup>(request).Result;

            if (response.StatusCode == HttpStatusCode.OK)
                return response.Data;
            else if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
                return null;
            else if (response.ErrorException != null)
                throw new AggregateException(response.ErrorException.Message, response.ErrorException);
            else if (response.ErrorMessage != null)
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode + "; ErrorMessage: " + response.ErrorMessage);
            else
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode);
        }

        private IEnumerable<StartupRole> GetStartupRoles(string profileId, RestClient client)
        {
            var exceptions = new List<Exception>();

            int  maxPages       = 1;
            bool foundResults   = false;

            string sharedApiToken;

            lock (this)
            {
                sharedApiToken = this.sharedApiTokens[this.sharedApiTokensIdx++];

                if (this.sharedApiTokensIdx >= this.sharedApiTokens.Count)
                    this.sharedApiTokensIdx = 0;
            }

            for (int page = 1; page <= maxPages; page++)
            {
                var request  = new RestRequest(string.Format("/1/startups/{0}/roles?page={1}&access_token={2}", profileId, page, sharedApiToken), Method.GET);
                var response = client.ExecuteTaskAsync<Roles>(request).Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    maxPages = response.Data.total;

                    foreach (var role in response.Data.startup_roles)
                        yield return role;

                    foundResults = true;
                }
                else if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
                    continue;
                else if (response.ErrorException != null)
                    exceptions.Add(new AggregateException(response.ErrorException.Message, response.ErrorException));
                else if (response.ErrorMessage != null)
                    exceptions.Add(new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode + "; ErrorMessage: " + response.ErrorMessage));
                else
                    exceptions.Add(new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode));

                if (page < maxPages)
                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }

            if (!foundResults && exceptions.Count > 0)
            {
                if (exceptions.Count == 1)
                    throw exceptions[0];

                throw new AggregateException("Could not execute external search query", exceptions);
            }
        }

        private UserProfile GetUserProfile(int userId, RestClient client)
        {
            if (userId <= 0)
                return null;

            string sharedApiToken;

            lock (this)
            {
                sharedApiToken = this.sharedApiTokens[this.sharedApiTokensIdx++];

                if (this.sharedApiTokensIdx >= this.sharedApiTokens.Count)
                    this.sharedApiTokensIdx = 0;
            }

            var request  = new RestRequest(string.Format("/1/users/{0}?access_token={1}", userId, sharedApiToken), Method.GET);
            var response = client.ExecuteTaskAsync<UserProfile>(request).Result;

            if (response.StatusCode == HttpStatusCode.OK)
                return response.Data;
            else if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
                return null;
            else if (response.ErrorException != null)
                throw new AggregateException(response.ErrorException.Message, response.ErrorException);
            else if (response.ErrorMessage != null)
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode + "; ErrorMessage: " + response.ErrorMessage);
            else
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode);
        }

        /// <summary>Builds the clues.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The clues.</returns>
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem      = result.As<StartupFullObject>();
            var roleResultItem  = result.As<UserProfile>();

            if (resultItem != null)
                return this.BuildCompanyClues(context, query, resultItem, request);
            if (roleResultItem != null)
                return this.BuildUserClues(context, query, roleResultItem, request);

            return new Clue[0];
        }

        private IEnumerable<Clue> BuildCompanyClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult<StartupFullObject> resultItem, IExternalSearchRequest request)
        {
            var code = this.GetOriginEntityCode(resultItem);

            var clue = new Clue(code, context.Organization);

            clue.Data.OriginProviderDefinitionId = this.Id;

            this.PopulateMetadata(clue.Data.EntityData, resultItem);

            if (resultItem.Data.Startup.logo_url != null)
                this.DownloadPreviewImage(context, resultItem.Data.Startup.logo_url, clue);

            yield return clue;

            var processingContext = (ProcessingContext)context;

            foreach (var role in resultItem.Data.Roles)
            {
                var roleMetadata = new EntityMetadataPart();

                this.PopulateSearchRoleMetadata(roleMetadata, role);

                processingContext.Workflow.StartSubWorkflow(() => new ExternalSearchCommand(processingContext, Guid.NewGuid(), roleMetadata) { CustomQueryInput = role });
            }
        }

        private IEnumerable<Clue> BuildUserClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult<UserProfile> resultItem, IExternalSearchRequest request)
        {
            if (resultItem.Data == null)
                yield break;

            var startupRole = query.CustomQueryInput as StartupRole;
            var userProfile = resultItem.Data;

            var roleCode = this.GetOriginEntityCode(userProfile);
            var roleClue = new Clue(roleCode, context.Organization);

            this.PopulateRoleMetadata(roleClue.Data.EntityData, userProfile);

            roleClue.Data.EntityData.Properties[AngelListVocabulary.Person.Organization]    = startupRole.startup.name;
            roleClue.Data.EntityData.Properties[AngelListVocabulary.Person.Role]            = startupRole.role != "employee" ? startupRole.role : null;

            var code = new EntityCode(EntityType.Organization, this.GetCodeOrigin(), startupRole.startup.id);
            var from = new EntityReference(roleCode);
            var to   = new EntityReference(code, startupRole.startup.name);
            var edge = new EntityEdge(from, to, EntityEdgeType.PartOf);

            roleClue.Data.EntityData.OutgoingEdges.Add(edge);

            if (userProfile.Image != null)
                this.DownloadPreviewImage(context, userProfile.Image, roleClue);

            yield return roleClue;
        }

        /// <summary>Gets the primary entity metadata.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The primary entity metadata.</returns>
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var companyResultItem   = result.As<StartupFullObject>();
            var userResultItem      = result.As<UserProfile>();

            if (companyResultItem != null)
                return this.CreateMetadata(companyResultItem);
            else if (userResultItem != null)
                return this.CreateMetadata(userResultItem);

            return null;
        }

        /// <summary>Gets the preview image.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The preview image.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem      = result.As<StartupFullObject>();
            var roleResultItem  = result.As<UserProfile>();

            if (resultItem != null && resultItem.Data.Startup.logo_url != null)
                return this.DownloadPreviewImageBlob(context, resultItem.Data.Startup.logo_url);
            if (roleResultItem != null && roleResultItem.Data.Image != null)
                return this.DownloadPreviewImageBlob(context, roleResultItem.Data.Image);

            return null;
        }

        /// <summary>Creates the metadata.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The metadata.</returns>
        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<StartupFullObject> resultItem)
        {
            var metadata = new EntityMetadataPart();

            this.PopulateMetadata(metadata, resultItem);

            return metadata;
        }

        /// <summary>Creates the metadata.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The metadata.</returns>
        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<UserProfile> resultItem)
        {
            var metadata = new EntityMetadataPart();

            this.PopulateRoleMetadata(metadata, resultItem.Data);

            return metadata;
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(IExternalSearchQueryResult<StartupFullObject> resultItem)
        {
            return new EntityCode(EntityType.Organization, this.GetCodeOrigin(), resultItem.Data.Startup.id);
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(UserProfile resultItem)
        {
            return new EntityCode(EntityType.Person, this.GetCodeOrigin(), resultItem.Id);
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(StartupRole resultItem)
        {
            return new EntityCode(EntityType.Person, this.GetCodeOrigin(), resultItem.tagged.id);
        }

        /// <summary>Gets the code origin.</summary>
        /// <returns>The code origin</returns>
        private CodeOrigin GetCodeOrigin()
        {
            return CodeOrigin.CluedIn.CreateSpecific("angelList");
        }

        /// <summary>Populates the metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="resultItem">The result item.</param>
        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<StartupFullObject> resultItem)
        {
            var code = this.GetOriginEntityCode(resultItem);

            metadata.EntityType         = EntityType.Organization;
            metadata.Name               = resultItem.Data.Startup.name;
            metadata.Description        = resultItem.Data.Startup.product_desc;
            metadata.OriginEntityCode   = code;

            metadata.Codes.Add(code);

            metadata.Properties[AngelListVocabulary.Organization.Id]                = resultItem.Data.Startup.id.PrintIfAvailable();
            metadata.Properties[AngelListVocabulary.Organization.Name]              = resultItem.Data.Startup.name;
            metadata.Properties[AngelListVocabulary.Organization.AngelListUrl]      = resultItem.Data.Startup.angellist_url;
            metadata.Properties[AngelListVocabulary.Organization.BlogUrl]           = resultItem.Data.Startup.blog_url;
            metadata.Properties[AngelListVocabulary.Organization.CommunityProfile]  = resultItem.Data.Startup.community_profile.PrintIfAvailable();
            metadata.Properties[AngelListVocabulary.Organization.CompanyUrl]        = resultItem.Data.Startup.company_url;
            metadata.Properties[AngelListVocabulary.Organization.CreatedAt]         = resultItem.Data.Startup.created_at;
            metadata.Properties[AngelListVocabulary.Organization.FollowerCount]     = resultItem.Data.Startup.follower_count.PrintIfAvailable();
            metadata.Properties[AngelListVocabulary.Organization.Hidden]            = resultItem.Data.Startup.hidden.ToString();
            metadata.Properties[AngelListVocabulary.Organization.HighConcept]       = resultItem.Data.Startup.high_concept;
            metadata.Properties[AngelListVocabulary.Organization.LogoUrl]           = resultItem.Data.Startup.logo_url;
            metadata.Properties[AngelListVocabulary.Organization.Quality]           = resultItem.Data.Startup.quality.PrintIfAvailable();

            metadata.Properties[AngelListVocabulary.Organization.StatusCreatedAt]   = resultItem.Data.Startup.status.PrintIfAvailable(v => v.created_at);
            metadata.Properties[AngelListVocabulary.Organization.StatusMessage]     = resultItem.Data.Startup.status.PrintIfAvailable(v => v.message);

            metadata.Properties[AngelListVocabulary.Organization.ThumbUrl]          = resultItem.Data.Startup.thumb_url;
            metadata.Properties[AngelListVocabulary.Organization.CrunchbaseUrl]     = resultItem.Data.Startup.crunchbase_url;
            metadata.Properties[AngelListVocabulary.Organization.TwitterUrl]        = resultItem.Data.Startup.twitter_url;
            metadata.Properties[AngelListVocabulary.Organization.FacebookUrl]       = resultItem.Data.Startup.facebook_url;
            metadata.Properties[AngelListVocabulary.Organization.LinkedInUrl]       = resultItem.Data.Startup.linkedin_url;
            metadata.Properties[AngelListVocabulary.Organization.UpdatedAt]         = resultItem.Data.Startup.updated_at;
            metadata.Properties[AngelListVocabulary.Organization.VideoUrl]          = resultItem.Data.Startup.video_url;
            metadata.Properties[AngelListVocabulary.Organization.LaunchDate]        = resultItem.Data.Startup.launch_date;
            metadata.Properties[AngelListVocabulary.Organization.ProductDesc]       = resultItem.Data.Startup.product_desc;
            metadata.Properties[AngelListVocabulary.Organization.CompanySize]       = resultItem.Data.Startup.company_size;

            if (resultItem.Data.Startup.locations != null)
                foreach (var location in resultItem.Data.Startup.locations)
                {
                    metadata.Tags.Add(new Tag(location.display_name, new EntityCode(EntityType.Tag, CodeOrigin.CluedIn.CreateSpecific("angelList"), location.id)));
                }

            if (resultItem.Data.Startup.markets != null)
                foreach (var market in resultItem.Data.Startup.markets)
                {
                    metadata.Tags.Add(new Tag(market.display_name, new EntityCode(EntityType.Tag, CodeOrigin.CluedIn.CreateSpecific("angelList"), market.id)));
                }

            if (resultItem.Data.Startup.company_type != null)
                foreach (var companyType in resultItem.Data.Startup.company_type)
                {
                    metadata.Tags.Add(new Tag(companyType.display_name, new EntityCode(EntityType.Tag, CodeOrigin.CluedIn.CreateSpecific("angelList"), companyType.id)));
                }
        }

        private void PopulateSearchRoleMetadata(EntityMetadataPart metadata, StartupRole startupRole)
        {
            var tagged  = startupRole.tagged;
            var code    = this.GetOriginEntityCode(startupRole);

            metadata.EntityType       = EntityType.Person;
            metadata.Name             = tagged.name;
            metadata.OriginEntityCode = code;

            metadata.Codes.Add(code);

            metadata.Properties[AngelListVocabulary.Person.Id]              = tagged.id.PrintIfAvailable();
            metadata.Properties[AngelListVocabulary.Person.AngellistUrl]    = tagged.angellist_url;
            metadata.Properties[AngelListVocabulary.Person.Name]            = tagged.name;
        }

        private void PopulateRoleMetadata(IEntityMetadata metadata, UserProfile userProfile)
        {
            var code = this.GetOriginEntityCode(userProfile);

            metadata.EntityType       = EntityType.Person;
            metadata.Name             = userProfile.Name;
            metadata.OriginEntityCode = code;

            metadata.Codes.Add(code);

            metadata.Properties[AngelListVocabulary.Person.AboutmeUrl]                  = userProfile.AboutmeUrl;
            metadata.Properties[AngelListVocabulary.Person.AngellistUrl]                = userProfile.AngellistUrl;
            metadata.Properties[AngelListVocabulary.Person.BehanceUrl]                  = userProfile.BehanceUrl;
            metadata.Properties[AngelListVocabulary.Person.Bio]                         = userProfile.Bio;
            metadata.Properties[AngelListVocabulary.Person.BlogUrl]                     = userProfile.BlogUrl;
            metadata.Properties[AngelListVocabulary.Person.DribbbleUrl]                 = userProfile.DribbbleUrl;
            metadata.Properties[AngelListVocabulary.Person.FacebookUrl]                 = userProfile.FacebookUrl;
            metadata.Properties[AngelListVocabulary.Person.FollowerCount]               = userProfile.FollowerCount.PrintIfAvailable();
            metadata.Properties[AngelListVocabulary.Person.GithubUrl]                   = userProfile.GithubUrl;
            metadata.Properties[AngelListVocabulary.Person.Id]                          = userProfile.Id.PrintIfAvailable();
            metadata.Properties[AngelListVocabulary.Person.Image]                       = userProfile.Image;
            metadata.Properties[AngelListVocabulary.Person.Investor]                    = userProfile.Investor.PrintIfAvailable();
            metadata.Properties[AngelListVocabulary.Person.LinkedinUrl]                 = userProfile.LinkedinUrl;
            metadata.Properties[AngelListVocabulary.Person.Name]                        = userProfile.Name;
            metadata.Properties[AngelListVocabulary.Person.OnlineBioUrl]                = userProfile.OnlineBioUrl;
            metadata.Properties[AngelListVocabulary.Person.ResumeUrl]                   = userProfile.ResumeUrl;
            metadata.Properties[AngelListVocabulary.Person.TwitterUrl]                  = userProfile.TwitterUrl;
            metadata.Properties[AngelListVocabulary.Person.WhatIDo]                     = userProfile.WhatIDo;
            metadata.Properties[AngelListVocabulary.Person.WhatIveBuilt]                = userProfile.WhatIveBuilt;

            metadata.Properties[AngelListVocabulary.Person.Location]                    = userProfile.Locations.PrintIfAvailable(l => l.FirstOrDefault(), l => l.display_name);
        }
    }
}
