﻿using Embark.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Embark.Conversion
{
    /// <summary>
    /// Interface to CRUD and other data commands to <see cref="ITextDataStore"/> and <seealso cref="ITextConverter"/>
    /// </summary>
    public class Collection  
    {
        internal Collection(string tag, ITextDataStore textDataStore, ITextConverter textConverter)
        {
            this.tag = tag;
            this.textDataStore = textDataStore;
            this.TextConverter = textConverter;
        }

        /// <summary>
        /// Text converter used by collection to serialize/deserialize to/from the <see cref="ITextDataStore"/>
        /// </summary>
        public ITextConverter TextConverter { get; private set; }

        private string tag;
        private ITextDataStore textDataStore;
        
        /// <summary>
        /// Get a type specific collection
        /// </summary>
        /// <typeparam name="T">The POCO class of the documents</typeparam>
        /// <returns><see cref="Collection{T}"/> interface to CRUD and other commands</returns>
        public Collection<T> AsGenericCollection<T>() where T : class
        {
            return new Collection<T>(this);
        }

        /// <summary>
        /// Insert a new POCO object into the collection
        /// </summary>
        /// <typeparam name="T">Any POCO class</typeparam>
        /// <param name="objectToInsert">The object to insert</param>
        /// <returns>The ID of the new document</returns>
        public long Insert<T>(T objectToInsert) where T : class
        {   
            string text = TextConverter.ToText(objectToInsert);

            TextConverter.ValidateUpload<T>(objectToInsert, text);

            return textDataStore.Insert(tag, text);
        }

        /// <summary>
        /// Update a entry in the collection
        /// </summary>
        /// <typeparam name="T">Any POCO class</typeparam> 
        /// <param name="id">The ID of the document</param>
        /// <param name="objectToUpdate">New value for the whole document. Increment/Differential updating is not supported (yet).</param>
        /// <returns>True if the document was updated</returns>
        public bool Update<T>(long id, T objectToUpdate)
        {
            string text = TextConverter.ToText(objectToUpdate);

            TextConverter.ValidateUpload<T>(objectToUpdate, text);

            return textDataStore.Update(tag, id.ToString(), text);
        }

        /// <summary>
        /// Remove an entry from the collection
        /// </summary>
        /// <param name="id">The ID of the document</param>
        /// <returns>True if the document was successfully removed.</returns>
        public bool Delete(long id)
        {
            return textDataStore.Delete(tag, id.ToString());
        }

        /// <summary>
        /// Select an existing entry in the collection
        /// </summary>
        /// <typeparam name="T">The type of the object in the document</typeparam>
        /// <param name="id">The Int64 ID of the document</param>
        /// <returns>The object entry saved in the document</returns>
        public T Get<T>(long id) where T : class
        {
            var text = textDataStore.Get(tag, id.ToString());

            return text == null ? null :
                TextConverter.ToObject<T>(text);
        }

        /// <summary>
        /// Select an existing entry in the collection, and return it in a <see cref="DocumentWrapper{T}"/>
        /// </summary>
        /// <typeparam name="T">The type of the object in the document</typeparam>
        /// <param name="id">The Int64 ID of the document</param>
        /// <returns>The document wrapper that contains the entity</returns>
        public DocumentWrapper<T> GetWrapper<T>(long id) where T : class
        {
            var text = textDataStore.Get(tag, id.ToString());

            return new DocumentWrapper<T>(id, text, this);
        }

        /// <summary>
        /// Get all documents in the collection
        /// </summary>
        /// <typeparam name="T">The POCO class represented by all documents</typeparam>
        /// <returns>A collection of <see cref="DocumentWrapper{T}"/> objects. <seealso cref="ExtensionMethods.Unwrap"/></returns>
        public IEnumerable<DocumentWrapper<T>> GetAll<T>() where T : class
        {
            return textDataStore.GetAll(tag)
                .Select(dataEnvelope => new DocumentWrapper<T>(dataEnvelope, this));
        }

        /// <summary>
        /// Get similar documents that have matching property values to an example object.
        /// </summary>
        /// <param name="searchObject">Example object to compare against</param>
        /// <typeparam name="T">The POCO class represented by all documents</typeparam>
        /// <returns><see cref="DocumentWrapper{T}"/> objects from the collection that match the search criterea. </returns>
        public IEnumerable<DocumentWrapper<T>> GetWhere<T>(object searchObject)
            where T : class
        {
            string searchText = TextConverter.ToText(searchObject);

            var searchResults = textDataStore.GetWhere(tag, searchText);

            foreach (var result in searchResults)
            {
                yield return new DocumentWrapper<T>(result, this);
            }
        }

        /// <summary>
        /// Get documents where same name property values are between values of a start and end example object
        /// </summary>
        /// <param name="startRange">The first object to compare against</param>
        /// <param name="endRange">A second object to comare values agianst to check if search is between example values</param>
        /// <typeparam name="T">The POCO class represented by all documents</typeparam>
        /// <returns><see cref="DocumentWrapper{T}"/> objects from the collection that are within the bounds of the search criterea.</returns>
        public IEnumerable<DocumentWrapper<T>> GetBetween<T>(object startRange, object endRange) where T : class
        {
            string startSearch = TextConverter.ToText(startRange);
            string endSearch = TextConverter.ToText(endRange);

            var searchResults = textDataStore.GetBetween(tag, startSearch, endSearch);

            foreach (var result in searchResults)
            {
                yield return new DocumentWrapper<T>(result, this);
            }
        }

    }
}