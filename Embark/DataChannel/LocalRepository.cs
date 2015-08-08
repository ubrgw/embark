﻿using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using Embark.Storage;
using Embark.TextConversion;

namespace Embark.DataChannel
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    internal class LocalRepository : ITextRepository
    {
        public LocalRepository(FileDataStore dataStore, ITextConverter textComparer)
        {
            this.dataStore = dataStore;
            this.textComparer = textComparer;
        }

        private FileDataStore dataStore;
        private ITextConverter textComparer;

        // Basic
        long ITextRepository.Insert(string tag, string objectToInsert) 
            => dataStore.Insert(tag, objectToInsert);

        bool ITextRepository.Update(string tag, long id, string objectToUpdate)
            => dataStore.Update(tag, id, objectToUpdate);

        bool ITextRepository.Delete(string tag, long id)
            => dataStore.Delete(tag, id);

        string ITextRepository.Get(string tag, long id)
            => dataStore.Get(tag, id);

        IEnumerable<DataEnvelope> ITextRepository.GetAll(string tag)
            => GetAll(tag);

        private IEnumerable<DataEnvelope> GetAll(string tag)
            => dataStore.GetAll(tag);

        IEnumerable<DataEnvelope> ITextRepository.GetWhere(string tag, string searchObject)
        {
            var allFiles = GetAll(tag);

            var propertyLookup = textComparer.ToComparisonObject(searchObject);

            var matches = allFiles
                .Select(envelope => new
                {
                    envelope = envelope,
                    graph = textComparer.ToComparisonObject(envelope.Text)
                })
                .Where(comparison => textComparer.IsMatch(propertyLookup, comparison.graph))
                .Select(e => e.envelope);

            return matches;
        }

        IEnumerable<DataEnvelope> ITextRepository.GetBetween(string tag, string startRange, string endRange)
        {
            var allFiles = GetAll(tag);

            var startLookup = textComparer.ToComparisonObject(startRange);
            var endLookup = textComparer.ToComparisonObject(endRange);

            var matches = allFiles
               .Select(envelope => new
               {
                   envelope = envelope,
                   graph = textComparer.ToComparisonObject(envelope.Text)
               })
               .Where(comparison => textComparer.IsBetweenMatch(startLookup, endLookup, comparison.graph))
               .Select(e => e.envelope);

            return matches;
        }
    }
}