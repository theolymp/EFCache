// Copyright (c) Pawel Kadluczka, Inc. All rights reserved. See License.txt in the project root for license information.

#region usings

using System;

#endregion

namespace EFCache
{
    [Serializable]
    internal struct ColumnMetadata
    {
        public ColumnMetadata(string name, string dataTypeName, Type dataType)
        {
            Name = name;
            DataTypeName = dataTypeName;
            DataType = dataType;
        }

        public string Name { get; }

        public string DataTypeName { get; }

        public Type DataType { get; }
    }
}