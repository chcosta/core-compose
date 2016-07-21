﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NuGet.Versioning;

namespace UpdateRepo
{
    public class PackageInfo
    {
        public string Id { get; set; }
        public NuGetVersion Version { get; set; }
    }
}
