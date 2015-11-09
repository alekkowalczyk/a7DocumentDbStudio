using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Utils
{
    public static class DocDbExtMethods
    {
        public async static Task<IEnumerable<T>> QueryAsync<T>(this IQueryable<T> query, int packagesToRead)
        {
            var docQuery = query.AsDocumentQuery();
            var batches = new List<IEnumerable<T>>();

            int packagesRead = 0;
            do
            {
                var batch = await docQuery.ExecuteNextAsync<T>();
                batches.Add(batch);
                packagesRead++;
            }
            while (docQuery.HasMoreResults && (packagesToRead == -1 || packagesToRead > packagesRead));

            var docs = batches.SelectMany(b => b);
            return docs;
        }

    }
}
