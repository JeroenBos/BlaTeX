// #nullable enable
// using System;
// using System.Reflection;
// using System.Threading.Tasks;
// using BlaTeX.Pages;
// using Bunit.Diffing;
// using Bunit.Extensions;
// using Bunit.RazorTesting;
// using Bunit.Rendering;
// using Microsoft.AspNetCore.Components;
// using Microsoft.JSInterop;

// namespace Bunit
// {
//     /// <summary>
//     /// A component used to create KaTeX snapshot tests.
//     /// Snapshot tests takes the math string and options as inputs, and an Expected section.
//     /// It then compares the result of letting the katex library render the inputs, using semantic HTML comparison.
//     /// </summary>
//     public class SimpleKaTeXTest : SnapshotTest
//     {
//         public KaTeXTest()
//         {
//             base.SetupAsync = (a, b) =>
//             {

//             };
//         }
//         /// <inheritdoc/>
//         protected override async Task Run()
//         {
//             Validate();

//             return base.Run();
//         }

//     }
// }