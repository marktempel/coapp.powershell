//-----------------------------------------------------------------------
// <copyright company="CoApp Project">
//     ResourceLib Original Code from http://resourcelib.codeplex.com
//     Original Copyright (c) 2008-2009 Vestris Inc.
//     Changes Copyright (c) 2011 Garrett Serack . All rights reserved.
// </copyright>
// <license>
// MIT License
// You may freely use and distribute this software under the terms of the following license agreement.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of 
// the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE
// </license>
//-----------------------------------------------------------------------

namespace ClrPlus.Windows.PeBinary.ResourceLib {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using Api.Structures;
    using Core.Collections;

    /// <summary>
    ///     This structure depicts the organization of data in a file-version resource. It typically contains a
    ///     list of language and code page identifier pairs that the version of the application or DLL supports.
    ///     http://msdn.microsoft.com/en-us/library/bb202818.aspx
    /// </summary>
    public class VarTable : ResourceTableHeader {
        private IDictionary<UInt16, UInt16> _languages = new XDictionary<UInt16, UInt16>();

        /// <summary>
        ///     A new table of language and code page identifier pairs.
        /// </summary>
        public VarTable() {
        }

        /// <summary>
        ///     A new table of language and code page identifier pairs.
        /// </summary>
        /// <param name="key">Table key.</param>
        public VarTable(string key) : base(key) {
        }

        /// <summary>
        ///     An existing table of language and code page identifier pairs.
        /// </summary>
        /// <param name="lpRes">Pointer to the beginning of the data.</param>
        internal VarTable(IntPtr lpRes) {
            Read(lpRes);
        }

        /// <summary>
        ///     A dictionary of language and code page identifier pairs.
        /// </summary>
        public IDictionary<UInt16, UInt16> Languages {
            get {
                return _languages;
            }
        }

        /// <summary>
        ///     Returns a code page identifier for a given language.
        /// </summary>
        /// <param name="key">Language ID.</param>
        /// <returns>Code page identifier.</returns>
        public UInt16 this[UInt16 key] {
            get {
                return _languages[key];
            }
            set {
                _languages[key] = value;
            }
        }

        /// <summary>
        ///     Read a table of language and code page identifier pairs.
        /// </summary>
        /// <param name="lpRes">Pointer to the beginning of the data.</param>
        /// <returns></returns>
        internal override IntPtr Read(IntPtr lpRes) {
            _languages.Clear();
            var pVar = base.Read(lpRes);

            while (pVar.ToInt32() < (lpRes.ToInt32() + _header.wLength)) {
                var var = (VarHeader)Marshal.PtrToStructure(pVar, typeof (VarHeader));
                _languages.Add(var.wLanguageIDMS, var.wCodePageIBM);
                pVar = new IntPtr(pVar.ToInt32() + Marshal.SizeOf(var));
            }

            return new IntPtr(lpRes.ToInt32() + _header.wLength);
        }

        /// <summary>
        ///     Write the table of language and code page identifier pairs to a binary stream.
        /// </summary>
        /// <param name="w">Binary stream.</param>
        /// <returns>Last unpadded position.</returns>
        internal override void Write(BinaryWriter w) {
            var headerPos = w.BaseStream.Position;
            base.Write(w);

            var languagesEnum = _languages.GetEnumerator();
            var valuePos = w.BaseStream.Position;
            while (languagesEnum.MoveNext()) {
                // id
                w.Write(languagesEnum.Current.Key);
                // code page
                w.Write(languagesEnum.Current.Value);
            }

            ResourceUtil.WriteAt(w, w.BaseStream.Position - valuePos, headerPos + 2);
            ResourceUtil.PadToDWORD(w);
            ResourceUtil.WriteAt(w, w.BaseStream.Position - headerPos, headerPos);
        }

        /// <summary>
        ///     String representation of the var table.
        /// </summary>
        /// <param name="indent">Indent.</param>
        /// <returns>String representation of the var table.</returns>
        public override string ToString(int indent) {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}BEGIN", new String(' ', indent)));
            var languagesEnumerator = _languages.GetEnumerator();
            while (languagesEnumerator.MoveNext()) {
                sb.AppendLine(string.Format("{0}VALUE \"Translation\", 0x{1:x}, 0x{2:x}", new String(' ', indent + 1), languagesEnumerator.Current.Key,
                    languagesEnumerator.Current.Value));
            }
            sb.AppendLine(string.Format("{0}END", new String(' ', indent)));
            return sb.ToString();
        }
    }
}