// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
#if RAZOR_2_1

using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;

namespace Microsoft.AspNetCore.Blazor.Razor
{
    public class BlazorExtensionInitializer : RazorExtensionInitializer
    {
        public override void Initialize(RazorProjectEngineBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            FunctionsDirective.Register(builder);
            InheritsDirective.Register(builder);
            TemporaryLayoutPass.Register(builder);
            TemporaryImplementsPass.Register(builder);

            builder.Features.Add(new ComponentDocumentClassifierPass());

            builder.Phases.Remove(builder.Phases.OfType<IRazorCSharpLoweringPhase>().Single());
            builder.Phases.Add(new BlazorLoweringPhase());
        }
    }
}

#endif
