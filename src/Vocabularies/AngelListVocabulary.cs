// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClearBitVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the ClearBitVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CluedIn.ExternalSearch.Providers.AngelList.Vocabularies
{
    /// <summary>The clear bit vocabulary.</summary>
    public static class AngelListVocabulary
    {
        /// <summary>
        /// Initializes static members of the <see cref="KnowledgeGraphVocabulary" /> class.
        /// </summary>
        static AngelListVocabulary()
        {
            Person = new AngelListPersonVocabulary();
            Organization = new AngelListOrganizationVocabulary();
        }

        /// <summary>Gets the organization.</summary>
        /// <value>The organization.</value>
        public static AngelListPersonVocabulary Person { get; private set; }
        public static AngelListOrganizationVocabulary Organization { get; private set; }
    }
}