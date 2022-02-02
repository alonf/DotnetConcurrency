//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: ReductionVariable.cs
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace GettingBackTheFreeLunch
{

    /// <summary>Debug view for the reductino variable</summary>
    /// <typeparam name="T">Specifies the type of the data being aggregated.</typeparam>
    internal sealed class ReductionVariableDebugView<T>
    {
        private readonly ReductionVariable<T> _variable;

        public ReductionVariableDebugView(ReductionVariable<T> variable)
        {
            _variable = variable;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Values => _variable.Values.ToArray();
    }
}