using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluiDBase
{
    public class Filter
    {
        protected readonly string[] _allowedContexts;
        protected readonly bool _emptyContextAllowed;


        public Filter(string[] allowedContexts, bool emptyContextAllowed)
        {
            _allowedContexts = allowedContexts ?? new string[0];
            _emptyContextAllowed = emptyContextAllowed;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">current testing context, may be several - delimeted by comma</param>
        /// <param name="useForEmpty">if true so empty testing context is tested by emptyContextAllowed parameter</param>
        /// <returns>Whether context needed to be excluded</returns>
        public bool Exclude(string context, bool useForEmpty)
        {
            if (ExcludeByContext(context, useForEmpty))
                return true;


            // todo label filter

            return false;
        }


        protected virtual bool ExcludeByContext(string context, bool useForEmpty)
        {
            List<string> testingContext = context?.Split(',')?
                .Select(x => x.Trim())?.Where(x => !string.IsNullOrWhiteSpace(x))?.ToList()
                ?? new List<string>();

            if (testingContext.Count == 0)
                return useForEmpty ? !_emptyContextAllowed : false;

            return !testingContext.Intersect(_allowedContexts, StringComparer.InvariantCultureIgnoreCase).Any();

            // changeSet: context:sync 
            // bat:   --contexts="Light,prod, sync_prod,sync" 

        }
    }
}
