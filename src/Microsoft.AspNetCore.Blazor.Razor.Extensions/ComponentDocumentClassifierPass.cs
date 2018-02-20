// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using System;
using System.IO;
using System.Linq;

namespace Microsoft.AspNetCore.Blazor.Razor
{
    internal class ComponentDocumentClassifierPass : DocumentClassifierPassBase, IRazorDocumentClassifierPass
    {
        protected override string DocumentKind => "Blazor.Component-0.1";

        protected override bool IsMatch(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            // Treat everything as a component by default if Blazor is part of the configuration.
            return true;
        }

        protected override void OnDocumentStructureCreated(
            RazorCodeDocument codeDocument, 
            NamespaceDeclarationIntermediateNode @namespace, 
            ClassDeclarationIntermediateNode @class, 
            MethodDeclarationIntermediateNode method)
        {
            @namespace.Content = (string)codeDocument.Items[BlazorCodeDocItems.Namespace];
            if (@namespace.Content == null)
            {
                @namespace.Content = "Blazor";
            }

            @class.BaseType = BlazorComponent.FullTypeName;
            @class.ClassName = (string)codeDocument.Items[BlazorCodeDocItems.ClassName];
            if (@class.ClassName == null)
            {
                @class.ClassName = codeDocument.Source.FilePath == null ? null : Path.GetFileNameWithoutExtension(codeDocument.Source.FilePath);
            }

            if (@class.ClassName == null)
            {
                @class.ClassName = "__BlazorComponent";
            }

            // Note that DefaultDocumentWriter's VisitMethodDeclaration in 2.0 is hardcoded to
            // emit methods with no parameters.
            //
            // This was added in 2.1, but we still need a workaround to support 2.0. In that case we
            // inject the parameter later in RazorCompiler.
            method.ReturnType = "void";
            method.MethodName = BlazorComponent.BuildRenderTree;
            method.Modifiers.Clear();
            method.Modifiers.Add("protected");
            method.Modifiers.Add("override");

            // Note that DefaultDocumentWriter's VisitMethodDeclaration in 2.0 is hardcoded to
            // emit methods with no parameters.
            //
            // This was added in 2.1, but we still need a workaround to support 2.0. In that case we
            // inject the parameter later in RazorCompiler.
#if RAZOR_2_1
            method.Parameters.Clear();
            method.Parameters.Add(new MethodParameter()
            {
                ParameterName = "builder",
                TypeName = "RenderTreeBuilder",
            });
#endif

            // We need to call the 'base' method as the first statement.
            var callBase = new CSharpCodeIntermediateNode();
            callBase.Children.Add(new IntermediateToken
            {
                Kind = TokenKind.CSharp,
                Content = $"base.{BlazorComponent.BuildRenderTree}(builder);" + Environment.NewLine
            });
            method.Children.Insert(0, callBase);
        }

        #region Workaround
        // This is a workaround for the fact that the base class doesn't provide good support
        // for replacing the IntermediateNodeWriter when building the code target. 
        void IRazorDocumentClassifierPass.Execute(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            base.Execute(codeDocument, documentNode);
            documentNode.Target = new BlazorCodeTarget(documentNode.Options, _targetExtensions);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var feature = Engine.Features.OfType<IRazorTargetExtensionFeature>();
            _targetExtensions = feature.FirstOrDefault()?.TargetExtensions.ToArray() ?? EmptyExtensionArray;
        }

        private static readonly ICodeTargetExtension[] EmptyExtensionArray = new ICodeTargetExtension[0];
        private ICodeTargetExtension[] _targetExtensions;
        #endregion
    }
}
