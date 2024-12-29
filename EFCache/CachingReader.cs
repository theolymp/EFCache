// Copyright (c) Pawel Kadluczka, Inc. All rights reserved. See License.txt in the project root for license information.

#region usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

#endregion

namespace EFCache
{
    internal class CachingReader : DbDataReader
    {
        private readonly int _recordsAffected;

        // TODO: multiple resultsets?
        private readonly IEnumerator<object[]> _resultRowsEnumerator;
        private readonly ColumnMetadata[] _tableMetadata;

        private State _state;

        internal CachingReader(CachedResults cachedResults)
        {
            Debug.Assert(cachedResults != null, "cachedResults is null");

            _tableMetadata = cachedResults.TableMetadata;
            _recordsAffected = cachedResults.RecordsAffected;
            _resultRowsEnumerator = cachedResults.Results.GetEnumerator();
            _state = State.BOF;
        }

        public override int Depth => throw new NotImplementedException();

        public override int FieldCount => _tableMetadata.Length;

        public override bool HasRows => throw new NotImplementedException();

        public override bool IsClosed
        {
            get
            {
                EnsureNotDisposed();

                return _state == State.Closed;
            }
        }

        public override object this[string name] => throw new NotImplementedException();

        public override object this[int ordinal] => throw new NotImplementedException();

        public override int RecordsAffected => _recordsAffected;

        public override bool GetBoolean(int ordinal)
        {
            return (bool)GetValue(ordinal);
        }

        public override bool IsDBNull(int ordinal)
        {
            return GetValue(ordinal) == DBNull.Value || GetValue(ordinal) == null;
        }

        public override bool NextResult()
        {
            // TODO: Multiple resultsets
            return false;
        }

        public override bool Read()
        {
            EnsureNotClosed();

            var result = _resultRowsEnumerator.MoveNext();

            _state = result ? State.Reading : State.EOF;

            return result;
        }

        public override byte GetByte(int ordinal)
        {
            return (byte)GetValue(ordinal);
        }

        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return (DateTime)GetValue(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return (decimal)GetValue(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return (double)GetValue(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            return (float)GetValue(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return (Guid)GetValue(ordinal);
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override int GetInt32(int ordinal)
        {
            return (int)GetValue(ordinal);
        }

        public override int GetOrdinal(string name)
        {
            return _tableMetadata.Select((x, i) => Tuple.Create(i, x)).FirstOrDefault(x => x.Item2.Name == name)
                ?.Item1 ?? -1;
        }

        public override int GetValues(object[] values)
        {
            Array.Copy(_resultRowsEnumerator.Current ?? throw new InvalidOperationException(), values, values.Length);
            return values.Length;
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override long GetInt64(int ordinal)
        {
            return (long)GetValue(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            EnsureReading();

            return _resultRowsEnumerator.Current?[ordinal];
        }

        public override short GetInt16(int ordinal)
        {
            return (short)GetValue(ordinal);
        }

        public override string GetDataTypeName(int ordinal)
        {
            return _tableMetadata[ordinal].DataTypeName;
        }

        public override string GetName(int ordinal)
        {
            return _tableMetadata[ordinal].Name;
        }

        public override string GetString(int ordinal)
        {
            return (string)GetValue(ordinal);
        }

        public override Type GetFieldType(int ordinal)
        {
            return _tableMetadata[ordinal].DataType;
        }

        public override void Close()
        {
            EnsureNotDisposed();

            _state = State.Closed;
        }

        protected override void Dispose(bool disposing)
        {
            // base.Dispose() will call Close()
            base.Dispose(disposing);

            _resultRowsEnumerator.Dispose();

            _state = State.Disposed;
        }

        private void EnsureNotBOF()
        {
            if (_state == State.BOF)
                throw new InvalidOperationException("The operation is invalid before reading any data.");
        }

        private void EnsureNotClosed()
        {
            if (IsClosed) throw new InvalidOperationException("Reader has already been closed.");
        }

        private void EnsureNotDisposed()
        {
            if (_state == State.Disposed) throw new InvalidOperationException("Object has already been disposed.");
        }

        private void EnsureNotEOF()
        {
            if (_state == State.EOF)
                throw new InvalidOperationException("The operation is invalid after reading all data.");
        }

        private void EnsureReading()
        {
            EnsureNotClosed();
            EnsureNotBOF();
            EnsureNotEOF();
        }

        private enum State
        {
            BOF,
            Reading,
            EOF,
            Closed,
            Disposed
        }
    }
}