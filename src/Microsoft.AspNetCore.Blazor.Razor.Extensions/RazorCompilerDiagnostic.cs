﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Blazor.Razor
{
    public class RazorCompilerDiagnostic
    {
        public DiagnosticType Type { get; }
        public string SourceFilePath { get; }
        public int Line { get; }
        public int Column { get; }
        public string Message { get; }

        public RazorCompilerDiagnostic(
            DiagnosticType type,
            string sourceFilePath,
            int line,
            int column,
            string message)
        {
            Type = type;
            SourceFilePath = sourceFilePath;
            Line = line;
            Column = column;
            Message = message;
        }

        public enum DiagnosticType
        {
            Warning,
            Error
        }

        public string FormatForConsole()
            => $"{SourceFilePath}({Line},{Column}): {FormatTypeAndCodeForConsole()}: {Message}";

        private string FormatTypeAndCodeForConsole()
            => $"{Type.ToString().ToLowerInvariant()} Blazor";
    }
}
