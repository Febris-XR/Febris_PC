// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.PCStatementManagerV3.Utilities
{
    class LocalStaticDetails
    {
        //counts in miliseconds
#if (DEBUG)
        internal static int TimeSpan = 60000;
#elif (STAGING)
internal static int TimeSpan = 300000;
#else
internal static int TimeSpan = 300000;
#endif

        //max zip storage for zip files (currently 10 gb)
        internal static long MaxZipStorage = 10737418240;
    }
}
