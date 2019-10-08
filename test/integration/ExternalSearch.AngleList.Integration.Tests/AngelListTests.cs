// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AngelListTests.cs" company="Clued In">
//   Copyright (c) 2019 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the angel list tests class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Registration;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Messages.Processing;
using CluedIn.Core.Processing;
using CluedIn.Core.Serialization;
using CluedIn.Core.Workflows;
using CluedIn.ExternalSearch;
using CluedIn.ExternalSearch.Providers.AngelList;
using CluedIn.ExternalSearch.Providers.AngelList.Model;
using CluedIn.Testing.Base.Context;
using CluedIn.Testing.Base.Processing.Actors;
using Moq;
using Xunit;

namespace ExternalSearch.AngleList.Integration.Tests
{
    public class AngelListTests
    {
        [Fact(Skip = "AngelList Api is no longer public")]
        public void Test()
        {
            using (var testContext = new TestContext())
            {
                this.Execute(testContext, "Sitecore", "https://angel.co/sitecore");
            }
        }

        [Fact(Skip = "GitHub Issue 829 - ref https://github.com/CluedIn-io/CluedIn/issues/829")]
        public void CachedQueryTest()
        {
            var name         = "Sitecore";
            var angelListUrl = "https://angel.co/sitecore";

            using (var testContext = new TestContext())
            {

                testContext.ExternalSearchRepository.Setup(r => r.GetCachedQuery(It.IsAny<ExecutionContext>(), It.IsAny<ExternalSearchQuery>())).Returns<ExecutionContext, ExternalSearchQuery>((ctx, q) =>
                {
                    var queryAngelListProfileId  = q.QueryParameters.GetValue<string, HashSet<string>>(ExternalSearchQueryParameter.Identifier.ToString(), new HashSet<string>()).FirstOrDefault();
                    var queryName                = q.QueryParameters.GetValue<string, HashSet<string>>(ExternalSearchQueryParameter.Name.ToString(), new HashSet<string>()).FirstOrDefault();

                    var query = new ExternalSearchQuery();

                    query.Id               = q.Id;
                    query.ProviderId       = q.ProviderId;
                    query.EntityType       = q.EntityType;
                    query.QueryKey         = q.QueryKey;
                    query.QueryParameters  = q.QueryParameters;
                    query.CreatedDate      = q.CreatedDate;
                    query.LastExecutedDate = q.LastExecutedDate;
                    query.Success          = true;
                    query.ResultCount      = 1;
                    query.Results          = new List<IExternalSearchQueryResult>()
                                             {
                                             new ExternalSearchQueryResult<StartupFullObject>(
                                                 query,
                                                 new StartupFullObject
                                                 {
                                                     SearchResult = new SearchResult
                                                        {
                                                            Id = "Dummy",
                                                            Name = queryName ?? name
                                                        },
                                                     Startup = new Startup()
                                                        {
                                                            id   = 1234,
                                                            name = queryName ?? name,
                                                            angellist_url = queryAngelListProfileId,
                                                        },
                                                     Roles = new List<StartupRole>()
                                                 })
                                             };

                    return query;
                });

                this.Execute(testContext, name, angelListUrl);

                testContext.ExternalSearchRepository.Verify(r => r.GetCachedQuery(It.IsAny<ExecutionContext>(), It.IsAny<ExternalSearchQuery>()), Times.Once);
            }
        }

        private void Execute(TestContext testContext, string name, string angelListUrl)
        {
            var properties = new EntityMetadataPart();
            properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Social.AngelCo, angelListUrl);

            IEntityMetadata entityMetadata = new EntityMetadataPart()
                {
                    Name        = name,
                    EntityType  = EntityType.Organization,
                    Properties  = properties.Properties 
                };

            var externalSearchProvider  = new Mock<AngelListExternalSearchProvider>(MockBehavior.Loose);
            var clues                   = new List<CompressedClue>();

            externalSearchProvider.CallBase = true;

            testContext.ProcessingHub.Setup(h => h.SendCommand(It.IsAny<ProcessClueCommand>())).Callback<IProcessingCommand>(c => clues.Add(((ProcessClueCommand)c).Clue));

            testContext.Container.Register(Component.For<IExternalSearchProvider>().UsingFactoryMethod(() => externalSearchProvider.Object));
            var context         = testContext.Context.ToProcessingContext();
            var command         = new ExternalSearchCommand();
            var actor           = new ExternalSearchProcessingAccessor(context.ApplicationContext);
            var workflow        = new Mock<Workflow>(MockBehavior.Loose, context, new EmptyWorkflowTemplate<ExternalSearchCommand>());
            
            workflow.CallBase = true;

            command.With(context);
            command.OrganizationId  = context.Organization.Id;
            command.EntityMetaData  = entityMetadata;
            command.Workflow        = workflow.Object;
            context.Workflow        = command.Workflow;

            // Act
            var result = actor.ProcessWorkflowStep(context, command);
            Assert.Equal(WorkflowStepResult.Repeat.SaveResult, result.SaveResult);

            result = actor.ProcessWorkflowStep(context, command);
            Assert.Equal(WorkflowStepResult.Success.SaveResult, result.SaveResult);
            context.Workflow.AddStepResult(result);
            
            context.Workflow.ProcessStepResult(context, command);

            // Assert
            testContext.ProcessingHub.Verify(h => h.SendCommand(It.IsAny<ProcessClueCommand>()), Times.AtLeastOnce);

            Assert.True(clues.Count > 0);
        }
    }
}
