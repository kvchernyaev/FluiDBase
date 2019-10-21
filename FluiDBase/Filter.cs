using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluiDBase
{
    public class Filter
    {
        readonly string[] _allowedContexts;
        readonly bool _emptyContextAllowed;


        public Filter(string[] allowedContexts, bool emptyContextAllowed)
        {
            _allowedContexts = allowedContexts ?? new string[0];
            _emptyContextAllowed = emptyContextAllowed;
        }


        public bool Exclude(string context, bool useForEmpty)
        {
            if (ExcludeByContext(context, useForEmpty))
                return true;


            // todo label filter

            return false;
        }


        bool ExcludeByContext(string context, bool useForEmpty)
        {
            List<string> testingContext = context?.Split(',')?.Select(x => x.Trim())?.Where(x => !string.IsNullOrWhiteSpace(x))?.ToList() ?? new List<string>();

            if (testingContext.Count == 0)
                return useForEmpty ? !_emptyContextAllowed : false;

            return !testingContext.Intersect(_allowedContexts, StringComparer.InvariantCultureIgnoreCase).Any();

            // changeSet: context:sync 
            // bat:   --contexts="Light,prod, sync_prod,sync" 

        }
    }
}
