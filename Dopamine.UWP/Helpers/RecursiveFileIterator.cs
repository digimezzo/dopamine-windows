using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Dopamine.UWP.Helpers
{
    public class RecursiveFileIterator
    {
        #region Variables
        private List<StorageFile> files;
        ConcurrentQueue<Exception> exceptions;
        #endregion

        #region ReadOnly Properties
        public ConcurrentQueue<Exception> Exceptions
        {
            get { return this.exceptions; }
        }
        #endregion

        #region Construction
        public RecursiveFileIterator()
        {
            files = new List<StorageFile>();
            exceptions = new ConcurrentQueue<Exception>();
        }
        #endregion

        #region Public
        public async Task<List<StorageFile>> GetFiles(StorageFolder folder, string[] validExtensions)
        {
            IReadOnlyList<IStorageItem> items = null;

            try
            {
                items = await folder.GetItemsAsync();
            }
            catch (Exception ex)
            {
                this.exceptions.Enqueue(ex);
            }

            if (items != null || items.Count > 0)
            {
                foreach (var item in items)
                {
                    if (item is StorageFile)
                    {
                        try
                        {
                            // Only add the file if it has an extension contained in iValidExtensions
                            if (validExtensions.Contains(((StorageFile)item).FileType.ToLower()))
                            {
                                files.Add(item as StorageFile);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.exceptions.Enqueue(ex);
                        }
                    }
                    else
                    {
                        await this.GetFiles(item as StorageFolder, validExtensions);
                    }
                }
            }

            return this.files;
        }
        #endregion
    }
}
